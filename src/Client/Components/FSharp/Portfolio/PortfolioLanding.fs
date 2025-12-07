module Components.FSharp.PortfolioLanding

open Elmish
open Feliz
open Shared
open Bindings.LucideIcon
open Components.FSharp.Portfolio
open Feliz
open Fable.React
open Browser.Dom

let init (): SharedPortfolioGallery.Model * Cmd<SharedWebAppModels.WebAppMsg> =
    SharedPortfolioGallery.PortfolioGallery, Cmd.none

let update (msg: SharedPortfolioGallery.Msg) (model: SharedPortfolioGallery.Model): SharedPortfolioGallery.Model * Cmd<SharedPortfolioGallery.Msg> =
    match msg, model with
    | SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppLandingView, _ ->
        SharedPortfolioGallery.PortfolioGallery, Cmd.none

    | SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppDesignView, _ ->
        SharedPortfolioGallery.DesignGallery (SharedDesignGallery.getInitialModel), Cmd.none

    | SharedPortfolioGallery.ArtGalleryMsg subMsg, SharedPortfolioGallery.DesignGallery m ->
        let updated, cmd = ArtGallery.update subMsg m
        SharedPortfolioGallery.DesignGallery updated, Cmd.map SharedPortfolioGallery.ArtGalleryMsg cmd

    | SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppCodeView, _ ->
        SharedPortfolioGallery.CodeGallery (SharedCodeGallery.getInitialModel), Cmd.none

    | SharedPortfolioGallery.CodeGalleryMsg subMsg, SharedPortfolioGallery.CodeGallery m ->
        let updated, cmd = CodeGallery.update subMsg m
        SharedPortfolioGallery.CodeGallery updated, Cmd.map SharedPortfolioGallery.CodeGalleryMsg cmd

    | _ -> SharedPortfolioGallery.PortfolioGallery, Cmd.none


type Step =
    { Command: string
      Output: string
      AfterOutputDelayMs: int }


let steps : Step array =
    [|
        { Command = "dotnet tool restore"
          Output = "restored 4 tools"
          AfterOutputDelayMs = 2000 }

        { Command = "pnpm install"
          Output = "packages installed successfully"
          AfterOutputDelayMs = 2000 }

        { Command = "dotnet run deploy"
          Output = "Building... OK ✅"
          AfterOutputDelayMs = 2000 }

        { Command = ""
          Output = "Deploying to the cloud 🚀"
          AfterOutputDelayMs = 2000 }

        { Command = ""
          Output = "Done!"
          AfterOutputDelayMs = 0 }
    |]

