namespace Client.Domain

open System
open Feliz.UseDeferred

// GRID GAMES THAT USE LIST OF LANE OBJECTS TO DEFINE THEIR GRID BOARD AND CONTENTS
module GridGame =
    
    // Grid Movement Directions
    type MovementDirection =
        | Up
        | Down
        | Left
        | Right

    type TapTileValue =
        | Minor
        | Modest
        | Major
        | Heart
        | Bomb

    type TapTile = {
        TapPosition: int
        LifeTime: int
        Value: TapTileValue
    }

    type TileSortValueTile = { Value : int option }

    // Object that can be in any given space
    // THIS SHOULD BE MADE GENERIC WHERE THE SUB IMPLEMENTS
    type LaneObject =
        | Ball
        | Blank
        | Blocker
        | Bomb
        | Goal
        | Heart
        | LaneLock
        | LaneKey
        | MoveArrow of MovementDirection
        | TileSortLaneObject of TileSortValueTile
        | TapTile of TapTile

    // List of LaneObjects that occupy a Grid Position
    type GridBoard = {
        GridPositions: LaneObject list
    }

    // Represents a given game state
    type RoundState =
        | Settings
        | Paused // change to idle (no game active)
        | Playing // there is an active round
        | Won // Round ended, model contains round details

    // unwrap optional int from tryFindIndex
    // this is pretty unnecessary and should be 'baked' into the model domain
    let unwrapIndex index =
        match index with
        | Some i -> i
        | _ -> -1

    // returns true or false based on if the lookupObject is at the gridPositionIndex
    let checkGridPositionForObject positions (gridPositionIndex: int) objToFind =
        positions.GridPositions.Item(gridPositionIndex) = objToFind

    // returns the object at a given positions index
    let getObjectAtPositionIndex (positions: LaneObject list) (positionIndex: int) = 
        positions.Item(positionIndex)

    // would return the first index if one is found
    let getObjectPositionIndex (positions: GridBoard) (positionObject: LaneObject) =
        List.tryFindIndex (fun x -> x = positionObject ) positions.GridPositions

    // updates the grid position with an object
    let updatePositionWithObject positions (object: LaneObject) position = 
        let gridLength = positions.GridPositions.Length - 1
        let newGrid =
            // [ for i in 0 .. gridLength do
            [ for i in 0 .. gridLength do
                if i = position
                    then object 
                    else positions.GridPositions.Item(i)
            ]
        {GridPositions = newGrid}

    // GRID POSITION REPRESENTATION
    // GridPositions Represented in column groups
    let getPositionsAsColumns (positions: GridBoard) gridDimension =
        Seq.toList (seq { 
            for i in 0 .. (gridDimension - 1) do
                yield Seq.toList (seq { 
                    for n in 0 .. (gridDimension - 1) do
                        yield! seq{ 
                            positions.GridPositions.Item(i + (n * gridDimension)) 
                        }
                })
        })

    // GridPositions Represented in row groups
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

    let modelValueAsString strin value =
        if value = -1 
            then strin + "\u221E";
            else strin + string value

module RollableGridGameHelpers =

    open GridGame

    let getPiecePositionIndex ( gameGridPositions: GridBoard ) positionObject =
        getObjectPositionIndex gameGridPositions positionObject
        |> unwrapIndex

    let getRolledBallPositionIndex wrapped ballPosition direction =
        match wrapped, direction with
        | false, MovementDirection.Up -> (ballPosition - 8)
        | true, MovementDirection.Up ->  ballPosition + 56
        | false, MovementDirection.Down -> (ballPosition + 8)
        | true, MovementDirection.Down ->  ballPosition - 56
        | false, MovementDirection.Left -> (ballPosition - 1)
        | true, MovementDirection.Left ->  ballPosition + 7
        | false, MovementDirection.Right -> (ballPosition + 1)
        | true, MovementDirection.Right ->  ballPosition - 7

    let checkDirectionalBound ballRollPositionIndex direction =
        match direction with
        | MovementDirection.Up -> ballRollPositionIndex >= 0
        | MovementDirection.Down -> ballRollPositionIndex <= 63
        | MovementDirection.Right -> ((ballRollPositionIndex) % 8) <> 0
        | MovementDirection.Left -> ((ballRollPositionIndex + 1) % 8) >= 1

    let checkGridBoardPositionForBlocker gridBoard positionIndex =
        gridBoard.GridPositions.Item ( positionIndex ) = Blocker

    let updateGameBoardMovedPosition gameBoard piecePosition movedPiecePosition =
        let ballToBlankGrid = updatePositionWithObject (gameBoard) Blank (piecePosition)
        updatePositionWithObject ballToBlankGrid Ball movedPiecePosition

    let nonWrappingPieceMovementPositionIndex ballPosition direction = 
        getRolledBallPositionIndex false ballPosition direction

    let wrappablePieceMovementPositionIndex ballPosition direction = 
        checkDirectionalBound ( getRolledBallPositionIndex false ballPosition direction ) direction
        |> fun x -> getRolledBallPositionIndex ( not x ) ballPosition direction 
    
    let checkDirectionMovementWithFunction movementPositionFunc ballPosition direction gameBoard =
        let updatedPiecePosition = movementPositionFunc ballPosition direction
        if checkGridBoardPositionForBlocker gameBoard updatedPiecePosition
            then gameBoard
            else updateGameBoardMovedPosition gameBoard ballPosition updatedPiecePosition

    let checkRollableDirectionMovement ballPosition direction gameBoard =
        checkDirectionMovementWithFunction nonWrappingPieceMovementPositionIndex ballPosition direction gameBoard
    
    let checkDirectionMovement ballPosition direction gameBoard =
        checkDirectionMovementWithFunction wrappablePieceMovementPositionIndex ballPosition direction gameBoard


