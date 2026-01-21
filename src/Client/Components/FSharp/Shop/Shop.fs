module Components.FSharp.Shop

open System
open Elmish
open Feliz
open Client.Components.Shop
open Client.Components.Shop.Common
open Client.Components.Shop.Common.Checkout
open Client.Components.Shop.Collection
open Client.Components.Shop.ShopHero
open Shared.Store.Cart
open Shared
open Shared.Store
open Shared.Api.Checkout
open Bindings.Stripe.StripePayment
open Feliz.UseDeferred
open Shared.StoreProductViewer
open Client.Components.Shop.Designer
open Client.Components.Shop.Checkout

module PrintfulMapping =

    let mapDesignToPlacement (d: Designs.DesignOptions) : PrintfulPlacement =
        {
            Placement = d.HitArea.ToPrintfulPlacement()
            Technique = d.Technique.ToPrintfulTechnique()
            Layers = [
                {
                    Type = "file"
                    Url = d.Asset.ImageUrl
                    Position = d.Position
                    LayerOptions = []
                }
            ]
        }

    let sizeToDefaultPosition =
        function
        | Small  -> { Width = 3.0; Height = 3.0; Left = 0.0; Top = 0.0 }
        | Medium -> { Width = 6.0; Height = 6.0; Left = 0.0; Top = 0.0 }
        | Large  -> { Width = 10.0; Height = 10.0; Left = 0.0; Top = 0.0 }

    let toPrintfulPlacements (placed: DesignOptions list) : PrintfulPlacement list =
        placed
        |> List.groupBy (fun d -> d.HitArea, d.Technique)
        |> List.map (fun ((hitArea, tech), ds) ->
            {
                Placement = hitArea.ToPrintfulPlacement()
                Technique = tech.ToPrintfulTechnique()
                Layers =
                    ds
                    |> List.map (fun d ->
                        {
                            Type = "file"
                            Url = d.Asset.ImageUrl
                            LayerOptions = d.LayerOptions
                            Position = d.Position |> Option.orElse (Some (sizeToDefaultPosition d.Size))
                        }
                    )
            }
        )

    let toCustomCartDU
        (qty   : int)
        (model : Designer.Model)
        : CartLineItem option =

        match model.SelectedVariantId,
            model.SelectedProduct,
            model.DesignOptions with

        // must have a variant, a base product, and at least one placed design
        | None, _, _ -> None
        | _, None, _ -> None
        | _, _, []   -> None

        | Some variantId, Some product, placedDesigns ->

            // 1. Build Printful placements from the placed designs
            let placements : PrintfulPlacement list =
                placedDesigns |> toPrintfulPlacements

            // 2. Preview image from the selected product
            let previewImage =

                product.thumbnailURL
                |> function
                    | null | "" -> None
                    | url       -> Some url

            // 3. Size + color from the designer selections
            let size =
                model.SelectedVariantSize
                |> Option.defaultValue ""

            // For now we’ll treat the selected color string as the "name",
            // and assume we *might* also use it as a hex code.
            let colorName, colorCodeOpt =
                match model.SelectedVariantColor with
                | None
                | Some "" ->
                    "Default", None
                | Some c ->
                    // naive heuristic: if it looks like a hex code, use it as both
                    if c.StartsWith("#") then c, Some c
                    else c, None

            // 4. Price — placeholder for now until you wire real pricing
            let unitPrice : decimal =
                // TODO: plug in a real price, from product/variant pricing
                45.0m

            // 5. Build the rich CartItem record (your commented-out type)
            let baseItem : CartItem =
                {
                    VariantId         = variantId
                    Placements        = placements
                    PreviewImage      = previewImage

                    CatalogProductId  = product.id
                    CatalogVariantId  = variantId
                    Name              = product.name
                    ThumbnailUrl      = product.thumbnailURL

                    Size              = size
                    ColorName         = colorName
                    ColorCodeOpt      = colorCodeOpt

                    Quantity          = qty
                    UnitPrice         = unitPrice
                    IsCustomDesign    = true
                }

            // 6. Wrap it in the DU
            Some (CartLineItem.Custom baseItem)

    let toTemplateCartDU
        (qty   : int)
        (model : Designer.Model)
        : CartLineItem option =

        match model.SelectedVariantId,
            model.SelectedProduct,
            model.DesignOptions with

        // must have a variant, a base product, and at least one placed design
        | None, _, _ -> None
        | _, None, _ -> None
        | _, _, []   -> None

        | Some variantId, Some product, placedDesigns ->

            // 1. Build Printful placements from the placed designs
            let placements : PrintfulPlacement list =
                placedDesigns |> toPrintfulPlacements

            // 2. Preview image from the selected product
            let previewImage =
                product.thumbnailURL
                |> function
                    | null | "" -> None
                    | url       -> Some url

            // 3. Size + color from the designer selections
            let size =
                model.SelectedVariantSize
                |> Option.defaultValue ""

            // For now we’ll treat the selected color string as the "name",
            // and assume we *might* also use it as a hex code.
            let colorName, colorCodeOpt =
                match model.SelectedVariantColor with
                | None
                | Some "" ->
                    "Default", None
                | Some c ->
                    // naive heuristic: if it looks like a hex code, use it as both
                    if c.StartsWith("#") then c, Some c
                    else c, None

            // 4. Price — placeholder for now until you wire real pricing
            let unitPrice : decimal =
                // TODO: plug in a real price, from product/variant pricing
                45.0m

            // 5. Build the rich CartItem record (your commented-out type)
            let baseItem : TemplateCartItem =
                {
                    VariantId = variantId
                    Quantity  = qty
                    TemplateId = 0
                    CatalogProductId = product.id
                    Name             = product.name
                    Size             = size
                    ColorName        = colorName
                    ColorCodeOpt     = colorCodeOpt
                    UnitPrice        = unitPrice
                    PreviewImage     = Some product.thumbnailURL
                }


            // 6. Wrap it in the DU
            Some (CartLineItem.Template baseItem)


    let tryMakeSyncCartItem (qty:int) (details: SyncProduct.SyncProduct) (selectedVariantId:int64) : SyncCartItem option =
        details.Variants
        |> List.tryFind (fun v -> v.Id = selectedVariantId)
        |> Option.map (fun v ->
            {
                SyncProductId = v.Id
                SyncVariantId = v.VariantId
                ExternalId = Some v.ExternalId
                CatalogVariantId = Some v.VariantProductVariantId
                Quantity      = qty
                Name          = v.Name |> Option.defaultValue details.Name
                ThumbnailUrl  = v.PreviewUrl |> Option.orElse v.ImageUrl |> Option.defaultValue ""
                // details.ThumbnailUrl
                Size          = v.Size |> Option.defaultValue ""
                ColorName     = v.Color |> Option.defaultValue ""
                ColorCodeOpt  = None // v.ColorCode
                UnitPrice     = v.RetailPrice |> Option.defaultValue 0m
                Currency      = v.Currency |> Option.defaultValue "USD"
            }
        )

    let tryMakeSyncProductCartItem (qty:int) (details: Shared.ShopProductViewer.ShopProductDetails) (selectedVariantId:int64) : SyncCartItem option =
        details.Variants
        |> List.iter ( fun v ->

            printfn $"SEARCH FOR VARIANT: {selectedVariantId} = {v.SyncVariantId} : {selectedVariantId = v.SyncVariantId}"
        )
        details.Variants
        |> List.tryFind (fun v -> v.SyncVariantId = selectedVariantId)
        |> Option.map (fun v ->
            {
                SyncProductId = v.SyncVariantId
                SyncVariantId = v.VariantId
                ExternalId = Some v.ExternalId
                CatalogVariantId = Some v.CatalogVariantId
                Quantity      = qty
                Name          = v.Name
                ThumbnailUrl  = v.PreviewUrl |> Option.orElse v.ImageUrl |> Option.defaultValue ""
                Size          = v.Size
                ColorName     = v.Color
                ColorCodeOpt  = None
                UnitPrice     = v.RetailPrice |> Option.defaultValue 0m
                Currency      = v.Currency
            }
        )


    let toSyncCartDU
        (qty   : int)
        (model : Product.Model)
        : CartLineItem option =
            match model.ProductDetails with
            | Deferred.Resolved details ->
                tryMakeSyncProductCartItem qty details (model.SelectedVariantId |> Option.defaultValue 0)
                |> Option.map (fun syncItem -> CartLineItem.Sync syncItem)
            | _ -> None


