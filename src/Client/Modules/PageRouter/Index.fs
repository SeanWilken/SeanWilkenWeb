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
    | Some ( Portfolio ( Code ( code ) ) ) -> 
        // match code with
        match code with
        | GoalRoll -> sprintf "/portfolio-goalRoll" 
        | TileTap -> sprintf "/portfolio-tileSmash" 
        | TileSort -> sprintf "/portfolio-tileSort"
        | CodeSection.Landing -> "/portfolio-code"
    | Some ( Portfolio ( Design ) ) ->
        sprintf "/portfolio-design"
    | Some ( Portfolio _ )
    | Some ( Portfolio Landing ) -> "/portfolio"
    | Some Contact -> "/contact"
    | None
    | Some Welcome -> "/welcome"

// router use combinators for better structured paths
let pageParser : Parser< Page -> Page,_ > =
    oneOf
        [
            map Page.Welcome ( s "welcome" )
            map Page.About ( s "about" )
            map ( Page.Portfolio Landing) ( s "portfolio" )
            map ( Page.Portfolio ( Code ( CodeSection.Landing ) ) ) ( s "portfolio-code" )
            map ( Page.Portfolio Design ) ( s "portfolio-design" )
            map ( Page.Portfolio ( Code ( CodeSection.GoalRoll ) ) ) ( s "portfolio-goalRoll" )
            map ( Page.Portfolio ( Code ( CodeSection.TileSort ) ) ) ( s "portfolio-tileSort" )
            map ( Page.Portfolio ( Code ( CodeSection.TileTap ) ) ) ( s "portfolio-tileSmash" )
            map Page.Contact ( s "contact" )
        ]

let urlParser location = 
    // printfn "url is: %A" location
    parsePath pageParser location

let urlUpdate (result: SharedPage.Page option) (model: SharedWebAppModels.WebAppModel) : SharedWebAppModels.WebAppModel * Cmd<SharedWebAppModels.WebAppMsg> =
    match result with
    | Some SharedPage.Page.About ->
        { model with CurrentAreaModel = SharedWebAppModels.About SharedAboutSection.getInitialModel },
        Navigation.newUrl (toPath (Some About)) 

    | Some (SharedPage.Page.Portfolio (SharedPage.Code SharedPage.CodeSection.Landing)) ->
        { model with CurrentAreaModel = SharedWebAppModels.Portfolio (SharedPortfolioGallery.CodeGallery SharedCodeGallery.CodeGallery) },
        Navigation.newUrl (toPath (Some (Portfolio (Code CodeSection.Landing)))) 

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
        Navigation.newUrl (toPath (Some (Portfolio Landing)))
    | Some SharedPage.Page.Contact ->
        { model with CurrentAreaModel = SharedWebAppModels.Contact },
        Navigation.newUrl (toPath (Some Contact)) 

    | None
    | Some SharedPage.Page.Welcome ->
        { model with CurrentAreaModel = SharedWebAppModels.Welcome },
        Navigation.newUrl (toPath (Some Welcome))
