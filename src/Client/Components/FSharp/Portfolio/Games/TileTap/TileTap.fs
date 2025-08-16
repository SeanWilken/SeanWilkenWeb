module Components.FSharp.Portfolio.Games.TileTap

open System
open Shared
open GridGame
open Elmish
open Fable.React
open Feliz
open Browser
open SharedTileTap
open Bindings.LucideIcon

(*

// Bomb diffuse by clicking a tile sequence around it?

// implement?
// store this on the model?
// let roundDifficultySpawnInterval difficulty =
//     match difficulty with
//     | SharedTileTap.TileTapDifficulty.Simple -> 6
//     | SharedTileTap.TileTapDifficulty.Easy -> 4
//     | SharedTileTap.TileTapDifficulty.Intermediate -> 2
//     | SharedTileTap.TileTapDifficulty.Hard -> 1

*)

// *********************************

// Module Content & Helpers -

let tileTapDescriptions = [
    // "Survival Mode:"
    "- Tap the tile before it's timer runs out."
    "- If the tile timer reaches 0 adds 1 mistake."
    "- Don't tap bombs, they add 3 mistakes."
    "- Tap a Heart to take away 1 mistake."
    "- Make it until the round timer ends."
    "- Hard mode: 1 mistake ends it all, unlimited time."
]


let modelTileTapRoundDetails ( model : SharedTileTap.Model ) = [
    ( modelValueAsString "Round Score: " model.CompletedRoundDetails.RoundScore )
    "Round Timer: " + ( SharedViewModule.gameTickClock model.CompletedRoundDetails.GameClock ) + modelValueAsString " / " model.RoundTimer
    modelValueAsString "Round Mistakes: " model.CompletedRoundDetails.RoundMistakes + modelValueAsString " / " model.AllowableRoundMistakes
]

// ********************************

// LifeCycle & LifeCycle Helper Functions

// checks the list of current active tiles
// against a list of all positions to ensure
// that the same index can't be called to insert a new tile.
let tileSpawnPosition activeTilePositionList =
    // all positions (take a new parameter for gridSizeCeiling?)
    let allGridPositions = [ 0..63 ] 
    // filter the activeTilePositions from the totalPositions
    List.filter ( fun x -> not (List.contains x activeTilePositionList ) ) ( allGridPositions )
    // if there are available positions, select one randomly
    |> fun availablePositions -> 
        if not ( availablePositions.IsEmpty ) 
            then availablePositions.[SharedTileSort.randomIndex availablePositions.Length] 
            else ( SharedTileSort.randomIndex ( allGridPositions.Length - 1 ) )

// Given a max ceiling int value,
let randomValue ceiling =
    Random().Next(ceiling)
// return a mapped out TileValue
let randomTapTile randVal =
    if randVal < 15 then TapTileValue.Bomb, 21
    elif randVal >= 15 && randVal < 50 then TapTileValue.Minor, 17
    elif randVal >= 50 && randVal < 75 then TapTileValue.Modest, 13
    elif randVal >= 75 && randVal < 95 then TapTileValue.Major, 9
    else TapTileValue.Heart, 9

// Given a Gridboard and desired position,
// return that Gridboard with a tile @ desired position
// What about Clock, Heart or Bomb???
let insertNewTapTile gridBoard position =
    let gridPositions = gridBoard.GridPositions
    let tapTileValue, tapTileLifeTime = randomTapTile ( randomValue ( 100 ) )
    { GridPositions = [
        for i in 0 .. ( gridPositions.Length - 1 ) do
            if i = position
                then TapTile { TapPosition = position; LifeTime = tapTileLifeTime; Value = tapTileValue } 
                else gridPositions.[i]
    ] }

// Given a Gridboard, return a list of it's tile positions
let activeTilePositionsFromBoard gridBoard =
    gridBoard.GridPositions
    |> List.map ( fun x -> 
        match x with 
        | TapTile x -> x.TapPosition
        | _ -> 0
    ) |> List.filter ( fun x -> x <> 0 )

// Increment the LifeTime value on every tile on the Gridboard
let tickActiveTiles gridBoard =
    { GridPositions =
        gridBoard.GridPositions
        |> List.map ( fun x -> 
            match x with 
            | TapTile x -> TapTile ( { x with LifeTime = x.LifeTime - 1 } )
            | _ -> Blank
        ) 
    }

