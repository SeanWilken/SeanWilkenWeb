module PageRouter

open Elmish
open Shared

// URL BROWSER UPDATES
open Elmish.UrlParser
open Elmish.Navigation
open Shared.SharedShop
open Shared.SharedShopDomain
open SharedPage


let toPath =
    function
    | Some About -> "/about"
    | Some Contact -> "/contact"
    | Some Landing -> "/landing"
    | Some ( Portfolio ( Code ( code ) ) ) -> 
        // match code with
        match code with
        | GoalRoll -> sprintf "/portfolio-goalRoll" 
        | TileTap -> sprintf "/portfolio-tileSmash" 
        | TileSort -> sprintf "/portfolio-tileSort"
        | CodeSection.CodeLanding -> "/portfolio-code"
    | Some ( Portfolio ( Design ) ) ->
        sprintf "/portfolio-design"
    | Some ( Portfolio _ )
    | Some ( Portfolio PortfolioSection.PortfolioLanding ) -> "/portfolio"
    | Some ( Services section ) -> section.toUrlString
    | Some Resume -> "/resume"
    | Some Welcome -> "/welcome"
    | Some (Shop ShopLanding) -> "/shop/landing"
    | Some (Shop Storefront) -> "/shop/storefront"
    | Some (Shop (Catalog category)) -> $"/shop/catalog/{category}"
    | Some (Shop (Product (category, id))) -> $"/shop/product/{category}/{id}"
    | Some (Shop ShoppingBag) -> "/shop/shoppingBag"
    | Some (Shop Checkout) -> "/shop/checkout"
    | Some (Shop Payment) -> "/shop/payment"
    | Some (Shop ShopSection.Contact) -> "/shop/contact"
    | Some (Shop ShopSection.Social) -> "/shop/social"
    | Some (Shop ShopSection.NotFound) -> "/notFound"
    | None -> "/"

let shopParser : Parser<ShopSection->Page,_> =
    oneOf [
        map ShopLanding (s "landing")
        map Storefront (s "storefront")
        map Catalog (s "catalog" </> str)  // string only, for category
        // Product parser: combine string and int segments
        map (fun category pid -> Product (category, pid)) ((s "product" </> str) </> i32)
        map ShoppingBag (s "shoppingBag")
        map Checkout (s "checkout")
        map Payment (s "payment")
    ]

// router use combinators for better structured paths
let pageParser : Parser< Page -> Page,_ > =
    oneOf
        [
            map Page.About ( s "about" )
            map Page.Contact ( s "contact" )
            map ( Page.Landing) ( s "landing" )
            map ( Page.Landing) ( s "index" )
            map ( Page.Services SharedWebAppViewSections.ProfessionalServicesView.SalesPlatform) ( s "services-sales" )
            map ( Page.Services SharedWebAppViewSections.ProfessionalServicesView.ServicesLanding) ( s "services" )
            map ( Page.Portfolio PortfolioSection.PortfolioLanding) ( s "portfolio" )
            map ( Page.Portfolio ( Code ( CodeSection.CodeLanding ) ) ) ( s "portfolio-code" )
            map ( Page.Portfolio Design ) ( s "portfolio-design" )
            map ( Page.Portfolio ( Code ( CodeSection.GoalRoll ) ) ) ( s "portfolio-goalRoll" )
            map ( Page.Portfolio ( Code ( CodeSection.TileSort ) ) ) ( s "portfolio-tileSort" )
            map ( Page.Portfolio ( Code ( CodeSection.TileTap ) ) ) ( s "portfolio-tileSmash" )
            map Page.Resume ( s "resume" )
            map (Page.Shop ) (s "shop" </> shopParser)
            map Page.Welcome ( s "welcome" )
        ]

let urlParser location = 
    // printfn "url is: %A" location
    parsePath pageParser location

let urlUpdate (result: SharedPage.Page option) (model: SharedWebAppModels.WebAppModel) : SharedWebAppModels.WebAppModel * Cmd<SharedWebAppModels.WebAppMsg> =
    match result with
    | None
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
        Navigation.newUrl (toPath (Some (Services SharedWebAppViewSections.ProfessionalServicesView.ServicesLanding)))
    | Some SharedPage.Page.Resume ->
        { model with CurrentAreaModel = SharedWebAppModels.Resume },
        Navigation.newUrl (toPath (Some Resume))
    | Some (SharedPage.Page.Shop area) ->
        match model.CurrentAreaModel with
        | SharedWebAppModels.Shop shop ->
            { model with CurrentAreaModel = SharedWebAppModels.Shop { shop with section = area } }
        | _ ->
            { model with CurrentAreaModel = SharedWebAppModels.Shop (getInitialModel area)  }
        , Navigation.newUrl (toPath (Some (Shop area)))
    | Some SharedPage.Page.Welcome ->
        { model with CurrentAreaModel = SharedWebAppModels.Welcome },
        Navigation.newUrl (toPath (Some Welcome))
