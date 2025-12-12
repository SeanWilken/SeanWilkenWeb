


namespace Client.Components.Shop.Collection

open Elmish
open Feliz
open Shared
open Client.Domain.Store.Collection
open Shared.SharedShopV2.PrintfulCatalog

module State =

    let getAllProductTemplates (request: Api.Printful.CatalogProductRequest.CatalogProductsQuery) : Cmd<Msg> =
        Cmd.OfAsync.either
            ( fun x -> Client.Api.productsApi.getProductTemplates x )
            request
            GotProducts
            FailedProducts

    let initModel : Model = {
        Filters    = defaultFilters
        Paging     = PrintfulCommon.emptyPaging
        SearchTerm = None
        Products   = []
        TotalCount = 0
        IsLoading  = false
        Error      = None
    }

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

    let getProductsForModel (model: Model) : Cmd<Msg> =
        Api.Printful.CatalogProductRequest.toApiQuery
            model.Paging
            model.Filters
        |> getAllProductTemplates

    let canLoadMore (model: Model) =
        if List.isEmpty model.Products && model.TotalCount = 0 then
            true   // initial load allowed
        else
            model.Paging.offset + model.Paging.limit < model.TotalCount

    let init (rawQuery: string option) : Model * Cmd<Msg> =
        let m0 =
            match rawQuery with
            | Some q -> applyQueryToModel q initModel
            | None   -> initModel

        let m1 = { m0 with IsLoading = true }

        m1,
        Api.Printful.CatalogProductRequest.toApiQuery
            m0.Paging
            m0.Filters
        |> getAllProductTemplates

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with

        | GetProducts ->
            let m1 = { model with IsLoading = true; Error = None }
            m1, getProductsForModel m1

        | GotProducts response ->
            let isAppend = model.Paging.offset > 0

            let products' =
                if isAppend then
                    model.Products @ response.templateItems
                else
                    response.templateItems

            { model with
                Products   = products'
                Paging     = response.paging
                TotalCount = response.paging.total
                IsLoading  = false
                Error      = None
            }, Cmd.none

        | FailedProducts ex ->
            { model with
                IsLoading = false
                Error     = Some ex.Message
            }, Cmd.none

        | InitFromQuery rawQuery ->
            let m1 = applyQueryToModel rawQuery model
            let m2 = { m1 with IsLoading = true }
            m2, getProductsForModel m2

        | LoadMore ->
            // Only bump paging if we *can* load more
            if canLoadMore model then
                let newPaging =
                    { model.Paging with
                        offset = model.Paging.offset + model.Paging.limit }

                let m1 = { model with Paging = newPaging; IsLoading = true }
                m1, Cmd.ofMsg GetProducts
            else
                model, Cmd.none

        | FiltersChanged filters ->
            let m1 =
                { model with
                    Filters   = filters
                    Paging    = { model.Paging with offset = 0 }
                    IsLoading = true
                }
            m1, getProductsForModel m1

        | SearchChanged term ->
            let m1 =
                { model with
                    SearchTerm = Some term
                    Paging     = { model.Paging with offset = 0 }
                }
            m1, Cmd.ofMsg GetProducts

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
            m1, Cmd.ofMsg GetProducts

        | ViewProduct _p ->
            model, Cmd.none

        | FeaturedClick _ ->
            model, Cmd.none

        | ApplyFilterPreset _ ->
            model, Cmd.none

        | SaveFilterPreset _ ->
            model, Cmd.none

