module Components.FSharp.CreateYourOwnProduct

open Feliz
open Feliz.DaisyUI
open Shared.SharedShopV2Domain
open Shared.SharedShopV2.BuildYourOwnProductWizard
open Elmish

let stepToIndex = function
    | SelectProduct -> 1
    | SelectVariant -> 2
    | SelectDesign -> 3
    | ConfigurePlacement -> 4
    | Review -> 5

let getAllProducts (request: Shared.Api.Printful.CatalogProductRequest.CatalogProductsQuery) : Cmd<ShopBuildYourOwnProductWizardMsg> =
    Cmd.OfAsync.either
        ( fun x -> Client.Api.productsApi.getProducts x )
        request
        ShopBuildYourOwnProductWizardMsg.GotAllProducts
        ShopBuildYourOwnProductWizardMsg.FailedAllProducts

let update (msg: Shared.SharedShopV2Domain.ShopBuildYourOwnProductWizardMsg) (state: Shared.SharedShopV2.BuildYourOwnProductWizard.Model) : Shared.SharedShopV2.BuildYourOwnProductWizard.Model * Cmd<Shared.SharedShopV2Domain.ShopBuildYourOwnProductWizardMsg> =
    match msg with
    | ShopBuildYourOwnProductWizardMsg.SwitchSection section ->
        state, Cmd.none
    | ShopBuildYourOwnProductWizardMsg.Next ->
        match state.currentStep with
        | SelectProduct -> { state with currentStep = SelectVariant }, Cmd.none
        | SelectVariant -> { state with currentStep = SelectDesign }, Cmd.none
        | SelectDesign -> { state with currentStep = ConfigurePlacement }, Cmd.none
        | ConfigurePlacement -> { state with currentStep = Review }, Cmd.none
        | Review -> state, Cmd.none
    | ShopBuildYourOwnProductWizardMsg.Back ->
        match state.currentStep with
        | SelectProduct -> state, Cmd.none
        | SelectVariant -> { state with currentStep = SelectProduct }, Cmd.none
        | SelectDesign -> { state with currentStep = SelectVariant }, Cmd.none
        | ConfigurePlacement -> { state with currentStep = SelectDesign }, Cmd.none
        | Review -> { state with currentStep = ConfigurePlacement }, Cmd.none
    | ShopBuildYourOwnProductWizardMsg.AddProductToBag catalogProduct ->
        state, Cmd.none

    | ShopBuildYourOwnProductWizardMsg.ChooseProduct product ->
        { state with selectedProduct = Some product; currentStep = SelectVariant }
        , Cmd.none

    | ShopBuildYourOwnProductWizardMsg.ChooseVariantSize size ->
        { state with selectedVariantSize = Some size } //; currentStep = SelectDesign }
        , Cmd.none

    | ShopBuildYourOwnProductWizardMsg.ChooseVariantColor color ->
        { state with selectedVariantColor = Some color } // ; currentStep = SelectDesign }
        , Cmd.none

    | ShopBuildYourOwnProductWizardMsg.ChooseDesign designId ->
        { state with selectedDesign = Some designId; currentStep = ConfigurePlacement }
        , Cmd.none

    | ShopBuildYourOwnProductWizardMsg.TogglePlacement (placement, status) ->
        let placements =
            if List.contains (placement, status) state.placements then
                state.placements |> List.except [(placement, status)]
            else
                (placement, status) :: state.placements
        { state with placements = placements }
        , Cmd.none

    | ShopBuildYourOwnProductWizardMsg.GetAllProducts ->
        state,
        Shared.Api.Printful.CatalogProductRequest.toApiQuery
            state.paging
            state.query
        |> getAllProducts
    
    | ShopBuildYourOwnProductWizardMsg.GotAllProducts catalogResponse ->
        { state with products = catalogResponse.products; paging = catalogResponse.paging }
        , Cmd.none

    | ShopBuildYourOwnProductWizardMsg.FailedAllProducts failure ->
        state, Cmd.none

type ColorOption = {
    Name: string
    Hex: string
}

[<ReactComponent>]
let ColorSwatch (color: ColorOption) =
    Daisy.tooltip [
        prop.children [
            Daisy.tooltipContent color.Name
            Html.div [
                prop.className "w-6 h-6 rounded border shadow-sm cursor-pointer"
                prop.style [ style.backgroundColor color.Hex ]
            ]
        ]
    ]

