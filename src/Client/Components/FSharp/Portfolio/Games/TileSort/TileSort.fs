module Components.FSharp.Portfolio.Games.TileSort

open Elmish
open Client.Domain
open SharedTileSort
open GridGame

// CURRENTLY BROKEN WHEN USING SHARED, AS I TRIED TO UPDATE TO USE GRIDGAME AND BROKE STUFF
// TODO:
    //IMPLEMENT:
        //Move Count Display
        //Round Timer
        // Unsolvable Puzzles can't be generated..check inversions...https://www.geeksforgeeks.org/check-instance-8-puzzle-solvable/


// DIFFICULTY HELPERS
// GET CASES FROM DESCRIMINATED UNION
open FSharp.Reflection
let tileDifficulties = FSharpType.GetUnionCases typeof<TileSortDifficulty>
let decodeDifficultyByString string =
    match string with
    | "Simple" -> Simple
    | "Easy" -> Easy
    | "Medium" -> Medium
    | "Hard" -> Hard
    | _ -> Simple

let difficultyToString difficulty =
    match difficulty with
    | Simple -> "Simple - 3x3"
    | Easy -> "Easy - 4x4"
    | Medium -> "Medium - 5x5"
    | Hard -> "Hard - 6x6"

// VIEW
let tileSortDescriptions = [ 
    "- Rearrange the tiles in correct ascending order, starting @ the top left position."
    "- Select one of the tiles adjacent to the empty space to slide that tile into the blank."
    "- The blank space must match the missing number." 
]

let controlList = [ 
    "Play", (SetGameState (Playing))
    "Settings", (SetGameState (Settings)) 
]

let gameControls = [
    "New Round", NewRound
    "Reset Round", ResetRound
    "Undo Move", RewindMove
    "3 x 3", UpdateDifficulty Simple
    "4 x 4", UpdateDifficulty Easy
    "5 x 5", UpdateDifficulty Medium
    "6 x 6", UpdateDifficulty Hard
]

let modelTileSortRoundDetails ( model : SharedTileSort.Model ) = [
    "You completed " + difficultyToString model.Difficulty + " difficulty."
    "It took you " + string model.Turns.Length + " number of moves."
]

//---------------------

let init (): SharedTileSort.Model * Cmd<Msg> =
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
            let gridAfterTileMove = selectedCanSwapWithBlank model.CurrentTiles ( TileSortLaneObject gameTile ) ( getGridDimension model.Difficulty )
            { model with CurrentTiles = gridAfterTileMove}, Cmd.ofMsg CheckSolution
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

open Fable.React
open Feliz
let laneObjectToTileSortTile tile dispatch =
    let (tileContent, onClickHandler, cssClass) =
        match tile with
        | TileSortLaneObject tileValue ->
            let displayValue = convertValueToProperString (getValueOrZeroFromGameTile tile)
            displayValue,
            (fun _ -> MoveTile tileValue |> dispatch),
            "valueTile bg-blue-800 hover:bg-blue-600 text-white font-bold text-xl w-16 h-16 flex items-center justify-center rounded shadow"
        | _ ->
            "",
            (fun _ -> MoveTile { Value = None } |> dispatch),
            "emptyTile bg-gray-700 w-16 h-16 rounded shadow"

    Html.div [
        prop.className cssClass
        prop.onClick onClickHandler
        prop.children [ Html.text tileContent ]
    ]

let tileSortRowCreator (tileRow: LaneObject list) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "flex justify-center gap-2 my-2"
        prop.children [
            for tile in tileRow -> laneObjectToTileSortTile tile dispatch
        ]
    ]

let tileSortGameBoard model dispatch =
    let tileRows = getPositionsAsRows model.CurrentTiles (getGridDimension model.Difficulty)
    Html.div [
        prop.className "flex flex-col items-center gap-2"
        prop.children [ for row in tileRows -> tileSortRowCreator row dispatch ]
    ]


let tileSortGameLoopCard (model: SharedTileSort.Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "card bg-base-200 shadow-md p-4 mt-4"
        prop.children [
            Html.div [
                // prop.className "flex flex-col space-y-6"
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
                                            Html.button [ 
                                                prop.className "btn btn-sm btn-secondary w-full"
                                                prop.onClick (fun _ -> ResetRound |> dispatch)
                                                prop.text "Restart Round"
                                            ]
                                        ]
                                    ]
                                    Html.div [
                                        prop.className "flex flex-col items-center justify-center gap-2 flex-1"
                                        prop.children [
                                            Html.h3 [ prop.className "mb-1 text-center"; prop.text "Select Level:" ]
                                            Html.div [
                                                prop.className "flex flex-row gap-2 justify-center"
                                                prop.children [
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.Difficulty = SharedTileSort.TileSortDifficulty.Simple then " text-primary" else ""))
                                                        prop.onClick (fun _ -> Msg.UpdateDifficulty SharedTileSort.TileSortDifficulty.Simple |> dispatch)
                                                        prop.text "Simple"
                                                    ]
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.Difficulty = SharedTileSort.TileSortDifficulty.Easy then " text-primary" else ""))
                                                        prop.onClick (fun _ -> Msg.UpdateDifficulty SharedTileSort.TileSortDifficulty.Easy |> dispatch)
                                                        prop.text "Easy"
                                                    ]
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.Difficulty = SharedTileSort.TileSortDifficulty.Medium then " text-primary" else ""))
                                                        prop.onClick (fun _ -> Msg.UpdateDifficulty SharedTileSort.TileSortDifficulty.Medium |> dispatch)
                                                        prop.text "Medium"
                                                    ]
                                                    Html.a [
                                                        prop.className ("btn btn-xs btn-outline" + (if model.Difficulty = SharedTileSort.TileSortDifficulty.Hard then " text-primary" else ""))
                                                        prop.onClick (fun _ -> Msg.UpdateDifficulty SharedTileSort.TileSortDifficulty.Hard |> dispatch)
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

let tileSortModalContent model dispatch =
    SharedViewModule.gameModalContent (
        Html.div [
            match model.GameState with
            | Won ->
                SharedViewModule.roundCompleteContent (modelTileSortRoundDetails model) (fun () -> SharedTileSort.Msg.ResetRound |> dispatch)
            | _ ->
                Html.div [
                    prop.className "flex flex-row justify-between items-center w-full mb-4 gap-4 relative p-2"
                    prop.children [
                        Html.div [
                            prop.className "card bg-base-200 shadow-lg p-4 flex-1 border-2 border-success-content"
                            prop.children [
                                Html.div [
                                    prop.className "text-success font-bold text-lg text-center"
                                    prop.text $"Difficulty: {model.Difficulty |> difficultyToString}"
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "card bg-base-200 shadow-lg p-4 flex-1 border-2 border-warning-content"
                            prop.children [
                                Html.div [
                                    prop.className "text-info font-bold text-lg text-center"
                                    prop.text $"Total Moves: {model.Turns.Length}"
                                ]
                            ]
                        ]
                    ]
                ]
                Html.div [
                    prop.className "my-4"
                    prop.children [ tileSortGameBoard model dispatch ]
                ]
                tileSortGameLoopCard model dispatch
            
        ]
            
    )
    

let view model dispatch =
    Html.div [
        SharedViewModule.sharedModalHeader "Tile Sort" tileSortDescriptions QuitGame dispatch
        tileSortModalContent model dispatch
    ]
