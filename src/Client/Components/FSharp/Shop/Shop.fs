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
            IsBusy         = false
            Error          = None
        }

let update (msg: ShopMsg) (model: Store.Model) : Store.Model * Cmd<ShopMsg> =
    printfn $"SHOP UPDATE MSG: {msg}"
    match msg with

    | RemoveCartItem cartItem ->
        // persist to local storage for recently removed?
        let updatedCartState = withRemovedItem cartItem model.Cart
        { model with Cart = updatedCartState }, Cmd.none
    
    | AddCartItem cartItem ->
        let updatedCartState = model.Cart.Items @ [ cartItem ] |> withItems
        { model with Cart = updatedCartState }, Cmd.none

    | UpdateCartQty (cartItem, qty) ->
        let updatedCartState = withUpdatedItemQuantity cartItem qty model.Cart
        { model with Cart = updatedCartState }, Cmd.none

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

let sectionToTab section =
    match section with
    | ShopSection.ShopLanding -> Hero
    | ShopSection.CollectionBrowser _ -> Collection
    | ShopSection.ProductViewer _ -> Product
    | ShopSection.ProductDesigner _ -> Designer
    | ShopSection.Cart -> Cart
    | ShopSection.Checkout -> Checkout
    | ShopSection.NotFound -> Hero

module Shop =
    open Client.Domain.Store.ProductViewer

    [<ReactComponent>]
    let View model (dispatch: Store.ShopMsg -> unit) =
        let tab = sectionToTab model.Section

        Html.div [
            prop.className "min-h-screen bg-base-100 text-base-content"
            prop.children [

                // Top tab bar
                Html.div [
                    prop.className "sticky top-0 z-40 bg-base-100/90 backdrop-blur border-b border-base-300"
                    prop.children [
                        Ui.Section.container [
                            Html.div [
                                prop.className "flex gap-6 sm:gap-8 py-3"
                                prop.children [
                                    let tabBtn t (label: string) =
                                        Html.button [
                                            prop.key label
                                            prop.className (
                                                Ui.tw [
                                                    "text-xs sm:text-sm font-medium uppercase tracking-[0.2em] pb-1 transition-all border-b-2 border-transparent"
                                                    if tab = t then "text-base-content border-base-content"
                                                    else "text-base-content/50 hover:text-base-content"
                                                ]
                                            )
                                            prop.text label
                                            prop.onClick (fun _ -> dispatch (ShopMsg.NavigateTo (tabToSection t)) )
                                        ]

                                    tabBtn Hero       "hero"
                                    tabBtn Collection "collection"
                                    // tabBtn Designer   "designer"
                                    match model.Section with
                                    | ProductViewer _ -> tabBtn Product    "product"
                                    | _ -> ()
                                    tabBtn Cart       "cart"
                                    tabBtn Checkout   "checkout"
                                ]
                            ]
                        ]
                    ]
                ]

                // Active body
                match model.Section with
                | ShopSection.ShopLanding ->
                    Hero.View {
                        OnShopCollection = fun () -> dispatch (ShopMsg.NavigateTo (tabToSection Collection))
                        OnExploreMore    = fun () -> dispatch (ShopMsg.NavigateTo (tabToSection Designer))
                    }

                | ShopSection.CollectionBrowser cb ->
                    Collection.View cb (ShopCollectionMsg >> dispatch)

                | ShopSection.ProductDesigner pd ->
                    Designer.View pd (ShopDesignerMsg >> dispatch)
                
                | ShopSection.ProductViewer pv ->
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
                | ShopSection.NotFound -> Html.div "Not found placeholder...."
            ]
        ]

[<ReactComponent>]
let ShopView (model: Store.Model) (dispatch: Store.ShopMsg -> unit) =
    Shop.View model dispatch
    



       
// For now, just hard-code your hero video path.
// Later we can make this configurable or pull from CMS.
// let heroVideoUrl = "/media/xero-effort-hero.mp4"

// Optional: fallback image (poster) if video fails or while loading
// let heroPosterUrl = "/img/xero-effort/hero-poster.jpg"