// Returns the first ( Some TapTile ) 
// if found on the given Gridboard.
let expiredTileFromGrid grid =
    ( List.tryFind ( fun x -> 
        match x with 
            | TapTile x -> x.LifeTime <= 0
            | _ -> false 
    ) grid.GridPositions )
    |> fun expiredTileOption -> 
        match expiredTileOption with 
        | Some ( TapTile x ) -> Some x
        | _ -> None

// Given a Gridboard and a TapTile
// swap the gridPosition to a blank
// if it matches the given TapTile.
let gridWithoutTile grid tileToRemove = 
    { GridPositions = [ 
        for tile in grid.GridPositions do
            match tile with 
            | TapTile x -> if x = tileToRemove then Blank else tile
            | _ -> tile
    ] }

// Given a tile, based on it's Value
// returns a score value to increment the score
// change this for the lifeTime to be reversed from curent implementation..
let calculateTilePointValue ( tile : TapTile )=
    match tile.Value with 
    | Minor -> 1 * tile.LifeTime
    | Modest -> 2 * tile.LifeTime
    | Major -> 5 * tile.LifeTime
    | _ -> 0

let gameModeRoundMistake ( model : SharedTileTap.Model ) ( mistakeValue : int ) : ( SharedTileTap.Model * Cmd<Msg> )=
    if model.GameMode = SharedTileTap.TileTapGameMode.Survival
        then 
            let totalRoundMistakes = if model.CurrentRoundDetails.RoundMistakes + mistakeValue < 0 then 0 else model.CurrentRoundDetails.RoundMistakes + mistakeValue
            printfn "%i" totalRoundMistakes
            let com = 
                if ( model.AllowableRoundMistakes > 0 ) && ( totalRoundMistakes >= model.AllowableRoundMistakes ) 
                    then Cmd.ofMsg EndRound
                    else Cmd.none
            { model with CurrentRoundDetails = { model.CurrentRoundDetails with RoundMistakes = totalRoundMistakes } }, com
        else
            let currentTimeExpired = model.CurrentRoundDetails.GameClock + ( mistakeValue * 4 )
            let com = if ( model.RoundTimer * 4 <= currentTimeExpired ) then Cmd.ofMsg EndRound else Cmd.none
            { model with 
                CurrentRoundDetails = { model.CurrentRoundDetails with RoundMistakes = model.CurrentRoundDetails.RoundMistakes + 1; GameClock = currentTimeExpired } 
            }, com

// Initialize the modules model
// No command dispatched currently
let init(): SharedTileTap.Model * Cmd<Msg> =
    SharedTileTap.initModel, Cmd.none

