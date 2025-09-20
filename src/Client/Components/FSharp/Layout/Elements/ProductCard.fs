module Components.FSharp.Layout.Elements.ProductCard

open Feliz
open Feliz.DaisyUI
open Fable.Core.JsInterop
open Shared.SharedShopDomain

type Props = {
    product: CatalogProduct
    onClick: CatalogProduct -> unit
}

[<ReactComponent>]
let ProductCard (props: Props) =
    let product = props.product

    Daisy.card [
        prop.className "shadow-md transition-transform hover:scale-[1.02] hover:shadow-lg rounded-lg overflow-hidden"
        prop.children [
            // Thumbnail Image
            Html.img [
                prop.src product.thumbnailURL
                prop.alt product.name
                prop.className "w-full h-48 object-cover"
            ]

            // Card Body
            Daisy.cardBody [
                prop.className "bg-base-100 text-base-content px-4 py-3"
                prop.children [
                    Html.h3 [
                        prop.className "text-lg font-semibold truncate"
                        prop.text product.name
                    ]

                    Html.p [
                        prop.className "text-sm opacity-70"
                        prop.text $"{product.variantCount} variants"
                    ]

                    Daisy.button.button [
                        prop.text "View Details"
                        prop.className "btn-sm btn-outline btn-primary mt-2"
                        prop.onClick (fun _ -> props.onClick product)
                    ]
                ]
            ]
        ]
    ]
