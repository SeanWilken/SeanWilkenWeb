module Components.FSharp.Portfolio.Games.GoalRoll

open Elmish
open Fable.React
open Feliz
open Client.Domain
open GridGame
open SharedGoalRoll
open SharedViewModule.SharedMicroGames

// Content descriptions
let goalRollDescriptions = [
    "- Use the arrows next to the ball in order to roll it in the desired direction."
    "- Travels in straight lines and stops when it hits a wall or blocked tile."
    "- There must be space for the movement arrow in order to roll."
    "- Have the ball stop on the goal to win."
]

// Main controls
let controlList = [ 
    "Play", (SetGameState Playing)
    "Settings", (SetGameState Settings) 
]

let gameControls = [
    "Reset Round", ResetRound
    "Level 0", LoadRound 0
    "Level 1", LoadRound 1
    "Level 2", LoadRound 2
    "Level 3", LoadRound 3
]

let modelGoalRollRoundDetails (model: SharedGoalRoll.Model) = [
    $"You completed level {model.LevelIndex}."
    $"It took you {model.MovesMade} number of moves."
]

// Helpers
let getBallRollPositionIndex pos dir =
    match dir with
    | Up -> pos - 8 | Down -> pos + 8
    | Left -> pos - 1 | Right -> pos + 1

let getNormalizedArrowPosition pos dir =
    match dir with
    | Up -> pos - 8 | Down -> pos + 8
    | _ -> pos % 8

let checkNormalizedArrowPosition pos dir =
    match dir with
    | Up -> pos > 0 | Down -> pos <= 64
    | Left -> pos <> 1 | Right -> pos <> 0

let gridWithoutMoveArrows g =
    { GridPositions = List.map (function MoveArrow _ -> Blank | x -> x) g.GridPositions }

let gridWithMovementArrow g dir =
    let ballIdx = SharedGoalRoll.getBallPositionIndex g
    let normBall = ballIdx + 1
    let normArrow = getNormalizedArrowPosition normBall dir
    if checkNormalizedArrowPosition normArrow dir then
        let targetIdx = getBallRollPositionIndex ballIdx dir
        if checkGridPositionForObject g targetIdx Blank then
            updatePositionWithObject g (MoveArrow dir) targetIdx
        else g
    else g

let gridWithGoal g goalPos = updatePositionWithObject g Goal goalPos

let checkDirectionForRollable pos grid targetIdx ballIdx =
    if checkGridPositionForObject grid targetIdx Blank || checkGridPositionForObject grid targetIdx Goal then
        updatePositionWithObject (updatePositionWithObject grid Blank ballIdx) Ball targetIdx
    else pos

let rec rollBall grid dir =
    let ballIdx = RollableGridGameHelpers.getPiecePositionIndex grid Ball
    let stripped = gridWithoutMoveArrows grid
    let targetIdx = getBallRollPositionIndex ballIdx dir
    if RollableGridGameHelpers.checkDirectionalBound targetIdx dir then
        let nextGrid = checkDirectionForRollable grid stripped targetIdx ballIdx
        if nextGrid = grid then grid else rollBall nextGrid dir
    else grid

// STATE
let init () = SharedGoalRoll.initModel, Cmd.none

let update msg model =
    match msg, model with
    | SetGameState s, m when m.GameState = Won -> { m with GameState = s; MovesMade = 0 }, Cmd.none
    | SetGameState s, m -> { m with GameState = s }, Cmd.none
    | RollBall d, m ->
        let newGrid = rollBall m.CurrentGrid d
        { m with CurrentGrid = newGrid; MovesMade = m.MovesMade + 1 }, Cmd.ofMsg CheckSolution
    | LoadRound lvl, _ ->
        let grid = SharedGoalRoll.loadRound lvl
        { LevelIndex = lvl; InitialGrid = grid; CurrentGrid = grid;
          BallPositionIndex = SharedGoalRoll.getBallPositionIndex grid;
          GoalPositionIndex = SharedGoalRoll.getGoalPositionIndex grid;
          GameState = Playing; MovesMade = 0 }, Cmd.none
    | ResetRound, m ->
        let grid = m.InitialGrid
        { m with CurrentGrid = grid; BallPositionIndex = SharedGoalRoll.getBallPositionIndex grid;
                  GameState = Playing; MovesMade = 0 }, Cmd.none
    | CheckSolution, m when SharedGoalRoll.getBallPositionIndex m.CurrentGrid = m.GoalPositionIndex ->
        let grid = m.InitialGrid
        { m with CurrentGrid = grid; BallPositionIndex = SharedGoalRoll.getBallPositionIndex grid;
                  GameState = Won }, Cmd.none
    | CheckSolution, m -> m, Cmd.none
    | QuitGame, m -> m, Cmd.ofMsg QuitGame

