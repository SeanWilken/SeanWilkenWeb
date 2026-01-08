module Components.FSharp.Portfolio.Games.PivotPoints

open Elmish
open Browser
open Feliz
open SharedViewModule.SharedMicroGames
open Bindings.LucideIcon
open Client.GameDomain
open Client.GameDomain.GridGame
    
// - Extras:
// speeds up like snake as more are picked up
// certain coins have certain effects (?) // turns to blocker // speed up round // reverse roll direction // etc..(?)

type PivotDirection =
    | Ascend
    | Descend

type Msg =
    // Game State
    | GameLoopTick
    | SetGameState of RoundState
    | SetDispatchPointer of float
    // Arrow Movement
    | MoveArrow // will be called on certain game ticks to move the balls position
    | PivotArrow of PivotDirection // pivot to either ascend or descend
    // round messages
    | Ignore // I really don't like this, how to assign NO message..need separate funcs
    | ResetRound // resets the board and player round details
    | EndRound // you crashed and will be brought to game over screen
    | ExitGameLoop // Call to ensure no window intervals running when game is exited
    | QuitGame // Leave this game and return to the code gallery

type LaneOrientation =
    | LaneRow
    | LaneColumn

type Model = {
    GameBoard : GridBoard // the playing game board
    BoardOrientation: LaneOrientation
    GameState : RoundState
    DispatchPointer: float
    GameClock: int
    RollInterval: int
    BallDirection: MovementDirection // direction of ball's momentum
    BallPosition: int // position of the ball currently
    CoinPosition: int // position of the coin to collect
    CoinsCollected: int // # of coins obtained
}

let demoGameBoard = {
    GridPositions = [
        Blocker; Blank; Blank; Blank; Blank; Blocker; Blank; Blank;
        Blank; Blocker; Blank; Blank; Blank; Blank; Blank; Blank;
        Blank; Blank; Blank; Blank; Blank; Blank; Blocker; Blank;
        Blank; Blocker; Blank; Blank; Blank; Blank; Blank; Blank;
        Blank; Blank; Blocker; Blank; Goal; Blank; Blank; Blank;
        Blank; Blank; Blocker; Blank; Blank; Blank; Blank; Blank;
        Ball; Blank; Blank; Blank; Blank; Blocker; Blank; Blank;
        Blank; Blank; Blank; Blank; Blank; Blank; Blank; Blocker;
    ] }

let initModel = {
    GameBoard = demoGameBoard
    GameState = Paused
    BoardOrientation = LaneColumn
    GameClock = 0
    RollInterval = 0
    DispatchPointer = 0.0
    BallDirection = Right
    BallPosition = 11
    CoinPosition = 37
    CoinsCollected = 0
}