[<ReactComponent>]
let TerminalTypingAnimation () =
    let stepIndex, setStepIndex = React.useStateWithUpdater(0)
    let charIndex, setCharIndex = React.useStateWithUpdater(0)
    let outputShown, setOutputShown = React.useStateWithUpdater(false)

    // Single effect that drives the entire state machine
    React.useEffect(
        (fun () ->
            if stepIndex >= steps.Length then
                React.createDisposable ignore
            else
                let step = steps[stepIndex]
                let cmdLen = step.Command.Length
                let hasCommand =
                    not (System.String.IsNullOrWhiteSpace step.Command)

                let timeoutId =
                    // 1) Typing the command
                    if not outputShown && hasCommand && charIndex < cmdLen 
                    then
                        window.setTimeout(
                            (fun _ ->
                                setCharIndex(fun i -> i + 1)
                            ),
                            60  // typing speed
                        )
                    else
                        window.setTimeout(
                            (fun _ ->
                                printfn "Showing output"
                                setOutputShown(fun _ -> true)
                            ),
                            0
                        )
                React.createDisposable(fun () ->
                    window.clearTimeout(timeoutId)
                )
        ),
        [| box stepIndex; box charIndex;|]
    )

    React.useEffect(
        (fun () ->
            if stepIndex >= steps.Length then
                React.createDisposable ignore
            else
                let step = steps[stepIndex]

                let timeoutId =
                    window.setTimeout(
                        (fun _ ->
                            printfn "Advancing to next step"
                            setCharIndex(fun _ -> 0)
                            setOutputShown(fun _ -> false)
                            setStepIndex(fun i -> i + 1)
                        ),
                        step.AfterOutputDelayMs
                    )

                React.createDisposable(fun () ->
                    window.clearTimeout(timeoutId)
                )
        ),
        [| box outputShown |]
    )

    // Render all fully completed steps (command + output)
    let completedSteps =
        [
            for i in 0 .. stepIndex - 1 do
                let step = steps[i]
                if not (System.String.IsNullOrWhiteSpace step.Command) then
                    Html.div [
                        prop.key (sprintf "cmd-%d" i)
                        prop.className "terminal-line text-green-400"
                        prop.text ("root@sean > " + step.Command)
                    ]
                Html.div [
                    prop.key (sprintf "out-%d" i)
                    prop.className "text-green-500 whitespace-pre-line"
                    prop.text step.Output
                ]
        ]

    // Render current step (strictly sequential)
    let currentStepView =
        if stepIndex < steps.Length then
            let step = steps[stepIndex]
            let cmdLen = step.Command.Length
            let hasCommand =
                not (System.String.IsNullOrWhiteSpace step.Command)

            let typed =
                if hasCommand && charIndex <= cmdLen then
                    step.Command.Substring(0, charIndex)
                else
                    step.Command
                |> fun x -> "root@sean > " + x

            React.fragment [
                // Show command line only if we actually have one
                if hasCommand then
                    Html.div [
                        prop.key "current-cmd"
                        prop.className "terminal-line text-green-400"
                        prop.children [
                            Html.span [ prop.text typed ]
                            // Cursor only on the command line while we're before the output
                            if not outputShown then
                                Html.span [ prop.className "terminal-cursor" ]
                        ]
                    ]

                // Show output ONLY when outputShown = true
                if outputShown then
                    Html.div [
                        prop.key "current-out"
                        prop.className "terminal-output text-green-500 whitespace-pre-line"
                        prop.text step.Output
                    ]

                // For output-only steps, show cursor on its own line while waiting
                if not hasCommand && not outputShown then
                    Html.div [
                        prop.key "current-wait"
                        prop.className "terminal-line text-green-400"
                        prop.children [
                            Html.span [ prop.text "> " ]
                            Html.span [ prop.className "terminal-cursor" ]
                        ]
                    ]
            ]
        else
            // All steps done – final prompt
            Html.div [
                prop.key "final-prompt"
                prop.className "terminal-line text-green-400"
                prop.children [
                    Html.span [ prop.text "> " ]
                    Html.span [ prop.className "terminal-cursor" ]
                ]
            ]

    Html.div [
        prop.className
            "bg-black text-green-500 font-mono rounded-xl p-4 w-full h-56 shadow-lg text-sm leading-relaxed border border-green-600 text-left"
        prop.children (completedSteps @ [ currentStepView ])
    ]

[<ReactComponent>]
let DesignGlassCard () =
    Html.div [
        prop.className
            "w-full h-56 rounded-xl flex items-center justify-center glass-card shadow-lg"
        prop.children [
            Html.h1 [
                prop.className "text-4xl font-bold design-word tracking-wide"
                prop.text "Artwork"
            ]
        ]
    ]


