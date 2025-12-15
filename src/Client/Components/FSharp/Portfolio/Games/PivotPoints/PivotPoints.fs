module Components.FSharp.Portfolio.Games.PivotPoints

open Client.Domain
open GridGame
open Elmish
open Browser
open Feliz
open SharedViewModule.SharedMicroGames
open Bindings.LucideIcon
    
// - Extras:
// speeds up like snake as more are picked up
// certain coins have certain effects (?) // turns to blocker // speed up round // reverse roll direction // etc..(?)


type LaneDetails = {
    Style : Feliz.IStyleAttribute list
    Message : SharedPivotPoint.Msg
}
// content descriptions
let pivotPointsDescriptions = [
    "- Select Start to begin the game." 
    "- Arrow will continue to move in the direction it is pointing in intervals."
    "- Pivot the arrow from it's current direction to either direction of the intersecting lane."
    "- Red will pivot the arrow to descend the intersecting lane (left in a row, up in a column)"
    "- Blue will pivot the arrow to ascend the intersecting lane (right in a row, down in a column)"
    "- Avoid crashing into the lane blockers, or it's game over."
    "- Guide the arrow to the flags to collect points."
    // "- The more points you get, the faster the movement interval scale gets." // - Need to implement
]

let modelPivotPointRoundDetails ( model : SharedPivotPoint.Model ) =
    [
        "You collected " + string model.CoinsCollected + " coins."
        "You lasted " + string ( SharedViewModule.gameTickClock model.GameClock ) + " seconds."
    ]

let controlList = [ 
    "Settings", (SharedPivotPoint.SetGameState (RoundState.Settings)) 
]

let ascendLane = Some { Style = [ style.color "#801515" ]; Message = SharedPivotPoint.PivotArrow SharedPivotPoint.PivotDirection.Ascend }
let descendLane = Some { Style = [ style.color "#143054" ]; Message = SharedPivotPoint.PivotArrow SharedPivotPoint.PivotDirection.Descend }
let floorLane = [ None; ascendLane; ascendLane; ascendLane; ascendLane; ascendLane; ascendLane; descendLane; ]
let ceilingLane = [ ascendLane; descendLane; descendLane; descendLane; descendLane; descendLane; descendLane; None ]

// Update functions ---------

// shared between goal roll and pivot points...
let moveArrowForward ( model : SharedPivotPoint.Model) =
    let gameBoard = model.GameBoard
    let piecePositionIndex = RollableGridGameHelpers.getPiecePositionIndex gameBoard Ball
    if piecePositionIndex = -1
        then gameBoard
        else RollableGridGameHelpers.checkDirectionMovement piecePositionIndex model.BallDirection gameBoard

// GAME LOOP FUNCTIONS

// Time drives main game state, as things happen in intervals contained within the main loop
let startGameLoop ( model : SharedPivotPoint.Model ) dispatch =
    if model.DispatchPointer = 0.0 && model.GameState = Paused
        then
            window.setInterval((fun _ -> dispatch (SharedPivotPoint.GameLoopTick)), 250)
            |> fun loopFloat -> dispatch (SharedPivotPoint.SetDispatchPointer loopFloat)
        else
            SharedViewModule.stopGameLoop model.DispatchPointer
            dispatch ( SharedPivotPoint.SetDispatchPointer 0.0 )

// COINS
let coinSpawnPosition ( gridBoard : GridBoard ) =
    let validPositions = 
        [ for i in 0 .. gridBoard.GridPositions.Length - 1 do
            if gridBoard.GridPositions.Item(i) = Blank 
                then i
                else -1 
        ]
    List.filter ( fun x -> ( x <> -1 ) ) ( validPositions )
    |> fun availablePositions -> 
        availablePositions.Item(SharedTileSort.randomIndex availablePositions.Length)

//----------------

// Lifecycle -------------

let init(): SharedPivotPoint.Model * Cmd<SharedPivotPoint.Msg> =
    SharedPivotPoint.initModel, Cmd.none