type ShopSectionModel =

    | ShopLanding // this is is a welcome page
    | ProductDesigner of Designer.Model // build your own, should be able to control whether or not we allow the user to upload their own images.
    | CollectionBrowser of Collection.Model
    | ProductViewer of Product.Model
    | Cart
    | Checkout
    | OrderHistory of OrderHistory.Model
    | NotFound

    // | Social
    // | Contact

type ShopLandingMsg =
    | SwitchSection of ShopSectionModel

type Tab =
    | Hero
    | Collection
    | Designer
    | Cart
    | History
    | Checkout

open Shared.Store.Cart
open Bindings.Stripe.StripePayment

type ShopMsg =

    | NavigateTo of ShopSectionModel
    // Landing / Drop Hero
    | ShopLanding of ShopLandingMsg
    // Product Collection
    | ShopCollectionMsg of Collection.Msg
    // Product Designer
    | ShopDesignerMsg of Designer.Msg
    // Product Browser
    | ShopProduct of Product.Msg
    // Order History
    | ShopOrderHistory of OrderHistory.Msg

    // Cart
    | AddCartItem      of CartLineItem
    | RemoveCartItem   of CartLineItem
    | UpdateCartQty    of CartLineItem * int

    // Checkout
    | CheckoutMsg of Checkout.Msg
    // Checkout → Checkout Preview (Step 1)
    | BeginCheckoutFromCart
    | CreatedDraftOrder  of Shared.Api.Checkout.CreateDraftOrderResponse
    | CheckoutDraftFailed  of string

    // Payment Session (Step 3)
    | LoadStripe
    | StripeLoaded of IStripe
    | StripeFailed of exn

    // | PaymentSuccess of string * string
    // | PaymentFailure of string

