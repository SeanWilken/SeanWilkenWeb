module Components.FSharp.Portfolio.CodeGallery

open Elmish
open Feliz
open Fable.Core.JsInterop
open Components.FSharp.Portfolio.Games
open Bindings.LucideIcon
open TSXDemos
open SharedViewModule.WebAppView

type Msg =
    | BackToPortfolio
    | LoadSection of CodeSection
    | LoadSourceCode of CodeSection
    | GoalRollMsg of GoalRoll.Msg
    | TileTapMsg of TileTap.Msg
    | TileSortMsg of TileSort.Msg
    | PivotPointMsg of PivotPoints.Msg
    | SynthNeverSetsMsg of SynthNeverSets.Msg
    | AnimationsMsg of AnimationDemo.Msg

type Model =
    | CodeGallery
    | GoalRoll of GoalRoll.Model
    | TileTap of TileTap.Model
    | TileSort of TileSort.Model
    | SynthNeverSets
    | Animations
    | PivotPoint of PivotPoints.Model
    | SourceCode of CodeSection

let getInitialModel = CodeGallery

let getSourceCode CodeSection : string = 
    match CodeSection with
    | CodeSection.TileSort -> importDefault "./TileSort/TileSort.fs?raw"
    | CodeSection.TileTap -> importDefault "./TileTap/TileTap.fs?raw"
    | CodeSection.GoalRoll -> importDefault "./GoalRoll/GoalRoll.fs?raw"
    | CodeSection.PivotPoint -> importDefault "./PivotPoints/PivotPoints.fs?raw"
    | CodeSection.SynthNeverSets -> importDefault "../../../Typescript/Demos/SynthNeverSets.tsx?raw"
    | CodeSection.Animations -> importDefault "../../../Typescript/Demos/Animations.tsx?raw"
    | _ -> ""

let init(): Model * Cmd<Msg> = CodeGallery, Cmd.none

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg, model with
    | LoadSection CodeLanding, _ -> CodeGallery, Cmd.none
    | LoadSection CodeSection.GoalRoll, _ ->
        let m, cmd = GoalRoll.init()
        GoalRoll m, Cmd.map GoalRollMsg cmd
    | GoalRollMsg GoalRoll.Msg.QuitGame, GoalRoll _ -> CodeGallery, Cmd.none
    | GoalRollMsg msg, GoalRoll m ->
        let m', cmd = GoalRoll.update msg m
        GoalRoll m', Cmd.map GoalRollMsg cmd

    | LoadSection CodeSection.TileSort, _ ->
        let m, cmd = TileSort.init()
        TileSort m, Cmd.map TileSortMsg cmd
    | TileSortMsg TileSort.Msg.QuitGame, TileSort _ -> CodeGallery, Cmd.none
    | TileSortMsg msg, TileSort m ->
        let m', cmd = TileSort.update msg m
        TileSort m', Cmd.map TileSortMsg cmd

    | LoadSection CodeSection.TileTap, _ ->
        let m, cmd = TileTap.init()
        TileTap m, Cmd.map TileTapMsg cmd
    | TileTapMsg TileTap.Msg.QuitGame, TileTap _ ->
        TileTap.update TileTap.Msg.ExitGameLoop |> ignore
        CodeGallery, Cmd.none
    | TileTapMsg msg, TileTap m ->
        let m', cmd = TileTap.update msg m
        TileTap m', Cmd.map TileTapMsg cmd

    | LoadSection CodeSection.PivotPoint, _ ->
        let m, cmd = PivotPoints.init()
        PivotPoint m, Cmd.map PivotPointMsg cmd
    | PivotPointMsg PivotPoints.Msg.QuitGame, PivotPoint _ ->
        PivotPoints.update PivotPoints.Msg.ExitGameLoop |> ignore
        CodeGallery, Cmd.none
    | PivotPointMsg msg, PivotPoint m ->
        let m', cmd = PivotPoints.update msg m
        PivotPoint m', Cmd.map PivotPointMsg cmd

    | LoadSection CodeSection.SynthNeverSets, _ ->
        SynthNeverSets, Cmd.none
    | SynthNeverSetsMsg SynthNeverSets.Msg.QuitGame, _ ->
        CodeGallery, Cmd.none
    | SynthNeverSetsMsg msg, SynthNeverSets ->
        SynthNeverSets, Cmd.none

    | LoadSection CodeSection.Animations, _ ->
        SynthNeverSets, Cmd.none
    | AnimationsMsg AnimationDemo.Msg.QuitDemo, _ ->
        CodeGallery, Cmd.none

    | LoadSourceCode codeSection, _ ->
        // let m, cmd = PivotPoints.init()
        SourceCode codeSection, Cmd.none

    | _ -> model, Cmd.none

