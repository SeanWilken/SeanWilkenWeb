module Components.FSharp.About

open Feliz
open Elmish
open Bindings.LucideIcon
open SharedViewModule.WebAppView
open Client.Components.Shop.Common.Ui.Animations


// ----------------------------------------------------------------------
// Content (unchanged from your original – just reused in new layout)
// ----------------------------------------------------------------------
type Msg =
    | ToggleContent of int
    | SwitchSection of AppView

type Model = {
    ActiveIndex : int
}

// add images
// revert back to string list for bullet points?
type ModalContent = {
    Title: string
    Image: string option
    MainContent: string
    PreviousLabel: string
    NextLabel: string
}

let getInitialModel = { ActiveIndex = 0 }

let update msg model =
    match msg with
    | ToggleContent index -> { model with ActiveIndex = index }, Cmd.none
    | SwitchSection _ -> model, Cmd.none

type TileContent =
    {
        Title   : string
        Summary : string
        Details : string
        Icon    : ReactElement
        Image   : string option
    }

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
        Icon = LucideIcon.BookOpen "w-10 h-10 mx-auto mb-6 opacity-60"
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
        Icon = LucideIcon.Briefcase "w-10 h-10 mx-auto mb-6 opacity-60"
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
        Icon = LucideIcon.UserCircle "w-10 h-10 mx-auto mb-6 opacity-60"
        Image = Some "./img/sailing-1.JPG"
    }
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

// Helper to pull the “Personal” long text for the personal section
let private personalDetailsText =
    tileContents
    |> List.tryFind (fun t -> t.Title = "Personal")
    |> Option.map (fun t -> t.Details.Trim())
    |> Option.defaultValue """
Curiosity and adaptability are at the heart of who I am. I love learning by doing, whether that's sailing across the Caribbean, driving coast-to-coast, or picking up a new technology just for fun. I believe in the value of challenge: every problem is an opportunity to grow, and every experience adds a new perspective.

Outside of coding, you'll find me exploring the world, tinkering with side projects, or kicking back with my friends and family. My personal philosophy is simple: stay curious, help others, and never stop improving. Life is best lived with a sense of adventure and a willingness to try new things.
"""

// ----------------------------------------------------------------------
// React components, matching the new mockup
// ----------------------------------------------------------------------

