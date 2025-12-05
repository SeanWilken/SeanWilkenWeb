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
            "bg-black text-green-500 font-mono rounded-lg p-4 max-w-lg mx-auto shadow-md text-sm leading-relaxed border border-green-600 text-left"
        prop.style [
            style.custom ("width", "-webkit-fill-available")
            style.custom ("height", "-webkit-fill-available")
        ]
        prop.children (completedSteps @ [ currentStepView ])
    ]

[<ReactComponent>]
let DesignGlassCard () =
    Html.div [
        prop.className "w-full max-w-xl h-60 mx-auto p-6 rounded-xl flex items-center justify-center glass-card"
        prop.style [ style.custom ("width", "-webkit-fill-available"); style.custom ("height", "-webkit-fill-available"); ]
        prop.children [
            Html.h1 [
                prop.className "text-4xl font-bold design-word tracking-wide"
                prop.text "Artwork"
            ]
        ]
    ]

let portfolioTile 
    (title: string) 
    (description: string) 
    (msg: SharedPortfolioGallery.Msg) 
    (dispatch: SharedPortfolioGallery.Msg -> unit) 
    (bgColor: string) 
    (svgGraphic: ReactElement) =
    
    Html.div [
        prop.className "group card h-[26rem] bg-base-200 shadow-xl cursor-pointer transition-transform hover:scale-[1.02]"
        prop.onClick (fun _ -> dispatch msg)
        prop.children [
            Html.div [
                prop.className "card-body items-center text-center justify-between"
                prop.children [
                    svgGraphic
                    Html.h2 [ 
                        prop.className $"card-title text-2xl text-{bgColor}"
                        prop.text title
                    ]
                    Html.p [
                        prop.className "text-sm text-base-content/80"
                        prop.text description
                    ]
                    Html.div [
                        prop.className "mt-4"
                        prop.children [
                            Html.span [
                                prop.className $"btn btn-outline btn-{bgColor} btn-sm"
                                prop.text $"See {title}"
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
        Html.div [
            prop.className "max-w-6xl mx-auto px-6 py-16"
            prop.children [
                Html.div [
                    prop.className "text-center mb-16 space-y-4"
                    prop.children [
                        Html.h1 [
                            prop.className "text-5xl font-extrabold tracking-tight text-primary"
                            prop.text "My Projects"
                        ]
                        Html.p [
                            prop.className "text-base-content/80 max-w-xl mx-auto"
                            prop.text "Explore a collection of my work,  from playable code demos to hand-drawn designs and illustrations."
                        ]
                    ]
                ]

                Html.div [
                    prop.className "grid grid-cols-1 md:grid-cols-2 gap-10"
                    prop.children [

                        portfolioTile 
                            "Code Experiments" 
                            "Play games, read the source, and peek under the hood."
                            (SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppCodeView)
                            dispatch 
                            "primary" 
                            // (TerminalCard())
                            (TerminalTypingAnimation())

                        portfolioTile 
                            "Design & Drawings" 
                            "Visual explorations, anatomy, and creative sketches."
                            (SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppDesignView)
                            dispatch 
                            "secondary" 
                            (DesignGlassCard())
                    ]
                ]
            ]
        ]

    | SharedPortfolioGallery.DesignGallery m ->
        ArtGallery.view m (SharedPortfolioGallery.ArtGalleryMsg >> dispatch)

    | SharedPortfolioGallery.CodeGallery m ->
        CodeGallery.view m (SharedPortfolioGallery.CodeGalleryMsg >> dispatch)