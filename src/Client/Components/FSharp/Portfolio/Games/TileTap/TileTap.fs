module Components.FSharp.Portfolio.Games.TileTap

open System
open Elmish
open Feliz
open Browser
open Bindings.LucideIcon
open Client.GameDomain.GridGame
open SharedViewModule.SharedMicroGames

(*

// Bomb diffuse by clicking a tile sequence around it?

// implement?
// store this on the model?
// let roundDifficultySpawnInterval difficulty =
//     match difficulty with
//     | TileTapDifficulty.Simple -> 6
//     | TileTapDifficulty.Easy -> 4
//     | TileTapDifficulty.Intermediate -> 2
//     | TileTapDifficulty.Hard -> 1

*)

// *********************************


type TileTapDifficulty =
    | Simple
    | Easy
    | Medium
    | Hard

// KINDA USELESS IMPLEMENTATION CURRENTLY,
// NEEDS MORE TWEAKS
type TileTapGameMode =
    | Survival
    | TimeAttack
(*
    Simple:
        - Mistakes are ignored.
        - 30 Second Round Timer
    Easy:
        - 5 Mistake Limit
        - 30 Second Round Timer
    Medium:
        - 3 Mistake Limit
        - 30 Second Round Timer
    Hard (Survival):
        - Go until you fail
        - No Mistakes, insta-fail
        - No Round Timer
*)

// function to determine the model based on difficulty
    // Simple -> model with AllowableMistakes = -1, timer = 30
    // Easy -> model with mistakes = 5, timer = 30
    // Medium -> model with mistakes = 3, timer = 30
    // Hard -> model with mistakes = 1, timer = -1
    // Time Attack (?) -> Hearts (-1 mistake), Clocks (slow down or speed up), Bomb (insta-fail)

// RoundConditions ->
    // AllowableRoundMistakes ->
        // ARM < 0 = Ignore Mistakes
        // ARM > 0 = Mistakes allowed up to ARM
            // RoundMistakes >= ARM then RoundEnd
    // RoundTimer ->
        // RT < 0 = Ignore Timer, play until ARM reached
        // RT > 0 = Timer value in seconds, checks against ( GameClock ticks / 4 )
            // RoundTimer < ( GameClock / 4 ) then RoundEnd

type TileTapRoundDetails = {
    RoundMistakes: int // How many mistakes were made that Round
    TilesSpawned: int // Current # of tiles spawned on the board
    TilesSmashed: int // # of tiles destroyed by the player
    RoundScore: int // Score of tiles destroyed within Round
    GameClock: int
}

let emptyTileTapRoundDetails = {
    RoundMistakes = 0
    TilesSpawned = 0
    TilesSmashed = 0
    RoundScore = 0
    GameClock = 0
}


type Msg =
    // GAME LOOP
    | SetGameState of RoundState
    | ChangeGameMode of TileTapGameMode
    | ChangeDifficulty of TileTapDifficulty
    | SetDispatchPointer of float // stores float pointer to dispatch setInterval loop
    | GameLoopTick // batches commands to check the lifetime and spawn new tiles
    // GRID TILES
    | CheckGridboardTiles // command to go through and check for expired tiles
    | SpawnNewActiveTile // places a new tile on the board
    | DestroyTile of TapTile // removes the tile (in a way that the player intended / made an effort)
    | ExpireTile of TapTile // remove the tile (in a way that the player missed the time interval)
    | Mistake of int
    // ROUND STATE
    | EndRound // FURTHER DEFINE THE REASON FOR ROUND ENDING
    | ResetRound
    | ExitGameLoop // stops the setInterval loop from running while not within this sub-module.
    | QuitGame // returns control to the parent, exiting to gallery


type Model = {
    TileTapGridBoard: GridBoard // grid board that will contain the various tiles
    LastSpawnInterval: int // Cooldown of new Tile being placed into the GameGrid
    GameMode: TileTapGameMode
    GameState: RoundState
    DispatchPointer: float // the float pointer to the GameLoop's dispatch
    RoundTimer: int // Max allowable seconds for this Round on GameClock
    AllowableRoundMistakes: int // max # of mistakes allowed before the round is considered 'lost' and will end
    // RoundTileLifeTime: int // How many GameTicks the tile will live for // tie into Value?
    CurrentRoundDetails: TileTapRoundDetails
    CompletedRoundDetails: TileTapRoundDetails
    Difficulty: TileTapDifficulty // current difficulty of the game
}

