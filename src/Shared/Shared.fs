namespace Shared

namespace Shared

open System

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

    type ServiceStat = {
        Label: string
        Value: string 
        Trend : StatTrend
    }

    type ServicePageModel = { 
        Id: string
        Name: string
        HeroTitle: string
        HeroSubtitle: string
        HeroBadge: string option
        HeroGradientClass: string
        CoreSectionTitle: string
        CoreFeatures: ServiceFeature list
        TierSectionTitle: string
        Tiers: ServiceTier list
        PricingSectionTitle: string
        PricingPlans: ServicePricingPlan list
        StatsSectionTitle: string
        Stats: ServiceStat list
        CtaText: string 
    }

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


module SharedShopDomain =
        
    type QuantityAdjustment =
        | Increment
        | Decrement

    type DraftResult = { code: string }

    // Stub/placeholder types for API results
    // Replace with your real server shared types

    type CheckoutTax = { required: bool; rate: float; shipping_taxable: bool }

    type CheckoutShippingRate = { rate: float; name: string }

    type HttpError = string

module PrintfulCommon =
    /// Paging information
    type PagingInfoDTO = {
        total: int
        offset: int
        limit: int
    }

    let emptyPaging = {
        total = 0
        offset = 0
        limit = 20
    }

    /// Navigation links (HATEOAS from Printful)
    type LinksDTO = {
        self: string
        next: string option
        first: string option
        last: string option
    }

    type HateoasLink = {
        href : string
    }

// Refactor Shop Types
module SharedShopV2 =

    module PrintfulCatalog =
        /// Simplified type for displaying catalog products in your app
        type CatalogProduct = {
            id: int
            name: string
            thumbnailURL: string
            description: string option
            brand: string option
            model: string option
            variantCount: int
            isDiscontinued: bool
            sizes: string list
            colors: {| Color : string; ColorCodeOpt : string option |} list
        }

        type Filters = {
            Categories: int list
            Colors: string list
            Sizes: string list
            Placements: string list
            Techniques: string list
            OnlyNew: bool
            SellingRegion: string option
            DestinationCountry: string option
            SortDirection: string option
            SortType: string option
        }

        let defaultFilters : Filters = {
            Categories = []
            Colors = []
            Sizes = []
            Placements = []
            Techniques = []
            OnlyNew = false
            SellingRegion = None
            DestinationCountry = None
            SortDirection = None
            SortType = None
        }

    open PrintfulCatalog

    module ProductTemplate =

        // Fable-safe DTOs only
        type OptionData = {
            id    : string
            value : string array
        }

        type Color = {
            color_name  : string
            color_codes : string array
        }

        type PlacementOption = {
            placement               : string
            display_name            : string
            technique_key           : string
            technique_display_name  : string
            options                 : string array
        }

        type PlacementOptionData = {
            ``type`` : string
            options  : string array
        }

        type ProductTemplate = {
            id                    : int
            product_id            : int
            external_product_id   : string option
            title                 : string
            available_variant_ids : int array
            option_data           : OptionData array
            colors                : Color array
            sizes                 : string array
            mockup_file_url       : string
            placements            : PlacementOption array
            created_at            : int64
            updated_at            : int64
            placement_option_data : PlacementOptionData array
            design_id             : string option
        }

        type ProductTemplatePaging = {
            total  : int
            limit  : int
            offset : int
        }

        type ProductTemplateResponse = {
            code   : int
            result : ProductTemplate list
            extra  : string array
            paging : PrintfulCommon.PagingInfoDTO
        }

        module ProductTemplateBrowser =

            type Model = {
                Filters: Filters
                Paging: PrintfulCommon.PagingInfoDTO
                Templates: ProductTemplate list
                SelectedTemplate: ProductTemplate option
                IsLoading: bool
                Error: string option
            }

            let initialModel () = {
                Filters = defaultFilters
                Paging = PrintfulCommon.emptyPaging
                Templates = []
                SelectedTemplate = None
                IsLoading = false
                Error = None
            }

    module BuildYourOwnProductWizard =

        type Step =
            | SelectProduct
            | SelectVariant
            | SelectDesign
            | ConfigurePlacement
            | Review

        type CatalogProductsFilters = {
            SelectedCategories: int list
            SelectedColors: string list
            SelectedPlacements: string list
            SelectedTechniques: string list
            OnlyNew: bool
            SellingRegion: string option
            DestinationCountry: string option
            SortDirection: string option
            SortType: string option
        }

        let defaultCatalogProductsFilters = {
            SelectedCategories = []
            SelectedColors = []
            SelectedPlacements = []
            SelectedTechniques = []
            OnlyNew = false
            SellingRegion = None
            DestinationCountry = None
            SortDirection = None
            SortType = None
        }

        type Model = {
            products: PrintfulCatalog.CatalogProduct list
            paging: PrintfulCommon.PagingInfoDTO
            query: Filters
            currentStep: Step
            selectedProduct: PrintfulCatalog.CatalogProduct option
            selectedVariantSize: string option
            selectedVariantColor: string option
            selectedDesign: string option
            placements: (string * string) list // placement, url
        }


        // ADD DEFERRED!!!!
        let initialState () = {
            products = []
            paging = PrintfulCommon.emptyPaging
            query = defaultFilters
            currentStep = SelectProduct
            selectedProduct = None
            selectedVariantSize = None
            selectedVariantColor = None
            selectedDesign = None
            placements = []
        }


    type ShopSection =
        | ShopLanding // this is is a welcome page
        | ShopTypeSelector // this is landing page, for selecting either of the two workflows
        | ProductTemplateBrowser of ProductTemplate.ProductTemplateBrowser.Model // product template browser, not yet created
        | BuildYourOwnWizard of BuildYourOwnProductWizard.Model // build your own, should be able to control whether or not we allow the user to upload their own images.
        | ShoppingBag
        | Checkout
        | Payment
        | Social
        | Contact
        | NotFound