module SharedGoalRoll =

    open GridGame

    type Msg =
        | SetGameState of RoundState
        | ResetRound
        | LoadRound of int
        | RollBall of GridGame.MovementDirection
        | CheckSolution
        | QuitGame

    type Model =
        {
            LevelIndex: int
            BallPositionIndex: int
            GoalPositionIndex: int
            InitialGrid: GridGame.GridBoard
            CurrentGrid: GridGame.GridBoard
            GameState: GridGame.RoundState
            MovesMade: int
        }

    // --------------------------------------
    // SHARABLE (IF REFACTOR AND LEVELS MADE MORE GENERIC FOR LOAD)
    let getBallPositionIndex (gameGridPositions: GridBoard) =
        getObjectPositionIndex gameGridPositions Ball
        |> unwrapIndex

    let getGoalPositionIndex (gameGridPositions: GridBoard) =
        getObjectPositionIndex gameGridPositions Goal
        |> unwrapIndex

    // ------------------------
    // LEVEL CREATION DOMAIN, PLEASE RELOCATE ME, UNSURE OF POSITION
    // SEED GENERATION???
    let Level0 = { 
        GridPositions = [
            Blank; Blocker; Blank; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Heart; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; LaneLock; Blank;
            Bomb; Blank; Blank; Ball; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blocker; Blank;
            Blank; Blank; Blank; Goal; Blank; Blank; Blank; Blank;
            Blank; Blank; LaneKey; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blank; Heart;
        ] }

    let Level1 = { 
        GridPositions = [
            Ball; Blank; Blank; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank;Blank; Blank;Blank; Blank;Blank; Blocker;
            Blank; Blank; Blocker; Goal; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blank; Blank; 
        ] }

    let Level2 = { 
        GridPositions = [
            Blank; Blank; Blank; Blank; Blank; Blank; Blocker; Blank;
            Blocker; Goal; Blank; Blank; Blank; Blank;Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blank; Blocker;
            Ball; Blank; Blank; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blank; Blank; Blank;
            Blocker; Blank; Blank; Blank; Blank; Blocker; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blocker; Blocker; Blank;
        ] }

    let Level3 = { 
        GridPositions = [
            Blocker; Blocker; Blocker; Blank; Blank; Blocker; Blank; Blank;
            Blocker; Blocker; Blank; Blank; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blocker; Blank; Blank;
            Blank; Blocker; Blank; Blank; Blocker; Blank; Blank; Blank;
            Blank; Blank; Blocker; Blocker; Blocker; Blank; Blank; Blank;
            Blank; Blank; Blocker; Blocker; Blank; Blank; Blank; Blank;
            Blank; Blank; Blank; Blank; Blank; Blocker; Goal; Blank;
            Blocker; Blank; Blank; Blank; Blank; Ball; Blocker; Blocker; 
        ] }

    // --------------------------------------------------------------

    // LEVEL AND MODEL
    let loadRound roundIndex =
        match roundIndex with
        | 1 -> Level1
        | 2 -> Level2
        | 3 -> Level3
        | _ -> Level0

    // initial model, no message or command
    let initModel =
        let round = loadRound 3;
        let initialModel = {
            LevelIndex = 3;
            InitialGrid = round;
            CurrentGrid = round;
            BallPositionIndex = getBallPositionIndex round;
            GoalPositionIndex = getGoalPositionIndex round;
            GameState = Playing;
            MovesMade = 0;
        }
        initialModel

module SharedPivotPoint = 
    open GridGame

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

module SharedTileTap =

    open GridGame

    type TileTapDifficulty =
        | Simple
        | Easy
        | Intermediate
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
        GameState: GridGame.RoundState
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
        | Intermediate -> { model with Difficulty = difficulty; RoundTimer = -1; AllowableRoundMistakes = 3 }
        | Hard -> { model with Difficulty = difficulty; RoundTimer = -1; AllowableRoundMistakes = 1 }

    let updateTimeAttackModeDifficulty  ( model : Model ) ( difficulty : TileTapDifficulty ) =
        match difficulty with
        | Simple -> { model with Difficulty = difficulty; RoundTimer = 90; AllowableRoundMistakes = -1 }
        | Easy -> { model with RoundTimer = 60; AllowableRoundMistakes = -1 }
        | Intermediate -> { model with RoundTimer = 45; AllowableRoundMistakes = -1 }
        | Hard -> { model with RoundTimer = 30; AllowableRoundMistakes = -1 }

    // When ChangeDifficulty Msg is dispatched,
    // returns model with different round parameters
    // based on requested TileTapDifficulty
    let updateModelGameMode  ( model : Model ) ( mode : TileTapGameMode ) =
        match mode with
        | Survival -> { model with GameMode = Survival; RoundTimer = 30; AllowableRoundMistakes = 10 } // Timer will last as long as you will
        | TimeAttack -> { model with GameMode = TimeAttack; RoundTimer = 30; AllowableRoundMistakes = -1 }

