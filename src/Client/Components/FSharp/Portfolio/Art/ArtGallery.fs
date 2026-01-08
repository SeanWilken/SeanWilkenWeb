module Components.FSharp.Portfolio.ArtGallery

open Elmish
open Feliz
open Fable.Core.JsInterop
open Browser.Dom
open Browser
open Bindings.LucideIcon

let galleryPieces = [
    "Bowing Bubbles", "jpg", "What's poppin?"
    "Out for Blood", "png", "The hunt is on."
    // "Harlot", "Unholy mother of sin..."
    // "Kcuf Em", "Kcuf me? F!#% you."
    // "BackStabber", "Never saw them comin'."
    "Caution: Very Hot", "jpg", "Ya' might get burnt..."
    // "Models", "jpg", "I work with them for a living..."
    // "Storm", "And with a thundering roar, she sang"
    // "Forever Burning", "Till death do us part."
    // "Space Tag", "Catch me round the solar system."
    "Misfortune", "png", "It was never in the cards to begin with."
]

let likeKey = "liked-artworks"

let getInitialLikes () =
    match localStorage.getItem likeKey with
    | null -> Set.empty
    | data -> 
        try 
            data.Split(',') |> Set.ofArray
        with _ -> Set.empty

let saveLikes (likes: Set<string>) =
    localStorage.setItem(likeKey, String.concat "," likes)

type Msg =
    | BackToPortfolio
    | SetCurrentPieceIndex of int

type Model = {
    CurrentPieceIndex: int option
}

let getInitialModel = { CurrentPieceIndex = None }

let init () = getInitialModel, Cmd.none

let update (msg: Msg) (model: Model) =
    match msg with
    | BackToPortfolio -> model, Cmd.none
    | _ -> model, Cmd.none

[<ReactComponent>]
let artCard (title: string, extension: string, description: string, isLiked: bool, onToggle: unit -> unit) =
    let fileName = title.Replace(":", "").Replace(" ", "-").ToLowerInvariant()
    Html.div [
        prop.className
            "relative card bg-base-100 shadow-md hover:shadow-xl hover:ring hover:ring-primary/30 \
             transition duration-300 hover:scale-[1.01]"
        prop.children [
            Html.figure [
                Html.img [
                    prop.src ($"../../img/artwork/{fileName}.{extension}")
                    prop.alt title
                    prop.className "w-full h-64 object-cover rounded-t"
                ]
            ]
            Html.div [
                prop.className "card-body p-4"
                prop.children [
                    Html.h2 [
                        prop.className "card-title text-lg text-primary"
                        prop.text title
                    ]
                    Html.p [
                        prop.className "text-sm text-base-content/80"
                        prop.text description
                    ]
                ]
            ]
            Html.button [
                prop.className "absolute top-3 right-3 btn btn-xs btn-circle btn-ghost bg-base-100/80 backdrop-blur"
                prop.onClick (fun _ -> onToggle())
                prop.children [
                    LucideIcon.Heart (
                        if isLiked then "w-4 h-4 text-red-500 fill-red-500"
                        else "w-4 h-4 text-base-content/70"
                    )
                ]
            ]
        ]
    ]

type RightHeaderIcon = {
    icon: ReactElement
    label: string
    externalLink: string option
    externalAlt: string option
}

type GalleryHeaderProps = {
    onClose: unit -> unit
    rightIcon: RightHeaderIcon option
}

// Gallery header controls with close and external link buttons
let galleryHeaderControls (props: GalleryHeaderProps) =
    Html.div [
        prop.className "flex items-center justify-between mb-8"
        prop.children [
            // Back button
            Html.button [
                prop.className "btn btn-ghost btn-sm gap-2 px-2"
                prop.custom ("data-tip", "Back")
                prop.onClick (fun _ -> props.onClose())
                prop.children [
                    LucideIcon.ChevronLeft "w-5 h-5"
                    Html.span [
                        prop.className "hidden sm:inline text-sm"
                        prop.text "Back to portfolio"
                    ]
                ]
            ]

            // Right icon (Github / Instagram)
            match props.rightIcon with
            | Some icon ->
                Html.a [
                    match icon.externalLink with
                    | Some href -> prop.href href
                    | None -> ()
                    prop.target "_blank"
                    prop.className "btn btn-ghost btn-sm gap-2 hover:bg-base-200"
                    prop.title (icon.externalAlt |> Option.defaultValue icon.label)
                    prop.children [
                        icon.icon
                        Html.span [
                            prop.className "sm:inline text-sm"
                            prop.text icon.label
                        ]
                    ]
                ]
            | None -> Html.div [] // keeps flex spacing happy
        ]
    ]

[<ReactComponent>]
let ArtGalleryGrid () =
    let likes, setLikes = React.useState(getInitialLikes ())

    let toggleLike (title: string) =
        let updated =
            if likes.Contains title then Set.remove title likes
            else Set.add title likes

        saveLikes updated
        setLikes updated

    Html.div [
        prop.className "grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6"
        prop.children [
            for title, ext, desc in galleryPieces do
                artCard (
                    title,
                    ext,
                    desc,
                    likes.Contains title,
                    (fun () -> toggleLike title)
                )
        ]
    ]

[<ReactComponent>]
let view (model: Model) (dispatch: Msg -> unit) =
    Html.section [
        prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 space-y-8"
        prop.children [

            galleryHeaderControls {
                onClose = fun () -> BackToPortfolio |> dispatch
                rightIcon = Some {
                    icon = LucideIcon.Instagram "w-5 h-5"
                    label = "Instagram"
                    externalLink = Some "https://www.instagram.com/xeroeffort/"
                    externalAlt = Some "View artwork on Instagram"
                }
            }

            Html.div [
                prop.className "text-center space-y-2"
                prop.children [
                    Html.h1 [
                        prop.className "text-4xl font-bold text-primary"
                        prop.text "Design Gallery"
                    ]
                    Html.p [
                        prop.className "text-sm text-base-content/70 max-w-xl mx-auto"
                        prop.text "A rotating selection of illustrations, sketches, and experiments in style."
                    ]
                ]
            ]

            ArtGalleryGrid ()
        ]
    ]
