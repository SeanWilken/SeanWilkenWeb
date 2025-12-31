module Index

open Elmish
open Shared
open PageRouter
open Client.Domain
open SharedWebAppModels
open Components.Layout.PageLayout

Fable.Core.JsInterop.importSideEffects "./index.css"

// the init has to have same signature and be called from the index html
let init ( path: SharedPage.Page option ) : SharedWebAppModels.WebAppModel * Cmd<WebAppMsg> =
    urlUpdate 
        path 
        { Theme = Cyberpunk; CurrentAreaModel = Welcome }

let update ( msg: WebAppMsg ) ( model: SharedWebAppModels.WebAppModel ): SharedWebAppModels.WebAppModel * Cmd<WebAppMsg> =
    match msg, model.CurrentAreaModel with
    
    // WELCOME PAGE
    | WelcomeMsg ( SharedWelcome.SwitchSection appSection ), Welcome ->
        model, Cmd.ofMsg ( SwitchToOtherApp appSection )

    // ABOUT PAGE
    | AboutMsg ( SharedAboutSection.SwitchSection appSection ), About m ->
        { model with CurrentAreaModel = About m }, Cmd.ofMsg (SwitchToOtherApp appSection)
    | AboutMsg msg, About ( m ) ->
        model, Cmd.none

    | ShopMsg msg, Shop shopModel ->
        match msg with
        | Store.ShopMsg.NavigateTo section -> urlUpdate ( Some (SharedPage.Shop section) ) model
        | _ -> 
            let updatedModel, com = Components.FSharp.Shop.update msg shopModel
            { model with CurrentAreaModel = Shop updatedModel }, Cmd.map ShopMsg com

    // PORTFOLIO PAGE
    | PortfolioMsg msg, Portfolio pm ->
        match msg, pm with
        | SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppLandingView, _ ->
            { model with CurrentAreaModel = Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio SharedPage.PortfolioSection.PortfolioLanding ) )
        | SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppCodeView, _ ->
           { model with CurrentAreaModel = Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio ( SharedPage.PortfolioSection.Code SharedPage.CodeSection.CodeLanding ) ) )
        | SharedPortfolioGallery.CodeGalleryMsg SharedCodeGallery.Msg.BackToPortfolio, _ -> 
            { model with CurrentAreaModel = Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio SharedPage.PortfolioSection.PortfolioLanding ) )
        | SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppDesignView, _ ->
           { model with CurrentAreaModel = Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio SharedPage.PortfolioSection.Design ) )
        | SharedPortfolioGallery.ArtGalleryMsg SharedDesignGallery.Msg.BackToPortfolio, _ -> 
            { model with CurrentAreaModel = Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio SharedPage.PortfolioSection.PortfolioLanding ) )
        | msg, m ->
            let portfolioModel, com = Components.FSharp.PortfolioLanding.update msg m
            { model with CurrentAreaModel = Portfolio portfolioModel }, Cmd.map PortfolioMsg com
    
    // Page Routing
    | SwitchToOtherApp section, _ ->
        printfn $"SWITCHING TO SECTION: {section}"
        match section with
        | SharedWebAppViewSections.AppView.AboutAppView -> model, Cmd.ofMsg ( LoadPage SharedPage.About )
        | SharedWebAppViewSections.AppView.ShopAppView -> model, Cmd.ofMsg ( LoadPage (SharedPage.Shop Store.ShopSection.ShopLanding) )
        | SharedWebAppViewSections.AppView.ContactAppView -> model, Cmd.ofMsg ( LoadPage SharedPage.Contact )
        | SharedWebAppViewSections.AppView.PortfolioAppCodeView -> model, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio ( SharedPage.Code SharedPage.CodeSection.CodeLanding ) ) )
        | SharedWebAppViewSections.AppView.PortfolioAppDesignView -> model, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio SharedPage.Design ) )
        | SharedWebAppViewSections.AppView.PortfolioAppLandingView -> model, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio SharedPage.PortfolioSection.PortfolioLanding ) )
        | SharedWebAppViewSections.AppView.ResumeAppView -> model, Cmd.ofMsg ( LoadPage SharedPage.Resume )
        | SharedWebAppViewSections.AppView.ProfessionalServicesAppView section   -> model, Cmd.ofMsg ( LoadPage (SharedPage.Services section) )
        | SharedWebAppViewSections.AppView.WelcomeAppView ->
            model, Cmd.ofMsg ( LoadPage SharedPage.Welcome )

    | LoadPage page, _ ->
        urlUpdate ( Some page ) model
    | ChangeTheme theme, _ ->
        printfn $"Selected theme: {theme.AsString}"
        Browser.Dom.document.documentElement.setAttribute("data-theme", theme.AsString)
        { model with Theme = theme }, Cmd.none

    | _ -> model, Cmd.none

open Feliz
open FSharp.Reflection

