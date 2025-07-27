module AboutSection

open Feliz
open Shared
open Bindings.LucideIcon
open Shared.SharedWebAppViewSections


type TileContent = {
    Title: string
    Summary: string
    Details: string
    Icon: ReactElement
    Image: string option
}

let tileContents: TileContent list = [
    {
        Title = "Website"
        Summary = "Learn how this site was built and why."
        Details = """
            This site was designed from scratch with the SAFE stack. It's deployed with FAKE to Azure and integrates custom layouts and Elmish update loops.
            It's a full example of my design, development, and deployment workflow.
        """
        Icon = LucideIcon.BookOpen "w-6 h-6"
        Image = Some "/img/josh-boak-unsplash-overview.jpg"
    }
    {
        Title = "Industry"
        Summary = "My experience and professional background."
        Details = """
            I've worked across the stack — building tools, solving architecture problems, and shipping code. From handling client requirements to implementing backend integrations.
        """
        Icon = LucideIcon.Briefcase "w-6 h-6"
        Image = Some "/img/bernd-dittrich-unsplash-office.jpg"
    }
    {
        Title = "Personal"
        Summary = "Some personal details if you're curious."
        Details = """
            I'm adaptable, curious, and love learning by doing. I've sailed across the Caribbean and driven coast-to-coast more than once.
            I value problem solving and growing through challenge.
        """
        Icon = LucideIcon.UserCircle "w-6 h-6"
        Image = Some "/img/sailing-1.JPG"
    }
]

let aboutNavCard index selectedIndex setSelectedIndex (content: TileContent) =
    let isActive = selectedIndex = Some index
    Html.div [
        prop.className (
            "card bg-base-100 shadow-md transition-all hover:shadow-xl p-4 text-left h-full " +
            (if isActive then "ring-2 ring-primary scale-[1.02]" else "")
        )
        prop.onClick (fun _ -> setSelectedIndex (Some index))
        prop.children [
            Html.div [
                prop.className "flex items-center gap-3 text-primary mb-2"
                prop.children [
                    content.Icon
                    Html.h2 [ prop.className "card-title text-xl"; prop.text content.Title ]
                ]
            ]
            Html.p [
                prop.className "text-sm text-base-content/80"
                prop.text content.Summary
            ]
        ]
    ]

let aboutContentDisplay (content: TileContent option) =
    match content with
    | None -> Html.none
    | Some content ->
        Html.div [
            prop.className "mt-12 grid grid-cols-1 md:grid-cols-2 gap-8 items-center bg-base-200 p-8 rounded-xl shadow-inner"
            prop.children [
                // Left side: Image
                match content.Image with
                | Some url ->
                    Html.img [
                        prop.src url
                        prop.alt $"{content.Title} image"
                        prop.className "rounded-lg w-full max-h-64 object-cover border border-base-300 shadow"
                    ]
                | None -> Html.none

                // Right side: Text content
                Html.div [
                    prop.className "space-y-4"
                    prop.children [
                        Html.h2 [
                            prop.className "text-2xl font-bold text-primary"
                            prop.text content.Title
                        ]
                        Html.p [
                            prop.className "text-base text-base-content"
                            prop.text content.Details
                        ]
                    ]
                ]
            ]
        ]
[<ReactComponent>]
let view model dispatch =
    let selectedIndex, setSelectedIndex = React.useState(Some 0)

    Html.div [
        prop.className "px-6 py-16 max-w-6xl mx-auto"
        prop.children [
            Html.h1 [
                prop.className "text-4xl font-bold text-center mb-12 text-primary"
                prop.text "About"
            ]

            // Selection Row
            Html.div [
                prop.className "flex flex-col md:flex-row gap-6 mb-12"
                prop.children (
                    tileContents
                    |> List.mapi (fun i content ->
                        Html.div [
                            prop.className "w-full md:w-1/3"
                            prop.children [
                                aboutNavCard i selectedIndex setSelectedIndex content
                            ]
                        ]
                    )
                )
            ]

            // Content / Fallback Section
            match selectedIndex with
            | Some idx when idx < tileContents.Length ->
                Html.div [
                    prop.className "mt-16"
                    prop.children [
                        aboutContentDisplay (Some tileContents[idx])
                    ]
                ]
            | _ ->
                Html.div [
                    prop.className "mt-16 bg-base-200 p-10 rounded-xl shadow-inner text-center space-y-8"
                    prop.children [
                        Html.h2 [
                            prop.className "text-3xl font-extrabold text-primary"
                            prop.text "Want to see what I've built?"
                        ]

                        Html.p [
                            prop.className "text-base-content/80 max-w-xl mx-auto"
                            prop.text "Explore my projects, demos, and source code — from interactive games to full-stack tools."
                        ]

                        Html.div [
                            prop.className "flex flex-wrap gap-4 justify-center items-center pt-4"
                            prop.children [
                                Html.img [
                                    prop.src "/img/project-1-thumb.jpg"
                                    prop.className "rounded-lg max-h-24 w-auto object-cover shadow"
                                    prop.alt "Project preview"
                                ]
                                Html.img [
                                    prop.src "/img/project-2-thumb.jpg"
                                    prop.className "rounded-lg max-h-24 w-auto object-cover shadow"
                                    prop.alt "Project preview"
                                ]
                            ]
                        ]

                        Html.button [
                            prop.className "btn btn-primary btn-lg"
                            prop.text "Explore Projects"
                            prop.onClick (fun _ -> dispatch (Shared.SharedWebAppModels.WebAppMsg.SwitchToOtherApp SharedWebAppViewSections.PortfolioAppLandingView))
                        ]
                    ]
                ]
        ]
    ]