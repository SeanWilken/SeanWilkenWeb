


namespace Client.Components.Shop.Collection

open Elmish
open Feliz
open Shared
open Client.Domain.Store.Collection
open Shared.SharedShopV2.PrintfulCatalog

module State =

    // type Model = {
    //     Filters    : Filters
    //     Paging     : PrintfulCommon.PagingInfoDTO
    //     SearchTerm : string option
    //     Products   : CatalogProduct list
    //     TotalCount : int
    //     IsLoading  : bool
    //     Error      : string option
    // }


    let initModel : Model = {
        Filters    = defaultFilters
        Paging     = PrintfulCommon.emptyPaging
        SearchTerm = None
        Products   = []
        TotalCount = 0
        IsLoading  = false
        Error      = None
    }

    // (same Query.parse / applyQueryToModel helpers as before…)

    let private loadProductsCmd (model: Model) : Cmd<Msg> =
        let q =
            Shared.Api.Printful.CatalogProductRequest.toApiQuery
                model.Paging
                model.Filters

        Cmd.OfAsync.either
            (fun qp -> Client.Api.productsApi.getProducts qp)
            q
            ProductsLoaded
            (fun ex -> LoadFailed ex.Message)

    
    let private applyQueryToModel (rawQuery: string) (model: Model) : Model =
        let q = SharedViewModule.Query.parse rawQuery

        let get key =
            Map.tryFind key q

        let getCsv key =
            get key
            |> Option.map (fun v -> v.Split(',', System.StringSplitOptions.RemoveEmptyEntries) |> Array.toList)
            |> Option.defaultValue []

        let getInt key defaultValue =
            match get key with
            | Some v ->
                match System.Int32.TryParse v with
                | true, n -> n
                | _       -> defaultValue
            | None -> defaultValue

        let categories =
            getCsv "categories"
            |> List.choose (fun s ->
                match System.Int32.TryParse s with
                | true, v -> Some v
                | _ -> None
            )

        let colors = getCsv "colors"

        let sortType =
            get "sort_type"

        let sortDir =
            get "sort_dir"

        let searchTerm = get "search"

        let offset = getInt "offset" model.Paging.offset
        let limit  = getInt "limit"  model.Paging.limit

        let filters : Filters =
            { model.Filters with
                Categories       = categories
                Colors           = colors
                SortType         = sortType
                SortDirection    = sortDir
            }

        {
            model with
                Filters    = filters
                Paging     = { model.Paging with offset = offset; limit = limit }
                SearchTerm = searchTerm
        }


    let init (rawQuery: string option) : Model * Cmd<Msg> =
        let m0 =
            match rawQuery with
            | Some q -> applyQueryToModel q initModel
            | None   -> initModel

        let m1 = { m0 with IsLoading = true }
        m1, loadProductsCmd m1

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | InitFromQuery rawQuery ->
            let m1 = applyQueryToModel rawQuery model
            let m2 = { m1 with IsLoading = true }
            m2, loadProductsCmd m2

        | LoadProducts ->
            let m1 = { model with IsLoading = true; Error = None }
            m1, loadProductsCmd m1

        | ProductsLoaded response ->
            { model with
                Products   = response.products
                TotalCount = response.paging.total
                Paging     = response.paging
                IsLoading  = false
                Error      = None
            }, Cmd.none

        | LoadFailed err ->
            { model with IsLoading = false; Error = Some err }, Cmd.none

        | LoadMore ->
            let newPaging =
                { model.Paging with offset = model.Paging.offset + model.Paging.limit }
            let m1 = { model with Paging = newPaging }
            m1, Cmd.ofMsg LoadProducts

        | FiltersChanged filters ->
            let m1 =
                { model with
                    Filters   = filters
                    Paging    = { model.Paging with offset = 0 }
                    IsLoading = true
                }
            m1, loadProductsCmd m1

        | SearchChanged term ->
            let m1 = { model with SearchTerm = Some term; Paging = { model.Paging with offset = 0 } }
            m1, Cmd.ofMsg LoadProducts

        | SortChanged (sortType, sortDir) ->
            let filters =
                { model.Filters with
                    SortType      = Some sortType
                    SortDirection = Some sortDir
                }
            let m1 =
                { model with
                    Filters = filters
                    Paging  = { model.Paging with offset = 0 }
                }
            m1, Cmd.ofMsg LoadProducts

        | ViewProduct p ->
            // bubble up to parent (router) to navigate to product detail
            model, Cmd.none

        | FeaturedClick _pOpt ->
            model, Cmd.none

        | ApplyFilterPreset _ ->
            model, Cmd.none

        | SaveFilterPreset _ ->
            model, Cmd.none


