namespace Client.Components.Shop.Checkout

open Feliz
open Client.Components.Shop.Common
open Shared.Store.Cart
open Shared.Store.Checkout
open Elmish
open Bindings.Stripe.StripePayment
open Fable.Core.JsInterop
open Shared.Store
open Shared.Api.Checkout
open Bindings.Stripe.StripePayment


module Checkout =
    type CheckoutStep =
        | Shipping
        | Payment
        | Review

    type ShippingInfo = {
        Email     : string
        FirstName : string
        LastName  : string
        Address   : string
        Apartment : string
        City      : string
        State     : string
        ZipCode   : string
        Country   : string
        Phone     : string
    }
    // we need shipping validation here....

    //
    type PaymentMethod =
        | Stripe
        // | ApplePay
        // | GooglePay

    type Field =
        | Email
        | FirstName
        | LastName
        | Address
        | Apartment
        | City
        | State
        | ZipCode
        | Phone

    type Model = {
        Step            : CheckoutStep

        CustomerShippingInfo    : ShippingInfo

        // SelectedShippingMethod  : ShippingOption option   // still used locally for UI
        // ShippingOptions : Shared.Api.Checkout.ShippingOption list

        // From server preview:
        PreviewTotals   : PreviewTotals option
        CheckoutPreviewLines : CheckoutPreviewLine list
        // QuoteTotals: QuoteTotals list option

        PaymentMethod   : PaymentMethod

        Cart : CartState

        // From server payment session:
        Stripe : IStripe option
        StripeSessionId    : string option
        StripeClientSecret : string option
        PrintfulDraftId    : string option

        IsBusy         : bool
        Error          : string option

        OrderConfirmation          : ConfirmOrderResponse option
    }

    type Msg =
        | UpdateShippingField of Field * string
        | SelectPaymentMethod of PaymentMethod
        | SubmitShipping

        // | SubmitCardPayment of IStripeElement
        
        // | PaymentSetupFailed of string
        
        | SubmitPayment of IElements
        | SubmitCompleted of Result<IElements * obj, exn>
        | ConfirmCompleted of Result<ConfirmPaymentResult, exn>
        | ConfirmationSuccess of ConfirmOrderResponse
        | ConfirmationFailed of exn
        
        | PaymentFailed of string
        
        | SetStep of CheckoutStep
        | BackToCart

    let initialShippingInfo : ShippingInfo = {
        Email     = ""
        FirstName = ""
        LastName  = ""
        Address   = ""
        Apartment = ""
        City      = ""
        State     = ""
        ZipCode   = ""
        Country   = "US"
        Phone     = ""
    }

    let initCheckoutModel cartState : Model = {
        Step            = Shipping
        CustomerShippingInfo    = initialShippingInfo
        PaymentMethod   = Stripe
        Cart           = cartState
        PreviewTotals   = None
        CheckoutPreviewLines = []
        StripeSessionId    = None
        StripeClientSecret = None
        Stripe = None
        PrintfulDraftId    = None
        IsBusy         = false
        Error          = None
        OrderConfirmation = None
    }

    module Helpers =

        let computeSubtotal (items: CartLineItem list) : decimal =
            items
            |> List.sumBy (fun i -> 
                match i with
                | CartLineItem.Sync s -> s.UnitPrice * decimal s.Quantity
                | CartLineItem.Custom c -> c.UnitPrice * decimal c.Quantity
                | CartLineItem.Template t -> t.UnitPrice * decimal t.Quantity
            )

    module Handlers =

        let paymentHandler model stripePaymentId status =
            Cmd.OfAsync.either
                Client.Api.checkoutApi.ConfirmOrder
                {
                    CustomerInfo = {
                        FirstName = model.CustomerShippingInfo.FirstName
                        LastName = model.CustomerShippingInfo.LastName
                        Email = model.CustomerShippingInfo.Email
                        Phone = None
                    }
                    StripeConfirmation = stripePaymentId
                    OrderDraftId = model.PrintfulDraftId |> Option.defaultValue ""
                    IsSuccess =
                        status = "success"
                }
                ConfirmationSuccess
                ConfirmationFailed

        // let confirmCardCmd (stripe: IStripe) clientSecret confirmParams =
        //     Cmd.OfPromise.either
        //         (fun (cs, cp) -> stripe.confirmCardPayment(cs, cp))
        //         (clientSecret, confirmParams)
        //         (fun res ->
        //             match res.error with
        //             | Some err ->
        //                 PaymentFailed (defaultArg err.message err.``type``)
        //             | None ->
        //                 match res.paymentIntent with
        //                 | None -> PaymentFailed "No payment intent returned."
        //                 | Some pi -> PaymentSucceeded (pi.id, pi.status))
        //         (fun ex -> PaymentFailed ex.Message)

        let submitPayment (elements: IElements) =
            Cmd.OfPromise.either
                (fun (el: IElements) -> el.submit())
                elements
                (fun submitResult -> SubmitCompleted (Ok (elements, submitResult)))
                (fun ex -> SubmitCompleted (Error ex))
    
        let confirmPaymentPromise (stripe: IStripe) clientSecret (elements: IElements) =
            let objParams =
                createObj [
                    "elements" ==> elements
                    "clientSecret" ==> clientSecret
                    // "confirmParams" ==> createObj [ "return_url" ==> "http://localhost:8080/shop/cart" ]
                    "redirect" ==> "if_required"
                ] |> unbox
            Cmd.OfPromise.either
                stripe.confirmPayment 
                objParams
                (fun x -> ConfirmCompleted (Ok x))
                (fun e -> ConfirmationFailed e)

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | SubmitShipping ->
            // parent call to server to get order totals
            model, Cmd.none

        | BackToCart ->
            // handled by parent Shop module, just bubble intent
            model, Cmd.none

        | SetStep step ->
            // TODO — integrate Stripe "create session" later
            { model with Step = step }, 
            Cmd.none

        | UpdateShippingField (field, value) ->
            let si = model.CustomerShippingInfo
            let updated =
                match field with
                | Email     -> { si with Email = value }
                | FirstName -> { si with FirstName = value }
                | LastName  -> { si with LastName = value }
                | Address   -> { si with Address = value }
                | Apartment -> { si with Apartment = value }
                | City      -> { si with City = value }
                | State     -> { si with State = value }
                | ZipCode   -> { si with ZipCode = value }
                | Phone     -> { si with Phone = value }

            { model with CustomerShippingInfo = updated }, Cmd.none

        // Not used currently
        | SelectPaymentMethod method ->
            { model with PaymentMethod = method }, Cmd.none

        | SubmitPayment ce ->
            model,
            Handlers.submitPayment ce

        | SubmitCompleted (Error ex) ->
            { model with Error = Some ex.Message }, Cmd.none

        | SubmitCompleted (Ok (elements, submitResult)) ->
            match model.Stripe, submitResult?error with
            | Some strp, None ->
                // proceed to confirm
                model,
                Handlers.confirmPaymentPromise 
                    strp
                    model.StripeClientSecret
                    elements
                    
            | None, _ -> model, Cmd.none
            | Some _, Some err ->
                { model with Error = Some err?message }, Cmd.none

        | PaymentFailed err ->
            { model with Error = Some err }, Cmd.none

        | ConfirmCompleted confirmResponse ->
            // TODO: call ConfirmOrder endpoint using model.DraftExternalId / PaymentIntentId
            match (confirmResponse: Result<ConfirmPaymentResult,exn>) with
            | Error e -> model, Cmd.none
            | Ok resp when resp.error.IsSome -> {model with Error = resp.error.Value.message }, Cmd.none
            | Ok resp when resp.paymentIntent.IsSome ->
                model,
                Handlers.paymentHandler model resp.paymentIntent.Value.id resp.paymentIntent.Value.status
            | _ -> model, Cmd.none

        | ConfirmationFailed err ->
            { model with Error = Some err.Message }, Cmd.none

        | ConfirmationSuccess confirmationResp ->
            // need to set cart and totals to none on parent, reset checkout and only use response from here....
            { model with OrderConfirmation = Some confirmationResp },
            Cmd.ofMsg (SetStep Review)