let update msg ( model : SharedPivotPoint.Model ) : SharedPivotPoint.Model * Cmd<SharedPivotPoint.Msg> = 
    match msg with
    | SharedPivotPoint.GameLoopTick ->
        let coinPoints = 
            match getObjectPositionIndex model.GameBoard Goal with
            | Some x -> 0
            | None -> 1
        let gridWithCoinUpdate = 
            if coinPoints = 1 
                then 
                    let newCoinPosition = coinSpawnPosition model.GameBoard
                    updatePositionWithObject model.GameBoard Goal newCoinPosition
                else model.GameBoard

        let tickedClock = model.GameClock + 1
        let rollInterval = model.RollInterval + 1
        if rollInterval > 2
            then { model with GameBoard = gridWithCoinUpdate; RollInterval = 0; GameClock = tickedClock; CoinsCollected = model.CoinsCollected + coinPoints }, Cmd.ofMsg SharedPivotPoint.MoveArrow
            else { model with GameBoard = gridWithCoinUpdate; RollInterval = rollInterval; GameClock = tickedClock; CoinsCollected = model.CoinsCollected + coinPoints }, Cmd.none
    | SharedPivotPoint.SetDispatchPointer pointer ->
        if model.GameState = Won 
            then SharedPivotPoint.initModel, Cmd.none
            else
                if pointer <> 0.0 then Playing else Paused
                |> fun gameRoundState -> { model with GameState = gameRoundState; DispatchPointer = pointer }, Cmd.none
    | SharedPivotPoint.SetGameState gameState ->
        if gameState <> Playing && model.DispatchPointer <> 0.0 
            then 
                SharedViewModule.stopGameLoop model.DispatchPointer
                { model with DispatchPointer = 0.0; GameState = gameState; }, Cmd.none
            else
                { model with GameState = gameState }, Cmd.none
    | SharedPivotPoint.PivotArrow pivotDirection ->
        match model.BallDirection with
        | MovementDirection.Up
        | MovementDirection.Down ->
            match pivotDirection with
            | SharedPivotPoint.Ascend ->
                { model with BallDirection = MovementDirection.Right; BoardOrientation = SharedPivotPoint.LaneOrientation.LaneRow }, Cmd.none
            | SharedPivotPoint.Descend ->
                { model with BallDirection = MovementDirection.Left; BoardOrientation = SharedPivotPoint.LaneOrientation.LaneRow }, Cmd.none
        | MovementDirection.Left
        | MovementDirection.Right ->
            match pivotDirection with
            | SharedPivotPoint.Ascend ->
                { model with BallDirection = Down; BoardOrientation = SharedPivotPoint.LaneOrientation.LaneColumn }, Cmd.none
            | SharedPivotPoint.Descend ->
                { model with BallDirection = Up; BoardOrientation = SharedPivotPoint.LaneOrientation.LaneColumn }, Cmd.none
    | SharedPivotPoint.MoveArrow ->
        let movedArrowGrid = moveArrowForward model
        if movedArrowGrid = model.GameBoard
            then model, Cmd.ofMsg SharedPivotPoint.EndRound
            else { model with GameBoard = movedArrowGrid }, Cmd.none
    | SharedPivotPoint.ResetRound ->
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        SharedPivotPoint.initModel, Cmd.none
    | SharedPivotPoint.EndRound ->
        SharedViewModule.stopGameLoop model.DispatchPointer
        { model with GameBoard = SharedPivotPoint.demoGameBoard; DispatchPointer = 0.0; GameState = Won }, Cmd.none
    | SharedPivotPoint.ExitGameLoop ->
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        model, Cmd.none
    | SharedPivotPoint.QuitGame ->
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        model, Cmd.none
    | _ -> model, Cmd.none

// --------------------------

// VIEW FUNCTIONS 
let roundStateToggleString (model: SharedPivotPoint.Model) =
    match model.GameState with
    | Won | Settings -> "Play"
    | _ when model.DispatchPointer <> 0.0 -> "Pause"
    | _ when model.GameClock <> 0 -> "Resume"
    | _ -> "Start"

let roundStateToggle (model: SharedPivotPoint.Model) dispatch =
    let toggleStr = roundStateToggleString model
    Html.button [
        prop.className "modalControls text-white hover:text-green-400"
        prop.onClick (fun _ -> startGameLoop model dispatch |> ignore)
        prop.text toggleStr
    ]