module SharedShopV2Domain =

    module ProductTemplateResponse =
        type ProductTemplatesResponse = {
            templateItems : SharedShopV2.ProductTemplate.ProductTemplate list
            paging : PrintfulCommon.PagingInfoDTO
        }

    module CatalogProductResponse =
        
        module CatalogProduct =

            open PrintfulCommon

            type ProductLinks = {
                self  : HateoasLink
                next  : HateoasLink option
                first : HateoasLink option
                last  : HateoasLink option
            }

            type Color = {
                name : string
                value : string option
                // image : string option
            }

            type Technique = {
                key : string
                display_name : string
            }

            type DesignPlacement = {
                placement : string
                display_name : string
            }

            type CatalogOption = {
                id : string
                title : string
                type' : string
            }

            type PrintfulProduct = {
                id              : int
                main_category_id: int
                ``type``        : string
                name            : string
                brand           : string option
                model           : string option
                image           : string
                variant_count   : int
                is_discontinued : bool
                description     : string
                sizes           : string array
                colors          : Color array
                techniques      : Technique array
                placements      : DesignPlacement array
                product_options : CatalogOption array
                _links          : ProductLinks
            }

            type PrintfulCatalogProductResponse = {
                data   : PrintfulProduct array
                paging : PrintfulCommon.PagingInfoDTO
                _links : ProductLinks
            }

        let mapPrintfulProduct (p: CatalogProduct.PrintfulProduct) : SharedShopV2.PrintfulCatalog.CatalogProduct =
            System.Console.WriteLine($"Mapping product: {p.name} with id {p.id}")
            p.colors |> Array.iter (fun (c: CatalogProduct.Color) -> 
                System.Console.WriteLine $"COLOR: {c.name} with code {c.value}"
            )

            { 
                id = p.id
                name = p.name
                thumbnailURL = p.image
                description = Some p.description
                brand = p.brand
                model = p.model
                variantCount = p.variant_count
                isDiscontinued = p.is_discontinued
                sizes = p.sizes |> List.ofArray
                colors = p.colors |> Array.map (fun c -> {| Color = c.name; ColorCodeOpt = c.value |} ) |> Array.toList
            }
        
        /// API response shaped for the client
        type CatalogProductsResponse = {
            products: SharedShopV2.PrintfulCatalog.CatalogProduct list
            paging: PrintfulCommon.PagingInfoDTO
            links: PrintfulCommon.LinksDTO
        }

        let mapPrintfulResponse (r: CatalogProduct.PrintfulCatalogProductResponse) : CatalogProductsResponse =
            { 
                products = 
                    r.data
                    |> Array.map mapPrintfulProduct
                    |> List.ofArray
                paging = r.paging
                links =
                    { 
                        self = r._links.self.href
                        next = r._links.next |> Option.map (fun l -> l.href)
                        first = r._links.first |> Option.map (fun l -> l.href)
                        last = r._links.last |> Option.map (fun l -> l.href)
                    } 
            }

    type ShopLandingMsg =
        | SwitchSection of SharedShopV2.ShopSection

    type ShopTypeSelectorMsg =
        | SwitchSection of SharedShopV2.ShopSection

    // extend to have invalid state impossible
    type ShopBuildYourOwnProductWizardMsg =
        | SwitchSection of SharedShopV2.ShopSection
        | Next
        | Back
        | ChooseProduct of SharedShopV2.PrintfulCatalog.CatalogProduct
        // | ChooseProduct of string
        | ChooseVariantSize of string
        | ChooseVariantColor of string
        | ChooseDesign of string
        | TogglePlacement of string * string
        | GetAllProducts
        | GotAllProducts of CatalogProductResponse.CatalogProductsResponse
        | FailedAllProducts of exn
        | AddProductToBag of SharedShopV2.PrintfulCatalog.CatalogProduct

    // extend to have invalid state impossible
    type ShopProductTemplatesMsg =
        | SwitchSection of SharedShopV2.ShopSection
        | Next
        | Back
        | ViewProductTemplate of SharedShopV2.ProductTemplate.ProductTemplate
        | ChooseVariantSize of string
        | ChooseVariantColor of string
        | GetProductTemplates
        | GotProductTemplates of ProductTemplateResponse.ProductTemplatesResponse
        | FailedProductTemplates of exn
        | AddProductToBag of SharedShopV2.ProductTemplate.ProductTemplate

