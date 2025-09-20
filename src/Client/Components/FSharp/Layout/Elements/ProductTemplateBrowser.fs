module Components.FSharp.Pages.ProductTemplateBrowser

open Feliz
open Feliz.DaisyUI
open Shared.SharedShopDomain
open Shared
open Components.FSharp.Layout.Elements.Pagination

type Props = {
    templates : Api.Printful.ProductTemplate.ProductTemplate list
    total : int
    limit : int
    offset : int
    loadTemplate : int -> unit   // load one by ID
    setPage : int -> unit        // pagination handler
}

[<ReactComponent>]
let ProductTemplateBrowser (props: Props) =
    let (selected, setSelected) = React.useState<Api.Printful.ProductTemplate.ProductTemplate option>(None)

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
                    props.templates
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
                        total = props.total
                        limit = props.limit
                        offset = props.offset
                        onPageChange = props.setPage
                    }
                ]
            ]
        ]