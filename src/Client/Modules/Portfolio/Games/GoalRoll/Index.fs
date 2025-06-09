module GoalRoll

open Elmish
open Fable.React
open Feliz
open Shared
open Shared.GridGame
open Shared.SharedGoalRoll

// ---------TODO---------
    // - MovementMade (Undo Move)
    // - Roll Ball when next to flag (lvl3)
    // shared view

// content descriptions
let goalRollDescriptions = [
    "- Use the arrows next to the ball in order to roll it in the desired direction."
    "- Travels in straight lines and stops when it hits a wall or blocked tile."
    "- There must be space for the movement arrow in order to roll."
    "- Have the ball stop on the goal to win."
]

// content selection controls
let controlList = [ 
    "Play", (SetGameState (Playing))
    "Settings", (SetGameState (Settings)) 
]

let gameControls = [
    "Reset Round", ResetRound
    "Level 0", LoadRound 0
    "Level 1", LoadRound 1
    "Level 2", LoadRound 2
    "Level 3", LoadRound 3
]

let modelGoalRollRoundDetails ( model : SharedGoalRoll.Model ) = [
    "You completed level " + string model.LevelIndex + "."
    "It took you " + string model.MovesMade + " number of moves."
]

let levelCeiling = 3

let getBallRollPositionIndex ballPosition direction =
    match direction with
    | Up -> (ballPosition - 8)
    | Down -> (ballPosition + 8)
    | Left -> (ballPosition - 1)
    | Right -> (ballPosition + 1)

let getNormalizedArrowPosition normalizedBallPosition direction =
    match direction with
    | Up -> (normalizedBallPosition - 8)
    | Down -> (normalizedBallPosition + 8)
    | _ -> (normalizedBallPosition % 8)

let checkNormalizedArrowPosition normalizedArrowPosition direction =
    match direction with
    | Up -> normalizedArrowPosition > 0
    | Down -> normalizedArrowPosition <= 64
    | Left -> normalizedArrowPosition <> 1
    | Right -> normalizedArrowPosition <> 0

let gridWithoutMoveArrows positions =
    let thing = List.map (fun x -> match x with | MoveArrow _ -> Blank | _ -> x ) positions.GridPositions
    { GridPositions = thing }

let gridWithMovementArrow positions direction =
    let ballPositionIndex = SharedGoalRoll.getBallPositionIndex positions
    let normalizedBallPositionIndex = ballPositionIndex + 1
    let normalizedArrowPositionIndex = getNormalizedArrowPosition normalizedBallPositionIndex direction
    let validArrowPosition = checkNormalizedArrowPosition normalizedArrowPositionIndex direction
    if validArrowPosition
        then 
            let thing = getBallRollPositionIndex ballPositionIndex direction
            if (checkGridPositionForObject positions thing Blank)
                then updatePositionWithObject positions (MoveArrow direction) thing
                else positions
        else positions

let gridWithGoal positions goalPosition =
    updatePositionWithObject positions Goal goalPosition

// this should really check for some kind of rollable bit to indicate can roll through / over?
let checkDirectionForRollable positions arrowlessGrid ballRollPositionIndex ballPositionIndex =
    if (checkGridPositionForObject arrowlessGrid ballRollPositionIndex Blank) || (checkGridPositionForObject arrowlessGrid ballRollPositionIndex Goal)
        then 
            let ballToBlankGrid = updatePositionWithObject (arrowlessGrid) Blank (ballPositionIndex)
            let ballRolledGrid = updatePositionWithObject ballToBlankGrid Ball ballRollPositionIndex
            ballRolledGrid
        else positions

let rec rollBall positions direction =
    let ballPositionIndex =  RollableGridGameHelpers.getPiecePositionIndex positions Ball
    let arrowlessGrid = gridWithoutMoveArrows positions
    let ballRollPositionIndex = getBallRollPositionIndex ballPositionIndex direction
    if RollableGridGameHelpers.checkDirectionalBound ballRollPositionIndex direction
        then 
            let grid = checkDirectionForRollable positions arrowlessGrid ballRollPositionIndex ballPositionIndex
            if grid = positions
                then positions
                else rollBall grid direction
        else positions

// --------------------------------------------------------------
//STATE LIFECYCLE

let init (): SharedGoalRoll.Model * Cmd<Msg> =
    SharedGoalRoll.initModel, Cmd.none

let update ( msg: Msg ) ( model: SharedGoalRoll.Model ): SharedGoalRoll.Model * Cmd<Msg> =
    match msg, model with
    | SetGameState gameState, model ->
        if model.GameState = Won
            then { model with GameState = gameState; MovesMade = 0 }, Cmd.none
            else { model with GameState = gameState }, Cmd.none
    | RollBall direction, model ->
        let roundMoves = model.MovesMade + 1
        let boardAfterRoll = rollBall model.CurrentGrid direction
        { model with CurrentGrid = boardAfterRoll; MovesMade = roundMoves }, Cmd.ofMsg CheckSolution
    | LoadRound levelIndex, model ->
        let newRound = SharedGoalRoll.loadRound levelIndex
        let newRoundModel : SharedGoalRoll.Model = { 
            LevelIndex = levelIndex
            InitialGrid = newRound
            CurrentGrid = newRound
            BallPositionIndex = SharedGoalRoll.getBallPositionIndex newRound
            GoalPositionIndex = SharedGoalRoll.getGoalPositionIndex newRound
            GameState = Playing
            MovesMade = 0
        }
        newRoundModel, Cmd.none
    | ResetRound, model ->
        let resetRound = model.InitialGrid
        { model with 
            CurrentGrid = resetRound;
            BallPositionIndex = SharedGoalRoll.getBallPositionIndex resetRound
            GameState = Playing
            MovesMade = 0
        }, Cmd.none 
    | CheckSolution, model ->
        let resetRound = model.InitialGrid
        let resetBallPosition = SharedGoalRoll.getBallPositionIndex resetRound
        if SharedGoalRoll.getBallPositionIndex model.CurrentGrid = model.GoalPositionIndex
            then
                { model with CurrentGrid = resetRound; BallPositionIndex = resetBallPosition; GameState = Won }, Cmd.none
            else
                model, Cmd.none
    | QuitGame, model -> model, Cmd.ofMsg QuitGame

