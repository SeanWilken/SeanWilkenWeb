module Components.FSharp.Portfolio.Games.TileSort

open System
open Elmish
open Client.GameDomain.GridGame
open Feliz
open SharedViewModule.SharedMicroGames
open Bindings.LucideIcon


// Determines the grid dimensions
type TileSortDifficulty =
    | Simple // 3x3
    | Easy // 4x4
    | Medium // 5x5
    | Hard // 6x6

type Msg =
    | SetGameState of RoundState
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
    GameState: RoundState // determines whether the puzzle has been solved or in progress
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
    let gameTile = TileSortLaneObject { Value = Some randomTileValue }
    // recurse back through if there are remaining positions
    match remain with
    | [] -> { GridPositions = gameTile :: assignedTiles.GridPositions }
    | y::ys -> createRandomGameTiles { GridPositions = gameTile :: assignedTiles.GridPositions } remain

// Step 3: Randomly blank a tile value to None, to act as the void space for sliding a tile.
// remove one entry randomly to get the blank starting position
let randomBlankPosition (assignedTiles: GridBoard) =
    let selectedTile = selectRandomTilePosition assignedTiles
    List.map (fun x -> if x = selectedTile then TileSortLaneObject { Value = None } else x ) assignedTiles.GridPositions

// STEP 4: ASSIGN GENERATED TILE ROUND TO INITIAL AND CURRENT
let createNewRoundGameBoardBasedOnDifficulty difficulty =
    let newRoundTilePositions = generateGameBoardPositionsBasedOffDifficulty difficulty
    let randomizedNewRoundTilePositions = createRandomGameTiles unassignedTiles newRoundTilePositions
    {GridPositions = randomBlankPosition randomizedNewRoundTilePositions}

//---------------------
let getValueOrZeroFromGameTile (gameTile) =
    match gameTile with
    | TileSortLaneObject i ->
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
        then tiles, false
        else
            let colSwap = checkSwapInColumn selectedIndex blankIndex tiles selected gridDimension
            let rowSwap = checkSwapInRow selectedIndex blankIndex tiles selected gridDimension
            if (colSwap || rowSwap)
                then swapBlankWithSelected tiles selected selectedIndex blankIndex, true
                else tiles, false



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
    let filteredByMissing = List.filter (fun x -> x = TileSortLaneObject { Value = missingValue }) currentTiles.GridPositions
    List.isEmpty filteredByMissing


// simplify calls of calculating and checking Blanks calculated value
let checkBlankTileIsAtCorrectPosition currentTiles =
    calculatedBlankValueNotFoundInTiles currentTiles

//---------------------
// VALUE POSITION VALIDATION
// gameboard list is the same as the list if sorted by value
// need to filter out None
let checkTilesInCorrectOrder (currentTiles: GridBoard) =
    (List.filter (fun x -> x <> TileSortLaneObject { Value = None }) currentTiles.GridPositions) = (List.filter (fun x -> x <> TileSortLaneObject { Value = None }) currentTiles.GridPositions |> List.sortBy (fun x -> Some x))


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

//---------------------

let init (): Model * Cmd<Msg> =
    let initialDifficulty = Simple
    let initialRound = createNewRoundGameBoardBasedOnDifficulty initialDifficulty
    let model = {
        Difficulty = initialDifficulty
        CurrentTiles = initialRound
        InitialTiles = initialRound
        Turns = []
        GameState = Playing
    }
    model, Cmd.none
//---------------------
let update ( msg: Msg ) ( model: Model ): Model * Cmd<Msg> =
    match msg with
    | SetGameState gameState ->
        { model with GameState = gameState }, Cmd.none
    | NewRound ->
        let newRound = createNewRound model
        newRound, Cmd.none
    | ResetRound ->
        resetRound model, Cmd.none
    | MoveTile gameTile ->
        match gameTile.Value with
        | Some i ->
            let gridAfterTileMove, moveMade = selectedCanSwapWithBlank model.CurrentTiles ( TileSortLaneObject gameTile ) ( getGridDimension model.Difficulty )
            if moveMade
            then
                { model with 
                    CurrentTiles = gridAfterTileMove 
                    Turns = [ i ] @ model.Turns
                }, Cmd.ofMsg CheckSolution
            else
                model, Cmd.none
        | None ->
            model, Cmd.none
    | RewindMove ->
        rewindCurrentTiles model, Cmd.none
    | UpdateDifficulty difficulty ->
        let newDiff = changeDifficulty difficulty model
        let newRound = createNewRound newDiff
        newRound, Cmd.none
    | CheckSolution ->
        match winValidator model.CurrentTiles with
        | true -> 
            { model with GameState = Won }, Cmd.none
        | false -> model, Cmd.none
    | Solved -> { model with GameState = Won }, Cmd.none // Was used as a test state for button
    | QuitGame -> model, Cmd.none




let private tileSortTileBase (sizePx: int) : IStyleAttribute list =
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

let private tileSortEmptyTile (sizePx: int) =
    tileSortTileBase sizePx @ [
        style.custom("background", "rgba(10, 20, 40, 0.55)")
        style.custom("border", "1px solid rgba(255,255,255,0.10)")
        style.custom("box-shadow", "inset 0 0 10px rgba(0,0,0,0.55)")
    ]