[<ReactComponent>]
let AboutHero selectedIndex setSelectedIndex =
    Html.section [
        prop.className "pt-28 pb-20 px-6 md:px-8 lg:px-12"
        prop.children [
            Html.div [
                prop.className "max-w-6xl mx-auto"
                prop.children [

                    // About badge (uses .about-badge CSS from mockup)
                    Html.div [
                        prop.className "flex justify-center mb-12 md:mb-16"
                        prop.children [
                            Html.div [
                                prop.className "about-badge text-[0.65rem] tracking-[0.2em]"
                                prop.children [
                                    LucideIcon.UserCircle "w-4 h-4 opacity-60"
                                    Html.span "ABOUT"
                                ]
                            ]
                        ]
                    ]

                    // Main heading
                    Html.h1 [
                        prop.className "cormorant-font text-4xl sm:text-5xl lg:text-6xl font-light text-center mb-8 leading-tight"
                        prop.children [
                            Html.span "A developer, a systems thinker,"
                            Html.br [ prop.className "hidden sm:block" ]
                            Html.span "and a perpetual tinkerer."
                        ]
                    ]

                    Html.p [
                        prop.className "text-center text-sm opacity-60 mb-16 max-w-3xl mx-auto leading-loose"
                        prop.text "This site is equal parts portfolio, playground, and living notebook. Here's a bit more about how I work, where I've been, and what keeps me curious."
                    ]

                    // Stat badges row (uses .stat-badge CSS)
                    Html.div [
                        prop.className "flex flex-wrap justify-center gap-4 mb-20"
                        prop.children [

                            Html.div [
                                prop.className "stat-badge rounded-full"
                                prop.children [
                                    LucideIcon.Code2 "w-5 h-5 opacity-60"
                                    Html.span [
                                        prop.className "opacity-80 text-xs sm:text-sm"
                                        prop.text "10+ years building products"
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.className "stat-badge rounded-full"
                                prop.children [
                                    LucideIcon.Cloud "w-5 h-5 opacity-60"
                                    Html.span [
                                        prop.className "opacity-80 text-xs sm:text-sm"
                                        prop.text "F#, TypeScript, SAFE stack, Azure, AI"
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.className "stat-badge rounded-full"
                                prop.children [
                                    LucideIcon.Compass "w-5 h-5 opacity-60"
                                    Html.span [
                                        prop.className "opacity-80 text-xs sm:text-sm"
                                        prop.text "Healthcare, e-commerce, automation and tooling"
                                    ]
                                ]
                            ]
                        ]
                    ]

                    Html.div [
                        prop.className "hero-image mb-16 md:mb-24 rounded-2xl overflow-hidden"
                        prop.children [
                            Html.img [
                                prop.src (tileContents[selectedIndex].Image |> Option.defaultValue "./img/josh-boak-unsplash-overview.jpg")
                                prop.alt "About hero"
                                prop.className "w-full h-80 md:h-96 object-cover"
                            ]
                        ]
                    ]

                    // Three info cards (Website / Industry / Personal)
                    Html.div [
                        prop.className "grid md:grid-cols-3 gap-6 mb-16 md:mb-24"
                        prop.children [
                            tileContents
                            |> List.mapi (
                                fun i tile ->
                                    Html.div [
                                        prop.className "info-card text-center"
                                        prop.onClick ( fun _ -> setSelectedIndex i )
                                        prop.children [
                                            tile.Icon
                                            Html.h3 [
                                                prop.className "cormorant-font text-2xl font-light mb-3"
                                                prop.text tile.Title
                                            ]
                                            Html.p [
                                                prop.className "text-[0.76rem] sm:text-xs opacity-50 leading-relaxed"
                                                prop.text (tile.Summary)
                                            ]
                                        ]
                                    ]

                            )
                            |> React.fragment
                        ]
                    ]

                ]
            ]
        ]
    ]

[<ReactComponent>]
let AboutPersonalSection (tileContent: TileContent) =
    
    Html.section [
        prop.className "py-5 md:py-16 px-6 md:px-8 lg:px-12"
        prop.children [
            Html.div [
                prop.className "max-w-5xl mx-auto"
                prop.children [
                    ScrollReveal {|
                        Variant   = Snap
                        Delay     = 0.08
                        Threshold = 0.45
                        Children =
                            Html.div [
                                prop.className "content-section mb-12 md:mb-16"
                                prop.children [


                                    Html.div [
                                        prop.className "flex items-center gap-3 mb-6 md:mb-8"
                                        prop.children [
                                            LucideIcon.Info "w-5 h-5 opacity-60"
                                            Html.span [
                                                prop.className "text-[0.7rem] tracking-[0.2em] uppercase opacity-60"
                                                prop.text tileContent.Title
                                            ]
                                        ]
                                    ]

                                    Html.h2 [
                                        prop.className "cormorant-font text-3xl md:text-4xl font-light mb-6 md:mb-8 leading-tight"
                                        prop.text tileContent.Summary
                                    ]

                                    Html.p [
                                        prop.className "text-sm opacity-70 leading-loose whitespace-pre-line"
                                        prop.text tileContent.Details
                                    ]
                                ]
                            ]
                    |}
                ]
            ]
        ]
    ]


[<ReactComponent>]
let AboutWorkAndTech () =
    Html.section [
        prop.className "pb-16 md:pb-20 px-6 md:px-8 lg:px-12"
        prop.children [
            Html.div [
                prop.className "max-w-5xl mx-auto"
                prop.children [
                    Html.div [
                        prop.className "grid lg:grid-cols-2 gap-8 md:gap-10"
                        prop.children [

                            // How I like to work
                            Html.div [
                                prop.className "content-section"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-center gap-3 mb-4 md:mb-6"
                                        prop.children [
                                            LucideIcon.Check "w-5 h-5 opacity-60"
                                            Html.h3 [
                                                prop.className "cormorant-font text-2xl font-light"
                                                prop.text "How I like to work"
                                            ]
                                        ]
                                    ]
                                    Html.p [
                                        prop.className "text-[0.78rem] sm:text-xs opacity-60 mb-4 md:mb-6 leading-relaxed"
                                        prop.text "I'm happiest when I'm close to the problem, collaborating with people who care, and shipping things that actually get used. Strong opinions, loosely held. Clean code, pragmatically applied."
                                    ]
                                    Html.ul [
                                        prop.className "space-y-3 text-[0.78rem] sm:text-xs opacity-70"
                                        prop.children [
                                            Html.li [
                                                prop.className "flex items-start gap-2"
                                                prop.children [
                                                    Html.span [ prop.className "opacity-60 mt-1"; prop.text "•" ]
                                                    Html.span "Translate messy requirements into clear, testable designs."
                                                ]
                                            ]
                                            Html.li [
                                                prop.className "flex items-start gap-2"
                                                prop.children [
                                                    Html.span [ prop.className "opacity-60 mt-1"; prop.text "•" ]
                                                    Html.span "Balance rapid prototyping with a solid foundation."
                                                ]
                                            ]
                                            Html.li [
                                                prop.className "flex items-start gap-2"
                                                prop.children [
                                                    Html.span [ prop.className "opacity-60 mt-1"; prop.text "•" ]
                                                    Html.span "Leave things better structured than I found them."
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                            // Tech & topics I gravitate toward
                            Html.div [
                                prop.className "content-section"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-center gap-3 mb-4 md:mb-6"
                                        prop.children [
                                            LucideIcon.Sparkles "w-5 h-5 opacity-60"
                                            Html.h3 [
                                                prop.className "cormorant-font text-2xl font-light"
                                                prop.text "Tech & topics I gravitate toward"
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "flex flex-wrap gap-2"
                                        prop.children [
                                            for tag in tags do
                                                Html.span [
                                                    prop.className "tech-badge rounded-full"
                                                    prop.text tag
                                                ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

[<ReactComponent>]
let AboutCta (dispatch: Msg -> unit) =
    Html.section [
        prop.className "py-16 md:py-20 px-6 md:px-8 lg:px-12 mb-16 md:mb-24"
        prop.children [
            Html.div [
                prop.className "max-w-4xl mx-auto text-center"
                prop.children [
                    Html.p [
                        prop.className "text-sm opacity-60 mb-8 md:mb-10 leading-loose"
                        prop.text "If you want to see how all of this shows up in code and products, the portfolio is where the fun starts, or dive right into the services offered."
                    ]
                    Html.div [
                        prop.className "flex flex-col sm:flex-row gap-4 sm:gap-6 justify-center"
                        prop.children [
                            Html.button [
                                prop.className "cta-btn"
                                prop.text "Explore projects & demos"
                                prop.onClick (fun _ ->
                                    dispatch (SwitchSection PortfolioAppLandingView)
                                )
                            ]
                            Html.button [
                                prop.className "cta-btn"
                                prop.text "Explore services offered"
                                prop.onClick (fun _ ->
                                    dispatch (SwitchSection (ProfessionalServicesAppView ServicesLanding))
                                )
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// ----------------------------------------------------------------------
// Root About view (Elmish entry point)
// ----------------------------------------------------------------------

[<ReactComponent>]
let View (model: Model) (dispatch: Msg -> unit) =
    let activeIndex, sectionDetails =
        if model.ActiveIndex >= 0 && model.ActiveIndex < tileContents.Length
        then model.ActiveIndex, tileContents[model.ActiveIndex]
        else 0, tileContents[0]
    Html.main [
        prop.className "w-full"
        prop.children [
            AboutHero activeIndex ( fun idx -> ToggleContent idx |> dispatch )
            AboutPersonalSection sectionDetails
            ScrollReveal {|
                Variant   = FadeIn
                Delay     = 0.08
                Threshold = 0.45
                Children = AboutWorkAndTech()
            |}
            ScrollReveal {|
                Variant   = SlideLeft
                Delay     = 0.08
                Threshold = 0.45
                Children = AboutCta dispatch
            |}
        ]
    ]
