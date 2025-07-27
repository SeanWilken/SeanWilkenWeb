module CodeGallery

open Elmish
open Feliz
open Shared
open SharedCodeGallery

let init(): Model * Cmd<Msg> = CodeGallery, Cmd.none

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg, model with
    | LoadSection Gallery, _ -> CodeGallery, Cmd.none
    | LoadSection GallerySection.GoalRoll, _ ->
        let m, cmd = GoalRoll.init()
        GoalRoll m, Cmd.map GoalRollMsg cmd
    | GoalRollMsg SharedGoalRoll.Msg.QuitGame, GoalRoll _ -> CodeGallery, Cmd.none
    | GoalRollMsg msg, GoalRoll m ->
        let m', cmd = GoalRoll.update msg m
        GoalRoll m', Cmd.map GoalRollMsg cmd

    | LoadSection GallerySection.TileSort, _ ->
        let m, cmd = TileSort.init()
        TileSort m, Cmd.map TileSortMsg cmd
    | TileSortMsg SharedTileSort.Msg.QuitGame, TileSort _ -> CodeGallery, Cmd.none
    | TileSortMsg msg, TileSort m ->
        let m', cmd = TileSort.update msg m
        TileSort m', Cmd.map TileSortMsg cmd

    | LoadSection GallerySection.TileTap, _ ->
        let m, cmd = TileTap.init()
        TileTap m, Cmd.map TileTapMsg cmd
    | TileTapMsg SharedTileTap.Msg.QuitGame, TileTap _ ->
        TileTap.update SharedTileTap.Msg.ExitGameLoop |> ignore
        CodeGallery, Cmd.none
    | TileTapMsg msg, TileTap m ->
        let m', cmd = TileTap.update msg m
        TileTap m', Cmd.map TileTapMsg cmd

    | LoadSection GallerySection.PivotPoint, _ ->
        let m, cmd = PivotPoints.init()
        PivotPoint m, Cmd.map PivotPointMsg cmd
    | PivotPointMsg SharedPivotPoint.Msg.QuitGame, PivotPoint _ ->
        PivotPoints.update SharedPivotPoint.Msg.ExitGameLoop |> ignore
        CodeGallery, Cmd.none
    | PivotPointMsg msg, PivotPoint m ->
        let m', cmd = PivotPoints.update msg m
        PivotPoint m', Cmd.map PivotPointMsg cmd

    | _ -> model, Cmd.none


// HEADER
let codeGalleryHeader =
    Html.div [
        prop.className "text-center mb-12"
        prop.children [
            Html.h1 [
                prop.className "text-5xl font-bold text-primary"
                prop.text "Code Experiments"
            ]
            Html.p [
                prop.className "text-lg text-base-content mt-2"
                prop.text "Choose an interactive code demo. Each game highlights a different UI or logic challenge."
            ]
        ]
    ]


// CARD
let codeGalleryCard (title: string) (description: string) msg dispatch =
    Html.div [
        prop.className "card bg-base-200 hover:bg-base-300 shadow-xl transition duration-300 ease-in-out hover:scale-[1.01]"
        prop.children [
            Html.div [
                prop.className "card-body space-y-3"
                prop.children [
                    Html.h2 [ prop.className "card-title text-2xl text-primary"; prop.text title ]
                    Html.p [ prop.className "text-base-content"; prop.text description ]
                    Html.div [
                        prop.className "card-actions justify-end"
                        prop.children [
                            Html.button [
                                prop.className "btn btn-sm btn-primary"
                                prop.text "Launch"
                                prop.onClick (fun _ -> dispatch msg)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]


// MAIN VIEW
let view model dispatch =
    match model with
    | CodeGallery ->
        Html.div [
            prop.className "max-w-6xl mx-auto px-6 py-12"
            prop.children [
                SharedViewModule.galleryHeaderControls {
                    onClose = fun () -> BackToPortfolio |> dispatch
                    rightIcon = Some (
                        {
                            icon = Bindings.LucideIcon.LucideIcon.Github "w-6 h-6"
                            label = "Github"
                            externalLink = Some "https://github.com/seanwilken"
                            externalAlt = Some "Go to Github"
                        }
                    )
                }
                codeGalleryHeader
                Html.div [
                    prop.className "grid gap-8 sm:grid-cols-1 md:grid-cols-2"
                    prop.children [
                        codeGalleryCard "Goal Roll" "Roll the ball in straight lines to the goal." (LoadSection SharedCodeGallery.GoalRoll) dispatch
                        codeGalleryCard "Tile Sort" "Arrange the tiles in the correct order." (LoadSection SharedCodeGallery.TileSort) dispatch
                        codeGalleryCard "Tile Tap" "Tap to smash tiles while avoiding bombs." (LoadSection SharedCodeGallery.TileTap) dispatch
                        codeGalleryCard "Pivot Points" "Pivot the ball to collect coins across lanes." (LoadSection SharedCodeGallery.PivotPoint) dispatch
                    ]
                ]
            ]
        ]

    | GoalRoll m -> GoalRoll.view m (GoalRollMsg >> dispatch)
    | TileSort m -> TileSort.view m (TileSortMsg >> dispatch)
    | TileTap m  -> TileTap.view m (TileTapMsg >> dispatch)
    | PivotPoint m -> PivotPoints.view m (PivotPointMsg >> dispatch)