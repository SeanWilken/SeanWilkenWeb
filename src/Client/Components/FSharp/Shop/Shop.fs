module Components.FSharp.ShopV2

open Elmish
open Feliz
open Client.Domain.Store
open Client.Domain
open Client.Components.Shop
open Client.Components.Shop.Common
open Client.Components.Shop.Collection
open Client.Components.Shop.ShopHero
open Client.Domain.Store.PrintfulMapping
open Shared.Store.Cart



let update (msg: ShopMsg) (model: Model) : Model * Cmd<ShopMsg> =
    match msg with
    | RemoveCartItem cartItem ->
        // persist to local storage for recently removed?
        let updatedCartState = withRemovedItem cartItem model.Cart
        printfn $"REMOVING!!!"
        { model with Cart = updatedCartState }, Cmd.none
    | AddCartItem cartItem ->
        let updatedCartState = model.Cart.Items @ [ cartItem ] |> withItems
        printfn $"ADDING!!!"
        { model with Cart = updatedCartState }, Cmd.none
    | UpdateCartQty (cartItem, qty) ->
        let updatedCartState = withUpdatedItemQuantity cartItem qty model.Cart
        printfn $"UPDATING!!!"
        { model with Cart = updatedCartState }, Cmd.none
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
                | Collection.ViewProduct pt ->
                    // navigate to product viewer
                    let seed =  Shared.StoreProductViewer.ProductSeed.SeedTemplate pt
                    // let pvModel, pvCmd = 
                    Client.Components.Shop.Product.initFromSeed seed Shared.StoreProductViewer.ReturnTo.BackToCollection
                    |> fun (m, cmd) -> 
                        { model with Section = ProductViewer m }, 
                        Cmd.batch [
                            // cmd |> Cmd.map (fun m -> ViewProduct (Shared.StoreProductViewer.ProductKey.Template pt.product_id, Some seed, Shared.StoreProductViewer.ReturnTo.BackToCollection) )
                            // Cmd.ofMsg (NavigateTo (ShopSection.ProductViewer m))
                            // Navigation.newUrl $"/shop/product/template/{pt.product_id}"   
                        ]
                | _ -> 
                    State.update subMsg cb
                    |> fun (m, cmd) -> 
                        { model with Section = CollectionBrowser m }, 
                        cmd |> Cmd.map ShopCollectionMsg
            updatedModel,
            cmd' 

        | _ ->
            let landingModel, msg = State.init None            
            { model with Section = CollectionBrowser landingModel },
            Cmd.map ShopCollectionMsg msg
    
    | ViewProduct (productKey, productSeedOpt, returnTo) ->
            // let collectionModel, cmd' = Produc.update subMsg cb
            let productViewModel =
                ProductViewer (
                    ProductViewer.initModel
                        productKey
                        productSeedOpt
                        returnTo
                ) 
            { model with Section = productViewModel },
            // Cmd.none |> Cmd.map ShopCollectionMsg
            // Cmd.batch [
            //     // Cmd.ofMsg  (ViewProduct (productKey, productSeedOpt, returnTo) )
            // ]
            Cmd.ofMsg (ShopProduct ProductViewer.Msg.LoadDetails)
            // Cmd.ofMsg ( productViewModel)
    
    | ShopProduct subMsg ->
        match model.Section with
        | ProductViewer pv ->
            let productViewMdl, cmd' = Client.Components.Shop.Product.update subMsg pv
            { model with Section = ProductViewer productViewMdl },
            cmd' |> Cmd.map ShopProduct
        | _ ->
            let landingModel = 
                ProductViewer.initModel 
                    (Shared.StoreProductViewer.ProductKey.Template 0)
                    None
                    Shared.StoreProductViewer.ReturnTo.BackToCollection    
            { model with Section = ProductViewer landingModel },
            // Cmd.map ShopProduct
            Cmd.ofMsg (NavigateTo (ProductViewer landingModel))
            // Cmd.none

    | NavigateTo section ->
        // need to do url here?
        match section with
        | ShopSection.ProductDesigner pd ->
            // fresh designer state

            let model' = { model with Section = ShopSection.ProductDesigner pd }

            // immediately ask the designer to load products
            let cmd =
                ProductDesigner.Msg.LoadProducts
                |> ShopDesignerMsg
                |> Cmd.ofMsg

            model', cmd
        | ShopSection.ProductViewer pv ->
            let model' = { model with Section = ShopSection.ProductViewer pv }

            // immediately ask the designer to load products
            let cmd =
                ProductViewer.Msg.LoadDetails |> ShopProduct |> Cmd.ofMsg

            model', cmd

        | _ ->
            { model with Section = section }, Cmd.none

    | ShopLanding msg ->
        match msg with
        | ShopLandingMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)