// takes union case string, returns an app section
let areaStringToAppSection string =
    printfn $"SECTION: {string}"
    match string with
    | "About" -> SharedWebAppViewSections.AppView.AboutAppView
    | "Contact" -> SharedWebAppViewSections.AppView.ContactAppView
    | "Projects" ->  SharedWebAppViewSections.AppView.PortfolioAppLandingView
    | "Resume" ->  SharedWebAppViewSections.AppView.ResumeAppView
    | "Welcome" ->  SharedWebAppViewSections.AppView.WelcomeAppView
    | "Shop" ->  SharedWebAppViewSections.AppView.ShopAppView
    | "Services" ->  SharedWebAppViewSections.AppView.ProfessionalServicesAppView SharedWebAppViewSections.ProfessionalServicesView.ServicesLanding
    | _ -> SharedWebAppViewSections.AppView.WelcomeAppView

// returns the current model as an area
let currentWebAppSection model = 
    match model with
    | SharedWebAppModels.Welcome -> SharedWebAppViewSections.AppView.WelcomeAppView
    | SharedWebAppModels.About _ -> SharedWebAppViewSections.AppView.AboutAppView
    | SharedWebAppModels.Portfolio _ -> SharedWebAppViewSections.AppView.PortfolioAppLandingView
    | SharedWebAppModels.Contact -> SharedWebAppViewSections.AppView.ContactAppView
    | SharedWebAppModels.Resume -> SharedWebAppViewSections.AppView.ResumeAppView
    | SharedWebAppModels.Services serviceModel -> SharedWebAppViewSections.AppView.ProfessionalServicesAppView serviceModel.CurrentSection
    | SharedWebAppModels.Settings // -> SharedWebAppViewSections.AppView.Help
    | SharedWebAppModels.Help // -> SharedWebAppViewSections.AppView.Help
    | SharedWebAppModels.Shop _ -> SharedWebAppViewSections.AppView.ShopAppView

// Union cases of the different web app sub modules, in order to create elements
// and reference the union type in code.
let contentAreas = FSharpType.GetUnionCases typeof<SharedWebAppModels.Model>

let heroBanner dispatch =
    Html.section [
        prop.className "relative bg-hero-gradient text-white overflow-hidden"
        prop.children [
            Html.canvas [
                prop.id "ferrofluid-canvas"
                prop.className "absolute inset-0 w-full h-full z-0 pointer-events-none"
            ]
            Html.div [
                prop.className "relative z-10 max-w-4xl mx-auto py-32 px-6 text-center"
                prop.children [
                    Html.h1 [
                        prop.className "text-5xl font-heading font-bold tracking-tight"
                        prop.text "Sean Wilken"
                    ]
                    Html.p [
                        prop.className "mt-4 text-xl font-sans text-white/90"
                        prop.text "Web Developer and ethusiast, crafting unique and problem facing solutions via technology since 2015"
                    ]
                    Html.button [
                        prop.className "mt-8 btn btn-primary btn-lg transition hover:scale-105"
                        prop.text "Explore Projects"
                        prop.onClick (fun _ -> dispatch (SwitchToOtherApp SharedWebAppViewSections.AppView.PortfolioAppLandingView))
                    ]
                ]
            ]
            // TSXHeaderCanvasWrapper.TSXHeaderCanvasComponent 
            //     {|
            //         text = "TypeScript Components"
            //         textColor = "255, 255, 255"
            //     |}
            // TSXNavBarWrapper.Gallery () 
            // PhysicsPlayground.view()
            MyApp.Components.ProgrammingExamplesPage.view()
        ]
    ]