type StripeState = {
    Stripe: IStripe option
    Elements: IElements option
    CardElement: IStripeElement option
    PaymentClientSecret: string option
    PaymentIntentId: string option
    PaymentStatus: string
    IsLoadingPayment: bool
    PaymentError: string option
}

let initStripe clientSecret intentId =
    {
        Stripe = None
        Elements = None
        CardElement = None
        PaymentClientSecret = Some clientSecret
        PaymentIntentId = Some intentId
        PaymentStatus = "Not started"
        IsLoadingPayment = false
        PaymentError = None
    }



type Model = {
    Section: ShopSectionModel
    Cart: CartState
    // PayPalOrderReference: string option
    Checkout: Checkout.Model option
    Stripe: StripeState option
    Error: string option
}

let shopSectionToSectionModel (section: SharedViewModule.WebAppView.ShopSection) =
    match section with
    | SharedViewModule.WebAppView.ShopSection.Cart  -> ShopSectionModel.Cart
    | SharedViewModule.WebAppView.ShopSection.Checkout -> ShopSectionModel.Checkout
    | SharedViewModule.WebAppView.ShopSection.CollectionBrowser -> CollectionBrowser Collection.initModel
    | SharedViewModule.WebAppView.ShopSection.ProductDesigner -> ProductDesigner (Designer.initialModel())
    | SharedViewModule.WebAppView.ShopSection.ProductViewer id -> ShopSectionModel.ProductViewer (Product.initModel id None ReturnTo.BackToCollection)
    | SharedViewModule.WebAppView.ShopSection.OrderHistory -> OrderHistory (OrderHistory.initModel())
    | SharedViewModule.WebAppView.ShopSection.NotFound
    | SharedViewModule.WebAppView.ShopSection.ShopLanding -> ShopSectionModel.ShopLanding

let getInitialModel shopSection =
    {
        Section = shopSectionToSectionModel shopSection
        Cart = emptyCart
        Stripe = None
        Checkout = None
        Error = None
    }



let updateCheckoutModelWithCheckoutPreviewResponse (model: Checkout.Model) (draftResponse: CreateDraftOrderResponse) : Checkout.Model =
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

