module PageRouter

open Elmish
open Shared

// URL BROWSER UPDATES
open Elmish.UrlParser
open Elmish.Navigation
// open Shared.SharedShop
open Shared.SharedShopV2Domain
open SharedPage


let toPath =
    function
    | Some About -> "/about"
    | Some Contact -> "/contact"
    | Some Landing -> "/landing"
    | Some ( Portfolio ( Code ( code ) ) ) -> 
        match code with
        | GoalRoll -> "/portfolio-goalRoll" 
        | TileTap -> "/portfolio-tileSmash" 
        | TileSort -> "/portfolio-tileSort"
        | CodeSection.CodeLanding -> "/portfolio-code"
    | Some ( Portfolio Design ) -> "/portfolio-design"
    | Some ( Portfolio _ )
    | Some ( Portfolio PortfolioSection.PortfolioLanding ) -> "/portfolio"
    | Some ( Services section ) -> section.toUrlString
    | Some Resume -> "/resume"
    | Some Welcome -> "/welcome"
    | Some (Shop SharedShopV2.ShopSection.ShopLanding) -> "/shop/landing"
    | Some (Shop SharedShopV2.ShopSection.ShopTypeSelector) -> "/shop/select"
    | Some (Shop (SharedShopV2.ShopSection.ProductTemplateBrowser _)) -> "/shop/templates"
    | Some (Shop (SharedShopV2.ShopSection.BuildYourOwnWizard _)) -> "/shop/build"
    | Some (Shop SharedShopV2.ShopSection.ShoppingBag) -> "/shop/shoppingBag"
    | Some (Shop SharedShopV2.ShopSection.Checkout) -> "/shop/checkout"
    | Some (Shop SharedShopV2.ShopSection.Payment) -> "/shop/payment"
    | Some (Shop SharedShopV2.ShopSection.Contact) -> "/shop/contact"
    | Some (Shop SharedShopV2.ShopSection.Social) -> "/shop/social"
    | Some (Shop SharedShopV2.ShopSection.NotFound) -> "/notFound"
    | None -> "/"


let shopParser : Parser<SharedShopV2.ShopSection->Page,_> =
    oneOf [
        map SharedShopV2.ShopLanding (s "landing")
        map SharedShopV2.ShopTypeSelector (s "select")
        map (SharedShopV2.ProductTemplateBrowser (SharedShopV2.ProductTemplate.ProductTemplateBrowser.initialModel())) (s "templates")
        map (SharedShopV2.BuildYourOwnWizard (SharedShopV2.BuildYourOwnProductWizard.initialState ())) (s "build")
        map SharedShopV2.ShoppingBag (s "shoppingBag")
        map SharedShopV2.Checkout (s "checkout")
        map SharedShopV2.Payment (s "payment")
    ]


// router use combinators for better structured paths
// let pageParser : Parser< Page -> Page,_ > =
//     oneOf
//         [
//             map Page.About ( s "about" )
//             map Page.Contact ( s "contact" )
//             map ( Page.Landing) ( s "landing" )
//             map ( Page.Landing) ( s "index" )
//             map ( Page.Services SharedWebAppViewSections.ProfessionalServicesView.SalesPlatform) ( s "services-sales" )
//             map ( Page.Services SharedWebAppViewSections.ProfessionalServicesView.ServicesLanding) ( s "services" )
//             map ( Page.Portfolio PortfolioSection.PortfolioLanding) ( s "portfolio" )
//             map ( Page.Portfolio ( Code ( CodeSection.CodeLanding ) ) ) ( s "portfolio-code" )
//             map ( Page.Portfolio Design ) ( s "portfolio-design" )
//             map ( Page.Portfolio ( Code ( CodeSection.GoalRoll ) ) ) ( s "portfolio-goalRoll" )
//             map ( Page.Portfolio ( Code ( CodeSection.TileSort ) ) ) ( s "portfolio-tileSort" )
//             map ( Page.Portfolio ( Code ( CodeSection.TileTap ) ) ) ( s "portfolio-tileSmash" )
//             map Page.Resume ( s "resume" )
//             map (Page.Shop ) (s "shop" </> shopParser)
//             map Page.Welcome ( s "welcome" )
//         ]

let pageParser : Parser<Page -> Page,_> =
    oneOf [
        map Page.About (s "about")
        map Page.Contact (s "contact")
        map Page.Landing (s "landing")
        map Page.Landing (s "index")
        map (Page.Services SharedWebAppViewSections.ProfessionalServicesView.AI) (s "ai-services")
        map (Page.Services SharedWebAppViewSections.ProfessionalServicesView.Automation) (s "automation-services")
        map (Page.Services SharedWebAppViewSections.ProfessionalServicesView.Development) (s "development-services")
        map (Page.Services SharedWebAppViewSections.ProfessionalServicesView.Integration) (s "integration-services")
        map (Page.Services SharedWebAppViewSections.ProfessionalServicesView.SalesPlatform) (s "sales-services")
        map (Page.Services SharedWebAppViewSections.ProfessionalServicesView.ServicesLanding) (s "services")
        map (Page.Services SharedWebAppViewSections.ProfessionalServicesView.Website) (s "web-services")
        map (Page.Portfolio PortfolioSection.PortfolioLanding) (s "portfolio")
        map (Page.Portfolio (Code CodeSection.CodeLanding)) (s "portfolio-code")
        map (Page.Portfolio Design) (s "portfolio-design")
        map (Page.Portfolio (Code CodeSection.GoalRoll)) (s "portfolio-goalRoll")
        map (Page.Portfolio (Code CodeSection.TileSort)) (s "portfolio-tileSort")
        map (Page.Portfolio (Code CodeSection.TileTap)) (s "portfolio-tileSmash")
        map Page.Resume (s "resume")
        map Page.Welcome (s "welcome")
        map Page.Shop (s "shop" </> shopParser)
    ]

