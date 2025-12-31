module Components.FSharp.Shop

open Elmish
open Feliz
open Client.Domain.Store
open Client.Domain
open Client.Domain.Store.Checkout
open Client.Components.Shop
open Client.Components.Shop.Common
open Client.Components.Shop.Common.Checkout
open Client.Components.Shop.Collection
open Client.Components.Shop.ShopHero
open Client.Domain.Store.PrintfulMapping
open Shared.Store.Cart
open Shared
open Shared.Store
open Shared.Api.Checkout
open Bindings.Stripe.StripePayment

let updateCheckoutModelWithCheckoutPreviewResponse model (draftResponse: CreateDraftOrderResponse) : Model =
    match draftResponse with
    | CreatedTemp tempDraft ->
        { model with
            Cart = {
                model.Cart with
                    Items =
                        tempDraft.PreviewLines
                        |> List.map (fun previewLine ->
                            tryFindProductByKindInCart
                                previewLine.Item.Kind
                                previewLine.Item
                                model.Cart.Items
                        )
                        |> List.choose id
            } 
            PreviewTotals   =
                Some {
                    Currency = "usd"
                    ShippingName = tempDraft.DraftOrderTotals.ShippingName
                    Subtotal = tempDraft.DraftOrderTotals.Subtotal
                    Tax      = tempDraft.DraftOrderTotals.Tax
                    Shipping = tempDraft.DraftOrderTotals.Shipping
                    Total    = tempDraft.DraftOrderTotals.Total
                }
            IsBusy         = false
            Error          = None
        }
    | CreatedFinal finalDraft ->
        { model with
            Cart = {
                model.Cart with
                    Items =
                        finalDraft.OrderLines
                        |> List.map (fun previewLine ->
                            tryFindProductByKindInCart
                                previewLine.Item.Kind
                                previewLine.Item
                                model.Cart.Items
                        )
                        |> List.choose id
            } 
            PreviewTotals   =
                Some {
                    Currency = "usd"
                    ShippingName = finalDraft.OrderTotals.ShippingName
                    Subtotal = finalDraft.OrderTotals.Subtotal
                    Tax      = finalDraft.OrderTotals.Tax
                    Shipping = finalDraft.OrderTotals.Shipping
                    Total    = finalDraft.OrderTotals.Total
                }
            StripeClientSecret = Some finalDraft.StripeSecret
            StripeSessionId = Some finalDraft.StripePaymentIntentId
            PrintfulDraftId = Some finalDraft.DraftOrderId
            IsBusy         = false
            Error          = None
        }