module SharedTileSort =

    open GridGame

    // Determines the grid dimensions
    type TileSortDifficulty =
        | Simple // 3x3
        | Easy // 4x4
        | Medium // 5x5
        | Hard // 6x6
    
    type Msg =
        | SetGameState of GridGame.RoundState
        | NewRound // Starts a New Round, initializing the board again with the selected difficulty
        | ResetRound // Resets the round to the same initial configuration
        | RewindMove // Undo the last tile move made by the user
        | MoveTile of TileSortValueTile
        | UpdateDifficulty of TileSortDifficulty // Change the current difficulty for the given one
        | CheckSolution // Run the current board through the win validation check logic
        | Solved // Board is in the winning configuration
        | QuitGame // Quits back one level, exiting the game.

    type Model = {
        Difficulty: TileSortDifficulty // default at initial
        InitialTiles: GridBoard // set at start of round
        CurrentTiles: GridBoard // changing list of game tiles to represent moves
        Turns: int list // list of moves made for rewind / total number of moves to solve.
        GameState: GridGame.RoundState // determines whether the puzzle has been solved or in progress
    }

    // REFACTOR TO GAMES HELPER? A LOT OF THIS IS REUSABLE / REQUIRED BY GOALROLL
    // HELPER FUNCTIONS FOR RANDOMIZATION
    // Random Instance
    let rand = Random()

    // return random number within ceiling
    let randomIndex maxNum = rand.Next(maxNum)

    // selects one tile randomly from a given list
    let selectRandomTilePosition ( assignedTiles: GridBoard ) = assignedTiles.GridPositions.[randomIndex(assignedTiles.GridPositions.Length)]

    //---------------------
    // INITIAL GAMEBOARD FUNCTIONS
    // Step 1: Create a list of values, to be used as the tile values.
    // Returns list of int to be used as the Tile values
    let getGridDimension difficulty =
        match difficulty with
        | Simple -> 3
        | Easy -> 4
        | Medium -> 5
        | Hard -> 6

    let createListOfIntValues gridDimension =
        [1..(gridDimension * gridDimension)]

    let generateGameBoardPositionsBasedOffDifficulty difficulty =
        createListOfIntValues (getGridDimension difficulty)

    // Step 2: Iterate recursively through the generated list of tile values,
        // selects a value randomly from the list of values to create a GameTile, -- THIS SHOULD USE AN EXPOSED SAFE CONSTRUCTOR!!!
        // adds created GameTile to new list that will hold the initial order of the tiles.
    // small helper for empty list for new games
    let unassignedTiles = { GridPositions = [] }

    // recursive function to iterate through list of generated game tile values
    let rec createRandomGameTiles (assignedTiles: GridBoard) (remainingIndexes: int list) =
        // take random element from the array list
        let randomTileValue = remainingIndexes.[randomIndex remainingIndexes.Length]
        // filter out the selected element
        let remain = List.filter(fun elem -> elem <> randomTileValue) remainingIndexes
        // create game tile from index and the selected element // THIS SHOULD USE A CONSTRUCTOR FUNCTION
        // let gameTile = { SortTile = Some randomTileValue;}
        let gameTile = GridGame.TileSortLaneObject { Value = Some randomTileValue }
        // recurse back through if there are remaining positions
        match remain with
        | [] -> { GridPositions = gameTile :: assignedTiles.GridPositions }
        | y::ys -> createRandomGameTiles { GridPositions = gameTile :: assignedTiles.GridPositions } remain

    // Step 3: Randomly blank a tile value to None, to act as the void space for sliding a tile.
    // remove one entry randomly to get the blank starting position
    let randomBlankPosition (assignedTiles: GridBoard) =
        let selectedTile = selectRandomTilePosition assignedTiles
        List.map (fun x -> if x = selectedTile then GridGame.TileSortLaneObject { Value = None } else x ) assignedTiles.GridPositions

    // STEP 4: ASSIGN GENERATED TILE ROUND TO INITIAL AND CURRENT
    let createNewRoundGameBoardBasedOnDifficulty difficulty =
        let newRoundTilePositions = generateGameBoardPositionsBasedOffDifficulty difficulty
        let randomizedNewRoundTilePositions = createRandomGameTiles unassignedTiles newRoundTilePositions
        {GridPositions = randomBlankPosition randomizedNewRoundTilePositions}

    //---------------------
    let getValueOrZeroFromGameTile (gameTile) =
        match gameTile with
        | GridGame.TileSortLaneObject i ->
            match i.Value with
            | Some i -> i
            | None -> 0
        | _ -> 0

    let convertValueToProperString tileValue =
        match tileValue with
        | 0 -> ""
        | _ -> string tileValue

    // //---------------------
    let selectedTilePositionIndex (gameGrid : GridBoard) selectedTile : int option =
        gameGrid.GridPositions
        |> List.tryFindIndex ( fun x -> x = selectedTile )
    
    let blankTilePositionIndex (gameGrid : GridBoard) =
        gameGrid.GridPositions
        |> List.tryFindIndex (
            fun x ->
                match x with
                | TileSortLaneObject tile ->
                    tile.Value = None
                | _ -> false
        )

    let checkTileGroup tileGroup selected =
        [ for tile in tileGroup do
            if tile = selected then true else false ]
        |> List.contains true

    let sortTilesFromGridBoard grid =
        List.filter ( fun x -> 
            match x with 
            | TileSortLaneObject x ->
                true
            | _ -> false
        ) grid.GridPositions

    let checkSwapInColumn selectedIndex blankIndex tiles selected gridDimension =
        let tileColumns = 
            getPositionsAsColumns tiles gridDimension
        let selectedColIndex = 
            unwrapIndex ( List.tryFindIndex ( fun x -> checkTileGroup x selected ) ( tileColumns ) )
        let blankColIndex = 
            unwrapIndex ( List.tryFindIndex ( fun x -> checkTileGroup x  ( TileSortLaneObject { Value = None } ) ) ( tileColumns ) )
        
        if ( selectedColIndex <> -1 && selectedColIndex = blankColIndex )
            then
                if selectedIndex + gridDimension = blankIndex || selectedIndex - gridDimension = blankIndex
                    then true
                    else false
            else false

    let checkSwapInRow selectedIndex blankIndex tiles selected gridDimension =
        let tileRows = 
            getPositionsAsRows tiles gridDimension
        let selectedColIndex = 
            unwrapIndex ( List.tryFindIndex ( fun x -> checkTileGroup x selected ) ( tileRows ) )
        let blankColIndex = 
            unwrapIndex ( List.tryFindIndex ( fun x -> checkTileGroup x  ( TileSortLaneObject { Value = None } ) ) ( tileRows ) )
        if ( selectedColIndex <> -1 && selectedColIndex = blankColIndex )
            then
                if selectedIndex + 1 = blankIndex || selectedIndex - 1 = blankIndex
                    then true
                    else false
            else false

    let swapBlankWithSelected tiles selected selectedIndex blankIndex =
        let selectedSwapped = updatePositionWithObject tiles selected blankIndex
        let blankSwapped = updatePositionWithObject selectedSwapped (TileSortLaneObject { Value = None }) selectedIndex
        blankSwapped

    let selectedCanSwapWithBlank tiles selected gridDimension =
        let selectedIndex = unwrapIndex ( getObjectPositionIndex tiles selected )
        let blankIndex = unwrapIndex ( getObjectPositionIndex tiles ( TileSortLaneObject { Value = None } ) )
        if selectedIndex = -1 || blankIndex = -1
            then tiles
            else
                let colSwap = checkSwapInColumn selectedIndex blankIndex tiles selected gridDimension
                let rowSwap = checkSwapInRow selectedIndex blankIndex tiles selected gridDimension
                if (colSwap || rowSwap) 
                    then swapBlankWithSelected tiles selected selectedIndex blankIndex
                    else tiles

    // // BLANK TILE VALUE POSITION VALIDATION
    // // gets the current index of the Blank Tile and adds one to the index
    let caclulatedValueOfBlankTile currentTiles =
        match (blankTilePositionIndex currentTiles) with
        | Some i -> Some (i + 1)
        | _ -> None

    // checks that the calculated value of the blank tile doesn't exist in the tiles
    // list should be empty, as the missing value should not exist
    let calculatedBlankValueNotFoundInTiles currentTiles =
        let missingValue = caclulatedValueOfBlankTile currentTiles
        let filteredByMissing = List.filter (fun x -> x = GridGame.TileSortLaneObject { Value = missingValue }) currentTiles.GridPositions
        List.isEmpty filteredByMissing


    // simplify calls of calculating and checking Blanks calculated value
    let checkBlankTileIsAtCorrectPosition currentTiles =
        calculatedBlankValueNotFoundInTiles currentTiles

    //---------------------
    // VALUE POSITION VALIDATION
    // gameboard list is the same as the list if sorted by value
    // need to filter out None
    let checkTilesInCorrectOrder (currentTiles: GridBoard) =
        (List.filter (fun x -> x <> GridGame.TileSortLaneObject { Value = None }) currentTiles.GridPositions) = (List.filter (fun x -> x <> GridGame.TileSortLaneObject { Value = None }) currentTiles.GridPositions |> List.sortBy (fun x -> Some x))


    //---------------------
    // WIN CONDITIONS VALIDATION
    let winValidator currentTiles =
        checkTilesInCorrectOrder currentTiles && checkBlankTileIsAtCorrectPosition currentTiles

    //---------------------
    // CHANGE DIFFICULTY - UPDATE DIFFICULTY WITH SELECTION
    let changeDifficulty difficulty model =
        { model with Difficulty = difficulty }

    // MOVE TILE - UPDATE TURNS WITH SELECTED
    let addTileValueToTurnHistory selectedTile model =
        { model with Turns = (selectedTile :: model.Turns) }

    // MOVE TILE - UPDATE CURRENT WITH SELECTED & BLANK SWAPPED
    let updateCurrentTilesWithMove gameboard model =
        { model with CurrentTiles = gameboard }

    // REWIND - POP HEAD FROM TURNS, SWAP WITH BLANK (REMOVES HEAD FROM TURNS)
    let rewindCurrentTiles model =
        if not (List.isEmpty model.Turns) then
            let rewindTile = model.Turns.Head
            let selectedTile = TileSortLaneObject { Value = Some rewindTile }
            let selectedIndex = unwrapIndex ( selectedTilePositionIndex model.CurrentTiles selectedTile )
            let blankIndex = unwrapIndex ( blankTilePositionIndex model.CurrentTiles )
            { model with 
                CurrentTiles = swapBlankWithSelected (model.CurrentTiles) (selectedTile) ( selectedIndex ) (blankIndex)
                Turns = model.Turns.Tail
            }
        else model

    let createNewRound model =
        let newRound = createNewRoundGameBoardBasedOnDifficulty model.Difficulty
        { model with
            InitialTiles = newRound
            CurrentTiles = newRound
            Turns = []
            GameState = Playing
        }

    // RESET ROUND - CURRENT TILES = INITIAL TILES && TURNS = []
    let resetRound model =
        { model with 
            CurrentTiles = model.InitialTiles
            Turns = []
        }

    let initModel =
        let initialDifficulty = Simple
        let initialRound = createNewRoundGameBoardBasedOnDifficulty initialDifficulty
        {
            Difficulty = initialDifficulty
            CurrentTiles = initialRound
            InitialTiles = initialRound
            Turns = []
            GameState = Playing
        }

