namespace Client.Shop

open Elmish
open Shared
open Shared.SharedShopV2
open Shared.SharedShopDomain
open Client.Domain
open Shared.SharedShopV2.PrintfulCatalog
open Shared.SharedShopV2.ProductTemplate
open Shared.SharedShopV2Domain.ProductTemplateResponse
open Shared.SharedShopV2Domain.CatalogProductResponse
open Shared.PrintfulCommon
open Client.Domain.Store

module Domain =

    // --- Cart sub-domain ---

    module Cart =

        type Model = {
            Items: CartItem list
        }

        type Msg =
            | AddTemplate of ProductTemplate.ProductTemplate
            // | AddCustom   of BuildYourOwnProductWizard.Model
            | RemoveItem  of CartItem
            | AdjustQty   of CartItem * QuantityAdjustment
            | Clear

        let init () = { Items = [] }

        let private adjustQty (item: CartItem) (adj: QuantityAdjustment) : CartItem =
            match item, adj with
            | Template (tpl, qty), Increment -> Template (tpl, qty + 1)
            | Template (tpl, qty), Decrement -> Template (tpl, max 1 (qty - 1))
            | Custom (byo, qty), Increment   -> Custom (byo, qty + 1)
            | Custom (byo, qty), Decrement   -> Custom (byo, max 1 (qty - 1))

        let update (msg: Msg) (model: Model) : Model =
            match msg with
            | AddTemplate tpl ->
                // naive: always add as new line; you can merge by id later
                { model with Items = Template (tpl, 1) :: model.Items }

            // | AddCustom byo ->
            //     { model with Items = Custom (byo, 1) :: model.Items }

            | RemoveItem item ->
                { model with Items = model.Items |> List.except [ item ] }

            | AdjustQty (item, adj) ->
                let items' =
                    model.Items
                    |> List.map (fun i -> if obj.ReferenceEquals(i, item) then adjustQty i adj else i)
                { model with Items = items' }

            | Clear ->
                { model with Items = [] }

    module Collection =

        type Model = {
            Filters    : Filters
            Paging     : PagingInfoDTO
            SearchTerm : string option
            Products   : CatalogProduct list
            ProductTemplates   : ProductTemplate list
            TotalCount : int
            IsLoading  : bool
            Error      : string option
        }

        type Msg =
            | InitFromQuery of string   // raw query string
            // Printful Base Products
            | LoadProducts
            | ProductsLoaded of CatalogProductsResponse
            | LoadFailed of string
            | ViewProduct of CatalogProduct
            | FeaturedClick of CatalogProduct option
            | LoadMore
            | FiltersChanged of Filters
            | SearchChanged of string
            | SortChanged of sortType: string * sortDir: string
            | ApplyFilterPreset of string
            | SaveFilterPreset of string

            // Printful STORE Products
            | GetProductTemplates
            | GotProductTemplates of ProductTemplatesResponse
            | FailedProductTemplates of exn
            | ViewProductTemplate of ProductTemplate
            | ViewFeaturedProductTemplate of ProductTemplate option

        let initModel () : Model = { 
                Filters    = defaultFilters
                Paging     = emptyPaging
                SearchTerm = None
                Products   = []
                ProductTemplates   = []
                TotalCount = 0
                IsLoading  = false
                Error      = None 
            }
        
        let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
            match msg with
            | InitFromQuery _ ->
                // TODO: parse query → filters/paging/search
                model, Cmd.ofMsg LoadProducts

            | LoadProducts ->
                // TODO: call client API: Client.Api.productsApi.getProducts
                { model with IsLoading = true }, Cmd.none

            | ProductsLoaded res ->
                { model with
                    Products   = res.products
                    TotalCount = res.paging.total
                    Paging     = res.paging
                    IsLoading  = false
                    Error      = None },
                Cmd.none

            | LoadFailed err ->
                { model with IsLoading = false; Error = Some err }, Cmd.none

            | ViewProduct _p ->
                // The parent will handle navigation to product detail
                model, Cmd.none

            | FeaturedClick _ ->
                // Could trigger overlay or same as ViewProduct
                model, Cmd.none

            | LoadMore ->
                let nextPaging =
                    { model.Paging with
                        offset = model.Paging.offset + model.Paging.limit }
                { model with Paging = nextPaging }, Cmd.ofMsg LoadProducts

            | FiltersChanged filters ->
                { model with Filters = filters; Paging = { model.Paging with offset = 0 } },
                Cmd.ofMsg LoadProducts

            | SearchChanged term ->
                { model with SearchTerm = Some term; Paging = { model.Paging with offset = 0 } },
                Cmd.ofMsg LoadProducts

            | SortChanged (sortType, sortDir) ->
                let updatedFilters =
                    { model.Filters with
                        SortType      = Some sortType
                        SortDirection = Some sortDir }
                { model with Filters = updatedFilters; Paging = { model.Paging with offset = 0 } },
                Cmd.ofMsg LoadProducts

            | ApplyFilterPreset _key ->
                // TODO: load preset from localStorage (client-side)
                model, Cmd.ofMsg LoadProducts

            | SaveFilterPreset _key ->
                // TODO: write preset to localStorage
                model, Cmd.none

    module ProductDesigner =

        open Shared.PrintfulCommon

        type StepDesigner =
            | SelectBaseProduct
            | SelectVariants
            | SelectCustomDesign
            | ConfigureDesignPlacement
            | ReviewDesign
            
        type Msg =
            | GoToStep of StepDesigner
            | SelectBase of CatalogProduct
            | SelectColor of string
            | SelectSize of string
            | AddDesignPlaceholder
            | LoadProducts
            | ProductsLoaded of CatalogProductsResponse
            | LoadFailed of string
            | ViewProduct of CatalogProduct

        type Model = {
            products: CatalogProduct list
            paging: PagingInfoDTO
            query: Filters
            currentStep: StepDesigner
            selectedProduct: CatalogProduct option
            selectedVariantSize: string option
            selectedVariantColor: string option
            selectedDesign: string option
            placements: (string * string) list // placement, url
        }

        open Elmish
        
        // ADD DEFERRED!!!!
        let initialModel () = {
            products = []
            paging = emptyPaging
            query = defaultFilters
            currentStep = SelectBaseProduct
            selectedProduct = None
            selectedVariantSize = None
            selectedVariantColor = None
            selectedDesign = None
            placements = []
        }

    module ShopAppView =

        type ShopSection =
            | DropLanding
            | ProductDesigner
            | CollectionBrowser
            | ShoppingBag
            | Checkout
            // | Payment
            // | Social
            | Contact
            | NotFound

        type Msg =
            | BackToPortfolio
            | LoadSection of ShopSection
            | ProductDesignerMsg of ProductDesigner.Msg
            | CollectionBrowserMsg of Collection.Msg

        type Model =
            | DropHero
            | Designer of ProductDesigner.Model
            | ProductCollection of SharedTileTap.Model
            | Other

        let getInitialModel = DropHero