// VIEW
// let goalRollRowCreator row dispatch =
//     Html.div [
//         prop.className "flex justify-center items-center space-x-1 my-1"
//         prop.children [
//             for pos in row ->
//                 let (cls: string, txt: string, onClick) =
//                     match pos with
//                     | Blocker -> "bg-gray-800", SharedViewModule.GamePieceIcons.blocker, None
//                     | Ball -> "bg-blue-500", SharedViewModule.GamePieceIcons.ball, None
//                     | Goal -> "bg-green-600", SharedViewModule.GamePieceIcons.goalFlag, None
//                     | Heart -> "bg-pink-400", SharedViewModule.GamePieceIcons.heart, None
//                     | LaneLock -> "bg-yellow-600", SharedViewModule.GamePieceIcons.lock, None
//                     | LaneKey -> "bg-indigo-500", SharedViewModule.GamePieceIcons.key, None
//                     | Bomb -> "bg-red-700", SharedViewModule.GamePieceIcons.bomb, None
//                     | MoveArrow dir -> 
//                         "bg-base-300 hover:bg-base-200 cursor-pointer",
//                         SharedViewModule.GamePieceIcons.directionArrowImage dir,
//                         Some (fun _ -> dispatch (RollBall dir))
//                     | _ -> "bg-gray-900", SharedViewModule.GamePieceIcons.empty, None

//                 Html.div [
//                     prop.className ($"{cls} w-10 h-10 flex items-center justify-center rounded shadow text-sm")
//                     prop.text txt
//                     prop.onClick (fun _ -> match onClick with Some f -> f() | _ -> ())

//                 ]
//         ]
//     ]

// let goalRollLevelCreator model dispatch =
//     let g = model.CurrentGrid
//     let arrows = [Up; Down; Left; Right]
//     let withArrows = List.fold gridWithMovementArrow g arrows
//     let withGoal = if getGoalPositionIndex g = -1 then gridWithGoal withArrows model.GoalPositionIndex else withArrows
//     let rows = getPositionsAsRows withGoal 8
//     Html.div [ for row in rows -> goalRollRowCreator row dispatch ]

// let goalRollModalContent model dispatch =
//     SharedViewModule.gameModalContent (
//         Html.div [
//             match model.GameState with
//             | Won ->  SharedViewModule.roundCompleteContent (modelGoalRollRoundDetails model) (fun () -> SharedGoalRoll.Msg.ResetRound |> dispatch)
//             | _ -> 
//                 Html.div [
//                     prop.className "my-4"
//                     prop.children [ goalRollLevelCreator model dispatch ]
//                 ]
//         ]
//     )

let getPositionsAsRows (positions: GridBoard) gridDimension =
    Seq.toList (seq { 
        for i in 0 .. (gridDimension - 1) do
            yield Seq.toList (seq {
                for n in 0 .. (gridDimension - 1) do
                    yield! seq{ 
                        positions.GridPositions.Item(n + (i * gridDimension))
                    }
            })
    })

