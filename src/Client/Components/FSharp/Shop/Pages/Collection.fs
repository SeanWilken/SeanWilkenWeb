


namespace Client.Components.Shop.Collection

open Elmish
open Feliz
open Shared
open Client.Domain.Store.Collection

module State =
    open Feliz.UseDeferred

    let getAllSyncProducts (request: Api.Printful.SyncProduct.GetSyncProductsRequest) : Cmd<Msg> =
        Cmd.OfAsync.either
            ( fun x -> Client.Api.productsApi.getSyncProducts x )
            request
            GotSyncProducts
            FailedSyncProducts

    let initModel : Model = {
        Filters    = defaultFilters
        Paging     = PrintfulCommon.emptyPaging
        SearchTerm = None
        Products   = Deferred.HasNotStartedYet
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

    let productsOrEmpty products =
        match products with
        | Deferred.HasNotStartedYet
        | Deferred.InProgress
        | Deferred.Failed _ -> []
        | Deferred.Resolved products -> products

    let canLoadMore (model: Model) =
        if model.Products = Deferred.InProgress
        then false
        elif List.isEmpty (productsOrEmpty model.Products) && model.TotalCount = 0 then
            true
        else
            model.Paging.offset + model.Paging.limit < model.TotalCount

    let syncProductRequestFromModel model : Api.Printful.SyncProduct.GetSyncProductsRequest =
        {
            limit  = None
            offset = None
        }
    

    let init (rawQuery: string option) : Model * Cmd<Msg> =
        let m0 =
            match rawQuery with
            | Some q -> applyQueryToModel q initModel
            | None   -> initModel

        let m1 = { m0 with IsLoading = true }

        m1,
        getAllSyncProducts (syncProductRequestFromModel m1)
            

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        // ??
        | ViewSyncProduct _ ->
            model, Cmd.none

        | GetSyncProducts ->

            let m1 = { model with IsLoading = true; Error = None }
            m1, getAllSyncProducts (syncProductRequestFromModel m1)

        | GotSyncProducts response ->

            response.items
            |> List.iter (fun p -> printfn $"Sync Product: {p.Id} - {p.Name}")
            let products' =
                productsOrEmpty model.Products @ response.items
                |> Deferred.Resolved

            { model with
                Products   = products'
                Paging     = response.paging
                TotalCount = response.paging.total
                IsLoading  = false
                Error      = None
            }, Cmd.none
            

        | FailedSyncProducts ex ->
            { model with
                IsLoading = false
                Error     = Some ex.Message
            }, Cmd.none

        | LoadMore ->
            // Only bump paging if we *can* load more
            if canLoadMore model then
                let newPaging =
                    { model.Paging with
                        offset = model.Paging.offset + model.Paging.limit }

                let m1 = { model with Paging = newPaging; IsLoading = true }
                m1,
                Cmd.ofMsg GetSyncProducts
            else
                model, Cmd.none

        | FiltersChanged filters ->
            let m1 =
                { model with
                    Filters   = filters
                    Paging    = { model.Paging with offset = 0 }
                    IsLoading = true
                }
            m1, Cmd.ofMsg GetSyncProducts

        | SearchChanged term ->
            let m1 =
                { model with
                    SearchTerm = Some term
                    Paging     = { model.Paging with offset = 0 }
                }
            m1, Cmd.ofMsg GetSyncProducts

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
            m1, Cmd.ofMsg GetSyncProducts

        | ApplyFilterPreset _ ->
            model, Cmd.none

        | SaveFilterPreset _ ->
            model, Cmd.none

module Collection =
    open Client.Components.Shop.Common.Ui
    open Feliz.UseDeferred
    open Shared.StoreProductViewer.SyncProduct

    type Props = {
        Products       : Deferred<SyncProductSummary list>
        TotalCount     : int
        IsLoading      : bool
        CanLoadMore    : bool
        Paging: PrintfulCommon.PagingInfoDTO
        Filters: Filters
        SearchTerm: string option
        OnViewProduct  : SyncProductSummary -> unit
        OnLoadMore     : unit -> unit
    }

    [<ReactComponent>]
    let CollectionView (props: Props) dispatch =
        Section.container [
            Html.div [
                prop.className "py-10 md:py-20 space-y-16"
                prop.children [
                    // CatalogHeader.CatalogHeader {
                    //     Title      = "New Arrivals"
                    //     Subtitle   = Some "Curated templates from the Xero Effort catalog."
                    //     TotalCount = props.TotalCount
                    //     Filters    = props.Filters
                    //     Paging     = props.Paging
                    //     SearchTerm = props.SearchTerm
                    //     DisableControls = true
                    //     OnSearchChanged = (fun term -> dispatch (SearchChanged term))
                    //     OnFiltersChanged = (fun newFilters -> dispatch (FiltersChanged newFilters))
                    //     OnPageChange = (fun _ -> () )
                    // }

                    // Featured block
                    let featured = 
                        props.Products
                        |> State.productsOrEmpty
                        |> List.tryHead

                    match featured with
                    | Some f ->
                        
                        Html.div [
                            prop.className "group cursor-pointer"
                            prop.onClick (fun _ -> props.OnViewProduct f)
                            prop.children [
                                Html.div [
                                    prop.className "grid grid-cols-1 lg:grid-cols-2 bg-base-200 overflow-hidden"
                                    prop.children [

                                        // Image side
                                        Html.div [
                                            prop.className "relative aspect-square overflow-hidden"
                                            prop.children [
                                                match f.ThumbnailUrl with
                                                | None ->
                                                    Html.div [
                                                        prop.className "absolute inset-0 w-full h-full flex items-center justify-center bg-base-300 text-base-content/50"
                                                        prop.text "No Image"
                                                    ]
                                                | Some tni ->
                                                    Html.img [
                                                        prop.src tni
                                                        prop.alt f.Name
                                                        prop.className "absolute inset-0 w-full h-full object-cover"
                                                    ]
                                            ]
                                        ]

                                        // Details side
                                        Html.div [
                                            prop.className "p-10 md:p-16 flex flex-col justify-center space-y-6 bg-base-100"
                                            prop.children [
                                                Html.h3 [
                                                    prop.className "text-3xl md:text-4xl font-light tracking-tight text-base-content cormorant-font"
                                                    prop.text f.Name
                                                ]
                                                Html.p [
                                                    prop.className "text-base md:text-lg text-base-content/70 leading-relaxed"
                                                    prop.text "Premium quality garment with multiple variants and colorways."
                                                ]
                                                Html.div [
                                                    prop.className "flex items-center gap-8 pt-4"
                                                    prop.children [
                                                        Html.div [
                                                            prop.className "space-y-1 flex gap-2 items-baseline"
                                                            prop.children [
                                                                Html.p [
                                                                    prop.className "text-2xl md:text-3xl font-light"
                                                                    prop.text $"{f.VariantCount} "
                                                                ]
                                                                Html.p [
                                                                    prop.className "text-md text-base-content/60"
                                                                    prop.text $" variants available"
                                                                ]
                                                            ]
                                                        ]
                                                        Html.div [
                                                            prop.className "inset-0 bg-neutral/0 group-hover:bg-neutral/10 transition-colors duration-300 flex items-center justify-center"
                                                            prop.children [
                                                                Html.button [
                                                                    prop.className (Btn.primary [ "rounded-none px-10 py-3 shadow-lg" ])
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
                            ]
                        ]
                    | None ->
                        Html.none

                    // Product grid
                    Html.div [
                        prop.className "grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-x-8 gap-y-12"
                        prop.children [
                            for p in State.productsOrEmpty props.Products do
                                Html.div [
                                    prop.key (string p.Id)
                                    prop.className "group cursor-pointer space-y-3"
                                    prop.onClick (fun _ -> props.OnViewProduct p)
                                    prop.children [

                                        // Image card
                                        Html.div [
                                            prop.className "relative aspect-[3/4] bg-base-200 overflow-hidden rounded-lg"
                                            prop.children [
                                                match p.ThumbnailUrl with
                                                | None ->
                                                    Html.div [
                                                        prop.className "absolute inset-0 w-full h-full flex items-center justify-center bg-base-300 text-base-content/50"
                                                        prop.text "No Image"
                                                    ]
                                                | Some tni ->
                                                    Html.img [
                                                        prop.src tni
                                                        prop.alt p.Name
                                                        prop.className "absolute inset-0 w-full h-full object-cover transform transition-all duration-700 group-hover:scale-110 group-hover:opacity-80"
                                                    ]
                                            ]
                                        ]

                                        // Text
                                        Html.div [
                                            prop.className "space-y-1"
                                            prop.children [
                                                Html.div [
                                                    prop.className "flex items-start justify-between gap-2 align-middle"
                                                    prop.children [
                                                        Html.div [
                                                            prop.className "flex-1 min-w-0"
                                                            prop.children [
                                                                Html.p [
                                                                    prop.className "text-[0.65rem] uppercase tracking-[0.25em] text-base-content/50 mb-1"
                                                                    prop.text "Apparel"
                                                                ]
                                                                Html.h3 [
                                                                    prop.className "font-medium text-base md:text-lg leading-tight truncate cormorant-font"
                                                                    prop.text p.Name
                                                                ]
                                                            ]
                                                        ]
                                                        Html.button [
                                                            prop.className
                                                                "btn btn-primary btn-md rounded-none shadow-lg" 
                                                            prop.text "Shop now"
                                                        ]
                                                    ]
                                                ]
                                                Html.p [
                                                    prop.className "text-xs text-base-content/60"
                                                    prop.text $"{p.VariantCount} options"
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
    let View (model: Model) (dispatch: Client.Domain.Store.Collection.Msg -> unit) : ReactElement =
        React.useEffect(
            fun _ ->
                match model.Products with
                | Deferred.HasNotStartedYet -> dispatch LoadMore
                | _ -> ()
            , [| box model.Products |]
        ) 
        CollectionView 
            {
                Products       = model.Products
                TotalCount     = model.TotalCount
                IsLoading      = model.IsLoading
                Filters = model.Filters
                Paging = model.Paging
                SearchTerm = model.SearchTerm
                CanLoadMore    = State.canLoadMore model
                OnViewProduct  = fun p -> dispatch (ViewSyncProduct p)
                OnLoadMore     = fun _ -> LoadMore |> dispatch
            }
            dispatch