// Handles the various different messages that
// can be dispatched throughout the use and
// interaction of this module
let update msg ( model : SharedTileTap.Model ) =
    match msg with
    // 'Tick' Interval in which the module operates.
    // this is dispatched through the browser windows setInterval function
    // Checks against Round 'Allowances' to see if the round should be stopped.
    // If round is still in play, then it will dispatch 
    // to check for expired tiles
    // spawn a new tile if the spawn interval is matched or exceeded
    | GameLoopTick -> // this should check against Round 'Allowances'
        // If more than mistakes are made than the difficulty on the model allows
        printfn "%i" model.CurrentRoundDetails.RoundMistakes
        if ( model.AllowableRoundMistakes > 0 ) && ( model.CurrentRoundDetails.RoundMistakes >= model.AllowableRoundMistakes ) then model, Cmd.ofMsg EndRound
        // End the round when the GameClock exceeds the RoundTimer.  ( -1 : no round timer)
        elif ( model.RoundTimer > 0 ) && ( ( model.CurrentRoundDetails.GameClock / 4 ) >= model.RoundTimer ) then model, Cmd.ofMsg EndRound
        // No conditions to end round met, batch check grid and spawn tile commands
        else { model with CurrentRoundDetails = { model.CurrentRoundDetails with GameClock = model.CurrentRoundDetails.GameClock + 1 } }, Cmd.batch [ Cmd.ofMsg ( CheckGridboardTiles ); Cmd.ofMsg ( SpawnNewActiveTile ) ]
    // Game starts
    | SetDispatchPointer flt ->
        // if I set the float as any float that isn't zero, you are playing
        if flt <> 0.0 
            then Playing 
            else Paused // if it is zero you are paused (or other state)
        |> fun gameRoundState -> { model with GameState = gameRoundState; DispatchPointer = flt }, Cmd.none
    | SetGameState gameState ->
        if model.DispatchPointer <> 0.0 then SharedViewModule.stopGameLoop model.DispatchPointer
        { model with GameState = gameState; DispatchPointer = 0.0 }, Cmd.none
    // Change Round Parameters based on requested difficulty
    | ChangeGameMode gameMode ->
        SharedTileTap.updateModelGameMode model gameMode, Cmd.ofMsg ResetRound
    | ChangeDifficulty difficulty ->
        if model.GameMode = SharedTileTap.TileTapGameMode.Survival
            then SharedTileTap.updateSurvivalModeDifficulty model difficulty, Cmd.ofMsg ResetRound
            else SharedTileTap.updateTimeAttackModeDifficulty model difficulty, Cmd.ofMsg ResetRound
    // If there hasn't been a new tile placed onto the Gridboard
    // but the interval is matched or exceeded, generate the Gridboard with a new tile added
    // otherwise increment the LastSpawnInterval by one
    | SpawnNewActiveTile ->
        // spawnInterval should be based on difficulty?
        // check if max amount of tiles is reached && no tile spawned in last 2 ticks (.5 second)
        if ( model.LastSpawnInterval >= 2 ) then //( model.TilesSpawned < 10 ) && 
            // filter a list of all positions by what is already active
            activeTilePositionsFromBoard model.TileTapGridBoard
            // select a position within the Gridboard to place a new tile
            |> fun activeTilePositionList -> tileSpawnPosition activeTilePositionList 
            // return the Gridboard with the selected position filled by a new tile
            |> fun selectedPosition -> insertNewTapTile model.TileTapGridBoard selectedPosition
            // model with new taptile in active tile list
            |> fun gridBoardWithTile -> { 
                model with 
                    TileTapGridBoard = gridBoardWithTile; 
                    LastSpawnInterval = 0; 
                    CurrentRoundDetails = { model.CurrentRoundDetails with TilesSpawned = model.CurrentRoundDetails.TilesSpawned + 1 } 
            }, Cmd.none
        else
            // increment the spawn interval, as one was not spawned this pass.
            { model with LastSpawnInterval = model.LastSpawnInterval + 1 }, Cmd.none
    // Will check the Gridboard for expired tiles after
    // incrementing all their respective lifetime values
    // Dispatch Mistake & ExpireTile if one exists
    | CheckGridboardTiles ->
        let activeTilePositionList = ( activeTilePositionsFromBoard model.TileTapGridBoard )
        if not(activeTilePositionList.IsEmpty) then
            let tickedGrid = tickActiveTiles model.TileTapGridBoard
            expiredTileFromGrid tickedGrid
            |> fun expiredTile ->
                match expiredTile with
                | Some x -> 
                    match x.Value with 
                    | TapTileValue.Heart                        
                    | TapTileValue.Bomb ->
                        Cmd.ofMsg ( ExpireTile x )
                    | _ ->
                        Cmd.batch [ Cmd.ofMsg ( Mistake (1) ); Cmd.ofMsg ( ExpireTile x ) ]
                | None -> Cmd.none
            |> fun comms -> { model with TileTapGridBoard = tickedGrid }, comms
        else model, Cmd.none
    // Tile times out - remove that tile from the board
    | ExpireTile expiredTile ->
        let grid = gridWithoutTile model.TileTapGridBoard expiredTile
        { model with TileTapGridBoard = grid }, Cmd.none
    // Player tapped the tile before LifeTime was exceeded,
    // removes the tappedTile from the Gridboard and increments the RoundScore
    // based on the tappedTile's Value & LifeTime
    | DestroyTile tappedTile ->
        // need to do some stuff based on game mode
        gridWithoutTile model.TileTapGridBoard tappedTile
        |> fun grid -> 
            { model with 
                TileTapGridBoard = grid
                CurrentRoundDetails = {
                    model.CurrentRoundDetails with 
                        TilesSmashed = model.CurrentRoundDetails.TilesSmashed + 1
                        RoundScore = model.CurrentRoundDetails.RoundScore + ( calculateTilePointValue tappedTile )
                }
        }, match tappedTile.Value with
            | TapTileValue.Heart -> Cmd.ofMsg ( Mistake (-1) )
            | TapTileValue.Bomb -> Cmd.ofMsg ( Mistake (3) )
            | _ -> Cmd.none
    // User didn't tap the tile in time or 
    // selected a harmful or blank tile
    | Mistake mistakeValue ->
        // needs to be based on selected mode: Survival || Time Attack
        // negative value mistakes are helpful actually as they reduce total mistakes made or increase the clock
        // some might say, a "happy little mistake" :^)
        if (model.DispatchPointer <> 0.0)
            then gameModeRoundMistake model mistakeValue
            else model, Cmd.none
    // Round has finished, cleanup the intervalTimer
    | EndRound -> 
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        SharedTileTap.endRound model, Cmd.none
    // Resets the values for a round aside 
    // from it's difficulty parameters
    | ResetRound ->
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        SharedTileTap.resetRound model, Cmd.none
    // Stops the setInterval dispatch loop when the game is exited
    | ExitGameLoop -> 
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        model, Cmd.ofMsg QuitGame
    // Msg that will be intercepted by CodeGallery
    // where User will be returned to
    | QuitGame -> 
        model, Cmd.none

