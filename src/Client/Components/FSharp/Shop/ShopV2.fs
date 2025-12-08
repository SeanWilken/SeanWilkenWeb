module Components.FSharp.ShopV2

open System
open System
open Elmish
open Elmish.UrlParser
open Elmish.Navigation
open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Shared.SharedShopDomain
open Client.Domain.SharedShop
open Client.Domain


let sendMessage (_paypalOrderRef: string) : Cmd<ShopMsg> = Cmd.none


let init shopSection =
    Client.Domain.SharedShop.getInitialModel shopSection
    , Cmd.none // getAllProducts defaultProductsRequest


let update (msg: ShopMsg) (model: Model) : Model * Cmd<ShopMsg> =
    match msg with
    | ShopCollectionMsg subMsg ->
        match model.Section with
        | Client.Domain.SharedShopV2.CollectionBrowser cb ->
            let collectionModel, cmd' =
                Client.Components.Shop.Collection.State.update subMsg cb
                // Client.Components.Shop.Collection.State.update subMsg cb
            // Client.Components.Shop.Collection.Collection.collectionView model.Collection

            { model with
                // If you have a dedicated section for collection, use that here.
                // For now I’ll leave Section as-is so navigation is controlled by NavigateTo.
                Section = SharedShopV2.CollectionBrowser collectionModel },
            cmd' |> Cmd.map ShopCollectionMsg
        | _ ->
            printfn $"HANDLE ME: SHOP V2: UPDATE"
            let landingModel, msg = Client.Components.Shop.Collection.State.init None
            { model with Section = SharedShopV2.ShopSection.CollectionBrowser landingModel },
            Cmd.map ShopCollectionMsg msg
    | NavigateTo section ->
        // need to do url here

        { model with Section = section }, Cmd.none
        // Navigation.newUrl (toPath (Some Landing))

    | ShopMsg.ShopLanding msg ->
        match msg with
        | SharedShopV2Domain.ShopLandingMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)

    | ShopMsg.ShopTypeSelection msg ->
        match msg with
        | SharedShopV2Domain.ShopTypeSelectorMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)

    | ShopMsg.ShopBuildYourOwnWizard msg ->
        match model.Section, msg with
        | SharedShopV2.ShopSection.BuildYourOwnWizard _, SharedShopV2Domain.ShopBuildYourOwnProductWizardMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)
        | SharedShopV2.ShopSection.BuildYourOwnWizard byow, _ ->
            let newModel, childCmd = CreateYourOwnProduct.update msg byow
            { model with Section = SharedShopV2.ShopSection.BuildYourOwnWizard newModel },
            Cmd.map ShopMsg.ShopBuildYourOwnWizard childCmd

        | _ ->
            { model with Section = SharedShopV2.ShopSection.BuildYourOwnWizard (SharedShopV2.BuildYourOwnProductWizard.initialState ()) },
            Cmd.none

    | ShopMsg.ShopStoreProductTemplates msg ->

        match model.Section, msg with
        | SharedShopV2.ShopSection.ProductTemplateBrowser _, SharedShopV2Domain.ShopProductTemplatesMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)
        | SharedShopV2.ShopSection.ProductTemplateBrowser ptb, _ ->
            let newModel, childCmd = Components.FSharp.Pages.ProductTemplateBrowser.update msg ptb
            { model with Section = SharedShopV2.ShopSection.ProductTemplateBrowser newModel },
            Cmd.map ShopMsg.ShopStoreProductTemplates childCmd
        | _ ->
            { model with Section = SharedShopV2.ShopSection.ProductTemplateBrowser (SharedShopV2.ProductTemplate.ProductTemplateBrowser.initialModel ()) },
            Cmd.none

let pathToTitleString (path: string) =
    path.Replace ("/", " ")

let headerLink (linkTitle: string) =
    Html.a [
        prop.href linkTitle
        prop.className "navigationLink satorshi-font"
        prop.text (pathToTitleString linkTitle)
    ]

let spanOrControl (maybeControl: ReactElement option) : ReactElement =
    match maybeControl with
    | Some control -> control
    | None -> Html.span []

let contentHeader (sectionTitle: string) (contentNavigation: ReactElement option) : ReactElement =
    Html.div [ 
        prop.className "contentNavigation"
        prop.children [
            Html.div [ 
                prop.className "navigationControls"
                prop.children [
                    Html.div [ 
                        prop.className "headerContentTitle"
                        prop.children [
                            Html.text sectionTitle
                            spanOrControl contentNavigation
                        ]
                    ]
                ]
            ]
        ]
    ]

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
                prop.onClick (fun _ -> dispatch (NavigateTo SharedShopV2.ShopSection.ShopTypeSelector))
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
                                prop.onClick (fun _ ->
                                    dispatch (NavigateTo SharedShopV2.ShopSection.ShopTypeSelector))
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



