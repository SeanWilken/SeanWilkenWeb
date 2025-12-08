module Components.FSharp.About

open System
open Feliz
open Shared
open Bindings.LucideIcon
open Client.Domain
open Components.FSharp.Layout.MultiContent

let tileContents : TileContent list = [
    {
        Title = "Website"
        Summary = "Discover the story behind this site, the technology stack, and the philosophy that drives its design."
        Details = """
            This site is a hands-on showcase of my approach to modern web development. Built from the ground up using the SAFE stack (Saturn, Azure, Fable, Elmish), it demonstrates how functional programming and strong typing can create robust, maintainable applications.

            There is a mix of F# and Typescript components loaded throughout the website, to demonstrate the power of leveraging both languages complementary to one another through bindings.
            
            The deployment pipeline uses FAKE for automated builds and Azure for cloud hosting, ensuring reliability and scalability. Custom layouts, Elmish update loops, and a modular architecture make the site both flexible and easy to extend.
            
            Beyond the technical, this site reflects my commitment to clarity, usability, and developer experience. Every page is crafted to be both informative and interactive, with real code experiments and demos you can launch and explore.
        """
        Icon = LucideIcon.BookOpen "w-6 h-6"
        Image = Some "./img/josh-boak-unsplash-overview.jpg"
    }
    {
        Title = "Industry"
        Summary = "A look at my professional journey, the problems I've solved, and the impact I've made across teams and projects."
        Details = """
            My career spans full-stack development, architecture, and technical leadership. I've built tools that empower teams, designed systems that scale, and shipped products that solve real business challenges.
            
            From collaborating with clients to define requirements, to integrating complex backend services, my work is rooted in understanding both the technical and human sides of software. I thrive in environments where learning is constant and where the right solution matters more than the easy one.
            
            Whether it's mentoring junior developers, leading code reviews, or architecting new features, I bring a focus on quality, communication, and long-term value. My experience covers web, cloud, and desktop, with a passion for automation and developer tooling.
        """
        Icon = LucideIcon.Briefcase "w-6 h-6"
        Image = Some "./img/bernd-dittrich-unsplash-office.jpg"
    }
    {
        Title = "Personal"
        Summary = "Get to know the person behind the code: my interests, adventures, and what drives me outside of work."
        Details = """
            Curiosity and adaptability are at the heart of who I am. I love learning by doing, whether that's sailing across the Caribbean, driving coast-to-coast, or picking up a new technology just for fun.
            
            I believe in the value of challenge: every problem is an opportunity to grow, and every experience adds a new perspective. Outside of coding, you'll find me exploring the world, tinkering with side projects, or kicking back with my friends and family.
            
            My personal philosophy is simple: stay curious, help others, and never stop improving. Life is best lived with a sense of adventure and a willingness to try new things.
        """
        Icon = LucideIcon.UserCircle "w-6 h-6"
        Image = Some "./img/sailing-1.JPG"
    }
]

open System