// --------------------------------------

// CONTENT --------

open Shared
open SharedTileTap
open Feliz

let tileColorClass (tile: TapTile) =
    match tile.Value with
    | TapTileValue.Bomb -> "bg-red-600"      // Bomb is red
    | TapTileValue.Heart -> "bg-pink-300"    // Heart stays pink
    | Minor -> "bg-green-700"                // Minor: dark green
    | Modest -> "bg-green-500"               // Modest: medium green
    | Major -> "bg-green-300"                // Major: light green
    
let tileView colorClass tapTile (tileImage: string) dispatch = 
    Html.button [
        prop.className $"gameTile {colorClass} w-10 h-10 flex flex-col items-center justify-center rounded-lg shadow cursor-pointer relative border-2 border-white"
        prop.onClick (fun _ -> DestroyTile(tapTile) |> dispatch)
        prop.children [
            Html.span [
                match tapTile.Value with
                | TapTileValue.Bomb -> prop.text SharedViewModule.GamePieceIcons.bomb
                | TapTileValue.Heart -> prop.text SharedViewModule.GamePieceIcons.heart
                | Minor | Modest | Major -> prop.text "★" // Use a star icon for balls
                | _ -> prop.text SharedViewModule.GamePieceIcons.empty
            ]
            Html.span [
                prop.className "mt-1 text-xs font-bold text-white"
                prop.text (SharedViewModule.gameTickClock tapTile.LifeTime)
            ]
        ]
    ]

let tileTapRowCreator (rowPositions: LaneObject list) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "flex justify-center items-center space-x-2 my-2"
        prop.children [
            for position in rowPositions do
                match position with
                | TapTile tile ->
                    let colorClass = tileColorClass tile
                    tileView
                        colorClass
                        tile
                        "" // icon now handled in tileView
                        dispatch
                | _ ->
                    Html.div [
                        prop.className "blankTile bg-base-200 w-10 h-10 flex items-center justify-center rounded-lg shadow cursor-pointer"
                        prop.onClick (fun _ -> Mistake(1) |> dispatch)
                        prop.text SharedViewModule.GamePieceIcons.empty
                    ]
        ]
    ]

let tileTapBoardView (gridPositions: GridBoard) (dispatch: Msg -> unit) =
    let board = GridGame.getPositionsAsRows gridPositions 8
    Html.div [
        prop.className "flex flex-col items-center space-y-4"
        prop.children [ for row in board -> tileTapRowCreator row dispatch ]
    ]

// *********************************

// Controls are being made in more custom override fashion,
// as the needs for this submodule require a bit more of a workaround
// due to Async.RunAsyncronously: not available in Fable
// wanting to set the window interval to dispatch game tick functions
    // while still being able to set the GameLoopTick's pointer returned setInterval on the model
// Tried with subscriptions, but as implemented it would be firing off regardless of the current
    // model that was being used / viewed by the User.
        // launch a sub-program with the subscriptions being fired off when that module is launched?
// Currently hacked around the Elmish dispatch loop

// Time drives main game state, as things happen in intervals contained within the main loop
let startGameLoop ( model : SharedTileTap.Model ) dispatch =
    if model.DispatchPointer = 0.0 && model.GameState = Paused
        then
            window.setInterval((fun _ -> dispatch (GameLoopTick)), 250)
            |> fun loopFloat -> dispatch (SetDispatchPointer loopFloat)
        else
            window.clearInterval(model.DispatchPointer)
            dispatch ( SetDispatchPointer 0.0 )

let roundStateToggleString ( model : SharedTileTap.Model ) = 
    if model.GameState = Won || model.GameState = Settings
        then "Play"
        elif ( model.DispatchPointer <> 0.0 ) then "Pause" 
        else if model.CurrentRoundDetails.GameClock <> 0 then "Resume" else "Start"

