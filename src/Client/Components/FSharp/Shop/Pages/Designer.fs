namespace Client.Components.Shop

open Feliz
open Client.Components.Shop.Common
open Client.Domain.SharedShopV2

module Designer =

    open BuildYourOwnProductWizard
    open Client.Components.Shop.Common.Ui
    open Shared.SharedShopV2.PrintfulCatalog

    type Msg =
        | GoToStep of Step
        | SelectBase of CatalogProduct
        | SelectColor of string
        | SelectSize of string
        | AddDesignPlaceholder
        // ... extend with real design / placement messages later

    type Props = {
        Model   : Model
        Dispatch: Msg -> unit
    }

    let private stepOrder : Step list =
        [ SelectProduct; SelectVariant; SelectDesign; ConfigurePlacement; Review ]

    let private stepLabel = function
        | SelectProduct      -> "Base Product"
        | SelectVariant      -> "Color & Size"
        | SelectDesign       -> "Add Designs"
        | ConfigurePlacement -> "Position"
        | Review             -> "Review"

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
                        Html.div [
                            prop.className "text-[7rem] md:text-[8rem] font-light text-base-content/20"
                            prop.text (model.selectedProduct |> Option.map (fun p -> string p.id) |> Option.defaultValue "?")
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

    let view (props: Props) =
        Section.container [
            Html.div [
                prop.className "py-10 md:py-20"
                prop.children [
                    stepper props.Model props.Dispatch

                    Html.div [
                        prop.className "grid grid-cols-1 lg:grid-cols-5 gap-10 lg:gap-12"
                        prop.children [

                            // Left content (wizard panels)
                            Html.div [
                                prop.className "lg:col-span-2 space-y-8"
                                prop.children [
                                    match props.Model.currentStep with
                                    | SelectProduct ->
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
                                                // Later: map real catalog products, template info, etc.
                                                Html.div [
                                                    prop.className "alert alert-info text-sm"
                                                    prop.text "TODO: Bind to ProductTemplate / CatalogProduct list."
                                                ]
                                            ]
                                        ]

                                    | SelectVariant ->
                                        Html.div [
                                            prop.className "space-y-6"
                                            prop.children [
                                                Html.h2 [
                                                    prop.className "text-2xl md:text-3xl font-light"
                                                    prop.text "Select Color & Size"
                                                ]
                                                Html.div [
                                                    prop.className "alert alert-info text-sm"
                                                    prop.text "TODO: Use ProductTemplate.colors / sizes arrays here."
                                                ]
                                            ]
                                        ]

                                    | SelectDesign ->
                                        Html.div [
                                            prop.className "space-y-6"
                                            prop.children [
                                                Html.h2 [
                                                    prop.className "text-2xl md:text-3xl font-light"
                                                    prop.text "Add Designs"
                                                ]
                                                Html.div [
                                                    prop.className "alert alert-info text-sm"
                                                    prop.text "TODO: Hook into your design library / uploads."
                                                ]
                                            ]
                                        ]

                                    | ConfigurePlacement ->
                                        Html.div [
                                            prop.className "space-y-6"
                                            prop.children [
                                                Html.h2 [
                                                    prop.className "text-2xl md:text-3xl font-light"
                                                    prop.text "Configure Placement"
                                                ]
                                                Html.div [
                                                    prop.className "alert alert-info text-sm"
                                                    prop.text "TODO: Use placement_option_data & placements from ProductTemplate."
                                                ]
                                            ]
                                        ]

                                    | Review ->
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
                                                                            prop.text (props.Model.selectedProduct |> Option.map (fun p -> p.name) |> Option.defaultValue "Not selected")
                                                                        ]
                                                                    ]
                                                                ]
                                                                Html.div [
                                                                    prop.className "flex justify-between"
                                                                    prop.children [
                                                                        Html.span [ prop.className "opacity-60"; prop.text "Color" ]
                                                                        Html.span [ prop.text (props.Model.selectedVariantColor |> Option.defaultValue "-") ]
                                                                    ]
                                                                ]
                                                                Html.div [
                                                                    prop.className "flex justify-between"
                                                                    prop.children [
                                                                        Html.span [ prop.className "opacity-60"; prop.text "Size" ]
                                                                        Html.span [ prop.text (props.Model.selectedVariantSize |> Option.defaultValue "-") ]
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
                                prop.children [ preview props.Model ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
