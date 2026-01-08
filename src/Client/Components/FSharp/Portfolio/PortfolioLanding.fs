module Components.FSharp.PortfolioLanding

open Elmish
open Feliz
open Bindings.LucideIcon
open Components.FSharp.Portfolio
open Fable.React
open Browser.Dom
open SharedViewModule.WebAppView

type Msg =
    | LoadSection of AppView
    | ArtGalleryMsg of ArtGallery.Msg
    | CodeGalleryMsg of CodeGallery.Msg

type Model =
    | PortfolioGallery
    | CodeGallery of CodeGallery.Model
    | DesignGallery of ArtGallery.Model

let getInitialModel = PortfolioGallery

let init (): Model * Cmd<Msg> =
    PortfolioGallery, Cmd.none

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg, model with
    | LoadSection PortfolioAppLandingView, _ ->
        PortfolioGallery, Cmd.none

    | LoadSection PortfolioAppDesignView, _ ->
        DesignGallery { CurrentPieceIndex = None }, Cmd.none

    | ArtGalleryMsg subMsg, DesignGallery m ->
        let updated, cmd = ArtGallery.update subMsg m
        DesignGallery updated, Cmd.map ArtGalleryMsg cmd

    | LoadSection PortfolioAppCodeView, _ ->
        CodeGallery CodeGallery.getInitialModel, Cmd.none

    | CodeGalleryMsg subMsg, CodeGallery m ->
        let updated, cmd = CodeGallery.update subMsg m
        CodeGallery updated, Cmd.map CodeGalleryMsg cmd

    | _ -> PortfolioGallery, Cmd.none


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

    // Typing effect
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
                    if not outputShown && hasCommand && charIndex < cmdLen then
                        window.setTimeout(
                            (fun _ -> setCharIndex (fun i -> i + 1)),
                            60
                        )
                    else
                        window.setTimeout(
                            (fun _ -> setOutputShown (fun _ -> true)),
                            0
                        )

                React.createDisposable(fun () -> window.clearTimeout(timeoutId))
        ),
        [| box stepIndex; box charIndex |]
    )

    // Advance through steps after output delay
    React.useEffect(
        (fun () ->
            if stepIndex >= steps.Length || not outputShown then
                React.createDisposable ignore
            else
                let step = steps[stepIndex]
                let timeoutId =
                    window.setTimeout(
                        (fun _ ->
                            setCharIndex (fun _ -> 0)
                            setOutputShown (fun _ -> false)
                            setStepIndex (fun i -> i + 1)
                        ),
                        step.AfterOutputDelayMs
                    )

                React.createDisposable(fun () -> window.clearTimeout(timeoutId))
        ),
        [| box outputShown |]
    )

    // Completed steps
    let completedSteps =
        [
            for i in 0 .. stepIndex - 1 do
                let step = steps[i]

                if not (System.String.IsNullOrWhiteSpace step.Command) then
                    Html.div [
                        prop.key (sprintf "cmd-%d" i)
                        prop.className "terminal-line"
                        prop.children [
                            Html.span [
                                prop.className "terminal-prompt"
                                prop.text "root@sean"
                            ]
                            Html.span [
                                prop.className "terminal-command"
                                prop.text (" > " + step.Command)
                            ]
                        ]
                    ]

                Html.div [
                    prop.key (sprintf "out-%d" i)
                    prop.className "terminal-line opacity-60"
                    prop.text step.Output
                ]
        ]

    // Current step
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

            React.fragment [
                if hasCommand then
                    Html.div [
                        prop.key "current-cmd"
                        prop.className "terminal-line"
                        prop.children [
                            Html.span [
                                prop.className "terminal-prompt"
                                prop.text "root@sean"
                            ]
                            Html.span [
                                prop.className "terminal-command"
                                prop.text (" > " + typed)
                            ]
                        ]
                    ]

                if outputShown then
                    Html.div [
                        prop.key "current-out"
                        prop.className "terminal-line"
                        prop.text step.Output
                    ]

                if not hasCommand && not outputShown then
                    Html.div [
                        prop.key "current-wait"
                        prop.className "terminal-line"
                        prop.children [
                            Html.span [
                                prop.className "terminal-prompt"
                                prop.text "> "
                            ]
                        ]
                    ]
            ]
        else
            Html.div [
                prop.key "final-prompt"
                prop.className "terminal-line"
                prop.children [
                    Html.span [
                        prop.className "terminal-prompt"
                        prop.text "> "
                    ]
                ]
            ]

    Html.div [
        // `.terminal` is styled in your CSS; add some rounding + shadow
        prop.className "terminal w-full text-sm leading-relaxed text-left"
        prop.children (completedSteps @ [ currentStepView ])
    ]