let tileTapGameLoopCard (model: SharedTileTap.Model) (dispatch: Msg -> unit) =
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
                                                prop.onClick (fun _ -> ResetRound |> dispatch)
                                                prop.text "Restart Round"
                                            ]
                                        ]
                                    ]
                                    Html.div [
                                        prop.className "flex flex-col items-center justify-center gap-2 flex-1"
                                        prop.children [
                                            Html.h3 [ prop.className "mb-1 text-error text-center"; prop.text "Game Mode:" ]
                                            Html.div [
                                                prop.className "flex flex-row gap-2 justify-center"
                                                prop.children [
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.GameMode = SharedTileTap.TileTapGameMode.Survival then " text-primary" else ""))
                                                        prop.onClick (fun _ -> ChangeGameMode SharedTileTap.TileTapGameMode.Survival |> dispatch)
                                                        prop.text "Survival"
                                                    ]
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.GameMode = SharedTileTap.TileTapGameMode.TimeAttack then " text-primary" else ""))
                                                        prop.onClick (fun _ -> ChangeGameMode SharedTileTap.TileTapGameMode.TimeAttack |> dispatch)
                                                        prop.text "Time Attack"
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                    Html.div [
                                        prop.className "flex flex-col items-center justify-center gap-2 flex-1"
                                        prop.children [
                                            Html.h3 [ prop.className "mb-1 text-center"; prop.text "Round Difficulty:" ]
                                            Html.div [
                                                prop.className "flex flex-row gap-2 justify-center"
                                                prop.children [
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.Difficulty = SharedTileTap.TileTapDifficulty.Simple then " text-primary" else ""))
                                                        prop.onClick (fun _ -> ChangeDifficulty SharedTileTap.TileTapDifficulty.Simple |> dispatch)
                                                        prop.text "Simple"
                                                    ]
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.Difficulty = SharedTileTap.TileTapDifficulty.Easy then " text-primary" else ""))
                                                        prop.onClick (fun _ -> ChangeDifficulty SharedTileTap.TileTapDifficulty.Easy |> dispatch)
                                                        prop.text "Easy"
                                                    ]
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.Difficulty = SharedTileTap.TileTapDifficulty.Intermediate then " text-primary" else ""))
                                                        prop.onClick (fun _ -> ChangeDifficulty SharedTileTap.TileTapDifficulty.Intermediate |> dispatch)
                                                        prop.text "Medium"
                                                    ]
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.Difficulty = SharedTileTap.TileTapDifficulty.Hard then " text-primary" else ""))
                                                        prop.onClick (fun _ -> ChangeDifficulty SharedTileTap.TileTapDifficulty.Hard |> dispatch)
                                                        prop.text "Hard"
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
        ]
    ]
    
// -------------------------------------

let tileTapModalContent (model: SharedTileTap.Model) dispatch =
    SharedViewModule.gameModalContent (
        Html.div [
            match model.GameState with
            | Won -> SharedViewModule.roundCompleteContent (modelTileTapRoundDetails model) (fun () -> SharedTileTap.Msg.ResetRound |> dispatch)
            | _ ->
                Html.div [
                    prop.className "flex flex-row justify-between items-center w-full mb-4 gap-4 relative p-2"
                    prop.children [
                        Html.div [
                            prop.className "card bg-base-200 shadow-lg p-4 flex-1 border-2 border-success-content"
                            prop.children [
                                Html.div [
                                    prop.className "text-success font-bold text-lg text-center"
                                    prop.text $"HP: {model.AllowableRoundMistakes - model.CurrentRoundDetails.RoundMistakes}"
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card bg-base-200 shadow-lg p-4 flex-1 border-2 border-info-content"
                            prop.children [
                                Html.div [
                                    prop.className "text-error font-bold text-lg text-center"
                                    prop.text $"Game Clock: {SharedViewModule.gameTickClock model.CurrentRoundDetails.GameClock}"
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card bg-base-200 shadow-lg p-4 flex-1 border-2 border-warning-content"
                            prop.children [
                                Html.div [
                                    prop.className "text-info font-bold text-lg text-center"
                                    prop.text $"Round Score: {model.CurrentRoundDetails.RoundScore}"
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "my-4"
                    prop.children [ tileTapBoardView model.TileTapGridBoard dispatch ]
                ]
                tileTapGameLoopCard model dispatch
        ]
    )

// -------------------------------------
// MODULE VIEW ----
let view model dispatch =
    Html.div [
        SharedViewModule.sharedModalHeader "Tile Tap" tileTapDescriptions QuitGame dispatch
        tileTapModalContent model dispatch
    ]
