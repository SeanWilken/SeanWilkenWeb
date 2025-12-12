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
open Shared.SharedShopV2.Cart

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
            let collectionModel, cmd' = State.update subMsg cb
            { model with Section = CollectionBrowser collectionModel },
            cmd' |> Cmd.map ShopCollectionMsg
        | _ ->
            let landingModel, msg = State.init None            
            { model with Section = CollectionBrowser landingModel },
            Cmd.map ShopCollectionMsg msg

    | NavigateTo section ->
        // need to do url here?
        match section with
        | ShopSection.ProductDesigner _ ->
            // fresh designer state
            let pdModel = ProductDesigner.initialModel ()

            let model' =
                { model with Section = ShopSection.ProductDesigner pdModel }

            // immediately ask the designer to load products
            let cmd =
                ProductDesigner.Msg.LoadProducts
                |> ShopDesignerMsg
                |> Cmd.ofMsg

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

// For now, just hard-code your hero video path.
// Later we can make this configurable or pull from CMS.
let heroVideoUrl = "/media/xero-effort-hero.mp4"

// Optional: fallback image (poster) if video fails or while loading
let heroPosterUrl = "/img/xero-effort/hero-poster.jpg"

let private featuredDrops (homeGifUrls: string list) =
    let cards =
        homeGifUrls
        |> List.mapi (fun idx url ->
            Html.div [
                prop.key (string idx)
                prop.className
                    "rounded-3xl overflow-hidden shadow-lg border border-base-300/60 \
                     bg-base-100/90 hover:-translate-y-[3px] hover:shadow-xl transition-transform duration-200"
                prop.children [
                    Html.img [
                        prop.src url
                        prop.alt (sprintf "Featured drop %d" (idx + 1))
                        prop.className "w-full h-full object-cover"
                    ]
                ]
            ])

    Html.section [
        prop.className "max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 pt-10 pb-16 space-y-4"
        prop.children [
            Html.div [
                prop.className "flex items-center justify-between gap-3"
                prop.children [
                    Html.div [
                        Html.h2 [
                            prop.className "text-sm font-semibold tracking-wide uppercase"
                            prop.text "Featured drops"
                        ]
                        Html.p [
                            prop.className "text-[11px] text-base-content/60"
                            prop.text "Just a taste. Collections go deeper."
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.className "grid gap-6 md:grid-cols-2"
                prop.children cards
            ]
        ]
    ]

let hero dispatch =
    Html.section [
        prop.className
            "relative w-full min-h-[75vh] flex flex-col justify-center items-center \
             bg-black text-white overflow-hidden"

        prop.children [

            // fog, no PNG
            Html.div [ prop.className "fog-layer" ]

            Html.h1 [
                prop.className "text-6xl font-black tracking-tight text-center"
                prop.text "Xero Effort"
            ]
            Html.h5 [
                prop.className "mt-4 text-sm text-white/60 uppercase  text-center"
                prop.text  "Streetwear, prints, and digital mischief for people who took one step past the EXIT sign on purpose. Out with the old, in with the new—never to be heard from again."
            ]

            Html.p [
                prop.className "mt-4 text-sm text-white/60 uppercase tracking-[0.15em]"
                prop.children [
                    Html.strong [ prop.text "Limited-time drops" ]
                    Html.text " • "
                    Html.strong [ prop.text "No restocks" ]
                    Html.text " • "
                    Html.strong [ prop.text "Get it before it's gone" ]
                ]
                // prop.text "Limited-time drops • No restocks • Digital relics"
            ]

            Html.button [
                prop.className "mt-10 btn btn-sm px-6 bg-white text-black hover:bg-white/80"
                prop.onClick (fun _ -> dispatch (NavigateTo (CollectionBrowser (Collection.initModel()))))
                prop.text "Enter collections →"
            ]
        ]
    ]

let shopHero dispatch =
    Html.section [
        prop.className
            // "relative w-full min-h-[72vh] sm:min-h-[80vh] overflow-hidden flex items-center justify-center"
            "relative w-full min-h-[75vh] flex flex-col justify-center items-center \
             bg-black text-white overflow-hidden"
        prop.children [

            // 🎥 Background video
            Html.video [
                prop.className "absolute inset-0 w-full h-full object-cover hero-video"
                prop.src "/videos/xero-hero.mp4"      // ⬅️ put your mp4 here
                prop.autoPlay true
                prop.loop true
                prop.muted true
                prop.playsInline true
            ]

            // 🌫 theme-tinted scrim so text is readable in all themes
            Html.div [ prop.className "hero-scrim" ]

            // 🌫 fog + 👁‍🗨 film grain (both theme-tinted via CSS vars)
            Html.div [ prop.className "fog-layer" ]
            Html.div [ prop.className "hero-noise" ]

            // 🧊 foreground content (all using DaisyUI utility colors)
            Html.div [
                prop.className
                    "relative z-10 max-w-3xl px-6 py-10 text-center space-y-4 text-base-content"
                prop.children [

                    // main title
                    Html.h1 [
                        prop.className
                            "text-4xl sm:text-6xl md:text-7xl font-black leading-tight text-primary-content drop-shadow-lg"
                        prop.text "Xero Effort"
                    ]

                    Html.h5 [
                        prop.className "mt-4 text-sm text-white/60 uppercase  text-center"
                        prop.text  "Streetwear, prints, and digital mischief for people who took one step past the EXIT sign on purpose. Out with the old, in with the new—never to be heard from again."
                    ]
                    
                    Html.div [
                        prop.className "flex flex-wrap items-center justify-center gap-3 text-[11px]"
                        prop.children [
                            Html.div [
                                prop.className "flex flex-wrap gap-2"
                                prop.children [
                                    Html.span [
                                        prop.className
                                            "badge badge-sm badge-outline border-white/40 text-white/60"
                                        prop.text "Limited-run drops"
                                    ]
                                    Html.span [
                                        prop.className
                                            "badge badge-sm badge-outline border-white/40 text-white/60"
                                        prop.text "Get it before it's gone"
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // CTA
                    Html.div [
                        prop.className "pt-4"
                        prop.children [
                            Html.button [
                                prop.className
                                    "btn btn-primary rounded-full px-8 gap-2 shadow-lg shadow-primary/40"
                                prop.text "Enter collections"
                                prop.onClick (fun _ -> dispatch (NavigateTo (CollectionBrowser (Collection.initModel()))))
                            ]
                        ]
                    ]

                    // tiny disclaimer line
                    Html.p [
                        prop.className "text-[11px]  text-white/60 uppercase pt-2"
                        prop.text "Limited runs. Once it's gone, it only lives in screenshots and regrets."
                    ]
                ]
            ]
        ]
    ]

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


let tabToSection tab =
    match tab with
    | Hero -> ShopSection.ShopLanding
    | Collection -> ShopSection.CollectionBrowser (Collection.initModel())
    | Designer -> ShopSection.ProductDesigner (ProductDesigner.initialModel())
    | Cart  -> ShopSection.ShoppingBag
    | Checkout -> ShopSection.Payment
    | Product -> ShopSection.NotFound

let sectionToTab section =
    match section with
    | ShopSection.ShopLanding -> Hero
    | ShopSection.CollectionBrowser _ -> Collection
    | ShopSection.ProductDesigner _ -> Designer
    | ShopSection.ShoppingBag -> Cart
    | ShopSection.Payment
    | ShopSection.Checkout -> Checkout
    | ShopSection.NotFound -> Hero

module Shop =

    [<ReactComponent>]
    let view model (dispatch: Store.ShopMsg -> unit) =
        let tab = sectionToTab model.Section

        let productDetails : Product.ProductDetails =
            {
                Name        = "Essential Crew Tee"
                Price       = 45m
                Description = "Premium cotton construction with a modern fit. Designed for everyday wear with exceptional comfort and durability. Sustainably produced."
                ReviewCount = 128
                Sizes       = [ "XS"; "S"; "M"; "L"; "XL"; "XXL" ]
                Colors      = [ "bg-neutral"; "bg-base-100 border"; "bg-base-300"; "bg-primary" ]
            }

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
                    Product.view {
                        Product     = productDetails
                        OnAddToCart = ignore
                        OnAddToWish = ignore
                    }

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

let view (model: Store.Model) (dispatch: Store.ShopMsg -> unit) =
    Shop.view model dispatch
       