namespace Client.Components.Shop.Common

open Feliz

module Ui =
    /// Simple helper to concatenate tailwind classes cleanly
    let inline tw (classes: string list) =
        classes
        |> List.filter (System.String.IsNullOrWhiteSpace >> not)
        |> String.concat " "

    /// DaisyUI button variants
    module Btn =
        let primary extra =
            tw ("btn btn-primary btn-lg tracking-[0.25em]" :: extra)

        let outline extra =
            tw ("btn btn-outline btn-lg tracking-[0.25em]" :: extra)

        let ghost extra =
            tw ("btn btn-ghost" :: extra)

    module Section =
        let container (children: ReactElement list) =
            Html.section [
                prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8"
                prop.children children
            ]

        let headerTagArea tagIcon (areaName: string) =
            Html.div [
                prop.className "flex justify-center mb-12 md:mb-16"
                prop.children [
                    Html.div [
                        prop.className "about-badge text-[0.65rem] tracking-[0.2em]"
                        prop.children [
                            tagIcon
                            Html.span areaName
                        ]
                    ]
                ]
            ]
    
    module LoadMoreProductsButton =
        type Props = {
            CanLoadMore : bool
            IsLoading : bool
            OnLoadMore : unit -> unit
        }
        let loadMoreProductsButton props =
            Html.div [
                prop.className "text-center pt-6"
                prop.children [
                    Html.button [
                        prop.className (Btn.outline [ "px-10 py-3 rounded-none" ])
                        prop.disabled (not props.CanLoadMore || props.IsLoading)
                        prop.text (
                            if props.IsLoading then "Loading…"
                            elif not props.CanLoadMore then "No more products"
                            else "Load More Products"
                        )
                        prop.onClick (fun _ ->
                            if props.CanLoadMore && not props.IsLoading
                            then props.OnLoadMore()
                        )
                    ]
                ]
            ]

    module CatalogHeader =
        
        open Shared.PrintfulCommon
        type SortOption = {
            Key   : string   // maps to sort_type
            Label : string
        }

        type SortDirection =
            | Asc
            | Desc

            member this.ToKey() =
                match this with
                | Asc  -> "asc"
                | Desc -> "desc"

        /// Props for the shared header
        type Props = {
            Title         : string
            Subtitle      : string option
            TotalCount    : int
            Filters       : Filters
            Paging        : PagingInfoDTO
            SearchTerm    : string option
            DisableControls : bool

            // callbacks
            OnSearchChanged : string -> unit
            // OnSortChanged   : string * string -> unit
            // OnToggleNewOnly : bool -> unit
            OnPageChange    : int -> unit
            OnFiltersChanged : Filters -> unit
        }

        let private sortOptions : SortOption list = [
            { Key = "featured"; Label = "Featured" }
            { Key = "price";    Label = "Price"    }
            { Key = "newest";   Label = "Newest"   }
        ]

        let private selectedSortKey (filters: Filters) =
            filters.SortType |> Option.defaultValue "featured"


        // helper creators
        let withSort filters (sortType: string, sortDir: string) =
            { filters with
                SortType      = Some sortType
                SortDirection = Some sortDir
            }

        let withRegion filters (regionOpt: string option) =
            { filters with SellingRegion = regionOpt }

        let withDestination filters (destOpt: string option) =
            { filters with DestinationCountry = destOpt }

        let withTechnique filters (techOpt: string option) =
            let techniques =
                match techOpt with
                | None -> []
                | Some key -> [ key ]
            { filters with Techniques = techniques }

        let withNewOnly filters (flag: bool) =
            { filters with OnlyNew = flag }

        let withColor filters (colorOpt: string option) =
            let colors =
                match colorOpt with
                | None      -> []
                | Some name -> [ name ]
            { filters with Colors = colors }

        let private selectedSortDir (filters: Filters) =
            filters.SortDirection |> Option.defaultValue "asc"

        [<ReactComponent>]
        let private filterControl (title: string) currentOption (optionPairs: List<string * string>) onChange =
            Html.div [
                prop.className "flex flex-col gap-1"
                prop.children [
                    Html.span [
                        prop.className "text-[10px] uppercase tracking-[0.2em] text-base-content/50"
                        prop.text title
                    ]
                    Html.select [
                        prop.className
                            "select select-sm rounded-none text-xs uppercase tracking-[0.2em] min-w-[9rem]"
                        let current = currentOption |> Option.defaultValue ""
                        prop.value current
                        prop.onChange (fun (v: string) -> onChange v )
                        prop.children [
                            optionPairs
                            |> List.map ( fun (v, lbl) ->
                                Html.option [ prop.value v; prop.text lbl ]
                            )
                            |> React.fragment
                        ]
                    ]
                ]
            ]

        [<ReactComponent>]
        let headerRight (props: Props) =
            Html.div [
                prop.className "flex flex-col md:flex-row md:items-center gap-3 md:gap-4 justify-end"
                prop.children [

                    // --- "new only" toggle ---
                    Html.label [
                        prop.className "label cursor-pointer gap-2"
                        prop.children [
                            Html.span [
                                prop.className "label-text text-[11px] uppercase tracking-[0.2em] text-base-content/60"
                                prop.text "New Only"
                            ]
                            Html.input [
                                prop.type'.checkbox
                                prop.className "checkbox checkbox-xs"
                                prop.isChecked props.Filters.OnlyNew
                                prop.onChange (fun (ckd: bool) ->
                                    withNewOnly props.Filters ckd |> props.OnFiltersChanged
                                )
                            ]
                        ]
                    ]

                    // --- Region (selling_region_name) ---
                    // filterControl
                    //     "Region"
                    //     f.SellingRegion
                    //     [
                    //         "", "All"
                    //         "US", "US"
                    //         "EU", "EU"
                    //         "GLOBAL", "Global" 
                    //     ]
                    //     (fun v ->
                    //         let value = if v = "" then None else Some v
                    //         withRegion props.Filters value |> props.OnFiltersChanged
                    //     )

                    // --- Color ---
                    filterControl
                        "Color"
                        (props.Filters.Colors |> List.tryHead)
                        [
                            "", "All"
                            "black", "Black"
                            "white", "White"
                            "grey", "Grey" 
                            "red", "Red"  
                            "blue", "Blue" 
                            "green", "Green"
                        ]
                        (fun v ->
                            let value = if v = "" then None else Some v
                            withColor props.Filters value |> props.OnFiltersChanged
                        )

                    // --- Destination Country ---
                    // filterControl
                    //     "Ship To"
                    //     props.Filters.DestinationCountry
                    //     [
                    //         "", "Any"
                    //         "US", "USA"
                    //         "CA", "Canada"
                    //         "GB", "UK" 
                    //     ]
                    //     (fun v ->
                    //         let value = if v = "" then None else Some v
                    //         withDestination props.Filters value |> props.OnFiltersChanged
                    //     )

                    // --- Technique ---
                    filterControl
                        "Technique"
                        (props.Filters.Techniques |> List.tryHead)
                        [
                            "", "Any"
                            "dtg", "DTG"
                            "embroidery", "Embroidery"
                            "sublimation", "Sublimation" 
                        ]
                        (fun v ->
                            let value = if v = "" then None else Some v
                            withTechnique props.Filters value |> props.OnFiltersChanged
                        )


                    // --- Sort ---
                    filterControl
                        "Sort"
                        (Some (
                            (props.Filters.SortType |> Option.defaultValue "featured")
                            + ":"
                            + (props.Filters.SortDirection |> Option.defaultValue "asc")
                        ))
                        [
                            "featured", "Featured"
                            "created_at", "Created"
                            "name", "Name"
                        ]
                        (fun v ->
                            let parts = v.Split(':')
                            let k = if parts.Length > 0 && parts[0] <> "" then parts[0] else "featured"
                            let d = if parts.Length > 1 && parts[1] <> "" then parts[1] else "asc"
                            withSort props.Filters (k, d) |> props.OnFiltersChanged
                        )
                ]
            ]

        [<ReactComponent>]
        let CatalogHeader (props: Props) =
            Html.div [
                prop.className "flex flex-col gap-4 pb-4 md:pb-6 p-6"

                prop.children [
                    // Top row: title + counts
                    Html.div [
                        prop.className "flex flex-col md:flex-row md:items-end md:justify-between gap-3"
                        prop.children [
                            Html.div [
                                prop.className "space-y-1"
                                prop.children [
                                    Html.h1 [
                                        prop.className "cormorant-font text-2xl md:text-4xl font-light tracking-tight text-base-content"
                                        prop.text props.Title
                                    ]
                                    match props.Subtitle with
                                    | Some sub ->
                                        Html.p [
                                            prop.className "text-sm text-base-content/60"
                                            prop.text sub
                                        ]
                                    | None -> Html.none
                                    Html.p [
                                        prop.className "text-xs text-base-content/50"
                                        prop.text $"{props.TotalCount} item(s)"
                                    ]
                                ]
                            ]

                            // Right side: search + sort + new-only
                            if not props.DisableControls
                            then headerRight props
                        ]
                    ]
                ]
            ]

        [<ReactComponent>]
        let twoActionButton visibilityClass onPrimaryClick onSecondaryClick = 
            // Actions
            Html.div [
                prop.className (
                    "flex flex-col sm:flex-row gap-4 justify-center transition-all duration-1000 delay-600 transform " +
                    visibilityClass
                )
                prop.children [
                    Html.button [
                        prop.onClick (fun _ -> onPrimaryClick())
                        prop.className "btn btn-neutral px-8 py-4 bg-gray-900 text-white text-sm tracking-widest hover:bg-gray-800 transition-all duration-300 group rounded-none"
                        prop.children [
                            Html.span [
                                prop.className "flex items-center justify-center gap-3"
                                prop.children [
                                    Html.span [ prop.text "CONTINUE SHOPPING" ]
                                    Html.i [
                                        prop.className "lucide lucide-arrow-right w-4 h-4 group-hover:translate-x-1 transition-transform"
                                    ]
                                ]
                            ]
                        ]
                    ]
                    Html.button [
                        prop.onClick (fun _ -> onSecondaryClick())
                        prop.className "btn px-8 py-4 bg-white text-gray-900 text-sm tracking-widest border border-gray-300 hover:border-gray-900 transition-all duration-300 rounded-none"
                        prop.text "VIEW ORDER DETAILS"
                    ]
                ]
            ]

        [<ReactComponent>]
        let helpSection visibilityClass =
            // Help section
            Html.div [
                prop.className (
                    "mt-16 text-center transition-all duration-1000 delay-800 transform " +
                    visibilityClass
                )
                prop.children [
                    Html.p [
                        prop.className "text-gray-500 text-sm mb-4"
                        prop.text "Questions about your order?"
                    ]
                    Html.a [
                        prop.href "/contact"
                        prop.className "text-sm text-gray-900 underline hover:text-gray-600 transition-colors tracking-wide"
                        prop.text "Contact us"
                    ]
                ]
            ]

        [<ReactComponent>]
        let footer =
            // Footer
            Html.footer [
                prop.className "border-t border-gray-200 bg-white mt-24"
                prop.children [
                    Html.div [
                        prop.className "max-w-7xl mx-auto px-6 py-12"
                        prop.children [
                            Html.div [
                                prop.className "flex justify-between items-center"
                                prop.children [
                                    Html.div [
                                        prop.className "text-xs tracking-wider text-gray-400 uppercase"
                                        prop.text "XERO EFFORT"
                                    ]
                                    Html.div [
                                        prop.className "flex gap-6 text-xs tracking-wider text-gray-400"
                                        prop.children [
                                            Html.a [
                                                prop.href "/terms"
                                                prop.className "hover:text-gray-900 transition-colors"
                                                prop.text "TERMS"
                                            ]
                                            Html.a [
                                                prop.href "/privacy"
                                                prop.className "hover:text-gray-900 transition-colors"
                                                prop.text "PRIVACY"
                                            ]
                                            Html.a [
                                                prop.href "/returns"
                                                prop.className "hover:text-gray-900 transition-colors"
                                                prop.text "RETURNS"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]


    module Animations =
        open Bindings.FramerMotion

        type RevealVariant =
            | FadeUp
            | FadeIn
            | ScaleUp
            | SlideRight
            | SlideLeft
            | Snap

        type ScrollRevealProps = {|

            Variant   : RevealVariant
            Delay     : float
            Threshold : float
            Children  : ReactElement
        |}

        [<ReactComponent>]
        let ScrollReveal (props: ScrollRevealProps) =
            let variants : obj =
                match props.Variant with
                | FadeUp ->
                    box {|
                        hidden  = {| opacity = 0.; y = 60 |}
                        visible = {| 
                            opacity = 1.; y = 0
                            transition = {| duration = 0.8; delay = props.Delay; ease = "easeOut" |}
                        |}
                    |}
                | FadeIn ->
                    box {|
                        hidden  = {| opacity = 0. |}
                        visible = {| 
                            opacity = 1.
                            transition = {| duration = 1.0; delay = props.Delay; ease = "easeOut" |}
                        |}
                    |}
                | ScaleUp ->
                    box {|
                        hidden  = {| opacity = 0.; scale = 0.9 |}
                        visible = {| 
                            opacity = 1.; scale = 1.
                            transition = {| duration = 0.7; delay = props.Delay; ease = "easeOut" |}
                        |}
                    |}
                | SlideRight ->
                    box {|
                        hidden  = {| opacity = 0.; x = -60 |}
                        visible = {| 
                            opacity = 1.; x = 0
                            transition = {| duration = 0.8; delay = props.Delay; ease = "easeOut" |}
                        |}
                    |}
                | SlideLeft ->
                    box {|
                        hidden  = {| opacity = 0.; x = 60 |}
                        visible = {| 
                            opacity = 1.; x = 0
                            transition = {| duration = 0.8; delay = props.Delay; ease = "easeOut" |}
                        |}
                    |}
                | Snap ->
                    box {|
                        hidden  = {| opacity = 0.; y = 40; scale = 0.95 |}
                        visible = {| 
                            opacity = 1.; y = 0; scale = 1.
                            transition = {| duration = 0.45; delay = props.Delay; ease = "easeOut" |}
                        |}
                    |}

            MotionDiv [
                prop.custom ("initial", "hidden")
                prop.custom ("whileInView", "visible")
                prop.custom ("viewport", box {| once = true; amount = props.Threshold |})
                prop.custom ("variants", variants)
                prop.children [ props.Children ]
            ]


        type ProgressiveRevealProps = {
            Children : ReactElement
        }

        /// Simple scroll fade/translate; no useScroll bindings required
        [<ReactComponent>]
        let ProgressiveReveal (props: ProgressiveRevealProps) =
            MotionDiv [
                prop.custom ("initial", box {| opacity = 0.; y = 80 |})
                prop.custom ("whileInView", box {| opacity = 1.; y = 0 |})
                prop.custom ("viewport", box {| amount = 0.4; once = false |})
                prop.custom (
                    "transition",
                    box {| duration = 0.9; ease = "easeOut" |}
                )
                prop.children [ props.Children ]
            ]



module Checkout =
    open Shared.Store
    open Shared.Api.Checkout

    /// Map a CartLineItem (UI / domain) -> CheckoutCartItem (API DTO)
    let toCheckoutItem (item: CartLineItem) : CheckoutCartItem =
        match item with
        | CartLineItem.Template t ->
            {
                Name = t.Name
                ThumbnailUrl = t.PreviewImage |> Option.defaultValue ""
                Kind             = CartItemKind.Template
                Quantity         = t.Quantity
                ExternalProductId = None
                SyncProductId    = None
                SyncVariantId    = None
                CatalogProductId = Some t.CatalogProductId
                CatalogVariantId = Some t.VariantId
                TemplateId       = Some t.TemplateId
            }

        | CartLineItem.Sync s ->
            {
                Name = s.Name
                ThumbnailUrl = s.ThumbnailUrl
                Kind             = CartItemKind.Sync
                Quantity         = s.Quantity
                ExternalProductId = s.ExternalId
                SyncProductId    = Some s.SyncProductId
                SyncVariantId    = Some s.SyncVariantId
                CatalogProductId = None
                CatalogVariantId = s.CatalogVariantId
                TemplateId       = None
            }

        | CartLineItem.Custom c ->
            {
                Name = c.Name
                ThumbnailUrl = c.ThumbnailUrl
                Kind             = CartItemKind.Custom
                Quantity         = c.Quantity
                ExternalProductId = None
                SyncProductId    = None
                SyncVariantId    = None
                CatalogProductId = Some c.CatalogProductId
                CatalogVariantId = Some c.CatalogVariantId
                TemplateId       = None
            }

    let toLineItem (item: CartLineItem) : LineItem =
        match item with
        | CartLineItem.Template t ->
            {
                externalId = ""
                productId = t.CatalogProductId
                variantId = t.VariantId
                quantity = t.Quantity
            }

        | CartLineItem.Sync s ->
            printfn $"SYNC PRODUCT ID: {s.SyncProductId}"
            {
                externalId = "" // s.ExternalId |> Option.defaultValue ""
                productId = s.SyncProductId
                variantId = s.CatalogVariantId |> Option.defaultValue 0
                quantity = s.Quantity
            }
            
        | CartLineItem.Custom c ->
            {
                externalId = ""
                productId = c.CatalogProductId
                variantId = c.CatalogVariantId
                quantity = c.Quantity
            }
                
    /// Map a whole cart to the DTO list used in Checkout requests.
    let toCheckoutItems (items: CartLineItem list) : CheckoutCartItem list =
        items |> List.map toCheckoutItem
    let toLineItems (items: CartLineItem list) : LineItem list =
        items |> List.map toLineItem

    // ----------------------------------------------------------------
    // Optional: reverse mapping (only if you ever want to rebuild the
    // CartLineItem from a CheckoutCartItem + some extra lookup data).
    // Typically you *don't* need this because you already keep the
    // original CartLineItem list on the client and just send a pared-
    // down DTO to the server.
    // ----------------------------------------------------------------

    type SyncVariantLookup = int64 * int64 -> SyncCartItem option
    type TemplateLookup    = int * int * int option -> TemplateCartItem option
    type CustomLookup      = int * int -> CartItem option

    let tryFromCheckoutItem
        (syncLookup    : SyncVariantLookup)
        (templateLookup: TemplateLookup)
        (customLookup  : CustomLookup)
        (dto           : CheckoutCartItem)
        : CartLineItem option =

        match dto.Kind with
        | CartItemKind.Sync ->
            match dto.SyncProductId, dto.SyncVariantId with
            | Some spid, Some svid ->
                syncLookup (spid, svid)
                |> Option.map CartLineItem.Sync
            | _ -> None

        | CartItemKind.Template ->
            match dto.CatalogProductId, dto.CatalogVariantId, dto.TemplateId with
            | Some cpid, Some cvid, tid ->
                templateLookup (cpid, cvid, tid)
                |> Option.map CartLineItem.Template
            | _ -> None

        | CartItemKind.Custom ->
            match dto.CatalogProductId, dto.CatalogVariantId with
            | Some cpid, Some cvid ->
                customLookup (cpid, cvid)
                |> Option.map CartLineItem.Custom
            | _ -> None


    let tryFindSyncItemInCart syncProductId syncVariantId cartItems =
        cartItems
        |> List.filter ( fun it ->
                match it with
                | CartLineItem.Sync _ -> true
                |  _ -> false
        )
        |> List.tryFind ( fun it ->
                match it with
                | CartLineItem.Sync ti ->
                    ti.SyncProductId = syncProductId
                    && ti.SyncVariantId = syncVariantId
                |  _ -> false
        )

    let tryFindCustomItemInCart catalogProductId catalogVariantId cartItems =
        cartItems
        |> List.filter ( fun it ->
                match it with
                | CartLineItem.Custom _ -> true
                |  _ -> false
        )
        |> List.tryFind ( fun it ->
                match it with
                | CartLineItem.Custom ti ->
                    ti.CatalogProductId = catalogProductId
                    && ti.CatalogVariantId = catalogVariantId
                |  _ -> false
        )

    let tryFindTemplateItemInCart templateId cartItems =
        cartItems
        |> List.filter ( fun it ->
                match it with
                | CartLineItem.Template _ -> true
                |  _ -> false
        )
        |> List.tryFind ( fun it ->
                match it with
                | CartLineItem.Template ti ->
                    ti.TemplateId = templateId
                |  _ -> false
        )

    let tryFindProductByKindInCart itemKind (item: CheckoutCartItem) cartItems =
        match itemKind with
        | CartItemKind.Template -> 
            tryFindTemplateItemInCart
                (item.TemplateId |> Option.defaultValue 0) 
                cartItems
        | CartItemKind.Sync -> 
            tryFindSyncItemInCart
                (item.SyncProductId |> Option.defaultValue 0)
                (item.SyncVariantId |> Option.defaultValue 0)
                cartItems
        | CartItemKind.Custom -> 
            tryFindCustomItemInCart 
                (item.CatalogProductId |> Option.defaultValue 0)
                (item.CatalogVariantId |> Option.defaultValue 0)
                cartItems

