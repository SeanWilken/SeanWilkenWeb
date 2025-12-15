module Components.FSharp.Portfolio.CodeGallery

open Elmish
open Feliz
open Client.Domain
open SharedCodeGallery
open Fable.Core.JsInterop
open Components.FSharp.Portfolio.Games
open Components.FSharp.Interop.TSXCanvas
open Bindings.LucideIcon

let getSourceCode gallerySection : string = 
    match gallerySection with
    | GallerySection.TileSort -> importDefault "./TileSort/TileSort.fs?raw"
    | GallerySection.TileTap -> importDefault "./TileTap/TileTap.fs?raw"
    | GallerySection.GoalRoll -> importDefault "./GoalRoll/GoalRoll.fs?raw"
    | GallerySection.PivotPoint -> importDefault "./PivotPoints/PivotPoints.fs?raw"
    | _ -> ""

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

    | LoadSourceCode gallerySection, _ ->
        // let m, cmd = PivotPoints.init()
        SourceCode gallerySection, Cmd.none

    | _ -> model, Cmd.none


// HEADER
let codeGalleryHeader =
    Html.div [
        prop.className "text-center space-y-2"
        prop.children [
            Html.h1 [
                prop.className "text-4xl font-bold text-primary"
                prop.text "Code Experiments"
                // prop.text "Design Gallery"
            ]
            Html.p [
                prop.className "text-sm text-base-content/70 max-w-xl mx-auto"
                prop.text "Play with interactive demos built in F#. Each experiment explores a different UI, game loop, or logic challenge."
            ]
        ]
    ]

// tiny helper to pick an icon + tag per game
let private gameMeta title =
    match title with
    | "Goal Roll" ->
        LucideIcon.Target "w-4 h-4", "Physics"
    | "Tile Sort" ->
        LucideIcon.PanelLeft "w-4 h-4", "Logic"
    | "Tile Tap" ->
        LucideIcon.MousePointerClick "w-4 h-4", "Reflex"
    | "Pivot Points" ->
        LucideIcon.GitBranch "w-4 h-4", "Pathing"
    | _ ->
        LucideIcon.Gamepad2 "w-4 h-4", "Experiment"

