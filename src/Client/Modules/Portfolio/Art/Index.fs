module ArtGallery

open Elmish
open Feliz
open Shared


let galleryPieces = [
    "Bowing Bubbles", "What's poppin?"
    "Out for Blood", "The hunt is on."
    "Harlot", "Unholy mother of sin..."
    "Kcuf Em", "Kcuf me? F!#% you."
    "Misfortune", "It was never in the cards to begin with."
    "BackStabber", "Never saw them comin'."
    "Caution Very Hot", "Ya' might get burnt..."
]

let init () =
    SharedDesignGallery.getInitialModel, Cmd.none

let update (msg: SharedDesignGallery.Msg) (model: SharedDesignGallery.Model) =
    match msg with
    | SharedDesignGallery.SetCurrentPieceIndex offset ->
        let newIndex =
            let next = model.CurrentPieceIndex + offset
            if next < 0 then 0
            elif next >= galleryPieces.Length then model.CurrentPieceIndex
            else next
        { model with CurrentPieceIndex = newIndex }, Cmd.none
    | SharedDesignGallery.BackToPortfolio -> model, Cmd.none


let getGalleryCardByIndex index =
    galleryPieces.[index]

let headerControls dispatch =
    Html.div [
        prop.className "flex justify-between items-center mb-4"
        prop.children [
            Html.button [
                prop.className "btn btn-square btn-outline"
                prop.onClick (fun _ -> dispatch SharedDesignGallery.BackToPortfolio)
                prop.children [
                    Html.img [ prop.src "/imgs/icons/X-it.png"; prop.className "w-10 h-10" ]
                ]
            ]
            Html.a [
                prop.href "https://www.instagram.com/xeroeffort/"
                prop.target "_blank"
                prop.className "btn btn-ghost"
                prop.children [
                    Html.img [ prop.src "/imgs/icons/IG.png"; prop.className "w-10 h-10" ]
                ]
            ]
        ]
    ]

let galleryContent (piece: string) (description: string) =
    Html.div [
        prop.className "flex flex-col items-center text-center gap-4"
        prop.children [
            Html.h1 [ 
                prop.className "text-3xl font-bold text-primary"
                prop.text piece 
            ]
            Html.img [
                prop.src ($"/imgs/{piece}.jpeg")
                prop.className "rounded-lg shadow-md max-w-md w-full"
            ]
            Html.p [ 
                prop.className "text-base text-base-content"
                prop.text description 
            ]
        ]
    ]

let desktopNav dispatch =
    Html.div [
        prop.className "hidden md:flex justify-between w-full mt-8"
        prop.children [
            Html.button [
                prop.className "btn btn-outline"
                prop.onClick (fun _ -> dispatch (SharedDesignGallery.SetCurrentPieceIndex -1))
                prop.text "← PREV"
            ]
            Html.button [
                prop.className "btn btn-outline"
                prop.onClick (fun _ -> dispatch (SharedDesignGallery.SetCurrentPieceIndex 1))
                prop.text "NEXT →"
            ]
        ]
    ]

let mobileNav dispatch =
    Html.div [
        prop.className "flex md:hidden justify-between w-full mt-6"
        prop.children [
            Html.button [
                prop.onClick (fun _ -> dispatch (SharedDesignGallery.SetCurrentPieceIndex -1))
                prop.className "btn btn-square"
                prop.children [ Html.img [ prop.src "/imgs/icons/LeftNavButton.png"; prop.className "w-10" ] ]
            ]
            Html.button [
                prop.onClick (fun _ -> dispatch (SharedDesignGallery.SetCurrentPieceIndex 1))
                prop.className "btn btn-square"
                prop.children [ Html.img [ prop.src "/imgs/icons/RightNavButton.png"; prop.className "w-10" ] ]
            ]
        ]
    ]

let view (model: SharedDesignGallery.Model) (dispatch: SharedDesignGallery.Msg -> unit) =
    let piece, description = getGalleryCardByIndex model.CurrentPieceIndex

    Html.div [
        prop.className "max-w-3xl mx-auto px-4 py-6 flex flex-col items-center"
        prop.children [
            headerControls dispatch
            galleryContent piece description
            desktopNav dispatch
            mobileNav dispatch
        ]
    ]
