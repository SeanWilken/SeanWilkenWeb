module Portfolio

open Elmish
open Feliz
open Shared
open Bindings.LucideIcon


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


type PortfolioSplitCardStyle =
    | SplitCodeCard
    | SplitDesignCard

let styleCardBySplitStyle cardStyle =
    match cardStyle with
    | SplitCodeCard -> "CODE", "generalPortfolioCodeCard"
    | SplitDesignCard -> "DESIGN", "generalPortfolioDesignCard"

// let header (title: string) (blurbs: string list) =
//     Html.div [
//         prop.className "text-center mb-12"
//         prop.children [
//             Html.h1 [ 
//                 prop.className "text-4xl font-bold text-primary"
//                 prop.text title
//             ]
//             for b in blurbs do
//                 Html.h2 [
//                     prop.className "text-lg text-secondary"
//                     prop.text b
//                 ]
//         ]
//     ]


let headerControls dispatch =
    Html.div [
        prop.className "flex justify-between items-center mb-8"
        prop.children [
            Html.button [
                prop.className "btn btn-ghost"
                prop.onClick (fun _ -> dispatch SharedDesignGallery.BackToPortfolio)
                prop.children [
                    LucideIcon.ChevronLeft "w-6 h-6"
                    Html.span "Back"
                ]
            ]
            Html.a [
                prop.href "https://www.instagram.com/xeroeffort/"
                prop.target "_blank"
                prop.className "btn btn-ghost"
                prop.children [
                    LucideIcon.Instagram "w-6 h-6"
                    Html.span "Instagram"
                ]
            ]
        ]
    ]

[<ReactComponent>]
let TerminalTypingAnimation () =
    Html.div [
        prop.className "bg-black text-green-500 font-mono rounded-lg p-4 max-w-lg mx-auto shadow-md text-sm leading-relaxed border border-green-600"
        prop.children [
            Html.div [
                prop.className "terminal-line typing-line-1"
                prop.text "> dotnet fable watch"
            ]
            Html.div [
                prop.className "terminal-output"
                prop.text "watch mode enabled..."
            ]
            Html.div [
                prop.className "terminal-line typing-line-2"
                prop.text "> pnpm install"
            ]
            Html.div [
                prop.className "terminal-output"
                prop.text "packages installed successfully"
            ]
            Html.div [
                prop.className "terminal-line typing-line-3"
                prop.text "> fake build"
            ]
            Html.div [
                prop.className "terminal-output"
                prop.text "Building... OK\nDeploying to Azure... Done"
            ]
            Html.span [
                prop.className "cursor"
            ]
        ]
    ]

[<ReactComponent>]
let TerminalCard () =
    Html.div [
        prop.className "terminal-card w-full max-w-xl h-60 mx-auto overflow-hidden"
        prop.children [
            Html.div [
                prop.className "terminal-header"
                prop.children [
                    Html.span [ prop.text "░▒▓   …/SeanWilkenWeb   main !?   v24.3.0   11:56 " ]
                ]
            ]
            Html.div [
                prop.className "terminal-body"
                prop.children [
                    Html.div [ prop.text "➜ echo 'Deploying project...'" ]
                    Html.div [ prop.text "Deploying project..." ]
                    Html.div [ prop.text "Uploading files..." ]
                    Html.div [ prop.text "Done ✅" ]
                    Html.div [ 
                        prop.text "➜ "
                        prop.className "cursor-blink"
                    ]
                ]
            ]
        ]
    ]

[<ReactComponent>]
let DesignGlassCard () =
    Html.div [
        prop.className "w-full max-w-xl h-60 mx-auto p-6 rounded-xl flex items-center justify-center glass-card"
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
                        prop.className "card-title text-2xl text-primary"
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
                            prop.text "Explore a collection of my work — from playable code demos to hand-drawn designs and illustrations."
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