let goalRollRowCreator (row: LaneObject list) (dispatch: Msg -> unit) =
    Html.div [
        // replace flex-row rows with grid sizing that matches mock gap
        prop.className "flex justify-center items-center gap-1 my-1"
        prop.children [
            for pos in row do

                // Common palette strings (match mock)
                let emptyBg = "rgba(10, 20, 40, 0.5)"
                let emptyBorder = "1px solid rgba(255, 255, 255, 0.10)"
                let emptyShadow = "inset 0 0 10px rgba(0,0,0,0.5)"

                match pos with
                | Blocker ->
                    mkTile
                        "linear-gradient(135deg, rgba(255, 0, 100, 0.40), rgba(139, 0, 0, 0.40))"
                        "1px solid rgba(255, 0, 100, 0.60)"
                        "inset 0 0 20px rgba(255,0,100,0.30), 0 0 15px rgba(255,0,100,0.20)"
                        None
                        [
                            // hatch overlay
                            Html.div [
                                prop.className "absolute inset-0"
                                prop.style [
                                    style.custom(
                                        "background",
                                        "repeating-linear-gradient(45deg, transparent, transparent 8px, rgba(255, 0, 100, 0.20) 8px, rgba(255, 0, 100, 0.20) 16px)"
                                    )
                                ]
                            ]
                            centered (Html.div [
                                prop.className "text-[24px] opacity-60"
                                prop.style [ style.color "#ff0066" ]
                                prop.text "✕"
                            ])
                        ]

                | Goal ->
                    mkTile
                        "linear-gradient(135deg, rgba(0, 255, 255, 0.40), rgba(0, 139, 139, 0.40))"
                        "1px solid rgba(0, 255, 255, 0.80)"
                        "inset 0 0 20px rgba(0,255,255,0.30), 0 0 20px rgba(0,255,255,0.30)"
                        (Some "glow 2s ease-in-out infinite")
                        [
                            centered (
                                Html.div [
                                    prop.className "text-[32px] font-bold"
                                    prop.style [
                                        style.color "#00ffff"
                                        style.custom("text-shadow", "0 0 10px rgba(0,255,255,0.8)")
                                    ]
                                    // swap this to a Lucide trophy if you prefer
                                    prop.text SharedViewModule.GamePieceIcons.goalFlag
                                ]
                            )
                        ]

                | MoveArrow dir ->
                    // Clickable “available direction” tile: cyan pulse
                    mkTile
                        "rgba(0, 255, 255, 0.15)"
                        "1px solid rgba(0, 255, 255, 0.60)"
                        "inset 0 0 15px rgba(0,255,255,0.20), 0 0 15px rgba(0,255,255,0.30)"
                        (Some "neonPulse 2s ease-in-out infinite")
                        [
                            Html.button [
                                prop.className "direction-btn absolute inset-0 flex items-center justify-center"
                                prop.onClick (fun _ -> dispatch (RollBall dir))
                                prop.style [
                                    style.custom("background", "transparent")
                                    style.borderWidth 0
                                    style.cursor.pointer
                                ]
                                prop.children [
                                    Html.div [
                                        prop.className "text-[20px] font-bold"
                                        prop.style [
                                            style.color "#00ffff"
                                            style.custom("text-shadow", "0 0 10px rgba(0,255,255,1)")
                                        ]
                                        prop.text (SharedViewModule.GamePieceIcons.directionArrowImage dir)
                                    ]
                                ]
                            ]
                        ]

                | Ball ->
                    // Dark tile background + orb glow
                    mkTile
                        emptyBg
                        emptyBorder
                        emptyShadow
                        None
                        [
                            // outer glow
                            Html.div [
                                prop.className "absolute"
                                prop.style [
                                    style.custom("inset", "-12px")
                                    style.custom("background", "radial-gradient(circle, rgba(0, 255, 255, 0.5) 0%, transparent 70%)")
                                    style.custom("border-radius", "9999px")
                                    style.custom("filter", "blur(15px)")
                                ]
                            ]
                            // orb
                            Html.div [
                                prop.className "absolute"
                                prop.style [
                                    style.custom("inset", "8px")
                                    style.custom("background", "radial-gradient(circle at 35% 35%, #00ffff, #0099cc 50%, #006699 100%)")
                                    style.custom("border-radius", "9999px")
                                    style.custom("box-shadow", "0 0 30px rgba(0,255,255,0.8), inset 0 0 20px rgba(255,255,255,0.3), inset 0 0 30px rgba(0,0,0,0.3)")
                                    style.custom("border", "2px solid rgba(0,255,255,0.6)")
                                    style.overflow.hidden
                                ]
                                prop.children [
                                    // highlight
                                    Html.div [
                                        prop.className "absolute"
                                        prop.style [
                                            style.top (length.perc 20)
                                            style.left (length.perc 20)
                                            style.width (length.perc 35)
                                            style.height (length.perc 35)
                                            style.custom("background", "radial-gradient(circle, rgba(255,255,255,0.7) 0%, transparent 70%)")
                                            style.custom("border-radius", "9999px")
                                            style.custom("filter", "blur(4px)")
                                        ]
                                    ]
                                    // subtle scan stripes
                                    Html.div [
                                        prop.className "absolute inset-0 opacity-30"
                                        prop.style [
                                            style.backgroundImage "repeating-linear-gradient(45deg, transparent, transparent 4px, rgba(0, 255, 255, 0.10) 4px, rgba(0, 255, 255, 0.10) 5px)"
                                        ]
                                    ]
                                ]
                            ]
                        ]

                // Keep your other pieces if you want; for now render them like “neutral tiles with centered icon”
                | Heart
                | LaneLock
                | LaneKey
                | Bomb ->
                    let icon =
                        match pos with
                        | Heart -> SharedViewModule.GamePieceIcons.heart
                        | LaneLock -> SharedViewModule.GamePieceIcons.lock
                        | LaneKey -> SharedViewModule.GamePieceIcons.key
                        | Bomb -> SharedViewModule.GamePieceIcons.bomb
                        | _ -> ""

                    mkTile
                        emptyBg
                        emptyBorder
                        emptyShadow
                        None
                        [
                            centered (
                                Html.div [
                                    prop.className "text-[22px] opacity-80"
                                    prop.style [ style.color "#9aa4b2" ]
                                    prop.text icon
                                ]
                            )
                        ]

                | _ ->
                    mkTile emptyBg emptyBorder emptyShadow None []
        ]
    ]

