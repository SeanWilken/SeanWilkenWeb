module SharedViewModule

open Feliz
open Browser

module GamePieceIcons =
    open Shared.GridGame

    let blocker = "🧱"
    let ball = "⚽"
    let goalFlag = "🚩"
    let heart = "❤️"
    let lock = "🔒"
    let key = "🔑"
    let bomb = "💣"
    let upArrow = "⬆️"
    let downArrow = "⬇️"
    let leftArrow = "⬅️"
    let rightArrow = "➡️"
    let empty = ""

    let directionArrowImage direction =
        match direction with
        | MovementDirection.Up -> upArrow
        | MovementDirection.Down -> downArrow
        | MovementDirection.Left -> leftArrow
        | MovementDirection.Right -> rightArrow

// Back button with image, clickable dispatch
let backToGallery (dispatchMsg: unit -> unit) (dispatch: _ -> unit) =
    Html.div [
        prop.children [
            Html.a [
                prop.className "relativeBackButton cursor-pointer"
                prop.onClick (fun _ -> dispatchMsg () |> dispatch)
                prop.children [
                    Html.img [
                        prop.src "./imgs/icons/X-it.png"
                        prop.className "w-12 h-12"
                        prop.alt "Close"
                    ]
                ]
            ]
        ]
    ]

// Navigation button with letters spaced out in columns
let bigNavButton (clickFunc: unit -> unit) (name: string) (dispatch: _ -> unit) =
    Html.a [
        prop.onClick (fun _ -> clickFunc () |> dispatch)
        prop.className "bigSectionNavigationButton cursor-pointer"
        prop.children [
            Html.div [
                prop.className "flex space-x-2 justify-center items-center text-xl font-semibold"
                prop.children (
                    name
                    |> Seq.map (fun c -> Html.span [ prop.text (string c) ])
                    |> Seq.toList
                )
            ]
        ]
    ]

// Simple flat button for switching section
let sharedSwitchSectionButton msg (buttonString: string) dispatch =
    Html.button [
        prop.className "btn btn-ghost"
        prop.text buttonString
        prop.onClick (fun _ -> msg |> dispatch)
    ]

// Modal content wrapper for games
let gameModalContent content =
    Html.div [
        prop.className "gameGridContainer"
        prop.children [ content ]
    ]

// Modal header with close button and title
let sharedModalHeader (gameTitle: string) msg dispatch =
    Html.div [
        prop.className "flex justify-between items-center p-3 border-b border-gray-300"
        prop.children [
            Html.div [] // Placeholder for possible left-side content

            Html.div [
                prop.className "headerTitle text-xl font-bold"
                prop.children [ Html.h1 [ prop.text gameTitle ] ]
            ]

            Html.a [
                prop.className "cursor-pointer"
                prop.onClick (fun _ -> msg |> dispatch)
                prop.children [
                    Html.img [
                        prop.src "./imgs/icons/X-it.png"
                        prop.className "w-16 h-16"
                        prop.alt "Close"
                    ]
                ]
            ]
        ]
    ]

// Helper for code modal control selections
let codeModalControlSelections controlActions dispatch =
    Html.div [
        prop.children (
            controlActions
            |> List.map (fun (label: string, msg) ->
                Html.div [
                    prop.children [
                        Html.a [
                            prop.className "cursor-pointer text-primary hover:underline"
                            prop.text label
                            prop.onClick (fun _ -> msg |> dispatch)
                        ]
                    ]
                ]
            )
        )
    ]

// Modal controls content container
let codeModalControlsContent controlList dispatch =
    Html.div [
        prop.className "modalAltContent p-4"
        prop.children [
            codeModalControlSelections controlList dispatch
        ]
    ]

// Round complete content card
let roundCompleteContent (gameStatDetails: string list) =
    Html.div [
        prop.className "levelCompletedCard p-6 bg-base-200 rounded-lg shadow-md text-white"
        prop.children [
            Html.div [ prop.className "mb-2"; prop.text "Round Over!" ]
            Html.div [
                prop.className "text-xl font-semibold mb-2"
                prop.text "Details:"
            ]
            yield! gameStatDetails |> List.map (fun stat ->
                Html.p [ prop.className "mb-1"; prop.text stat ]
            )
        ]
    ]

let stopGameLoop (loopId: float) = window.clearInterval loopId

let gameTickClock (ticks: int) = string (ticks / 4)

// Game instructions content for code modal
let modalInstructionContent instructionList =
    Html.div [
        prop.className "modalAltContent p-4"
        prop.children (
            instructionList |> List.map (fun (instr: string) -> Html.p [ prop.text instr ])
        )
    ]


// Footer buttons for code modal controls
let codeModalFooter controlList dispatch =
    Html.div [
        prop.className "flex justify-around py-2 border-t border-gray-300"
        prop.children (
            controlList
            |> List.map (fun (title: string, msg) ->
                Html.div [
                    prop.className "modalControls cursor-pointer text-lg font-semibold text-primary hover:text-primary-focus"
                    prop.onClick (fun _ -> msg |> dispatch)
                    prop.children [ Html.h1 [ prop.text title ] ]
                ]
            )
        )
    ]

// Gallery header controls with close and external link buttons
let galleryHeaderControls hrefLink hrefImg msg dispatch =
    Html.div [
        prop.className "flex justify-between items-center p-2"
        prop.children [
            Html.a [
                prop.className "closeModal cursor-pointer"
                prop.onClick (fun _ -> msg |> dispatch)
                prop.children [
                    Html.img [ prop.src "./imgs/icons/X-it.png"; prop.className "w-16 h-16"; prop.alt "Close" ]
                ]
            ]
            Html.span [
                prop.className "modalExternalLink"
                prop.children [
                    Html.a [
                        prop.href hrefLink
                        prop.children [
                            Html.img [ prop.src hrefImg; prop.className "w-16 h-16"; prop.alt "External Link" ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// Determine if viewport width is >= 900px
let viewPortModalBreak =
    window.innerWidth >= 900.0

// Shared modal wrapper
let sharedViewModal (isActive: bool) (header: ReactElement) (content: ReactElement) (footer: ReactElement) =
    Html.div [
        prop.className (if isActive then "modal modal-open" else "modal")
        prop.children [
            Html.div [ prop.className "modal-box" ; prop.children [ header; content; footer ] ]
            Html.label [ prop.className "modal-backdrop"; prop.htmlFor "" ]
        ]
    ]

// Shared split header (title + blurbs)
let sharedSplitHeader (title: string) contentBlurb =
    Html.div [
        prop.className "generalViewTitleCard p-4 bg-base-200 rounded-md shadow-md"
        prop.children [
            Html.h1 [ prop.className "text-3xl font-bold mb-2"; prop.text title ]
            yield! contentBlurb |> List.map (fun (blurb: string) -> Html.h2 [ prop.className "text-xl"; prop.text blurb ])
        ]
    ]

// Shared split view layout (header + two columns)
let sharedSplitView header childLeft childRight =
    Html.div [
        prop.className "paddedContainerHeader p-4"
        prop.children [
            header
            Html.div [
                prop.className "flex flex-row space-x-4 h-40"
                prop.children [
                    Html.div [ prop.className "w-1/2 overflow-auto"; prop.children [ childLeft ] ]
                    Html.div [ prop.className "w-1/2 overflow-auto"; prop.children [ childRight ] ]
                ]
            ]
        ]
    ]
