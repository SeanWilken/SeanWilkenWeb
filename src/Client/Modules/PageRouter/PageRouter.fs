module PageRouter

open Elmish
open Shared

// URL BROWSER UPDATES
open Elmish.UrlParser
open Elmish.Navigation
open Client.Domain
open Components.FSharp.Portfolio.CodeGallery
open Shared.StoreProductViewer
open Components.FSharp
open Components.FSharp.Services.Landing
open Client.Domain.WebAppModels
open SharedViewModule.WebAppView
open Client.Components.Shop

let toPath =
    function
    | Some About -> "/about"
    | Some Contact -> "/contact"
    | Some ( Portfolio PortfolioSection.PortfolioLanding ) -> "/projects/"
    | Some ( Portfolio ( PortfolioSection.SourceCode section ) ) -> 
        match section with
        | GoalRoll -> "/projects/source/goalRoll" 
        | TileTap -> "/projects/source/tileSmash" 
        | TileSort -> "/projects/source/tileSort"
        | PivotPoint -> "/projects/source/pivotPoint"
        | SynthNeverSets -> "/projects/source/synthNeverSets"
        | Animations -> "/projects/source/animations"
        | CodeLanding -> "/projects/source/"
    | Some ( Portfolio ( Code code ) ) -> 
        match code with
        | GoalRoll -> "/projects/code/goalRoll" 
        | TileTap -> "/projects/code/tileSmash" 
        | TileSort -> "/projects/code/tileSort"
        | PivotPoint -> "/projects/code/pivotPoint"
        | SynthNeverSets -> "/projects/code/synthNeverSets"
        | Animations -> "/projects/code/animations"
        | CodeLanding -> "/projects/code/"
    | Some ( Portfolio ( Design area ) ) -> 
        match area with
        | DesignGallery -> "/projects/designs/" 
        | DesignViewer id -> $"/projects/design/{id}" 
    | Some Resume -> "/resume"
    | Some ( Services section ) -> section.toUrlString
    | Some (Shop ShopLanding) -> "/shop/"
    | Some (Shop CollectionBrowser) -> "/shop/collection"
    | Some (Shop ProductDesigner) -> "/shop/designer"
    | Some (Shop (ProductViewer (ProductKey.Template id) )) -> $"/shop/template/{id}"
    | Some (Shop (ProductViewer (ProductKey.Sync id ) )) -> $"/shop/sync/{id}"
    | Some (Shop (ProductViewer (ProductKey.Catalog id ) )) -> $"/shop/catalog/{id}"
    | Some (Shop Cart) -> "/shop/cart"
    | Some (Shop Checkout) -> "/shop/checkout"
    | Some (Shop OrderHistory) -> "/shop/orders"
    | Some (Shop NotFound) -> "/notFound"
    | Some Welcome -> "/welcome"
    | None -> "/welcome"

let int64Parser : Parser<int64 -> 'a, 'a> =
    custom "INT64" (fun str ->
        match System.Int64.TryParse(str) with
        | true, v -> Ok v
        | _ -> Error "Invalid int64"
    )

let shopParser : Parser<ShopSection -> Page, _> =
    oneOf [
        map ShopLanding (s "")

        map
            (fun catalogId -> ProductViewer (Catalog catalogId))
            (s "catalog" </> i32)

        map
            (fun templateId -> ProductViewer (Template templateId))
            (s "template" </> i32)

        map
            (fun syncId -> ProductViewer (Sync syncId))
            (s "sync" </> int64Parser)

        map CollectionBrowser (s "collection")
        map ProductDesigner (s "designer")
        map Cart (s "cart")
        map OrderHistory (s "orders")
        map Checkout (s "checkout")
    ]

let codeGalleryParser : Parser<CodeSection->PortfolioSection,_> =
    oneOf [
        map CodeLanding (s "")
        map GoalRoll (s "goalRoll")
        map TileSort (s "tileSort")
        map TileTap (s "tileSmash")
        map PivotPoint (s "pivotPoint")
        map SynthNeverSets (s "synthNeverSets")
        map Animations (s "animations")
    ]

let sourceCodeViewerParser : Parser<CodeSection->PortfolioSection,_> =
    oneOf [
        map CodeLanding (s "")
        map GoalRoll (s "goalRoll")
        map TileSort (s "tileSort")
        map TileTap (s "tileSmash")
        map PivotPoint (s "pivotPoint")
        map SynthNeverSets (s "synthNeverSets")
        map Animations (s "animations")
    ]

let designParser : Parser<DesignSection->PortfolioSection,_> =
    oneOf [
        map DesignGallery (s "")
        map DesignViewer (s "design" </> i32 )
    ]

let portfolioParser : Parser<PortfolioSection->Page,_> =
    oneOf [
        map PortfolioLanding (s "")
        map (fun cs -> SourceCode cs) (s "source" </> sourceCodeViewerParser)
        map (fun cs -> Code cs) (s "code" </> codeGalleryParser)
        map (fun ds -> Design ds) (s "designs" </> designParser)
    ]