let private renderSelectedSection (content: TileContent) =
    let paragraphs =
        content.Details.Split(
            [| "\n\n"; "\r\n\r\n" |],
            StringSplitOptions.RemoveEmptyEntries
        )

    Html.section [
        prop.className "mt-10 md:mt-12"
        prop.children [
            Html.div [
                // full card area
                prop.className "rounded-3xl bg-base-100 shadow-xl border border-base-300 overflow-hidden"
                prop.children [

                    // FULL WIDTH IMAGE
                    match content.Image with
                    | Some src ->
                        Html.div [
                            prop.className "relative w-full h-72 md:h-96"
                            prop.children [
                                Html.img [
                                    prop.src src
                                    prop.className "absolute inset-0 w-full h-full object-cover"
                                ]
                                Html.div [
                                    prop.className "absolute bottom-0 inset-x-0 h-24 bg-gradient-to-t from-base-100 via-base-100/40 to-transparent pointer-events-none"
                                ]
                            ]
                        ]
                    | None -> Html.none

                    // CONTENT BELOW IMAGE
                    Html.div [
                        prop.className "px-6 py-8 md:px-10 md:py-10 space-y-6"
                        prop.children [

                            // Tag + Icon
                            Html.div [
                                prop.className "inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-semibold tracking-wide uppercase"
                                prop.children [
                                    content.Icon
                                    Html.span content.Title
                                ]
                            ]

                            // Title
                            Html.h2 [
                                prop.className "text-2xl md:text-3xl font-bold text-base-content"
                                prop.text content.Summary
                            ]

                            // Body paragraphs
                            Html.div [
                                prop.className "space-y-4 text-base-content/80 text-sm md:text-base leading-relaxed"
                                prop.children [
                                    for p in paragraphs do
                                        Html.p p
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]


let tags = [
    "F# / functional design"
    "TypeScript & React"
    "Elmish & state machines"
    "AI & workflow automation"
    "Developer tooling"
    "Data pipelines"
    "Clean, readable UX"
]

[<ReactComponent>]
let view model dispatch =
    let selectedIndex, setSelectedIndex = React.useState(Some 0)

    Html.div [
        prop.className "px-6 py-12 md:py-16 max-w-6xl mx-auto space-y-10"
        prop.children [

            // HERO
            Html.section [
                prop.className "space-y-4 text-center"
                prop.children [
                    Html.span [
                        prop.className "inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-primary/10 text-primary text-xs font-semibold tracking-[0.2em] uppercase"
                        prop.children [
                            LucideIcon.UserCircle "w-4 h-4"
                            Html.span "About"
                        ]
                    ]
                    Html.h1 [
                        prop.className "clash-font text-4xl md:text-5xl font-extrabold text-base-content"
                        prop.text "A developer, a systems thinker, and a perpetual tinkerer."
                    ]
                    Html.p [
                        prop.className "max-w-3xl mx-auto text-sm md:text-base text-base-content/80"
                        prop.text "This site is equal parts portfolio, playground, and living notebook. Here's a bit more about how I work, where I've been, and what keeps me curious."
                    ]

                    // quick stats row
                    Html.div [
                        prop.className "mt-4 flex flex-wrap justify-center gap-4 text-xs md:text-sm text-base-content/70"
                        prop.children [
                            Html.div [
                                prop.className "px-3 py-1 rounded-full bg-base-200 flex items-center gap-2"
                                prop.children [
                                    LucideIcon.Code2 "w-4 h-4 text-primary"
                                    Html.span "≈ 10+ years building products"
                                ]
                            ]
                            Html.div [
                                prop.className "px-3 py-1 rounded-full bg-base-200 flex items-center gap-2"
                                prop.children [
                                    LucideIcon.Cloud "w-4 h-4 text-secondary"
                                    Html.span "F#, TypeScript, SAFE stack, Azure, AI"
                                ]
                            ]
                            Html.div [
                                prop.className "px-3 py-1 rounded-full bg-base-200 flex items-center gap-2"
                                prop.children [
                                    LucideIcon.Compass "w-4 h-4 text-accent"
                                    Html.span "Healthcare, e-commerce, automation and tooling"
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // Selection Row
            Html.section [
                prop.className "flex flex-col md:flex-row gap-6"
                prop.children (
                    tileContents
                    |> List.mapi (fun i content ->
                        Html.div [
                            prop.className "w-full md:w-1/3"
                            prop.children [
                                multiContentNavCard i selectedIndex setSelectedIndex content
                            ]
                        ])
                )
            ]

            // Selected content
            match selectedIndex with
            | Some idx when idx >= 0 && idx < tileContents.Length ->
                renderSelectedSection tileContents[idx]
            | _ ->
                Html.none

            // Extra “themes / tech I enjoy” section
            Html.section [
                prop.className "mt-10 md:mt-12 grid gap-6 md:grid-cols-[minmax(0,1.4fr)_minmax(0,1.6fr)] items-start"
                prop.children [

                    Html.div [
                        prop.className "rounded-2xl bg-base-200/70 border border-base-300 p-6 space-y-3"
                        prop.children [
                            Html.h2 [
                                prop.className "text-lg font-semibold text-base-content flex items-center gap-2"
                                prop.children [
                                    LucideIcon.HeartHandshake "w-5 h-5 text-primary"
                                    Html.span "How I like to work"
                                ]
                            ]
                            Html.p [
                                prop.className "text-sm text-base-content/80"
                                prop.text "I'm happiest when I'm close to the problem, collaborating with people who care, and shipping things that actually get used. Strong opinions, loosely held; clean code, pragmatically applied."
                            ]
                            Html.ul [
                                prop.className "mt-2 space-y-1 text-sm text-base-content/80 list-disc list-inside"
                                prop.children [
                                    Html.li "Translate messy requirements into clear, testable designs."
                                    Html.li "Balance rapid prototyping with a solid foundation."
                                    Html.li "Leave things better structured than I found them."
                                ]
                            ]
                        ]
                    ]

                    Html.div [
                        prop.className "rounded-2xl bg-base-200/50 border border-base-300 p-6 space-y-3"
                        prop.children [
                            Html.h2 [
                                prop.className "text-lg font-semibold text-base-content flex items-center gap-2"
                                prop.children [
                                    LucideIcon.Sparkles "w-5 h-5 text-secondary"
                                    Html.span "Tech & topics I gravitate toward"
                                ]
                            ]
                            Html.div [
                                prop.className "flex flex-wrap gap-2 text-xs"
                                prop.children [
                                    for tag in tags do
                                        Html.span [
                                            prop.className "px-3 py-1 rounded-full bg-base-100 border border-base-300 text-base-content/80"
                                            prop.text tag
                                        ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // CTA to portfolio
            Html.section [
                prop.className "mt-10 md:mt-14 text-center space-y-4"
                prop.children [
                    Html.p [
                        prop.className "text-sm text-base-content/75"
                        prop.text "If you want to see how all of this shows up in code and products, the portfolio is where the fun starts, or dive right into the services offered."
                    ]
                    Html.div [
                        prop.className "btn-group inline-flex gap-4"
                        prop.children [

                            Html.button [
                                prop.className "btn btn-primary btn-md md:btn-lg"
                                prop.text "Explore projects & demos"
                                prop.onClick (fun _ ->
                                    dispatch (
                                        SharedWebAppModels.WebAppMsg.SwitchToOtherApp
                                            SharedWebAppViewSections.PortfolioAppLandingView
                                    ))
                            ]
                            Html.button [
                                prop.className "btn btn-primary btn-md md:btn-lg"
                                prop.text "Explore services offered"
                                prop.onClick (fun _ ->
                                    dispatch (
                                        SharedWebAppModels.WebAppMsg.SwitchToOtherApp (SharedWebAppViewSections.ProfessionalServicesAppView SharedWebAppViewSections.ServicesLanding)
                                    ))
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