// module State =
//     open Elmish
//     open Shared.PrintfulCommon
//     open Shared.SharedShopV2Domain.CatalogProductResponse

//     type Model = {
//         Filters    : Filters
//         Paging     : PagingInfoDTO
//         SearchTerm : string option
//         Products   : CatalogProduct list
//         TotalCount : int
//         IsLoading  : bool
//         Error      : string option
//     }

//     type Msg =
//         | InitFromQuery of string
//         | LoadProducts
//         | ProductsLoaded of CatalogProductsResponse
//         | LoadFailed of string
//         | ViewProduct of CatalogProduct
//         | FeaturedClick of CatalogProduct option
//         | LoadMore
//         | FiltersChanged of Filters
//         | SearchChanged of string
//         | SortChanged of sortType: string * sortDir: string
//         | ApplyFilterPreset of string
//         | SaveFilterPreset of string

//     let initModel : Model = {
//         Filters    = defaultFilters
//         Paging     = emptyPaging
//         SearchTerm = None
//         Products   = []
//         TotalCount = 0
//         IsLoading  = false
//         Error      = None
//     }

    // (same Query.parse / applyQueryToModel helpers as before…)

    
    // let private loadProductsCmd (model: Model) : Cmd<Msg> =
    //     let q =
    //         Shared.Api.Printful.CatalogProductRequest.toApiQuery
    //             model.Paging
    //             model.Filters

    //     Cmd.OfAsync.either
    //         (fun qp -> Client.Api.productsApi.getProducts qp)
    //         q
    //         ProductsLoaded
    //         (fun ex -> LoadFailed ex.Message)

    // let init (rawQuery: string option) : Model * Cmd<Msg> =
    //     let m0 =
    //         match rawQuery with
    //         | Some q -> applyQueryToModel q initModel
    //         | None   -> initModel

    //     let m1 = { m0 with IsLoading = true }
    //     m1, loadProductsCmd m1

    // let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    //     match msg with
    //     | InitFromQuery rawQuery ->
    //         let m1 = applyQueryToModel rawQuery model
    //         let m2 = { m1 with IsLoading = true }
    //         m2, loadProductsCmd m2

    //     | LoadProducts ->
    //         let m1 = { model with IsLoading = true; Error = None }
    //         m1, loadProductsCmd m1

    //     | ProductsLoaded response ->
    //         { model with
    //             Products   = response.products
    //             TotalCount = response.paging.total
    //             Paging     = response.paging
    //             IsLoading  = false
    //             Error      = None
    //         }, Cmd.none

    //     | LoadFailed err ->
    //         { model with IsLoading = false; Error = Some err }, Cmd.none

    //     | LoadMore ->
    //         let newPaging =
    //             { model.Paging with offset = model.Paging.offset + model.Paging.limit }
    //         let m1 = { model with Paging = newPaging }
    //         m1, Cmd.ofMsg LoadProducts

    //     | FiltersChanged filters ->
    //         let m1 =
    //             { model with
    //                 Filters   = filters
    //                 Paging    = { model.Paging with offset = 0 }
    //                 IsLoading = true
    //             }
    //         m1, loadProductsCmd m1

    //     | SearchChanged term ->
    //         let m1 = { model with SearchTerm = Some term; Paging = { model.Paging with offset = 0 } }
    //         m1, Cmd.ofMsg LoadProducts

    //     | SortChanged (sortType, sortDir) ->
    //         let filters =
    //             { model.Filters with
    //                 SortType      = Some sortType
    //                 SortDirection = Some sortDir
    //             }
    //         let m1 =
    //             { model with
    //                 Filters = filters
    //                 Paging  = { model.Paging with offset = 0 }
    //             }
    //         m1, Cmd.ofMsg LoadProducts

    //     | ViewProduct p ->
    //         // bubble up to parent (router) to navigate to product detail
    //         model, Cmd.none

    //     | FeaturedClick _pOpt ->
    //         model, Cmd.none

    //     | ApplyFilterPreset _ ->
    //         model, Cmd.none

    //     | SaveFilterPreset _ ->
    //         model, Cmd.none