let levelCeiling = 1
let gridDimension = 8
let generateEmptyTileTapGrid gridDimension =
    { GridPositions = [
            for i in 0 .. ((gridDimension * gridDimension) - 1) do
                Blank
        ]
    }

let initModel = {
    TileTapGridBoard = generateEmptyTileTapGrid gridDimension
    LastSpawnInterval = 2
    GameMode = Survival
    GameState = Paused
    DispatchPointer = 0.0
    RoundTimer = 30
    AllowableRoundMistakes = 5
    Difficulty = Simple
    // RoundTileLifeTime = 15 (just under 4 seconds)
    CurrentRoundDetails = emptyTileTapRoundDetails
    CompletedRoundDetails = emptyTileTapRoundDetails
}

let init(): Model * Cmd<Msg> =
    initModel, Cmd.none

// doesn't fucking work, pissing me the fuck off
let endRound model =
    { model with
        TileTapGridBoard = generateEmptyTileTapGrid gridDimension
        GameState = Won
        DispatchPointer = 0.0
        CurrentRoundDetails = emptyTileTapRoundDetails
        CompletedRoundDetails = model.CurrentRoundDetails
    }

let resetRound model =
    { model with
        TileTapGridBoard = generateEmptyTileTapGrid gridDimension
        LastSpawnInterval = 2
        GameState = Paused
        DispatchPointer = 0.0
        CurrentRoundDetails = emptyTileTapRoundDetails
    }

// When ChangeDifficulty Msg is dispatched,
// returns model with different round parameters
// based on requested TileTapDifficulty
let updateSurvivalModeDifficulty  ( model : Model ) ( difficulty : TileTapDifficulty ) =
    match difficulty with
    | Simple -> { model with Difficulty = difficulty; RoundTimer = 30; AllowableRoundMistakes = 10 }
    | Easy -> { model with Difficulty = difficulty; RoundTimer = 60; AllowableRoundMistakes = 5 }
    | Medium -> { model with Difficulty = difficulty; RoundTimer = -1; AllowableRoundMistakes = 3 }
    | Hard -> { model with Difficulty = difficulty; RoundTimer = -1; AllowableRoundMistakes = 1 }

let updateTimeAttackModeDifficulty  ( model : Model ) ( difficulty : TileTapDifficulty ) =
    match difficulty with
    | Simple -> { model with Difficulty = difficulty; RoundTimer = 90; AllowableRoundMistakes = -1 }
    | Easy -> { model with RoundTimer = 60; AllowableRoundMistakes = -1 }
    | Medium -> { model with RoundTimer = 45; AllowableRoundMistakes = -1 }
    | Hard -> { model with RoundTimer = 30; AllowableRoundMistakes = -1 }

// When ChangeDifficulty Msg is dispatched,
// returns model with different round parameters
// based on requested TileTapDifficulty
let updateModelGameMode  ( model : Model ) ( mode : TileTapGameMode ) =
    match mode with
    | Survival -> { model with GameMode = Survival; RoundTimer = 30; AllowableRoundMistakes = 10 } // Timer will last as long as you will
    | TimeAttack -> { model with GameMode = TimeAttack; RoundTimer = 30; AllowableRoundMistakes = -1 }


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
            then availablePositions.[randomIndex availablePositions.Length] 
            else ( randomIndex ( allGridPositions.Length - 1 ) )

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

let gameModeRoundMistake ( model : Model ) ( mistakeValue : int ) : ( Model * Cmd<Msg> )=
    if model.GameMode = TileTapGameMode.Survival
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


// Handles the various different messages that
// can be dispatched throughout the use and
// interaction of this module
let update msg ( model : Model ) =
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
        updateModelGameMode model gameMode, Cmd.ofMsg ResetRound
    | ChangeDifficulty difficulty ->
        if model.GameMode = TileTapGameMode.Survival
            then updateSurvivalModeDifficulty model difficulty, Cmd.ofMsg ResetRound
            else updateTimeAttackModeDifficulty model difficulty, Cmd.ofMsg ResetRound
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
        endRound model, Cmd.none
    // Resets the values for a round aside 
    // from it's difficulty parameters
    | ResetRound ->
        if (model.DispatchPointer <> 0.0) then SharedViewModule.stopGameLoop(model.DispatchPointer)
        resetRound model, Cmd.none
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



let private tapTileBase (sizePx: int) : IStyleAttribute list =
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