let private tileSortValueTile (sizePx: int) (isClickable: bool) =
    tileSortTileBase sizePx @ [
        style.custom("background", "linear-gradient(135deg, rgba(0,255,255,0.22), rgba(0,139,139,0.20))")
        style.custom("border", "1px solid rgba(0,255,255,0.55)")
        style.custom("box-shadow", "inset 0 0 18px rgba(0,255,255,0.10), 0 0 14px rgba(0,255,255,0.18)")
        style.custom("cursor", if isClickable then "pointer" else "default")
        style.custom("filter", if isClickable then "brightness(1.02)" else "none")
    ]





// let laneObjectToTileSortTile tile dispatch =
//     let (tileContent, onClickHandler, cssClass) =
//         match tile with
//         | TileSortLaneObject tileValue ->
//             let displayValue = convertValueToProperString (getValueOrZeroFromGameTile tile)
//             displayValue,
//             (fun _ -> MoveTile tileValue |> dispatch),
//             "valueTile bg-blue-800 hover:bg-blue-600 text-white font-bold text-xl w-16 h-16 flex items-center justify-center rounded shadow"
//         | _ ->
//             "",
//             (fun _ -> MoveTile { Value = None } |> dispatch),
//             "emptyTile bg-gray-700 w-16 h-16 rounded shadow"

//     Html.div [
//         prop.className cssClass
//         prop.onClick onClickHandler
//         prop.children [ Html.text tileContent ]
//     ]


let laneObjectToTileSortTile tile dispatch =
    // tweak per difficulty if you want (e.g., small on harder)
    let sizePx = 64

    match tile with
    | TileSortLaneObject tileValue ->
        let displayValue = convertValueToProperString (getValueOrZeroFromGameTile tile)

        Html.button [
            prop.type'.button
            prop.className "tile select-none"
            prop.style (
                tileSortValueTile sizePx true @ [
                    // Typography (use custom text-shadow)
                    style.custom("color", "#cfffff")
                    style.custom("font-weight", "800")
                    style.custom("font-size", "22px")
                    style.custom("letter-spacing", "1px")
                    style.custom("text-shadow", "0 0 12px rgba(0,255,255,0.45)")
                    // hover/active without relying on Tailwind arbitrary classes
                    style.custom("transition", "transform 0.15s ease, filter 0.15s ease")
                ]
            )
            prop.onClick (fun _ -> MoveTile tileValue |> dispatch)
            prop.onMouseEnter (fun _ -> ()) // placeholder if you want preview logic later
            prop.children [ Html.text displayValue ]
        ]

    | _ ->
        Html.div [
            prop.className "tile"
            prop.style (tileSortEmptyTile sizePx)
        ]



// let tileSortRowCreator (tileRow: LaneObject list) (dispatch: Msg -> unit) =
//     Html.div [
//         prop.className "flex justify-center gap-2 my-2"
//         prop.children [
//             for tile in tileRow -> laneObjectToTileSortTile tile dispatch
//         ]
//     ]

// let tileSortGameBoard model dispatch =
//     let tileRows = getPositionsAsRows model.CurrentTiles (getGridDimension model.Difficulty)
//     Html.div [
//         prop.className "flex flex-col items-center gap-2"
//         prop.children [ for row in tileRows -> tileSortRowCreator row dispatch ]
//     ]

let tileSortRowCreator (tileRow: LaneObject list) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "flex justify-center gap-1 my-1"
        prop.children [ for tile in tileRow -> laneObjectToTileSortTile tile dispatch ]
    ]

let tileSortGameBoard model dispatch =
    let tileRows = getPositionsAsRows model.CurrentTiles (getGridDimension model.Difficulty)

    Html.div [
        prop.className "flex flex-col items-center"
        prop.children [ for row in tileRows -> tileSortRowCreator row dispatch ]
    ]
    
let difficulties = [ Simple; Easy; Medium; Hard ]

let tileSortDescriptions = [ 
    "- Rearrange the tiles in correct ascending order, starting @ the top left position."
    "- Select one of the tiles adjacent to the empty space to slide that tile into the blank."
    "- The blank space must match the missing number." 
]

[<ReactComponent>]
let view (model: Model) (dispatch: Msg -> unit) (quitMsg: 'parentMsg) (dispatchParent: 'parentMsg -> unit) =

    let overlay : ReactElement option =
        match model.GameState with
        | Won ->
            Some (
                WinOverlay model.Turns.Length
                    (fun () -> dispatch ResetRound)
                    (Some (fun () -> dispatch ResetRound))
            )
        | _ -> None

    CyberShellResponsive {
        Left = 
            Html.div 
                [
                    TitlePanel "TILE SORT"
                    LevelSelectPanel
                        difficulties
                        model.Difficulty
                        (fun a b -> a = b)
                        (fun diff -> dispatch (UpdateDifficulty diff))
                    InstructionsPanel 
                        "HOW TO PLAY" 
                        tileSortDescriptions
                        ""
                        (fun () -> ())
                    ControlsPanel [
                        ControlButton "UNDO" Purple (model.Turns.Length > 0) (fun () -> dispatch RewindMove) (Some (LucideIcon.RotateCcw "w-4 h-4"))
                        ControlButton "RESTART" Red true (fun () -> dispatch ResetRound) None
                    ]
                ]
        Board =  
            BoardPanel ( tileSortGameBoard model dispatch )
        Overlay = overlay
        OnQuit = (fun () -> dispatchParent quitMsg)
    }