let homeView (homeGifUrls: string list) dispatch =
    Html.div [
        prop.className "space-y-0" // hero + content stack
        prop.children [
            // hero dispatch
            shopHero dispatch
            featuredDrops homeGifUrls
        ]
    ]

// 404 Not Found page
let notFoundView =
    Html.div [
        prop.className "contentBackground"
        prop.children [
            Html.div [ prop.text "Who invited you here?! There's nothing to be seen here, so GTFO!" ]
        ]
    ]


// ----------------------------
// HELPERS
// ----------------------------

let roundOrderTotal (total: float) : float option =
    let parts = total.ToString().Split('.') |> List.ofArray
    match parts with
    | [] -> Some 0.0
    | dollars :: rest ->
        let cents =
            rest
            |> List.tryHead
            |> Option.map (fun c -> if c.Length > 2 then c.Substring(0,2) else c)
            |> Option.defaultValue "00"
        let roundedString = dollars + "." + cents
        match System.Double.TryParse roundedString with
        | true, value -> Some value
        | _ -> None

// let calculateOrderBagTotal (shoppingBag: (SyncProductVariant * int) list) : float =
//     shoppingBag
//     |> List.map (fun (variant, qty) -> variant.variantPrice * float qty)
//     |> List.sum
//     |> roundOrderTotal
//     |> Option.defaultValue 0.0

let floatToString (f: float) = f.ToString()

let roundedGrandTotal (bag: float) (tax: float) (ship: float) : string =
    let taxAmount = bag * tax
    let total = bag + taxAmount + ship
    match roundOrderTotal total with
    | Some g -> g.ToString()
    | None -> ""


// Social page
let socialView (dispatch: Client.Domain.SharedShop.ShopMsg -> unit) =
    Html.div [
        contentHeader "SOCIAL SHIT SHOW" None
        Html.div [
            prop.className "homeContent"
            prop.children [
                Html.p [ prop.text "You just HAD to look into that abyss didn't yah..." ]
                Html.p [ prop.text "Well, it's too late now. It's looking right back at you." ]
                Html.p [ prop.text "It seems to be watching..waiting..for your next move.." ]
            ]
        ]
        Html.div [
            prop.className "navigationControls"
            prop.children [
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "Check out what kind of non-sense we are getting ourselves into..." ]
                        Html.a [ prop.href "https://www.instagram.com/xeroeffort"; prop.text "Xero Effort Instagram" ]
                    ]
                ]
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "I hope you have low expectations..." ]
                        Html.a [ prop.href "https://www.instagram.com/xeroeffort"; prop.text "Xero Effort Twitter" ]
                    ]
                ]
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "Feeling a bit lost and alone? Good, so is our discord server. Join up or get left behind." ]
                        Html.a [ prop.href "https://www.instagram.com/xeroeffort"; prop.text "Xero Effort Discord" ]
                    ]
                ]
            ]
        ]
    ]

// Contact page
let contactView =
    Html.div [
        contentHeader "ALL HANDS ON DECK" None
        Html.div [
            prop.className "homeContent"
            prop.children [ Html.p [ prop.text "Dying to get something off your chest?" ] ]
        ]
        Html.div [
            prop.className "navigationControls"
            prop.children [
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "Got questions, comments or concerns?" ]
                        Html.a [ prop.href "mailto:xeroeffortclub@gmail.com"; prop.text "General Inquiries" ]
                    ]
                ]
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "What's that boy? Your order is stuck in a well?..." ]
                        Html.a [ prop.href "mailto:xeroeffortclub@gmail.com"; prop.text "Order Inquiries / Issues" ]
                    ]
                ]
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "Think you got it all figured out?" ]
                        Html.a [ prop.href "mailto:xeroeffortclub@gmail.com"; prop.text "Business Inquiries" ]
                    ]
                ]
            ]
        ]
    ]



// open Feliz
open SharedShopV2
open SharedShopV2Domain
open Client.Domain.SharedShopV2Domain
open Client.Domain.SharedShopV2