module Collection =
    open Client.Components.Shop.Common.Ui
    open Shared.SharedShopV2.ProductTemplate
    open Client.Components.Shop.Common.Ui

    type Props = {
        Products       : ProductTemplate list
        TotalCount     : int
        IsLoading      : bool
        CanLoadMore    : bool
        Paging: Shared.PrintfulCommon.PagingInfoDTO
        Filters: Filters
        SearchTerm: string option
        OnViewProduct  : ProductTemplate -> unit
        OnFeaturedClick: ProductTemplate option -> unit
        OnLoadMore     : unit -> unit
    }

    let private tagClass tag =
        match tag with
        | "New"        -> "badge badge-neutral"
        | "Bestseller" -> "badge badge-outline border-base-content bg-base-100 text-base-content"
        | "Trending"   -> "badge badge-error text-error-content"
        | _            -> "badge badge-secondary text-secondary-content"

    let view (props: Props) =
        Section.container [
            Html.div [
                prop.className "py-10 md:py-20 space-y-16"
                prop.children [
                    // Header + filters
                    // Html.div [
                    //     prop.className "flex flex-col md:flex-row md:items-end md:justify-between gap-6 border-b border-base-300 pb-6 md:pb-8"
                    //     prop.children [
                    //         Html.div [
                    //             prop.className "space-y-1"
                    //             prop.children [
                    //                 Html.h2 [
                    //                     prop.className "text-3xl md:text-5xl font-light tracking-tight text-base-content"
                    //                     prop.text "New Arrivals"
                    //                 ]
                    //                 Html.p [
                    //                     prop.className "text-sm text-base-content/60"
                    //                     prop.text $"{props.TotalCount} Products"
                    //                 ]
                    //             ]
                    //         ]

                    //         Html.div [
                    //             prop.className "flex flex-wrap gap-3"
                    //             prop.children [
                    //                 Html.select [
                    //                     prop.className "select select-bordered uppercase text-xs tracking-[0.2em] rounded-none"
                    //                     prop.children [
                    //                         Html.option "All Categories"
                    //                         Html.option "Basics"
                    //                         Html.option "Outerwear"
                    //                         Html.option "Streetwear"
                    //                     ]
                    //                 ]
                    //                 Html.select [
                    //                     prop.className "select select-bordered uppercase text-xs tracking-[0.2em] rounded-none"
                    //                     prop.children [
                    //                         Html.option "Sort: Featured"
                    //                         Html.option "Price: Low → High"
                    //                         Html.option "Price: High → Low"
                    //                         Html.option "Newest"
                    //                     ]
                    //                 ]
                    //             ]
                    //         ]
                    //     ]
                    // ]
                    Client.Components.Shop.Common.Ui.CatalogHeader.CatalogHeader {
                        Title      = "New Arrivals"
                        Subtitle   = Some "Curated templates from the Xero Effort catalog."
                        TotalCount = props.TotalCount
                        Filters    = props.Filters
                        Paging     = props.Paging
                        SearchTerm = props.SearchTerm

                        OnSearchChanged = (fun term ->
                            // dispatch (SearchChanged term)
                            ()
                        )

                        OnSortChanged = (fun (sortType, sortDir) ->
                            // dispatch (SortChanged (sortType, sortDir))
                            ()
                        )

                        OnToggleNewOnly = (fun isNew ->
                            let newFilters = { props.Filters with OnlyNew = isNew }
                            // dispatch (FiltersChanged newFilters)
                            ()
                        )

                        OnPageChange = (fun newOffset ->
                            // You already have LoadMore, but this is more general:
                            let updated =
                                { props.Paging with offset = newOffset }
                            // dispatch (PagingChanged updated) // or a new Msg if you like
                            ()
                        )
                    }

                    // Featured block
                    let featured = props.Products |> List.tryHead

                    match featured with
                    | Some f ->
                        Html.div [
                            prop.className "group cursor-pointer"
                            prop.onClick (fun _ -> props.OnFeaturedClick (Some f))
                            prop.children [
                                Html.div [
                                    prop.className "grid grid-cols-1 lg:grid-cols-2 bg-base-200 overflow-hidden"
                                    prop.children [

                                        // Image side
                                        Html.div [
                                            prop.className "relative aspect-square overflow-hidden"
                                            prop.children [
                                                Html.img [
                                                    prop.src f.mockup_file_url
                                                    prop.alt f.title
                                                    prop.className "absolute inset-0 w-full h-full object-cover"
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

                                        // Details side
                                        Html.div [
                                            prop.className "p-10 md:p-16 flex flex-col justify-center space-y-6 bg-base-100"
                                            prop.children [
                                                Html.span [
                                                    prop.className "badge badge-neutral rounded-none uppercase tracking-[0.2em] text-[0.6rem]"
                                                    prop.text "Featured"
                                                ]
                                                Html.h3 [
                                                    prop.className "text-3xl md:text-4xl font-light tracking-tight text-base-content"
                                                    prop.text f.title
                                                ]
                                                Html.p [
                                                    prop.className "text-base md:text-lg text-base-content/70 leading-relaxed"
                                                    prop.text "Premium quality garment with multiple variants and colorways."
                                                ]
                                                Html.div [
                                                    prop.className "flex items-center gap-8 pt-4"
                                                    prop.children [
                                                        Html.div [
                                                            prop.className "space-y-1"
                                                            prop.children [
                                                                Html.p [
                                                                    prop.className "text-2xl md:text-3xl font-light"
                                                                    prop.text "From $25"
                                                                ]
                                                                Html.p [
                                                                    prop.className "text-xs text-base-content/60"
                                                                    prop.text $"{f.available_variant_ids.Length} variants available"
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
                                                Html.img [
                                                    prop.src p.mockup_file_url
                                                    prop.alt p.title
                                                    prop.className "absolute inset-0 w-full h-full object-cover transform transition-all duration-700 group-hover:scale-110 group-hover:opacity-80"
                                                ]

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
                                                                    prop.text "Apparel"
                                                                ]
                                                                Html.h3 [
                                                                    prop.className "font-medium text-base md:text-lg leading-tight truncate"
                                                                    prop.text p.title
                                                                ]
                                                            ]
                                                        ]
                                                        Html.p [
                                                            prop.className "text-lg font-light flex-shrink-0"
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

                    LoadMoreProductsButton.loadMoreProductsButton {
                        IsLoading = props.IsLoading
                        CanLoadMore = props.CanLoadMore
                        OnLoadMore = props.OnLoadMore
                    }
                ]
            ]
        ]

    [<ReactComponent>]
    let collectionView (model: Model) (dispatch: Client.Domain.Store.Collection.Msg -> unit) : ReactElement =
        view {
            Products       = model.Products
            TotalCount     = model.TotalCount
            IsLoading      = model.IsLoading
            Filters = model.Filters
            Paging = model.Paging
            SearchTerm = model.SearchTerm
            CanLoadMore    = State.canLoadMore model
            OnViewProduct  = (fun p -> dispatch (ViewProduct p))
            OnFeaturedClick = (fun pOpt ->
                FeaturedClick pOpt |> dispatch
            )
            OnLoadMore     = (fun _ -> LoadMore |> dispatch)
        }
