namespace Client.Shop

open Elmish
open Shared
open Shared.SharedShopV2
open Shared.SharedShopV2Domain
open Shared.SharedShopDomain
open Client.Domain

module Domain =

    // ------------------------
    // High-level sections
    // ------------------------

    /// More UI-centric alias around SharedShopV2.ShopSection if you want,
    /// or you can just reuse that type directly.
    type Section =
        | ShopLanding
        | ShopTypeSelector
        | Collection
        | ProductTemplates
        | BuildYourOwn
        | ShoppingBag
        | Checkout
        | Payment
        | Social
        | Contact
        | NotFound

    // ------------------------
    // Sub-module Models
    // ------------------------

    module Collection =

        type Model = {
            Filters    : PrintfulCatalog.Filters
            Paging     : PrintfulCommon.PagingInfoDTO
            SearchTerm : string option
            Products   : PrintfulCatalog.CatalogProduct list
            TotalCount : int
            IsLoading  : bool
            Error      : string option
        }

        type Msg =
            | InitFromQuery of string   // raw query string
            | LoadProducts
            | ProductsLoaded of CatalogProductResponse.CatalogProductsResponse
            | LoadFailed of string
            | ViewProduct of PrintfulCatalog.CatalogProduct
            | FeaturedClick of PrintfulCatalog.CatalogProduct option
            | LoadMore
            | FiltersChanged of PrintfulCatalog.Filters
            | SearchChanged of string
            | SortChanged of sortType: string * sortDir: string
            | ApplyFilterPreset of string
            | SaveFilterPreset of string

        let init () : Model * Cmd<Msg> =
            { Filters    = PrintfulCatalog.defaultFilters
              Paging     = PrintfulCommon.emptyPaging
              SearchTerm = None
              Products   = []
              TotalCount = 0
              IsLoading  = false
              Error      = None },
            Cmd.none

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

    // --- Template Browser (wrap your existing update/View) ---

    module TemplateBrowser =
        open Client.Domain.SharedShopV2Domain

        type Model = SharedShopV2.ProductTemplate.ProductTemplateBrowser.Model

        type Msg =
            | Init
            | Internal of ShopProductTemplatesMsg

        let init () : Model * Cmd<Msg> =
            let m = SharedShopV2.ProductTemplate.ProductTemplateBrowser.initialModel ()
            m, Cmd.ofMsg Init

        let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
            match msg with
            | Init ->
                // parent will call existing component's GetProductTemplates
                model, Cmd.none
            | Internal sub ->
                let m', cmd' = Components.FSharp.Pages.ProductTemplateBrowser.update sub model
                m', cmd' |> Cmd.map Internal

    // --- Cart sub-domain ---

    module Cart =
        open Client.Domain.SharedShop

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

    // --- Checkout (minimal for now; payment ignored) ---

    module Checkout =

        type Model = {
            Tax      : float option
            Shipping : float option
            PayPalRef: string option
        }

        type Msg =
            | SetTax       of float option
            | SetShipping  of float option
            | SetPayPalRef of string option

        let init () = { Tax = None; Shipping = None; PayPalRef = None }

        let update (msg: Msg) (model: Model) =
            match msg with
            | SetTax t       -> { model with Tax = t }
            | SetShipping s  -> { model with Shipping = s }
            | SetPayPalRef r -> { model with PayPalRef = r }

    // ------------------------
    // Top-level Model & Msg
    // ------------------------

    // type Model = {
    //     Section   : Section
    //     Collection: Collection.Model
    //     Templates : TemplateBrowser.Model
    //     Cart      : Cart.Model
    //     Checkout  : Checkout.Model
    // }

    // type Msg =
    //     | NavigateTo of Section * query: string option

    //     | CollectionMsg      of Collection.Msg
    //     | TemplateBrowserMsg of TemplateBrowser.Msg
    //     | CartMsg            of Cart.Msg
    //     | CheckoutMsg        of Checkout.Msg

    // let init (initialSection: Section) : Model * Cmd<Msg> =
    //     let collection, collectionCmd = Collection.init ()
    //     let templates, templatesCmd   = TemplateBrowser.init ()
    //     let cart                      = Cart.init ()
    //     let checkout                  = Checkout.init ()

    //     { Section    = initialSection
    //       Collection = collection
    //       Templates  = templates
    //       Cart       = cart
    //       Checkout   = checkout },
    //     Cmd.batch [
    //         collectionCmd |> Cmd.map CollectionMsg
    //         templatesCmd  |> Cmd.map TemplateBrowserMsg
    //     ]

    // let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    //     match msg with
    //     | NavigateTo (section, queryOpt) ->
    //         // TODO: parse queryOpt into Collection filters / paging etc.
    //         match section with
    //         | Section.Collection ->
    //             let collection', cmd' =
    //                 match queryOpt with
    //                 | Some q -> Collection.update (Collection.InitFromQuery q) model.Collection
    //                 | None   -> Collection.update Collection.LoadProducts model.Collection

    //             { model with Section = section; Collection = collection' },
    //             cmd' |> Cmd.map CollectionMsg

    //         | _ ->
    //             { model with Section = section }, Cmd.none

    //     | CollectionMsg sub ->
    //         let m', cmd' = Collection.update sub model.Collection
    //         { model with Collection = m' }, cmd' |> Cmd.map CollectionMsg

    //     | TemplateBrowserMsg sub ->
    //         let m', cmd' = TemplateBrowser.update sub model.Templates
    //         { model with Templates = m' }, cmd' |> Cmd.map TemplateBrowserMsg

    //     | CartMsg sub ->
    //         let cart' = Cart.update sub model.Cart
    //         { model with Cart = cart' }, Cmd.none

    //     | CheckoutMsg sub ->
    //         let checkout' = Checkout.update sub model.Checkout
    //         { model with Checkout = checkout' }, Cmd.none
