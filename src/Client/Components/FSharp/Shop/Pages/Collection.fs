


namespace Client.Components.Shop

open Elmish
open Feliz
open Shared
open Feliz.UseDeferred
open Shared.StoreProductViewer.SyncProduct
open Shared.Api.Printful.SyncProduct
open Client.Components.Shop.Common.Ui

module ProductCard =

    open TSXUtilities

    let safeStrings (xs: string list) =
        xs |> List.choose (fun s -> if isNull s || System.String.IsNullOrWhiteSpace s then None else Some s)
    let fmtPrice (d: decimal) = d.ToString("0.00")
    
    let priceText (product: ShopProductViewer.ShopProductListItem) =
        match product.PriceMin, product.PriceMax with
        | Some lo, Some hi when lo = hi -> $"${fmtPrice lo}"
        | Some lo, Some hi -> $"${fmtPrice lo}–${fmtPrice hi}"
        | Some lo, None -> $"from ${fmtPrice lo}"
        | None, Some hi -> $"up to ${fmtPrice hi}"
        | None, None -> "—"

    let distinctColors (product: ShopProductViewer.ShopProductListItem) = product.Colors |> safeStrings |> List.distinct
    let distinctSizes (product: ShopProductViewer.ShopProductListItem) = product.Sizes  |> safeStrings |> List.distinct

    [<ReactComponent>]
    let FeaturedProductCard (onSelect: ShopProductViewer.ShopProductListItem -> unit) (featuredProductOpt: ShopProductViewer.ShopProductListItem option) =
        match featuredProductOpt with
        | None -> Html.none
        | Some product ->
            let colors = distinctColors product
            let sizes = distinctSizes product
            Html.div [
                prop.className "group cursor-pointer border border-base-content/8 hover:border-base-content/15 transition-all duration-500"
                prop.onClick (fun _ -> onSelect product)
                prop.children [
                    Html.div [
                        prop.className "grid grid-cols-1 lg:grid-cols-2 bg-base-100"
                        prop.children [

                            // Image side
                            Html.div [
                                prop.className "relative aspect-square lg:aspect-3/4 overflow-hidden bg-base-200"
                                prop.children [
                                    match product.ThumbnailUrl with
                                    | None ->
                                        Html.div [
                                            prop.className "absolute inset-0 flex items-center justify-center text-base-content/30 text-sm tracking-widest uppercase"
                                            prop.text "No Image"
                                        ]
                                    | Some tni ->
                                        Html.img [
                                            prop.src tni
                                            prop.alt product.Name
                                            prop.className "absolute inset-0 w-full h-full object-cover transform transition-transform duration-700 group-hover:scale-105"
                                        ]
                                ]
                            ]

                            // Details side
                            Html.div [
                                prop.className "p-8 md:p-12 lg:p-16 flex flex-col justify-center space-y-6"
                                prop.children [
                                    // Category
                                    Html.p [
                                        prop.className "text-[0.65rem] uppercase tracking-[0.25em] text-base-content/50"
                                        prop.text "Apparel"
                                    ]
                                    
                                    // Title
                                    Html.h3 [
                                        prop.className "text-3xl md:text-4xl lg:text-5xl font-light tracking-tight text-base-content cormorant-font leading-tight"
                                        prop.text product.Name
                                    ]
                                    
                                    // Blank name if exists
                                    match product.BlankName with
                                    | Some bn when not (System.String.IsNullOrWhiteSpace bn) ->
                                        Html.p [
                                            prop.className "text-xs text-base-content/40 font-light"
                                            prop.text bn
                                        ]
                                    | _ -> Html.none
                                    
                                    // Description
                                    Html.p [
                                        prop.className "text-base md:text-lg text-base-content/70 leading-relaxed font-light"
                                        prop.text "Premium quality garment with multiple variants and colorways."
                                    ]
                                    
                                    // Divider
                                    Html.div [
                                        prop.className "border-t border-base-content/8 my-6"
                                    ]
                                    
                                    // Price and meta
                                    Html.div [
                                        prop.className "flex items-baseline gap-4"
                                        prop.children [
                                            Html.div [
                                                prop.className "text-2xl md:text-3xl font-light cormorant-font text-base-content"
                                                prop.text (priceText product)
                                            ]
                                            Html.p [
                                                prop.className "text-xs text-base-content/50 tracking-wider"
                                                prop.text $"{sizes.Length} sizes • {colors.Length} colors"
                                            ]
                                        ]
                                    ]
                                    
                                    // Colors
                                    if not colors.IsEmpty then
                                        Html.div [
                                            prop.className "space-y-2"
                                            prop.children [
                                                Html.p [
                                                    prop.className "text-xs uppercase tracking-widest text-base-content/50"
                                                    prop.text "Colors"
                                                ]
                                                Html.div [
                                                    prop.className "flex items-center gap-2 flex-wrap"
                                                    prop.children [
                                                        for colorName in colors |> List.truncate 8 do
                                                            Html.div [
                                                                prop.className "h-8 w-8 rounded-full ring-1 ring-base-content/15 hover:ring-base-content/30 transition-all cursor-pointer"
                                                                prop.style [
                                                                    match resolvePrintfulColor colorName with
                                                                    | [| single |] -> style.backgroundColor single
                                                                    | colors ->
                                                                        let stops =
                                                                            colors
                                                                            |> Array.mapi (fun i hex ->
                                                                                let start = (i * 100) / colors.Length
                                                                                let stop  = ((i + 1) * 100) / colors.Length
                                                                                $"{hex} {start}%%, {hex} {stop}%%"
                                                                            )
                                                                            |> String.concat ", "
                                                                        style.backgroundImage $"linear-gradient(90deg, {stops})"
                                                                ]
                                                                prop.title colorName
                                                            ]
                                                        if colors.Length > 8 then
                                                            Html.span [
                                                                prop.className "text-xs text-base-content/50 ml-2"
                                                                prop.text $"+{colors.Length - 8} more"
                                                            ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    
                                    // Sizes
                                    if not sizes.IsEmpty then
                                        Html.div [
                                            prop.className "space-y-2"
                                            prop.children [
                                                Html.p [
                                                    prop.className "text-xs uppercase tracking-widest text-base-content/50"
                                                    prop.text "Sizes"
                                                ]
                                                Html.div [
                                                    prop.className "flex items-center gap-2 flex-wrap"
                                                    prop.children [
                                                        for s in sizes |> List.truncate 8 do
                                                            Html.span [
                                                                prop.className "px-3 py-2 text-xs border border-base-content/15 hover:border-base-content/30 hover:bg-base-content/5 transition-all cursor-pointer tracking-wider"
                                                                prop.text s
                                                            ]
                                                        if sizes.Length > 8 then
                                                            Html.span [
                                                                prop.className "text-xs text-base-content/50"
                                                                prop.text $"+{sizes.Length - 8}"
                                                            ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    
                                    // Shop button
                                    Html.button [
                                        prop.className "btn btn-primary btn-lg w-full md:w-auto px-12 rounded-none mt-4"
                                        prop.text "Shop now"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

    [<ReactComponent>]
    let ProductCard (onSelect: ShopProductViewer.ShopProductListItem -> unit) (product: ShopProductViewer.ShopProductListItem) =

        let colors = distinctColors product
        let sizes  = distinctSizes product

        Html.div [
            prop.key (string product.SyncProductId)
            prop.className "group cursor-pointer"
            prop.onClick (fun _ -> onSelect product)
            prop.children [
                // Card container
                Html.div [
                    prop.className "border border-base-content/8 hover:border-base-content/15 transition-all duration-500 hover:-translate-y-1 bg-base-100"
                    prop.children [
                        // Image
                        Html.div [
                            prop.className "relative aspect-3/4 bg-base-200 overflow-hidden"
                            prop.children [
                                match product.ThumbnailUrl with
                                | None ->
                                    Html.div [
                                        prop.className "absolute inset-0 flex items-center justify-center text-base-content/30 text-sm tracking-widest uppercase"
                                        prop.text "No Image"
                                    ]
                                | Some tni ->
                                    Html.img [
                                        prop.src tni
                                        prop.alt product.Name
                                        prop.className "absolute inset-0 w-full h-full object-cover transform transition-transform duration-700 group-hover:scale-110"
                                    ]
                            ]
                        ]

                        // Content
                        Html.div [
                            prop.className "p-6 space-y-4"
                            prop.children [
                                // Category
                                Html.p [
                                    prop.className "text-[0.65rem] uppercase tracking-[0.25em] text-base-content/50"
                                    prop.text "Apparel"
                                ]
                                
                                // Title
                                Html.h3 [
                                    prop.className "text-xl md:text-2xl font-light cormorant-font leading-tight text-base-content"
                                    prop.text product.Name
                                ]
                                
                                // Blank name
                                match product.BlankName with
                                | Some bn when not (System.String.IsNullOrWhiteSpace bn) ->
                                    Html.p [
                                        prop.className "text-xs text-base-content/40 font-light truncate"
                                        prop.text bn
                                    ]
                                | _ -> Html.none
                                
                                // Price
                                Html.div [
                                    prop.className "text-lg font-light text-base-content cormorant-font"
                                    prop.text (priceText product)
                                ]
                                
                                // Meta info
                                Html.p [
                                    prop.className "text-xs text-base-content/50 tracking-wider"
                                    prop.text $"{sizes.Length} sizes • {colors.Length} colors"
                                ]
                                
                                // Divider
                                Html.div [
                                    prop.className "border-t border-base-content/8 pt-4"
                                ]
                                
                                // Colors row
                                if not colors.IsEmpty then
                                    Html.div [
                                        prop.className "flex items-center gap-2"
                                        prop.children [
                                            for colorName in colors |> List.truncate 5 do
                                                Html.div [
                                                    prop.className "h-6 w-6 rounded-full ring-1 ring-base-content/15 hover:ring-2 hover:ring-base-content/30 transition-all"
                                                    prop.style [
                                                        match resolvePrintfulColor colorName with
                                                        | [| single |] -> style.backgroundColor single
                                                        | colors ->
                                                            let stops =
                                                                colors
                                                                |> Array.mapi (fun i hex ->
                                                                    let start = (i * 100) / colors.Length
                                                                    let stop  = ((i + 1) * 100) / colors.Length
                                                                    $"{hex} {start}%%, {hex} {stop}%%"
                                                                )
                                                                |> String.concat ", "
                                                            style.backgroundImage $"linear-gradient(90deg, {stops})"
                                                    ]
                                                    prop.title colorName
                                                ]
                                            if colors.Length > 5 then
                                                Html.span [
                                                    prop.className "text-xs text-base-content/50 ml-1"
                                                    prop.text $"+{colors.Length - 5}"
                                                ]
                                        ]
                                    ]
                                
                                // Sizes row
                                if not sizes.IsEmpty then
                                    Html.div [
                                        prop.className "flex items-center gap-2 flex-wrap"
                                        prop.children [
                                            for s in sizes |> List.truncate 6 do
                                                Html.span [
                                                    prop.className "px-2 py-1 text-[0.7rem] border border-base-content/10 hover:border-base-content/20 transition-colors tracking-wider"
                                                    prop.text s
                                                ]
                                            if sizes.Length > 6 then
                                                Html.span [
                                                    prop.className "text-xs text-base-content/50"
                                                    prop.text $"+{sizes.Length - 6}"
                                                ]
                                        ]
                                    ]
                                
                                // Shop button
                                Html.button [
                                    prop.className "btn btn-primary w-full rounded-none mt-2"
                                    prop.text "Shop now"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

module Collection =

    open TSXUtilities

    type Model = {
        Filters    : Filters
        Paging     : PagingInfoDTO
        SearchTerm : string option
        Products   : Deferred<SyncProductSummary list>
        ShopProducts   : Deferred<list<ShopProductViewer.ShopProductListItem>>
        TotalCount : int
        IsLoading  : bool
        Error      : string option
    }

    type Msg =
        | LoadMore
        | FiltersChanged of Filters
        | SearchChanged of string
        | SortChanged of sortType: string * sortDir: string
        | ApplyFilterPreset of string
        | SaveFilterPreset of string
        // Mongo DB Shop Products
        | GetProducts
        | GotProducts of list<ShopProductViewer.ShopProductListItem>
        | FailedProducts of exn
        | ViewProduct of ShopProductViewer.ShopProductListItem
    
    let getAllShopProducts () : Cmd<Msg> =
        Cmd.OfAsync.either
            Client.Api.shopApi.GetProducts
            ()
            GotProducts
            FailedProducts

    let initModel : Model = {
        Filters    = defaultFilters
        Paging     = emptyPaging
        SearchTerm = None
        Products   = Deferred.HasNotStartedYet
        ShopProducts   = Deferred.HasNotStartedYet
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

    let shopProductsOrEmpty products =
        match products with
        | Deferred.HasNotStartedYet
        | Deferred.InProgress
        | Deferred.Failed _ -> []
        | Deferred.Resolved products -> products

    let canLoadMore (model: Model) =
        if model.Products = Deferred.InProgress
        then false
        elif List.isEmpty (shopProductsOrEmpty model.Products) && model.TotalCount = 0 then
            true
        else
            model.Paging.offset + model.Paging.limit < model.TotalCount

    let init (rawQuery: string option) : Model * Cmd<Msg> =
        let m0 =
            match rawQuery with
            | Some q -> applyQueryToModel q initModel
            | None   -> initModel

        let m1 = { m0 with IsLoading = true }

        m1,
        getAllShopProducts ()
            

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        // ??
        | ViewProduct _ ->
            model, Cmd.none

        | GetProducts ->

            let m1 = { model with IsLoading = true; Error = None }
            m1, getAllShopProducts ()

        | GotProducts response ->
            { model with
                ShopProducts   = 
                    response
                    |> Deferred.Resolved
                // Paging     = response.paging
                // TotalCount = response.paging.total
                IsLoading  = false
                Error      = if response.IsEmpty then Some "No items found" else None
            }, Cmd.none
            
        | FailedProducts ex ->
            { model with
                ShopProducts = Deferred.Failed ex
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
                Cmd.ofMsg GetProducts
            else
                model, Cmd.none

        | FiltersChanged filters ->
            let m1 =
                { model with
                    Filters   = filters
                    Paging    = { model.Paging with offset = 0 }
                    IsLoading = true
                }
            m1, Cmd.ofMsg GetProducts

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

        | ApplyFilterPreset _ ->
            model, Cmd.none

        | SaveFilterPreset _ ->
            model, Cmd.none

    type Props = {
        ShopProducts       : Deferred<ShopProductViewer.ShopProductListItem list>
        TotalCount     : int
        IsLoading      : bool
        CanLoadMore    : bool
        Paging: PrintfulCommon.PagingInfoDTO
        Filters: Filters
        SearchTerm: string option
        OnViewProductItem  : ShopProductViewer.ShopProductListItem -> unit
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

                    // Featured (Random)
                    ProductCard.FeaturedProductCard
                        props.OnViewProductItem
                        (props.ShopProducts
                        |> shopProductsOrEmpty
                        |> fun x ->
                            if x.IsEmpty
                            then None
                            else 
                                System.Random()
                                |> fun rnd -> System.Math.Clamp( rnd.Next x.Length , 0 , x.Length - 1 )
                                |> fun rndFeat -> x |> List.tryItem rndFeat)

                    // Product grid
                    Html.div [
                        prop.className "grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-x-8 gap-y-12"
                        prop.children [
                            shopProductsOrEmpty props.ShopProducts
                            |> List.map (ProductCard.ProductCard props.OnViewProductItem)
                            |> React.fragment
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
    let View (model: Model) (dispatch: Msg -> unit) : ReactElement =
        CollectionView 
            {
                ShopProducts       = model.ShopProducts
                TotalCount     = model.TotalCount
                IsLoading      = model.IsLoading
                Filters = model.Filters
                Paging = model.Paging
                SearchTerm = model.SearchTerm
                CanLoadMore    = canLoadMore model
                OnViewProductItem  = fun p -> dispatch (ViewProduct p)
                OnLoadMore     = fun _ -> LoadMore |> dispatch
            }
            dispatch
