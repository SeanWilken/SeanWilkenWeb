namespace Client.Components.Shop

open Feliz
open Elmish
open Client.Components.Shop.Common.Ui
open Shared.SharedShopV2.PrintfulCatalog
open Client.Domain.Store.ProductDesigner
open Bindings.LucideIcon

module Designer =
    open Client.Domain.Store.ProductDesigner.Designs
    open Shared.PrintfulCommon

    let private stepOrder : StepDesigner list =
        [ SelectBaseProduct; SelectVariants; ProductDesigner; ReviewDesign ]

    let private stepLabel = function
        | SelectBaseProduct      -> "Base Product"
        | SelectVariants      -> "Color & Size"
        | ProductDesigner       -> "Add Designs"
        | ReviewDesign             -> "Review"

    let private loadProductsCmd (model: Model) : Cmd<Msg> =
        let q =
            Shared.Api.Printful.CatalogProductRequest.toApiQuery
                model.paging
                model.query

        Cmd.OfAsync.either
            (fun qp -> Client.Api.productsApi.getProducts qp)
            q
            ProductsLoaded
            (fun ex -> LoadFailed ex.Message)

        // helpers for pretty labels
    let private hitAreaLabel (hit: DesignHitArea) =
        match hit with
        | DesignHitArea.Front      -> "Front"
        | DesignHitArea.Back       -> "Back"
        | DesignHitArea.Pocket     -> "Pocket"
        | DesignHitArea.LeftSleeve -> "Left Sleeve"
        | DesignHitArea.RightSleeve-> "Right Sleeve"
        | DesignHitArea.LeftLeg    -> "Left Leg"
        | DesignHitArea.RightLeg   -> "Right Leg"
        | DesignHitArea.LeftHalf   -> "Left Half"
        | DesignHitArea.RightHalf  -> "Right Half"
        | DesignHitArea.Center     -> "Center"
        | DesignHitArea.Custom s   -> s

    let private sizeLabel = function
        | DesignSize.Small  -> "Small"
        | DesignSize.Medium -> "Medium"
        | DesignSize.Large  -> "Large"

    let private techniqueLabel = function
        | DesignTechnique.DTG         -> "DTG"
        | DesignTechnique.Embroidery  -> "Embroidery"
        | DesignTechnique.Digital     -> "Screen Print"   // or "Digital"
        | DesignTechnique.Knitting    -> "Knitting"
        | DesignTechnique.Sublimation -> "Heat Transfer"  // tweak if you want
        | DesignTechnique.Other t     -> t

    let private techniqueFromString = function
        | "DTG"          -> DesignTechnique.DTG
        | "Embroidery"   -> DesignTechnique.Embroidery
        | "Screen Print" -> DesignTechnique.Digital
        | "Heat Transfer"-> DesignTechnique.Sublimation
        | other          -> DesignTechnique.Other other

    let private sizeFromString = function
        | "Small"  -> DesignSize.Small
        | "Medium" -> DesignSize.Medium
        | "Large"  -> DesignSize.Large
        | _        -> DesignSize.Medium

    let private upsertLayerOption (name: PrintfulLayerOptionName) (values: string list) (opts: PrintfulLayerOption list) =
        let rec loop acc remaining =
            match remaining with
            | [] ->
                // not found → append
                List.rev ({ Name = name; Values = values } :: acc)
            | x :: xs when x.Name = name ->
                // found → replace
                List.rev ({ x with Values = values } :: acc) @ xs
            | x :: xs ->
                loop (x :: acc) xs
        loop [] opts

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | GoToStep step ->
            { model with currentStep = step }, Cmd.none
        | SelectBase catalogProduct ->
            { model with selectedVariantId = Some catalogProduct.id; selectedProduct = Some catalogProduct }, Cmd.none
        | SelectColor color ->
            { model with selectedVariantColor = Some color }, Cmd.none
        | SelectSize size ->
            { model with selectedVariantSize = Some size }, Cmd.none

        | DesignerSearchChanged st -> { model with searchTerm = st }, Cmd.ofMsg LoadProducts
        | DesignerSortChanged (sortType, sortDir) -> model, Cmd.none
        | DesignerFiltersChanged filters -> { model with query = filters }, Cmd.ofMsg LoadProducts
        | DesignerPageChanged pagingInfo -> { model with paging = pagingInfo }, Cmd.ofMsg LoadProducts

        | AddAsset asset ->
            // If asset is already in selectedAssets, do nothing.
            let alreadySelected =
                model.selectedAssets
                |> List.exists (fun a -> a.Id = asset.Id)

            // Decide a default hit area + size for a newly added design
            let defaultHitArea =
                match model.placements with
                | p :: _ -> p.HitArea
                | []     -> DesignHitArea.Front

            let defaultSize = DesignSize.Medium

            let newPlaced =
                Designs.Defaults.toPlaced defaultHitArea defaultSize asset

            let newSelectedAssets =
                if alreadySelected then model.selectedAssets
                else model.selectedAssets @ [ asset ]

            let newPlacedDesigns = model.placedDesigns @ [ newPlaced ]
            let newActiveIndex = Some (newPlacedDesigns.Length - 1)

            { model with
                selectedAssets    = newSelectedAssets
                placedDesigns     = newPlacedDesigns
                activePlacedIndex = newActiveIndex
            },
            Cmd.none

        | RemoveAsset assetId ->
            let remainingAssets =
                model.selectedAssets
                |> List.filter (fun a -> a.Id <> assetId)

            let remainingPlaced =
                model.placedDesigns
                |> List.filter (fun pd -> pd.Asset.Id <> assetId)

            // Re-normalize active index, if any
            let newActive =
                match model.activePlacedIndex with
                | None -> None
                | Some idx ->
                    if remainingPlaced.IsEmpty then None
                    elif idx >= remainingPlaced.Length then Some (remainingPlaced.Length - 1)
                    else Some idx

            { model with
                selectedAssets    = remainingAssets
                placedDesigns     = remainingPlaced
                activePlacedIndex = newActive
            },
            Cmd.none

        | SetActivePlaced index ->
            // only set it if index in range
            let newActive =
                if index >= 0 && index < model.placedDesigns.Length
                then Some index
                else None

            { model with activePlacedIndex = newActive }, Cmd.none

        | UpdatePlacedPlacement (idx, hitArea) ->
            let updated =
                model.placedDesigns
                |> List.mapi (fun i d ->
                    if i = idx then { d with HitArea = hitArea }
                    else d
                )

            { model with placedDesigns = updated }, Cmd.none

        | UpdatePlacedSize (idx, size) ->
            let updated =
                model.placedDesigns
                |> List.mapi (fun i d ->
                    if i = idx then { d with Size = size }
                    else d
                )

            { model with placedDesigns = updated }, Cmd.none

        | UpdatePlacedTechnique (idx, tech) ->
            let updated =
                model.placedDesigns
                |> List.mapi (fun i d ->
                    if i = idx then { d with Technique = tech }
                    else d
                )

            { model with placedDesigns = updated }, Cmd.none

        | UpdatePlacedPositionTag (idx, tag) ->
            // For now, just store the tag in a synthetic layer option
            let updated =
                model.placedDesigns
                |> List.mapi (fun i d ->
                    if i = idx then
                        let newOptions =
                            upsertLayerOption (PrintfulLayerOptionName.Raw "position_tag") [ tag ] d.LayerOptions
                        { d with LayerOptions = newOptions }
                    else d
                )

            { model with placedDesigns = updated }, Cmd.none

        | LoadProducts -> 
            model, loadProductsCmd model
        | ProductsLoaded catalogProductsResponse ->
            { model with products = catalogProductsResponse.products; paging = catalogProductsResponse.paging }, Cmd.none
        | LoadFailed failureMsg ->
            model, Cmd.none
        | ViewProduct catalogProduct ->
            model, Cmd.none
        | BackToDropLanding ->
            model, Cmd.none
        | AddToCart _ ->
            model, Cmd.none

    type FooterControl = {
        ButtonMsg: Msg
        ButtonLabel: string
        IsDisabled: bool
        IsHidden: bool
    }

    let footerNav (previous: FooterControl) (next: FooterControl) dispatch  = 
        Html.div [
            prop.className "flex gap-3 pt-4"
            prop.children [
                if not previous.IsHidden
                then
                    Html.button [
                        prop.type'.button
                        prop.disabled previous.IsDisabled
                        prop.onClick (fun _ ->
                            if not previous.IsDisabled then dispatch previous.ButtonMsg
                        )
                        prop.className (
                            "flex-1 border border-base-content py-3 md:py-4 text-xs md:text-sm font-medium \
                            uppercase tracking-[0.2em] hover:bg-base-200/60 transition-colors"
                        )
                        prop.text previous.ButtonLabel
                    ]
                if not next.IsHidden
                then
                    Html.button [
                        prop.type'.button
                        prop.disabled next.IsDisabled
                        prop.onClick (fun _ ->
                            if not next.IsDisabled then dispatch next.ButtonMsg
                        )
                        prop.className (
                            "flex-1 py-3 md:py-4 text-xs md:text-sm font-medium uppercase tracking-[0.2em] \
                            transition-colors " +
                            (
                                if next.IsDisabled
                                then "bg-base-300 text-base-content/50 cursor-not-allowed"
                                else "bg-base-content text-base-100 hover:bg-base-content/90"
                            )
                        )
                        prop.text next.ButtonLabel
                    ]
            ]
        ]

    let private stepper (model: Model) (dispatch: Msg -> unit) =
        Html.div [
            prop.className "mb-10"
            prop.children [
                Html.div [
                    prop.className "flex items-center justify-between gap-2"
                    prop.children [
                        for step in stepOrder do
                            let currentIndex = stepOrder |> List.findIndex ((=) model.currentStep)
                            let index        = stepOrder |> List.findIndex ((=) step)
                            let isActive     = index = currentIndex
                            let isComplete   = index <  currentIndex

                            Html.div [
                                prop.key (string index)
                                prop.className "flex-1 relative"
                                prop.children [
                                    Html.div [
                                        prop.className "flex flex-col items-center"
                                        prop.children [
                                            Html.div [
                                                prop.className (
                                                    "w-9 h-9 rounded-full flex items-center justify-center text-xs font-medium transition-all"
                                                    + 
                                                        if isComplete then "bg-primary text-primary-content"
                                                        elif isActive then "bg-primary text-primary-content scale-110"
                                                        else "bg-base-200 text-base-content/50" 
                                                )
                                                prop.text (if isComplete then "✓" else string (index + 1))
                                            ]
                                            Html.span [
                                                prop.className(
                                                    "mt-2 text-[0.6rem] uppercase tracking-[0.25em]"
                                                    + 
                                                        if isActive then "text-base-content font-semibold"
                                                        else "text-base-content/50"
                                                )
                                                prop.text (stepLabel step)
                                                prop.onClick ( fun _ -> GoToStep step |> dispatch )
                                            ]
                                        ]
                                    ]

                                    if index < stepOrder.Length - 1 then
                                        Html.div [
                                            prop.className (
                                                "absolute top-[1.15rem] left-1/2 w-full h-px -z-10"
                                                + 
                                                    if isComplete then "bg-primary"
                                                    else "bg-base-200"
                                            )
                                        ]
                                ]
                            ]
                    ]
                ]
            ]
        ]

    let private preview (model: Model) =
        Html.div [
            prop.className "sticky top-28"
            prop.children [
                Html.div [
                    prop.className "aspect-square bg-gradient-to-br from-base-200 to-base-100 rounded-xl flex items-center justify-center relative overflow-hidden"
                    prop.children [
                        Html.img [
                            prop.src (model.selectedProduct |> Option.map (fun p -> string p.thumbnailURL) |> Option.defaultValue "?")
                            prop.alt (model.selectedProduct |> Option.map (fun p -> string p.name) |> Option.defaultValue "?")
                            prop.className "text-[7rem] md:text-[8rem] font-light text-base-content/20"
                        ]
                        match model.selectedVariantColor with
                        | Some colorCode ->
                            Html.div [
                                prop.className "absolute bottom-4 right-4 w-12 h-12 rounded-full border-4 border-base-100 shadow-lg"
                                prop.style [ style.backgroundColor colorCode ]
                            ]
                        | None -> Html.none
                    ]
                ]
                Html.div [
                    prop.className "mt-4 text-center"
                    prop.children [
                        Html.p [
                            prop.className "text-sm text-base-content/60"
                            prop.text "Live Preview"
                        ]
                        Html.p [
                            prop.className "text-xs text-base-content/40"
                            prop.text "Your design will appear here."
                        ]
                    ]
                ]
            ]
        ]
    
    let private baseProductRow
        (selected : CatalogProduct option)
        (dispatch : Msg -> unit)
        (p        : CatalogProduct) =
        let isActive =
            selected
            |> Option.exists (fun s -> s.id = p.id)

        Html.button [
            prop.key p.id
            prop.onClick (fun _ -> dispatch (SelectBase p))
            prop.className (
                "w-full text-left flex items-center gap-4 p-3 rounded-lg border transition-colors " +
                (if isActive
                then "bg-base-200 border-base-content/40"
                else "bg-base-100 border-base-300 hover:bg-base-200/80")
            )
            prop.children [
                // Thumbnail / numeric fallback
                Html.div [
                    prop.className "w-14 h-14 rounded-md bg-base-200 flex items-center justify-center overflow-hidden"
                    prop.children [
                        if System.String.IsNullOrWhiteSpace p.thumbnailURL |> not then
                            Html.img [
                                prop.src p.thumbnailURL
                                prop.alt p.name
                                prop.className "w-full h-full object-cover"
                            ]
                        else
                            Html.span [
                                prop.className "text-xs text-base-content/50"
                                prop.text (string p.id)
                            ]
                    ]
                ]

                // Text content
                Html.div [
                    prop.className "flex-1 min-w-0"
                    prop.children [
                        Html.p [
                            prop.className "text-[0.65rem] uppercase tracking-[0.25em] text-base-content/50"
                            prop.text (
                                p.brand
                                |> Option.orElse p.model
                                |> Option.defaultValue "Apparel"
                            )
                        ]
                        Html.p [
                            prop.className "font-medium text-sm truncate"
                            prop.text p.name
                        ]
                        Html.p [
                            prop.className "text-[0.7rem] text-base-content/60 truncate"
                            prop.text (
                                p.description
                                |> Option.defaultValue "Multi-variant Printful product."
                            )
                        ]
                    ]
                ]

                // Meta: variants / colors
                Html.div [
                    prop.className "flex flex-col items-end gap-1"
                    prop.children [
                        Html.span [
                            prop.className "text-[0.7rem] text-base-content/70"
                            prop.text $"{p.variantCount} variants"
                        ]
                        Html.span [
                            prop.className "text-[0.7rem] text-base-content/50"
                            prop.text $"{p.colors.Length} colors"
                        ]
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    let private catalogProductSelector model dispatch =
        Html.div [
            prop.className "space-y-6"
            prop.children [
                Html.h2 [
                    prop.className "text-2xl md:text-3xl font-light text-base-content"
                    prop.text "Choose Base Product"
                ]
                Html.p [
                    prop.className "text-sm text-base-content/60"
                    prop.text "Select a product template to start designing."
                ]
                Html.div [
                    prop.className "space-y-3 max-h-[28rem] overflow-y-auto pr-1"
                    prop.children [
                        if model.products |> List.isEmpty then
                            Html.div [
                                prop.className "alert alert-info text-sm"
                                prop.text "No catalog products loaded yet. Hook this up to your Printful catalog query."
                            ]
                        else
                            for p in model.products do
                                baseProductRow model.selectedProduct dispatch p
                    ]
                ]

                // Html.div [
                //     prop.className "text-center pt-6"
                //     prop.children [
                //         Html.button [
                //             prop.className (Btn.outline [ "px-10 py-3 rounded-none" ])
                //             prop.text "Load More Products"
                //             prop.onClick (fun _ -> dispatch LoadProducts)
                //         ]
                //     ]
                // ]

                // let canLoadMore (model: Model) =
                //     if List.isEmpty model.Products && model.TotalCount = 0 then
                //         true   // initial load allowed
                //     else
                //         model.Paging.offset + model.Paging.limit < model.TotalCount

                // State.canLoadMore model

                // Html.div [
                //     prop.className "text-center pt-6"
                //     prop.children [
                //         Html.button [
                //             prop.className (Btn.outline [ "px-10 py-3 rounded-none" ])
                //             prop.disabled (not props.CanLoadMore || props.IsLoading)
                //             prop.text (
                //                 if props.IsLoading then "Loading…"
                //                 elif not props.CanLoadMore then "No more products"
                //                 else "Load More Products"
                //             )
                //             prop.onClick (fun _ ->
                //                 if props.CanLoadMore && not props.IsLoading
                //                 then props.OnLoadMore()
                //             )
                //         ]
                //     ]
                // ]
                // LoadMoreProductsButton.loadMoreProductsButton {
                //     IsLoading = false
                //     CanLoadMore = true
                //     OnLoadMore = fun _ -> dispatch LoadProducts
                // }
                Html.div [
                    prop.className "flex centered"
                    prop.children [
                        Components.FSharp.Layout.Elements.Pagination.Pagination {
                                total = model.paging.total
                                limit = model.paging.limit
                                offset = model.paging.offset
                                onPageChange = fun i -> 
                                    printfn "Page change: %i" i
                                    dispatch (DesignerPageChanged { model.paging with offset = i })
                        }
                    ]
                ]

                footerNav
                    { IsHidden = false; IsDisabled = false; ButtonLabel = "Back"; ButtonMsg = BackToDropLanding }
                    { IsHidden = false; IsDisabled = model.selectedProduct.IsNone; ButtonLabel = "Select Options"; ButtonMsg = (GoToStep SelectVariants) }
                    dispatch                                                
            ]
        ]

    [<ReactComponent>]
    let private variantSelection model dispatch =
        Html.div [
            prop.className "space-y-6"
            prop.children [
                Html.h2 [
                    prop.className "text-2xl md:text-3xl font-light"
                    prop.text "Select Color & Size"
                ]

                match model.selectedProduct with
                | None ->
                    Html.div [
                        prop.className "alert alert-info text-sm"
                        prop.text "Choose a base product first, then you can select color and size."
                    ]

                | Some sp ->
                    // derive selected color name (optional)
                    let selectedColorHex = model.selectedVariantColor
                    let selectedColorName =
                        selectedColorHex
                        |> Option.bind (fun hex ->
                            sp.colors
                            |> List.tryFind (fun c -> c.ColorCodeOpt |> Option.defaultValue "" = hex)
                            |> Option.map (fun c -> c.Color)
                        )

                    Html.div [
                        prop.className "space-y-8"
                        prop.children [

                            // COLOR SELECTOR
                            Html.div [
                                prop.className "space-y-4"
                                prop.children [
                                    Html.label [
                                        prop.className "text-sm font-medium uppercase tracking-wider"
                                        prop.text "Color"
                                    ]

                                    Html.div [
                                        prop.className "grid grid-cols-3 gap-3 overflow-y-scroll"
                                        prop.children [
                                            for color in sp.colors do
                                                let hex =
                                                    color.ColorCodeOpt
                                                    |> Option.defaultValue "#000000"

                                                let isSelected =
                                                    Some hex = model.selectedVariantColor

                                                Html.button [
                                                    prop.key (color.Color + hex)
                                                    prop.type' "button"
                                                    prop.onClick (fun _ ->
                                                        SelectColor hex |> dispatch
                                                    )
                                                    prop.className (
                                                        "aspect-square rounded-lg border-2 transition-all hover:scale-105 " +
                                                        (if isSelected then
                                                            "border-base-content ring-2 ring-offset-2 ring-base-content"
                                                        else
                                                            "border-base-300")
                                                    )
                                                    prop.style [ style.backgroundColor hex ]
                                                    prop.children [
                                                        // special case for white-ish colors so they don't disappear
                                                        if hex.ToLower() = "#ffffff" || hex.ToLower() = "#fff" then
                                                            Html.div [
                                                                prop.className "w-full h-full border border-base-200 rounded-lg"
                                                            ]
                                                        else Html.none
                                                    ]
                                                ]
                                        ]
                                    ]

                                    match selectedColorName with
                                    | Some name ->
                                        Html.p [
                                            prop.className "text-sm text-base-content/60"
                                            prop.text $"Selected: {name}"
                                        ]
                                    | None -> Html.none
                                ]
                            ]

                            // SIZE SELECTOR
                            Html.div [
                                prop.className "space-y-4"
                                prop.children [
                                    Html.label [
                                        prop.className "text-sm font-medium uppercase tracking-wider"
                                        prop.text "Size"
                                    ]

                                    Html.div [
                                        prop.className "grid grid-cols-3 gap-3"
                                        prop.children [
                                            for size in sp.sizes do
                                                let isSelected =
                                                    Some size = model.selectedVariantSize

                                                Html.button [
                                                    prop.key size
                                                    prop.type' "button"
                                                    prop.onClick (fun _ ->
                                                        SelectSize size |> dispatch
                                                    )
                                                    prop.className (
                                                        "py-3 md:py-4 border-2 text-sm font-medium uppercase tracking-wider transition-all " +
                                                        (if isSelected then
                                                            "border-base-content bg-base-content text-base-100"
                                                        else
                                                            "border-base-300 hover:border-base-content")
                                                    )
                                                    prop.text size
                                                ]
                                        ]
                                    ]
                                ]
                            ]

                            // NAV BUTTONS
                            footerNav
                                { IsHidden = false; IsDisabled = false; ButtonLabel = "Back"; ButtonMsg = (GoToStep SelectBaseProduct) }
                                { IsHidden = false; IsDisabled = not (model.selectedVariantColor.IsSome && model.selectedVariantSize.IsSome); ButtonLabel = "Add Designs"; ButtonMsg = (GoToStep ProductDesigner) }
                                dispatch
                        ]
                    ]
            ]
        ]

    [<ReactComponent>]
    let private designStudioView (model: Model) (dispatch: Msg -> unit) =
        let designs      = model.placedDesigns
        let designCount  = designs |> List.length

        let activeIdxOpt = model.activePlacedIndex
        let activeDesignOpt =
            activeIdxOpt
            |> Option.bind (fun idx -> designs |> List.tryItem idx)

        // For the "Add Another Design" button: only assets not already used
        let usedAssetIds =
            designs |> List.map (fun pd -> pd.Asset.Id) |> Set.ofList

        let availableToAdd =
            model.availableAssets
            |> List.filter (fun a -> not (usedAssetIds.Contains a.Id))

        Html.div [
            prop.className "space-y-6"
            prop.children [

                // header row
                Html.div [
                    prop.className "flex items-center justify-between"
                    prop.children [
                        Html.h2 [
                            prop.className "text-2xl md:text-3xl font-light"
                            prop.text "Design Studio"
                        ]
                        Html.span [
                            prop.className "text-sm text-base-content/60"
                            prop.text (
                                let label = if designCount = 1 then "design" else "designs"
                                $"{designCount} {label}"
                            )
                        ]
                    ]
                ]

                // no designs yet → show asset grid
                if designCount = 0 then
                    Html.div [
                        Html.p [
                            prop.className "text-sm font-medium uppercase tracking-[0.2em] mb-4"
                            prop.text "Select Your First Design"
                        ]
                        Html.div [
                            prop.className "grid grid-cols-2 gap-4"
                            prop.children [
                                for asset in model.availableAssets do
                                    Html.button [
                                        prop.key (string asset.Id)
                                        prop.type'.button
                                        prop.onClick (fun _ -> dispatch (AddAsset asset))
                                        prop.className
                                            "border-2 border-base-300 p-4 cursor-pointer hover:border-base-content \
                                            transition-all group text-left rounded-xl bg-base-100"
                                        prop.children [
                                            Html.div [
                                                prop.className
                                                    "aspect-square bg-base-200 mb-3 flex items-center justify-center \
                                                    text-4xl font-light text-base-content/20 group-hover:scale-110 \
                                                    transition-transform rounded-lg overflow-hidden"
                                                prop.children [
                                                    Html.img [
                                                        prop.src asset.ImageUrl
                                                        prop.alt asset.Name
                                                        prop.className "w-full h-full object-cover"
                                                    ]
                                                ]
                                            ]
                                            Html.p [
                                                prop.className "text-sm font-medium"
                                                prop.text asset.Name
                                            ]
                                            Html.p [
                                                prop.className "text-xs text-base-content/60"
                                                prop.text (asset.Tagline |> Option.defaultValue "Artwork design")
                                            ]
                                        ]
                                    ]
                            ]
                        ]
                    ]
                else
                    // designs exist → show “Your Designs” + configure panel
                    Html.div [
                        prop.className "space-y-6"
                        prop.children [

                            // Your Designs list
                            Html.div [
                                prop.className "space-y-3"
                                prop.children [
                                    Html.p [
                                        prop.className "text-sm font-medium uppercase tracking-[0.2em]"
                                        prop.text "Your Designs"
                                    ]

                                    for idx, pd in designs |> List.indexed do
                                        let isActive =
                                            match activeIdxOpt with
                                            | Some i when i = idx -> true
                                            | _ -> false

                                        // try to get friendly placement label from model.placements
                                        let placementLabel =
                                            model.placements
                                            |> List.tryFind (fun p -> p.HitArea = pd.HitArea)
                                            |> Option.map (fun p -> p.Label)
                                            |> Option.defaultValue (hitAreaLabel pd.HitArea)

                                        Html.button [
                                            prop.key (sprintf "%O-%d" pd.InstanceId idx)
                                            prop.type'.button
                                            prop.onClick (fun _ -> dispatch (SetActivePlaced idx))
                                            prop.className (
                                                "w-full text-left p-4 border-2 rounded-xl transition-all " +
                                                (
                                                    if isActive
                                                    then "border-base-content bg-base-200/60"
                                                    else "border-base-300 hover:border-base-content/60 bg-base-100"
                                                )
                                            )
                                            prop.children [
                                                Html.div [
                                                    prop.className "flex items-center justify-between mb-2"
                                                    prop.children [
                                                        Html.span [
                                                            prop.className "font-medium text-sm"
                                                            prop.text pd.Asset.Name
                                                        ]
                                                        Html.button [
                                                            prop.type'.button
                                                            prop.onClick (fun e ->
                                                                e.stopPropagation()
                                                                dispatch (RemoveAsset pd.Asset.Id)
                                                            )
                                                            prop.className
                                                                "text-base-content/40 hover:text-base-content transition-colors"
                                                            prop.children [
                                                                LucideIcon.X "w-4 h-4"
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "text-[11px] text-base-content/60 space-y-1"
                                                    prop.children [
                                                        // line 1: placement (and maybe a future “position tag”)
                                                        Html.p [
                                                            prop.text placementLabel
                                                        ]
                                                        // line 2: size & technique
                                                        Html.p [
                                                            prop.text (
                                                                $"{sizeLabel pd.Size} • {techniqueLabel pd.Technique}"
                                                            )
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                ]
                            ]

                            // Add another design
                            Html.button [
                                prop.type'.button
                                prop.onClick (fun _ ->
                                    match availableToAdd with
                                    | next::_ ->
                                        dispatch (AddAsset next)
                                    | [] -> ()
                                )
                                prop.disabled (availableToAdd.IsEmpty)
                                prop.className
                                    "w-full border-2 border-dashed border-base-300 py-4 text-sm font-medium \
                                    uppercase tracking-[0.2em] hover:border-base-content transition-colors \
                                    disabled:opacity-50 disabled:cursor-not-allowed flex items-center \
                                    justify-center gap-2 rounded-xl"
                                prop.children [
                                    LucideIcon.Plus "w-4 h-4"
                                    Html.span [ prop.text "Add Another Design" ]
                                ]
                            ]

                            // configure active design
                            match activeIdxOpt, activeDesignOpt with
                            | Some activeIdx, Some pd ->
                                Html.div [
                                    prop.className "border-t pt-6 space-y-6"
                                    prop.children [
                                        Html.h3 [
                                            prop.className "text-sm font-medium uppercase tracking-[0.2em]"
                                            prop.text "Configure Design"
                                        ]

                                        // // Placement on garment (HitArea)
                                        Html.div [
                                            prop.className "space-y-3"
                                            prop.children [
                                                Html.label [
                                                    prop.className "text-sm font-medium uppercase tracking-[0.2em]"
                                                    prop.text "Placement on Garment"
                                                ]
                                                Html.select [
                                                    prop.className
                                                        "w-full px-4 py-3 border border-base-300 text-sm font-medium \
                                                        focus:outline-none focus:border-base-content transition-colors rounded-lg bg-base-100"
                                                    prop.value (
                                                        model.placements
                                                        |> List.tryFind (fun p -> p.HitArea = pd.HitArea)
                                                        |> Option.map (fun p -> p.Label)
                                                        |> Option.defaultValue (hitAreaLabel pd.HitArea)
                                                    )
                                                    prop.onChange (fun (value: string) ->
                                                        // find PlacementOption by label
                                                        match model.placements
                                                            |> List.tryFind (fun p -> p.Label = value) with
                                                        | Some po ->
                                                            dispatch (UpdatePlacedPlacement (activeIdx, po.HitArea))
                                                        | None ->
                                                            ()
                                                    )
                                                    prop.children [
                                                        for po in model.placements do
                                                            Html.option [
                                                                prop.key po.Label
                                                                prop.value po.Label
                                                                prop.text po.Label
                                                            ]
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // // Position (Top / Center / Bottom) – we just send a tag for now
                                        Html.div [
                                            prop.className "space-y-3"
                                            prop.children [
                                                Html.label [
                                                    prop.className "text-sm font-medium uppercase tracking-[0.2em]"
                                                    prop.text "Position"
                                                ]
                                                Html.div [
                                                    prop.className "grid grid-cols-3 gap-3"
                                                    prop.children [
                                                        for pos in [ "Top"; "Center"; "Bottom" ] do
                                                            Html.button [
                                                                prop.key pos
                                                                prop.type'.button
                                                                prop.onClick (fun _ ->
                                                                    dispatch (UpdatePlacedPositionTag (activeIdx, pos))
                                                                )
                                                                // we don’t have a stored tag yet – you can compare against
                                                                // some value in your model later; for now treat all as unselected
                                                                prop.className
                                                                    "py-3 border-2 text-xs md:text-sm font-medium uppercase \
                                                                    tracking-[0.2em] transition-all border-base-300 hover:border-base-content"
                                                                    // (
                                                                    //     if isSelected
                                                                    //     then "border-base-content bg-base-content text-base-100"
                                                                    //     else "border-base-300 hover:border-base-content"
                                                                    // )
                                                                prop.text pos
                                                            ]
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // // Size (Small / Medium / Large)
                                        Html.div [
                                            prop.className "space-y-3"
                                            prop.children [
                                                Html.label [
                                                    prop.className "text-sm font-medium uppercase tracking-[0.2em]"
                                                    prop.text "Size"
                                                ]
                                                Html.div [
                                                    prop.className "grid grid-cols-3 gap-3"
                                                    prop.children [
                                                        for sizeStr in [ "Small"; "Medium"; "Large" ] do
                                                            let sizeDU = sizeFromString sizeStr
                                                            let isSelected = (pd.Size = sizeDU)
                                                            Html.button [
                                                                prop.key sizeStr
                                                                prop.type'.button
                                                                prop.onClick (fun _ ->
                                                                    dispatch (UpdatePlacedSize (activeIdx, sizeDU))
                                                                )
                                                                prop.className (
                                                                    "py-3 border-2 text-xs md:text-sm font-medium uppercase \
                                                                    tracking-[0.2em] transition-all " +
                                                                    (
                                                                        if isSelected
                                                                        then "border-base-content bg-base-content text-base-100"
                                                                        else "border-base-300 hover:border-base-content"
                                                                    )
                                                                )
                                                                prop.text sizeStr
                                                            ]
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // // Printing Technique
                                        Html.div [
                                            prop.className "space-y-3"
                                            prop.children [
                                                Html.label [
                                                    prop.className "text-sm font-medium uppercase tracking-[0.2em]"
                                                    prop.text "Printing Technique"
                                                ]
                                                Html.div [
                                                    prop.className "grid grid-cols-2 gap-3"
                                                    prop.children [
                                                        for techLabel in [ "DTG"; "Embroidery"; "Screen Print"; "Heat Transfer" ] do
                                                            let techDU = techniqueFromString techLabel
                                                            let isSelected = (pd.Technique = techDU)
                                                            Html.button [
                                                                prop.key techLabel
                                                                prop.type'.button
                                                                prop.onClick (fun _ ->
                                                                    dispatch (UpdatePlacedTechnique (activeIdx, techDU))
                                                                )
                                                                prop.className (
                                                                    "py-3 border-2 text-[0.65rem] md:text-xs font-medium uppercase \
                                                                    tracking-[0.2em] transition-all " +
                                                                    (
                                                                        if isSelected
                                                                        then "border-base-content bg-base-content text-base-100"
                                                                        else "border-base-300 hover:border-base-content"
                                                                    )
                                                                )
                                                                prop.text techLabel
                                                            ]
                                                    ]
                                                ]
                                            ]
                                        ]

                                    ]
                                ]
                            | _ ->
                                Html.none
                    ]
                ]

                // footer nav
                footerNav
                    { IsHidden = false; IsDisabled = false; ButtonLabel = "Back"; ButtonMsg = (GoToStep StepDesigner.SelectVariants) }
                    { IsHidden = false; IsDisabled = designs.IsEmpty; ButtonLabel = "Next"; ButtonMsg = (GoToStep StepDesigner.ReviewDesign) }
                    dispatch

            ]
        ]

    [<ReactComponent>]
    let reviewDesignedProduct model dispatch =
        Html.div [
            prop.className "space-y-6"
            prop.children [
                Html.h2 [
                    prop.className "text-2xl md:text-3xl font-light"
                    prop.text "Review Your Design"
                ]
                Html.div [
                    prop.className "card bg-base-100 border border-base-300"
                    prop.children [
                        Html.div [
                            prop.className "card-body space-y-3 text-sm"
                            prop.children [
                                Html.div [
                                    prop.className "flex justify-between"
                                    prop.children [
                                        Html.span [ prop.className "opacity-60"; prop.text "Product" ]
                                        Html.span [
                                            prop.className "font-medium"
                                            prop.text (model.selectedProduct |> Option.map (fun p -> p.name) |> Option.defaultValue "Not selected")
                                        ]
                                    ]
                                ]
                                Html.div [
                                    prop.className "flex justify-between"
                                    prop.children [
                                        Html.span [ prop.className "opacity-60"; prop.text "Color" ]
                                        Html.span [ prop.text (model.selectedVariantColor |> Option.defaultValue "-") ]
                                    ]
                                ]
                                Html.div [
                                    prop.className "flex justify-between"
                                    prop.children [
                                        Html.span [ prop.className "opacity-60"; prop.text "Size" ]
                                        Html.span [ prop.text (model.selectedVariantSize |> Option.defaultValue "-") ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                footerNav
                    { IsHidden = false; IsDisabled = false; ButtonLabel = "Back"; ButtonMsg = (GoToStep ProductDesigner) }
                    { IsHidden = false; IsDisabled = false; ButtonLabel = "Add to Cart"; ButtonMsg = AddToCart 1 }
                    dispatch
            ]
        ]

    [<ReactComponent>]
    let view model dispatch =     
        Section.container [
            Html.div [
                prop.className "py-10 md:py-20"
                prop.children [

                    stepper model dispatch
                    
                    if model.currentStep = StepDesigner.SelectBaseProduct
                    then
                        Client.Components.Shop.Common.Ui.CatalogHeader.CatalogHeader {
                            Title      = "Choose Base Product"
                            Subtitle   = Some "Pick a blank from the catalog to start designing."
                            TotalCount = model.paging.total
                            Filters    = model.query              // or appropriate filters for designer
                            Paging     = model.paging
                            SearchTerm = if System.String.IsNullOrWhiteSpace model.searchTerm then None else Some model.searchTerm                     // or Some model.searchTerm if you have it
                            OnSearchChanged = (fun term -> dispatch (DesignerSearchChanged term) )
                            // OnSortChanged = (fun sortDetails -> dispatch (DesignerSortChanged sortDetails) )
                            OnFiltersChanged = fun filters -> dispatch (DesignerFiltersChanged filters)
                            OnPageChange = fun newOffset -> dispatch (DesignerPageChanged { model.paging with offset = newOffset })
                        }
                        

                    Html.div [
                        prop.className "grid grid-cols-1 lg:grid-cols-5 gap-10 lg:gap-12"
                        prop.children [

                            // Preview
                            Html.div [
                                prop.className "lg:col-span-3"
                                prop.children [ preview model ]
                            ]

                            // Left content (wizard panels)
                            Html.div [
                                prop.className "lg:col-span-2 space-y-8"
                                prop.children [
                                    match model.currentStep with
                                    | SelectBaseProduct -> catalogProductSelector model dispatch
                                    | SelectVariants -> variantSelection model dispatch
                                    | ProductDesigner -> designStudioView model dispatch
                                    | ReviewDesign -> reviewDesignedProduct model dispatch
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