module SharedWebAppViewSections =
    type ProfessionalServicesView =
        | ServicesLanding
        | AI
        | Automation
        | Integration
        | Website
        | SalesPlatform
        | Development

        member x.toUrlString  =
            match x with
            | ServicesLanding -> "/services"
            | AI -> "/ai-services"
            | Automation -> "/automation-services"
            | Integration -> "/integration-services"
            | Website -> "/web-services"
            | SalesPlatform -> "/sales-services"
            | Development -> "/development-services"

    type AppView =
        | AboutAppView
        | ContactAppView
        | ProfessionalServicesAppView of ProfessionalServicesView
        | LandingAppView
        | ShopAppView
        | PortfolioAppLandingView
        | PortfolioAppCodeView
        | PortfolioAppDesignView
        | ResumeAppView
        | WelcomeAppView

    let appSectionStringTitle appSection =
        match appSection with
        | AboutAppView -> "About"
        | PortfolioAppCodeView -> "Code"
        | ContactAppView -> "Contact"
        | ProfessionalServicesAppView _ -> "Services"
        | LandingAppView -> "Landing"
        | PortfolioAppDesignView -> "Design"
        | PortfolioAppLandingView -> "Projects"
        | ResumeAppView -> "Resume"
        | ShopAppView -> "Shop"
        | WelcomeAppView -> "Welcome"

module SharedCodeGallery =
    
    open SharedGoalRoll
    open SharedTileTap
    open SharedTileSort

    type GallerySection =
        | Gallery
        | GoalRoll
        | TileSort
        | TileTap
        | PivotPoint

    type Msg =
        | BackToPortfolio
        | LoadSection of GallerySection
        | LoadSourceCode of GallerySection
        | GoalRollMsg of SharedGoalRoll.Msg
        | TileTapMsg of SharedTileTap.Msg
        | TileSortMsg of SharedTileSort.Msg
        | PivotPointMsg of SharedPivotPoint.Msg

    type Model =
        | CodeGallery
        | GoalRoll of SharedGoalRoll.Model
        | TileTap of SharedTileTap.Model
        | TileSort of SharedTileSort.Model
        | PivotPoint of SharedPivotPoint.Model
        | SourceCode of GallerySection

    let getInitialModel = CodeGallery

