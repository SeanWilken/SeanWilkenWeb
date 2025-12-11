namespace Client.Components.Shop

open Feliz
open Client.Components.Shop.Common
open Client.Domain.Store
open Elmish

module Designer =

    // open BuildYourOwnProductWizard
    open Client.Components.Shop.Common.Ui
    open Shared.SharedShopV2.PrintfulCatalog
    open Client.Domain.Store.ProductDesigner
    open Bindings.LucideIcon

    type Props = {
        Model   : Model
        Dispatch: Msg -> unit
    }

    let private stepOrder : StepDesigner list =
        [ SelectBaseProduct; SelectVariants; SelectCustomDesign; ConfigureDesignPlacement; ReviewDesign ]

    let private stepLabel = function
        | SelectBaseProduct      -> "Base Product"
        | SelectVariants      -> "Color & Size"
        | SelectCustomDesign       -> "Add Designs"
        | ConfigureDesignPlacement -> "Position"
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

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | GoToStep step ->
            { model with currentStep = step }, Cmd.none
        | SelectBase catalogProduct ->
            { model with selectedProduct = Some catalogProduct }, Cmd.none
        | SelectColor color ->
            { model with selectedVariantColor = Some color }, Cmd.none
        | SelectSize size ->
            { model with selectedVariantSize = Some size }, Cmd.none
        | AddDesign design ->
            let unplaced : Designs.AppliedDesign = {
                Design = design
                Placement = Designs.defaultPlacement
                Size = ""
            }
            { model with selectedDesigns = model.selectedDesigns @ [ unplaced ] }, Cmd.none
        | RemoveDesign designId ->
            let designs = model.selectedDesigns |> List.filter ( fun x -> x.Design.Id <> designId )
            { model with selectedDesigns = designs }, Cmd.none

        | UpdateDesignPlacement (idx, placement) ->
            let updated =
                model.selectedDesigns
                |> List.mapi (fun i d ->
                // type Placement = {
                //     Id          : PlacementId   // internal key, e.g. "front", "back", "sleeve_left"
                //     Label       : string        // UI label, e.g. "Front", "Back", "Left Sleeve"
                //     Description : string option // optional helper text
                //     Category    : PlacementCategory
                //     IsDefault   : bool          // good candidate when first selecting
                // }
                    if i = idx then { d with Placement = { d.Placement with Id = placement; Label = placement } }
                    else d
                )
            { model with selectedDesigns = updated }, Cmd.none

        | UpdateDesignSize (idx, size) ->
            let updated =
                model.selectedDesigns
                |> List.mapi (fun i d ->
                    if i = idx then { d with Size = size }
                    else d
                )
            { model with selectedDesigns = updated }, Cmd.none
        | LoadProducts -> 
            model, loadProductsCmd model
        | ProductsLoaded catalogProductsResponse ->
            { model with products = catalogProductsResponse.products; paging = catalogProductsResponse.paging }, Cmd.none
        | LoadFailed failureMsg ->
            printfn $"Failed API Load: {failureMsg}"
            model, Cmd.none
        | ViewProduct catalogProduct ->
            model, Cmd.none

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

    let view model dispatch =        
        Section.container [
            Html.div [
                prop.className "py-10 md:py-20"
                prop.children [

                    stepper model dispatch

                    Html.div [
                        prop.className "grid grid-cols-1 lg:grid-cols-5 gap-10 lg:gap-12"
                        prop.children [

                            // Left content (wizard panels)
                            Html.div [
                                prop.className "lg:col-span-2 space-y-8"
                                prop.children [
                                    match model.currentStep with
                                    | SelectBaseProduct ->
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
                                                Html.div [
                                                    prop.className "text-center pt-6"
                                                    prop.children [
                                                        Html.button [
                                                            prop.className (Btn.outline [ "px-10 py-3 rounded-none" ])
                                                            prop.text "Load More Products"
                                                            prop.onClick (fun _ -> dispatch LoadProducts)
                                                        ]
                                                    ]
                                                ]
                                                match model.selectedProduct with
                                                | None -> Html.none
                                                | Some sp ->
                                                    Html.div [
                                                        prop.className "text-center pt-6"
                                                        prop.children [
                                                            Html.button [
                                                                prop.className (Btn.outline [ "px-10 py-3 rounded-none" ])
                                                                prop.text "Next"
                                                                prop.onClick (fun _ -> dispatch (GoToStep StepDesigner.SelectVariants))
                                                            ]
                                                        ]
                                                    ]
                                            ]
                                        ]

                                    | SelectVariants ->
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
                                                                        prop.className "grid grid-cols-3 gap-3"
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
                                                            Html.div [
                                                                prop.className "flex gap-3 pt-4"
                                                                prop.children [
                                                                    Html.button [
                                                                        prop.type' "button"
                                                                        prop.className "flex-1 border border-base-content py-3 md:py-4 text-sm font-medium uppercase tracking-wider hover:bg-base-200/60 transition-colors"
                                                                        prop.text "Back"
                                                                        prop.onClick (fun _ -> GoToStep SelectBaseProduct |> dispatch )
                                                                    ]
                                                                    Html.button [
                                                                        prop.type' "button"
                                                                        let canContinue =
                                                                            model.selectedVariantColor.IsSome
                                                                            && model.selectedVariantSize.IsSome

                                                                        prop.disabled (not canContinue)
                                                                        prop.className (
                                                                            "flex-1 py-3 md:py-4 text-sm font-medium uppercase tracking-wider transition-colors " +
                                                                            (if canContinue then
                                                                                "bg-base-content text-base-100 hover:bg-base-content/90"
                                                                            else
                                                                                "bg-base-300 text-base-content/50 cursor-not-allowed")
                                                                        )
                                                                        prop.text "Add Designs"
                                                                        prop.onClick (fun _ ->
                                                                            if canContinue then
                                                                                GoToStep SelectCustomDesign |> dispatch
                                                                        )
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                            ]
                                        ]

                                    | SelectCustomDesign ->
                                        Html.div [
                                            prop.className "space-y-6"
                                            prop.children [

                                                // header row: title + count
                                                Html.div [
                                                    prop.className "flex items-center justify-between"
                                                    prop.children [
                                                        Html.h2 [
                                                            prop.className "text-2xl md:text-3xl font-light"
                                                            prop.text "Add Designs"
                                                        ]
                                                        Html.span [
                                                            prop.className "text-sm text-base-content/60"
                                                            prop.text (sprintf "%d added" model.selectedDesigns.Length)
                                                        ]
                                                    ]
                                                ]

                                                // design grid (mock designs for now)
                                                Html.div [
                                                    prop.className "grid grid-cols-2 gap-4"
                                                    prop.children [
                                                        for d in Designs.active do
                                                            Html.button [
                                                                prop.key d.Id
                                                                prop.type'.button
                                                                prop.onClick (fun _ -> dispatch (AddDesign d))
                                                                prop.className
                                                                    "border-2 border-base-300 p-4 cursor-pointer \
                                                                    hover:border-base-content transition-all group text-left rounded-xl bg-base-100"
                                                                prop.children [

                                                                    // “artwork” placeholder square
                                                                    Html.div [
                                                                        prop.className
                                                                            "aspect-square bg-base-200 mb-3 flex items-center justify-center \
                                                                            text-4xl font-light text-base-content/20 group-hover:scale-110 \
                                                                            transition-transform rounded-lg"
                                                                        prop.text d.Id
                                                                    ]

                                                                    Html.p [
                                                                        prop.className "text-sm font-medium"
                                                                        prop.text d.Name
                                                                    ]

                                                                    Html.p [
                                                                        prop.className "text-xs text-base-content/60"
                                                                        prop.text (d.Tagline |> Option.defaultValue "Artwork design")
                                                                    ]
                                                                ]
                                                            ]
                                                    ]
                                                ]

                                                // added designs summary
                                                if not model.selectedDesigns.IsEmpty then
                                                    Html.div [
                                                        prop.className "border-t pt-6 space-y-3"
                                                        prop.children [
                                                            Html.p [
                                                                prop.className "text-sm font-medium uppercase tracking-[0.2em]"
                                                                prop.text "Added designs"
                                                            ]

                                                            for idx, d in model.selectedDesigns |> List.indexed do
                                                                Html.div [
                                                                    prop.key (sprintf "%s-%d" d.Design.Id idx)
                                                                    prop.className "flex items-center justify-between p-3 bg-base-200/40 rounded-lg"
                                                                    prop.children [
                                                                        Html.span [
                                                                            prop.className "text-sm"
                                                                            prop.text d.Design.Name
                                                                        ]
                                                                        Html.button [
                                                                            prop.type'.button
                                                                            prop.onClick (fun _ -> dispatch (RemoveDesign d.Design.Id))
                                                                            prop.className "text-base-content/40 hover:text-base-content transition-colors"
                                                                            prop.children [
                                                                                LucideIcon.X "w-4 h-4"
                                                                            ]
                                                                        ]
                                                                    ]
                                                                ]
                                                        ]
                                                    ]
                                                else
                                                    Html.none

                                                // footer actions
                                                Html.div [
                                                    prop.className "flex gap-3 pt-4"
                                                    prop.children [
                                                        Html.button [
                                                            prop.type'.button
                                                            prop.onClick (fun _ -> dispatch (GoToStep StepDesigner.SelectVariants))
                                                            prop.className
                                                                "flex-1 border border-base-content py-3 md:py-4 text-xs md:text-sm font-medium \
                                                                uppercase tracking-[0.2em] hover:bg-base-200/60 transition-colors rounded-full"
                                                            prop.text "Back"
                                                        ]
                                                        Html.button [
                                                            prop.type'.button
                                                            let disabled = model.selectedDesigns.IsEmpty
                                                            prop.disabled disabled
                                                            prop.onClick (fun _ ->
                                                                if not disabled then
                                                                    dispatch (GoToStep StepDesigner.ConfigureDesignPlacement)
                                                            )
                                                            prop.className (
                                                                "flex-1 py-3 md:py-4 text-xs md:text-sm font-medium uppercase tracking-[0.2em] \
                                                                rounded-full transition-colors " +
                                                                (if disabled
                                                                then "bg-base-300 text-base-content/50 cursor-not-allowed"
                                                                else "bg-base-content text-base-100 hover:bg-base-content/90")
                                                            )
                                                            prop.text "Position designs"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]

                                    // inside Designer.view, in the match:
                                    | ConfigureDesignPlacement ->
                                        let activeIdx = model.activeDesignIndex
                                        let activeDesignOpt =
                                            match activeIdx with
                                            | None -> None
                                            | Some idx -> model.selectedDesigns |> List.tryItem idx

                                        Html.div [
                                            prop.className "space-y-6"
                                            prop.children [

                                                Html.h2 [
                                                    prop.className "text-2xl md:text-3xl font-light"
                                                    prop.text "Position Designs"
                                                ]

                                                Html.div [
                                                    prop.className "space-y-4"
                                                    prop.children [

                                                        // Active design label
                                                        Html.label [
                                                            prop.className "text-sm font-medium uppercase tracking-[0.2em] flex justify-between"
                                                            prop.children [
                                                                Html.span [ prop.text "Active Design" ]
                                                                Html.span [
                                                                    prop.className "opacity-70"
                                                                    prop.text (
                                                                        activeDesignOpt
                                                                        |> Option.map (fun ad -> ad.Design.Name)
                                                                        |> Option.defaultValue "Select a design"
                                                                    )
                                                                ]
                                                            ]
                                                        ]

                                                        // Design list (like layers)
                                                        Html.div [
                                                            prop.className "space-y-3"
                                                            prop.children [
                                                                for idx, ad in model.selectedDesigns |> List.indexed do
                                                                    let isActive = Some idx = activeIdx
                                                                    Html.button [
                                                                        prop.key (sprintf "%s-%d" ad.Design.Id idx)
                                                                        prop.type'.button
                                                                        prop.onClick (fun _ -> dispatch (SetActiveDesign idx))
                                                                        prop.className (
                                                                            "w-full text-left p-4 border-2 rounded-xl transition-all flex flex-col gap-1 " +
                                                                            (if isActive
                                                                            then "border-base-content bg-base-200/60"
                                                                            else "border-base-300 hover:border-base-content/60 bg-base-100")
                                                                        )
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.className "flex items-center justify-between"
                                                                                prop.children [
                                                                                    Html.span [
                                                                                        prop.className "font-medium text-sm"
                                                                                        prop.text ad.Design.Name
                                                                                    ]
                                                                                    LucideIcon.Layers "w-4 h-4 opacity-60"
                                                                                ]
                                                                            ]
                                                                            Html.div [
                                                                                prop.className "text-[11px] text-base-content/60"
                                                                                prop.text (sprintf "%s • %s" (string ad.Placement.Category) ad.Size)
                                                                            ]
                                                                        ]
                                                                    ]
                                                            ]
                                                        ]

                                                        match activeIdx, activeDesignOpt with
                                                        | Some idx, Some ad ->

                                                            Html.div [
                                                                prop.className "border-t pt-6 space-y-4"
                                                                prop.children [

                                                                    // Placement selector
                                                                    Html.div [
                                                                        prop.className "space-y-3"
                                                                        prop.children [
                                                                            Html.label [
                                                                                prop.className "text-sm font-medium uppercase tracking-[0.2em]"
                                                                                prop.text "Placement"
                                                                            ]
                                                                            Html.select [
                                                                                prop.className
                                                                                    "w-full px-4 py-3 border border-base-300 text-sm font-medium \
                                                                                    rounded-lg bg-base-100"
                                                                                prop.value (string ad.Placement.Category)
                                                                                prop.onChange (fun (value: string) ->
                                                                                    dispatch (UpdateDesignPlacement (idx, value))
                                                                                )
                                                                                prop.children [
                                                                                    for p in model.placements do
                                                                                        Html.option [
                                                                                            prop.key p.Label
                                                                                            prop.value p.Label
                                                                                            prop.text p.Label
                                                                                        ]
                                                                                ]
                                                                            ]
                                                                        ]
                                                                    ]

                                                                    // Size buttons
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
                                                                                    for size in [ "Small"; "Medium"; "Large" ] do
                                                                                        let isSelected = ad.Size = size
                                                                                        Html.button [
                                                                                            prop.key size
                                                                                            prop.type'.button
                                                                                            prop.onClick (fun _ ->
                                                                                                dispatch (UpdateDesignSize (idx, size))
                                                                                            )
                                                                                            prop.className (
                                                                                                "py-3 border-2 text-xs md:text-sm font-medium uppercase \
                                                                                                tracking-[0.2em] rounded-full transition-all " +
                                                                                                (if isSelected
                                                                                                then "border-base-content bg-base-content text-base-100"
                                                                                                else "border-base-300 hover:border-base-content")
                                                                                            )
                                                                                            prop.text size
                                                                                        ]
                                                                                ]
                                                                            ]
                                                                        ]
                                                                    ]

                                                                    // Move / rotate / scale stubs
                                                                    Html.div [
                                                                        prop.className "flex gap-3"
                                                                        prop.children [
                                                                            Html.button [
                                                                                prop.type'.button
                                                                                prop.className
                                                                                    "flex-1 flex items-center justify-center gap-2 py-3 border \
                                                                                    border-base-300 text-xs md:text-sm font-medium rounded-full \
                                                                                    hover:border-base-content transition-colors"
                                                                                prop.children [
                                                                                    LucideIcon.Move "w-4 h-4"
                                                                                    Html.span [ prop.text "Move" ]
                                                                                ]
                                                                            ]
                                                                            Html.button [
                                                                                prop.type'.button
                                                                                prop.className
                                                                                    "flex-1 flex items-center justify-center gap-2 py-3 border \
                                                                                    border-base-300 text-xs md:text-sm font-medium rounded-full \
                                                                                    hover:border-base-content transition-colors"
                                                                                prop.children [
                                                                                    LucideIcon.RotateCw "w-4 h-4"
                                                                                    Html.span [ prop.text "Rotate" ]
                                                                                ]
                                                                            ]
                                                                            Html.button [
                                                                                prop.type'.button
                                                                                prop.className
                                                                                    "flex-1 flex items-center justify-center gap-2 py-3 border \
                                                                                    border-base-300 text-xs md:text-sm font-medium rounded-full \
                                                                                    hover:border-base-content transition-colors"
                                                                                prop.children [
                                                                                    LucideIcon.ZoomIn "w-4 h-4"
                                                                                    Html.span [ prop.text "Scale" ]
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

                                                // Footer nav
                                                Html.div [
                                                    prop.className "flex gap-3 pt-4"
                                                    prop.children [
                                                        Html.button [
                                                            prop.type'.button
                                                            prop.onClick (fun _ -> dispatch (GoToStep StepDesigner.SelectCustomDesign))
                                                            prop.className
                                                                "flex-1 border border-base-content py-3 md:py-4 text-xs md:text-sm font-medium \
                                                                uppercase tracking-[0.2em] hover:bg-base-200/60 transition-colors rounded-full"
                                                            prop.text "Back"
                                                        ]
                                                        Html.button [
                                                            prop.type'.button
                                                            prop.onClick (fun _ -> dispatch (GoToStep StepDesigner.ReviewDesign))
                                                            prop.className
                                                                "flex-1 bg-base-content text-base-100 py-3 md:py-4 text-xs md:text-sm font-medium \
                                                                uppercase tracking-[0.2em] hover:bg-base-content/90 transition-colors rounded-full"
                                                            prop.text "Review product"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]

                                    | ReviewDesign ->
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
                                            ]
                                        ]
                                ]
                            ]

                            // Preview
                            Html.div [
                                prop.className "lg:col-span-3"
                                prop.children [ preview model ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
