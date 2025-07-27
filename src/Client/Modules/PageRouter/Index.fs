module PageRouter

open Elmish
open Shared

// URL BROWSER UPDATES
open Elmish.UrlParser
open Elmish.Navigation
open SharedPage

let checkLevelIsValid levelInt ceiling floor =
    if levelInt > ceiling then ceiling
    elif levelInt < floor then floor
    else levelInt

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
    | Some Resume -> "/resume"
    | Some Welcome -> "/welcome"
    | Some Index
    | None -> "/"

// router use combinators for better structured paths
let pageParser : Parser< Page -> Page,_ > =
    oneOf
        [
            map Page.About ( s "about" )
            map Page.Contact ( s "contact" )
            map ( Page.Landing) ( s "landing" )
            map ( Page.Landing) ( s "index" )
            map ( Page.Portfolio PortfolioSection.PortfolioLanding) ( s "portfolio" )
            map ( Page.Portfolio ( Code ( CodeSection.CodeLanding ) ) ) ( s "portfolio-code" )
            map ( Page.Portfolio Design ) ( s "portfolio-design" )
            map ( Page.Portfolio ( Code ( CodeSection.GoalRoll ) ) ) ( s "portfolio-goalRoll" )
            map ( Page.Portfolio ( Code ( CodeSection.TileSort ) ) ) ( s "portfolio-tileSort" )
            map ( Page.Portfolio ( Code ( CodeSection.TileTap ) ) ) ( s "portfolio-tileSmash" )
            map Page.Resume ( s "resume" )
            map Page.Welcome ( s "welcome" )
        ]

let urlParser location = 
    // printfn "url is: %A" location
    parsePath pageParser location

let urlUpdate (result: SharedPage.Page option) (model: SharedWebAppModels.WebAppModel) : SharedWebAppModels.WebAppModel * Cmd<SharedWebAppModels.WebAppMsg> =
    match result with
    | None
    | Some SharedPage.Page.Index
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
    | Some SharedPage.Page.Resume ->
        printfn $"Navigating to Resume page"
        { model with CurrentAreaModel = SharedWebAppModels.Resume },
        Navigation.newUrl (toPath (Some Resume))
    | Some SharedPage.Page.Welcome ->
        { model with CurrentAreaModel = SharedWebAppModels.Welcome },
        Navigation.newUrl (toPath (Some Welcome))