let private timerBadge (txt: string) =
    Html.div [
        prop.className "absolute top-1 right-1 px-1.5 py-0.5 text-[10px] font-bold leading-none"
        prop.style [
            style.custom("background", "rgba(10,20,40,0.75)")
            style.custom("border", "1px solid rgba(0,255,255,0.25)")
            style.custom("box-shadow", "0 0 10px rgba(0,255,255,0.15)")
            style.custom("color", "#cfffff")
            style.custom("text-shadow", "0 0 10px rgba(0,255,255,0.35)")
        ]
        prop.text txt
    ]

type TapVisual =
    { Bg: string
      Border: string
      Shadow: string
      Icon: string
      IconColor: string
      IconShadow: string
      OverlayHatch: bool }

let private tapVisual (tile: TapTile) : TapVisual =
    match tile.Value with
    | TapTileValue.Bomb ->
        { Bg = "linear-gradient(135deg, rgba(255,0,100,0.45), rgba(139,0,0,0.45))"
          Border = "1px solid rgba(255,0,100,0.70)"
          Shadow = "inset 0 0 20px rgba(255,0,100,0.25), 0 0 18px rgba(255,0,100,0.18)"
          Icon = SharedViewModule.GamePieceIcons.bomb
          IconColor = "#ff2b7a"
          IconShadow = "0 0 12px rgba(255,0,100,0.55)"
          OverlayHatch = true }

    | TapTileValue.Heart ->
        { Bg = "linear-gradient(135deg, rgba(255,105,180,0.32), rgba(180,60,120,0.28))"
          Border = "1px solid rgba(255,105,180,0.55)"
          Shadow = "inset 0 0 18px rgba(255,105,180,0.18), 0 0 18px rgba(255,105,180,0.12)"
          Icon = SharedViewModule.GamePieceIcons.heart
          IconColor = "#ffd1e8"
          IconShadow = "0 0 12px rgba(255,105,180,0.45)"
          OverlayHatch = false }

    | Minor ->
        { Bg = "linear-gradient(135deg, rgba(0,255,255,0.18), rgba(0,139,139,0.18))"
          Border = "1px solid rgba(0,255,255,0.45)"
          Shadow = "inset 0 0 18px rgba(0,255,255,0.10), 0 0 14px rgba(0,255,255,0.12)"
          Icon = "★"
          IconColor = "#bfffff"
          IconShadow = "0 0 10px rgba(0,255,255,0.40)"
          OverlayHatch = false }

    | Modest ->
        { Bg = "linear-gradient(135deg, rgba(0,255,255,0.24), rgba(0,139,139,0.22))"
          Border = "1px solid rgba(0,255,255,0.55)"
          Shadow = "inset 0 0 20px rgba(0,255,255,0.12), 0 0 16px rgba(0,255,255,0.16)"
          Icon = "★"
          IconColor = "#cfffff"
          IconShadow = "0 0 12px rgba(0,255,255,0.55)"
          OverlayHatch = false }

    | Major ->
        { Bg = "linear-gradient(135deg, rgba(0,255,255,0.32), rgba(0,139,139,0.28))"
          Border = "1px solid rgba(0,255,255,0.70)"
          Shadow = "inset 0 0 22px rgba(0,255,255,0.14), 0 0 20px rgba(0,255,255,0.22)"
          Icon = "★"
          IconColor = "#e8ffff"
          IconShadow = "0 0 14px rgba(0,255,255,0.70)"
          OverlayHatch = false }

let tileView (tapTile: TapTile) (dispatch: Msg -> unit) =
    let sizePx = 64
    let v = tapVisual tapTile
    let timeTxt = SharedViewModule.gameTickClock tapTile.LifeTime

    Html.button [
        prop.type'.button
        prop.className "tile select-none"
        prop.style (
            tapTileBase sizePx @ [
                style.custom("background", v.Bg)
                style.custom("border", v.Border)
                style.custom("box-shadow", v.Shadow)
                style.custom("cursor", "pointer")
                style.custom("transition", "transform 0.15s ease, filter 0.15s ease")
            ]
        )
        prop.onClick (fun _ -> DestroyTile tapTile |> dispatch)
        prop.children [
            // optional hatch overlay (bomb)
            if v.OverlayHatch then
                Html.div [
                    prop.className "absolute inset-0"
                    prop.style [
                        style.custom(
                            "background",
                            "repeating-linear-gradient(45deg, transparent, transparent 8px, rgba(255,0,100,0.18) 8px, rgba(255,0,100,0.18) 16px)"
                        )
                    ]
                ]

            // icon
            Html.div [
                prop.className "text-[26px] font-black"
                prop.style [
                    style.custom("color", v.IconColor)
                    style.custom("text-shadow", v.IconShadow)
                ]
                prop.text v.Icon
            ]

            // timer badge
            timerBadge timeTxt
        ]
    ]

