module Components.FSharp.About

open Feliz
open Elmish
open Bindings.LucideIcon
open SharedViewModule.WebAppView
open Client.Components.Shop.Common.Ui.Animations

type Msg =
    | ToggleContent of int
    | SwitchSection of AppView

type Model = {
    ActiveIndex : int
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
        Title = "How I Work"
        Summary = "How I approach software development, technical decisions, and building maintainable systems."
        Details = """
            I tend to do my best work where product needs, system design, and implementation all meet. I like getting close to the problem, understanding the real constraints, and turning rough requirements into software that is clear, maintainable, and actually useful.

            My background spans full-stack applications, backend APIs, workflow-heavy systems, and user-facing product work. I care a lot about structure, naming, and keeping complexity manageable, especially in codebases that need to evolve over time.

            I am most comfortable in environments where I can contribute across layers: shaping architecture, building features, refining workflows, and improving how a system behaves in practice. I value strong technical foundations, but I try to stay pragmatic about tradeoffs and delivery.
        """
        Icon = LucideIcon.BookOpen "w-10 h-10 mx-auto mb-6 opacity-60"
        Image = Some "https://seanwilken.com/img/josh-boak-unsplash-overview.jpg"
    }
    {
        Title = "Professional Background"
        Summary = "A look at the kinds of systems I've built and the problems I've worked on professionally."
        Details = """
            My work has primarily focused on full-stack engineering, backend systems, and workflow-driven applications across healthcare, e-commerce, and client platforms. I have worked on regulated healthcare software, custom backend services, validation and submission workflows, real-time systems, and user-facing web applications.

            A lot of the work I enjoy sits at the intersection of product and systems: building APIs, shaping data and process flows, improving application structure, and helping teams deliver software that is both useful and maintainable.

            I have also spent a meaningful part of my career helping guide implementation, mentoring other engineers, and improving technical direction when projects needed stronger structure or clearer execution.
        """
        Icon = LucideIcon.Briefcase "w-10 h-10 mx-auto mb-6 opacity-60"
        Image = Some "https://seanwilken.com/img/bernd-dittrich-unsplash-office.jpg"
    }
    {
        Title = "Beyond Work"
        Summary = "A bit about the curiosity, interests, and mindset I bring outside of software."
        Details = """
            Outside of engineering, I tend to be drawn to things that involve exploration, making, and figuring things out by doing. That same curiosity shows up in how I approach side projects, game ideas, hardware concepts, design work, and new technologies.

            I enjoy building things even when there is no immediate reason to, whether that means experimenting with a new stack, refining a project idea, or chasing down a problem just because it is interesting. A lot of my best learning has come from that kind of hands-on exploration.

            More broadly, I try to stay adaptable, keep learning, and remain open to challenge. I value growth, good people, and work that feels thoughtful rather than purely performative.
        """
        Icon = LucideIcon.UserCircle "w-10 h-10 mx-auto mb-6 opacity-60"
        Image = Some "https://seanwilken.com/img/sailing-1.JPG"
    }
]

let tags = [
    "F# and typed systems"
    "TypeScript and React"
    "Backend APIs"
    "Workflow-heavy applications"
    "AI-assisted tooling"
    "Developer tooling"
    "Maintainable UI systems"
]

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
                            Html.span "Full-stack engineer with a focus on:"
                            Html.br [ prop.className "hidden sm:block" ]
                            Html.span "systems, workflows, and maintainable software"
                        ]
                    ]

                    Html.p [
                        prop.className "text-center text-sm opacity-60 mb-16 max-w-3xl mx-auto leading-loose"
                        prop.text "This site brings together my professional experience, technical interests, and ongoing experiments. It is a place to see how I think about software, the kinds of systems I like building, and some of the work behind that."
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
                                        prop.text "10+ years building software"
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.className "stat-badge rounded-full"
                                prop.children [
                                    LucideIcon.Cloud "w-5 h-5 opacity-60"
                                    Html.span [
                                        prop.className "opacity-80 text-xs sm:text-sm"
                                        prop.text "F#, TypeScript, React, Python, C#"
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.className "stat-badge rounded-full"
                                prop.children [
                                    LucideIcon.Compass "w-5 h-5 opacity-60"
                                    Html.span [
                                        prop.className "opacity-80 text-xs sm:text-sm"
                                        prop.text "Healthcare, e-commerce, workflows, and tooling"
                                    ]
                                ]
                            ]
                        ]
                    ]

                    Html.div [
                        prop.className "hero-image mb-16 md:mb-24 rounded-2xl overflow-hidden"
                        prop.children [
                            Html.img [
                                prop.src (tileContents[selectedIndex].Image |> Option.defaultValue "https://seanwilken.com/img/josh-boak-unsplash-overview.jpg")
                                prop.alt "About hero"
                                prop.className "w-full h-80 md:h-96 object-cover"
                            ]
                        ]
                    ]

                    Html.div [
                        prop.className "grid md:grid-cols-3 gap-6 mb-16 md:mb-24"
                        prop.children [
                            tileContents
                            |> List.mapi (
                                fun i tile ->
                                    Html.div [
                                        prop.className "info-card text-center cursor-pointer"
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
                                        prop.text "I work best when I am close to the problem, collaborating with thoughtful people, and building things that actually get used. I care about clarity, maintainability, and making practical progress without overcomplicating the solution."
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
                                                    Html.span "Balance rapid iteration with strong technical foundations."
                                                ]
                                            ]
                                            Html.li [
                                                prop.className "flex items-start gap-2"
                                                prop.children [
                                                    Html.span [ prop.className "opacity-60 mt-1"; prop.text "•" ]
                                                    Html.span "Leave systems more structured and maintainable than I found them."
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
                        prop.text "If you want to see how this shows up in practice, take a look at my portfolio work, resume, or skills breakdown."
                    ]
                    Html.div [
                        prop.className "flex flex-col sm:flex-row gap-4 sm:gap-6 justify-center"
                        prop.children [
                            Html.button [
                                prop.className "cta-btn"
                                prop.text "Explore projects & Demos"
                                prop.onClick (fun _ ->
                                    dispatch (SwitchSection PortfolioAppLandingView)
                                )
                            ]
                            Html.button [
                                prop.className "cta-btn"
                                prop.text "View Skills Breakdown"
                                prop.onClick (fun _ ->
                                    dispatch (SwitchSection (ProfessionalSkillsAppView SkillsLanding))
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