module Api =

    open System.Threading.Tasks

    type TaxAddress = {
        Country : string
        State   : string
        City    : string
        Zip     : string
    }

    type ShippingRequest = {
        Address : TaxAddress
        Items   : string list
    }

    type Product = {
        Id : string
        Name : string
        Price : decimal
    }

    module Printful =

        module CatalogProductRequest =

            type CatalogProductsQuery = {
                category_ids: int list option
                colors: string list option
                limit: int option
                newOnly: bool option
                offset: int option
                placements: string list option
                selling_region_name: string option
                sort_direction: string option
                sort_type: string option
                techniques: string list option
                destination_country: string option
            }

            let toApiQuery (paging: PrintfulCommon.PagingInfoDTO) (stateFilters: SharedShopV2.PrintfulCatalog.Filters) : CatalogProductsQuery =
                {
                    category_ids = if stateFilters.Categories |> List.isEmpty then None else Some stateFilters.Categories
                    colors = if stateFilters.Colors |> List.isEmpty then None else Some stateFilters.Colors
                    limit = Some paging.limit
                    offset = Some paging.offset
                    newOnly = Some stateFilters.OnlyNew
                    placements = if stateFilters.Placements |> List.isEmpty then None else Some stateFilters.Placements
                    selling_region_name = stateFilters.SellingRegion
                    sort_direction = stateFilters.SortDirection
                    sort_type = stateFilters.SortType
                    techniques = if stateFilters.Techniques |> List.isEmpty then None else Some stateFilters.Techniques
                    destination_country = stateFilters.DestinationCountry
                }

    type ProductApi = {
        getProducts : 
            Printful.CatalogProductRequest.CatalogProductsQuery -> 
                Async<SharedShopV2Domain.CatalogProductResponse.CatalogProductsResponse>
        getProductTemplates : 
            Printful.CatalogProductRequest.CatalogProductsQuery -> 
                Async<SharedShopV2Domain.ProductTemplateResponse.ProductTemplatesResponse>
        // getProductVariants : int -> Async<SharedShopDomain.CatalogVariant list>
    }

    type PaymentApi = {
        getTaxRate : TaxAddress -> Async<SharedShopDomain.CheckoutTax>
        getShipping : ShippingRequest -> Async<decimal>
        createPayPalOrder : decimal -> Async<string>   // returns order id
        capturePayPalOrder : string -> Async<bool>     // capture by order id
    }

