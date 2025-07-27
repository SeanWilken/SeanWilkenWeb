module SharedViewModule

open Feliz
open Browser
open Bindings.LucideIcon

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
open Bindings.LucideIcon

let sharedModalHeader (gameTitle: string) msg dispatch =
    Html.div [
        prop.className "flex items-center justify-between px-6 py-4 border-b border-base-300 bg-base-100"
        prop.children [

            // Left content placeholder (e.g. breadcrumbs or icons)
            Html.div [
                prop.className "w-16 h-16"
                prop.children [ Html.none ]
            ]

            // Centered Title
            Html.h1 [
                prop.className "text-2xl font-bold text-primary text-center flex-1"
                prop.text gameTitle
            ]

            // Right-side close button
            Html.button [
                prop.className "btn btn-square btn-ghost tooltip"
                prop.custom("data-tip", "Close")
                prop.onClick (fun _ -> dispatch msg)
                prop.children [
                    LucideIcon.X "w-6 h-6 text-base-content"
                ]
            ]
        ]
    ]

// Renders each modal control action as a nicely styled link button
let codeModalControlSelections (controlActions: (string * 'msg) list) (dispatch: 'msg -> unit) =
    Html.div [
        prop.className "flex flex-col gap-3"
        prop.children (
            controlActions
            |> List.map (fun (label, msg) ->
                Html.button [
                    prop.className "text-primary hover:text-accent hover:underline text-lg font-medium transition-colors"
                    prop.text label
                    prop.onClick (fun _ -> dispatch msg)
                ]
            )
        )
    ]

// Container wrapper for modal controls with consistent layout
let codeModalControlsContent (controlList: (string * 'msg) list) (dispatch: 'msg -> unit) =
    Html.div [
        prop.className "modalAltContent p-6 bg-base-100 rounded-lg shadow-inner"
        prop.children [
            codeModalControlSelections controlList dispatch
        ]
    ]

// Round complete content card
let roundCompleteContent (gameStatDetails: string list) =
    Html.div [
        prop.className "bg-base-200 text-center rounded-xl shadow-lg p-6 text-base-content space-y-4"
        prop.children [
            Html.h2 [
                prop.className "text-2xl font-bold text-success"
                prop.text "🎉 Round Complete!"
            ]
            Html.div [
                prop.className "text-lg font-semibold text-base-content/80"
                prop.text "Stats Summary:"
            ]
            Html.ul [
                prop.className "list-disc list-inside space-y-1"
                prop.children (
                    gameStatDetails
                    |> List.map (fun stat ->
                        Html.li [ prop.text stat ]
                    )
                )
            ]
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
        prop.className "flex flex-wrap justify-center gap-4 py-4 border-t border-base-300 bg-base-100"
        prop.children (
            controlList
            |> List.map (fun (title: string, msg) ->
                Html.button [
                    prop.className "btn btn-sm md:btn-md btn-outline text-primary hover:btn-primary transition"
                    prop.text title
                    prop.onClick (fun _ -> dispatch msg)
                ]
            )
        )
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
        prop.className "flex justify-between items-center p-2"
        prop.children [
            Html.button [
                prop.className "btn btn-square btn-ghost text-error hover:text-error-content"
                prop.onClick (fun _ -> props.onClose())
                prop.children [
                    LucideIcon.X "w-6 h-6"
                ]
            ]
            match props.rightIcon with
            | Some icon ->
                Html.a [
                    match icon.externalLink with 
                    | None -> ()
                    | Some extLink -> prop.href extLink
                    prop.target "_blank"
                    prop.className "btn btn-ghost hover:bg-base-200"
                    prop.title (icon.externalAlt |> Option.defaultValue "External Link")
                    prop.children [
                        icon.icon
                        Html.span icon.label
                    ]
                ]
            | None -> Html.none
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
        prop.className "p-4 bg-base-200 rounded-md shadow-md"
        prop.children [
            Html.h1 [ prop.className "text-3xl font-bold mb-2"; prop.text title ]
            yield! contentBlurb |> List.map (fun (blurb: string) -> Html.h2 [ prop.className "text-xl"; prop.text blurb ])
        ]
    ]

// Shared split view layout (header + two columns)
let sharedSplitView header childLeft childRight =
    Html.div [
        prop.className "p-4"
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
