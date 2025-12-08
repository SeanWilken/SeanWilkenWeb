module Components.FSharp.Portfolio.Games.PivotPoints

open Client.Domain
open GridGame
open Elmish
open Browser
open Feliz
    
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

let rulesAndSettingsGameControls (model: SharedPivotPoint.Model) dispatch =
    let toggleStr = roundStateToggleString model
    Html.div [
        prop.className "space-y-4 text-center"
        prop.children [
            Html.button [
                prop.className "modalControls text-lg text-white hover:text-blue-400"
                prop.text $"{toggleStr} Round"
                prop.onClick (fun _ -> startGameLoop model dispatch |> ignore)
            ]
            Html.button [
                prop.className "modalControls text-lg text-white hover:text-red-400"
                prop.text "Restart Round"
                prop.onClick (fun _ -> SharedPivotPoint.ResetRound |> dispatch)
            ]
        ]
    ]

let modalGameSettingsView model dispatch =
    Html.div [
        prop.className "modalAltContent text-center"
        prop.children [ rulesAndSettingsGameControls model dispatch ]
    ]

let gameRulesAndSettingsView model dispatch =
    Html.div [
        SharedViewModule.modalInstructionContent pivotPointsDescriptions
        modalGameSettingsView model dispatch
    ]

let gameContentViewControls model controlList dispatch =
    Html.div [
        prop.className "pt-4 flex flex-wrap gap-4 justify-center items-center"
        prop.children (
            [ roundStateToggle model dispatch ] @
            [
                for (controlTitle: string, controlMsg) in controlList ->
                    Html.button [
                        prop.className "modalControls text-white text-lg font-semibold hover:text-green-400"
                        prop.text controlTitle
                        prop.onClick (fun _ -> dispatch controlMsg)
                    ]
            ]
        )
    ]

let pivotPointTileView rollDirection position style message dispatch =
    let iconPath =
        match position with
        | Ball -> SharedViewModule.GamePieceIcons.directionArrowImage rollDirection
        | Blocker -> SharedViewModule.GamePieceIcons.blocker
        | Goal -> SharedViewModule.GamePieceIcons.goalFlag
        | _ -> SharedViewModule.GamePieceIcons.empty

    Html.div [
        prop.className "w-10 h-10 flex items-center justify-center rounded shadow cursor-pointer bg-gray-800"
        prop.style style
        prop.onClick (fun _ -> message |> dispatch)
        prop.children [ Html.div [ prop.text iconPath; prop.className "w-6 h-6" ] ]
    ]

let parseLaneDetails (details: LaneDetails option) =
    match details with
    | Some d -> d.Style, d.Message
    | None -> [], SharedPivotPoint.Ignore

let pivotPointsLaneCreator isRow rollDirection laneDetail rowPositions dispatch =
    let style, message = parseLaneDetails laneDetail
    Html.div [
        prop.className (if isRow then "flex justify-center items-center space-x-1 my-1" else "")
        prop.children [
            for obj in rowPositions ->
                pivotPointTileView rollDirection obj style message dispatch
        ]
    ]

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

let pivotPointsGameLoopCard (model: SharedPivotPoint.Model) (dispatch: SharedPivotPoint.Msg -> unit) =
    let toggleString = roundStateToggleString model
    Html.div [
        prop.className "card bg-base-200 shadow-md p-4 mt-4"
        prop.children [
            Html.div [
                prop.className "flex flex-col space-y-6"
                prop.children [
                    Html.div [
                        prop.className "modalAltContent p-4"
                        prop.children [ 
                            Html.div [
                                prop.className "flex flex-col md:flex-row gap-4 justify-between items-stretch"
                                prop.children [
                                    Html.div [
                                        prop.className "flex flex-col items-center justify-center gap-2 flex-1"
                                        prop.children [
                                            Html.a [
                                                prop.className "btn btn-sm btn-primary w-full"
                                                prop.onClick (fun _ -> startGameLoop model dispatch |> ignore)
                                                prop.text (toggleString + " Round")
                                            ]
                                            Html.a [
                                                prop.className "btn btn-sm btn-secondary w-full"
                                                prop.onClick (fun _ -> SharedPivotPoint.Msg.ResetRound |> dispatch)
                                                prop.text "Restart Round"
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let pivotPointsModalContent (model: SharedPivotPoint.Model) dispatch =
    SharedViewModule.gameModalContent (
        Html.div [
            match model.GameState with
            | Won -> SharedViewModule.roundCompleteContent (modelPivotPointRoundDetails model) (fun () -> SharedPivotPoint.ResetRound |> dispatch)
            | _ ->
                Html.div [
                    prop.className "flex flex-row justify-between items-center w-full mb-4 gap-4 relative p-2"
                    prop.children [
                        Html.div [
                            prop.className "card bg-base-200 shadow-lg p-4 flex-1 border-2 border-info-content"
                            prop.children [
                                Html.div [
                                    prop.className "text-error font-bold text-lg text-center"
                                    prop.text $"Game Clock: {SharedViewModule.gameTickClock model.GameClock}"
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card bg-base-200 shadow-lg p-4 flex-1 border-2 border-warning-content"
                            prop.children [
                                Html.div [
                                    prop.className "text-info font-bold text-lg text-center"
                                    prop.text $"Coins: {model.CoinsCollected}"
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "my-4"
                    prop.children [ pivotPointsBoardView model dispatch ]
                ]
                pivotPointsGameLoopCard model dispatch
        ]
    )

let view (model: SharedPivotPoint.Model) dispatch =
    Html.div [
        SharedViewModule.sharedModalHeader "Pivot Points" pivotPointsDescriptions SharedPivotPoint.QuitGame dispatch
        pivotPointsModalContent model dispatch
        // gameContentViewControls model controlList dispatch
    ]