let servicesParser : Parser<ProfessionalServicesView->Page,_> =
    oneOf [
        map ProfessionalServicesView.ServicesLanding (s "")
        map AI (s "ai-services")
        map Automation (s "automation-services")
        map Development (s "development-services")
        map Integration (s "integration-services")
        map SalesPlatform (s "sales-services")
        map Website (s "web-services")
    ]

let pageParser : Parser<Page -> Page,_> =
    oneOf [
        map Page.About (s "about")
        map Page.Contact (s "contact")
        map Page.Resume (s "resume")
        map Page.Welcome (s "welcome")
        map Page.Portfolio (s "projects" </> portfolioParser)
        map Page.Shop (s "shop" </> shopParser)
        map Page.Services (s "services" </> servicesParser)
    ]

let urlParser location = parsePath pageParser location

let navCmd page = Navigation.newUrl (toPath (Some page))

let urlUpdate (result: Page option) (model: WebAppModels.WebAppModel) : WebAppModels.WebAppModel * Cmd<WebAppMsg> =
    match result with
    | Some About ->
        { model with CurrentAreaModel = WebAppModels.About About.getInitialModel },
        Cmd.none
    | Some Contact ->
        { model with CurrentAreaModel = WebAppModels.Contact },
        Cmd.none
    | Some (Portfolio (Code CodeLanding)) ->
        { model with CurrentAreaModel = WebAppModels.Portfolio (PortfolioLanding.Model.CodeGallery CodeGallery) },
        Cmd.none 
    | Some (Portfolio (Code GoalRoll)) ->
        { model with CurrentAreaModel = WebAppModels.Portfolio (PortfolioLanding.Model.CodeGallery ( Model.GoalRoll Portfolio.Games.GoalRoll.initModel)) },
        Cmd.none
    | Some (Portfolio (Code TileSort)) ->
        { model with CurrentAreaModel = WebAppModels.Portfolio (PortfolioLanding.Model.CodeGallery ( Model.TileSort Portfolio.Games.TileSort.initModel)) },
        Cmd.none
    | Some (Portfolio (Code TileTap)) ->
        { model with CurrentAreaModel = WebAppModels.Portfolio (PortfolioLanding.Model.CodeGallery ( Model.TileTap Portfolio.Games.TileTap.initModel)) },
        Cmd.none
    | Some (Portfolio (Code SynthNeverSets)) ->
        { model with CurrentAreaModel = WebAppModels.Portfolio (PortfolioLanding.Model.CodeGallery Model.SynthNeverSets) },
        Cmd.none
    | Some (Portfolio (Code Animations)) ->
        { model with CurrentAreaModel = WebAppModels.Portfolio (PortfolioLanding.Model.CodeGallery Model.Animations) },
        Cmd.none
    | Some (Portfolio (Design DesignGallery)) ->
        { model with CurrentAreaModel = WebAppModels.Portfolio (PortfolioLanding.Model.DesignGallery Portfolio.ArtGallery.initialModel) },
        Cmd.ofMsg (PortfolioMsg (PortfolioLanding.Msg.ArtGalleryMsg Portfolio.ArtGallery.Msg.LoadGalleryPieces))
    | Some (Portfolio _) ->
        { model with CurrentAreaModel = WebAppModels.Portfolio PortfolioLanding.Model.PortfolioGallery },
        Cmd.none
    | Some (Services section) ->
        { model with CurrentAreaModel = WebAppModels.Services (getInitialModel section) },
        Cmd.none
    | Some Resume ->
        { model with CurrentAreaModel = WebAppModels.Resume },
        Cmd.none
    | Some (Shop area) ->
        let initCmd =
            match area with
            | ProductViewer pk ->
                let id =
                    match pk with
                    | Sync s -> s
                    | Catalog c -> c
                    | Template t -> t
                    |> int
                Cmd.ofMsg (ShopMsg (Shop.ShopMsg.ShopProduct (Product.Msg.LoadProductDetails id)))
            | ProductDesigner -> Cmd.ofMsg (ShopMsg (Shop.ShopMsg.ShopDesignerMsg Designer.Msg.LoadProducts))
            | CollectionBrowser -> Cmd.ofMsg (ShopMsg (Shop.ShopMsg.ShopCollectionMsg Collection.Msg.LoadMore))
            | _ -> Cmd.none
        match model.CurrentAreaModel with
        | WebAppModels.Shop shop ->
            { model with CurrentAreaModel = WebAppModels.Shop { shop with Section = Shop.shopSectionToSectionModel area } }
        | _ -> 
            { model with CurrentAreaModel = WebAppModels.Shop (Shop.getInitialModel area)  }
        , initCmd
    | None
    | Some Welcome ->
        { model with CurrentAreaModel = WebAppModels.Welcome },
        Cmd.none