let codeGalleryCard (title: string) (description: string) gallerySection dispatch =
    let icon, tag = gameMeta title

    Html.div [
        prop.className
            "group card bg-base-100/90 border border-base-200/80 rounded-2xl shadow-md hover:shadow-xl hover:-translate-y-[3px] hover:border-primary/50 transition duration-200 ease-out overflow-hidden"
        prop.children [

            // gradient strip
            Html.div [
                prop.className
                    "h-1.5 w-full rounded-t-2xl bg-gradient-to-r from-primary via-secondary to-accent"
            ]

            Html.div [
                prop.className "card-body space-y-4"
                prop.children [

                    // title row
                    Html.div [
                        prop.className "flex items-center gap-2"
                        prop.children [
                            Html.div [
                                prop.className
                                    "inline-flex items-center justify-center w-8 h-8 rounded-full bg-primary/10 text-primary"
                                prop.children [ icon ]
                            ]
                            Html.div [
                                prop.children [
                                    Html.h2 [
                                        prop.className "card-title text-base sm:text-lg text-primary"
                                        prop.text title
                                    ]
                                    Html.span [
                                        prop.className
                                            "mt-0.5 inline-flex items-center gap-1 text-[11px] px-2 py-0.5 rounded-full bg-base-200/80 text-base-content/60 uppercase tracking-wide"
                                        prop.text tag
                                    ]
                                ]
                            ]
                        ]
                    ]


                    // description
                    Html.p [
                        prop.className "text-sm text-base-content/80"
                        prop.text description
                    ]

                    // footer buttons
                    Html.div [
                        prop.className "card-actions justify-end gap-2 pt-2"
                        prop.children [
                            Html.button [
                                prop.className
                                    "btn btn-xs sm:btn-sm btn-outline group-hover:btn-secondary/90"
                                prop.text "Source code"
                                prop.onClick (fun _ -> LoadSourceCode gallerySection |> dispatch)
                            ]
                            Html.button [
                                prop.className
                                    "btn btn-xs sm:btn-sm btn-primary gap-1 group-hover:translate-y-[-0.5px]"
                                prop.onClick (fun _ -> dispatch (LoadSection gallerySection))
                                prop.children [
                                    Html.span [ prop.text "Launch demo" ]
                                    LucideIcon.Play "w-3 h-3"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]



type Props = {|
    Section: GallerySection
    OnBack: unit -> unit
|}


let SourceViewer = React.functionComponent(fun (props: Props) ->
    Html.div [
        prop.className "max-w-6xl mx-auto p-4 space-y-4"

        // Header with back button
        prop.children [
            Html.div [
                prop.className "flex items-center justify-between"
                prop.children [
                    Html.button [
                        prop.className "btn btn-primary btn-sm"
                        prop.onClick (fun _ -> props.OnBack())
                        prop.text "Back to gallery"
                    ]
                    Html.h2 [
                        prop.className "text-xl font-bold"
                        prop.text (sprintf "Source: %s.fs" (props.Section.ToString()))
                    ]
                ]
            ]

            // Code block
            Html.div [
                prop.className "overflow-auto rounded-lg bg-base-200 p-4"
                prop.children [
                    Html.pre [
                        prop.className "text-sm font-mono whitespace-pre"
                        prop.children [
                            Html.code [
                                prop.text (getSourceCode props.Section)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
)

let view model dispatch =
    match model with
    | CodeGallery ->
        Html.section [
            // prop.className "max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-12 space-y-8"
            prop.className
                "relative max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-12 space-y-8 bg-gradient-to-b from-primary/5 via-transparent to-transparent"
            prop.children [
                SharedViewModule.galleryHeaderControls {
                    onClose = fun () -> BackToPortfolio |> dispatch
                    rightIcon = Some {
                        icon = LucideIcon.Github "w-5 h-5"
                        label = "GitHub"
                        externalLink = Some "https://github.com/seanwilken"
                        externalAlt = Some "Go to GitHub"
                    }
                }

                // hero
                Html.div [
                    prop.className "text-center space-y-2"
                    prop.children [
                        Html.h1 [
                            prop.className "text-4xl sm:text-5xl font-bold text-primary"
                            prop.text "Code Experiments"
                        ]
                        Html.p [
                            prop.className "text-sm sm:text-base text-base-content/80 max-w-2xl mx-auto"
                            prop.text
                                "Play with interactive demos built in F#. Each experiment explores a different UI, game loop, or logic challenge."
                        ]
                        Html.p [
                            prop.className "text-[11px] text-base-content/60"
                            prop.text "Built for fun in F# and TypeScript."
                        ]
                    ]
                ]

                // panel around the cards
                Html.div [
                    prop.className
                        "rounded-3xl border border-base-200/70 bg-base-100/80 shadow-sm px-4 sm:px-6 py-6 sm:py-8"
                    prop.children [
                        Html.div [
                            prop.className "flex flex-wrap items-center justify-between gap-2 text-[11px] px-4 pb-4"
                            prop.children [
                                Html.div [
                                    prop.className "inline-flex items-center gap-1 text-base-content/70"
                                    prop.children [
                                        LucideIcon.Gamepad2 "w-3 h-3"
                                        Html.span [ prop.text "4 playable experiments" ]
                                    ]
                                ]
                                Html.span [
                                    prop.className "text-base-content/50"
                                    prop.text "F# • SAFE stack • TypeScript"
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "grid gap-6 sm:grid-cols-1 md:grid-cols-2"
                            prop.children [
                                // meta strip
                                codeGalleryCard "Goal Roll"  "Roll the ball in straight lines to the goal."            SharedCodeGallery.GoalRoll  dispatch
                                codeGalleryCard "Tile Sort"  "Arrange the tiles in the correct order."                 SharedCodeGallery.TileSort  dispatch
                                codeGalleryCard "Tile Tap"   "Tap tiles to smash them while avoiding bombs."          SharedCodeGallery.TileTap   dispatch
                                codeGalleryCard "Pivot Points" "Pivot the ball to collect coins across lanes."        SharedCodeGallery.PivotPoint dispatch
                            ]
                        ]
                    ]
                ]
            ]
        ]


    | GoalRoll m   -> GoalRoll.view m (GoalRollMsg >> dispatch) (LoadSection Gallery) dispatch
    | TileSort m   -> TileSort.view m (TileSortMsg >> dispatch) (LoadSection Gallery) dispatch
    | TileTap m    -> TileTap.view  m (TileTapMsg >> dispatch) (LoadSection Gallery) dispatch
    | PivotPoint m -> PivotPoints.view m (PivotPointMsg >> dispatch) (LoadSection Gallery) dispatch
    | SourceCode gallerySection ->
        SourceViewer {|
            Section = gallerySection
            OnBack  = fun () -> dispatch (LoadSection Gallery)
        |}
