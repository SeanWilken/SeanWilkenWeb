module Index

open Elmish
open Shared
open PageRouter
open Client.Domain.WebAppModels
open Components.Layout.PageLayout
open Components.FSharp
open SharedViewModule.WebAppView

Fable.Core.JsInterop.importSideEffects "./index.css"

// the init has to have same signature and be called from the index html
let init ( path: Page option ) : WebAppModel * Cmd<WebAppMsg> =
    urlUpdate 
        path 
        { Theme = Cyberpunk; CurrentAreaModel = Model.Welcome }

let sectionModelToPage (shopSection: Shop.ShopSectionModel) : ShopSection =
    match shopSection with
    | Shop.ShopSectionModel.ProductDesigner _ -> ProductDesigner
    | Shop.ShopSectionModel.CollectionBrowser _ -> CollectionBrowser
    | Shop.ShopSectionModel.ProductViewer pv ->  ShopSection.ProductViewer pv.Key
    | Shop.ShopSectionModel.OrderHistory _ -> OrderHistory
    | Shop.ShopSectionModel.Cart -> Cart
    | Shop.ShopSectionModel.ShopLanding -> ShopLanding
    | Shop.ShopSectionModel.Checkout -> Checkout
    | Shop.ShopSectionModel.NotFound ->  NotFound

let update ( msg: WebAppMsg ) ( model: WebAppModel ): WebAppModel * Cmd<WebAppMsg> =
    match msg, model.CurrentAreaModel with
    
    // WELCOME PAGE
    | WelcomeMsg ( Welcome.Msg.SwitchSection appSection ), Model.Welcome ->
        model, Cmd.ofMsg ( SwitchToOtherApp appSection )

    // ABOUT PAGE
    | AboutMsg ( About.SwitchSection appSection ), Model.About m ->
        { model with CurrentAreaModel = Model.About m }, Cmd.ofMsg (SwitchToOtherApp appSection)
    | AboutMsg msg, Model.About ( m ) ->
        let mdl, cmd = About.update msg m
        { model with CurrentAreaModel = Model.About mdl }, cmd

    | ShopMsg msg, Model.Shop shopModel ->
        match msg with
        | Shop.ShopMsg.NavigateTo section ->
           
            model, Cmd.ofMsg (NavigatePage ( Shop (sectionModelToPage section) ))
        | _ -> 
            let updatedModel, com = Shop.update msg shopModel
            { model with CurrentAreaModel = Model.Shop updatedModel }, Cmd.map ShopMsg com

    // PORTFOLIO PAGE
    | PortfolioMsg msg, Model.Portfolio pm ->

        match msg with
        | PortfolioLanding.Msg.LoadSection PortfolioAppLandingView ->
            { model with CurrentAreaModel = Model.Portfolio pm }, Cmd.ofMsg ( NavigatePage ( Portfolio PortfolioLanding ) )
        | PortfolioLanding.Msg.ArtGalleryMsg Portfolio.ArtGallery.Msg.BackToPortfolio -> 
            { model with CurrentAreaModel = Model.Portfolio pm }, Cmd.ofMsg ( NavigatePage ( Portfolio PortfolioLanding ) )
        | PortfolioLanding.Msg.CodeGalleryMsg Portfolio.CodeGallery.Msg.BackToPortfolio -> 
            { model with CurrentAreaModel = Model.Portfolio pm }, Cmd.ofMsg ( NavigatePage ( Portfolio PortfolioLanding ) )
            
        | PortfolioLanding.Msg.LoadSection PortfolioAppCodeView ->
            { model with CurrentAreaModel = Model.Portfolio pm }, Cmd.ofMsg ( NavigatePage ( Portfolio ( Code CodeLanding ) ) )
        | PortfolioLanding.Msg.CodeGalleryMsg (Portfolio.CodeGallery.Msg.LoadSection demo) ->
            { model with CurrentAreaModel = Model.Portfolio pm }, Cmd.ofMsg ( NavigatePage ( Portfolio ( Code demo ) ) )
        | PortfolioLanding.Msg.LoadSection PortfolioAppDesignView ->
           { model with CurrentAreaModel = Model.Portfolio pm }, Cmd.ofMsg ( NavigatePage ( Portfolio (Design DesignGallery ) ) )
        | msg ->
            let portfolioModel, com = 
                match model.CurrentAreaModel with
                | Model.Portfolio pm ->
                    PortfolioLanding.update msg pm
                | _ -> PortfolioLanding.init ()
            { model with CurrentAreaModel = Model.Portfolio portfolioModel }, Cmd.map PortfolioMsg com
    
    // Page Routing
    | SwitchToOtherApp section, _ ->
        printfn $"SWITCHING TO SECTION: {section}"
        match section with
        | AboutAppView -> model, Cmd.ofMsg ( NavigatePage About )
        | ShopAppView -> model, Cmd.ofMsg ( NavigatePage (Shop ShopLanding) )
        | ContactAppView -> model, Cmd.ofMsg ( NavigatePage Contact )
        | PortfolioAppCodeView -> model, Cmd.ofMsg ( NavigatePage ( Portfolio (Code CodeLanding ) ) )
        | PortfolioAppDesignView -> model, Cmd.ofMsg ( NavigatePage ( Portfolio (Design DesignGallery) ) )
        | PortfolioAppLandingView -> model, Cmd.ofMsg ( NavigatePage ( Portfolio PortfolioLanding ) )
        | ResumeAppView -> model, Cmd.ofMsg ( NavigatePage Resume )
        | ProfessionalServicesAppView section   -> model, Cmd.ofMsg ( NavigatePage (Services section) )
        | WelcomeAppView -> model, Cmd.ofMsg ( NavigatePage Welcome )

    | NavigatePage page, _ ->
        printfn $"Selected page: {page}"
        model, navCmd page
    | ChangeTheme theme, _ ->
        printfn $"Selected theme: {theme.AsString}"
        Browser.Dom.document.documentElement.setAttribute("data-theme", theme.AsString)
        { model with Theme = theme }, Cmd.none

    | _ -> model, Cmd.none

open Feliz

[<ReactComponent>]
let View (model: WebAppModel) (dispatch: WebAppMsg -> unit) =
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
                    | Model.Welcome -> Welcome.View (WelcomeMsg >> dispatch)
                    | Model.About aboutModel -> About.View aboutModel (AboutMsg >> dispatch)
                    | Model.Services serviceModel -> Services.Landing.View serviceModel (ServicesMsg >> dispatch)
                    | Model.Contact -> Contact.View ()
                    // Shop
                    | Model.Shop shopModel -> Shop.ShopView shopModel (ShopMsg >> dispatch)
                    // Portfolio - Games & Gallery
                    | Model.Portfolio portfolioModel -> PortfolioLanding.View portfolioModel (PortfolioMsg >> dispatch)
                    // TSX Page
                    | Model.Resume -> Interop.Resume.ResumePage ()
                ]
            ]
    |}