// let rulesAndSettingsGameControls (model: SharedPivotPoint.Model) dispatch =
//     let toggleStr = roundStateToggleString model
//     Html.div [
//         prop.className "space-y-4 text-center"
//         prop.children [
//             Html.button [
//                 prop.className "modalControls text-lg text-white hover:text-blue-400"
//                 prop.text $"{toggleStr} Round"
//                 prop.onClick (fun _ -> startGameLoop model dispatch |> ignore)
//             ]
//             Html.button [
//                 prop.className "modalControls text-lg text-white hover:text-red-400"
//                 prop.text "Restart Round"
//                 prop.onClick (fun _ -> SharedPivotPoint.ResetRound |> dispatch)
//             ]
//         ]
//     ]

// let modalGameSettingsView model dispatch =
//     Html.div [
//         prop.className "modalAltContent text-center"
//         prop.children [ rulesAndSettingsGameControls model dispatch ]
//     ]

// let gameRulesAndSettingsView model dispatch =
//     Html.div [
//         SharedViewModule.modalInstructionContent pivotPointsDescriptions
//         modalGameSettingsView model dispatch
//     ]

// let gameContentViewControls model controlList dispatch =
//     Html.div [
//         prop.className "pt-4 flex flex-wrap gap-4 justify-center items-center"
//         prop.children (
//             [ roundStateToggle model dispatch ] @
//             [
//                 for (controlTitle: string, controlMsg) in controlList ->
//                     Html.button [
//                         prop.className "modalControls text-white text-lg font-semibold hover:text-green-400"
//                         prop.text controlTitle
//                         prop.onClick (fun _ -> dispatch controlMsg)
//                     ]
//             ]
//         )
//     ]

// let pivotPointTileView rollDirection position style message dispatch =
//     let iconPath =
//         match position with
//         | Ball -> SharedViewModule.GamePieceIcons.directionArrowImage rollDirection
//         | Blocker -> SharedViewModule.GamePieceIcons.blocker
//         | Goal -> SharedViewModule.GamePieceIcons.goalFlag
//         | _ -> SharedViewModule.GamePieceIcons.empty

//     Html.div [
//         prop.className "w-10 h-10 flex items-center justify-center rounded shadow cursor-pointer bg-gray-800"
//         prop.style style
//         prop.onClick (fun _ -> message |> dispatch)
//         prop.children [ Html.div [ prop.text iconPath; prop.className "w-6 h-6" ] ]
//     ]

// let parseLaneDetails (details: LaneDetails option) =
//     match details with
//     | Some d -> d.Style, d.Message
//     | None -> [], SharedPivotPoint.Ignore

type LaneHighlight =
    | NoHighlight
    | HighlightAscend
    | HighlightDescend

let private pivotTileBase (sizePx: int) : IStyleAttribute list =
    [
        style.width sizePx
        style.height sizePx
        style.custom("border-radius", "0")
        style.custom("clip-path", "polygon(10% 0, 100% 0, 100% 90%, 90% 100%, 0 100%, 0 10%)")
        style.position.relative
        style.display.flex
        style.alignItems.center
        style.justifyContent.center
        style.userSelect.none
    ]