let shopTypeSelectorView (dispatch: ShopTypeSelectorMsg -> unit) =
    Html.div [
        prop.className "flex flex-col items-center justify-center gap-8 p-8"
        prop.children [
            Html.h2 [
                prop.className "text-2xl font-bold"
                prop.text "Choose how you want to shop"
            ]

            Html.div [
                prop.className "grid grid-cols-1 gap-6 w-full max-w-3xl"
                prop.children [

                    Html.div [
                        prop.className "p-6 border rounded-2xl shadow hover:shadow-lg cursor-pointer transition"
                        prop.onClick (fun _ -> dispatch (ShopTypeSelectorMsg.SwitchSection (ShopSection.BuildYourOwnWizard (BuildYourOwnProductWizard.initialState ()))))
                        prop.children [
                            Html.h3 [ prop.className "text-xl font-semibold"; prop.text "📅 We'll be right back!" ]
                            // Html.hr [ prop.className "my-4" ]
                            Html.p [ prop.className "text-md opacity-70 text-center p-2"; prop.text "Currently disabled, while we upgrade to API v2!" ]
                        ]
                    ]

                    // Html.div [
                    //     prop.className "p-6 border rounded-2xl shadow hover:shadow-lg cursor-pointer transition"
                    //     // prop.onClick (fun _ -> dispatch (ShopTypeSelectorMsg.SwitchSection (ShopSection.BuildYourOwnWizard (BuildYourOwnProductWizard.initialState ()))))
                    //     prop.children [
                    //         Html.h3 [ prop.className "text-xl font-semibold text-left"; prop.text "🛠️ Build Your Own" ]
                    //         Html.p [ prop.className "text-md opacity-70 text-center p-2"; prop.text "Customize products step by step with your own designs." ]
                    //     ]
                    // ]

                    // Html.div [
                    //     prop.className "p-6 border rounded-2xl shadow hover:shadow-lg cursor-pointer transition"
                    //     // prop.onClick (fun _ -> dispatch (ShopTypeSelectorMsg.SwitchSection (ShopSection.ProductTemplateBrowser (ProductTemplate.ProductTemplateBrowser.initialModel()))))
                    //     prop.children [
                    //         Html.h3 [ prop.className "text-xl font-semibold text-left"; prop.text "🛍️ Browse Store Templates" ]
                    //         Html.p [ prop.className "text-md opacity-70 text-center p-2"; prop.text "Pick from pre-made product templates and order quickly." ]
                    //     ]
                    // ]
                ]
            ]
        ]
    ]

open Feliz
open Client.Components.Shop
open Client.Components.Shop.Common
open Client.Components.Shop.Common.Types
open SharedShopV2
open Client.Components.Shop.Collection
open Client.Components.Shop.ShopHero

module LuxuryMockup =
    open Client.Domain.SharedShopV2

    type Tab =
        | Hero
        | Collection
        | Designer
        | Product
        | Cart
        | Checkout

    [<ReactComponent>]
    let view model (dispatch: Client.Domain.SharedShop.ShopMsg -> unit) =
        let (tab, setTab) = React.useState Tab.Hero

        let productDetails : Product.ProductDetails =
            {
                Name        = "Essential Crew Tee"
                Price       = 45m
                Description = "Premium cotton construction with a modern fit. Designed for everyday wear with exceptional comfort and durability. Sustainably produced."
                ReviewCount = 128
                Sizes       = [ "XS"; "S"; "M"; "L"; "XL"; "XXL" ]
                Colors      = [ "bg-neutral"; "bg-base-100 border"; "bg-base-300"; "bg-primary" ]
            }

        // Minimal designer model for now
        let wizardModel =
            BuildYourOwnProductWizard.initialState ()

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
                                            prop.onClick (fun _ -> setTab t)
                                        ]

                                    tabBtn Tab.Hero       "hero"
                                    tabBtn Tab.Collection "collection"
                                    tabBtn Tab.Designer   "designer"
                                    tabBtn Tab.Product    "product"
                                    tabBtn Tab.Cart       "cart"
                                    tabBtn Tab.Checkout   "checkout"
                                ]
                            ]
                        ]
                    ]
                ]

                // Active body
                match tab with
                | Tab.Hero ->
                    Hero.view {
                        OnShopCollection = (fun () -> setTab Tab.Collection)
                        OnExploreMore    = (fun () -> setTab Tab.Collection)
                    }

                | Tab.Collection ->
                    match model.Section with
                    | CollectionBrowser cmodel  ->
                        Collection.collectionView
                            cmodel
                            (fun msg -> msg |> dispatch )
                    | _ ->

                        Collection.collectionView
                            State.initModel
                            (fun msg -> msg |> dispatch )


                | Tab.Product ->
                    Product.view {
                        Product     = productDetails
                        OnAddToCart = ignore
                        OnAddToWish = ignore
                    }

                | Tab.Designer ->
                    Designer.view {
                        Model    = wizardModel
                        Dispatch = ignore
                    }
                | Tab.Cart ->
                    Cart.Cart.view
                        { Items   = []; }
                        (fun _ -> ())

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

// View dispatcher: select page content based on Model.CurrentPage
let view (model: Client.Domain.SharedShop.Model) (dispatch: Client.Domain.SharedShop.ShopMsg -> unit) =
    LuxuryMockup.view model (dispatch)
       