let tileTapRowCreator (rowPositions: LaneObject list) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "flex align-center items-center gap-1 my-1"
        prop.children [
            for position in rowPositions do
                match position with
                | TapTile tile ->
                    tileView tile dispatch

                | _ ->
                    // blank (click = mistake)
                    Html.button [
                        prop.type'.button
                        prop.className "tile select-none"
                        prop.style (
                            tapTileBase 64 @ [
                                style.custom("background", "rgba(10,20,40,0.55)")
                                style.custom("border", "1px solid rgba(255,255,255,0.10)")
                                style.custom("box-shadow", "inset 0 0 10px rgba(0,0,0,0.55)")
                                style.custom("cursor", "pointer")
                            ]
                        )
                        prop.onClick (fun _ -> Mistake 1 |> dispatch)
                        prop.children [
                            Html.div [
                                prop.className "text-[18px] opacity-40"
                                prop.style [ style.custom("color", "#9aa4b2") ]
                                prop.text SharedViewModule.GamePieceIcons.empty
                            ]
                        ]
                    ]
        ]
    ]

let tileTapBoardView (gridPositions: GridBoard) (dispatch: Msg -> unit) =
    let board = getPositionsAsRows gridPositions 8
    Html.div [
        prop.className "flex flex-col"
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
let startGameLoop ( model : Model ) dispatch =
    if model.DispatchPointer = 0.0 && model.GameState = Paused
        then
            window.setInterval((fun _ -> dispatch (GameLoopTick)), 250)
            |> fun loopFloat -> dispatch (SetDispatchPointer loopFloat)
        else
            window.clearInterval(model.DispatchPointer)
            dispatch ( SetDispatchPointer 0.0 )

let roundStateToggleString ( model : Model ) = 
    if model.GameState = Won || model.GameState = Settings
        then "PLAY"
        elif ( model.DispatchPointer <> 0.0 ) then "PAUSE" 
        else if model.CurrentRoundDetails.GameClock <> 0 then "RESUME" else "START"

let difficulties = [ Simple; Easy; Medium; Hard ]

[<ReactComponent>]
let view (model: Model) (dispatch: Msg -> unit) (quitMsg: 'parentMsg) (dispatchParent: 'parentMsg -> unit) =

    let overlay : ReactElement option =
        match model.GameState with
        | Won ->
            Some (
                WinOverlay model.CompletedRoundDetails.RoundScore
                    (fun () -> dispatch ResetRound)
                    (Some (fun () -> dispatch ResetRound))
            )
        | _ -> None

    let toggleString = roundStateToggleString model

    CyberShellResponsive {
        Left = 
            Html.div 
                [
                    TitlePanel "TILE TAP"
                    LevelSelectPanel
                        difficulties
                        model.Difficulty
                        (fun a b -> a = b)
                        (fun diff -> dispatch (ChangeDifficulty diff))
                    CyberPanel {
                        ClassName = "p-5"
                        ClipPath = Some "polygon(0 0, 100% 0, 100% calc(100% - 15px), calc(100% - 15px) 100%, 0 100%)"
                        Children = [
                            Html.div [
                                prop.style [ style.display.flex; style.justifyContent.spaceAround; style.alignItems.center ]
                                prop.children [
                                    StatBlock "" $"HP: {model.AllowableRoundMistakes - model.CurrentRoundDetails.RoundMistakes}" "#00ffff"
                                    StatBlock "" $"Game Clock: {SharedViewModule.gameTickClock model.CurrentRoundDetails.GameClock}" "#ff00ff"
                                    StatBlock " " $"Score: {model.CurrentRoundDetails.RoundScore}" "#ff00ff"
                                ]
                            ]
                        ]
                    }
                    InstructionsPanel 
                        "HOW TO PLAY" 
                        tileTapDescriptions
                        ""
                        (fun () -> ())
                    ControlsPanel [
                        ControlButton toggleString Purple (model.RoundTimer > 0) (fun () -> startGameLoop model dispatch |> ignore ) (Some (LucideIcon.RotateCcw "w-4 h-4"))
                        ControlButton "RESTART" Red true (fun () -> dispatch ResetRound) None
                    ]
                ]
        Board = BoardPanel ( tileTapBoardView model.TileTapGridBoard dispatch )
        Overlay = overlay
        OnQuit = (fun () -> dispatchParent quitMsg)
    }