open Feliz
open Shared
open Bindings.LucideIcon


let tabToSection tab =
    match tab with
    | Hero -> ShopSection.ShopLanding
    | Collection -> ShopSection.CollectionBrowser (Collection.initModel())
    | Designer -> ShopSection.ProductDesigner (ProductDesigner.initialModel())
    | Cart  -> ShopSection.ShoppingBag
    | Checkout -> ShopSection.Payment
    | Product -> ShopSection.ProductViewer (ProductViewer.initModel (StoreProductViewer.ProductKey.Template 0) None StoreProductViewer.ReturnTo.BackToCollection )

let sectionToTab section =
    match section with
    | ShopSection.ShopLanding -> Hero
    | ShopSection.CollectionBrowser _ -> Collection
    | ShopSection.ProductDesigner _ -> Designer
    | ShopSection.ShoppingBag -> Cart
    | ShopSection.ProductViewer _ -> Product
    | ShopSection.Payment
    | ShopSection.Checkout -> Checkout
    | ShopSection.NotFound -> Hero

module Shop =
    open Client.Domain.Store.ProductViewer

    [<ReactComponent>]
    let view model (dispatch: Store.ShopMsg -> unit) =
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
                                    tabBtn Designer   "designer"
                                    tabBtn Product    "product"
                                    tabBtn Cart       "cart"
                                    tabBtn Checkout   "checkout"
                                ]
                            ]
                        ]
                    ]
                ]

                // Active body
                match tab with
                | Tab.Hero ->
                    Hero.view {
                        OnShopCollection = fun () -> dispatch (ShopMsg.NavigateTo (tabToSection Collection))
                        OnExploreMore    = fun () -> dispatch (ShopMsg.NavigateTo (tabToSection Designer))
                    }

                | Tab.Collection ->
                    Collection.collectionView
                        (
                            match model.Section with
                            | CollectionBrowser cmodel -> cmodel
                            | _ -> State.initModel
                        )
                        (ShopCollectionMsg >> dispatch)

                | Tab.Designer ->
                    Designer.view
                        (
                            match model.Section with
                            | ProductDesigner dmodel -> dmodel
                            | _ -> ProductDesigner.initialModel ()
                        )
                        (ShopDesignerMsg >> dispatch)
                
                | Tab.Product ->
                    printfn $"WE SHOULD HAVE LOADED PRODUCTS"
                    Product.view 
                        (
                            match model.Section with
                            | ProductViewer pmodel -> pmodel
                            | _ -> initModel (StoreProductViewer.ProductKey.Template 0) None StoreProductViewer.ReturnTo.BackToCollection
                        )
                        (ShopProduct >> dispatch)

                | Tab.Cart ->
                    Cart.Cart.view 
                        {
                            Cart = model.Cart
                            OnRemove = fun x -> dispatch (RemoveCartItem x) 
                            OnUpdateQuantity  = fun itemAndQty -> dispatch (UpdateCartQty itemAndQty) 
                            OnContinueShopping = fun _ -> dispatch (ShopMsg.NavigateTo (tabToSection Hero))
                            OnCheckout         = fun _ -> dispatch (ShopMsg.NavigateTo (tabToSection Checkout))
                        }

                | Tab.Checkout ->
                    Checkout.Checkout.view 
                        {
                            Step           = Checkout.Checkout.CheckoutStep.Shipping
                            ShippingInfo   =
                                {
                                    Email     = ""
                                    FirstName = ""
                                    LastName  = ""
                                    Address   = ""
                                    Apartment = ""
                                    City      = ""
                                    State     = ""
                                    ZipCode   = ""
                                    Country   = ""
                                    Phone     = ""
                                }
                            ShippingMethod  = Checkout.Checkout.ShippingMethod.Standard
                            PaymentMethod  = Checkout.Checkout.PaymentMethod.Card
                            Items          = []
                        } 
                        (fun _ -> ())
            ]
        ]

[<ReactComponent>]
let view (model: Store.Model) (dispatch: Store.ShopMsg -> unit) =
    Shop.view model dispatch
       



       
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