module SharedDesignGallery =

    type Msg =
        | BackToPortfolio
        | SetCurrentPieceIndex of int

    type Model = {
        CurrentPieceIndex: int
    }

    let getInitialModel = { CurrentPieceIndex = 0; }

module SharedServices =

    type ServiceFeature = { 
        Title: string
        Description: string 
    }

    type ServiceTier = { 
        Name: string
        Items: string list
    }

    type ServicePricingPlan = {
        Name: string
        Setup: string
        Monthly: string
    }

    type StatTrend =
    | Up
    | Down

    type ServiceStat =
        { 
            Label : string
            Value : string
            Trend : StatTrend }

    type ServiceIndustry =
        { 
            Name        : string
            Summary     : string
            Outcomes    : string list }

    type ServiceCapability =
        { 
            Heading     : string
            Icon        : string      // emoji for now
            Description : string }

    type ServicePageModel =
        { 
            Id                     : string
            Name                   : string
            HeroTitle              : string
            HeroSubtitle           : string
            HeroBadge              : string option
            HeroGradientClass      : string

            CoreSectionTitle       : string
            CoreFeatures           : ServiceFeature list

            TierSectionTitle       : string
            Tiers                  : ServiceTier list

            PricingSectionTitle    : string
            PricingPlans           : ServicePricingPlan list

            StatsSectionTitle      : string
            Stats                  : ServiceStat list

            // NEW
            IndustriesSectionTitle : string option
            Industries             : ServiceIndustry list

            CapabilitiesSectionTitle : string option
            Capabilities             : ServiceCapability list

            CtaText                : string }

    type Industry =
        | Contractor
        | Lawyer
        | Doctor
        | InsuranceAgency
        | RealEstate
        | Retail
        | Restaurant
        | SmallBusiness
        | Other

    type SharedServiceSectionMsg =
        | GoToLanding
        | GoToSection of SharedWebAppViewSections.ProfessionalServicesView

    type Msg =
        | LoadSection of SharedWebAppViewSections.AppView
        | AISectionMsg of SharedServiceSectionMsg

    type Model = {
        CurrentSection: SharedWebAppViewSections.ProfessionalServicesView
    }
        
    let getInitialModel section = { CurrentSection = section }

module SharedPortfolioGallery =
    
    open SharedCodeGallery
    open SharedDesignGallery

    type Msg =
        | LoadSection of SharedWebAppViewSections.AppView
        | ArtGalleryMsg of SharedDesignGallery.Msg
        | CodeGalleryMsg of SharedCodeGallery.Msg

    type Model =
        | PortfolioGallery
        | CodeGallery of SharedCodeGallery.Model
        | DesignGallery of SharedDesignGallery.Model

    let getInitialModel = PortfolioGallery

module SharedAboutSection =

    type Msg =
        | ToggleModal of int
        | SwitchModal of int
        | SwitchSection of SharedWebAppViewSections.AppView

    type Model = {
        ActiveModalIndex : int
        ModalIsActive : bool
    }

    // add images
    // revert back to string list for bullet points?
    type ModalContent = {
        Title: string
        Image: string option
        MainContent: string
        PreviousLabel: string
        NextLabel: string
    }
       
    let getInitialModel = {ActiveModalIndex = 0; ModalIsActive = false}

module SharedWelcome = 
    type Msg =
        | SwitchSection of SharedWebAppViewSections.AppView





