// HEADER (small helper you can keep if you want; not strictly required)
let codeGalleryHeader =
    Html.div [
        prop.className "text-center space-y-2"
        prop.children [
            Html.h1 [
                prop.className "text-4xl font-bold text-primary"
                prop.text "Code Experiments"
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
        LucideIcon.Target "w-4 h-4", "Puzzle"
    | "Tile Sort" ->
        LucideIcon.PanelLeft "w-4 h-4", "Logic"
    | "Tile Tap" ->
        LucideIcon.MousePointerClick "w-4 h-4", "Reflex"
    | "Pivot Points" ->
        LucideIcon.GitBranch "w-4 h-4", "Patterns"
    | "Synth Never Sets" ->
        LucideIcon.Compass "w-4 h-4", "Simple"
    | "Animations" ->
        LucideIcon.Nucleus "w-4 h-4", "Patterns"
    | _ ->
        LucideIcon.Gamepad2 "w-4 h-4", "Experiment"

// “experiment card” for the ALL EXPERIMENTS grid
[<ReactComponent>]
let CodeGalleryCard (title: string) (description: string) CodeSection dispatch =
    let icon, tag = gameMeta title

    Html.div [
        prop.className
            "group border border-base-200/80 bg-base-100/90 shadow-md hover:shadow-2xl hover:-translate-y-3 transition duration-200 ease-out overflow-hidden relative"
        prop.children [

            // top gradient bar
            Html.div [
                prop.className
                    "h-1.5 w-full bg-gradient-to-r from-primary via-secondary to-accent"
            ]

            Html.div [
                prop.className "p-6 sm:p-8 space-y-5"
                prop.children [

                    // title row
                    Html.div [
                        prop.className "flex items-center gap-4"
                        prop.children [
                            Html.div [
                                prop.className
                                    "inline-flex items-center justify-center w-12 h-12 rounded-full bg-primary/15 text-primary"
                                prop.children [ icon ]
                            ]
                            Html.div [
                                prop.children [
                                    Html.h3 [
                                        prop.className "serif text-2xl font-light"
                                        prop.text title
                                    ]
                                    Html.span [
                                        prop.className
                                            "mt-1 inline-flex items-center gap-1 text-[11px] px-2 py-0.5 rounded-full bg-base-200/80 text-base-content/60 uppercase tracking-[0.15em]"
                                        prop.text tag
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // description
                    Html.p [
                        prop.className "text-sm text-base-content/70 leading-relaxed"
                        prop.text description
                    ]

                    // footer buttons
                    Html.div [
                        prop.className "flex flex-wrap gap-3 pt-2 justify-around"
                        prop.children [
                            Html.button [
                                prop.className
                                    "inline-flex-1 btn btn-xs sm:btn-sm btn-outline tracking-[0.15em] uppercase text-[0.7rem]"
                                prop.text "Source code"
                                prop.onClick (fun _ -> LoadSourceCode CodeSection |> dispatch)
                            ]
                            Html.button [
                                prop.className
                                    "btn btn-xs sm:btn-sm btn-primary gap-2 tracking-[0.15em] uppercase text-[0.7rem]"
                                prop.onClick (fun _ -> dispatch (LoadSection CodeSection))
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


// -------- FEATURED EXPERIMENT COMPONENT (reusable) --------

type FeaturedExperimentProps = {| 
    Title       : string
    Tag         : string
    Description : string
    Icon        : ReactElement
    OnSource    : unit -> unit
    OnLaunch    : unit -> unit
    Media       : ReactElement
|}

[<ReactComponent>]
let FeaturedExperiment (props: FeaturedExperimentProps) =
    Html.div [
        prop.className
            "rounded-3xl border border-base-200/80 bg-gradient-to-br from-base-200/80 via-base-100 to-base-100 px-6 py-8 lg:px-10 lg:py-10 shadow-xl transition-transform duration-300 hover:-translate-y-2"
        prop.children [
            Html.div [
                prop.className "grid lg:grid-cols-2 gap-10 lg:gap-12 items-center"
                prop.children [

                    // LEFT: text
                    Html.div [
                        prop.children [

                            Html.div [
                                prop.className "flex items-center gap-4 mb-6"
                                prop.children [
                                    Html.div [
                                        prop.className
                                            "w-16 h-16 rounded-full bg-gradient-to-br from-primary to-secondary flex items-center justify-center text-white"
                                        prop.children [ props.Icon ]
                                    ]
                                    Html.div [
                                        prop.children [
                                            Html.h3 [
                                                prop.className "serif text-3xl font-light mb-1"
                                                prop.text props.Title
                                            ]
                                            Html.span [
                                                prop.className
                                                    "inline-flex items-center px-3 py-1 rounded-full bg-primary/15 border border-primary/40 text-[0.65rem] tracking-[0.15em] uppercase"
                                                prop.text props.Tag
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                            Html.p [
                                prop.className "text-sm text-base-content/70 mb-8 leading-relaxed"
                                prop.text props.Description
                            ]

                            Html.div [
                                prop.className "flex flex-wrap gap-4"
                                prop.children [
                                    Html.button [
                                        prop.className
                                            "inline-flex items-center gap-2 border border-base-300 px-4 py-2 text-[0.7rem] tracking-[0.15em] uppercase hover:border-base-400 hover:-translate-y-0.5 transition"
                                        prop.onClick (fun _ -> props.OnSource())
                                        prop.children [
                                            LucideIcon.Code2 "w-3 h-3"
                                            Html.span [ prop.text "Source code" ]
                                        ]
                                    ]
                                    Html.button [
                                        prop.className
                                            "inline-flex items-center gap-2 px-5 py-2 bg-primary text-primary-content text-[0.7rem] tracking-[0.15em] uppercase hover:bg-primary/90 hover:-translate-y-0.5 shadow-md transition"
                                        prop.onClick (fun _ -> props.OnLaunch())
                                        prop.children [
                                            Html.span [ prop.text "Launch demo" ]
                                            LucideIcon.ArrowRight "w-3 h-3"
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // RIGHT: media slot (video / placeholder)
                    Html.div [
                        prop.className
                            "aspect-video rounded-2xl border border-base-300/60 bg-gradient-to-br from-primary/10 via-base-100/40 to-secondary/10 flex items-center justify-center overflow-hidden"
                        prop.children [ props.Media ]
                    ]
                ]
            ]
        ]
    ]


// Simple placeholder media until you drop in an actual <video>
let goalRollPreviewMedia =
    Html.div [
        prop.className "text-center text-base-content/60 space-y-3"
        prop.children [
            LucideIcon.PlayCircle "w-16 h-16 mx-auto opacity-70"
            Html.p [
                prop.className "text-xs tracking-[0.15em] uppercase"
                prop.text "Preview demo"
            ]
        ]
    ]


// ---------- SOURCE VIEWER (unchanged) ----------

type Props = {| Section: CodeSection; OnBack: unit -> unit |}

let SourceViewer = React.functionComponent(fun (props: Props) ->
    Html.div [
        prop.className "max-w-6xl mx-auto p-4 space-y-4"
        prop.children [
            Html.div [
                prop.className "flex items-center justify-between"
                prop.children [
                    Html.button [
                        prop.className "btn btn-sm btn-outline rounded-full gap-1 tracking-[0.15em] uppercase text-[0.7rem]"
                        prop.onClick (fun _ -> props.OnBack())
                        prop.children [
                            LucideIcon.ArrowLeft "w-3 h-3"
                            Html.span [ prop.text "Back to gallery" ]
                        ]
                    ]
                    Html.h2 [
                        prop.className "text-xl font-bold"
                        prop.text (sprintf "Source: %s.fs" (props.Section.ToString()))
                    ]
                ]
            ]

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


// ---------------- MAIN VIEW ----------------

let view model dispatch =
    match model with
    | CodeGallery ->
        Html.section [
            prop.className
                "relative max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-16 space-y-16"
            prop.children [

                // Top nav: back + GitHub
                Html.div [
                    prop.className "flex items-center justify-between"
                    prop.children [
                        Html.button [
                            prop.className
                                "inline-flex items-center gap-2 text-[0.7rem] tracking-[0.15em] uppercase text-base-content/60 hover:text-base-content transition hover:-translate-x-1"
                            prop.onClick (fun _ -> BackToPortfolio |> dispatch)
                            prop.children [
                                LucideIcon.ArrowLeft "w-4 h-4"
                                Html.span [ prop.text "Back to portfolio" ]
                            ]
                        ]

                        Html.a [
                            prop.href "https://github.com/seanwilken"
                            prop.target "_blank"
                            prop.className
                                "inline-flex items-center gap-2 text-[0.7rem] tracking-[0.15em] uppercase border border-base-300 rounded-full px-4 py-2 hover:bg-base-200/60 hover:border-base-400 transition"
                            prop.children [
                                LucideIcon.Github "w-4 h-4"
                                Html.span [ prop.text "GitHub" ]
                            ]
                        ]
                    ]
                ]

                // HERO
                Html.div [
                    prop.className "text-center max-w-3xl mx-auto space-y-6"
                    prop.children [
                        Html.h1 [
                            prop.className "serif text-5xl sm:text-6xl font-light leading-tight"
                            prop.text "Code Experiments"
                        ]
                        Html.p [
                            prop.className "text-sm sm:text-base text-base-content/70 leading-loose"
                            prop.text
                                "Play with interactive demos built in F#. Each experiment explores a different UI, game loop, or logic challenge. These are living prototypes—proof that functional programming can be playful and surprisingly fun."
                        ]
                        Html.div [
                            prop.className "flex flex-wrap justify-center gap-3"
                            prop.children [
                                Html.span [
                                    prop.className
                                        "px-4 py-1 border border-base-300 rounded-full text-[0.65rem] tracking-[0.15em] uppercase"
                                    prop.text "F#"
                                ]
                                Html.span [
                                    prop.className
                                        "px-4 py-1 border border-base-300 rounded-full text-[0.65rem] tracking-[0.15em] uppercase"
                                    prop.text "SAFE stack"
                                ]
                                Html.span [
                                    prop.className
                                        "px-4 py-1 border border-base-300 rounded-full text-[0.65rem] tracking-[0.15em] uppercase"
                                    prop.text "TypeScript"
                                ]
                            ]
                        ]
                        Html.p [
                            prop.className "text-[0.7rem] uppercase tracking-[0.18em] text-base-content/40"
                            prop.text "Built for fun in F# and TypeScript"
                        ]
                    ]
                ]

                // STATS
                Html.div [
                    prop.className "max-w-xl mx-auto"
                    prop.children [
                        Html.div [
                            prop.className "grid grid-cols-1 sm:grid-cols-3 gap-4 sm:gap-6"
                            prop.children [
                                Html.div [
                                    prop.className
                                        "rounded-2xl border border-base-200/80 bg-base-100/80 text-center px-4 py-6 shadow-sm hover:shadow-md hover:-translate-y-1 transition"
                                    prop.children [
                                        Html.div [
                                            prop.className "text-3xl font-light mb-1"
                                            prop.text "5"
                                        ]
                                        Html.p [
                                            prop.className "text-[0.7rem] uppercase tracking-[0.18em] text-base-content/60"
                                            prop.text "Playable\nExperiments"
                                        ]
                                    ]
                                ]
                                Html.div [
                                    prop.className
                                        "rounded-2xl border border-base-200/80 bg-base-100/80 text-center px-4 py-6 shadow-sm hover:shadow-md hover:-translate-y-1 transition"
                                    prop.children [
                                        Html.div [
                                            prop.className "text-3xl font-light mb-1"
                                            prop.text "100%"
                                        ]
                                        Html.p [
                                            prop.className "text-[0.7rem] uppercase tracking-[0.18em] text-base-content/60"
                                            prop.text "Open\nSource"
                                        ]
                                    ]
                                ]
                                Html.div [
                                    prop.className
                                        "rounded-2xl border border-base-200/80 bg-base-100/80 text-center px-4 py-6 shadow-sm hover:shadow-md hover:-translate-y-1 transition"
                                    prop.children [
                                        Html.div [
                                            prop.className "text-3xl font-light mb-1"
                                            prop.text "F#"
                                        ]
                                        Html.p [
                                            prop.className "text-[0.7rem] uppercase tracking-[0.18em] text-base-content/60"
                                            prop.text "Functional\nFirst"
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]

                // FEATURED EXPERIMENT (Goal Roll for now)
                Html.div [
                    prop.children [
                        Html.p [
                            prop.className
                                "text-[0.7rem] tracking-[0.2em] uppercase text-base-content/50 text-center mb-6"
                            prop.text "Featured experiment"
                        ]
                        FeaturedExperiment {| 
                            Title       = "Synth Never Sets"
                            Tag         = "TSX Game"
                            Description =
                                "Roll the windows down on a never ending synthwave drive."
                            Icon        = LucideIcon.Compass "w-8 h-8"
                            OnSource    = (fun () -> LoadSourceCode CodeSection.SynthNeverSets |> dispatch)
                            OnLaunch    = (fun () -> LoadSection CodeSection.SynthNeverSets |> dispatch)
                            Media       = goalRollPreviewMedia
                        |}
                    ]
                ]

                // ALL EXPERIMENTS GRID
                Client.Components.Shop.Common.Ui.Animations.ProgressiveReveal {
                    Children =
                        Html.div [
                            prop.className "space-y-8"
                            prop.children [
                                Html.p [
                                    prop.className
                                        "text-[0.7rem] tracking-[0.2em] uppercase text-base-content/50 text-center"
                                    prop.text "All experiments"
                                ]
                                Html.div [
                                    prop.className "grid gap-8 md:grid-cols-2"
                                    prop.children [
                                        CodeGalleryCard
                                            "Animations"
                                            "Check out some style animations."
                                            CodeSection.Animations
                                            dispatch

                                        CodeGalleryCard
                                            "Goal Roll"
                                            "Roll the ball in straight lines to the goal."
                                            CodeSection.GoalRoll
                                            dispatch

                                        CodeGalleryCard
                                            "Pivot Points"
                                            "Pivot the ball to collect coins across lanes."
                                            CodeSection.PivotPoint
                                            dispatch

                                        CodeGalleryCard
                                            "Synth Never Sets"
                                            "Roll the windows down on a never ending synthwave drive.Roll the windows down on a never ending synthwave drive."
                                            CodeSection.SynthNeverSets
                                            dispatch

                                        CodeGalleryCard
                                            "Tile Sort"
                                            "Arrange the tiles in the correct order."
                                            CodeSection.TileSort
                                            dispatch

                                        CodeGalleryCard
                                            "Tile Tap"
                                            "Tap tiles to smash them while avoiding bombs."
                                            CodeSection.TileTap
                                            dispatch
                                    ]
                                ]
                            ]
                        ]
                }

                // CTA
                
                Client.Components.Shop.Common.Ui.Animations.ProgressiveReveal {
                    Children =
                        Html.div [
                            prop.className "max-w-3xl mx-auto text-center space-y-6 pt-4"
                            prop.children [
                                Html.h2 [
                                    prop.className "serif text-3xl sm:text-4xl font-light"
                                    prop.text "Want to see the code?"
                                ]
                                Html.p [
                                    prop.className "text-sm text-base-content/70 leading-loose"
                                    prop.text
                                        "Every experiment is open source. Browse the repositories, fork the code, or use them as learning examples for your own F# projects."
                                ]
                                Html.div [
                                    prop.className "flex flex-col sm:flex-row gap-4 justify-center"
                                    prop.children [
                                        Html.a [
                                            prop.href "https://github.com/seanwilken"
                                            prop.target "_blank"
                                            prop.className
                                                "inline-flex items-center justify-center gap-2 border border-base-300 rounded-full px-5 py-2 text-[0.7rem] tracking-[0.15em] uppercase hover:bg-base-200/60 hover:border-base-400 transition"
                                            prop.children [
                                                LucideIcon.Github "w-4 h-4"
                                                Html.span [ prop.text "View all repositories" ]
                                            ]
                                        ]
                                        Html.button [
                                            prop.className
                                                "inline-flex items-center justify-center gap-2 rounded-full px-5 py-2 bg-base-200 text-base-content text-[0.7rem] tracking-[0.15em] uppercase hover:bg-base-300 transition"
                                            prop.onClick (fun _ -> BackToPortfolio |> dispatch)
                                            prop.children [
                                                LucideIcon.LayoutTemplate "w-4 h-4"
                                                Html.span [ prop.text "Back to portfolio" ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                }
            ]
        ]

    | GoalRoll m   -> GoalRoll.view m (GoalRollMsg >> dispatch) (LoadSection CodeLanding) dispatch
    | TileSort m   -> TileSort.view m (TileSortMsg >> dispatch) (LoadSection CodeLanding) dispatch
    | TileTap m    -> TileTap.view  m (TileTapMsg >> dispatch) (LoadSection CodeLanding) dispatch
    | PivotPoint m -> PivotPoints.view m (PivotPointMsg >> dispatch) (LoadSection CodeLanding) dispatch
    | Animations -> 
        AnimationDemo.DemoShowcase 
            {|
                onBack = Some (fun () -> LoadSection CodeLanding |> dispatch)
                initialIndex = Some 0 
            |}
    | SynthNeverSets -> 
        SynthNeverSets.View () (SynthNeverSetsMsg >> dispatch) (LoadSection CodeLanding) dispatch
        // Components.TSX.Portfolio.Games.SynthNeverSets.View (LoadSection Gallery) dispatch
    | SourceCode codeSection ->
        SourceViewer {| Section = codeSection; OnBack = fun () -> dispatch (LoadSection CodeLanding) |}