module View =
    open Checkout

    let private labelStep (step: CheckoutStep) =
        match step with
        | Shipping -> "Shipping"
        | Payment  -> "Payment"
        | Review   -> "Review"

    [<ReactComponent>]
    let CheckoutStepper (model: Model) =
        let steps =
            [ Shipping; Payment; Review ]

        Html.div [
            prop.className "mb-10 md:mb-12"
            prop.children [
                Html.div [
                    prop.className "flex items-center justify-between gap-4"
                    prop.children [
                        for (index, step) in steps |> List.indexed do
                            let currentIndex = steps |> List.findIndex ((=) model.Step)
                            let isActive   = step = model.Step
                            let isComplete = index < currentIndex

                            Html.div [
                                prop.key (string index)
                                prop.className "flex-1 relative"
                                prop.children [
                                    Html.div [
                                        prop.className "flex flex-col items-center"
                                        prop.children [
                                            Html.div [
                                                prop.className (Ui.tw [
                                                    "w-12 h-12 md:w-12 md:h-12 rounded-full flex items-center justify-center text-xs md:text-sm font-medium transition-all duration-300"
                                                    if isComplete then "bg-primary text-primary-content"
                                                    elif isActive then "bg-primary text-primary-content scale-110"
                                                    else "bg-base-200 text-base-content/50"
                                                ])
                                                prop.children (
                                                    match step with
                                                    | Shipping  -> Bindings.LucideIcon.LucideIcon.Truck
                                                    | Payment  -> Bindings.LucideIcon.LucideIcon.CircleDollarSign
                                                    | Review  -> Bindings.LucideIcon.LucideIcon.PackageCheck
                                                    |> fun ico -> ico "w-4 h-4"
                                                )
                                            ]
                                            Html.span [
                                                prop.className (Ui.tw [
                                                    "mt-2 text-[0.65rem] md:text-xs uppercase tracking-[0.25em]"
                                                    if isActive then "text-base-content font-semibold"
                                                    else "text-base-content/50"
                                                ])
                                                prop.text (labelStep step)
                                            ]
                                        ]
                                    ]
                                    if index < steps.Length - 1 then
                                        Html.div [
                                            prop.className (Ui.tw [
                                                "absolute top-[1.3rem] md:top-[1.6rem] left-1/2 w-full h-px -z-10"
                                                if isComplete then "bg-primary"
                                                else "bg-base-300"
                                            ])
                                        ]
                                ]
                            ]
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    let CheckoutShipping (model: Model) (dispatch: Msg -> unit) =
        let subtotal =
            Helpers.computeSubtotal model.Cart.Items

        Html.form [
            prop.className "space-y-8"
            prop.onSubmit (fun e ->
                e.preventDefault()
                dispatch SubmitShipping
            )
            prop.children [

                // Shipping info
                Html.div [
                    prop.className "space-y-5"
                    prop.children [
                        Html.h2 [
                            prop.className "text-2xl font-light"
                            prop.text "Shipping Information"
                        ]

                        Html.div [
                            prop.className "space-y-4"
                            prop.children [

                                // Email
                                Html.div [
                                    Html.label [
                                        prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                        prop.text "Email Address"
                                    ]
                                    Html.input [
                                        prop.className "input input-bordered w-full rounded-none"
                                        prop.type' "email"
                                        prop.required true
                                        prop.value model.CustomerShippingInfo.Email
                                        prop.placeholder "you@example.com"
                                        prop.onChange (fun v -> dispatch (UpdateShippingField (Email, v)))
                                    ]
                                ]

                                // Names
                                Html.div [
                                    prop.className "grid grid-cols-1 sm:grid-cols-2 gap-4"
                                    prop.children [
                                        Html.div [
                                            Html.label [
                                                prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                                prop.text "First Name"
                                            ]
                                            Html.input [
                                                prop.className "input input-bordered w-full rounded-none"
                                                prop.required true
                                                prop.value model.CustomerShippingInfo.FirstName
                                                prop.onChange (fun v -> dispatch (UpdateShippingField (FirstName, v)))
                                            ]
                                        ]
                                        Html.div [
                                            Html.label [
                                                prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                                prop.text "Last Name"
                                            ]
                                            Html.input [
                                                prop.className "input input-bordered w-full rounded-none"
                                                prop.required true
                                                prop.value model.CustomerShippingInfo.LastName
                                                prop.onChange (fun v -> dispatch (UpdateShippingField (LastName, v)))
                                            ]
                                        ]
                                    ]
                                ]

                                // Address
                                Html.div [
                                    Html.label [
                                        prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                        prop.text "Address"
                                    ]
                                    Html.input [
                                        prop.className "input input-bordered w-full rounded-none"
                                        prop.required true
                                        prop.value model.CustomerShippingInfo.Address
                                        prop.placeholder "Street address"
                                        prop.onChange (fun v -> dispatch (UpdateShippingField (Address, v)))
                                    ]
                                ]

                                // Apt
                                Html.div [
                                    Html.label [
                                        prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                        prop.text "Apartment, Suite, etc. (Optional)"
                                    ]
                                    Html.input [
                                        prop.className "input input-bordered w-full rounded-none"
                                        prop.value model.CustomerShippingInfo.Apartment
                                        prop.onChange (fun v -> dispatch (UpdateShippingField (Apartment, v)))
                                    ]
                                ]

                                // City / State / ZIP
                                Html.div [
                                    prop.className "grid grid-cols-1 sm:grid-cols-3 gap-4"
                                    prop.children [
                                        Html.div [
                                            Html.label [
                                                prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                                prop.text "City"
                                            ]
                                            Html.input [
                                                prop.className "input input-bordered w-full rounded-none"
                                                prop.required true
                                                prop.value model.CustomerShippingInfo.City
                                                prop.onChange (fun v -> dispatch (UpdateShippingField (City, v)))
                                            ]
                                        ]
                                        Html.div [
                                            Html.label [
                                                prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                                prop.text "State"
                                            ]
                                            Html.input [
                                                prop.className "input input-bordered w-full rounded-none"
                                                prop.required true
                                                prop.value model.CustomerShippingInfo.State
                                                prop.onChange (fun v -> dispatch (UpdateShippingField (State, v)))
                                            ]
                                        ]
                                        Html.div [
                                            Html.label [
                                                prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                                prop.text "ZIP Code"
                                            ]
                                            Html.input [
                                                prop.className "input input-bordered w-full rounded-none"
                                                prop.required true
                                                prop.value model.CustomerShippingInfo.ZipCode
                                                prop.onChange (fun v -> dispatch (UpdateShippingField (ZipCode, v)))
                                            ]
                                        ]
                                    ]
                                ]

                                // Phone
                                Html.div [
                                    Html.label [
                                        prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                        prop.text "Phone Number"
                                    ]
                                    Html.input [
                                        prop.className "input input-bordered w-full rounded-none"
                                        prop.type' "tel"
                                        prop.required true
                                        prop.placeholder "(555) 123-4567"
                                        prop.value model.CustomerShippingInfo.Phone
                                        prop.onChange (fun v -> dispatch (UpdateShippingField (Phone, v)))
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]

                // Shipping method
                // Html.div [
                //     prop.className "space-y-5 pt-6 border-t border-base-300"
                //     prop.children [
                        
                //         Html.div [
                //             prop.className "space-y-3"
                //             prop.children [
                //                 match model.PreviewTotals with
                //                 | None ->
                                    
                //                 | Some pt ->
                //                         Html.div [
                //                             prop.key (pt.ShippingName)
                //                             prop.className (Ui.tw [
                //                                 "border-2 p-4 md:p-5 cursor-pointer transition-all"
                //                                 "border-primary bg-base-200/60"
                //                             ])
                //                             prop.children [
                //                                 Html.div [
                //                                     prop.className "flex items-center justify-between"
                //                                     prop.children [
                //                                         Html.div [
                //                                             prop.className "flex items-center gap-3"
                //                                             prop.children [
                //                                                 Html.div [
                //                                                     prop.className (Ui.tw [
                //                                                         "w-5 h-5 rounded-full border-2 flex items-center justify-center"
                //                                                         "border-primary"
                //                                                         // else "border-base-300"
                //                                                     ])
                //                                                     prop.children [
                //                                                         // if isActive then
                //                                                             Html.div [
                //                                                                 prop.className "w-2.5 h-2.5 rounded-full bg-primary"
                //                                                             ]
                //                                                     ]
                //                                                 ]
                //                                                 Html.div [
                //                                                     Html.p [
                //                                                         prop.className "font-medium"
                //                                                         prop.text pt.ShippingName
                //                                                     ]
                //                                                 ]
                //                                             ]
                //                                         ]
                //                                         Html.p [
                //                                             prop.className "text-sm md:text-lg font-light"
                //                                             prop.text (if pt.Shipping = 0m then "FREE" else "$" + string pt.Shipping)
                //                                         ]
                //                                     ]
                //                                 ]
                //                             ]
                //                         ]
                //             ]
                //         ]
                //     ]
                // ]

                let isDisabled =
                    System.String.IsNullOrWhiteSpace model.CustomerShippingInfo.Email ||
                    System.String.IsNullOrWhiteSpace model.CustomerShippingInfo.FirstName ||
                    System.String.IsNullOrWhiteSpace model.CustomerShippingInfo.LastName ||
                    System.String.IsNullOrWhiteSpace model.CustomerShippingInfo.City ||
                    System.String.IsNullOrWhiteSpace model.CustomerShippingInfo.State ||
                    System.String.IsNullOrWhiteSpace model.CustomerShippingInfo.ZipCode

                Html.div [
                    prop.className "flex flex-col md:flex-row gap-3 pt-6"
                    prop.children [
                        Html.button [
                            prop.className "btn btn-outline rounded-none flex-1"
                            prop.type' "button"
                            prop.text "Back to Cart"
                            prop.onClick (fun _ -> dispatch BackToCart)
                        ]
                        Html.button [
                            prop.className "btn btn-primary rounded-none flex-1"
                            prop.disabled isDisabled
                            prop.type' "submit"
                            prop.text "Calculate Totals"
                        ]
                    ]
                ]
            ]
        ]

    open Browser.Types

    type StripeCardHostProps =
        {|
            Stripe: IStripe option
            ClientSecret: string option
            // OnCardPaymentReady: IStripeElement option -> unit
            OnPaymentReady: IElements option -> unit
        |}

    let stripeOptions : obj * obj =
        let style : obj =
            box 
                {| 
                    ``base`` = 
                        box {| 
                            fontSize = "18px"
                            lineHeight = "1.6"
                            color = "#111827"
                            backgroundColor = "#F9FAFB"
                            borderRadius = "0.5rem"
                        |}
                    invalid = box {| color = "#DC2626" |}
                |}
        let classes : obj = 
            box {| 
                // host element (outside iframe)
                ``base`` = "px-4 py-3 rounded-lg border border-base-300 bg-base-100 shadow-sm"
                focus = "ring-2 ring-primary/70 border-primary"
                invalid = "border-error"
            |}

        style, classes

    

    let safeCssVar name fallback = 
        let value = TSXUtilities.getCssVar name 
        if System.String.IsNullOrWhiteSpace value
        then fallback else TSXUtilities.convertOklchToHex value


    [<ReactComponent>]
    let StripeCardHost (props: StripeCardHostProps) =

        // Ref to the div where Stripe will mount the card
        let containerRef = React.useRef<HTMLElement option> None

        // Store created card element so we don’t recreate/remount every render
        let paymentRef = React.useRef<IStripeElement option>(None)
        let elementsRef = React.useRef<IElements option>(None)

        React.useEffect(
            (fun () ->
                printfn $"Stripe: {props.Stripe}"
                printfn $"Secret: {props.ClientSecret}"
                printfn $"Ref: {containerRef.current}"
                match props.Stripe, props.ClientSecret, containerRef.current with
                | Some stripe, Some clientSecret, Some containerEl ->

                    let stripeAppearance =
                        createObj [
                            "theme" ==> "flat"
                            "variables" ==> createObj [
                                "colorPrimary" ==> safeCssVar "--color-primary" "#0A84FF"
                                "colorBackground" ==> safeCssVar "--color-background" "#ffffff"
                                "colorText" ==> safeCssVar "--color-text" "#111827"
                            ]
                        ]

                    printfn $"appearance: {stripeAppearance}"
                    // Only create/mount once
                    if paymentRef.current.IsNone then
                        let opts =
                            jsOptions<StripeElementsOptions>(fun o ->
                                o.clientSecret <- Some clientSecret
                                o.appearance <- Some stripeAppearance
                            )
                        let elements = stripe.elements opts
                        let style, classes = stripeOptions
                        let paymentElem = elements.create(
                            "payment", 
                            jsOptions(fun o -> 
                                o?style <- style
                                o?classes <- classes
                            )
                        )
                        // ⭐ Use the HTMLElement overload here
                        paymentElem.mount(containerEl)
                        elementsRef.current <- Some elements
                        paymentRef.current <- Some paymentElem
                        props.OnPaymentReady (Some elements)
                | _ -> ()

                // cleanup on unmount / dependencies change
                React.createDisposable(fun () ->
                    match paymentRef.current with
                    | Some card ->
                        card.destroy()
                        paymentRef.current <- None
                    | None -> ()
                )
            ),
            [| box props.Stripe; box props.ClientSecret |]
        )

        Html.div [
            // ✅ This now matches prop.ref‘s expected type
            prop.ref containerRef
            prop.className "border rounded p-3 min-h-[60px]"
        ]

    [<ReactComponent>]
    let CheckoutPayment (model: Model) (dispatch: Msg -> unit) =
        let elementsGroup, setElementsGroup = React.useState None

        Html.div [
            prop.className "space-y-4"
            prop.children [
                // Stripe card host: only gets Stripe + clientSecret
                StripeCardHost {|
                    Stripe = model.Stripe
                    ClientSecret = model.StripeClientSecret
                    OnPaymentReady = setElementsGroup
                |}

                Html.button [
                    prop.className "btn btn-primary"
                    prop.disabled (model.IsBusy || model.Stripe.IsNone || model.StripeClientSecret.IsNone)
                    prop.text (if model.IsBusy then "Processing..." else "Pay now")
                    prop.onClick (fun _ -> 
                        match elementsGroup with
                        | None -> ()
                        | Some eg -> dispatch (SubmitPayment eg)
                    )
                ]

                match model.Error with
                | Some err ->
                    Html.p [ prop.className "text-red-500"; prop.text err ]
                | None -> Html.none
            ]
        ]

    let getCartLineItemDetails item = //imgLabel, name, color, size, quantity, price =
        match item with
        | CartLineItem.Sync s ->
            (string s.SyncProductId) + ":" + (string s.SyncVariantId)
            , s.ThumbnailUrl, s.Name, s.ColorName, s.Size, s.Quantity, s.UnitPrice
        | CartLineItem.Custom c ->
            (string c.CatalogProductId) + ":" + (string c.CatalogVariantId)
            , c.ThumbnailUrl, c.Name, c.ColorName, c.Size, c.Quantity, c.UnitPrice
        | CartLineItem.Template t ->
            (string t.CatalogProductId) + ":" + (string t.VariantId) + ":" + (string t.TemplateId)
            , t.PreviewImage |> Option.defaultValue t.Name, t.Name, t.ColorName, t.Size, t.Quantity, t.UnitPrice


    [<ReactComponent>]
    let CheckoutReview (model: Model) (dispatch: Msg -> unit) =
        let subtotal =
            Helpers.computeSubtotal model.Cart.Items

        Html.div [
            prop.className "space-y-8"
            prop.children [

                Html.h2 [
                    prop.className "text-2xl font-light"
                    prop.text "Review Your Order"
                ]

                // Shipping address
                Html.div [
                    prop.className "border border-base-300 rounded-lg p-5 md:p-6 space-y-3"
                    prop.children [
                        Html.div [
                            prop.className "flex items-center justify-between"
                            prop.children [
                                Html.h3 [
                                    prop.className "text-xs font-semibold uppercase tracking-[0.25em]"
                                    prop.text "Shipping Address"
                                ]
                                Html.button [
                                    prop.className "text-xs underline hover:text-base-content/70"
                                    prop.text "Edit"
                                    prop.onClick (fun _ -> dispatch (SetStep Shipping))
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "text-sm text-base-content/70 space-y-0.5"
                            prop.children [
                                Html.p $"{model.CustomerShippingInfo.FirstName} {model.CustomerShippingInfo.LastName}"
                                Html.p model.CustomerShippingInfo.Address
                                if not (System.String.IsNullOrWhiteSpace model.CustomerShippingInfo.Apartment) then
                                    Html.p model.CustomerShippingInfo.Apartment
                                Html.p $"{model.CustomerShippingInfo.City}, {model.CustomerShippingInfo.State} {model.CustomerShippingInfo.ZipCode}"
                                Html.p model.CustomerShippingInfo.Phone
                                Html.p [
                                    prop.className "mt-1"
                                    prop.text model.CustomerShippingInfo.Email
                                ]
                            ]
                        ]
                    ]
                ]

                // Shipping method
                Html.div [
                    prop.className "border border-base-300 rounded-lg p-5 md:p-6 space-y-3"
                    prop.children [
                        Html.div [
                            prop.className "flex items-center justify-between"
                            prop.children [
                                Html.h3 [
                                    prop.className "text-xs font-semibold uppercase tracking-[0.25em]"
                                    prop.text "Shipping Method"
                                ]
                                Html.button [
                                    prop.className "text-xs underline hover:text-base-content/70"
                                    prop.text "Edit"
                                    prop.onClick (fun _ -> dispatch (SetStep Shipping))
                                ]
                            ]
                        ]
                        Html.p [
                            prop.className "text-sm text-base-content/70"
                            prop.text (model.PreviewTotals |> Option.map ( fun (x: PreviewTotals) -> x.ShippingName ) |> Option.defaultValue "")
                        ]
                    ]
                ]

                // Payment
                Html.div [
                    prop.className "border border-base-300 rounded-lg p-5 md:p-6 space-y-3"
                    prop.children [
                        Html.div [
                            prop.className "flex items-center justify-between"
                            prop.children [
                                Html.h3 [
                                    prop.className "text-xs font-semibold uppercase tracking-[0.25em]"
                                    prop.text "Payment Method"
                                ]
                                Html.button [
                                    prop.className "text-xs underline hover:text-base-content/70"
                                    prop.text "Edit"
                                    prop.onClick (fun _ -> dispatch (SetStep Payment))
                                ]
                            ]
                        ]
                        Html.p [
                            prop.className "text-sm text-base-content/70"
                            prop.text (model.PaymentMethod.ToString())
                        ]
                    ]
                ]

                // Items
                Html.div [
                    prop.className "border border-base-300 rounded-lg p-5 md:p-6 space-y-3"
                    prop.children [
                        Html.h3 [
                            prop.className "text-xs font-semibold uppercase tracking-[0.25em]"
                            prop.text "Order Items"
                        ]
                        Html.div [
                            prop.className "space-y-3"
                            prop.children [
                                for item in model.Cart.Items do
                                    let idx, imgLabel, name, color, size, quantity, price = getCartLineItemDetails item
                                    Html.div [
                                        prop.key idx
                                        prop.className "flex gap-3 pb-3 border-b border-base-300 last:border-0"
                                        prop.children [
                                            Html.div [
                                                prop.className "w-16 h-16 bg-base-200 rounded-md flex items-center justify-center text-xl font-light text-base-content/30 flex-shrink-0"
                                                prop.text imgLabel
                                            ]
                                            Html.div [
                                                prop.className "flex-1"
                                                prop.children [
                                                    Html.p [
                                                        prop.className "text-sm font-medium"
                                                        prop.text name
                                                    ]
                                                    Html.p [
                                                        prop.className "text-xs text-base-content/60"
                                                        prop.text $"{color} - {size} - Qty: {quantity}"
                                                    ]
                                                ]
                                            ]
                                            Html.p [
                                                prop.className "text-sm font-light"
                                                prop.text (sprintf "$%.2f" (price * decimal quantity))
                                            ]
                                        ]
                                    ]
                            ]
                        ]
                    ]
                ]

                Html.div [
                    prop.className "flex flex-col md:flex-row gap-3 pt-4"
                    prop.children [
                        Html.button [
                            prop.className "btn btn-primary rounded-none flex-1"
                            prop.text "Continue shopping"
                            prop.onClick (fun _ -> ())
                        ]
                    ]
                ]
            ]
        ]

    type PreviewTotalField =
        | OrderSubtotal
        | OrderShipping
        | OrderTax
        | OrderTotal

    let getTotalStringOrDefault (previewTotalOpt: PreviewTotals option) field =
        match previewTotalOpt with
        | None -> "-"
        | Some pt -> 
            match field with
            | OrderSubtotal -> pt.Subtotal
            | OrderShipping -> pt.Shipping
            | OrderTax -> pt.Tax
            | OrderTotal -> pt.Total
            |> fun x -> x.ToString("F2")
            |> fun x ->
                printfn $"x: {x}"
                x

    [<ReactComponent>]
    let CheckoutSummarySidebar (model: Model) =
        Html.div [
            prop.className "sticky top-28 border border-base-300 rounded-lg p-6 md:p-8 space-y-6 bg-base-100"
            prop.children [
                Html.h2 [
                    prop.className "text-lg md:text-xl font-light pb-3 border-b border-base-300"
                    prop.text "Order Summary"
                ]

                Html.div [
                    prop.className "space-y-3 text-sm"
                    prop.children [
                        for item in model.Cart.Items do
                            let idx, imgLabel, name, color, size, quantity, price = getCartLineItemDetails item
                            Html.div [
                                prop.key idx
                                prop.className "flex items-center gap-3"
                                prop.children [
                                    Html.img [
                                        prop.className "w-14 h-14 bg-base-200 rounded-md flex items-center justify-center text-lg font-light text-base-content/30 flex-shrink-0"
                                        prop.src imgLabel
                                        prop.alt name
                                    ]
                                    Html.div [
                                        prop.className "flex-1 min-w-0"
                                        prop.children [
                                            Html.p [
                                                prop.className "text-sm font-medium truncate"
                                                prop.text (name.Split("/") |> Array.tryHead |> Option.defaultValue "")
                                            ]
                                            Html.p [
                                                prop.className "text-[0.7rem] text-base-content/60"
                                                prop.text $"Color: {color} - Size: {size}"
                                            ]
                                            Html.p [
                                                prop.className "text-[0.7rem] text-base-content/60"
                                                prop.text $"Qty: {quantity}"
                                            ]
                                        ]
                                    ]
                                    Html.p [
                                        prop.className "text-sm font-light"
                                        prop.text (sprintf "$ %.2f" (price * decimal quantity))
                                    ]
                                ]
                            ]
                    ]
                ]

                Html.div [
                    prop.className "space-y-3 pt-4 border-t border-base-300 text-sm"
                    prop.children [
                        Html.div [
                            prop.className "flex items-center justify-between text-base-content/70"
                            prop.children [
                                Html.span "Subtotal"
                                Html.span ("$ " + getTotalStringOrDefault model.PreviewTotals OrderSubtotal)
                            ]
                        ]
                        Html.div [
                            prop.className "flex items-center justify-between text-base-content/70"
                            prop.children [
                                Html.span "Shipping"
                                Html.span (
                                    getTotalStringOrDefault model.PreviewTotals OrderShipping
                                    |> fun x -> if x = "0.00" then "FREE" else "$ " + x)
                            ]
                        ]
                        Html.div [
                            prop.className "flex items-center justify-between text-base-content/70"
                            prop.children [
                                Html.span "Tax"
                                Html.span ("$ " + getTotalStringOrDefault model.PreviewTotals OrderTax)
                            ]
                        ]
                        Html.div [
                            prop.className "pt-3 border-t border-base-300 flex items-center justify-between text-lg"
                            prop.children [
                                Html.span [ prop.className "font-medium"; prop.text "Total" ]
                                Html.span [ prop.className "font-light";  prop.text ("$ " + getTotalStringOrDefault model.PreviewTotals OrderTotal) ]
                            ]
                        ]
                    ]
                ]

                Html.div [
                    prop.className "space-y-2 pt-4 border-t border-base-300 text-xs md:text-sm text-base-content/70"
                    prop.children [
                        Html.div [
                            prop.className "flex items-center gap-2"
                            prop.children [
                                Html.span "🔒"
                                Html.span "Secure checkout with Stripe"
                            ]
                        ]
                        Html.div [
                            prop.className "flex items-center gap-2"
                            prop.children [
                                Html.span "✓"
                                Html.span "Free returns within 30 days"
                            ]
                        ]
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    let Checkout (model: Model) (dispatch: Msg -> unit) =
        Html.section [
            prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 md:py-20"
            prop.children [
                CheckoutStepper model

                match model.Step, model.OrderConfirmation with
                | Review, Some confirmationResp ->
                    OrderConfirmation.View confirmationResp
                | _ -> 
                    Html.div [
                        prop.className "grid grid-cols-1 lg:grid-cols-3 gap-10 lg:gap-12"
                        prop.children [
                            Html.div [
                                prop.className "lg:col-span-2 space-y-8"
                                prop.children [
                                    match model.Step with
                                    | Shipping -> CheckoutShipping model dispatch
                                    | Payment  -> CheckoutPayment model dispatch
                                    | _ -> ()
                                ]
                            ]
                            Html.div [
                                prop.className "lg:col-span-1"
                                prop.children [ CheckoutSummarySidebar model ]
                            ]
                        ]
                    ]
            ]
        ]
