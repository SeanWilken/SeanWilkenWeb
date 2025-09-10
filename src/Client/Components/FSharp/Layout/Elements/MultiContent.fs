module Components.FSharp.Layout.MultiContent

open Feliz
open Shared

type TileContent = {
    Title: string
    Summary: string
    Details: string
    Icon: ReactElement
    Image: string option
}

let multiContentNavCard index selectedIndex setSelectedIndex (content: TileContent) =
    let isActive = selectedIndex = Some index
    Html.div [
        prop.className (
            "card bg-base-100 shadow-md transition-all hover:shadow-xl p-4 text-left h-full " +
            (if isActive then "ring-2 ring-primary scale-[1.02]" else "")
        )
        prop.onClick (fun _ -> setSelectedIndex (Some index))
        prop.children [
            Html.div [
                prop.className "flex items-center gap-3 text-primary mb-2"
                prop.children [
                    content.Icon
                    Html.h2 [ prop.className "clash-font card-title text-xl"; prop.text content.Title ]
                ]
            ]
            Html.p [
                prop.className "satoshi-font text-sm text-base-content/80"
                prop.text content.Summary
            ]
        ]
    ]

let selectedContentDisplay (content: TileContent option) =
    match content with
    | None -> Html.none
    | Some content ->
        Html.div [
            prop.className "mt-12 grid grid-cols-1 md:grid-cols-2 gap-8 items-center bg-base-200 p-8 rounded-xl shadow-inner"
            prop.children [
                // Left side: Image
                match content.Image with
                | Some url ->
                    Html.img [
                        prop.src url
                        prop.alt $"{content.Title} image"
                        prop.className "rounded-lg w-full max-h-64 object-cover border border-base-300 shadow"
                    ]
                | None -> Html.none

                // Right side: Text content
                Html.div [
                    prop.className "space-y-4"
                    prop.children [
                        Html.h2 [
                            prop.className "clash-font text-2xl text-primary"
                            prop.text content.Title
                        ]
                        Html.p [
                            prop.className "satoshi-font text-base text-base-content"
                            prop.text content.Details
                        ]
                    ]
                ]
            ]
        ]