open State

module Collection =
    open Client.Components.Shop.Common.Ui

    type Props = {
        Products       : CatalogProduct list
        TotalCount     : int
        OnViewProduct  : CatalogProduct -> unit
        OnFeaturedClick: CatalogProduct option -> unit
        OnLoadMore     : unit -> unit
    }

    let private tagClass tag =
        match tag with
        | "New"        -> "badge badge-neutral"
        | "Bestseller" -> "badge badge-outline border-base-content bg-base-100 text-base-content"
        | "Trending"   -> "badge badge-error text-error-content"
        | _            -> "badge badge-secondary text-secondary-content"

    /// Derive a cosmetic “tag” from Printful data (just for UI flair)
    let private tagForProduct (p: CatalogProduct) : string option =
        if p.isDiscontinued then Some "Last Chance"
        elif p.variantCount > 20 then Some "Bestseller"
        else None

    let view (props: Props) =
        Section.container [
            Html.div [
                prop.className "py-10 md:py-20 space-y-16"
                prop.children [

                    // Header + filters
                    Html.div [
                        prop.className "flex flex-col md:flex-row md:items-end md:justify-between gap-6 border-b border-base-300 pb-6 md:pb-8"
                        prop.children [
                            Html.div [
                                prop.className "space-y-1"
                                prop.children [
                                    Html.h2 [
                                        prop.className "text-3xl md:text-5xl font-light tracking-tight text-base-content"
                                        prop.text "New Arrivals"
                                    ]
                                    Html.p [
                                        prop.className "text-sm text-base-content/60"
                                        prop.text $"{props.TotalCount} Products"
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.className "flex flex-wrap gap-3"
                                prop.children [
                                    Html.select [
                                        prop.className "select select-bordered uppercase text-xs tracking-[0.2em] rounded-none"
                                        prop.children [
                                            Html.option "All Categories"
                                            Html.option "Basics"
                                            Html.option "Outerwear"
                                            Html.option "Streetwear"
                                        ]
                                    ]
                                    Html.select [
                                        prop.className "select select-bordered uppercase text-xs tracking-[0.2em] rounded-none"
                                        prop.children [
                                            Html.option "Sort: Featured"
                                            Html.option "Price: Low → High"
                                            Html.option "Price: High → Low"
                                            Html.option "Newest"
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Featured block (head of products)
                    let featured =
                        props.Products
                        |> List.tryHead

                    match featured with
                    | Some f ->
                        Html.div [
                            prop.className "group cursor-pointer"
                            prop.onClick (fun _ -> props.OnFeaturedClick (Some f))
                            prop.children [
                                Html.div [
                                    prop.className "grid grid-cols-1 lg:grid-cols-2 bg-base-200 overflow-hidden"
                                    prop.children [

                                        // “Image” side – using id as big numeral
                                        Html.div [
                                            prop.className "relative aspect-square overflow-hidden"
                                            prop.children [
                                                Html.div [
                                                    prop.className "absolute inset-0 bg-gradient-to-br from-base-300 to-base-100 flex items-center justify-center"
                                                    prop.children [
                                                        Html.div [
                                                            prop.className "text-[7rem] md:text-[9rem] font-light text-base-content/20 transform group-hover:scale-110 transition-transform duration-700"
                                                            prop.text (string f.id)
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "absolute inset-0 bg-neutral/0 group-hover:bg-neutral/10 transition-colors duration-300 flex items-center justify-center opacity-0 group-hover:opacity-100"
                                                    prop.children [
                                                        Html.button [
                                                            prop.className (Btn.primary [ "rounded-none px-10 py-3 shadow-lg" ])
                                                            prop.text "View Product"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // Details
                                        Html.div [
                                            prop.className "p-10 md:p-16 flex flex-col justify-center space-y-6 bg-base-100"
                                            prop.children [
                                                Html.span [
                                                    prop.className "badge badge-neutral rounded-none uppercase tracking-[0.2em] text-[0.6rem]"
                                                    prop.text "Featured"
                                                ]
                                                Html.h3 [
                                                    prop.className "text-3xl md:text-4xl font-light tracking-tight text-base-content"
                                                    prop.text f.name
                                                ]
                                                Html.p [
                                                    prop.className "text-base md:text-lg text-base-content/70 leading-relaxed"
                                                    prop.text (
                                                        f.description
                                                        |> Option.defaultValue "Premium quality garment with multiple variants and colorways."
                                                    )
                                                ]
                                                Html.div [
                                                    prop.className "flex items-center gap-8 pt-4"
                                                    prop.children [
                                                        Html.div [
                                                            prop.className "space-y-1"
                                                            prop.children [
                                                                // TODO: wire actual pricing when you have it
                                                                Html.p [
                                                                    prop.className "text-2xl md:text-3xl font-light"
                                                                    prop.text "From $25"
                                                                ]
                                                                Html.p [
                                                                    prop.className "text-xs text-base-content/60"
                                                                    prop.text $"{f.colors.Length} colors available"
                                                                ]
                                                            ]
                                                        ]
                                                        Html.button [
                                                            prop.className "btn btn-ghost no-animation px-0 gap-2 uppercase text-xs tracking-[0.25em]"
                                                            prop.text "Shop now"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    | None ->
                        Html.none

                    // Product grid
                    Html.div [
                        prop.className "grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-x-8 gap-y-12"
                        prop.children [
                            for p in props.Products do
                                Html.div [
                                    prop.key p.id
                                    prop.className "group cursor-pointer space-y-3"
                                    prop.onClick (fun _ -> props.OnViewProduct p)
                                    prop.children [

                                        // Image card
                                        Html.div [
                                            prop.className "relative aspect-[3/4] bg-base-200 overflow-hidden rounded-lg"
                                            prop.children [

                                                // Tag (derived)
                                                match tagForProduct p with
                                                | Some tag ->
                                                    Html.div [
                                                        prop.className "absolute top-3 left-3 z-10"
                                                        prop.children [
                                                            Html.span [
                                                                prop.className (tagClass tag)
                                                                prop.text tag
                                                            ]
                                                        ]
                                                    ]
                                                | None -> Html.none

                                                // Large numeral (id)
                                                Html.div [
                                                    prop.className "absolute inset-0 flex items-center justify-center text-7xl font-light text-base-content/20 transform transition-all duration-700 group-hover:scale-110 group-hover:opacity-50"
                                                    prop.text (string p.id)
                                                ]

                                                // Quick view CTA
                                                Html.div [
                                                    prop.className "absolute inset-x-0 bottom-0 pb-6 flex justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300"
                                                    prop.children [
                                                        Html.button [
                                                            prop.className "btn btn-primary btn-sm rounded-none shadow-lg"
                                                            prop.text "Quick View"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // Text
                                        Html.div [
                                            prop.className "space-y-1"
                                            prop.children [
                                                Html.div [
                                                    prop.className "flex items-start justify-between gap-2"
                                                    prop.children [
                                                        Html.div [
                                                            prop.className "flex-1 min-w-0"
                                                            prop.children [
                                                                Html.p [
                                                                    prop.className "text-[0.65rem] uppercase tracking-[0.25em] text-base-content/50 mb-1"
                                                                    prop.text (
                                                                        p.brand
                                                                        |> Option.orElse p.model
                                                                        |> Option.defaultValue "Apparel"
                                                                    )
                                                                ]
                                                                Html.h3 [
                                                                    prop.className "font-medium text-base md:text-lg leading-tight truncate"
                                                                    prop.text p.name
                                                                ]
                                                            ]
                                                        ]
                                                        Html.p [
                                                            prop.className "text-lg font-light flex-shrink-0"
                                                            // placeholder until you wire price
                                                            prop.text "From $25"
                                                        ]
                                                    ]
                                                ]
                                                Html.p [
                                                    prop.className "text-xs text-base-content/60"
                                                    prop.text $"{p.colors.Length} colors"
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                        ]
                    ]

                    // Load more
                    Html.div [
                        prop.className "text-center pt-6"
                        prop.children [
                            Html.button [
                                prop.className (Btn.outline [ "px-10 py-3 rounded-none" ])
                                prop.text "Load More Products"
                                prop.onClick (fun _ -> props.OnLoadMore())
                            ]
                        ]
                    ]
                ]
            ]
        ]

// Client.Domain.SharedShopV2.Collection.Msg.FeaturedClick
    let collectionView (model: Model) (dispatch: Client.Domain.Store.ShopMsg -> unit) : ReactElement =
        view {
            Products        = model.Products
            TotalCount      = model.TotalCount
            OnViewProduct   = (fun p -> dispatch (Client.Domain.Store.ShopMsg.ShopCollectionMsg (ViewProduct p)))
            OnFeaturedClick = (fun pOpt -> 
                FeaturedClick pOpt
                |> Client.Domain.Store.ShopMsg.ShopCollectionMsg 
                |> dispatch 
            )
            OnLoadMore      = (fun _ -> 
                LoadMore
                |> Client.Domain.Store.ShopMsg.ShopCollectionMsg 
                |> dispatch
            )
        }


// namespace Client.Components.Shop.Collection

// open Feliz
// open Client.Components.Shop.Common.Types
// open Client.Components.Shop.Common.Ui
// open Shared.SharedShopV2.PrintfulCatalog
// open Shared.PrintfulCommon
// open Shared.SharedShopV2Domain.CatalogProductResponse
// open Elmish

// module Collection =
//     open Browser


//     type Props = {
//         Products       : MockProduct list
//         TotalCount     : int
//         OnViewProduct  : MockProduct -> unit
//         OnFeaturedClick: MockProduct option -> unit
//         OnLoadMore     : unit -> unit
//     }

//     let private tagClass tag =
//         match tag with
//         | "New"        -> "badge badge-neutral"
//         | "Bestseller" -> "badge badge-outline border-base-content bg-base-100 text-base-content"
//         | "Trending"   -> "badge badge-error text-error-content"
//         | _            -> "badge badge-secondary text-secondary-content"

//     /// Everything the Collection page cares about
//     type Model = {
//         Filters    : Filters
//         Paging     : PagingInfoDTO
//         SearchTerm : string option
//         Products   : MockProduct list
//         TotalCount : int
//         IsLoading  : bool
//         Error      : string option
//     }

//     /// Messages local to the Collection page
//     type Msg =
//         | InitFromQuery of string  // raw "?section=collection&..."
//         | LoadProducts
//         | ProductsLoaded of CatalogProductsResponse
//         | LoadFailed of string
//         | ViewProduct of MockProduct
//         | FeaturedClick of MockProduct option
//         | LoadMore
//         | FiltersChanged of Filters
//         | SearchChanged of string
//         | SortChanged of sortType: string * sortDir: string
//         | ApplyFilterPreset of name:string
//         | SaveFilterPreset of name:string

//     let initModel : Model = {
//         Filters    = defaultFilters
//         Paging     = emptyPaging
//         SearchTerm = None
//         Products   = []
//         TotalCount = 0
//         IsLoading  = false
//         Error      = None
//     }

//     let private applyQueryToModel (rawQuery: string) (model: Model) : Model =
//         let q = SharedViewModule.Query.parse rawQuery

//         let get key =
//             Map.tryFind key q

//         let getCsv key =
//             get key
//             |> Option.map (fun v -> v.Split(',', System.StringSplitOptions.RemoveEmptyEntries) |> Array.toList)
//             |> Option.defaultValue []

//         let getInt key defaultValue =
//             match get key with
//             | Some v ->
//                 match System.Int32.TryParse v with
//                 | true, n -> n
//                 | _       -> defaultValue
//             | None -> defaultValue

//         let categories =
//             getCsv "categories"
//             |> List.choose (fun s ->
//                 match System.Int32.TryParse s with
//                 | true, v -> Some v
//                 | _ -> None
//             )

//         let colors = getCsv "colors"

//         let sortType =
//             get "sort_type"

//         let sortDir =
//             get "sort_dir"

//         let searchTerm = get "search"

//         let offset = getInt "offset" model.Paging.offset
//         let limit  = getInt "limit"  model.Paging.limit

//         let filters : Filters =
//             { model.Filters with
//                 Categories       = categories
//                 Colors           = colors
//                 SortType         = sortType
//                 SortDirection    = sortDir
//             }

//         {
//             model with
//                 Filters    = filters
//                 Paging     = { model.Paging with offset = offset; limit = limit }
//                 SearchTerm = searchTerm
//         }

//     let private loadProductsCmd (model: Model) : Cmd<Msg> =
//         let q =
//             Shared.Api.Printful.CatalogProductRequest.toApiQuery
//                 model.Paging
//                 model.Filters

//         Cmd.OfAsync.either
//             (fun qp -> Client.Api.productsApi.getProducts qp)
//             q
//             ProductsLoaded
//             (fun ex -> LoadFailed ex.Message)

//     let private mapToMock (p: CatalogProduct) : MockProduct =
//         {
//             Id       = p.id
//             Name     = p.name
//             // Printful catalog doesn’t give price here; placeholder or later enrichment:
//             Price    = 0m
//             Category = p.brand |> Option.defaultValue "Collection"
//             Colors   = p.colors.Length
//             Tag      = None
//         }

//     let init (rawQuery: string option) : Model * Cmd<Msg> =
//         let m0 =
//             match rawQuery with
//             | Some q -> applyQueryToModel q initModel
//             | None   -> initModel

//         let m1 = { m0 with IsLoading = true }
//         m1, loadProductsCmd m1

//     let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
//         match msg with
//         | InitFromQuery rawQuery ->
//             let m1 = applyQueryToModel rawQuery model
//             let m2 = { m1 with IsLoading = true }
//             m2, loadProductsCmd m2

//         | LoadProducts ->
//             let m1 = { model with IsLoading = true; Error = None }
//             m1, loadProductsCmd m1

//         | ProductsLoaded response ->
//             let products =
//                 response.products
//                 |> List.map mapToMock

//             { model with
//                 Products   = products
//                 TotalCount = response.paging.total
//                 Paging     = response.paging
//                 IsLoading  = false
//                 Error      = None
//             }, Cmd.none

//         | LoadFailed err ->
//             { model with
//                 IsLoading = false
//                 Error     = Some err
//             }, Cmd.none

//         | LoadMore ->
//             // bump offset, then reload
//             let newPaging =
//                 { model.Paging with
//                     offset = model.Paging.offset + model.Paging.limit
//                 }

//             let m1 = { model with Paging = newPaging }
//             m1, Cmd.ofMsg LoadProducts

//         | FiltersChanged filters ->
//             let m1 =
//                 { model with
//                     Filters   = filters
//                     Paging    = { model.Paging with offset = 0 }
//                     IsLoading = true
//                 }
//             // here you’d also encode filters into URL
//             m1, loadProductsCmd m1

//         | SearchChanged term ->
//             let m1 = { model with SearchTerm = Some term; Paging = { model.Paging with offset = 0 } }
//             m1, Cmd.ofMsg LoadProducts

//         | SortChanged (sortType, sortDir) ->
//             let filters =
//                 { model.Filters with
//                     SortType      = Some sortType
//                     SortDirection = Some sortDir
//                 }
//             let m1 =
//                 { model with
//                     Filters = filters
//                     Paging  = { model.Paging with offset = 0 }
//                 }
//             m1, Cmd.ofMsg LoadProducts

//         | ViewProduct p ->
//             // Let the parent handle navigation; we just bubble this up from view.
//             model, Cmd.none

//         | FeaturedClick _pOpt ->
//             // Same: parent decides what to do (navigate to product detail, open quick view, etc.)
//             model, Cmd.none

//         | ApplyFilterPreset name ->
//             // TODO: load preset from localStorage and apply to model.Filters; then LoadProducts
//             model, Cmd.none

//         | SaveFilterPreset name ->
//             // TODO: serialize model.Filters into localStorage under `name`
//             model, Cmd.none


//     // pseudo:
//     let savePreset name filters =
//         let json = Encode.Auto.toString(0, filters)
//         localStorage.setItem($"shop-filters-{name}", json)

//     let loadPreset name =
//         match localStorage.getItem($"shop-filters-{name}") with
//         | null -> None
//         | s ->
//             Decode.Auto.fromString<PrintfulCatalog.Filters>(s)
//             |> Result.toOption


//     let view (props: Props) =
//         Section.container [
//             Html.div [
//                 prop.className "py-10 md:py-20 space-y-16"
//                 prop.children [

//                     // Header + filters
//                     Html.div [
//                         prop.className "flex flex-col md:flex-row md:items-end md:justify-between gap-6 border-b border-base-300 pb-6 md:pb-8"
//                         prop.children [
//                             Html.div [
//                                 prop.className "space-y-1"
//                                 prop.children [
//                                     Html.h2 [
//                                         prop.className "text-3xl md:text-5xl font-light tracking-tight text-base-content"
//                                         prop.text "New Arrivals"
//                                     ]
//                                     Html.p [
//                                         prop.className "text-sm text-base-content/60"
//                                         prop.text $"{props.TotalCount} Products"
//                                     ]
//                                 ]
//                             ]

//                             Html.div [
//                                 prop.className "flex flex-wrap gap-3"
//                                 prop.children [
//                                     Html.select [
//                                         prop.className "select select-bordered uppercase text-xs tracking-[0.2em] rounded-none"
//                                         prop.children [
//                                             Html.option "All Categories"
//                                             Html.option "Basics"
//                                             Html.option "Outerwear"
//                                             Html.option "Streetwear"
//                                         ]
//                                     ]
//                                     Html.select [
//                                         prop.className "select select-bordered uppercase text-xs tracking-[0.2em] rounded-none"
//                                         prop.children [
//                                             Html.option "Sort: Featured"
//                                             Html.option "Price: Low → High"
//                                             Html.option "Price: High → Low"
//                                             Html.option "Newest"
//                                         ]
//                                     ]
//                                 ]
//                             ]
//                         ]
//                     ]

//                     // Featured block (use head option)
//                     let featured =
//                         props.Products
//                         |> List.tryHead

//                     match featured with
//                     | Some f ->
//                         Html.div [
//                             prop.className "group cursor-pointer"
//                             prop.onClick (fun _ -> props.OnFeaturedClick (Some f))
//                             prop.children [
//                                 Html.div [
//                                     prop.className "grid grid-cols-1 lg:grid-cols-2 bg-base-200 overflow-hidden"
//                                     prop.children [

//                                         // "Image"
//                                         Html.div [
//                                             prop.className "relative aspect-square overflow-hidden"
//                                             prop.children [
//                                                 Html.div [
//                                                     prop.className "absolute inset-0 bg-gradient-to-br from-base-300 to-base-100 flex items-center justify-center"
//                                                     prop.children [
//                                                         Html.div [
//                                                             prop.className "text-[7rem] md:text-[9rem] font-light text-base-content/20 transform group-hover:scale-110 transition-transform duration-700"
//                                                             prop.text (string f.Id)
//                                                         ]
//                                                     ]
//                                                 ]
//                                                 Html.div [
//                                                     prop.className "absolute inset-0 bg-neutral/0 group-hover:bg-neutral/10 transition-colors duration-300 flex items-center justify-center opacity-0 group-hover:opacity-100"
//                                                     prop.children [
//                                                         Html.button [
//                                                             prop.className (Btn.primary [ "rounded-none px-10 py-3 shadow-lg" ])
//                                                             prop.text "View Product"
//                                                         ]
//                                                     ]
//                                                 ]
//                                             ]
//                                         ]

//                                         // Details
//                                         Html.div [
//                                             prop.className "p-10 md:p-16 flex flex-col justify-center space-y-6 bg-base-100"
//                                             prop.children [
//                                                 Html.span [
//                                                     prop.className "badge badge-neutral rounded-none uppercase tracking-[0.2em] text-[0.6rem]"
//                                                     prop.text "Featured"
//                                                 ]
//                                                 Html.h3 [
//                                                     prop.className "text-3xl md:text-4xl font-light tracking-tight text-base-content"
//                                                     prop.text f.Name
//                                                 ]
//                                                 Html.p [
//                                                     prop.className "text-base md:text-lg text-base-content/70 leading-relaxed"
//                                                     prop.text "Premium heavyweight cotton blend with an oversized fit. The perfect balance of comfort and style for everyday wear."
//                                                 ]
//                                                 Html.div [
//                                                     prop.className "flex items-center gap-8 pt-4"
//                                                     prop.children [
//                                                         Html.div [
//                                                             prop.className "space-y-1"
//                                                             prop.children [
//                                                                 Html.p [
//                                                                     prop.className "text-2xl md:text-3xl font-light"
//                                                                     prop.text $"${f.Price}"
//                                                                 ]
//                                                                 Html.p [
//                                                                     prop.className "text-xs text-base-content/60"
//                                                                     prop.text $"{f.Colors} colors available"
//                                                                 ]
//                                                             ]
//                                                         ]
//                                                         Html.button [
//                                                             prop.className "btn btn-ghost no-animation px-0 gap-2 uppercase text-xs tracking-[0.25em]"
//                                                             prop.text "Shop now"
//                                                         ]
//                                                     ]
//                                                 ]
//                                             ]
//                                         ]
//                                     ]
//                                 ]
//                             ]
//                         ]
//                     | None ->
//                         Html.none

//                     // Product grid
//                     Html.div [
//                         prop.className "grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-x-8 gap-y-12"
//                         prop.children [
//                             for p in props.Products do
//                                 Html.div [
//                                     prop.key p.Id
//                                     prop.className "group cursor-pointer space-y-3"
//                                     prop.onClick (fun _ -> props.OnViewProduct p)
//                                     prop.children [

//                                         // Image card
//                                         Html.div [
//                                             prop.className "relative aspect-[3/4] bg-base-200 overflow-hidden rounded-lg"
//                                             prop.children [
//                                                 match p.Tag with
//                                                 | Some tag ->
//                                                     Html.div [
//                                                         prop.className "absolute top-3 left-3 z-10"
//                                                         prop.children [
//                                                             Html.span [
//                                                                 prop.className (tagClass tag)
//                                                                 prop.text tag
//                                                             ]
//                                                         ]
//                                                     ]
//                                                 | None -> Html.none

//                                                 Html.div [
//                                                     prop.className "absolute inset-0 flex items-center justify-center text-7xl font-light text-base-content/20 transform transition-all duration-700 group-hover:scale-110 group-hover:opacity-50"
//                                                     prop.text (string p.Id)
//                                                 ]

//                                                 Html.div [
//                                                     prop.className "absolute inset-x-0 bottom-0 pb-6 flex justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300"
//                                                     prop.children [
//                                                         Html.button [
//                                                             prop.className "btn btn-primary btn-sm rounded-none shadow-lg"
//                                                             prop.text "Quick View"
//                                                         ]
//                                                     ]
//                                                 ]
//                                             ]
//                                         ]

//                                         // Text
//                                         Html.div [
//                                             prop.className "space-y-1"
//                                             prop.children [
//                                                 Html.div [
//                                                     prop.className "flex items-start justify-between gap-2"
//                                                     prop.children [
//                                                         Html.div [
//                                                             prop.className "flex-1 min-w-0"
//                                                             prop.children [
//                                                                 Html.p [
//                                                                     prop.className "text-[0.65rem] uppercase tracking-[0.25em] text-base-content/50 mb-1"
//                                                                     prop.text p.Category
//                                                                 ]
//                                                                 Html.h3 [
//                                                                     prop.className "font-medium text-base md:text-lg leading-tight truncate"
//                                                                     prop.text p.Name
//                                                                 ]
//                                                             ]
//                                                         ]
//                                                         Html.p [
//                                                             prop.className "text-lg font-light flex-shrink-0"
//                                                             prop.text $"${p.Price}"
//                                                         ]
//                                                     ]
//                                                 ]
//                                                 Html.p [
//                                                     prop.className "text-xs text-base-content/60"
//                                                     prop.text $"{p.Colors} colors"
//                                                 ]
//                                             ]
//                                         ]
//                                     ]
//                                 ]
//                         ]
//                     ]

//                     // Load more
//                     Html.div [
//                         prop.className "text-center pt-6"
//                         prop.children [
//                             Html.button [
//                                 prop.className (Btn.outline [ "px-10 py-3 rounded-none" ])
//                                 prop.text "Load More Products"
//                                 prop.onClick (fun _ -> props.OnLoadMore())
//                             ]
//                         ]
//                     ]
//                 ]
//             ]
//         ]