let update (msg: ShopMsg) (model: Model) : Model * Cmd<ShopMsg> =
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
        | None -> Checkout.Checkout.initCheckoutModel model.Cart
        | Some checkoutModel -> checkoutModel
        |> fun mdl ->
                { model with Section = ShopSectionModel.Checkout; Checkout = Some mdl }, Cmd.none

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
            SharedViewModule.Env.stripePublishableKey
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
                Cmd.ofMsg (CheckoutMsg (Checkout.SetStep Checkout.Payment))
        | None -> model, Cmd.none

    | CheckoutMsg subMsg ->
        let additionalCmd =
            match subMsg with
            | Checkout.Msg.SubmitShipping ->
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
        | None -> Checkout.Checkout.initCheckoutModel model.Cart
        | Some checkoutModel -> checkoutModel
        |> Checkout.Checkout.update subMsg
        |> fun (mdl, cmd) ->    
            { model with Section = ShopSectionModel.Checkout; Checkout = Some mdl },
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
                | Designer.BackToDropLanding ->
                    Cmd.ofMsg (NavigateTo ShopSectionModel.ShopLanding)

                | Designer.AddToCart qty ->
                    match productDesignerMdl |> PrintfulMapping.toCustomCartDU qty with
                    | Some cartItem ->  Cmd.ofMsg (ShopMsg.AddCartItem (cartItem))
                    | None -> Cmd.none
                | _ ->
                    cmd' |> Cmd.map ShopDesignerMsg

            { model with Section = ProductDesigner productDesignerMdl }, cmd
        | _ -> 
            { model with Section = ProductDesigner (Designer.initialModel()) },
            Cmd.map ShopDesignerMsg (Cmd.ofMsg Designer.LoadProducts)

    | ShopCollectionMsg subMsg ->
        match model.Section with
        | CollectionBrowser cb ->
            let updatedModel, cmd' =
                match subMsg with
                | Collection.ViewProduct sp ->
                    let seed = Shared.StoreProductViewer.ProductSeed.SeedSync {
                        ExternalId = None
                        Id = sp.SyncProductId
                        Name = sp.Name
                        ThumbnailUrl = sp.ThumbnailUrl
                        VariantCount = sp.Sizes.Length * sp.Colors.Length
                    }
                    let pvModel, _ =
                        Product.initFromSeed seed Shared.StoreProductViewer.ReturnTo.BackToCollection
                    model, Cmd.ofMsg (NavigateTo (ProductViewer pvModel))
                | _ ->
                    Collection.update subMsg cb
                    |> fun (m, cmd) -> 
                        { model with Section = CollectionBrowser m }, 
                        cmd |> Cmd.map ShopCollectionMsg
            updatedModel, cmd' 

        | ProductViewer _ -> model, Cmd.none
        | _ ->
            let landingModel, msg = init None            
            { model with Section = CollectionBrowser landingModel },
            Cmd.map ShopCollectionMsg msg

    | ShopProduct subMsg ->
        match model.Section with
        | ProductViewer pv ->
            let viewMdl, cmd' = 
                match subMsg with
                | Product.GoBack ->
                    let initCB = Collection.initModel
                    model, Cmd.ofMsg (NavigateTo (CollectionBrowser initCB))
                | Product.PrimaryAction ->
                    match pv.ProductDetails, pv.SelectedVariantId with
                    | UseDeferred.Deferred.Resolved d, Some evid ->
                        match PrintfulMapping.tryMakeSyncProductCartItem 1 d evid with
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
                Product.initModel 
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
        | CollectionBrowser cb ->
            // fresh designer state
            let model' = { model with Section = CollectionBrowser cb }
            model', Cmd.none 
        | ProductDesigner pd ->
            // fresh designer state
            let model' = { model with Section = ProductDesigner pd }
            model', Cmd.none 
        | ProductViewer pv ->
            let model' =  { model with Section = ProductViewer pv }
            model', Cmd.none 
        | _ ->
            { model with Section = section }, Cmd.none

let tabToSection tab =
    match tab with
    | Hero -> ShopSectionModel.ShopLanding
    | Collection -> ShopSectionModel.CollectionBrowser Collection.initModel
    | Designer -> ShopSectionModel.ProductDesigner (Designer.initialModel())
    | Cart  -> ShopSectionModel.Cart
    | Checkout -> ShopSectionModel.Checkout
    | History -> ShopSectionModel.OrderHistory (OrderHistory.initModel())

let sectionToTab section =
    match section with
    | ShopSectionModel.ShopLanding -> Hero
    | CollectionBrowser _ -> Collection
    | ProductDesigner _ -> Designer
    | ShopSectionModel.Cart -> Cart
    | ShopSectionModel.Checkout -> Checkout
    | ShopSectionModel.OrderHistory _ -> History
    | _ -> Hero

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
    let View model (dispatch: ShopMsg -> unit) =
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
                | ShopSectionModel.ShopLanding ->                    
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

                | ShopSectionModel.Cart ->
                    Cart.Cart.View 
                        {
                            Cart = model.Cart
                            OnRemove = fun x -> dispatch (RemoveCartItem x) 
                            OnUpdateQuantity  = fun itemAndQty -> dispatch (UpdateCartQty itemAndQty) 
                            OnContinueShopping = fun _ -> dispatch (ShopMsg.NavigateTo (tabToSection Collection))
                            OnCheckout         = fun _ -> dispatch BeginCheckoutFromCart
                        }

                | ShopSectionModel.Checkout ->
                    Checkout.View.Checkout 
                        (match model.Checkout with
                        | None -> Checkout.initCheckoutModel model.Cart
                        | Some cmdl -> cmdl)
                        (CheckoutMsg >> dispatch)

                | OrderHistory ohm -> OrderHistory.OrderLookupComponent ohm (ShopMsg.ShopOrderHistory >> dispatch)
                | NotFound -> Html.div "Not found placeholder...."
            ]
        ]

[<ReactComponent>]
let ShopView (model: Model) (dispatch: ShopMsg -> unit) =
    Shop.View model dispatch