let urlParser location = 
    // printfn "url is: %A" location
    parsePath pageParser location

let urlUpdate (result: SharedPage.Page option) (model: SharedWebAppModels.WebAppModel) : SharedWebAppModels.WebAppModel * Cmd<SharedWebAppModels.WebAppMsg> =
    match result with
    | Some SharedPage.Page.Landing ->
        { model with CurrentAreaModel = SharedWebAppModels.Landing },
        Navigation.newUrl (toPath (Some Landing))
    | Some SharedPage.Page.About ->
        { model with CurrentAreaModel = SharedWebAppModels.About SharedAboutSection.getInitialModel },
        Navigation.newUrl (toPath (Some About))
    | Some SharedPage.Page.Contact ->
        { model with CurrentAreaModel = SharedWebAppModels.Contact },
        Navigation.newUrl (toPath (Some Contact)) 
    | Some (SharedPage.Page.Portfolio (SharedPage.Code SharedPage.CodeSection.CodeLanding)) ->
        { model with CurrentAreaModel = SharedWebAppModels.Portfolio (SharedPortfolioGallery.CodeGallery SharedCodeGallery.CodeGallery) },
        Navigation.newUrl (toPath (Some (Portfolio (Code CodeSection.CodeLanding)))) 
    | Some (SharedPage.Page.Portfolio (SharedPage.Code SharedPage.CodeSection.GoalRoll)) ->
        { model with CurrentAreaModel = SharedWebAppModels.Portfolio (SharedPortfolioGallery.CodeGallery (SharedCodeGallery.Model.GoalRoll SharedGoalRoll.initModel)) },
        Navigation.newUrl (toPath (Some (Portfolio (Code CodeSection.GoalRoll)))) 
    | Some (SharedPage.Page.Portfolio (SharedPage.Code SharedPage.CodeSection.TileSort)) ->
        { model with CurrentAreaModel = SharedWebAppModels.Portfolio (SharedPortfolioGallery.CodeGallery (SharedCodeGallery.Model.TileSort SharedTileSort.initModel)) },
        Navigation.newUrl (toPath (Some (Portfolio (Code CodeSection.TileSort)))) 
    | Some (SharedPage.Page.Portfolio (SharedPage.Code SharedPage.CodeSection.TileTap)) ->
        { model with CurrentAreaModel = SharedWebAppModels.Portfolio (SharedPortfolioGallery.CodeGallery (SharedCodeGallery.Model.TileTap SharedTileTap.initModel)) },
        Navigation.newUrl (toPath (Some (Portfolio (Code CodeSection.TileTap))))
    | Some (SharedPage.Page.Portfolio SharedPage.Design) ->
        { model with CurrentAreaModel = SharedWebAppModels.Portfolio (SharedPortfolioGallery.DesignGallery SharedDesignGallery.getInitialModel) },
        Navigation.newUrl (toPath (Some (Portfolio Design)))
    | Some (SharedPage.Page.Portfolio _) ->
        { model with CurrentAreaModel = SharedWebAppModels.Portfolio SharedPortfolioGallery.PortfolioGallery },
        Navigation.newUrl (toPath (Some (Portfolio PortfolioSection.PortfolioLanding)))
    | Some (SharedPage.Page.Services section) ->
        { model with CurrentAreaModel = SharedWebAppModels.Services (SharedServices.getInitialModel section) },
        Navigation.newUrl (toPath (Some (Services section)))
    | Some SharedPage.Page.Resume ->
        { model with CurrentAreaModel = SharedWebAppModels.Resume },
        Navigation.newUrl (toPath (Some Resume))
    | Some (SharedPage.Page.Shop area) ->
        match model.CurrentAreaModel with
        | SharedWebAppModels.Shop shop ->
            { model with CurrentAreaModel = SharedWebAppModels.Shop { shop with Section = area } }
        | _ ->
            { model with CurrentAreaModel = SharedWebAppModels.Shop (SharedShop.getInitialModel area)  }
        , Navigation.newUrl (toPath (Some (Shop area)))
    | None
    | Some SharedPage.Page.Welcome ->
        { model with CurrentAreaModel = SharedWebAppModels.Welcome },
        Navigation.newUrl (toPath (Some Welcome))