[<ReactComponent>]
let ColorSwatchGroup (colors: ColorOption list) =
    Html.div [
        prop.className "flex flex-wrap gap-2"
        prop.children (
            colors
            |> List.map (fun c -> ColorSwatch c)
        )
    ]

let progressBar (step: int) =
    let steps = 
        [ 
            "Choose Product", SelectProduct
            "Configure", SelectVariant
            "Artwork", SelectDesign
            "Placements", ConfigurePlacement
            "Review", Review 
        ]

    Daisy.progress [
        prop.className "progress progress-primary w-full mb-6"
        prop.value (steps |> List.findIndex (fun (_, s) -> stepToIndex s = step))
        prop.max steps.Length
    ]

let productCard (dispatch: ShopBuildYourOwnProductWizardMsg -> unit) (product: Shared.SharedShopV2.PrintfulCatalog.CatalogProduct) =
    Daisy.card [
        prop.className "w-64 shadow-xl hover:scale-105 transition cursor-pointer"
        prop.onClick (fun _ -> dispatch (ShopBuildYourOwnProductWizardMsg.ChooseProduct product))
        prop.children [
            Html.img [
                prop.src product.thumbnailURL
                prop.alt product.name
                prop.className "h-40 object-contain"
            ]
            Daisy.cardBody [
                Daisy.cardTitle [ Html.text product.name ]
                Html.p (product.brand |> Option.defaultValue "")
                product.colors
                |> List.map (
                    fun color ->
                        System.Console.WriteLine $"COLOR: {color.Color}"
                        System.Console.WriteLine $"COLOR CODE: {color.ColorCodeOpt}"
                        {
                            Name = color.Color
                            Hex = color.ColorCodeOpt |> Option.map (fun c -> c) |> Option.defaultValue ""
                        }
                        // Daisy.badge [ badge.prop.style [ style.backgroundColor (color.ColorCodeOpt |> Option.defaultValue "") ]; prop.text color.Color ]
                        // Html.span [ prop.className "w-4 h-4 rounded"; prop.style [ style.color (color.ColorCodeOpt |> Option.map ( fun ccd -> System.Console.WriteLine $"COLOR CODE: {ccd}"; "#" + ccd ) |> Option.defaultValue "") ] ]// (color.ColorCodeOpt |> Option.map ( fun x -> x + " !important") |> Option.defaultValue "") ]]
                )
                |> ColorSwatchGroup
                // |> React.fragment
            ]
        ]
    ]



// let getAllProductTemplates (request: Shared.Api.Printful.CatalogProductRequest.CatalogProductsQuery) : Cmd<Shared.SharedShop.ShopMsg> =
//     Cmd.OfAsync.either
//         ( fun x -> Client.Api.productsApi.getProductTemplates x )
//         request
//         GotProductTemplates
//         FailedProductTemplates

// let getAllProducts (request: Shared.Api.Printful.CatalogProductRequest.CatalogProductsQuery) : Cmd<Shared.SharedShopV2Domain.ShopBuildYourOwnProductWizardMsg> =
//     Cmd.OfAsync.either
//         ( fun x -> Client.Api.productsApi.getProducts x )
//         request
//         GotAllProducts
//         FailedAllProducts

// let defaultProductsRequest : Shared.Api.Printful.CatalogProductRequest.CatalogProductsQuery = 
//     {
//         category_ids = Some [ 1 ]
//         colors = None
//         destination_country = None
//         limit = None
//         newOnly = None
//         offset = None
//         placements = None
//         selling_region_name = None
//         sort_direction = None
//         sort_type = None
//         techniques = None
//     }


[<ReactComponent>]
let VariantSelectors
    (product: Shared.SharedShopV2.PrintfulCatalog.CatalogProduct)
    (selectedSize: string option)
    (selectedColor: string option)
    (onSizeChange: string -> unit)
    (onColorChange: string -> unit) =

    Html.div [
        prop.className "flex flex-col gap-4"

        prop.children [
            // size select
            Html.div [
                prop.children [
                    Daisy.label [ prop.text "Size" ]
                    Daisy.select [
                        prop.value (selectedSize |> Option.defaultValue "")
                        prop.onChange onSizeChange
                        prop.children [
                            Html.option [ prop.value ""; prop.text "Select size" ]
                            product.sizes
                            |> List.map ( fun s ->
                                Html.option [ prop.value s; prop.text s ]
                            )
                            |> React.fragment
                        ]
                    ]
                ]
            ]

            // color select
            Html.div [
                prop.children [
                    Daisy.label [ prop.text "Color" ]
                    Daisy.select [
                        prop.value (selectedColor |> Option.defaultValue "")
                        prop.onChange onColorChange
                        prop.children [
                            Html.option [ prop.value ""; prop.text "Select color" ]
                            product.colors
                            |> List.map (fun c ->
                                Html.option [ prop.value c.Color; prop.text c.Color ]
                            )
                            |> React.fragment
                        ]
                    ]
                ]
            ]
        ]
    ]

