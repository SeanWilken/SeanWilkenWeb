module Portfolio

open Elmish
open Feliz
open Shared


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

let header (title: string) (blurbs: string list) =
    Html.div [
        prop.className "text-center mb-12"
        prop.children [
            Html.h1 [ 
                prop.className "text-4xl font-bold text-primary"
                prop.text title
            ]
            for b in blurbs do
                Html.h2 [
                    prop.className "text-lg text-secondary"
                    prop.text b
                ]
        ]
    ]

let portfolioTile (title: string) (description: string) (msg: SharedPortfolioGallery.Msg) (dispatch: SharedPortfolioGallery.Msg -> unit) (bgColor: string) =
    Html.div [
        prop.className "flex flex-col gap-2 items-center text-center p-4 transition-transform hover:scale-105"
        prop.onClick (fun _ -> dispatch msg)
        prop.children [
            Html.div [
                prop.className $"card w-full bg-{bgColor} shadow-lg text-primary-content cursor-pointer"
                prop.children [
                    Html.div [
                        prop.className "card-body"
                        prop.children [
                            Html.h2 [ prop.className "card-title text-2xl"; prop.text title ]
                        ]
                    ]
                ]
            ]
            Html.p [
                prop.className "text-sm text-base-content"
                prop.text description
            ]
        ]
    ]

let view model dispatch =
    match model with
    | SharedPortfolioGallery.PortfolioGallery ->
        Html.div [
            prop.className "max-w-4xl mx-auto py-12 px-4"
            prop.children [
                header "Portfolio" [ "...where to begin..." ]
                Html.div [
                    prop.className "grid grid-cols-1 md:grid-cols-2 gap-8"
                    prop.children [
                        portfolioTile 
                            "Code" 
                            "Play some games & link to read their github gists." 
                            (SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppCodeView) 
                            dispatch 
                            "primary"

                        portfolioTile 
                            "Design" 
                            "Some drawings and designs I've done, or making." 
                            (SharedPortfolioGallery.LoadSection SharedWebAppViewSections.AppView.PortfolioAppDesignView) 
                            dispatch 
                            "secondary"
                    ]
                ]
            ]
        ]

    | SharedPortfolioGallery.DesignGallery m ->
        ArtGallery.view m (SharedPortfolioGallery.ArtGalleryMsg >> dispatch)

    | SharedPortfolioGallery.CodeGallery m ->
        CodeGallery.view m (SharedPortfolioGallery.CodeGalleryMsg >> dispatch)