// Web App Header Nav content using Feliz + DaisyUI classes
let headerContent (model: SharedWebAppModels.WebAppModel) dispatch =
    Html.div [
        prop.className "drawer drawer-end"
        prop.children [

            // Hidden checkbox toggles the drawer
            Html.input [
                prop.type' "checkbox"
                prop.id "mobile-nav-drawer"
                prop.className "drawer-toggle"
            ]

            // Page content
            Html.div [
                prop.className "drawer-content w-full"
                prop.children [
                    Html.header [
                        prop.className "navbar bg-base-200 px-4 py-2 shadow-md"


                        prop.children [

                            // Left: Logo
                            Html.div [
                                prop.className "navbar-start"
                                prop.children [
                                    Html.a [
                                        prop.className "text-2xl font-heading font-light text-base-content cursor-pointer"
                                        prop.onClick (fun _ -> dispatch (SwitchToOtherApp SharedWebAppViewSections.AppView.WelcomeAppView))
                                        prop.text "Sean Wilken"
                                    ]
                                ]
                            ]

                            // Center: Nav for desktop
                            Html.div [
                                prop.className "navbar-center hidden lg:flex"
                                prop.children [
                                    Html.div [
                                        prop.className "flex gap-3"
                                        prop.children [
                                            contentAreas
                                            |> Array.map (fun contentArea ->
                                                let name = contentArea.Name
                                                let section = areaStringToAppSection name
                                                let isActive = currentWebAppSection model.CurrentAreaModel = section
                                                Html.button [
                                                    prop.className (
                                                        "btn btn-ghost text-xl font-sans transition-all duration-300 group " +
                                                        (if isActive then "bg-secondary text-base-100 shadow-md scale-105" else "hover:bg-accent hover:text-base-100")
                                                    )
                                                    prop.onClick (fun _ -> dispatch (SwitchToOtherApp section))
                                                    prop.children [
                                                        name
                                                        |> Seq.map (fun c ->
                                                            Html.span [
                                                                prop.className "inline-block transition-transform duration-300 group-hover:scale-125"
                                                                prop.text (string c)
                                                            ])
                                                        |> Seq.toList
                                                        |> React.fragment
                                                    ]
                                                ]
                                            )
                                            |> Array.toList
                                            |> React.fragment
                                        ]
                                    ]
                                ]
                            ]

                            // Right: Theme selector + GitHub + Mobile menu trigger
                            Html.div [
                                prop.className "navbar-end flex items-center gap-2"
                                prop.children [
                                    Html.a [
                                        prop.href "https://github.com/seanwilken"
                                        prop.target "_blank"
                                        prop.className "btn btn-sm btn-square btn-ghost hover:text-primary"
                                        prop.children [
                                            Html.i [ prop.className "fa fa-github text-lg" ]
                                        ]
                                    ]

                                    // Drawer trigger for mobile
                                    Html.label [
                                        prop.htmlFor "mobile-nav-drawer"
                                        prop.className "btn btn-ghost btn-circle lg:hidden"
                                        prop.children [
                                            Html.i [ prop.className "fa fa-bars text-xl" ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // Drawer sidebar
            Html.div [
                prop.className "drawer-side z-50"
                prop.children [
                    Html.label [
                        prop.htmlFor "mobile-nav-drawer"
                        prop.className "drawer-overlay"
                    ]
                    Html.ul [
                        prop.className "menu p-4 w-64 min-h-full bg-base-100 text-base-content"
                        prop.children [
                            Html.li [ 
                                Html.h2 [ 
                                    prop.className "text-xl mb-4 font-heading text-base-content"
                                    prop.text "Navigate"
                                ]
                            ]
                            contentAreas
                            |> Array.map (fun contentArea ->
                                let name = contentArea.Name
                                let section = areaStringToAppSection name
                                let isActive = currentWebAppSection model.CurrentAreaModel = section
                                Html.li [
                                    Html.button [
                                        prop.className (
                                            "w-full text-left py-2 px-4 rounded transition-all duration-300 " +
                                            (if isActive then "bg-secondary text-base-100 font-semibold" else "hover:bg-base-300")
                                        )
                                        prop.onClick (fun _ -> dispatch (SwitchToOtherApp section))
                                        prop.text name
                                    ]
                                ]
                            )
                            |> Array.toList
                            |> React.fragment
                        ]
                    ]
                ]
            ]
        ]
    ]

//     // headerContent model dispatch
//     // heroBanner dispatch
    // Html.h1 [
    //     prop.className "font-heading text-3xl font-bold text-red-500"
    //     prop.text "Heading using Space grotesk"
    // ]
    // Html.p [
    //     prop.className "font-sans text-base"
    //     prop.text "Body content using DM Sans"
    // ]

[<ReactComponent>]
let View (model: SharedWebAppModels.WebAppModel) (dispatch: WebAppMsg -> unit) =
    PageLayout {|
        model = model
        dispatch = dispatch
        children =
            Html.div [
                prop.className "container mx-auto p-4"
                prop.children [
                    match model.CurrentAreaModel with
                    | Help 
                    | Settings
                    | SharedWebAppModels.Welcome -> Components.FSharp.Welcome.View (WelcomeMsg >> dispatch)
                    | SharedWebAppModels.About aboutModel -> Components.FSharp.About.View aboutModel dispatch
                    | SharedWebAppModels.Services serviceModel -> Components.FSharp.Services.Landing.View serviceModel dispatch
                    | SharedWebAppModels.Shop shopModel -> Components.FSharp.Shop.ShopView shopModel (ShopMsg >> dispatch)
                    
                    | SharedWebAppModels.Contact -> Components.FSharp.Contact.View ()

                    // Portfolio - Games & Gallery
                    | SharedWebAppModels.Portfolio SharedPortfolioGallery.PortfolioGallery ->
                        Components.FSharp.PortfolioLanding.View SharedPortfolioGallery.PortfolioGallery (PortfolioMsg >> dispatch)
                    | SharedWebAppModels.Portfolio (SharedPortfolioGallery.DesignGallery designModel) ->
                        Components.FSharp.PortfolioLanding.View (SharedPortfolioGallery.DesignGallery designModel) (PortfolioMsg >> dispatch)
                    | SharedWebAppModels.Portfolio (SharedPortfolioGallery.CodeGallery codeModel) ->
                        Components.FSharp.PortfolioLanding.View (SharedPortfolioGallery.CodeGallery codeModel) (PortfolioMsg >> dispatch)

                    // TSX Page
                    | SharedWebAppModels.Resume -> Components.FSharp.Interop.Resume.ResumePage ()
                ]
            ]
    |}