open Feliz
open Feliz.DaisyUI

type Design = {
    id : string
    name : string
    imageUrl : string
}

[<ReactComponent>]
let DesignSelector (designs: Design list) (selectedDesign: string option) (onSelect: string -> unit) =
    Html.div [
        prop.className "grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4"
        prop.children [
            for d in designs ->
                Daisy.card [
                    prop.key d.id
                    prop.className (
                        "cursor-pointer transition-transform hover:scale-105" +
                        (match selectedDesign with
                         | Some id when id = d.id -> " ring-2 ring-primary"
                         | _ -> "")
                    )
                    prop.onClick (fun _ -> onSelect d.id)
                    prop.children [
                        Html.img [
                            prop.src d.imageUrl
                            prop.alt d.name
                            prop.className "w-full h-32 object-cover rounded"
                        ]
                        Daisy.cardBody [
                            prop.className "p-2 text-center"
                            prop.children [
                                Html.p [ prop.className "text-sm font-medium"; prop.text d.name ]
                            ]
                        ]
                    ]
                ]
        ]
    ]

let render (model: Model) dispatch =
    React.useEffectOnce (
        fun _ ->
            dispatch GetAllProducts
            // let request =
            //     Shared.Api.Printful.CatalogProductRequest.toApiQuery
            //         model.paging
            //         model.query
            // getAllProducts request |> dispatch 
    )
    Html.div [
        // Progress bar
        Daisy.progress [
            prop.className "progress progress-primary w-full mb-6"
            prop.value (stepToIndex model.currentStep)
            prop.max 5
        ]

        match model.currentStep with
        | SelectProduct ->
            Html.div [
                prop.className "grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
                prop.children [
                    for p in model.products ->
                        productCard dispatch p
                ]
            ]

        | SelectVariant ->
            Daisy.card [
                Html.h2 "Step 2: Choose size & color"
                match model.selectedProduct with
                | None -> Html.p "No product selected"
                | Some p ->
                    VariantSelectors
                        (p)
                        model.selectedVariantSize
                        model.selectedVariantColor
                        (fun size -> dispatch (ShopBuildYourOwnProductWizardMsg.ChooseVariantSize size))
                        (fun color -> dispatch (ShopBuildYourOwnProductWizardMsg.ChooseVariantColor color))
                // Daisy.select [
                //     prop.onChange (ChooseVariant >> dispatch)
                //     prop.children [
                //         Html.option [ prop.value ""; prop.text "Select variant" ]
                //         Html.option [ prop.value "white-s"; prop.text "White / Small" ]
                //         Html.option [ prop.value "black-m"; prop.text "Black / Medium" ]
                //         Html.option [ prop.value "navy-l"; prop.text "Navy / Large" ]
                //     ]
                // ]
                Html.div [
                    Daisy.button.button [
                        button.outline
                        prop.onClick (fun _ -> dispatch ShopBuildYourOwnProductWizardMsg.Back)
                        prop.text "Back"
                    ]
                    Daisy.button.button [
                        button.primary
                        prop.disabled (model.selectedVariantColor.IsNone || model.selectedVariantSize.IsNone)
                        prop.onClick (fun _ -> dispatch ShopBuildYourOwnProductWizardMsg.Next)
                        prop.text "Next"
                    ]
                ]
            ]

        | SelectDesign ->
            Daisy.card [
                Html.h2 "Step 3: Choose a design"
                DesignSelector
                    [
                        { id = "1"; name = "Bowing Bubbles"; imageUrl = "img/artwork/BowingBubbles.jpg" }
                        { id = "2"; name = "Caution Very Hot Colorless"; imageUrl = "img/artwork/CautionVeryHotColorless.jpg" }
                        { id = "3"; name = "Misfortune"; imageUrl = "/img/artwork/Misfortune.png" }
                        { id = "4"; name = "Out for Blood"; imageUrl = "/img/artwork/Out for Blood.png" }
                    ]
                    model.selectedDesign
                    (fun designId -> dispatch (ChooseDesign designId))
                // Html.div [
                //     Html.img [
                //         prop.src "/images/designs/design1.png"
                //         prop.className "w-32 cursor-pointer hover:scale-105 transition"
                //         prop.onClick (fun _ -> dispatch (ChooseDesign "/images/designs/design1.png"))
                //     ]
                //     Html.img [
                //         prop.src "/images/designs/design2.png"
                //         prop.className "w-32 cursor-pointer hover:scale-105 transition"
                //         prop.onClick (fun _ -> dispatch (ChooseDesign "/images/designs/design2.png"))
                //     ]
                // ]
                Html.div [
                    Daisy.button.button [
                        button.outline
                        prop.onClick (fun _ -> dispatch ShopBuildYourOwnProductWizardMsg.Back)
                        prop.text "Back"
                    ]
                    Daisy.button.button [
                        button.primary
                        prop.disabled (model.selectedDesign.IsNone)
                        prop.onClick (fun _ -> dispatch ShopBuildYourOwnProductWizardMsg.Next)
                        prop.text "Next"
                    ]
                ]
            ]

        | ConfigurePlacement ->
            Daisy.card [
                Html.h2 "Step 4: Configure placement"
                Html.div [
                    Daisy.radio [ 
                        prop.name "placement"; 
                        prop.value "front"; 
                        prop.onChange (fun (e: Browser.Types.Event) -> 
                            TogglePlacement ("front", model.selectedDesign.Value) |> dispatch
                        ) 
                    ]
                    Html.span "Front"
                    Daisy.radio [ prop.name "placement"; prop.value "back"; prop.onChange (fun (e: Browser.Types.Event) -> dispatch (TogglePlacement ("back", model.selectedDesign.Value))) ]
                    Html.span "Back"
                    Daisy.radio [ prop.name "placement"; prop.value "left_chest"; prop.onChange (fun (e: Browser.Types.Event) -> dispatch (TogglePlacement ("left_chest", model.selectedDesign.Value))) ]
                    Html.span "Left Chest"
                ]
                Html.div [
                    Daisy.button.button [
                        button.outline
                        prop.onClick (fun _ -> dispatch ShopBuildYourOwnProductWizardMsg.Back)
                        prop.text "Back"
                    ]
                    Daisy.button.button [
                        button.primary
                        prop.onClick (fun _ -> dispatch ShopBuildYourOwnProductWizardMsg.Next)
                        prop.text "Next"
                    ]
                ]
            ]

        | Review ->
            let defaultStr = "N/A"
            Daisy.card [
                Html.h2 "Step 5: Review your product"
                Html.p $"Product: {model.selectedProduct |> Option.map (fun x -> x.name) |> Option.defaultValue defaultStr}"
                Html.p $"Variant Color: {model.selectedVariantColor |> Option.defaultValue defaultStr}"
                Html.p $"Variant Size: {model.selectedVariantSize |> Option.defaultValue defaultStr}"
                Html.p $"Design: {model.selectedDesign |> Option.defaultValue defaultStr}"
                Html.ul [
                    for (p, url) in model.placements do
                        Html.li $"{p}: {url}"
                ]
                Html.div [
                    Daisy.button.button [
                        button.outline
                        prop.onClick (fun _ -> dispatch ShopBuildYourOwnProductWizardMsg.Back)
                        prop.text "Back"
                    ]
                    // Daisy.button.button [
                    //     button.primary
                    //     prop.text "Add to Cart"
                    // ]
                    Daisy.button.button [
                        prop.text "Add to Bag"
                        prop.className "btn-primary"
                        prop.onClick (fun _ ->
                            match model.selectedProduct, model.selectedVariantColor, model.selectedVariantSize with
                            | Some product, Some color, Some size ->
                                // let built = buildCatalogVariant product variantId model.selectedDesign model.placements
                                 // dispatch (ShopMsg.AddProductToBag built)
                                dispatch (ShopBuildYourOwnProductWizardMsg.AddProductToBag product)
                            | _ -> ()
                        )
                    ]
                ]
            ]
    ]