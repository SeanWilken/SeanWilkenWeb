namespace Client.Components.Shop.Checkout

open Feliz
open Client.Components.Shop.Common
open Shared.Store.Cart

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

    type ShippingMethod =
        | Standard
        | Priority
        | Express

    type PaymentMethod =
        | Card
        | ApplePay
        | GooglePay

    type Model = {
        Step           : CheckoutStep
        ShippingInfo   : ShippingInfo
        ShippingMethod : ShippingMethod
        PaymentMethod  : PaymentMethod
        Items          : CartLineItem list
    }

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

    type Msg =
        | SetStep of CheckoutStep
        | UpdateShippingField of Field * string
        | SelectShippingMethod of ShippingMethod
        | SelectPaymentMethod of PaymentMethod
        | SubmitShipping
        | SubmitPayment
        | PlaceOrder
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
        Country   = "United States"
        Phone     = ""
    }

    let placedUpcharge (d: Client.Domain.Store.ProductDesigner.Designs.DesignOptions) =
        match d.HitArea with
        | Shared.PrintfulCommon.DesignHitArea.OutsideLabel -> 5m
        | _ -> 10m


    let computeTotals (items: CartLineItem list) (shippingMethod: ShippingMethod) =
        let subtotal =
            items
            |> List.sumBy (fun i -> 
                match i with
                | Custom c -> c.UnitPrice * decimal c.Quantity
                | Template t -> t.UnitPrice * decimal t.Quantity
            )

        let shippingCost =
            match shippingMethod with
            | Express  -> 25m
            | Priority -> 15m
            | Standard ->
                if subtotal > 100m then 0m else 12m

        let tax = subtotal * 0.08m
        let total = subtotal + tax + shippingCost
        subtotal, shippingCost, tax, total

    let private labelStep (step: CheckoutStep) =
        match step with
        | Shipping -> "Shipping"
        | Payment  -> "Payment"
        | Review   -> "Review"

    let private shippingMethodKey = function
        | Standard -> "standard"
        | Priority -> "priority"
        | Express  -> "express"

    let private shippingMethodDisplay = function
        | Standard -> "Standard Shipping"
        | Priority -> "Priority Shipping"
        | Express  -> "Express Shipping"

    let private paymentMethodDisplay = function
        | Card      -> "Credit / Debit Card"
        | ApplePay  -> "Apple Pay"
        | GooglePay -> "Google Pay"

    let private stepper (model: Model) (dispatch: Msg -> unit) =
        let steps =
            [ Shipping; Payment; Review ]

        Html.div [
            prop.className "mb-10 md:mb-12"
            prop.children [
                Html.h1 [
                    prop.className "text-3xl md:text-4xl font-light mb-6 md:mb-8"
                    prop.text "Checkout"
                ]
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
                                                    "w-10 h-10 md:w-12 md:h-12 rounded-full flex items-center justify-center text-xs md:text-sm font-medium transition-all duration-300"
                                                    if isComplete then "bg-primary text-primary-content"
                                                    elif isActive then "bg-primary text-primary-content scale-110"
                                                    else "bg-base-200 text-base-content/50"
                                                ])
                                                prop.text (
                                                    if isComplete then "✓"
                                                    else labelStep step  // just first letter if you like
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

    let private shippingForm (model: Model) (dispatch: Msg -> unit) =
        let subtotal, shippingCost, _, _ =
            computeTotals model.Items model.ShippingMethod

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
                                        prop.value model.ShippingInfo.Email
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
                                                prop.value model.ShippingInfo.FirstName
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
                                                prop.value model.ShippingInfo.LastName
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
                                        prop.value model.ShippingInfo.Address
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
                                        prop.value model.ShippingInfo.Apartment
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
                                                prop.value model.ShippingInfo.City
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
                                                prop.value model.ShippingInfo.State
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
                                                prop.value model.ShippingInfo.ZipCode
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
                                        prop.value model.ShippingInfo.Phone
                                        prop.onChange (fun v -> dispatch (UpdateShippingField (Phone, v)))
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]

                // Shipping method
                Html.div [
                    prop.className "space-y-5 pt-6 border-t border-base-300"
                    prop.children [
                        Html.h2 [
                            prop.className "text-2xl font-light"
                            prop.text "Shipping Method"
                        ]

                        Html.div [
                            prop.className "space-y-3"
                            prop.children [
                                let methods =
                                    [
                                        Standard, "Standard Shipping", "5–7 business days",
                                            (if subtotal > 100m then 0m else 12m)
                                        Priority, "Priority Shipping", "2–3 business days", 15m
                                        Express,  "Express Shipping",  "1–2 business days", 25m
                                    ]

                                for (m, name, time, price) in methods do
                                    let isActive = model.ShippingMethod = m

                                    Html.div [
                                        prop.key (shippingMethodKey m)
                                        prop.className (Ui.tw [
                                            "border-2 p-4 md:p-5 cursor-pointer transition-all"
                                            if isActive then "border-primary bg-base-200/60"
                                            else "border-base-300 hover:border-base-content"
                                        ])
                                        prop.onClick (fun _ -> dispatch (SelectShippingMethod m))
                                        prop.children [
                                            Html.div [
                                                prop.className "flex items-center justify-between"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center gap-3"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className (Ui.tw [
                                                                    "w-5 h-5 rounded-full border-2 flex items-center justify-center"
                                                                    if isActive then "border-primary"
                                                                    else "border-base-300"
                                                                ])
                                                                prop.children [
                                                                    if isActive then
                                                                        Html.div [
                                                                            prop.className "w-2.5 h-2.5 rounded-full bg-primary"
                                                                        ]
                                                                ]
                                                            ]
                                                            Html.div [
                                                                Html.p [
                                                                    prop.className "font-medium"
                                                                    prop.text name
                                                                ]
                                                                Html.p [
                                                                    prop.className "text-xs md:text-sm text-base-content/60"
                                                                    prop.text time
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm md:text-lg font-light"
                                                        prop.text (if price = 0m then "FREE" else sprintf "$%.0f" price)
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                            ]
                        ]
                    ]
                ]

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
                            prop.type' "submit"
                            prop.text "Continue to Payment"
                        ]
                    ]
                ]
            ]
        ]

    let private paymentForm (model: Model) (dispatch: Msg -> unit) =
        Html.form [
            prop.className "space-y-8"
            prop.onSubmit (fun e ->
                e.preventDefault()
                dispatch SubmitPayment
            )
            prop.children [

                Html.div [
                    prop.className "space-y-5"
                    prop.children [
                        Html.h2 [
                            prop.className "text-2xl font-light"
                            prop.text "Payment Method"
                        ]

                        Html.div [
                            prop.className "space-y-3"
                            prop.children [
                                let methods =
                                    [
                                        Card,      "Credit / Debit Card", "Secure payment via Stripe"
                                        ApplePay,  "Apple Pay",           "Fast checkout with Apple Pay"
                                        GooglePay, "Google Pay",          "Fast checkout with Google Pay"
                                    ]

                                for (m, name, desc) in methods do
                                    let isActive = model.PaymentMethod = m
                                    Html.div [
                                        prop.key (paymentMethodDisplay m)
                                        prop.className (Ui.tw [
                                            "border-2 p-4 md:p-5 cursor-pointer transition-all"
                                            if isActive then "border-primary bg-base-200/60"
                                            else "border-base-300 hover:border-base-content"
                                        ])
                                        prop.onClick (fun _ -> dispatch (SelectPaymentMethod m))
                                        prop.children [
                                            Html.div [
                                                prop.className "flex items-center gap-3"
                                                prop.children [
                                                    Html.div [
                                                        prop.className (Ui.tw [
                                                            "w-5 h-5 rounded-full border-2 flex items-center justify-center"
                                                            if isActive then "border-primary"
                                                            else "border-base-300"
                                                        ])
                                                        prop.children [
                                                            if isActive then
                                                                Html.div [
                                                                    prop.className "w-2.5 h-2.5 rounded-full bg-primary"
                                                                ]
                                                        ]
                                                    ]
                                                    Html.div [
                                                        Html.p [
                                                            prop.className "font-medium"
                                                            prop.text name
                                                        ]
                                                        Html.p [
                                                            prop.className "text-xs md:text-sm text-base-content/60"
                                                            prop.text desc
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                            ]
                        ]
                    ]
                ]

                if model.PaymentMethod = Card then
                    Html.div [
                        prop.className "space-y-5 pt-6 border-t border-base-300"
                        prop.children [
                            Html.div [
                                prop.className "bg-base-200/70 p-5 md:p-6 rounded-lg space-y-4"
                                prop.children [
                                    Html.p [
                                        prop.className "text-xs md:text-sm text-base-content/70 flex items-center gap-2"
                                        prop.text "Secured by Stripe – Your payment information is encrypted."
                                    ]

                                    Html.div [
                                        prop.className "space-y-4"
                                        prop.children [
                                            Html.div [
                                                Html.label [
                                                    prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                                    prop.text "Card Number"
                                                ]
                                                Html.input [
                                                    prop.className "input input-bordered w-full rounded-none bg-base-100"
                                                    prop.placeholder "1234 5678 9012 3456"
                                                    prop.required true
                                                ]
                                            ]
                                            Html.div [
                                                prop.className "grid grid-cols-1 sm:grid-cols-2 gap-4"
                                                prop.children [
                                                    Html.div [
                                                        Html.label [
                                                            prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                                            prop.text "Expiry Date"
                                                        ]
                                                        Html.input [
                                                            prop.className "input input-bordered w-full rounded-none bg-base-100"
                                                            prop.placeholder "MM / YY"
                                                            prop.required true
                                                        ]
                                                    ]
                                                    Html.div [
                                                        Html.label [
                                                            prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                                            prop.text "CVC"
                                                        ]
                                                        Html.input [
                                                            prop.className "input input-bordered w-full rounded-none bg-base-100"
                                                            prop.placeholder "123"
                                                            prop.required true
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.div [
                                                Html.label [
                                                    prop.className "block text-xs font-semibold uppercase tracking-[0.25em] mb-2"
                                                    prop.text "Cardholder Name"
                                                ]
                                                Html.input [
                                                    prop.className "input input-bordered w-full rounded-none bg-base-100"
                                                    prop.placeholder "Name on card"
                                                    prop.required true
                                                ]
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "flex items-center gap-2 pt-2"
                                        prop.children [
                                            Html.input [
                                                prop.className "checkbox checkbox-xs"
                                                prop.type' "checkbox"
                                                prop.defaultChecked true
                                            ]
                                            Html.span [
                                                prop.className "text-xs md:text-sm text-base-content/70"
                                                prop.text "Billing address same as shipping"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                Html.div [
                    prop.className "flex flex-col md:flex-row gap-3 pt-6"
                    prop.children [
                        Html.button [
                            prop.className "btn btn-outline rounded-none flex-1"
                            prop.type' "button"
                            prop.text "Back to Shipping"
                            prop.onClick (fun _ -> dispatch (SetStep Shipping))
                        ]
                        Html.button [
                            prop.className "btn btn-primary rounded-none flex-1"
                            prop.type' "submit"
                            prop.text "Review Order"
                        ]
                    ]
                ]
            ]
        ]

    let getCartLineItemDetails item = //imgLabel, name, color, size, quantity, price =
        match item with
        | Custom c ->
            (string c.CatalogProductId) + ":" + (string c.CatalogVariantId)
            , c.ThumbnailUrl, c.Name, c.ColorName, c.Size, c.Quantity, c.UnitPrice
        | Template t ->
            (string t.CatalogProductId) + ":" + (string t.VariantId) + ":" + (string t.TemplateId)
            , t.PreviewImage |> Option.defaultValue t.Name, t.Name, t.ColorName, t.Size, t.Quantity, t.UnitPrice

    let private review (model: Model) (dispatch: Msg -> unit) =
        let subtotal, shippingCost, tax, total =
            computeTotals model.Items model.ShippingMethod

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
                                Html.p $"{model.ShippingInfo.FirstName} {model.ShippingInfo.LastName}"
                                Html.p model.ShippingInfo.Address
                                if not (System.String.IsNullOrWhiteSpace model.ShippingInfo.Apartment) then
                                    Html.p model.ShippingInfo.Apartment
                                Html.p $"{model.ShippingInfo.City}, {model.ShippingInfo.State} {model.ShippingInfo.ZipCode}"
                                Html.p model.ShippingInfo.Phone
                                Html.p [
                                    prop.className "mt-1"
                                    prop.text model.ShippingInfo.Email
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
                            prop.text (shippingMethodDisplay model.ShippingMethod)
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
                            prop.text (paymentMethodDisplay model.PaymentMethod)
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
                                for item in model.Items do
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
                            prop.className "btn btn-outline rounded-none flex-1"
                            prop.text "Back to Payment"
                            prop.onClick (fun _ -> dispatch (SetStep Payment))
                        ]
                        Html.button [
                            prop.className "btn btn-primary rounded-none flex-1"
                            prop.text "Place Order"
                            prop.onClick (fun _ -> dispatch PlaceOrder)
                        ]
                    ]
                ]
            ]
        ]

    let private summarySidebar (model: Model) =
        let subtotal, shippingCost, tax, total =
            computeTotals model.Items model.ShippingMethod

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
                        for item in model.Items do
                            let idx, imgLabel, name, color, size, quantity, price = getCartLineItemDetails item
                            Html.div [
                                prop.key idx
                                prop.className "flex items-center gap-3"
                                prop.children [
                                    Html.div [
                                        prop.className "w-14 h-14 bg-base-200 rounded-md flex items-center justify-center text-lg font-light text-base-content/30 flex-shrink-0"
                                        prop.text imgLabel
                                    ]
                                    Html.div [
                                        prop.className "flex-1 min-w-0"
                                        prop.children [
                                            Html.p [
                                                prop.className "text-sm font-medium truncate"
                                                prop.text name
                                            ]
                                            Html.p [
                                                prop.className "text-[0.7rem] text-base-content/60"
                                                prop.text $"Qty: {quantity}"
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

                Html.div [
                    prop.className "space-y-3 pt-4 border-t border-base-300 text-sm"
                    prop.children [
                        Html.div [
                            prop.className "flex items-center justify-between text-base-content/70"
                            prop.children [
                                Html.span "Subtotal"
                                Html.span (sprintf "$%.2f" subtotal)
                            ]
                        ]
                        Html.div [
                            prop.className "flex items-center justify-between text-base-content/70"
                            prop.children [
                                Html.span "Shipping"
                                Html.span (if shippingCost = 0m then "FREE" else sprintf "$%.2f" shippingCost)
                            ]
                        ]
                        Html.div [
                            prop.className "flex items-center justify-between text-base-content/70"
                            prop.children [
                                Html.span "Tax"
                                Html.span (sprintf "$%.2f" tax)
                            ]
                        ]
                        Html.div [
                            prop.className "pt-3 border-t border-base-300 flex items-center justify-between text-lg"
                            prop.children [
                                Html.span [ prop.className "font-medium"; prop.text "Total" ]
                                Html.span [ prop.className "font-light";  prop.text (sprintf "$%.2f" total) ]
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

    let view (model: Model) (dispatch: Msg -> unit) =
        Html.section [
            prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 md:py-20"
            prop.children [
                stepper model dispatch

                Html.div [
                    prop.className "grid grid-cols-1 lg:grid-cols-3 gap-10 lg:gap-12"
                    prop.children [
                        Html.div [
                            prop.className "lg:col-span-2 space-y-8"
                            prop.children [
                                match model.Step with
                                | Shipping -> shippingForm model dispatch
                                | Payment  -> paymentForm model dispatch
                                | Review   -> review model dispatch
                            ]
                        ]
                        Html.div [
                            prop.className "lg:col-span-1"
                            prop.children [ summarySidebar model ]
                        ]
                    ]
                ]
            ]
        ]