let update (msg: ShopMsg) (model: Store.Model) : Store.Model * Cmd<ShopMsg> =
    printfn $"SHOP UPDATE MSG: {msg}"
    match msg with

    | RemoveCartItem cartItem ->
        // persist to local storage for recently removed?
        let updatedCartState = withRemovedItem cartItem model.Cart
        { model with Cart = updatedCartState; Checkout = None }, Cmd.none
    
    | AddCartItem cartItem ->
        let updatedCartState = model.Cart.Items @ [ cartItem ] |> withItems
        { model with Cart = updatedCartState; Checkout = None }, Cmd.none

    | UpdateCartQty (cartItem, qty) ->
        let updatedCartState = withUpdatedItemQuantity cartItem qty model.Cart
        { model with Cart = updatedCartState; Checkout = None }, Cmd.none

    | BeginCheckoutFromCart ->
        match model.Checkout with
        | None -> Checkout.Iniitializers.initCheckoutModel model.Cart
        | Some checkoutModel -> checkoutModel
        |> fun mdl ->
                { model with Section = ShopSection.Checkout; Checkout = Some mdl }, Cmd.none

    | CreatedDraftOrder response ->
        let chkoutModel =
            match model.Checkout with
            | None -> None
            | Some chkt ->
                updateCheckoutModelWithCheckoutPreviewResponse chkt response
                |> Some
        match response with
        | CreatedTemp _ ->
            { model with Checkout = chkoutModel },
            Cmd.ofMsg LoadStripe
        | CreatedFinal final ->
            { model with
                Stripe = Some (initStripe final.StripeSecret final.StripePaymentIntentId)
                Checkout = chkoutModel
            },
            Cmd.ofMsg LoadStripe
    
    | CheckoutDraftFailed response ->
        { model with Error = Some response },
        Cmd.none

    | LoadStripe -> 
        model,
        Cmd.OfPromise.either
            loadStripe
            Env.stripePublishableKey
            StripeLoaded
            StripeFailed
    
    | StripeFailed ex ->
        { model with Error = Some ex.Message }, Cmd.none
    | StripeLoaded stripe ->
        match model.Stripe with
        | Some stripeMdl ->
            match model.Checkout with
            | None ->
                { model with Stripe = Some stripeMdl },
                Cmd.none
            | Some chkout ->
                { model with
                    Checkout = Some { chkout with Stripe = Some stripe  } 
                    Stripe = Some stripeMdl
                },
                Cmd.ofMsg (CheckoutMsg (SetStep Payment))
        | None -> model, Cmd.none

    | CheckoutMsg subMsg ->
        let additionalCmd =
            match subMsg with
            | SubmitShipping ->
                match model.Checkout with
                | None -> Cmd.none
                | Some mdl ->          
                    Cmd.OfAsync.either
                        Client.Api.checkoutApi.CreateDraftOrder
                        {
                            items =  toLineItems model.Cart.Items
                            shippingOptionId = "STANDARD"
                            totalsCents = 
                                match mdl.PreviewTotals with
                                | None -> 0
                                | Some pt -> int (pt.Total * 100m)
                            address = {
                                name = (mdl.CustomerShippingInfo.FirstName + " " + mdl.CustomerShippingInfo.LastName)
                                address1     = mdl.CustomerShippingInfo.Address
                                city      = mdl.CustomerShippingInfo.City
                                state     = mdl.CustomerShippingInfo.State
                                postalCode       = mdl.CustomerShippingInfo.ZipCode
                                countryCode   = mdl.CustomerShippingInfo.Country
                                email = mdl.CustomerShippingInfo.Email
                                phone = 
                                    if System.String.IsNullOrWhiteSpace mdl.CustomerShippingInfo.Phone
                                    then None
                                    else Some mdl.CustomerShippingInfo.Phone
                            }
                            isTemp = false
                        }
                        CreatedDraftOrder
                        (fun ex -> CheckoutDraftFailed ex.Message)
            | _ -> Cmd.none

        match model.Checkout with
        | None -> Checkout.Iniitializers.initCheckoutModel model.Cart
        | Some checkoutModel -> checkoutModel
        |> Checkout.Checkout.update subMsg
        |> fun (mdl, cmd) ->    
            { model with Section = ShopSection.Checkout; Checkout = Some mdl },
            Cmd.batch [
                additionalCmd
                Cmd.map CheckoutMsg cmd
            ]

    | ShopDesignerMsg subMsg ->
        match model.Section with
        | ProductDesigner pd ->
            let productDesignerMdl, cmd' = Designer.update subMsg pd
            let cmd =
                match subMsg with
                | ProductDesigner.BackToDropLanding ->
                    Cmd.ofMsg (NavigateTo ShopSection.ShopLanding)

                | ProductDesigner.AddToCart qty ->
                    match productDesignerMdl |> toCustomCartDU qty with
                    | Some cartItem ->  Cmd.ofMsg (ShopMsg.AddCartItem (cartItem))
                    | None -> Cmd.none
                | _ ->
                    cmd' |> Cmd.map ShopDesignerMsg

            { model with Section = ProductDesigner productDesignerMdl }, cmd
        | _ -> 
            { model with Section = ProductDesigner (ProductDesigner.initialModel()) },
            Cmd.map ShopDesignerMsg (Cmd.ofMsg ProductDesigner.LoadProducts)

    | ShopCollectionMsg subMsg ->
        match model.Section with
        | CollectionBrowser cb ->
            let updatedModel, cmd' =
                match subMsg with
                | Collection.ViewSyncProduct sp ->
                    let seed = Shared.StoreProductViewer.ProductSeed.SeedSync sp
                    let pvModel, _ =
                        Product.initFromSeed
                            seed
                            Shared.StoreProductViewer.ReturnTo.BackToCollection
                    { model with Section = ProductViewer pvModel }, Cmd.ofMsg (ShopProduct ProductViewer.Msg.LoadDetails)
                | _ ->
                    State.update subMsg cb
                    |> fun (m, cmd) -> 
                        { model with Section = CollectionBrowser m }, 
                        cmd |> Cmd.map ShopCollectionMsg
            updatedModel, cmd' 

        | ProductViewer _ -> model, Cmd.none
        | _ ->
            let landingModel, msg = State.init None            
            { model with Section = CollectionBrowser landingModel },
            Cmd.map ShopCollectionMsg msg

    | ShopProduct subMsg ->
        match model.Section with
        | ProductViewer pv ->
            let viewMdl, cmd' = 
                match subMsg with
                | ProductViewer.GoBack ->
                    let initCB = Collection.initModel ()
                    model, Cmd.ofMsg (NavigateTo (CollectionBrowser initCB))
                | ProductViewer.PrimaryAction ->
                    match pv.Details, pv.SelectedVariantId with
                    | UseDeferred.Deferred.Resolved d, Some evid ->
                        match tryMakeSyncCartItem 1 d.product evid with
                        | Some item -> model, Cmd.ofMsg (ShopMsg.AddCartItem (CartLineItem.Sync item))
                        | None      -> model, Cmd.none
                    | _ ->
                        model, Cmd.none
                | _ ->
                    Product.update subMsg pv
                    |> fun (mdl', cmd') -> 
                    { model with Section = ProductViewer mdl' }
                    , cmd' |> Cmd.map ShopProduct
            viewMdl, cmd'
        | _ ->
            let landingModel = 
                ProductViewer.initModel 
                    (Shared.StoreProductViewer.ProductKey.Template 0)
                    None
                    Shared.StoreProductViewer.ReturnTo.BackToCollection    
            { model with Section = ProductViewer landingModel },
            Cmd.ofMsg (NavigateTo (ProductViewer landingModel))

    | ShopOrderHistory histryMsg ->
        match model.Section with
        | OrderHistory ohm ->
            OrderHistory.update histryMsg ohm
            |> fun (mdl', cmd') -> 
            { model with Section = OrderHistory mdl' }
            , cmd' |> Cmd.map ShopOrderHistory
        | _ ->
            let ohm = OrderHistory.initModel()
            { model with Section = OrderHistory ohm },
            Cmd.ofMsg (NavigateTo (OrderHistory ohm))
            
    | ShopLanding msg ->
        match msg with
        | ShopLandingMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)

    | NavigateTo section ->
        // need to do url here?
        match section with
        | ShopSection.CollectionBrowser cb ->
            // fresh designer state
            let model' = { model with Section = ShopSection.CollectionBrowser cb }
            // immediately ask the designer to load products
            model', Cmd.ofMsg (ShopCollectionMsg Collection.LoadMore)
        | ShopSection.ProductDesigner pd ->
            // fresh designer state
            let model' = { model with Section = ShopSection.ProductDesigner pd }
            // immediately ask the designer to load products
            model', Cmd.ofMsg (ShopDesignerMsg ProductDesigner.Msg.LoadProducts)
        | ShopSection.ProductViewer pv ->
            let model' = { model with Section = ShopSection.ProductViewer pv }
            model', Cmd.ofMsg (ShopProduct ProductViewer.Msg.LoadDetails)
        | _ ->
            { model with Section = section }, Cmd.none

let tabToSection tab =
    match tab with
    | Hero -> ShopSection.ShopLanding
    | Collection -> ShopSection.CollectionBrowser (Collection.initModel())
    | Designer -> ShopSection.ProductDesigner (ProductDesigner.initialModel())
    | Product -> ShopSection.ProductViewer (ProductViewer.initModel (StoreProductViewer.ProductKey.Template 0) None StoreProductViewer.ReturnTo.BackToCollection )
    | Cart  -> ShopSection.Cart
    | Checkout -> ShopSection.Checkout
    | History -> ShopSection.OrderHistory (OrderHistory.initModel())

let sectionToTab section =
    match section with
    | ShopSection.ShopLanding -> Hero
    | CollectionBrowser _ -> Collection
    | ProductViewer _ -> Product
    | ProductDesigner _ -> Designer
    | ShopSection.Cart -> Cart
    | ShopSection.Checkout -> Checkout
    | ShopSection.OrderHistory _ -> History
    | NotFound -> Hero

type NavItemDisplay =
    | TextItem of string
    | IconItem of string * ReactElement


open Bindings.LucideIcon

let iconGlyph (icon: ReactElement) (count: int option) =
    Html.div [
        prop.className "relative inline-flex items-center justify-center"
        prop.children [
            // Bag icon itself
            icon

            // Little badge in the top-right corner
            match count with
            | Some c when c > 0 ->
                Html.div [
                    prop.className
                        "absolute -top-1 -right-2 flex items-center justify-center \
                            w-5 h-5 rounded-full \
                            bg-base-content text-base-100 text-[20px] font-bold pt-1 pb-2.5"
                    prop.text (if c > 9 then "9+" else string c)
                ]
            | _ -> Html.none
        ]
    ]

module Shop =

    [<ReactComponent>]
    let View model (dispatch: Store.ShopMsg -> unit) =
        let tab = sectionToTab model.Section

        let cartItems = model.Cart.Items

        let tabBtn (cartItems: list<CartLineItem>) t (navItemDisplay: NavItemDisplay) =
            match navItemDisplay with
            | TextItem txt ->
                Html.button [
                    prop.key txt
                    prop.className (
                        Ui.tw [
                            "cormorant-font text-xs sm:text-sm font-medium uppercase tracking-[0.2em] pb-1 \
                            transition-all border-b-2 border-transparent"
                            if tab = t then "text-base-content border-base-content"
                            else "text-base-content/50 hover:text-base-content"
                        ]
                    )
                    prop.text txt
                    prop.onClick (fun _ -> dispatch (ShopMsg.NavigateTo (tabToSection t)) )
                ]

            | IconItem (k, _ico) ->
                let count = 
                    if t = Cart
                    then Some cartItems.Length
                    else None
                Html.button [
                    prop.key k
                    prop.className (
                        Ui.tw [
                            "relative inline-flex items-center justify-center h-10 w-10 \
                            border-b-2 border-transparent"
                            if tab = t then "text-base-content border-base-content"
                            else "text-base-content/60 hover:text-base-content"
                        ]
                    )
                    prop.onClick (fun _ -> dispatch (ShopMsg.NavigateTo (tabToSection t)) )
                    prop.children [ 
                        iconGlyph _ico count
                    ]
                ]




        Html.div [
            prop.className "min-h-screen bg-base-100 text-base-content"
            prop.children [

                // Top tab bar
                Html.div [
                    prop.className "nav-main"
                    prop.children [
                        // Ui.Section.container [
                            Html.div [
                                prop.className "nav-content"
                                prop.style [ style.display.flex; style.gap 60; style.justifyContent.spaceAround; style.justifySelf.center ]
                                prop.children [
                                    tabBtn cartItems Hero (TextItem "Xero")
                                    tabBtn cartItems Collection (TextItem "collection")
                                    // tabBtn Designer   "designer"
                                    match model.Section with
                                    | ProductViewer _ -> tabBtn cartItems Product (TextItem "product")
                                    | _ -> ()
                                ]
                            ]
                            Html.div [
                                prop.style [ style.display.flex; style.justifySelf.end' ]
                                prop.children [
                                    tabBtn cartItems History (IconItem ("history", (Bindings.LucideIcon.LucideIcon.PackageSearch "w-7 h-7 text-base-content/70")))
                                    tabBtn cartItems Cart (IconItem ("cart", (Bindings.LucideIcon.LucideIcon.ShoppingCart "w-7 h-7 text-base-content/70")))
                                ]
                            ]
                    ]
                ]

                // Active body
                match model.Section with
                | ShopSection.ShopLanding ->                    
                    Html.div [
                        Hero.View {
                            OnShopCollection = fun () -> dispatch (ShopMsg.NavigateTo (tabToSection Collection))
                            OnExploreMore    = fun () -> dispatch (ShopMsg.NavigateTo (tabToSection Designer))
                        }
                        // needs api call
                        // Sections.FeaturedProductsSection()
                        Sections.StoryProgressiveSection()
                        Sections.PhilosophySection()
                        Sections.FinalCtaSection (fun () -> dispatch (ShopMsg.NavigateTo (tabToSection Collection)))
                    ]

                | CollectionBrowser cb ->
                    Collection.View cb (ShopCollectionMsg >> dispatch)

                | ProductDesigner pd ->
                    Designer.View pd (ShopDesignerMsg >> dispatch)
                
                | ProductViewer pv ->
                    Product.View pv (ShopProduct >> dispatch)

                | ShopSection.Cart ->
                    Cart.Cart.View 
                        {
                            Cart = model.Cart
                            OnRemove = fun x -> dispatch (RemoveCartItem x) 
                            OnUpdateQuantity  = fun itemAndQty -> dispatch (UpdateCartQty itemAndQty) 
                            OnContinueShopping = fun _ -> dispatch (ShopMsg.NavigateTo (tabToSection Collection))
                            OnCheckout         = fun _ -> dispatch BeginCheckoutFromCart
                        }

                | ShopSection.Checkout ->
                    Checkout.View.Checkout 
                        (match model.Checkout with
                        | None -> Checkout.Iniitializers.initCheckoutModel model.Cart
                        | Some cmdl -> cmdl)
                        (CheckoutMsg >> dispatch)

                | OrderHistory ohm -> OrderHistory.OrderLookupComponent ohm (ShopMsg.ShopOrderHistory >> dispatch)
                | NotFound -> Html.div "Not found placeholder...."
            ]
        ]

[<ReactComponent>]
let ShopView (model: Store.Model) (dispatch: Store.ShopMsg -> unit) =
    Shop.View model dispatch