module Store =

    open Shared.PrintfulCommon
    open Shared.PrintfulStoreDomain.ProductTemplateResponse

    module Collection =

        type Model = {
            Filters    : Filters
            Paging     : PagingInfoDTO
            SearchTerm : string option
            Products   : Deferred<ProductTemplate list>
            TotalCount : int
            IsLoading  : bool
            Error      : string option
        }

        type Msg =
            | InitFromQuery of string   // raw query string
            | LoadMore
            | FiltersChanged of Filters
            | SearchChanged of string
            | SortChanged of sortType: string * sortDir: string
            | ApplyFilterPreset of string
            | SaveFilterPreset of string
            // Printful STORE Products
            | GetProducts
            | GotProducts of ProductTemplatesResponse
            | FailedProducts of exn
            | ViewProduct of ProductTemplate
            | FeaturedClick of ProductTemplate option

        let initModel () : Model = { 
                Filters    = defaultFilters
                Paging     = emptyPaging
                SearchTerm = None
                Products   = Deferred.HasNotStartedYet
                TotalCount = 0
                IsLoading  = false
                Error      = None 
            }


    module ProductDesigner =

        module DesignerOptions =

            type DesignSize =
                | Small
                | Medium
                | Large

            type VerticalAlign = Top | Center | Bottom
            type TransformMode = ModePosition | ModeRotate | ModeScale

        open DesignerOptions
        
        module ProductOptions =

            // Color and Size ?

            type DesignTechnique =
                | DTG
                | Embroidery
                | Digital
                | Knitting
                | Sublimation
                | Other of string

                member this.ToPrintfulTechnique() =
                    match this with
                    | DTG         -> "dtg"
                    | Embroidery  -> "embroidery"
                    | Digital     -> "digital"
                    | Knitting    -> "knitting"
                    | Sublimation -> "sublimation"
                    | Other t     -> t

            // ------------------------
            // Top-level asset/image
            // ------------------------

            type AssetId = Guid

            /// A reusable artwork asset (library or upload)
            type ImageAsset = {
                Id            : AssetId
                Name          : string
                ImageUrl      : string
                Tagline       : string option
                TechniqueHint : DesignTechnique option
                WidthPx  : int option
                HeightPx : int option
            }

        open ProductOptions

        type StepDesigner =
            | SelectBaseProduct
            | SelectVariants
            | ProductDesigner
            | ReviewDesign

        module Designs =

            open DesignerOptions

            // ------------------------
            // Placed instance
            // ------------------------

            /// One *use* of an ImageAsset on a specific product (front, back, etc.)
            type DesignOptions = {
                /// Distinguishes multiple uses of the *same* asset (front + back, etc.)
                InstanceId   : Guid
                Asset        : ImageAsset
                HitArea      : DesignHitArea
                Size         : DesignSize
                Technique    : DesignTechnique
                Position     : PrintfulPosition option
                VerticalAlign : VerticalAlign option
                LayerOptions : PrintfulLayerOption list
            }

            
            type Placement =
                | Front
                | Back
                | LargeBack
                | LeftSleeve
                | RightSleeve
                | OutsideLabel

            module Defaults =

                /// “Promote” a raw ImageAsset into a first DesignOptions with defaults
                let toPlaced (hitArea: DesignHitArea) (size: DesignSize) (asset: ImageAsset) : DesignOptions =
                    {
                        InstanceId   = Guid.NewGuid()
                        Asset        = asset
                        HitArea      = hitArea
                        Size         = size
                        Technique    = asset.TechniqueHint |> Option.defaultValue DTG
                        Position     = None
                        VerticalAlign= None
                        LayerOptions = []
                    }

                let all : ImageAsset list = [
                    {
                        Id           = Guid.NewGuid()
                        Name         = "Bowing Bubbles"
                        ImageUrl     = "./../img/artwork/bowing-bubbles.jpg"
                        Tagline      = Some "Sweet as chewed gum."
                        TechniqueHint = None
                        WidthPx  = None
                        HeightPx = None
                    }
                    {
                        Id           = Guid.NewGuid()
                        Name         = "Caution: Very Hot"
                        ImageUrl     = "./../img/artwork/caution-very-hot.jpg"
                        Tagline      = Some "Handle with xero effort."
                        TechniqueHint = None
                        WidthPx  = None
                        HeightPx = None
                    }
                    {
                        Id           = Guid.NewGuid()
                        Name         = "Misfortune"
                        ImageUrl     = "./../img/artwork/misfortune.png"
                        Tagline      = Some "It was always somewhere in the cards."
                        TechniqueHint = None
                        WidthPx  = None
                        HeightPx = None
                    }
                    {
                        Id           = Guid.NewGuid()
                        Name         = "Out for Blood"
                        ImageUrl     = "./../img/artwork/out-for-blood.png"
                        Tagline      = Some "Always thirsty."
                        TechniqueHint = None
                        WidthPx  = None
                        HeightPx = None
                    }
                ]

            /// Helper to go from a PlacedDesign to a PrintfulPlacement + Layer
            module ToPrintful =

                let private sizeToRelativeBox = function
                    // these are “relative” sizes (0.0–1.0 of print area); you can refine later
                    | Small  -> 0.4
                    | Medium -> 0.7
                    | Large  -> 1.0

                /// Given a product’s print-area dimensions (inches), derive a rough position box
                let autoCenterPosition (areaWidth: float) (areaHeight: float) (size: DesignSize) : PrintfulPosition =
                    let scale  = sizeToRelativeBox size
                    let width  = areaWidth * scale
                    let height = areaHeight * scale
                    {
                        Width  = width
                        Height = height
                        Left   = (areaWidth  - width)  / 2.0
                        Top    = (areaHeight - height) / 2.0
                    }

                /// Convert a single DesignOptions into a PrintfulPlacement
                let toPlacementAndLayer
                    (printAreaWidth : float)
                    (printAreaHeight: float)
                    (pd: DesignOptions)
                    : PrintfulPlacement =

                    let position =
                        pd.Position
                        |> Option.defaultWith (fun () -> autoCenterPosition printAreaWidth printAreaHeight pd.Size)

                    {
                        Placement = pd.HitArea.ToPrintfulPlacement()
                        Technique = pd.Technique.ToPrintfulTechnique()
                        Layers    = [
                            {
                                Type         = "file"
                                Url          = pd.Asset.ImageUrl
                                LayerOptions = pd.LayerOptions
                                Position     = Some position
                            }
                        ]
                    }

        open Designs

        type Transform = {
            Pos      : PrintfulPosition
            Rotation : float
        }

        type DesignLayer = {
            Id        : string
            Name      : string
            Technique : ProductOptions.DesignTechnique
            ActivePlacement : Placement
            // store transforms per placement so switching tabs doesn’t destroy work
            ByPlacement : Map<Placement, Transform>
        }

        type NormRect = { X: float; Y: float; W: float; H: float } // 0..1 in preview space

        type Msg =
            
            | BackToDropLanding
            | LoadProducts
            | ProductsLoaded of Shared.PrintfulStoreDomain.CatalogProductResponse.CatalogProductsResponse
            | LoadFailed of string
            | ViewProduct of CatalogProduct
            
            | GoToStep of StepDesigner
            | SelectBase of CatalogProduct
            | SelectColor of string
            | SelectSize of string

            // image-level selection
            | AddAsset    of ImageAsset
            | RemoveAsset of AssetId

            | DesignerSearchChanged of string
            | DesignerSortChanged of string * string
            | DesignerFiltersChanged of Filters
            | DesignerPageChanged of PagingInfoDTO

            | SelectActiveLayer of int
            | SetPlacement of layerIdx:int * placement:Placement

            // placement-level edits
            | SetActivePlaced of int
            | UpdatePlacedPlacement of index:int * DesignHitArea
            | UpdatePlacedSize      of index:int * DesignSize
            | UpdatePlacedTechnique  of index:int * DesignTechnique
            | UpdatePlacedPosition  of index:int * PrintfulPosition option
            | UpdatePlacedVerticalAlign of index:int * VerticalAlign
            | SetTransformMode of TransformMode

            | AddToCart of quantity:int
            

        type Model = {
            Products             : Deferred<CatalogProduct list>
            Paging               : PagingInfoDTO
            Query                : Filters
            SearchTerm           : string

            CurrentStep          : StepDesigner
            SelectedProduct      : CatalogProduct option
            SelectedVariantSize  : string option
            SelectedVariantColor : string option

            // NEW
            SelectedVariantId   : int option
            AvailableAssets      : ImageAsset list  // library/mock list
            SelectedAssets       : ImageAsset list  // “added designs” in the Add Designs step
            DesignOptions        : DesignOptions list
            ActivePlacedIndex    : int option
            TransformMode     : TransformMode
            Placements           : PlacementOption list
        }
 
        // ADD DEFERRED!!!!
        let initialModel () = {
            Products = Deferred.HasNotStartedYet
            Paging = emptyPaging
            Query = defaultFilters
            SearchTerm = ""
            CurrentStep = SelectBaseProduct
            SelectedProduct = None
            SelectedVariantSize = None
            SelectedVariantColor = None
            SelectedVariantId = None
            AvailableAssets = Defaults.all
            SelectedAssets = []
            DesignOptions = []
            ActivePlacedIndex = None
            TransformMode = ModePosition
            Placements = [
                {
                    Label   = "Front"
                    HitArea = DesignHitArea.Front
                }
                {
                    Label   = "Back"
                    HitArea = DesignHitArea.Back
                }
                {
                    Label   = "Pocket"
                    HitArea = DesignHitArea.Pocket
                }
                {
                    Label   = "Left Sleeve"
                    HitArea = DesignHitArea.LeftSleeve
                }
                {
                    Label   = "Right Sleeve"
                    HitArea = DesignHitArea.RightSleeve
                }
                {
                    Label   = "Left Leg"
                    HitArea = DesignHitArea.LeftLeg
                }
                {
                    Label   = "Right Leg"
                    HitArea = DesignHitArea.RightLeg
                }
                {
                    Label   = "Left Half"
                    HitArea = DesignHitArea.LeftHalf
                }
                {
                    Label   = "Right Half"
                    HitArea = DesignHitArea.RightHalf
                }
                {
                    Label   = "Center"
                    HitArea = DesignHitArea.Center
                }
            ]
        }
    
    
    module ProductViewer =

        open Shared.StoreProductViewer

        type Model =
            { 
                Key                 : ProductKey
                Seed                : ProductSeed option
                ReturnTo            : ReturnTo
                Details             : Deferred<GetDetailsResponse>
                SelectedColor       : string option
                SelectedSize        : string option
                SelectedVariantId   : int option
            }

        type Msg =
            | InitWith of key:ProductKey * seed:ProductSeed option * returnTo:ReturnTo
            | LoadDetails
            | GotDetails of GetDetailsResponse
            | FailedDetails of exn

            | SelectColor of string
            | SelectSize  of string
            | SelectVariant of int

            // “primary CTA”
            | PrimaryAction

            // navigation hooks
            | GoBack

        let keyFromSeed = function
            | SeedCatalog p  -> Catalog p.id
            | SeedTemplate t -> Template t.id

        let initModel (key: ProductKey) (seed: ProductSeed option) (returnTo: ReturnTo) : Model =
            { 
                Key               = key
                Seed              = seed
                ReturnTo          = returnTo
                Details           = Deferred.HasNotStartedYet
                SelectedColor     = None
                SelectedSize      = None
                SelectedVariantId = None }

        let detailsReq (m: Model) : GetDetailsRequest =
            { 
                Key               = m.Key
                SelectedColorOpt  = m.SelectedColor
                SelectedSizeOpt   = m.SelectedSize
                SelectedVariantId = m.SelectedVariantId }

    
    open ProductDesigner.Designs
    open Shared.StoreProductViewer

    module PrintfulMapping =
        open Shared.Store.Cart

        let mapDesignToPlacement (d: ProductDesigner.Designs.DesignOptions) : PrintfulPlacement =
            {
                Placement = d.HitArea.ToPrintfulPlacement()
                Technique = d.Technique.ToPrintfulTechnique()
                Layers = [
                    {
                        Type = "file"
                        Url = d.Asset.ImageUrl
                        Position = d.Position
                        LayerOptions = []
                    }
                ]
            }

        let sizeToDefaultPosition =
            function
            | Small  -> { Width = 3.0; Height = 3.0; Left = 0.0; Top = 0.0 }
            | Medium -> { Width = 6.0; Height = 6.0; Left = 0.0; Top = 0.0 }
            | Large  -> { Width = 10.0; Height = 10.0; Left = 0.0; Top = 0.0 }

        let toPrintfulPlacements (placed: DesignOptions list) : PrintfulPlacement list =
            placed
            |> List.groupBy (fun d -> d.HitArea, d.Technique)
            |> List.map (fun ((hitArea, tech), ds) ->
                {
                    Placement = hitArea.ToPrintfulPlacement()
                    Technique = tech.ToPrintfulTechnique()
                    Layers =
                        ds
                        |> List.map (fun d ->
                            {
                                Type = "file"
                                Url = d.Asset.ImageUrl
                                LayerOptions = d.LayerOptions
                                Position = d.Position |> Option.orElse (Some (sizeToDefaultPosition d.Size))
                            }
                        )
                }
            )

        let toCustomCartDU
            (qty   : int)
            (model : ProductDesigner.Model)
            : CartLineItem option =

            match model.SelectedVariantId,
                model.SelectedProduct,
                model.DesignOptions with

            // must have a variant, a base product, and at least one placed design
            | None, _, _ -> None
            | _, None, _ -> None
            | _, _, []   -> None

            | Some variantId, Some product, placedDesigns ->

                // 1. Build Printful placements from the placed designs
                let placements : PrintfulPlacement list =
                    placedDesigns |> toPrintfulPlacements

                // 2. Preview image from the selected product
                let previewImage =
                    product.thumbnailURL
                    |> function
                        | null | "" -> None
                        | url       -> Some url

                // 3. Size + color from the designer selections
                let size =
                    model.SelectedVariantSize
                    |> Option.defaultValue ""

                // For now we’ll treat the selected color string as the "name",
                // and assume we *might* also use it as a hex code.
                let colorName, colorCodeOpt =
                    match model.SelectedVariantColor with
                    | None
                    | Some "" ->
                        "Default", None
                    | Some c ->
                        // naive heuristic: if it looks like a hex code, use it as both
                        if c.StartsWith("#") then c, Some c
                        else c, None

                // 4. Price — placeholder for now until you wire real pricing
                let unitPrice : decimal =
                    // TODO: plug in a real price, from product/variant pricing
                    45.0m

                // 5. Build the rich CartItem record (your commented-out type)
                let baseItem : CartItem =
                    {
                        VariantId         = variantId
                        Placements        = placements
                        PreviewImage      = previewImage

                        CatalogProductId  = product.id
                        CatalogVariantId  = variantId
                        Name              = product.name
                        ThumbnailUrl      = product.thumbnailURL

                        Size              = size
                        ColorName         = colorName
                        ColorCodeOpt      = colorCodeOpt

                        Quantity          = qty
                        UnitPrice         = unitPrice
                        IsCustomDesign    = true
                    }

                // 6. Wrap it in the DU
                Some (Custom baseItem)


    type ShopSection =

        | ShopLanding // this is is a welcome page

        | ProductDesigner of ProductDesigner.Model // build your own, should be able to control whether or not we allow the user to upload their own images.
        
        | CollectionBrowser of Collection.Model
        
        | ProductViewer of ProductViewer.Model
        
        | ShoppingBag
        | Checkout
        | Payment
        | NotFound

        // | Social
        // | Contact
        
    type ShopLandingMsg =
        | SwitchSection of ShopSection

    // extend to have invalid state impossible
    type ShopProductTemplatesMsg =
        | SwitchSection of ShopSection
        | Next
        | Back
        | ViewProductTemplate of ProductTemplate
        | ChooseVariantSize of string
        | ChooseVariantColor of string
        | GetProductTemplates
        | GotProductTemplates of Shared.PrintfulStoreDomain.ProductTemplateResponse.ProductTemplatesResponse
        | FailedProductTemplates of exn
        | AddProductToBag of ProductTemplate


    type Tab =
        | Hero
        | Collection
        | Designer
        | Product
        | Cart
        | Checkout

    open Shared.Store.Cart

    type ShopMsg =
    
        | NavigateTo of ShopSection

        | ShopLanding of ShopLandingMsg
        
        | ShopCollectionMsg of Collection.Msg
        
        | ShopDesignerMsg of ProductDesigner.Msg
        
        | ViewProduct of ProductKey * ProductSeed option * ReturnTo
        
        | ShopProduct of ProductViewer.Msg
        
        | AddCartItem      of CartLineItem
        | RemoveCartItem   of CartLineItem
        | UpdateCartQty    of CartLineItem * int

    type Model = {
        Section: ShopSection
        Cart: CartState
        // PayPalOrderReference: string option
        CheckoutTax: float option
        CheckoutShipping: float option
    }

    let getInitialModel shopSection = {
        Section = shopSection
        Cart = emptyCart
        // PayPalOrderReference = None
        CheckoutTax = None
        CheckoutShipping = None
    }