let private pivotTileStyle (highlight: LaneHighlight) : IStyleAttribute list =
    // baseline “empty tile”
    let bg = "rgba(10, 20, 40, 0.55)"
    let border = "1px solid rgba(255,255,255,0.10)"
    let shadow = "inset 0 0 10px rgba(0,0,0,0.55)"

    // lane highlight overrides (match your red/blue intent but cyber)
    let bg', border', shadow', anim =
        match highlight with
        | HighlightAscend ->
            "linear-gradient(135deg, rgba(0,255,255,0.14), rgba(0,139,139,0.14))",
            "1px solid rgba(0,255,255,0.35)",
            "inset 0 0 16px rgba(0,255,255,0.10), 0 0 12px rgba(0,255,255,0.10)",
            Some "neonPulse 2s ease-in-out infinite"
        | HighlightDescend ->
            "linear-gradient(135deg, rgba(255,0,100,0.16), rgba(139,0,0,0.14))",
            "1px solid rgba(255,0,100,0.35)",
            "inset 0 0 16px rgba(255,0,100,0.10), 0 0 12px rgba(255,0,100,0.10)",
            None
        | NoHighlight ->
            bg, border, shadow, None

    [
        style.custom("background", bg')
        style.custom("border", border')
        style.custom("box-shadow", shadow')
        match anim with
        | Some a -> style.custom("animation", a)
        | None -> style.custom("animation", "none")
    ]

// let private pivotIcon (rollDirection: Client.Domain.GridGame.MovementDirection) (position: LaneObject) =
//     match position with
//     | Ball -> SharedViewModule.GamePieceIcons.directionArrowImage rollDirection
//     | Blocker -> SharedViewModule.GamePieceIcons.blocker
//     | Goal -> SharedViewModule.GamePieceIcons.goalFlag
//     | _ -> SharedViewModule.GamePieceIcons.empty

let private pivotTileContent (rollDirection: Client.Domain.GridGame.MovementDirection) (position: LaneObject) =
    match position with
    | Blocker ->
        [
            // hatch overlay
            Html.div [
                prop.className "absolute inset-0"
                prop.style [
                    style.custom(
                        "background",
                        "repeating-linear-gradient(45deg, transparent, transparent 8px, rgba(255,0,100,0.18) 8px, rgba(255,0,100,0.18) 16px)"
                    )
                ]
            ]
            // center glyph
            Html.div [
                prop.className "text-[24px] font-black"
                prop.style [
                    style.custom("color", "#ff2b7a")
                    style.custom("text-shadow", "0 0 14px rgba(255,0,100,0.55)")
                    style.custom("filter", "drop-shadow(0 0 10px rgba(255,0,100,0.35))")
                ]
                // Use your existing icon string, but styled
                prop.text SharedViewModule.GamePieceIcons.blocker
            ]
        ]

    | Goal ->
        [
            Html.div [
                prop.className "absolute inset-0 flex items-center justify-center"
                prop.style [
                    style.custom("animation", "float 2s ease-in-out infinite")
                ]
                prop.children [
                    Html.div [
                        prop.className "text-[26px] font-black"
                        prop.style [
                            style.custom("color", "#00ffff")
                            style.custom("text-shadow", "0 0 16px rgba(0,255,255,0.7)")
                            style.custom("filter", "drop-shadow(0 0 10px rgba(0,255,255,0.6))")
                        ]
                        prop.text SharedViewModule.GamePieceIcons.goalFlag
                    ]
                ]
            ]
        ]

    | Ball ->
        let dirGlyph = SharedViewModule.GamePieceIcons.directionArrowImage rollDirection
        [
            // outer glow
            Html.div [
                prop.className "absolute"
                prop.style [
                    style.custom("inset", "-10px")
                    style.custom("border-radius", "9999px")
                    style.custom("background", "radial-gradient(circle, rgba(0,255,255,0.45) 0%, transparent 70%)")
                    style.custom("filter", "blur(14px)")
                ]
            ]

            // orb
            Html.div [
                prop.className "absolute"
                prop.style [
                    style.custom("inset", "8px")
                    style.custom("border-radius", "9999px")
                    style.custom(
                        "background",
                        "radial-gradient(circle at 35% 35%, #00ffff, #0099cc 50%, #006699 100%)"
                    )
                    style.custom("border", "2px solid rgba(0,255,255,0.55)")
                    style.custom("box-shadow", "0 0 26px rgba(0,255,255,0.65), inset 0 0 18px rgba(255,255,255,0.25)")
                    style.custom("overflow", "hidden")
                ]
                prop.children [
                    // glossy highlight
                    Html.div [
                        prop.className "absolute"
                        prop.style [
                            style.custom("top", "18%")
                            style.custom("left", "18%")
                            style.custom("width", "35%")
                            style.custom("height", "35%")
                            style.custom("border-radius", "9999px")
                            style.custom("background", "radial-gradient(circle, rgba(255,255,255,0.65) 0%, transparent 70%)")
                            style.custom("filter", "blur(3px)")
                        ]
                    ]
                    // subtle scan stripes
                    Html.div [
                        prop.className "absolute inset-0"
                        prop.style [
                            style.custom(
                                "background-image",
                                "repeating-linear-gradient(45deg, transparent, transparent 4px, rgba(0,255,255,0.10) 4px, rgba(0,255,255,0.10) 5px)"
                            )
                            style.custom("opacity", "0.35")
                        ]
                    ]
                ]
            ]

            // optional direction hint on top of the ball
            Html.div [
                prop.className "relative z-10 text-[16px] font-black"
                prop.style [
                    style.custom("color", "#e8ffff")
                    style.custom("text-shadow", "0 0 10px rgba(0,255,255,0.65)")
                ]
                prop.text dirGlyph
            ]
        ]

    | _ ->
        // empty tile => no content
        []

let pivotPointTileView
    (rollDirection: Client.Domain.GridGame.MovementDirection)
    (position: LaneObject)
    (highlight: LaneHighlight)
    (message: SharedPivotPoint.Msg)
    (dispatch: SharedPivotPoint.Msg -> unit) =

    let sizePx = 64

    Html.button [
        prop.type'.button
        prop.className "tile select-none"
        prop.style (
            pivotTileBase sizePx
            @ pivotTileStyle highlight
            @ [
                style.custom("cursor", "pointer")
                style.custom("transition", "transform 0.15s ease, filter 0.15s ease")
            ]
        )
        prop.onClick (fun _ -> dispatch message)
        prop.children [
            Html.div [
                prop.className "text-[22px] font-black"
                prop.style [
                    style.custom("color", "#cfffff")
                    style.custom("text-shadow", "0 0 10px rgba(0,255,255,0.25)")
                    // bomb/goal/etc could be colored per-icon later if desired
                ]
                prop.children (pivotTileContent rollDirection position)
            ]
        ]
    ]

let parseLaneDetails (details: LaneDetails option) : LaneHighlight * SharedPivotPoint.Msg =
    match details with
    | Some d ->
        match d.Message with
        | SharedPivotPoint.PivotArrow SharedPivotPoint.PivotDirection.Ascend ->
            HighlightAscend, d.Message
        | SharedPivotPoint.PivotArrow SharedPivotPoint.PivotDirection.Descend ->
            HighlightDescend, d.Message
        | _ ->
            NoHighlight, d.Message
    | None ->
        NoHighlight, SharedPivotPoint.Ignore


let pivotPointsLaneCreator isRow rollDirection laneDetail rowPositions dispatch =
    let highlight, message = parseLaneDetails laneDetail
    Html.div [
        prop.className (if isRow then "flex justify-center items-center gap-1 my-1" else "")
        prop.children [
            for obj in rowPositions ->
                pivotPointTileView rollDirection obj highlight message dispatch
        ]
    ]


// let pivotPointsLaneCreator isRow rollDirection laneDetail rowPositions dispatch =
//     let style, message = parseLaneDetails laneDetail
//     Html.div [
//         prop.className (if isRow then "flex justify-center items-center space-x-1 my-1" else "")
//         prop.children [
//             for obj in rowPositions ->
//                 pivotPointTileView rollDirection obj style message dispatch
//         ]
//     ]

let laneStyleCenterPositions ceiling position =
    [ for i in 0 .. ceiling - 1 ->
        if i < position then Some { Style = [ style.backgroundColor "#801515" ]; Message = SharedPivotPoint.PivotArrow SharedPivotPoint.PivotDirection.Descend }
        elif i > position then Some { Style = [ style.backgroundColor "#143054" ]; Message = SharedPivotPoint.PivotArrow SharedPivotPoint.PivotDirection.Ascend }
        else None ]

let laneStyleCreator ballLaneIndex ceiling =
    if ballLaneIndex = 0 then floorLane
    elif ballLaneIndex = 7 then ceilingLane
    else laneStyleCenterPositions ceiling ballLaneIndex

let findBallLaneIndex (gridLanes: LaneObject list list) =
    gridLanes
    |> List.mapi (fun i lane -> if List.contains Ball lane then Some i else None)
    |> List.choose id
    |> List.tryHead
    |> Option.defaultValue -1

let laneView isRow (model: SharedPivotPoint.Model) (board: list<list<LaneObject>>) (laneStyles: list<LaneDetails option>) ceiling dispatch =
    Html.div [
        prop.className (if isRow then "flex flex-col justify-center items-center" else "flex flex-row justify-center items-center")
        prop.children [
            for i in 0 .. ceiling - 1 ->
                pivotPointsLaneCreator isRow model.BallDirection (laneStyles[i]) (board[i]) dispatch
        ]
    ]

let pivotPointsBoardView (model: SharedPivotPoint.Model) dispatch =
    let ceiling = 8
    let gridBoard = model.GameBoard

    match model.BallDirection with
    | MovementDirection.Left | MovementDirection.Right ->
        let board = GridGame.getPositionsAsRows gridBoard ceiling
        let ballLaneIndex = findBallLaneIndex board
        let laneStyles = laneStyleCreator ballLaneIndex ceiling
        laneView true model board laneStyles ceiling dispatch

    | MovementDirection.Up | MovementDirection.Down ->
        let board = GridGame.getPositionsAsColumns gridBoard ceiling
        let ballLaneIndex = findBallLaneIndex board
        let laneStyles = laneStyleCreator ballLaneIndex ceiling
        laneView false model board laneStyles ceiling dispatch


[<ReactComponent>]
let view (model: SharedPivotPoint.Model) (dispatch: SharedPivotPoint.Msg -> unit) (quitMsg: 'parentMsg) (dispatchParent: 'parentMsg -> unit) =
    let (showInfo, setShowInfo) = React.useState(false)

    let checkIfDirectionHorizontal (model: SharedPivotPoint.Model) =
        match model.BallDirection with
        | Up | Down -> false
        | Left | Right -> true

    let dpadState =
        { CanUp = checkIfDirectionHorizontal model
          CanDown = checkIfDirectionHorizontal model
          CanLeft = checkIfDirectionHorizontal model |> not
          CanRight = checkIfDirectionHorizontal model |> not
          Disabled = (model.GameState = Won) }

    let overlay : ReactElement option =
        match model.GameState with
        | Won ->
            Some (
                WinOverlay model.CoinsCollected
                    (fun () -> dispatch SharedPivotPoint.ResetRound)
                    (Some (fun () -> dispatch SharedPivotPoint.ResetRound))
            )
        | _ -> None

    let toggleString = roundStateToggleString model

    CyberShellResponsive {
        Left = 
            Html.div 
                [
                    TitlePanel "PIVOT POINT"
                    CyberPanel {
                        ClassName = "p-5"
                        ClipPath = Some "polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%)"
                        Children = [
                            Html.div [
                                prop.style [ style.display.flex; style.justifyContent.spaceAround; style.alignItems.center ]
                                prop.children [
                                    StatBlock "" $"Game Clock: {SharedViewModule.gameTickClock model.GameClock}" "#ff00ff"
                                    StatBlock " " $"Score: {model.CoinsCollected}" "#ff00ff"
                                ]
                            ]
                        ]
                    }
                    if showInfo 
                    then 
                        InstructionsPanel 
                            "HOW TO PLAY" 
                            pivotPointsDescriptions
                            "CLOSE"
                            (fun () -> setShowInfo(false))
                    else 
                        DPadInfoPanel
                            "CLOCK"
                            (string model.GameClock)
                            "SCORE"
                            (string model.CoinsCollected)
                            dpadState 
                            (fun dir ->
                                match dir with
                                | Up | Left -> dispatch (SharedPivotPoint.PivotArrow SharedPivotPoint.PivotDirection.Descend)
                                | Down | Right -> dispatch (SharedPivotPoint.PivotArrow SharedPivotPoint.PivotDirection.Ascend)
                            )
                    ControlsPanel [
                        ControlButton "INFO" Cyan true (fun () -> setShowInfo(not showInfo)) (Some (LucideIcon.Info "w-4 h-4"))
                        ControlButton toggleString Purple (true) (fun () -> startGameLoop model dispatch |> ignore ) (Some (LucideIcon.RotateCcw "w-4 h-4"))
                        ControlButton "RESTART" Red true (fun () -> dispatch SharedPivotPoint.ResetRound) None
                    ]
                ]
        Board = BoardPanel ( pivotPointsBoardView model dispatch )
        Overlay = overlay
        OnQuit = (fun () -> dispatchParent quitMsg)
    }
