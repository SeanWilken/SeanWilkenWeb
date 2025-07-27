module GoalRoll

open Elmish
open Fable.React
open Feliz
open Shared
open Shared.GridGame
open Shared.SharedGoalRoll

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
let goalRollRowCreator row dispatch =
    Html.div [
        prop.className "flex justify-center items-center space-x-1 my-1"
        prop.children [
            for pos in row ->
                let (cls: string, txt: string, onClick) =
                    match pos with
                    | Blocker -> "bg-gray-800", SharedViewModule.GamePieceIcons.blocker, None
                    | Ball -> "bg-blue-500", SharedViewModule.GamePieceIcons.ball, None
                    | Goal -> "bg-green-600", SharedViewModule.GamePieceIcons.goalFlag, None
                    | Heart -> "bg-pink-400", SharedViewModule.GamePieceIcons.heart, None
                    | LaneLock -> "bg-yellow-600", SharedViewModule.GamePieceIcons.lock, None
                    | LaneKey -> "bg-indigo-500", SharedViewModule.GamePieceIcons.key, None
                    | Bomb -> "bg-red-700", SharedViewModule.GamePieceIcons.bomb, None
                    | MoveArrow dir -> 
                        "bg-base-300 hover:bg-base-200 cursor-pointer",
                        SharedViewModule.GamePieceIcons.directionArrowImage dir,
                        Some (fun _ -> dispatch (RollBall dir))
                    | _ -> "bg-gray-900", SharedViewModule.GamePieceIcons.empty, None

                Html.div [
                    prop.className ($"{cls} w-10 h-10 flex items-center justify-center rounded shadow text-sm")
                    prop.text txt
                    prop.onClick (fun _ -> match onClick with Some f -> f() | _ -> ())

                ]
        ]
    ]

let goalRollLevelCreator model dispatch =
    let g = model.CurrentGrid
    let arrows = [Up; Down; Left; Right]
    let withArrows = List.fold gridWithMovementArrow g arrows
    let withGoal = if getGoalPositionIndex g = -1 then gridWithGoal withArrows model.GoalPositionIndex else withArrows
    let rows = getPositionsAsRows withGoal 8
    Html.div [ for row in rows -> goalRollRowCreator row dispatch ]

let goalRollModalContent model dispatch =
    SharedViewModule.gameModalContent (
        match model.GameState with
        | Settings -> 
            Html.div [
                prop.className "grid grid-cols-1 md:grid-cols-2 gap-8 items-start"
                prop.children [
                    SharedViewModule.codeModalControlsContent gameControls dispatch
                    SharedViewModule.modalInstructionContent goalRollDescriptions
                ]
            ]
        | Won ->  SharedViewModule.roundCompleteContent (modelGoalRollRoundDetails model)
        | _ -> goalRollLevelCreator model dispatch
    )

let view model dispatch =
    Html.div [
        SharedViewModule.sharedModalHeader "Goal Roll" QuitGame dispatch
        goalRollModalContent model dispatch
        SharedViewModule.codeModalFooter controlList dispatch
    ]