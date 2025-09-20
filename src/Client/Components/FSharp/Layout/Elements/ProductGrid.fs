module Components.FSharp.Layout.Elements.ProductGrid

open Feliz
// open Feliz.DaisyUI
open Components.FSharp.Layout.Elements.ProductCard

[<ReactComponent>]
let ProductGrid (products: Shared.SharedShopV2.PrintfulCatalog.CatalogProduct list) loadProductCallback =
    Html.div [
        prop.className "grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
        prop.children (
            products
            |> List.map (fun product ->
                ProductCard {
                    product = product
                    onClick = fun p -> 
                        Browser.Dom.console.log($"Clicked on: {p.id}")
                        loadProductCallback p
                }
            )
        )
    ]
