module Components.FSharp.Portfolio.ArtGallery

open Elmish
open Feliz
open Fable.Core.JsInterop
open Browser.Dom
open Browser
open Shared
open Bindings.LucideIcon

let galleryPieces = [
    // "Bowing Bubbles", "What's poppin?"
    // "Out for Blood", "The hunt is on."
    // "Harlot", "Unholy mother of sin..."
    // "Kcuf Em", "Kcuf me? F!#% you."
    // "BackStabber", "Never saw them comin'."
    // "Caution Very Hot", "Ya' might get burnt..."
    // "Models", "I work with them for a living..."
    // "Storm", "And with a thundering roar, she sang"
    // "Forever Burning", "Till death do us part."
    // "Space Tag", "Catch me round the solar system."
    "Misfortune", "It was never in the cards to begin with."
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
    | Shared.SharedDesignGallery.BackToPortfolio -> model, Cmd.none
    | _ -> model, Cmd.none


// let headerControls dispatch =
//     Html.div [
//         prop.className "flex justify-between items-center mb-8"
//         prop.children [
//             Html.button [
//                 prop.className "btn btn-ghost"
//                 prop.onClick (fun _ -> dispatch SharedDesignGallery.BackToPortfolio)
//                 prop.children [
//                     LucideIcon.ChevronLeft "w-6 h-6"
//                     Html.span "Back"
//                 ]
//             ]
//             Html.a [
//                 prop.href "https://www.instagram.com/xeroeffort/"
//                 prop.target "_blank"
//                 prop.className "btn btn-ghost"
//                 prop.children [
//                     LucideIcon.Instagram "w-6 h-6"
//                     Html.span "Instagram"
//                 ]
//             ]
//         ]
//     ]

[<ReactComponent>]
let artCard (title: string, description: string) =
    let likes, setLikes = React.useState(getInitialLikes ())

    let isLiked = likes.Contains title
    let toggleLike _ =
        let updated =
            if isLiked then Set.remove title likes
            else Set.add title likes
        saveLikes updated
        setLikes updated

    Html.div [
        prop.className "relative card bg-base-100 shadow-md hover:shadow-xl hover:ring hover:ring-primary/30 transition duration-300 hover:scale-[1.01]"
        prop.children [
            Html.figure [
                prop.children [
                    Html.img [
                        prop.src ($"/img/artwork/{title}.jpeg")
                        prop.alt title
                        prop.className "w-full h-64 object-cover rounded-t"
                    ]
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
                prop.className "absolute top-2 right-2 btn btn-sm btn-circle btn-ghost"
                prop.onClick toggleLike
                prop.children [
                    Html.i [
                        prop.className (if isLiked then "text-red-500 ri-heart-fill" else "text-base-content ri-heart-line")
                        prop.style [ style.fontSize 20 ]
                    ]
                ]
            ]
        ]
    ]

let galleryGrid () =
    Html.div [
        prop.className "grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6"
        prop.children [
            for i in 0 .. galleryPieces.Length - 1 do
                let title, desc = fst galleryPieces[i], snd galleryPieces[i]
                artCard (title, desc)
        ]
    ]

let view (model: SharedDesignGallery.Model) (dispatch: SharedDesignGallery.Msg -> unit) =
    Html.div [
        prop.className "max-w-7xl mx-auto px-6 py-12"
        prop.children [
            // headerControls dispatch
            SharedViewModule.galleryHeaderControls {
                onClose = fun () -> SharedDesignGallery.BackToPortfolio |> dispatch
                rightIcon = Some (
                    {
                        icon = Bindings.LucideIcon.LucideIcon.Instagram "w-6 h-6"
                        label = "Instagram"
                        externalLink = Some "https://www.instagram.com/xeroeffort/"
                        externalAlt = Some "Go to Instagram"
                    }
                )
            }
            Html.h1 [
                prop.className "text-4xl font-bold text-center text-primary mb-10"
                prop.text "Design Gallery"
            ]
            galleryGrid ()
        ]
    ]