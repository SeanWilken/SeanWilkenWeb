namespace Shared

open System

type Todo = { Id: Guid; Description: string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) = {
        Id = Guid.NewGuid()
        Description = description
    }

type ITodosApi = {
    getTodos: unit -> Async<Todo list>
    addTodo: Todo -> Async<Todo list>
}


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
    type AppView =
        | AboutAppView
        | ContactAppView
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

module SharedServicesSection =

    type OfferedService =
        | AIServices
        | APIIntegrations
        | AutomationServices
        | WebDevelopment
        | UIUXDesign
        
    type Msg =
        | ChangeServiceView of OfferedService

    type Model = {
        CurrentService: OfferedService
    }

module SharedShop =

    
    type ProductColor =
        | White
        | Black
        | Ash
        | Asphalt
        | Aqua
        | CharcoalGray
        | Gold
        | Maroon
        | Mustard
        | Navy
        | Red
        | Silver
        | AthleticHeather
        | BlackHeather
        | DarkGreyHeather
        | DeepHeather
        | HeatherDarkGray
        | HeatherOlive
        | HeatherBlue
        | HeatherNavy
        | DarkHeather
        | HeatherRaspberry
        | HeatherDust
        | HeatherDeepTeal
        | NoColor of string

    type ProductSize =
        | XS
        | S
        | XS_S
        | M
        | L
        | M_L
        | XL
        | XXL
        | XXXL
        | XXXXL
        | OSFA
        | NoSize of string

    type ProductOption =
        | ProductSize
        | ProductColor

    type CollectionTag =
        | Limited
        | Unlimited

    let collectionTagToString = function
        | Limited -> "limited"
        | Unlimited -> "unlimited"

    type ProductClass =
        | TShirts
        | LongSleeves
        | Sweaters
        | Hoodies
        | Hats
        | NoClass

    type CatalogProduct = {
        name: string
        id: int
        thumbnailURL: string
    }

    type ProductVariant = {
        name: string
        id: int
        price: string
    }

    type StateCode =
        | NY
        | CA
        | FL
        | TX
        | NO_STATE

    type CountryCode =
        | US
        | MEX
        | CAN
        | NO_COUNTRY

    type CustomerAddress = {
        firstName: string
        lastName: string
        streetAddress: string
        city: string
        stateCode: StateCode
        countryCode: CountryCode
        zipCode: int
    }

    type OrderType =
        | Order
        | Draft

    type Customer = {
        firstName: string
        lastName: string
        userName: string
        password: string
        savedShippingAddress: CustomerAddress option
        orders: (OrderType * int) list
    }

    type CustomerSignUpForm = {
        firstName: string
        lastName: string
        userName: string
        password: string
        confirmPassword: string
    }

    let defaultCustomerSignUpForm = {
        firstName = ""
        lastName = ""
        userName = ""
        password = ""
        confirmPassword = ""
    }

    type CustomerAddressForm = {
        firstName: string
        lastName: string
        streetAddress: string
        city: string
        stateCode: string
        countryCode: string
        zipCode: string
    }

    let defaultCustomerAddressForm = {
        firstName = ""
        lastName = ""
        streetAddress = ""
        city = ""
        stateCode = ""
        countryCode = ""
        zipCode = ""
    }

    type RequestResponse =
        | SuccessfulRequest of string
        | FailedRequest of string

    type SignUpRequestResponse =
        | SignUpSuccess of Customer
        | SignUpFailed of RequestResponse list

    type UserSignUpFormField =
        | FirstNameField of string
        | LastNameField of string
        | UserNameField of string
        | PasswordField of string
        | ConfirmPasswordField of string

    type CustomerAddressFormField =
        | ShippingFirstNameField of string
        | ShippingLastNameField of string
        | ShippingStreetAddressField of string
        | ShippingCityField of string
        | ShippingStateCodeField of string
        | ShippingCountryCodeField of string
        | ShippingZipCodeField of string

    type OrderItem = {
        externalVariantId : string
        itemQuantity : int
        itemRetailPrice : string
    }

    type OrderCosts = {
        orderSubTotal : string
        orderShipping : string
        orderTaxRate : string
        orderTax : string
        orderTotal : string
    }

    type CustomerDraftOrder = {
        recipient : CustomerAddressForm
        orderItems : OrderItem list
        orderCosts : OrderCosts
    }


    type SyncProductVariant = {
        externalSyncVariantId : string
        variantName : string
        variantSize : ProductSize
        variantColor : ProductColor
        variantPrice : float
        variantHeroImagePath : string
        variantAltImagePaths : string list
    }

    type SyncProduct = {
        name : string
        collectionTag : CollectionTag
        syncProductId : int
        syncProductHeroImagePath : string
        syncProductAltImagePaths : string list
        productVariations : SyncProductVariant list
    }

    type ProductCollection = {
        collectionName : string
        collectionTag : CollectionTag
        products : SyncProduct list
    }

    type SyncVariationLookup =
        | Successful of SyncProductVariant
        | Failed of string

    type ShopSection =
        | ShopLanding
        | Storefront
        | Catalog of string
        | Product of string * int
        | ShoppingBag
        | Checkout
        | Payment
        | Social
        | Contact
        | NotFound

        
    type QuantityAdjustment =
        | Increment
        | Decrement

    type DraftResult = { code: string }

    // Stub/placeholder types for API results
    // Replace with your real server shared types

    type CheckoutTax = { taxRequired: bool; taxRate: float }

    type CheckoutShippingRate = { shippingRate: string }

    type HttpError = string

    type ShopMsg =
        | NavigateTo of ShopSection
        | UpdateCustomerForm of UserSignUpFormField
        | UpdateAddressForm of CustomerAddressFormField
        | CheckUserSignUpForm
        | CheckAddressForm
        | UpdateProductColor of SyncProduct * ProductColor
        | UpdateProductSize of SyncProduct * ProductSize
        | AddVariantToShoppingBag of SyncProductVariant
        | DeleteVariantFromShoppingBag of SyncProductVariant
        | AdjustLineItemQuantity of QuantityAdjustment * SyncProductVariant
        | TestApiTaxRate
        | TestApiShippingRate
        | Send // apparently testing paypal
        | GotResult of Result<string, HttpError>
        | GotTaxRateResult of Result<CheckoutTax, HttpError>
        | GotShippingRateResult of Result<CheckoutShippingRate list, HttpError>
        | TestApiCustomerDraft of CustomerDraftOrder
        | GotCustomerOrderDraftResult of Result<DraftResult, HttpError>
        | GetAllProducts
        | GotAllProducts of Result<CatalogProduct list, HttpError>
        | GetProductVariants of int
        | GotProductVariants of Result<ProductVariant list, HttpError>

    type Model = {
        section: ShopSection
        customer : Customer option
        productVariationOptionsSelected : ( ProductColor option * ProductSize option )
        validSyncVariantId : String option
        // -- make this paypal order reference, only set by return of the GotDraftResults, if there is a value, can render the JS Paypal button
        payPalOrderReference :  String option 
        shoppingBag : List<SyncProductVariant * int>
        // -- ( Variant Line Item, Qty )
        checkoutTaxShipping : float option * float option 
        // -- Tax, Shipping 
        //    HOME PAGE
        homeGif : String
        //    SIGN UP
        customerSignUpForm : CustomerSignUpForm 
        customerAddressForm : CustomerAddressForm 
        validationResults : List<RequestResponse>
        allProducts : List<CatalogProduct> option
        productVariants : List<ProductVariant> option
    }

    let getInitialModel shopSection = {
        section = shopSection
        customer = None
        productVariationOptionsSelected = (None, None)
        validSyncVariantId = None
        payPalOrderReference = None
        shoppingBag = []
        checkoutTaxShipping = (None, None)
        homeGif = ""
        customerSignUpForm = defaultCustomerSignUpForm
        customerAddressForm = defaultCustomerAddressForm
        validationResults = []
        allProducts = None
        productVariants = None
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
        | Index
        | Landing
        | Portfolio of PortfolioSection
        | Resume
        | Shop of SharedShop.ShopSection
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
        | Services of SharedServicesSection.Model
        | Landing
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
        | LoadPage of SharedPage.Page
        | ErrorMsg of exn // WIP?
        | ChangeTheme of Theme

// Ensure that the Client and Server use same end-point
module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type IPageApi =
    { GetPage :  string -> Async<SharedWebAppModels.Model> }