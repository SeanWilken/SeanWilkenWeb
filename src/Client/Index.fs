// ClientApp
module Index

open Elmish
open Shared
open PageRouter
open SharedWebAppModels

// Section Items
open Welcome
// open AboutSection
// open Portfolio
// open Contact

Fable.Core.JsInterop.importSideEffects "./index.css"

// the init has to have same signature and be called from the index html
let init ( path: SharedPage.Page option ) : SharedWebAppModels.WebAppModel * Cmd<WebAppMsg> =
    PageRouter.urlUpdate 
        path 
        { Theme = Theme.Cyberpunk; CurrentAreaModel = SharedWebAppModels.Model.Welcome }

let update ( msg: WebAppMsg ) ( model: SharedWebAppModels.WebAppModel ): SharedWebAppModels.WebAppModel * Cmd<WebAppMsg> =
    match msg, model.CurrentAreaModel with
    
    // WELCOME PAGE
    | WelcomeMsg ( SharedWelcome.SwitchSection appSection ), SharedWebAppModels.Welcome ->
        model, Cmd.ofMsg ( SwitchToOtherApp appSection )

    // ABOUT PAGE
    | AboutMsg ( SharedAboutSection.SwitchSection appSection ), SharedWebAppModels.About m ->
        { model with CurrentAreaModel = SharedWebAppModels.About m }, Cmd.ofMsg (SwitchToOtherApp appSection)
    | AboutMsg msg, SharedWebAppModels.Model.About ( m ) ->
        let updateModel, com = AboutSection.update msg m
        { model with CurrentAreaModel = SharedWebAppModels.About updateModel }, Cmd.none

    // PORTFOLIO PAGE
    | PortfolioMsg msg, SharedWebAppModels.Portfolio pm ->
        match msg, pm with
        | SharedPortfolioGallery.LoadSection ( SharedWebAppViewSections.AppSection.PortfolioAppLandingView ), _ ->
            { model with CurrentAreaModel = SharedWebAppModels.Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio SharedPage.PortfolioSection.Landing ) )
        | SharedPortfolioGallery.LoadSection ( SharedWebAppViewSections.AppSection.PortfolioAppCodeView ), _ ->
           { model with CurrentAreaModel = SharedWebAppModels.Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio ( SharedPage.PortfolioSection.Code ( SharedPage.CodeSection.Landing ) ) ) )
        | SharedPortfolioGallery.CodeGalleryMsg SharedCodeGallery.Msg.BackToPortfolio, _ -> 
            { model with CurrentAreaModel = SharedWebAppModels.Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio SharedPage.PortfolioSection.Landing ) )
        | SharedPortfolioGallery.LoadSection ( SharedWebAppViewSections.AppSection.PortfolioAppDesignView ), _ ->
           { model with CurrentAreaModel = SharedWebAppModels.Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio ( SharedPage.PortfolioSection.Design ) ) )
        | SharedPortfolioGallery.ArtGalleryMsg SharedDesignGallery.Msg.BackToPortfolio, _ -> 
            { model with CurrentAreaModel = SharedWebAppModels.Portfolio pm }, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio SharedPage.PortfolioSection.Landing ) )
        | msg, m ->
            let portfolioModel, com = Portfolio.update msg m
            { model with CurrentAreaModel = SharedWebAppModels.Portfolio portfolioModel }, Cmd.map PortfolioMsg com
    
    // Page Routing
    | SwitchToOtherApp section, _ ->
        match section with
        | SharedWebAppViewSections.AppSection.AboutAppView -> model, Cmd.ofMsg ( LoadPage SharedPage.About )
        | SharedWebAppViewSections.AppSection.PortfolioAppLandingView -> model, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio ( SharedPage.PortfolioSection.Landing ) ) )
        | SharedWebAppViewSections.AppSection.PortfolioAppCodeView -> model, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio ( SharedPage.Code ( SharedPage.CodeSection.Landing ) ) ) )
        | SharedWebAppViewSections.AppSection.PortfolioAppDesignView -> model, Cmd.ofMsg ( LoadPage ( SharedPage.Portfolio ( SharedPage.Design ) ) )
        | SharedWebAppViewSections.AppSection.ContactAppView -> model, Cmd.ofMsg ( LoadPage SharedPage.Contact )
        | SharedWebAppViewSections.AppSection.WelcomeAppView
        | _ ->
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
    match string with
    | "About" -> SharedWebAppViewSections.AppSection.AboutAppView
    | "Portfolio" ->  SharedWebAppViewSections.AppSection.PortfolioAppLandingView
    | "Contact" -> SharedWebAppViewSections.AppSection.ContactAppView
    | "Welcome"
    | _ -> SharedWebAppViewSections.AppSection.WelcomeAppView