// let private featuredDrops (homeGifUrls: string list) =
//     let cards =
//         homeGifUrls
//         |> List.mapi (fun idx url ->
//             Html.div [
//                 prop.key (string idx)
//                 prop.className
//                     "rounded-3xl overflow-hidden shadow-lg border border-base-300/60 \
//                      bg-base-100/90 hover:-translate-y-[3px] hover:shadow-xl transition-transform duration-200"
//                 prop.children [
//                     Html.img [
//                         prop.src url
//                         prop.alt (sprintf "Featured drop %d" (idx + 1))
//                         prop.className "w-full h-full object-cover"
//                     ]
//                 ]
//             ])

//     Html.section [
//         prop.className "max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 pt-10 pb-16 space-y-4"
//         prop.children [
//             Html.div [
//                 prop.className "flex items-center justify-between gap-3"
//                 prop.children [
//                     Html.div [
//                         Html.h2 [
//                             prop.className "text-sm font-semibold tracking-wide uppercase"
//                             prop.text "Featured drops"
//                         ]
//                         Html.p [
//                             prop.className "text-[11px] text-base-content/60"
//                             prop.text "Just a taste. Collections go deeper."
//                         ]
//                     ]
//                 ]
//             ]
//             Html.div [
//                 prop.className "grid gap-6 md:grid-cols-2"
//                 prop.children cards
//             ]
//         ]
//     ]

// ----------------------------
// HELPERS
// ----------------------------

// Social page
// let socialView (dispatch: Store.ShopMsg -> unit) =
//     Html.div [
//         contentHeader "SOCIAL SHIT SHOW" None
//         Html.div [
//             prop.className "homeContent"
//             prop.children [
//                 Html.p [ prop.text "You just HAD to look into that abyss didn't yah..." ]
//                 Html.p [ prop.text "Well, it's too late now. It's looking right back at you." ]
//                 Html.p [ prop.text "It seems to be watching..waiting..for your next move.." ]
//             ]
//         ]
//         Html.div [
//             prop.className "navigationControls"
//             prop.children [
//                 Html.div [
//                     prop.className "homeContent"
//                     prop.children [
//                         Html.p [ prop.text "Check out what kind of non-sense we are getting ourselves into..." ]
//                         Html.a [ prop.href "https://www.instagram.com/xeroeffort"; prop.text "Xero Effort Instagram" ]
//                     ]
//                 ]
//                 Html.div [
//                     prop.className "homeContent"
//                     prop.children [
//                         Html.p [ prop.text "I hope you have low expectations..." ]
//                         Html.a [ prop.href "https://www.instagram.com/xeroeffort"; prop.text "Xero Effort Twitter" ]
//                     ]
//                 ]
//                 Html.div [
//                     prop.className "homeContent"
//                     prop.children [
//                         Html.p [ prop.text "Feeling a bit lost and alone? Good, so is our discord server. Join up or get left behind." ]
//                         Html.a [ prop.href "https://www.instagram.com/xeroeffort"; prop.text "Xero Effort Discord" ]
//                     ]
//                 ]
//             ]
//         ]
//     ]

// // Contact page
// let contactView =
//     Html.div [
//         contentHeader "ALL HANDS ON DECK" None
//         Html.div [
//             prop.className "homeContent"
//             prop.children [ Html.p [ prop.text "Dying to get something off your chest?" ] ]
//         ]
//         Html.div [
//             prop.className "navigationControls"
//             prop.children [
//                 Html.div [
//                     prop.className "homeContent"
//                     prop.children [
//                         Html.p [ prop.text "Got questions, comments or concerns?" ]
//                         Html.a [ prop.href "mailto:xeroeffortclub@gmail.com"; prop.text "General Inquiries" ]
//                     ]
//                 ]
//                 Html.div [
//                     prop.className "homeContent"
//                     prop.children [
//                         Html.p [ prop.text "What's that boy? Your order is stuck in a well?..." ]
//                         Html.a [ prop.href "mailto:xeroeffortclub@gmail.com"; prop.text "Order Inquiries / Issues" ]
//                     ]
//                 ]
//                 Html.div [
//                     prop.className "homeContent"
//                     prop.children [
//                         Html.p [ prop.text "Think you got it all figured out?" ]
//                         Html.a [ prop.href "mailto:xeroeffortclub@gmail.com"; prop.text "Business Inquiries" ]
//                     ]
//                 ]
//             ]
//         ]
//     ]