[<ReactComponent>]
let GithubProfileCard () =
    Html.div [
        prop.className "rounded-2xl border border-base-200 bg-base-100/80 shadow-md flex flex-col overflow-hidden"
        prop.children [
            // gradient header strip
            Html.div [
                prop.className "h-2 w-full shrink-0 rounded-t-2xl bg-gradient-to-r from-primary via-secondary to-accent"
            ]

            Html.div [
                prop.className "p-4 sm:p-5 space-y-4 flex-1 flex flex-col"
                prop.children [

                    // avatar + name
                    Html.div [
                        prop.className "flex items-center gap-3"
                        prop.children [
                            Html.div [
                                prop.className "w-10 h-10 rounded-full bg-primary/10 text-primary flex items-center justify-center font-semibold"
                                prop.text "SW"
                            ]
                            Html.div [
                                prop.children [
                                    Html.div [
                                        prop.className "text-sm font-semibold"
                                        prop.text "Sean Wilken"
                                    ]
                                    Html.div [
                                        prop.className "text-xs text-base-content/70"
                                        prop.text "@seanwilken"
                                    ]
                                ]
                            ]
                        ]
                    ]

                    Html.p [
                        prop.className "text-xs text-base-content/80"
                        prop.text "F# / TypeScript engineer building healthcare systems, tools, and playful experiments."
                    ]

                    // stats row
                    // Html.div [
                    //     prop.className "flex gap-6 text-[11px] text-base-content/70"
                    //     prop.children [
                    //         Html.div [
                    //             prop.className "space-y-0.5"
                    //             prop.children [
                    //                 Html.div [ prop.className "font-semibold text-sm"; prop.text "120+" ]
                    //                 Html.div [ prop.text "Repos" ]
                    //             ]
                    //         ]
                    //         Html.div [
                    //             prop.className "space-y-0.5"
                    //             prop.children [
                    //                 Html.div [ prop.className "font-semibold text-sm"; prop.text "40+" ]
                    //                 Html.div [ prop.text "Pinned projects" ]
                    //             ]
                    //         ]
                    //         Html.div [
                    //             prop.className "space-y-0.5"
                    //             prop.children [
                    //                 Html.div [ prop.className "font-semibold text-sm"; prop.text "10+ yrs" ]
                    //                 Html.div [ prop.text "In production" ]
                    //             ]
                    //         ]
                    //     ]
                    // ]

                    // focus areas
                    Html.div [
                        prop.className "space-y-1"
                        prop.children [
                            Html.div [
                                prop.className "text-[11px] font-semibold uppercase tracking-wide text-base-content/60"
                                prop.text "Focus"
                            ]
                            Html.div [
                                prop.className "flex flex-wrap gap-2"
                                prop.children [
                                    Html.span [ prop.className "badge badge-ghost badge-xs"; prop.text "Healthcare" ]
                                    Html.span [ prop.className "badge badge-ghost badge-xs"; prop.text "AI & automation" ]
                                    Html.span [ prop.className "badge badge-ghost badge-xs"; prop.text "E-Commerce" ]
                                    Html.span [ prop.className "badge badge-ghost badge-xs"; prop.text "Dev tools" ]
                                ]
                            ]
                        ]
                    ]

                    // core stack
                    Html.div [
                        prop.className "space-y-1"
                        prop.children [
                            Html.div [
                                prop.className "text-[11px] font-semibold uppercase tracking-wide text-base-content/60"
                                prop.text "Interests"
                            ]
                            Html.div [
                                prop.className "flex flex-wrap gap-1.5 text-[11px]"
                                prop.children [
                                    Html.span [ prop.className "badge badge-outline badge-xs"; prop.text "F#" ]
                                    Html.span [ prop.className "badge badge-outline badge-xs"; prop.text "AI" ]
                                    Html.span [ prop.className "badge badge-outline badge-xs"; prop.text "Functional programming" ]
                                    Html.span [ prop.className "badge badge-outline badge-xs"; prop.text "React" ]
                                ]
                            ]
                        ]
                    ]

                    // currently building
                    Html.div [
                        prop.className "space-y-1"
                        prop.children [
                            Html.div [
                                prop.className "text-[11px] font-semibold uppercase tracking-wide text-base-content/60"
                                prop.text "Currently building"
                            ]
                            Html.ul [
                                prop.className "text-[11px] text-base-content/75 space-y-0.5 list-disc list-inside"
                                prop.children [
                                    Html.li [ prop.text "Trauma registry submission tooling" ]
                                    Html.li [ prop.text "AI-powered automation & reminders" ]
                                    Html.li [ prop.text "Interactive portfolio & code demos" ]
                                ]
                            ]
                        ]
                    ]

                    Html.div [
                        prop.className "pt-2 mt-auto"
                        prop.children [
                            Html.a [
                                prop.className "btn btn-xs sm:btn-sm btn-outline w-full justify-center gap-2"
                                prop.href "https://github.com/seanwilken"
                                prop.target "_blank"
                                prop.children [
                                    LucideIcon.Github "w-3 h-3"
                                    Html.span [ prop.text "View GitHub profile" ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

[<ReactComponent>]
let ArtworkStrip (title: string) =
    Html.div [
        prop.className "w-full rounded-xl glass-card flex items-center justify-center py-3"
        prop.children [
            Html.span [
                prop.className "text-lg font-semibold design-word tracking-wide"
                prop.text title
            ]
        ]
    ]

let portfolioTile
    (title: string)
    (description: string)
    (msg: SharedPortfolioGallery.Msg)
    (dispatch: SharedPortfolioGallery.Msg -> unit)
    (accentClass: string)
    (icon: ReactElement) =

    Html.div [
        prop.className
            "group card bg-base-200/60 border border-base-300/70 shadow-sm hover:shadow-md cursor-pointer transition-transform hover:-translate-y-[2px] h-full"
        prop.onClick (fun _ -> dispatch msg)
        prop.children [
            Html.div [
                prop.className "card-body gap-3"
                prop.children [
                    Html.div [
                        prop.className "flex items-center gap-3"
                        prop.children [
                            Html.div [
                                prop.className $"inline-flex items-center justify-center w-9 h-9 rounded-full bg-{accentClass}/10 text-{accentClass}"
                                prop.children [ icon ]
                            ]
                            ArtworkStrip title
                        ]
                    ]

                    Html.p [
                        prop.className "text-sm text-base-content/80"
                        prop.text description
                    ]

                    Html.div [
                        prop.className "pt-1"
                        prop.children [
                            Html.span [
                                prop.className $"link link-hover text-{accentClass} text-xs font-medium"
                                prop.text $"Open {title} gallery →"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let view model dispatch =
    match model with
    | SharedPortfolioGallery.PortfolioGallery ->
        Html.section [
            prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16 space-y-12"
            prop.children [
                Html.div [
                    prop.className "card bg-base-100/80 overflow-hidden"
                    prop.children [
                        Html.div [
                            prop.className "card-body p-6 sm:p-8 lg:p-10 space-y-8"
                            prop.children [

                                // FULL-WIDTH TOP STRIP (uses that empty space)
                                Html.div [
                                    prop.className "flex flex-wrap items-center gap-2 text-xs font-medium"
                                    prop.children [
                                        Html.span [
                                            prop.className "px-3 py-1 rounded-full bg-primary/10 text-primary"
                                            prop.text "Portfolio"
                                        ]
                                        Html.span [
                                            prop.className "px-3 py-1 rounded-full bg-base-200 text-base-content/80"
                                            prop.text "Code • Design • Experiments"
                                        ]
                                    ]
                                ]

                                // 2/3 – 1/3 GRID: left copy, right GitHub profile
                                Html.div [
                                    // 1 col on mobile, 8 cols from md and up
                                    prop.className "flex p-2"
                                    prop.children [

                                        // LEFT: text + CTAs (5/8 ≈ 2/3)
                                        Html.div [
                                            prop.className "space-y-6 md:col-span-5"
                                            prop.children [
                                                Html.h1 [
                                                    prop.className "text-4xl sm:text-5xl font-extrabold"
                                                    prop.text "Ideas that evolve. Builds that ship. Sketches that breathe."
                                                ]

                                                Html.p [
                                                    prop.className "text-base-content/80 max-w-xl"
                                                    prop.text
                                                        "A living collection of the things I build: production-grade F# systems, playful code experiments, and hand-drawn artwork. Browse by what you care about most, or dive straight into the galleries."
                                                ]

                                                Html.div [
                                                    prop.className "flex flex-wrap gap-3"
                                                    prop.children [
                                                        Html.div [
                                                            prop.className "badge badge-outline gap-1"
                                                            prop.children [
                                                                LucideIcon.Code2 "w-3 h-3"
                                                                Html.span [ prop.text "Engineering" ]
                                                            ]
                                                        ]
                                                        Html.div [
                                                            prop.className "badge badge-outline gap-1"
                                                            prop.children [
                                                                LucideIcon.PenTool "w-3 h-3"
                                                                Html.span [ prop.text "Design & Art" ]
                                                            ]
                                                        ]
                                                        Html.div [
                                                            prop.className "badge badge-outline gap-1"
                                                            prop.children [
                                                                LucideIcon.FlaskConical "w-3 h-3"
                                                                Html.span [ prop.text "Experiments" ]
                                                            ]
                                                        ]
                                                    ]
                                                ]

                                                Html.div [
                                                    prop.className "flex flex-wrap gap-3 pt-2"
                                                    prop.children [
                                                        Html.button [
                                                            prop.className "btn btn-primary btn-lg gap-2"
                                                            prop.onClick (fun _ ->
                                                                dispatch (
                                                                    SharedPortfolioGallery.LoadSection
                                                                        SharedWebAppViewSections.AppView.PortfolioAppCodeView
                                                                )
                                                            )
                                                            prop.children [
                                                                LucideIcon.PlayCircle "w-4 h-4"
                                                                Html.span [ prop.text "Browse code projects" ]
                                                            ]
                                                        ]

                                                        Html.button [
                                                            prop.className "btn btn-ghost btn-lg gap-2"
                                                            prop.onClick (fun _ ->
                                                                dispatch (
                                                                    SharedPortfolioGallery.LoadSection
                                                                        SharedWebAppViewSections.AppView.PortfolioAppDesignView
                                                                )
                                                            )
                                                            prop.children [
                                                                LucideIcon.Image "w-4 h-4"
                                                                Html.span [ prop.text "See design & drawings" ]
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "text-xs text-base-content/60 pt-2 space-x-4"
                                                    prop.children [
                                                        Html.span [ prop.text "10+ years shipping production systems" ]
                                                        Html.span [ prop.text "F#, TypeScript, SAFE stack" ]
                                                        Html.span [ prop.text "Healthcare • AI • Tools" ]
                                                    ]
                                                ]

                                            ]
                                        ]
                                        Html.div [
                                            prop.className "flex flex-wrap items-center gap-2 text-xs font-medium"
                                            prop.children [ GithubProfileCard() ]
                                        ]
                                    ]
                                ]
                            ]
                        ]

                    ]
                ]

                // BROWSE BY FOCUS (simple cards)
                Html.div [
                    prop.className "space-y-6"
                    prop.children [

                        Html.div [
                            prop.className "flex items-center justify-between gap-3"
                            prop.children [
                                Html.h2 [
                                    prop.className "text-sm font-semibold tracking-tight"
                                    prop.text "Browse by focus"
                                ]
                                Html.p [
                                    prop.className "text-[11px] text-base-content/70"
                                    prop.text "Pick a lane or hop between them — both galleries share the same underlying story."
                                ]
                            ]
                        ]

                        Html.div [
                            prop.className "grid grid-cols-1 md:grid-cols-2 gap-5"
                            prop.children [
                                portfolioTile
                                    "Code Experiments"
                                    "Interactive demos, tools, and prototypes. Play with the UI and read the source behind it."
                                    (SharedPortfolioGallery.LoadSection
                                        SharedWebAppViewSections.AppView.PortfolioAppCodeView)
                                    dispatch
                                    "primary"
                                    (LucideIcon.Gamepad2 "w-4 h-4")

                                portfolioTile
                                    "Design & Drawings"
                                    "Visual explorations, studies, and sketches that inform how I think about UI and products."
                                    (SharedPortfolioGallery.LoadSection
                                        SharedWebAppViewSections.AppView.PortfolioAppDesignView)
                                    dispatch
                                    "secondary"
                                    (LucideIcon.PenTool "w-4 h-4")
                            ]
                        ]
                        TerminalTypingAnimation()
                    ]
                ]
            ]
        ]

    | SharedPortfolioGallery.DesignGallery m ->
        ArtGallery.view m (SharedPortfolioGallery.ArtGalleryMsg >> dispatch)

    | SharedPortfolioGallery.CodeGallery m ->
        CodeGallery.view m (SharedPortfolioGallery.CodeGalleryMsg >> dispatch)
