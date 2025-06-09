module CodeGallery

open Elmish
open Feliz
open Shared
open SharedCodeGallery


let init(): SharedCodeGallery.Model * Cmd<SharedCodeGallery.Msg> =
    SharedCodeGallery.CodeGallery, Cmd.none

let update (msg: SharedCodeGallery.Msg) (model: SharedCodeGallery.Model): SharedCodeGallery.Model * Cmd<SharedCodeGallery.Msg> =
    match msg, model with
    | LoadSection Gallery, _ ->
        SharedCodeGallery.CodeGallery, Cmd.none
    | LoadSection GallerySection.GoalRoll, _ ->
        let m, cmd = GoalRoll.init()
        GoalRoll m, Cmd.map GoalRollMsg cmd
    | GoalRollMsg SharedGoalRoll.Msg.QuitGame, GoalRoll _ ->
        CodeGallery, Cmd.none
    | GoalRollMsg msg, GoalRoll m ->
        let m', cmd = GoalRoll.update msg m
        GoalRoll m', Cmd.map GoalRollMsg cmd

    | LoadSection GallerySection.TileSort, _ ->
        let m, cmd = TileSort.init()
        TileSort m, Cmd.map TileSortMsg cmd
    | TileSortMsg SharedTileSort.Msg.QuitGame, TileSort _ ->
        SharedCodeGallery.CodeGallery, Cmd.none
    | TileSortMsg msg, TileSort m ->
        let m', cmd = TileSort.update msg m
        TileSort m', Cmd.map TileSortMsg cmd

    | SharedCodeGallery.LoadSection GallerySection.TileTap, _ ->
        let m, cmd = TileTap.init()
        TileTap m, Cmd.map TileTapMsg cmd
    | SharedCodeGallery.TileTapMsg SharedTileTap.Msg.QuitGame, TileTap _ ->
        TileTap.update SharedTileTap.Msg.ExitGameLoop |> ignore
        SharedCodeGallery.CodeGallery, Cmd.none
    | SharedCodeGallery.TileTapMsg msg, TileTap m ->
        let m', cmd = TileTap.update msg m
        TileTap m', Cmd.map TileTapMsg cmd

    | SharedCodeGallery.LoadSection GallerySection.PivotPoint, _ ->
        printfn $"SHOULD LOAD PIVOT POINT"
        let m, cmd = PivotPoints.init()
        PivotPoint m, Cmd.map PivotPointMsg cmd
    | SharedCodeGallery.PivotPointMsg SharedPivotPoint.Msg.QuitGame, PivotPoint m ->
        PivotPoints.update SharedPivotPoint.Msg.ExitGameLoop |> ignore
        SharedCodeGallery.CodeGallery, Cmd.none
    | SharedCodeGallery.PivotPointMsg msg, PivotPoint m ->
        let m', cmd = PivotPoints.update msg m
        PivotPoint m', Cmd.map PivotPointMsg cmd

    | _, _ -> model, Cmd.none


let codeGalleryHeader dispatch =
    Html.div [
        prop.className "text-center mb-6"
        prop.children [
            Html.h1 [ prop.className "text-4xl font-bold"; prop.text "Code Gallery" ]
            Html.p [
                prop.className "text-lg text-base-content mt-2"
                prop.text "Select a game module to try out, and configure your round in settings."
            ]
        ]
    ]

let codeGalleryCard (title: string) (description: string) msg dispatch =
    Html.div [
        prop.className "card bg-base-100 shadow-md hover:shadow-xl transition w-full md:w-2/3 mx-auto mb-4"
        prop.children [
            Html.div [
                prop.className "card-body"
                prop.children [
                    Html.h2 [ prop.className "card-title"; prop.text title ]
                    Html.p [ prop.text description ]
                    Html.div [
                        prop.className "card-actions justify-end"
                        prop.children [
                            Html.button [
                                prop.className "btn btn-primary"
                                prop.text "Play"
                                prop.onClick (fun _ -> dispatch msg)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]


let tileSortSelection dispatch =
    codeGalleryCard 
        "Tile Sort"
        "Arrange the tiles in the correct order, with the empty space being the missing number."
        (SharedCodeGallery.LoadSection SharedCodeGallery.TileSort)
        dispatch

let tileTapSelection dispatch =
    codeGalleryCard 
        "Tile Tap"
        "Tap to smash as many tiles as you can while avoiding bombs."
        (SharedCodeGallery.LoadSection SharedCodeGallery.TileTap)
        dispatch

let goalRollSelection dispatch =
    codeGalleryCard 
        "Goal Roll"
        "Roll the ball in straight line movements to the goal."
        (SharedCodeGallery.LoadSection SharedCodeGallery.GoalRoll)
        dispatch

let pivotPointsSelection dispatch =
    codeGalleryCard 
        "Pivot Points"
        "Pivot the ball across lanes to collect coins."
        (SharedCodeGallery.LoadSection SharedCodeGallery.PivotPoint)
        dispatch


let view model dispatch =
    match model with
    | CodeGallery ->
        Html.div [
            prop.className "max-w-4xl mx-auto px-4 py-8"
            prop.children [
                SharedViewModule.galleryHeaderControls "" "" BackToPortfolio dispatch
                codeGalleryHeader dispatch
                codeGalleryCard "Goal Roll" "Roll the ball in straight lines to the goal." (LoadSection SharedCodeGallery.GoalRoll) dispatch
                codeGalleryCard "Tile Sort" "Arrange the tiles in the correct order." (LoadSection SharedCodeGallery.TileSort) dispatch
                codeGalleryCard "Tile Tap" "Tap to smash tiles while avoiding bombs." (LoadSection SharedCodeGallery.TileTap) dispatch
                codeGalleryCard "Pivot Points" "Pivot the ball to collect coins across lanes." (LoadSection SharedCodeGallery.PivotPoint) dispatch
            ]
        ]
    | GoalRoll m -> GoalRoll.view m (GoalRollMsg >> dispatch)
    | TileSort m -> TileSort.view m (TileSortMsg >> dispatch)
    | TileTap m  -> TileTap.view m (TileTapMsg >> dispatch)
    | PivotPoint m -> PivotPoints.view m (PivotPointMsg >> dispatch)