// -------- GOAL ROLL VIEW --------

let icon src = Html.img [ prop.src src; prop.className "w-6 h-6" ]

let goalRollRowCreator (rowPositions: LaneObject list) dispatch =
    Html.div [
        prop.className "flex justify-center items-center space-x-1 my-1"
        prop.children [
            for position in rowPositions do
                match position with
                | Blocker ->
                    Html.div [
                        prop.className "bg-gray-800 w-10 h-10 flex items-center justify-center rounded shadow"
                        prop.text SharedViewModule.GamePieceIcons.blocker
                    ]
                | Ball ->
                    Html.div [
                        prop.className "bg-blue-500 w-10 h-10 flex items-center justify-center rounded shadow"
                        prop.text SharedViewModule.GamePieceIcons.ball
                    ]
                | Goal ->
                    Html.div [
                        prop.className "bg-green-600 w-10 h-10 flex items-center justify-center rounded shadow"
                        prop.text SharedViewModule.GamePieceIcons.goalFlag
                    ]
                | Heart ->
                    Html.div [
                        prop.className "bg-pink-400 w-10 h-10 flex items-center justify-center rounded shadow"
                        prop.text SharedViewModule.GamePieceIcons.heart
                    ]
                | LaneLock ->
                    Html.div [
                        prop.className "bg-yellow-600 w-10 h-10 flex items-center justify-center rounded shadow"
                        prop.text SharedViewModule.GamePieceIcons.lock
                    ]
                | LaneKey ->
                    Html.div [
                        prop.className "bg-indigo-500 w-10 h-10 flex items-center justify-center rounded shadow"
                        prop.text SharedViewModule.GamePieceIcons.key
                    ]
                | Bomb ->
                    Html.div [
                        prop.className "bg-red-700 w-10 h-10 flex items-center justify-center rounded shadow"
                        prop.text SharedViewModule.GamePieceIcons.bomb
                    ]
                | MoveArrow direction ->
                    Html.button [
                        prop.className "bg-gray-300 hover:bg-gray-400 w-10 h-10 flex items-center justify-center rounded shadow cursor-pointer"
                        prop.onClick (fun _ -> RollBall direction |> dispatch)
                        prop.text (SharedViewModule.GamePieceIcons.directionArrowImage direction)
                    ]
                | _ ->
                    Html.div [
                        prop.className "bg-gray-900 w-10 h-10 flex items-center justify-center rounded shadow"
                        prop.text SharedViewModule.GamePieceIcons.empty
                    ]
        ]
    ]

let gameRulesAndSettingsView model dispatch =
    Html.div [
        SharedViewModule.modalInstructionContent goalRollDescriptions
        SharedViewModule.codeModalControlsContent gameControls dispatch
    ]

let gameContentViewControls model controlList dispatch =
    Html.div [
        prop.className "pt-4 flex flex-wrap justify-center gap-4"
        prop.children [
            for controlTitle: string, controlMsg in controlList do
                Html.button [
                    prop.className "modalControls text-white text-lg font-semibold hover:text-green-400"
                    prop.text controlTitle
                    prop.onClick (fun _ -> controlMsg |> dispatch)
                ]
        ]
    ]

let goalRollLevelCreator (goalRollModel: SharedGoalRoll.Model) dispatch =
    let positions = goalRollModel.CurrentGrid

    let gridWithUpArrow = gridWithMovementArrow positions Up
    let gridWithDownArrow = gridWithMovementArrow gridWithUpArrow Down
    let gridWithLeftArrow = gridWithMovementArrow gridWithDownArrow Left
    let gridWithRightArrow = gridWithMovementArrow gridWithLeftArrow Right

    let gameGrid =
        if SharedGoalRoll.getBallPositionIndex positions <> goalRollModel.GoalPositionIndex then
            if SharedGoalRoll.getGoalPositionIndex positions = -1 then
                gridWithGoal gridWithRightArrow goalRollModel.GoalPositionIndex
            else
                gridWithRightArrow
        else
            gridWithRightArrow

    let gridRows = getPositionsAsRows gameGrid 8

    Html.div [
        for row in gridRows do
            goalRollRowCreator row dispatch
    ]

let goalRollModalContent (model: SharedGoalRoll.Model) dispatch =
    SharedViewModule.gameModalContent (
        match model.GameState with
        | Settings -> gameRulesAndSettingsView model dispatch
        | Won -> SharedViewModule.roundCompleteContent (modelGoalRollRoundDetails model)
        | Paused
        | Playing -> goalRollLevelCreator model dispatch
    )

let view (model: SharedGoalRoll.Model) dispatch =
    Html.div [
        SharedViewModule.sharedModalHeader "Goal Roll" QuitGame dispatch
        goalRollModalContent model dispatch
        SharedViewModule.codeModalFooter controlList dispatch
    ]
