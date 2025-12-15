module SharedViewModule

open Feliz
open Browser
open Bindings.LucideIcon
open Client.Domain.GridGame

module GamePieceIcons =
    open Client.Domain.GridGame

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


// Modal content wrapper for games
let gameModalContent content =
    Html.div [
        prop.className "gameGridContainer"
        prop.children [ content ]
    ]

// Modal header with close button and title

let sharedModalHeader (gameTitle: string) (gameInstructions: string list) msg dispatch =
    let (showInfo, setShowInfo) = React.useState(false)
    Html.div [
        prop.className "bg-base-200 shadow-md flex items-center justify-between px-6 py-4"
        prop.children [

            // Left content placeholder (e.g. breadcrumbs or icons)
            Html.button [
                prop.className "btn btn-square btn-ghost tooltip"
                prop.custom("data-tip", "Close")
                prop.onClick (fun _ -> dispatch msg)
                prop.children [
                    LucideIcon.X "w-6 h-6 text-base-content"
                ]
            ]

            // Centered Title
            Html.h1 [
                prop.className "text-2xl font-bold text-primary text-center flex-1"
                prop.text gameTitle
            ]

            // Right-side close button
            Html.div [
                prop.children [ 
                    
                    Html.span [
                        prop.onClick ( fun _ -> setShowInfo(true))
                        prop.children [
                            LucideIcon.Info "cursor-pointer hover:text-blue-700 text-blue-500"
                        ]
                    ]
                    if showInfo then
                        Html.div [
                            prop.className "absolute top-4 left-2 right-2 bg-base-100 border border-base-300 rounded shadow-lg p-4 w-64 z-20"
                            prop.children [
                                Html.h3 [ prop.className "font-bold mb-2"; prop.text "Game Rules" ]
                                Html.ul [
                                    for rule in gameInstructions do
                                        Html.li [ prop.className "mb-1 text-sm"; prop.text rule ]
                                ]
                                Html.button [
                                    prop.className "btn btn-xs btn-primary mt-2 w-full"
                                    prop.onClick (fun _ -> setShowInfo(false))
                                    prop.text "Close"
                                ]
                            ]
                        ]
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
let roundCompleteContent (gameStatDetails: string list) restartGame =
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
            Html.button [
                prop.className "btn btn-primary mt-4"
                prop.onClick (fun _ -> restartGame())
                prop.text "Restart Round"
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
    
// Shared
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


module Query =
    open System

    let private splitOn (c: char) (s: string) =
        s.Split(c, StringSplitOptions.RemoveEmptyEntries ||| StringSplitOptions.TrimEntries)
        |> Array.toList

    let parse (query: string) =
        // strip leading '?'
        let q = if query.StartsWith("?") then query.Substring(1) else query
        q
        |> splitOn '&'
        |> List.choose (fun pair ->
            match splitOn '=' pair with
            | [ key; value ] -> Some (key.ToLowerInvariant(), System.Uri.UnescapeDataString value)
            | _ -> None
        )
        |> Map.ofList

module SharedMicroGames =
    type CyberPanelProps = { 
        ClassName: string
        ClipPath: string option
        Children: ReactElement list 
    }

    let CyberPanel (p: CyberPanelProps) =
        Html.div [
            prop.className ("cyber-panel " + p.ClassName)
            if p.ClipPath.IsSome then
                prop.style [ style.custom("clip-path", p.ClipPath.Value) ]
            prop.children p.Children
        ]

    let CyberBackground () =
        // You can keep DaisyUI theme enabled globally; this background is self-contained.
        Html.div [
            prop.className "absolute inset-0 overflow-hidden pointer-events-none"
            prop.children [

                // Diagonal repeating neon
                Html.div [
                    prop.className "absolute inset-0 opacity-50"
                    prop.style [
                        style.backgroundImage
                            "repeating-linear-gradient(45deg, rgba(0,255,255,0.03) 0px, transparent 2px, transparent 8px, rgba(0,255,255,0.03) 10px), repeating-linear-gradient(-45deg, rgba(255,0,255,0.03) 0px, transparent 2px, transparent 8px, rgba(255,0,255,0.03) 10px)"
                        style.custom("animation", "gridPulse 4s ease-in-out infinite")
                    ]
                ]

                // Sliding grid
                Html.div [
                    prop.className "absolute inset-0 opacity-20"
                    prop.style [
                        style.backgroundImage
                            "linear-gradient(rgba(0,255,255,0.15) 1px, transparent 1px), linear-gradient(90deg, rgba(0,255,255,0.15) 1px, transparent 1px)"
                        style.backgroundSize "60px 60px"
                        style.custom("animation", "gridSlide 30s linear infinite")
                    ]
                ]

                // Cyan glow blob
                Html.div [
                    prop.className "absolute opacity-100"
                    prop.style [
                        style.top (length.perc 15)
                        style.left (length.perc 10)
                        style.width 600
                        style.height 600
                        style.custom("background", "radial-gradient(circle, rgba(0,255,255,0.15) 0%, transparent 60%)")
                        style.custom ("filter", "blur(80px)")
                        style.custom("animation", "float 10s ease-in-out infinite")
                    ]
                ]

                // Magenta glow blob
                Html.div [
                    prop.className "absolute opacity-100"
                    prop.style [
                        style.bottom (length.perc 10)
                        style.right (length.perc 15)
                        style.width 500
                        style.height 500
                        style.custom("background", "radial-gradient(circle, rgba(255,0,255,0.15) 0%, transparent 60%)")
                        style.custom ("filter", "blur(80px)")
                        style.custom("animation", "float 12s ease-in-out infinite reverse")
                    ]
                ]

                // Scanlines
                Html.div [
                    prop.className "absolute inset-0"
                    prop.style [
                        style.backgroundImage "repeating-linear-gradient(0deg, transparent, transparent 2px, rgba(0,255,255,0.03) 2px, rgba(0,255,255,0.03) 4px)"
                        style.custom("animation", "scanline 8s linear infinite")
                    ]
                ]
            ]
        ]

    // // ---------------------------
    // // Shell layout (left / center / right)
    // // ---------------------------

    type CyberShellResponsiveProps = {
            Left: ReactElement
            Board: ReactElement
            Overlay: ReactElement option
            OnQuit: unit -> unit
        }

    let CyberShellResponsive (p: CyberShellResponsiveProps) =
        Html.div [
            prop.className "relative min-h-screen w-full overflow-hidden bg-[#050a15]"
            prop.children [
                CyberBackground()

                Html.div [
                    // padding like the mockup, and keep it full width
                    prop.className "relative z-[1] w-full px-6 py-6 lg:px-10 lg:py-10"
                    prop.children [
                        Html.div [
                            // Mobile: 1 col (stack)
                            // lg+: 3 cols where left spans 1, board spans 2
                            prop.className "mx-auto w-full max-w-[1600px] grid grid-cols-1 lg:grid-cols-3 gap-8 lg:gap-10 items-start"
                            prop.children [
                                Html.div [
                                    // Controls rail
                                    prop.className "lg:col-span-1 flex flex-col gap-8"
                                    prop.children [ p.Left ]
                                ]
                                Html.div [
                                    // Board area
                                    prop.className "lg:col-span-2 min-w-0"
                                    prop.children [ p.Board ]
                                ]
                            ]
                        ]
                    ]
                ]

                // close button
                Html.button [
                    prop.className "btn btn-ghost btn-square absolute top-4 left-4 z-[5] text-cyan-300 hover:text-cyan-100"
                    prop.onClick (fun _ -> p.OnQuit())
                    prop.children [ LucideIcon.ChevronLeft "w-6 h-6" ]
                ]

                match p.Overlay with
                | Some o -> o
                | None -> Html.none
            ]
        ]

    // ---------------------------
    // Panels
    // ---------------------------
    let TitlePanel (title: string) =
        CyberPanel {
            ClassName = "p-5"
            ClipPath = Some "polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%)"
            Children = [
                Html.h1 [
                    prop.className "m-0 text-[32px] font-bold uppercase leading-[1.2] tracking-[3px]"
                    prop.style [
                        style.backgroundImage "linear-gradient(135deg, #00ffff, #ff00ff)"
                        style.custom("-webkit-background-clip", "text")
                        style.custom("-webkit-text-fill-color", "transparent")
                        style.custom("text-shadow", "0 0 20px rgba(0,255,255,0.5)")
                    ]
                    prop.children [
                        Html.text title
                        // for i, line in titleLines |> List.indexed do
                        //     if i > 0 then Html.br []
                    ]
                ]
            ]
        }

    let StatBlock (label: string) (value: string) (accent: string) =
        Html.div [
            prop.children [
                Html.div [
                    prop.className "text-[10px] uppercase tracking-[2px] opacity-70 text-center"
                    prop.style [ style.color accent ]
                    prop.text label
                ]
                Html.div [
                    prop.className "text-[36px] font-bold text-center"
                    prop.style [
                        style.color accent
                        style.custom("text-shadow", (sprintf "0 0 20px %s" (if accent = "#00ffff" then "rgba(0,255,255,0.8)" else "rgba(255,0,255,0.8)")))
                    ]
                    prop.text value
                ]
            ]
        ]

    let StatsPanel (levelValue: string) (movesValue: string) =
        CyberPanel {
            ClassName = "p-5"
            ClipPath = Some "polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%)"
            Children = [
                StatBlock "LEVEL" levelValue "#00ffff"
                Html.div [
                    prop.className "my-4 h-px"
                    prop.style [ style.backgroundImage "linear-gradient(90deg, transparent, rgba(0,255,255,0.5), transparent)" ]
                ]
                StatBlock "MOVES" movesValue "#ff00ff"
            ]
        }

    type CyberButtonStyle =
        | Cyan
        | Magenta
        | Red
        | Purple
        | Dim

    let private buttonStyle (kind: CyberButtonStyle) (enabled: bool) =
        let bg, border, color, shadow, opacity =
            match kind, enabled with
            | _, false ->
                "rgba(255,255,255,0.05)", "1px solid rgba(255,255,255,0.1)", "#666", "none", "0.4"
            | Cyan, true ->
                "linear-gradient(135deg, rgba(0,255,255,0.30), rgba(0,139,139,0.30))",
                "1px solid rgba(0,255,255,0.50)",
                "#00ffff",
                "0 0 20px rgba(0,255,255,0.30)",
                "1"
            | Magenta, true ->
                "linear-gradient(135deg, rgba(255,0,255,0.30), rgba(139,0,139,0.30))",
                "1px solid rgba(255,0,255,0.50)",
                "#ff00ff",
                "0 0 20px rgba(255,0,255,0.30)",
                "1"
            | Red, true ->
                "linear-gradient(135deg, rgba(255,0,0,0.30), rgba(139,0,0,0.30))",
                "1px solid rgba(255,0,0,0.50)",
                "#ff0066",
                "0 0 20px rgba(255,0,0,0.30)",
                "1"
            | Purple, true ->
                "linear-gradient(135deg, rgba(138,43,226,0.30), rgba(75,0,130,0.30))",
                "1px solid rgba(138,43,226,0.50)",
                "#ff00ff",
                "0 0 20px rgba(138,43,226,0.30)",
                "1"
            | Dim, true ->
                "rgba(255,255,255,0.05)", "1px solid rgba(255,255,255,0.2)", "#aaa", "none", "1"

        [ 
            style.custom("background", bg)
            style.custom("border", border)
            style.custom("color", color)
            style.custom("box-shadow", shadow)
            style.custom("opacity", opacity)
            style.custom("clip-path", "polygon(8px 0, 100% 0, 100% calc(100% - 8px), calc(100% - 8px) 100%, 0 100%, 0 8px)") 
        ]

    let ControlButton (label: string) (kind: CyberButtonStyle) (enabled: bool) (onClick: unit -> unit) (icon: ReactElement option) =
        Html.button [
            prop.disabled (not enabled)
            prop.className "w-full py-3 flex items-center justify-center gap-2 font-bold uppercase tracking-[1px] text-[14px] transition-all"
            prop.style (buttonStyle kind enabled)
            prop.onClick (fun _ -> if enabled then onClick())
            prop.children [
                match icon with Some i -> i | None -> Html.none
                Html.text label
            ]
        ]

    let ControlsPanel (controls: ReactElement list) =
        CyberPanel {
            ClassName = "p-5"
            ClipPath = Some "polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%)"
            Children = [
                Html.div [
                    prop.className "flex flex-col gap-3"
                    prop.children controls
                ]
            ]
        }

    let BoardPanel (board: ReactElement) =
        CyberPanel {
            ClassName = "p-6 sm:p-10 flex items-center justify-center min-h-[520px] sm:min-h-[680px] lg:min-h-[820px]"
            ClipPath = Some "polygon(15px 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%, 0 15px)"
            Children = [ board ]
        }

    let LevelSelectPanel<'T>
        (levels: List<'T> ) 
        (current: 'T)
        (isEqualTo: 'T -> 'T -> bool)
        (onSelect: 'T -> unit) =
        CyberPanel {
            ClassName = "p-4"
            ClipPath = Some "polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%)"
            Children = [
                Html.div [
                    prop.className "text-[10px] uppercase tracking-[2px] opacity-70 text-center mb-3"
                    prop.style [ style.color "#00ffff" ]
                    prop.text "SELECT LEVEL"
                ]
                Html.div [
                    prop.className "flex gap-2 justify-center"
                    prop.children [
                        for lvl in levels do
                            let isCurrent = isEqualTo lvl current
                            Html.button [
                                prop.className "level-btn p-2 font-bold text-[18px] w-[44px] h-[44px]"
                                prop.style [
                                    style.custom("border-radius", "0")
                                    style.custom("clip-path", "polygon(20% 0, 100% 0, 100% 80%, 80% 100%, 0 100%, 0 20%)")
                                    style.custom("background",
                                        if isCurrent then "linear-gradient(135deg, rgba(0,255,255,0.40), rgba(0,139,139,0.40))"
                                        else "rgba(255,255,255,0.05)")
                                    style.custom("border",
                                        if isCurrent then "1px solid #00ffff"
                                        else "1px solid rgba(255,255,255,0.2)")
                                    style.custom("color", if isCurrent then "#00ffff" else "#666")
                                    style.custom("box-shadow", if isCurrent then "0 0 20px rgba(0,255,255,0.4)" else "none")
                                    style.custom("text-shadow", if isCurrent then "0 0 10px rgba(0,255,255,0.8)" else "none")
                                ]
                                prop.onClick (fun _ -> onSelect lvl)
                                prop.text (string lvl)
                            ]
                    ]
                ]
            ]
        }

    let InstructionsPanel (title: string) (lines: string list) closeLabel (onClose: unit -> unit) =
        CyberPanel {
            ClassName = "p-5"
            ClipPath = Some "polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%)"
            Children = [
                Html.div [
                    prop.style [ style.custom("animation", "slideIn 0.3s ease-out") ]
                    prop.children [
                        Html.div [
                            prop.className "font-bold text-[14px] mb-3 tracking-[2px] uppercase"
                            prop.style [ style.color "#00ffff" ]
                            prop.text title
                        ]
                        Html.div [
                            prop.className "text-[13px] leading-[1.8]"
                            prop.style [ style.color "#aaa"; style.fontFamily "monospace" ]
                            prop.children [
                                for l in lines do
                                    Html.div [ prop.text l ]
                            ]
                        ]
                        if not (System.String.IsNullOrWhiteSpace closeLabel)
                        then 
                            Html.button [
                                prop.className "btn btn-xs btn-ghost mt-4 text-cyan-200 hover:text-cyan-50"
                                prop.onClick (fun _ -> onClose())
                                prop.text closeLabel
                            ]
                    ]
                ]
            ]
        }

    // ---------------------------
    // DPad
    // ---------------------------

    type DPadState = { 
            CanUp: bool
            CanDown: bool
            CanLeft: bool
            CanRight: bool
            Disabled: bool
        }

    let private dpadBtn (txt: string) (enabled: bool) (onClick: unit -> unit) (clip: string) =
        Html.button [
            prop.className "dpad-btn"
            prop.disabled (not enabled)
            prop.onClick (fun _ -> if enabled then onClick())
            prop.style [
                style.width 56
                style.height 56
                style.custom("border-radius", "0")
                style.custom("clip-path", clip)
                style.custom("background",
                    if enabled then "linear-gradient(135deg, rgba(0,255,255,0.40), rgba(0,139,139,0.40))"
                    else "rgba(255,255,255,0.05)")
                style.custom("border",
                    if enabled then "1px solid rgba(0,255,255,0.8)"
                    else "1px solid rgba(255,255,255,0.1)")
                style.custom("color", if enabled then "#00ffff" else "#444")
                style.custom("box-shadow", if enabled then "0 0 20px rgba(0,255,255,0.3)" else "none")
                style.custom("opacity", if enabled then "1" else "0.3")
                style.custom("text-shadow", if enabled then "0 0 10px rgba(0,255,255,1)" else "none")
                style.fontSize 24
            ]
            prop.text txt
        ]

    let DPadPanel (state: DPadState) (onMove: Client.Domain.GridGame.MovementDirection -> unit) =
        Html.div [
            prop.className "flex flex-col items-center gap-2"
            prop.children [
                dpadBtn "▲" (state.CanUp && not state.Disabled)
                    (fun () -> onMove Up)
                    "polygon(30% 0, 70% 0, 100% 30%, 100% 100%, 0 100%, 0 30%)"

                Html.div [
                    prop.className "flex items-center gap-2"
                    prop.children [
                        dpadBtn "◀" (state.CanLeft && not state.Disabled)
                            (fun () -> onMove Left)
                            "polygon(30% 0, 100% 0, 100% 100%, 30% 100%, 0 70%, 0 30%)"

                        Html.div [
                            prop.style [
                                style.width 56
                                style.height 56
                                style.custom("background", "rgba(10, 20, 40, 0.8)")
                                style.custom("border-radius", "9999px")
                                style.custom("border", "2px solid rgba(0,255,255,0.3)")
                                style.display.flex
                                style.alignItems.center
                                style.justifyContent.center
                            ]
                            prop.children [
                                Html.div [
                                    prop.style [
                                        style.width 16
                                        style.height 16
                                        style.custom("border-radius", "9999px")
                                        style.backgroundImage "linear-gradient(135deg, #00ffff, #0099cc)"
                                        style.custom("box-shadow", "0 0 12px rgba(0,255,255,0.8)")
                                    ]
                                ]
                            ]
                        ]

                        dpadBtn "▶" (state.CanRight && not state.Disabled)
                            (fun () -> onMove Right)
                            "polygon(0 0, 70% 0, 100% 30%, 100% 70%, 70% 100%, 0 100%)"
                    ]
                ]

                dpadBtn "▼" (state.CanDown && not state.Disabled)
                    (fun () -> onMove Down)
                    "polygon(0 0, 100% 0, 100% 70%, 70% 100%, 30% 100%, 0 70%)"
            ]
        ]

    let DPadInfoPanel info1Label info1Value info2Label info2Value (state: DPadState) (onMove: Client.Domain.GridGame.MovementDirection -> unit) =
        let enabled (canDir: bool) = canDir && not state.Disabled

        let statMini (label: string) (value: string) (accent: string) =
            Html.div [
                prop.className "flex flex-col gap-2"
                prop.children [
                    Html.div [
                        prop.className "text-[14px] uppercase tracking-[3px] opacity-90"
                        prop.style [ style.color accent ]
                        prop.text label
                    ]
                    Html.div [
                        prop.className "text-[34px] font-bold leading-none"
                        prop.style [
                            style.color accent
                            style.custom  (
                                "text-shadow",
                                if accent = "#00ffff"
                                then "0 0 18px rgba(0,255,255,0.75)"
                                else "0 0 18px rgba(255,0,255,0.75)"
                            )
                        ]
                        prop.text value
                    ]
                ]
            ]

        CyberPanel {
            // Make this panel tall like the screenshot
            ClassName = "p-6 flex flex-col min-h-[520px]"
            ClipPath = Some "polygon(15px 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%, 0 15px)"
            Children = [
                // Top stats row
                Html.div [
                    prop.className "flex items-start justify-between px-2"
                    prop.children [
                        statMini info1Label info1Value "#00ffff"
                        statMini info2Label info2Value "#ff00ff"
                    ]
                ]

                // Spacer to push D-pad cluster to bottom
                Html.div [ prop.className "flex-1" ]

                // Bottom-anchored D-pad cluster (matches screenshot)
                Html.div [
                    prop.className "w-full flex items-center justify-center pb-2"
                    prop.children [
                        DPadPanel {
                            CanUp = state.CanUp
                            CanDown = state.CanDown
                            CanLeft = state.CanLeft
                            CanRight = state.CanRight
                            Disabled = state.Disabled
                        } onMove
                    ]
                ]
            ]
        }



    // ---------------------------
    // Win overlay
    // ---------------------------

    let WinOverlay (moves: int) (onRetry: unit -> unit) (onNext: (unit -> unit) option) =
        Html.div [
            prop.className "fixed inset-0 z-[100] flex items-center justify-center"
            prop.style [
                style.backgroundColor "rgba(0,0,0,0.9)"
                style.custom("backdrop-filter", "blur(10px)")
                style.custom("animation", "slideIn 0.3s ease-out")
            ]
            prop.children [
                CyberPanel {
                    ClassName = "p-12 text-center"
                    ClipPath = Some "polygon(20px 0, 100% 0, 100% calc(100% - 20px), calc(100% - 20px) 100%, 0 100%, 0 20px)"
                    Children = [
                        Html.div [
                            prop.className "text-[64px] mb-4"
                            prop.style [ style.custom("animation", "float 2s ease-in-out infinite") ]
                            prop.text "⚡"
                        ]
                        Html.h2 [
                            prop.className "m-0 mb-4 text-[36px] font-bold uppercase tracking-[3px]"
                            prop.style [
                                style.backgroundImage "linear-gradient(135deg, #00ffff, #ff00ff)"
                                style.custom("-webkit-background-clip", "text")
                                style.custom("-webkit-text-fill-color", "transparent")
                                style.custom("text-shadow", "0 0 30px rgba(0,255,255,0.5)")
                            ]
                            prop.text "LEVEL COMPLETE"
                        ]
                        Html.p [
                            prop.className "text-[18px] mb-8 tracking-[1px]"
                            prop.style [ style.color "#00ffff" ]
                            prop.text $"COMPLETED IN {moves} MOVES"
                        ]
                        Html.div [
                            prop.className "flex gap-4 justify-center"
                            prop.children [
                                Html.button [
                                    prop.className "px-7 py-3 font-bold uppercase tracking-[1px]"
                                    prop.style (buttonStyle Purple true @ [ style.custom("border-radius", "0"); style.custom("clip-path", "polygon(8px 0, 100% 0, 100% calc(100% - 8px), calc(100% - 8px) 100%, 0 100%, 0 8px)") ])
                                    prop.onClick (fun _ -> onRetry())
                                    prop.text "TRY AGAIN"
                                ]
                                match onNext with
                                | None -> Html.none
                                | Some next ->
                                    Html.button [
                                        prop.className "px-7 py-3 font-bold uppercase tracking-[1px]"
                                        prop.style (buttonStyle Cyan true @ [ style.custom("border-radius", "0") ])
                                        prop.onClick (fun _ -> next())
                                        prop.text "NEXT LEVEL"
                                    ]
                            ]
                        ]
                    ]
                }
            ]
        ]





    let private tileBaseStyle : IStyleAttribute list =
        [
            style.width 64
            style.height 64
            style.custom("border-radius", "0")
            style.custom("clip-path", "polygon(10% 0, 100% 0, 100% 90%, 90% 100%, 0 100%, 0 10%)")
            style.position.relative
            style.overflow.visible
        ]

    /// GOAL ROLL
    /// 
    /// 
    

    let mkTile (bg: string) (border: string) (shadow: string) (anim: string option) (children: ReactElement list) =
        Html.div [
            prop.className "tile select-none"
            prop.style (
                tileBaseStyle
                @ [
                    style.custom("background", bg)
                    style.custom("border", border)
                    style.custom("box-shadow", shadow)
                    match anim with
                    | Some a -> style.custom("animation", a)
                    | None -> style.custom("animation", "none")
                ]
            )
            prop.children children
        ]

    let centered (content: ReactElement) =
        Html.div [
            prop.className "absolute inset-0 flex items-center justify-center"
            prop.children [ content ]
        ]