module SharedPage =
    
    type CodeSection =
        | CodeLanding
        | GoalRoll
        | TileTap
        | TileSort
        | PivotPoint

    type PortfolioSection =
        | PortfolioLanding
        | Code of CodeSection
        | Design //of int // load a design index

    type Page =
        | About
        | Contact
        | Landing
        | Portfolio of PortfolioSection
        | Services of SharedWebAppViewSections.ProfessionalServicesView
        | Resume
        | Shop of Store.ShopSection
        | Welcome

module SharedWebAppModels =
    
    type Theme =
        | Light
        | Dark
        | Cupcake
        | Bumblebee
        | Emerald
        | Corporate
        | Synthwave
        | Retro
        | Cyberpunk
        | Valentine
        | Halloween
        | Garden
        | Forest
        | Aqua
        | Lofi
        | Pastel
        | Fantasy
        | Wireframe
        | Black
        | Luxury
        | Dracula
        | Cmyk
        | Autumn
        | Business
        | Acid
        | Lemonade
        | Night
        | Coffee
        | Winter
        | Dim
        | Nord
        | Sunset

        member this.AsString =
            match this with
            | Light -> "light"
            | Dark -> "dark"
            | Cupcake -> "cupcake"
            | Bumblebee -> "bumblebee"
            | Emerald -> "emerald"
            | Corporate -> "corporate"
            | Synthwave -> "synthwave"
            | Retro -> "retro"
            | Cyberpunk -> "cyberpunk"
            | Valentine -> "valentine"
            | Halloween -> "halloween"
            | Garden -> "garden"
            | Forest -> "forest"
            | Aqua -> "aqua"
            | Lofi -> "lofi"
            | Pastel -> "pastel"
            | Fantasy -> "fantasy"
            | Wireframe -> "wireframe"
            | Black -> "black"
            | Luxury -> "luxury"
            | Dracula -> "dracula"
            | Cmyk -> "cmyk"
            | Autumn -> "autumn"
            | Business -> "business"
            | Acid -> "acid"
            | Lemonade -> "lemonade"
            | Night -> "night"
            | Coffee -> "coffee"
            | Winter -> "winter"
            | Dim -> "dim"
            | Nord -> "nord"
            | Sunset -> "sunset"

    type Model =
        | About of SharedAboutSection.Model
        | Contact
        | Help
        | Settings
        | Shop of Store.Model
        | Landing
        | Services of SharedServices.Model
        | Portfolio of SharedPortfolioGallery.Model
        | Resume
        | Welcome

    type WebAppModel = {
        CurrentAreaModel: Model
        Theme: Theme
    }

    type WebAppMsg =
        | WelcomeMsg of SharedWelcome.Msg
        | AboutMsg of SharedAboutSection.Msg
        | PortfolioMsg of SharedPortfolioGallery.Msg
        | ShopMsg of Store.ShopMsg
        | SwitchToOtherApp of SharedWebAppViewSections.AppView
        | ServicesMsg of SharedServices.Msg
        | LoadPage of SharedPage.Page
        | ErrorMsg of exn // WIP?
        | ChangeTheme of Theme