[<ReactComponent>]
let GithubProfileCard () =
    Client.Components.Shop.Common.Ui.Animations.ProgressiveReveal {
        Children =
            Html.div [
                prop.className "profile-card"
                prop.children [
                    Html.div [
                        prop.className "flex items-center gap-4 mb-6"
                        prop.children [
                            Html.div [
                                prop.className "w-16 h-16 rounded-full bg-linear-to-br from-blue-400 to-purple-500 flex items-center justify-center text-white font-medium"
                                prop.text "SW"
                            ]
                            Html.div [
                                Html.h3 [
                                    prop.className "serif text-xl font-medium"
                                    prop.text "Sean Wilken"
                                ]
                                Html.p [
                                    prop.className "text-xs opacity-50"
                                    prop.text "@seanwilken"
                                ]
                            ]
                        ]
                    ]

                    Html.p [
                        prop.className "text-xs opacity-60 mb-6 leading-relaxed"
                        prop.text "F# / TypeScript engineer building healthcare systems, tools, and playful experiments."
                    ]

                    Html.div [
                        prop.className "space-y-3 mb-6"
                        prop.children [
                            Html.div [
                                prop.className "flex items-center gap-2 text-xs"
                                prop.children [
                                    Html.span [ prop.className "opacity-50"; prop.text "Likes" ]
                                    Html.span [ prop.className "font-medium"; prop.text "61K" ]
                                ]
                            ]
                            Html.div [
                                prop.className "flex items-center gap-2 text-xs"
                                prop.children [
                                    Html.span [ prop.className "opacity-50"; prop.text "Projects" ]
                                    Html.span [ prop.className "font-medium"; prop.text "87 collections" ]
                                ]
                            ]
                            Html.div [
                                prop.className "flex items-center gap-2 text-xs"
                                prop.children [
                                    Html.span [ prop.className "opacity-50"; prop.text "Currently Building" ]
                                ]
                            ]
                        ]
                    ]

                    Html.ul [
                        prop.className "space-y-2 text-xs opacity-60 mb-8"
                        prop.children [
                            Html.li [ prop.text "• Clinical AI agents & chart review tools" ]
                            Html.li [ prop.text "• AI-powered automation & workflows" ]
                            Html.li [ prop.text "• Interactive portfolios & code demos" ]
                        ]
                    ]

                    Html.a [
                        prop.className "cta-btn w-full text-center"
                        prop.href "https://github.com/seanwilken"
                        prop.target "_blank"
                        prop.children [
                            Html.span [ prop.text "View GitHub profile" ]
                        ]
                    ]
                ]
            ]
    }



[<ReactComponent>]
let private FocusTile
    (onClickHandler : Browser.Types.Event -> unit)
    (icon : ReactElement)
    (tag  : string)
    (body : string)
    (linkText: string)
    =
    Html.div [
        prop.className "max-w-xl space-y-6 category-card"
        prop.onClick onClickHandler
        prop.children [
            // Tag row
            Html.div [
                prop.className "flex items-center gap-3 text-[0.7rem] tracking-[0.22em] uppercase text-base-content/60"
                prop.children [
                    Html.span [ prop.className "text-sm"; prop.children [ icon ] ]
                    Html.span tag
                ]
            ]

            // Main body text (the big serif paragraph)
            Html.p [
                prop.className "font-serif font-bold leading-relaxed text-base-content/90"
                prop.text body
            ]

            // Small link at the bottom
            Html.button [
                prop.className "text-xs md:text-sm text-base-content/60 hover:text-base-content/90 transition-colors"
                prop.text linkText
            ]
        ]
    ]

[<ReactComponent>]
let BrowseByFocusSection (dispatch) =
    Html.section [
        prop.className "w-full py-20 md:py-24"
        prop.children [
            Html.div [
                prop.className "mx-auto w-full max-w-6xl px-6 lg:px-10"
                prop.children [
                    // Heading + subtitle
                    Html.div [
                        prop.className "max-w-2xl space-y-3"
                        prop.children [
                            Html.h2 [
                                prop.className "font-serif text-3xl md:text-4xl text-base-content"
                                prop.text "Browse by focus"
                            ]
                            Html.p [
                                prop.className "text-sm md:text-[0.9rem] text-base-content/60"
                                prop.text "Pick a lens or tag (browse shop → tech gallery) about the core model/principles."
                            ]
                        ]
                    ]

                    // Tiles
                    Html.div [
                        prop.className "mt-16 grid gap-y-16 gap-x-24 md:grid-cols-2"
                        prop.children [
                            FocusTile
                                (fun _ -> LoadSection AppView.PortfolioAppCodeView |> dispatch)
                                (Html.span "</>")
                                "Code Experiments"
                                "Interactive demos, tools, and prototypes. Play with the UI and read the source behind it."
                                "Open-code experiments gallery →"

                            FocusTile
                                (fun _ -> LoadSection AppView.PortfolioAppDesignView |> dispatch)
                                (Html.span "✎")
                                "Design & Drawings"
                                "Visual explorations, studies, and sketches that inform how I think about UI and products."
                                "Open Design & Drawings gallery →"
                        ]
                    ]
                ]
            ]
        ]
    ]