let goalRollLevelCreator model dispatch =
    let g = model.CurrentGrid
    let arrows = [ Up; Down; Left; Right ]
    let withArrows = List.fold gridWithMovementArrow g arrows
    let withGoal =
        if getGoalPositionIndex g = -1 then
            gridWithGoal withArrows model.GoalPositionIndex
        else
            withArrows

    let rows = getPositionsAsRows withGoal 8

    // This wrapper matches the mock “grid in a panel” spacing
    Html.div [
        prop.className "flex flex-col"
        prop.children [ for row in rows -> goalRollRowCreator row dispatch ]
    ]
let goalRollModalContent model dispatch =
    Html.div [
        match model.GameState with
        | Won ->
            SharedViewModule.roundCompleteContent (modelGoalRollRoundDetails model) (fun () -> ResetRound |> dispatch)
        | _ ->
            BoardPanel (
                Html.div [
                    // center grid exactly like the mock
                    prop.className "flex items-center justify-center"
                    prop.children [ goalRollLevelCreator model dispatch ]
                ]
            )
    ]


open SharedViewModule.SharedMicroGames
open Bindings.LucideIcon

let private instructionLines : string list =
    [ "→ Click arrows or use WASD/Arrow keys"
      "→ Ball rolls until hitting wall/blocker"
      "→ Collect keys to unlock goals"
      "→ Land on goal flag to complete"
      "→ Ctrl+Z or Undo button to step back" ]

let view (model: SharedGoalRoll.Model) (dispatch: SharedGoalRoll.Msg -> unit) (quitMsg: 'parentMsg) (dispatchParent: 'parentMsg -> unit) =
    let (showInfo, setShowInfo) = React.useState(false)

    // You probably already have this logic somewhere; map it to 4 bools:
    let canUp, canDown, canLeft, canRight =
        // TODO: plug your available-direction logic here
        true, true, true, true

    let dpadState =
        { CanUp = canUp
          CanDown = canDown
          CanLeft = canLeft
          CanRight = canRight
          Disabled = (model.GameState = Won) }

    let overlay : ReactElement option =
        match model.GameState with
        | Won ->
            Some (
                WinOverlay model.MovesMade
                    (fun () -> dispatch ResetRound)
                    (Some (fun () -> dispatch (LoadRound (model.LevelIndex + 1))))
            )
        | _ -> None

    CyberShellResponsive {
        Left = 
            Html.div 
                [
                    TitlePanel "GOAL ROLL"
                    LevelSelectPanel 
                        [1;2;3] 
                        model.LevelIndex
                        (fun a b -> a = b)
                        (fun lvl -> dispatch (LoadRound lvl))
                    if showInfo 
                    then 
                        InstructionsPanel 
                            "HOW TO PLAY" 
                            instructionLines
                            "CONTROLS"
                            (fun () -> setShowInfo(false))
                    else 
                        DPadInfoPanel 
                            "LEVEL"
                            (string model.LevelIndex)
                            "MOVES"
                            (string model.MovesMade)
                            dpadState 
                            (fun dir -> dispatch (RollBall dir))
                    ControlsPanel [
                        ControlButton "INFO" Cyan true (fun () -> setShowInfo(not showInfo)) (Some (LucideIcon.Info "w-4 h-4"))
                        // ControlButton "UNDO" Purple canUndo (fun () -> dispatch Undo) (Some (LucideIcon.RotateCcw "w-4 h-4"))
                        ControlButton "RESTART" Red true (fun () -> dispatch ResetRound) None
                    ]
                ]
        Board =  goalRollModalContent model dispatch
        Overlay = overlay
        OnQuit = (fun () -> dispatchParent quitMsg)
    }