// returns the current model as an area
let currentWebAppSection model = 
    match model with
    | SharedWebAppModels.Welcome -> SharedWebAppViewSections.AppSection.WelcomeAppView
    | SharedWebAppModels.About _ -> SharedWebAppViewSections.AppSection.AboutAppView
    | SharedWebAppModels.Portfolio _ -> SharedWebAppViewSections.AppSection.PortfolioAppLandingView
    | SharedWebAppModels.Contact -> SharedWebAppViewSections.AppSection.ContactAppView

// Union cases of the different web app sub modules, in order to create elements
// and reference the union type in code.
let contentAreas = FSharpType.GetUnionCases typeof<SharedWebAppModels.Model>

let availableThemes = [ 
    Light; Dark; Cyberpunk; Retro; Cupcake
    Bumblebee; Emerald; Corporate; Synthwave
    Retro; Cyberpunk; Valentine; Halloween
    Garden; Forest; Aqua; Lofi; Pastel; Fantasy
    Wireframe; Black; Luxury; Dracula; Cmyk
    Autumn; Business; Acid; Lemonade; Night
    Coffee; Winter; Dim; Nord; Sunset;
]

let themeSelector (currentTheme: SharedWebAppModels.Theme) (dispatch: WebAppMsg -> unit) =
    Html.div [
        prop.className "dropdown dropdown-end"
        prop.children [
            Html.label [
                prop.tabIndex 0
                prop.className "btn m-1"
                prop.text "Theme"
            ]
            Html.ul [
                prop.tabIndex 0
                prop.className "dropdown-content menu p-2 shadow bg-base-100 rounded-box w-52"
                prop.children [
                    for theme in availableThemes ->
                        Html.li [
                            Html.a [
                                prop.text theme.AsString
                                prop.onClick (fun _ -> dispatch (ChangeTheme theme))
                                if currentTheme = theme then prop.className "active"
                            ]
                        ]
                ]
            ]
        ]
    ]


// Web App Header Nav content using Feliz + DaisyUI classes
let headerContent (model: SharedWebAppModels.WebAppModel) dispatch =
    Html.div [
        prop.className "flex items-center justify-between p-4 bg-base-200 shadow-md"
        prop.children [
            Html.div [
                prop.className "flex items-center space-x-4 cursor-pointer"
                prop.onClick (fun _ -> dispatch (SwitchToOtherApp SharedWebAppViewSections.AppSection.WelcomeAppView))
                prop.children [
                    Html.img [ prop.src "./imgs/icons/Flat logo backless.png"; prop.className "w-16 h-16" ]
                    Html.p [ prop.className "text-xl font-bold"; prop.text "Sean Wilken" ]
                ]
            ]

            Html.div [
                prop.className "flex space-x-4"
                let currentSection = currentWebAppSection model.CurrentAreaModel
                prop.children [

                    contentAreas
                    |> Array.map (fun contentArea ->
                        let areaName = contentArea.Name
                        let areaAppSection = areaStringToAppSection areaName
                        let classes =
                            if currentSection = areaAppSection then
                                "btn btn-ghost btn-active"
                            else
                                "btn btn-ghost"
                        Html.button [
                            prop.className classes
                            prop.text areaName
                            prop.onClick (fun _ -> dispatch (SwitchToOtherApp areaAppSection))
                        ])
                    |> Array.toList
                    |> React.fragment
                ]
            ]
            Html.div [
                prop.className "flex space-x-4 cursor-pointer"
                prop.children [
                    themeSelector model.Theme dispatch
                ]
            ]
        ]
    ]

let view (model: SharedWebAppModels.WebAppModel) (dispatch: WebAppMsg -> unit) =
    Html.div [
        headerContent model dispatch
        Html.div [
            prop.className "container mx-auto p-4"
            prop.children [
                match model.CurrentAreaModel with
                | SharedWebAppModels.About aboutModel -> AboutSection.view aboutModel (AboutMsg >> dispatch)
                | SharedWebAppModels.Welcome -> Welcome.view (WelcomeMsg >> dispatch)
                | SharedWebAppModels.Portfolio SharedPortfolioGallery.PortfolioGallery -> Portfolio.view SharedPortfolioGallery.PortfolioGallery (PortfolioMsg >> dispatch)
                | SharedWebAppModels.Portfolio (SharedPortfolioGallery.DesignGallery designModel) -> Portfolio.view (SharedPortfolioGallery.DesignGallery designModel) (PortfolioMsg >> dispatch)
                | SharedWebAppModels.Portfolio (SharedPortfolioGallery.CodeGallery codeModel) -> Portfolio.view (SharedPortfolioGallery.CodeGallery codeModel) (PortfolioMsg >> dispatch)
                | SharedWebAppModels.Contact -> Contact.view
            ]
        ]
    ]
