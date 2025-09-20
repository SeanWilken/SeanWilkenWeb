module Components.FSharp.Layout.ProductBrowser

open Feliz
open Components.FSharp.Layout.Elements
open Shared.SharedShopDomain
open Components.FSharp.Layout.Elements.Pagination
open Components.FSharp.Layout.Elements.ProductGrid

type Props = {
    products: CatalogProduct list
    total: int
    limit: int
    offset: int
    loadProduct: CatalogProduct -> unit
    setPage: int -> unit
}

[<ReactComponent>]
let ProductBrowser (props: Props) =
    Html.div [
        // Filters / Sorting would go here
        Html.div [
            prop.className "mb-6 flex justify-between items-center"
            prop.children [
                Html.h2 [
                    prop.className "text-xl font-semibold"
                    prop.text "Products"
                ]
                Html.div [
                    Html.label [ prop.text "Sort by:" ]
                    Html.select [
                        prop.className "ml-2 select-sm"
                        // prop.onChange (fun ev -> Browser.Dom.console.log($"Sort: {ev}"))
                        prop.children [
                            Html.option [ prop.value "bestselling"; prop.text "Best Selling" ]
                            Html.option [ prop.value "newest"; prop.text "Newest" ]
                            Html.option [ prop.value "price"; prop.text "Price" ]
                        ]
                    ]
                ]
            ]
        ]

        // Product Grid
        ProductGrid props.products props.loadProduct

        // Pagination
        Html.div [
            prop.className "mt-8 flex justify-center"
            prop.children [
                Pagination {
                    total = props.total
                    limit = props.limit
                    offset = props.offset
                    onPageChange = props.setPage
                }
            ]
        ]
    ]