[<ReactComponent>]
let View (model: Model) dispatch =
    match model with
    | PortfolioGallery ->
        Html.div [
            // HERO SECTION
            Html.section [
                prop.className "pt-28 pb-20 px-6 md:px-8 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto"
                        prop.children [
                            
                            Client.Components.Shop.Common.Ui.Section.headerTagArea
                                (LucideIcon.PlayCircle "w-4 h-4 opacity-60")
                                "PORTFOLIO"

                            Html.div [
                                prop.className "grid lg:grid-cols-2 gap-16 items-start"
                                prop.children [

                                    // Left: taglines + CTAs
                                    Html.div [
                                        prop.children [
                                            Html.h1 [
                                                prop.className "serif text-5xl lg:text-6xl font-light mb-8 leading-tight"
                                                prop.text "Ideas that evolve."
                                            ]
                                            Html.h1 [
                                                prop.className "serif text-5xl lg:text-6xl font-light mb-8 leading-tight"
                                                prop.text "Code that matters."
                                            ]
                                            Html.h1 [
                                                prop.className "serif text-5xl lg:text-6xl font-light mb-12 leading-tight"
                                                prop.text "Sketches that breathe."
                                            ]

                                            Html.p [
                                                prop.className "text-sm opacity-60 mb-12 leading-loose max-w-lg"
                                                prop.text
                                                    "A living collection of the things I build: production-grade F# systems, playful code experiments, and hand-drawn artwork. Browse by what you care about most, or dive straight into the galleries."
                                            ]

                                            Html.div [
                                                prop.className "flex flex-wrap gap-3 text-xs opacity-50"
                                                prop.children [
                                                    Html.span [ prop.text "10+ years shipping production systems" ]
                                                    Html.span [ prop.text "•" ]
                                                    Html.span [ prop.text "F#, TypeScript, SAFE stack" ]
                                                    Html.span [ prop.text "•" ]
                                                    Html.span [ prop.text "Healthcare • AI • Tools" ]
                                                ]
                                            ]
                                        ]
                                    ]

                                    // Right: profile card
                                    Html.div [
                                        GithubProfileCard()
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // BROWSE BY FOCUS
            // Html.section [
            //     prop.className "py-24 px-4 sm:px-6 lg:px-12"
            //     prop.children [
            //         Html.div [
            //             prop.className "max-w-6xl mx-auto"
            //             prop.children [
            //                 Html.div [
            //                     prop.className "mb-16"
            //                     prop.children [
            //                         Html.h2 [
            //                             prop.className "serif text-4xl lg:text-5xl font-light mb-4"
            //                             prop.text "Browse by focus"
            //                         ]
            //                         Html.p [
            //                             prop.className "text-sm opacity-50"
            //                             prop.text "Pick a lens or tag (browse shop → tech gallery) about the core model/principles."
            //                         ]
            //                     ]
            //                 ]

            //                 Html.div [
            //                     prop.className "grid lg:grid-cols-2 gap-8"
            //                     prop.children [
            //                         browseCategory
            //                             "Interactive demos, tools, and prototypes. Play with the UI and read the source behind it."
            //                             "Demos and Games →"
            //                             "Code Experiments"
            //                             (LucideIcon.Gamepad2 "w-5 h-5")
            //                             (fun _ ->
            //                                 dispatch (
            //                                     SharedPortfolioGallery.LoadSection
            //                                         SharedWebAppViewSections.AppView.PortfolioAppCodeView
            //                                 )
            //                             )

            //                         browseCategory
            //                             "Visual explorations, studies, and sketches that inform how I think about UI and products."
            //                             "Art and Design →"
            //                             "Design & Drawings"
            //                             (LucideIcon.PenTool "w-5 h-5")
            //                             (fun _ ->
            //                                 dispatch (
            //                                     SharedPortfolioGallery.LoadSection
            //                                         SharedWebAppViewSections.AppView.PortfolioAppDesignView
            //                                 )
            //                             )
            //                     ]
            //                 ]
            //             ]
            //         ]
            //     ]
            // ]
            BrowseByFocusSection dispatch

            // TERMINAL SNIPPET
            Html.section [
                prop.className "py-24 px-4 sm:px-6 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-4xl mx-auto"
                        prop.children [
                            TerminalTypingAnimation()
                        ]
                    ]
                ]
            ]
        ]

    | DesignGallery m ->
        ArtGallery.view m (ArtGalleryMsg >> dispatch)

    | CodeGallery m ->
        CodeGallery.view m (CodeGalleryMsg >> dispatch)
