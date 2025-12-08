module Components.FSharp.Pages.ProductTemplateBrowser

open Feliz
open Feliz.DaisyUI
open Client.Domain
open Elmish
open Shared
open SharedShopV2
open SharedShopV2.ProductTemplate.ProductTemplateBrowser
open SharedShopV2Domain
open Components.FSharp.Layout.Elements.Pagination

let getAllProductTemplates (request: Api.Printful.CatalogProductRequest.CatalogProductsQuery) : Cmd<SharedShopV2Domain.ShopProductTemplatesMsg> =
    Cmd.OfAsync.either
        ( fun x -> Client.Api.productsApi.getProductTemplates x )
        request
        GotProductTemplates
        FailedProductTemplates

let update (msg: ShopProductTemplatesMsg) (model: ProductTemplate.ProductTemplateBrowser.Model) : ProductTemplate.ProductTemplateBrowser.Model * Cmd<SharedShopV2Domain.ShopProductTemplatesMsg> =
    match msg with
    | GetProductTemplates ->
        model,  
        Shared.Api.Printful.CatalogProductRequest.toApiQuery
            model.Paging
            model.Filters
        |> getAllProductTemplates

    | GotProductTemplates response ->
        { model with 
            Templates = response.templateItems
            Paging = response.paging
            Error = None
        }, Cmd.none

    | FailedProductTemplates ex ->
        { model with Error = Some ex.Message }, Cmd.none

    | ViewProductTemplate template ->
        { model with SelectedTemplate = Some template }, Cmd.none

    | SwitchSection _ ->
        { model with SelectedTemplate = None }, Cmd.none

    | Next ->
        let newOffset = model.Paging.offset + model.Paging.limit
        { model with Paging = { model.Paging with offset = newOffset } },
        Cmd.ofMsg GetProductTemplates

    | Back ->
        let newOffset = max 0 (model.Paging.offset - model.Paging.limit)
        { model with Paging = { model.Paging with offset = newOffset } },
        Cmd.ofMsg GetProductTemplates
        
    | ChooseVariantSize size ->
        // You may want to track this in the model later
        model, Cmd.none

    | ChooseVariantColor color ->
        // Same idea here
        model, Cmd.none

    | AddProductToBag template ->
        // Hook into your bag/cart domain
        model, Cmd.none

[<ReactComponent>]
let ProductTemplateBrowser (props: ProductTemplate.ProductTemplateBrowser.Model) dispatch =
    let (selected, setSelected) = React.useState<ProductTemplate.ProductTemplate option>(None)
    React.useEffectOnce (
        fun _ ->
            dispatch GetProductTemplates
            // let request =
            //     Shared.Api.Printful.CatalogProductRequest.toApiQuery
            //         model.paging
            //         model.query
            // getAllProducts request |> dispatch 
    )
    match selected with
    | Some template ->
        Html.div [
            prop.className "max-w-5xl mx-auto"
            prop.children [
                Daisy.button.button [
                    prop.className "btn-sm btn-outline mb-6"
                    prop.text "← Back to Templates"
                    prop.onClick (fun _ -> setSelected None)
                ]

                Html.div [
                    prop.className "grid grid-cols-1 md:grid-cols-2 gap-6"
                    prop.children [
                        // Thumbnail
                        Html.img [
                            prop.src template.mockup_file_url
                            prop.alt template.title
                            prop.className "w-full object-cover rounded-lg shadow"
                        ]

                        // Template Info
                        Html.div [
                            Html.h2 [
                                prop.className "text-2xl font-bold mb-2"
                                prop.text template.title
                            ]

                            Html.p [
                                prop.className "opacity-70 mb-4"
                                prop.text $"Template ID: {template.id}"
                            ]

                            // Html.div [
                            //     Html.h3 [ 
                            //         prop.className "font-semibold mb-2"
                            //         prop.text "Options" 
                            //     ]
                            //     Html.ul [
                            //         prop.className "list-disc ml-6 space-y-1"
                            //         prop.children (
                            //             template.option_data
                            //             |> Array.map (fun opt ->
                            //                 Html.li [ prop.text $"{opt.Name}: {opt.Values |> String.concat ", "}" ]
                            //             )
                            //         )
                            //     ]
                            // ]

                            Html.div [
                                Html.h3 [
                                    prop.className "font-semibold mt-4 mb-2"
                                    prop.text "Available Sizes"
                                ]
                                Html.p (String.concat ", " template.sizes)
                            ]

                            Html.div [
                                Html.h3 [
                                    prop.className "font-semibold mt-4 mb-2"
                                    prop.text "Colors"
                                ]
                                Html.div [
                                    prop.className "flex flex-wrap gap-2"
                                    prop.children (
                                        template.colors
                                        |> Array.map (fun c ->
                                            Html.div [
                                                prop.className "w-6 h-6 rounded-full border shadow"
                                                prop.style [ style.backgroundColor (c.color_codes |> Array.head) ]
                                                prop.title c.color_name
                                            ]
                                        )
                                    )
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    | None ->
        Html.div [
            Daisy.button.button [
                prop.className "btn-sm btn-outline mb-6"
                prop.text "← Back to Shop Type"
                prop.onClick (fun _ ->  dispatch (SwitchSection ShopTypeSelector))
            ]

            Html.div [
                prop.className "mb-6 flex justify-between items-center"
                prop.children [
                    Html.h2 [
                        prop.className "text-xl font-semibold"
                        prop.text "Product Templates"
                    ]
                ]
            ]

            // Grid view
            Html.div [
                prop.className "grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
                prop.children (
                    props.Templates
                    |> List.map (fun template ->
                        Daisy.card [
                            prop.className "shadow-md hover:shadow-lg transition-transform hover:scale-[1.02] rounded-lg overflow-hidden"
                            prop.onClick (fun _ -> setSelected (Some template))
                            prop.children [
                                Html.img [
                                    prop.src template.mockup_file_url
                                    prop.alt template.title
                                    prop.className "w-full h-48 object-cover"
                                ]
                                Daisy.cardBody [
                                    Html.h3 [
                                        prop.className "font-semibold truncate"
                                        prop.text template.title
                                    ]
                                    Html.p [
                                        prop.className "text-sm opacity-70"
                                        prop.text $"{template.available_variant_ids.Length} variants"
                                    ]
                                ]
                            ]
                        ]
                    )
                )
            ]

            // Pagination
            Html.div [
                prop.className "mt-8 flex justify-center"
                prop.children [
                    Pagination {
                        total = props.Paging.total
                        limit = props.Paging.limit
                        offset = props.Paging.offset
                        onPageChange = fun i -> printfn "Page change: %i" i
                    }
                ]
            ]
        ]