type LaneDetails = {
    Style : Feliz.IStyleAttribute list
    Message : Msg
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

let modelPivotPointRoundDetails ( model : Model ) =
    [
        "You collected " + string model.CoinsCollected + " coins."
        "You lasted " + string ( SharedViewModule.gameTickClock model.GameClock ) + " seconds."
    ]

let controlList = [ 
    "Settings", (SetGameState (RoundState.Settings)) 
]

let ascendLane = Some { Style = [ style.color "#801515" ]; Message = PivotArrow Ascend }
let descendLane = Some { Style = [ style.color "#143054" ]; Message = PivotArrow Descend }
let floorLane = [ None; ascendLane; ascendLane; ascendLane; ascendLane; ascendLane; ascendLane; descendLane; ]
let ceilingLane = [ ascendLane; descendLane; descendLane; descendLane; descendLane; descendLane; descendLane; None ]

// Update functions ---------

// shared between goal roll and pivot points...
let moveArrowForward ( model : Model) =
    let gameBoard = model.GameBoard
    let piecePositionIndex = RollableGridGameHelpers.getPiecePositionIndex gameBoard Ball
    if piecePositionIndex = -1
        then gameBoard
        else RollableGridGameHelpers.checkDirectionMovement piecePositionIndex model.BallDirection gameBoard

// GAME LOOP FUNCTIONS

// Time drives main game state, as things happen in intervals contained within the main loop
let startGameLoop ( model : Model ) dispatch =
    if model.DispatchPointer = 0.0 && model.GameState = Paused
        then
            window.setInterval((fun _ -> dispatch (GameLoopTick)), 250)
            |> fun loopFloat -> dispatch (SetDispatchPointer loopFloat)
        else
            SharedViewModule.stopGameLoop model.DispatchPointer
            dispatch ( SetDispatchPointer 0.0 )

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
        availablePositions.Item(randomIndex availablePositions.Length)

//----------------

// Lifecycle -------------

let init(): Model * Cmd<Msg> =
    initModel, Cmd.none

let update msg ( model : Model ) : Model * Cmd<Msg> = 
    match msg with
    | GameLoopTick ->
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
            then { model with GameBoard = gridWithCoinUpdate; RollInterval = 0; GameClock = tickedClock; CoinsCollected = model.CoinsCollected + coinPoints }, Cmd.ofMsg MoveArrow
            else { model with GameBoard = gridWithCoinUpdate; RollInterval = rollInterval; GameClock = tickedClock; CoinsCollected = model.CoinsCollected + coinPoints }, Cmd.none
    | SetDispatchPointer pointer ->
        if model.GameState = Won 
            then initModel, Cmd.none
            else
                if pointer <> 0.0 then Playing else Paused
                |> fun gameRoundState -> { model with GameState = gameRoundState; DispatchPointer = pointer }, Cmd.none
    | SetGameState gameState ->
        if gameState <> Playing && model.DispatchPointer <> 0.0 
            then 
                SharedViewModule.stopGameLoop model.DispatchPointer
                { model with DispatchPointer = 0.0; GameState = gameState; }, Cmd.none
            else
                { model with GameState = gameState }, Cmd.none
    | PivotArrow pivotDirection ->
        match model.BallDirection with
        | MovementDirection.Up
        | MovementDirection.Down ->
            match pivotDirection with
            | Ascend ->
                { model with BallDirection = MovementDirection.Right; BoardOrientation = LaneOrientation.LaneRow }, Cmd.none
            | Descend ->
                { model with BallDirection = MovementDirection.Left; BoardOrientation = LaneOrientation.LaneRow }, Cmd.none
        | MovementDirection.Left
        | MovementDirection.Right ->
            match pivotDirection with
            | Ascend ->
                { model with BallDirection = Down; BoardOrientation = LaneOrientation.LaneColumn }, Cmd.none
            | Descend ->
                { model with BallDirection = Up; BoardOrientation = LaneOrientation.LaneColumn }, Cmd.none
    | MoveArrow ->
        let movedArrowGrid = moveArrowForward model
        if movedArrowGrid = model.GameBoard
            then model, Cmd.ofMsg EndRound
            else { model with GameBoard = movedArrowGrid }, Cmd.none
    | ResetRound ->
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        initModel, Cmd.none
    | EndRound ->
        SharedViewModule.stopGameLoop model.DispatchPointer
        { model with GameBoard = demoGameBoard; DispatchPointer = 0.0; GameState = Won }, Cmd.none
    | ExitGameLoop ->
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        model, Cmd.none
    | QuitGame ->
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        model, Cmd.none
    | _ -> model, Cmd.none

// --------------------------

// VIEW FUNCTIONS 
let roundStateToggleString (model: Model) =
    match model.GameState with
    | Won | Settings -> "Play"
    | _ when model.DispatchPointer <> 0.0 -> "Pause"
    | _ when model.GameClock <> 0 -> "Resume"
    | _ -> "Start"

let roundStateToggle (model: Model) dispatch =
    let toggleStr = roundStateToggleString model
    Html.button [
        prop.className "modalControls text-white hover:text-green-400"
        prop.onClick (fun _ -> startGameLoop model dispatch |> ignore)
        prop.text toggleStr
    ]

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


let private pivotTileContent (rollDirection: MovementDirection) (position: LaneObject) =
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
    (rollDirection: MovementDirection)
    (position: LaneObject)
    (highlight: LaneHighlight)
    (message: Msg)
    (dispatch: Msg -> unit) =

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

let parseLaneDetails (details: LaneDetails option) : LaneHighlight * Msg =
    match details with
    | Some d ->
        match d.Message with
        | PivotArrow Ascend ->
            HighlightAscend, d.Message
        | PivotArrow Descend ->
            HighlightDescend, d.Message
        | _ ->
            NoHighlight, d.Message
    | None ->
        NoHighlight, Ignore


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
        if i < position then Some { Style = [ style.backgroundColor "#801515" ]; Message = PivotArrow Descend }
        elif i > position then Some { Style = [ style.backgroundColor "#143054" ]; Message = PivotArrow Ascend }
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

let laneView isRow (model: Model) (board: list<list<LaneObject>>) (laneStyles: list<LaneDetails option>) ceiling dispatch =
    Html.div [
        prop.className (if isRow then "flex flex-col justify-center items-center" else "flex flex-row justify-center items-center")
        prop.children [
            for i in 0 .. ceiling - 1 ->
                pivotPointsLaneCreator isRow model.BallDirection (laneStyles[i]) (board[i]) dispatch
        ]
    ]

let pivotPointsBoardView (model: Model) dispatch =
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
let view (model: Model) (dispatch: Msg -> unit) (quitMsg: 'parentMsg) (dispatchParent: 'parentMsg -> unit) =
    let (showInfo, setShowInfo) = React.useState(false)

    let checkIfDirectionHorizontal (model: Model) =
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
                    (fun () -> dispatch ResetRound)
                    (Some (fun () -> dispatch ResetRound))
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
                                | Up | Left -> dispatch (PivotArrow Descend)
                                | Down | Right -> dispatch (PivotArrow Ascend)
                            )
                    ControlsPanel [
                        ControlButton "INFO" Cyan true (fun () -> setShowInfo(not showInfo)) (Some (LucideIcon.Info "w-4 h-4"))
                        ControlButton toggleString Purple (true) (fun () -> startGameLoop model dispatch |> ignore ) (Some (LucideIcon.RotateCcw "w-4 h-4"))
                        ControlButton "RESTART" Red true (fun () -> dispatch ResetRound) None
                    ]
                ]
        Board = BoardPanel ( pivotPointsBoardView model dispatch )
        Overlay = overlay
        OnQuit = (fun () -> dispatchParent quitMsg)
    }
