module Components.FSharp.Portfolio.ArtGallery

open Elmish
open Feliz
open Fable.Core.JsInterop
open Browser.Dom
open Browser
open Client.Domain
open Bindings.LucideIcon

let galleryPieces = [
    "Bowing Bubbles", "jpg", "What's poppin?"
    "Out for Blood", "png", "The hunt is on."
    // "Harlot", "Unholy mother of sin..."
    // "Kcuf Em", "Kcuf me? F!#% you."
    // "BackStabber", "Never saw them comin'."
    "Caution: Very Hot", "jpg", "Ya' might get burnt..."
    // "Models", "I work with them for a living..."
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

let init () = SharedDesignGallery.getInitialModel, Cmd.none

let update (msg: SharedDesignGallery.Msg) (model: SharedDesignGallery.Model) =
    match msg with
    | Client.Domain.SharedDesignGallery.BackToPortfolio -> model, Cmd.none
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
                    prop.src ($"./img/artwork/{fileName}.{extension}")
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
let view (model: SharedDesignGallery.Model) (dispatch: SharedDesignGallery.Msg -> unit) =
    Html.section [
        prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 space-y-8"
        prop.children [

            SharedViewModule.galleryHeaderControls {
                onClose = fun () -> SharedDesignGallery.BackToPortfolio |> dispatch
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
