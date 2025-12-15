module Components.FSharp.Portfolio.Games.TileSort

open Elmish
open Client.Domain
open SharedTileSort
open GridGame
open Feliz
open SharedViewModule.SharedMicroGames
open Bindings.LucideIcon

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

[<ReactComponent>]
let view (model: SharedTileSort.Model) (dispatch: SharedTileSort.Msg -> unit) (quitMsg: 'parentMsg) (dispatchParent: 'parentMsg -> unit) =

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