module SharedShop =

    type CartItem =
        | Template of SharedShopV2.ProductTemplate.ProductTemplate * qty:int
        // below is awful
        | Custom of SharedShopV2.BuildYourOwnProductWizard.Model * qty:int


    type ShopMsg =
        | NavigateTo of SharedShopV2.ShopSection
        | ShopLanding of SharedShopV2Domain.ShopLandingMsg
        | ShopTypeSelection of SharedShopV2Domain.ShopTypeSelectorMsg
        | ShopBuildYourOwnWizard of SharedShopV2Domain.ShopBuildYourOwnProductWizardMsg
        | ShopStoreProductTemplates of SharedShopV2Domain.ShopProductTemplatesMsg
        // | ShoppingBagMsg of ShoppingBag.Msg
        // | CheckoutMsg of Checkout.Msg
        // | PaymentMsg of Payment.Msg
        // | ApiFailed of exn
        // | UpdateCustomerForm of UserSignUpFormField
        // | UpdateAddressForm of CustomerAddressFormField
        // | CheckUserSignUpForm
        // | CheckAddressForm
        // | UpdateProductColor of SyncProduct * ProductColor
        // | UpdateProductSize of SyncProduct * ProductSize
        // | AddVariantToShoppingBag of SyncProductVariant
        // | DeleteVariantFromShoppingBag of SyncProductVariant
        // | AdjustLineItemQuantity of QuantityAdjustment * SyncProductVariant
        // Testing
        // | TestApiTaxRate
        // | TestApiShippingRate
        // | Send // apparently testing paypal
        // | GotResult of Result<string, HttpError>
        // | TestApiCustomerDraft of CustomerDraftOrder
        // | GotCustomerOrderDraftResult of Result<DraftResult, HttpError>
        // Tax
        // | GetTaxRateResult of CustomerAddress
        // | GotTaxRateResult of CheckoutTax
        // | FailedTaxRateResult of exn
        // Shipping Rates
        // | GetShippingRateResult of Result<CheckoutShippingRate list, HttpError>
        // | GotShippingRateResult of CheckoutShippingRate list
        // | FailedShippingRateResult of exn
        // Product Variants
        // | GetProductVariants of int
        // | GotProductVariants of SharedShopDomain.CatalogVariant list
        // | FailedProductVariants of exn
        // | RemoveProductFromBag of int

    // I don't like this model, and I need to unify the overall types to be able to submit a printful order 
    // at the end based on the items configured in either workflow.
    // type Model = {
    //     section: SharedShopV2.ShopSection
    //     // customer : Customer option
    //     // productVariationOptionsSelected : ( ProductColor option * ProductSize option )
    //     validSyncVariantId : String option
    //     // -- make this paypal order reference, only set by return of the GotDraftResults, if there is a value, can render the JS Paypal button
    //     payPalOrderReference :  String option 
    //     // shoppingBag : List<CatalogProduct * int>
    //     // shoppingBag : List<SyncProductVariant * int>
    //     // -- ( Variant Line Item, Qty )
    //     checkoutTaxShipping : float option * float option 
    //     // -- Tax, Shipping 
    //     //    HOME PAGE
    //     homeGif : String
    //     //    SIGN UP
    //     // customerSignUpForm : CustomerSignUpForm 
    //     // customerAddressForm : CustomerAddressForm 
    //     // validationResults : List<RequestResponse>
    //     allProducts : SharedShopV2.PrintfulCatalog.CatalogProduct list option
    //     // productVariants : List<CatalogVariant> option
    //     productTemplates : SharedShopV2.ProductTemplate.ProductTemplate list
    //     productTemplate : SharedShopV2.ProductTemplate.ProductTemplateBrowser.Model option
    //     buildYourOwn: SharedShopV2.BuildYourOwnProductWizard.Model option
    // }
    // let getInitialModel shopSection = {
    //     section = shopSection
    //     // customer = None
    //     // productVariationOptionsSelected = (None, None)
    //     validSyncVariantId = None
    //     payPalOrderReference = None
    //     // shoppingBag = []
    //     checkoutTaxShipping = (None, None)
    //     homeGif = ""
    //     // customerSignUpForm = defaultCustomerSignUpForm
    //     // customerAddressForm = defaultCustomerAddressForm
    //     // validationResults = []
    //     allProducts = None
    //     // productVariants = None
    //     productTemplates = []
    //     productTemplate = None
    //     buildYourOwn = None
    // }

    type Model = {
        Section: SharedShopV2.ShopSection
        Cart: CartItem list
        PayPalOrderReference: string option
        CheckoutTax: float option
        CheckoutShipping: float option
    }

    let getInitialModel shopSection = {
        Section = shopSection
        Cart = []
        PayPalOrderReference = None
        CheckoutTax = None
        CheckoutShipping = None
    }

module SharedPage =
    
    type CodeSection =
        | CodeLanding
        | GoalRoll
        | TileTap
        | TileSort

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
        | Shop of SharedShopV2.ShopSection
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


    // Represents which of the web app's subsections is to be displayed
    // Welcome -> not much to see here, a landing page with element to drive along user interaction
    // AboutSection -> Overview of the purpose of the web app, in this case some details about it's creator
    // Portfolio -> Split view landing page to separate categories from one another at a high level
    // Contact -> How to get in touch with the entity the web app represents
    type Model =
        | About of SharedAboutSection.Model
        | Contact
        | Help
        | Settings
        | Shop of SharedShop.Model
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
        | ShopMsg of SharedShop.ShopMsg
        | SwitchToOtherApp of SharedWebAppViewSections.AppView
        | ServicesMsg of SharedServices.Msg
        | LoadPage of SharedPage.Page
        | ErrorMsg of exn // WIP?
        | ChangeTheme of Theme

// Ensure that the Client and Server use same end-point
module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName
