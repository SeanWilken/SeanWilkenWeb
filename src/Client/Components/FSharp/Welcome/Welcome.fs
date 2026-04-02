module Components.FSharp.Welcome

open Feliz
open Bindings.LucideIcon
open Components.Layout.LayoutElements
open SharedViewModule
open Client.Components.Shop.Common.Ui.Animations

// ---------- Types ----------
type Msg =
    | SwitchSection of WebAppView.AppView

type ExploreTileProps = {
    ImgSrc      : string
    Section     : WebAppView.AppView
    Description : string
    Dispatch    : Msg -> unit
    ButtonLabel : string
}

type WelcomeProps = {| dispatch: Msg -> unit |}

// ---------- Explore tile (card) ----------
[<ReactComponent>]
let ExploreTile (props: ExploreTileProps) =
    let title = WebAppView.appSectionStringTitle props.Section

    Html.div [
        prop.className "explore-card"
        prop.children [
            Html.div [
                prop.className "aspect-4/3 overflow-hidden bg-neutral-100"
                prop.children [
                    Html.img [
                        prop.src props.ImgSrc
                        prop.alt title
                        prop.className "w-full h-full object-cover"
                    ]
                ]
            ]
            Html.div [
                prop.className "p-6"
                prop.children [
                    Html.h3 [
                        prop.className "cormorant-font text-xl sm:text-2xl font-medium mb-2"
                        prop.text title
                    ]
                    Html.p [
                        prop.className "text-md sm:text-md opacity-60 mb-6 leading-relaxed"
                        prop.text props.Description
                    ]
                    Html.button [
                        prop.className "card-btn"
                        prop.text props.ButtonLabel
                        prop.onClick (fun _ -> props.Dispatch (SwitchSection props.Section))
                    ]
                ]
            ]
        ]
    ]

// ---------- Hero section ----------
[<ReactComponent>]
let WelcomeHero (props: WelcomeProps) =
    Html.section [
        prop.className "gradient-dark-to-light min-h-[72svh] md:min-h-[78svh] flex items-center justify-center pt-24 md:pt-28 px-4 sm:px-6 lg:px-8 w-full"
        prop.children [
            Html.div [
                prop.className "max-w-5xl mx-auto text-center w-full inter-font"
                prop.children [
                    Html.h2 [
                        prop.className "cormorant-font text-base sm:text-lg lg:text-xl tracking-[0.18em] uppercase mb-5 md:mb-6"
                        prop.text "SEAN WILKEN"
                    ]
                    Html.h1 [
                        prop.className "text-4xl sm:text-5xl md:text-6xl lg:text-7xl xl:text-8xl text-base-content/60  font-light mb-6 sm:mb-8 leading-tight"
                        prop.text "Senior Software Engineer"
                    ]
                    Html.p [
                        prop.className "text-base sm:text-lg sm:mb-12 lg:text-lg opacity-80 mb-8 max-w-2xl mx-auto leading-relaxed px-4"
                        prop.text "I build full-stack applications, backend services, and workflow-driven systems that support real products and operational teams."
                    ]
                    Html.div [
                        prop.className "flex flex-col sm:flex-row gap-4 justify-center px-4"
                        prop.children [
                            Html.div [
                                prop.children [
                                    Html.button [
                                        prop.className "premium-btn w-full sm:w-auto"
                                        prop.text "View Portfolio"
                                        prop.onClick (fun _ ->
                                            props.dispatch (SwitchSection WebAppView.AppView.PortfolioAppLandingView)
                                        )
                                    ]
                                ]
                            ]
                            Html.div [
                                prop.children [
                                    Html.button [
                                        prop.className "premium-btn w-full sm:w-auto"
                                        prop.text "View Resume"
                                        prop.onClick (fun _ ->
                                            props.dispatch (SwitchSection WebAppView.AppView.ResumeAppView)
                                        )
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// ---------- “Explore the Site” section ----------
[<ReactComponent>]
let ExploreSection (props: WelcomeProps) =
    Html.section [
        prop.className "inter-font py-16 sm:py-20 lg:py-32 px-4 sm:px-6 lg:px-8 bg-base-200 w-full"
        prop.children [
            Html.div [
                prop.className "max-w-5xl mx-auto"
                prop.children [

                    // Section title
                    Html.div [
                        prop.className "text-center mb-12 sm:mb-16 lg:mb-20"
                        prop.children [
                            Html.h2 [
                                prop.className "cormorant-font text-3xl sm:text-4xl lg:text-5xl font-light section-title inline-block"
                                prop.text "Start Here"
                            ]
                        ]
                    ]

                    // Cards grid
                    Html.div [
                        prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 lg:gap-8"
                        prop.children [

                            ScrollReveal {|
                                Variant   = FadeIn
                                Delay     = 0.08
                                Threshold = 0.3
                                Children = 
                                    ExploreTile {
                                        ImgSrc      = "https://seanwilken.com/img/joao-ferrao-resume-unsplash.jpg"
                                        Section     = WebAppView.AppView.ResumeAppView
                                        Description = "Resume: Professional experience, technical strengths, and selected accomplishments."
                                        Dispatch    = props.dispatch
                                        ButtonLabel = "View Resume"
                                    }
                            |}

                            ScrollReveal {|
                                Variant   = FadeIn
                                Delay     = 0.08
                                Threshold = 0.3
                                Children = 
                                    ExploreTile {
                                        ImgSrc      = "https://seanwilken.com/img/walkator-unsplash-code.jpg"
                                        Section     = WebAppView.AppView.PortfolioAppCodeView
                                        Description = "Code: Engineering-focused portfolio work, experiments, and implementation details."
                                        Dispatch    = props.dispatch
                                        ButtonLabel = "Explore Code"
                                    }
                            |}

                            ScrollReveal {|
                                Variant   = FadeIn
                                Delay     = 0.08
                                Threshold = 0.3
                                Children = 
                                    ExploreTile {
                                        ImgSrc      = "https://seanwilken.com/img/ales-nesetril-unsplash-dev.jpg"
                                        Section     = WebAppView.AppView.AboutAppView
                                        Description = "About: Who I am, what I build, and the kind of work I enjoy doing."
                                        Dispatch    = props.dispatch
                                        ButtonLabel = "Learn More"
                                    }
                            |}

                            ScrollReveal {|
                                Variant   = FadeIn
                                Delay     = 0.08
                                Threshold = 0.3
                                Children = 
                                    ExploreTile {
                                        ImgSrc      = "https://seanwilken.com/img/ian-schneider-services-unsplash.jpg"
                                        Section     = WebAppView.AppView.ProfessionalSkillsAppView WebAppView.ProfessionalSkillsView.SkillsLanding
                                        Description = "Skills: Engineering strengths across frontend, backend, workflow systems, and delivery."
                                        Dispatch    = props.dispatch
                                        ButtonLabel = "Review Skills"
                                    }
                            |}

                            ScrollReveal {|
                                Variant   = FadeIn
                                Delay     = 0.08
                                Threshold = 0.3
                                Children = 
                                    ExploreTile {
                                        ImgSrc      = "https://seanwilken.com/img/ann-artroom-unsplash.jpg"
                                        Section     = WebAppView.AppView.PortfolioAppDesignView
                                        Description = "Designs: UI explorations, visual work, and selected creative projects."
                                        Dispatch    = props.dispatch
                                        ButtonLabel = "Browse Designs"
                                    }
                            |}

                            ScrollReveal {|
                                Variant   = FadeIn
                                Delay     = 0.08
                                Threshold = 0.3
                                Children = 
                                    ExploreTile {
                                        ImgSrc      = "https://seanwilken.com/img/nikola-duza-unsplash-shop.jpg"
                                        Section     = WebAppView.AppView.ShopAppView
                                        Description = "Shop: Creative products, prints, and apparel connected to my projects."
                                        Dispatch    = props.dispatch
                                        ButtonLabel = "Shop Now"
                                    }
                            |}

                        ]
                    ]
                ]
            ]
        ]
    ]

// ---------- Skills / Stack / Coming Soon sections ----------
[<ReactComponent>]
let SkillsSection (props: WelcomeProps) =
    Html.section [
        prop.className "py-16 sm:py-20 lg:py-32 px-4 sm:px-6 lg:px-8 bg-base-100 w-full"
        prop.children [
            Html.div [
                prop.className "max-w-7xl mx-auto"
                prop.children [
                    SectionGrid {
                        Title = "Engineering Strengths"
                        Items = [
                            {
                                Heading = "Full-Stack Engineering"
                                Icon = "🧩"
                                Description = "Building production applications across frontend, backend, APIs, and deployment."
                                NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalSkillsAppView WebAppView.ProfessionalSkillsView.FullStack) |> props.dispatch
                            }
                            {
                                Heading = "Backend APIs & Systems"
                                Icon = "🛠️"
                                Description = "Custom servers, validation pipelines, document workflows, and client-facing APIs."
                                NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalSkillsAppView WebAppView.ProfessionalSkillsView.Backend) |> props.dispatch
                            }
                            {
                                Heading = "Frontend Development"
                                Icon = "🖥️"
                                Description = "Responsive interfaces, typed client architecture, and maintainable UI systems."
                                NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalSkillsAppView WebAppView.ProfessionalSkillsView.Frontend) |> props.dispatch
                            }
                            {
                                Heading = "Workflow Automation"
                                Icon = "⚙️"
                                Description = "Stateful processing, validation, notifications, and operational tooling."
                                NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalSkillsAppView WebAppView.ProfessionalSkillsView.Automation) |> props.dispatch
                            }
                            {
                                Heading = "AI & LLM Integrations"
                                Icon = "🤖"
                                Description = "Document analysis, retrieval-based workflows, and AI-assisted product features."
                                NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalSkillsAppView WebAppView.ProfessionalSkillsView.AI) |> props.dispatch
                            }
                            {
                                Heading = "Leadership & Mentorship"
                                Icon = "🧭"
                                Description = "Hands-on technical leadership, mentoring engineers, and guiding implementation quality."
                                NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalSkillsAppView WebAppView.ProfessionalSkillsView.Leadership) |> props.dispatch
                            }
                            {
                                Heading = "Cloud & Platform Delivery"
                                Icon = "☁️"
                                Description = "Containers, CI/CD, cloud infrastructure, and operational reliability."
                                NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalSkillsAppView WebAppView.ProfessionalSkillsView.PlatformDelivery) |> props.dispatch
                            }
                        ]
                    }
                ]
            ]
        ]
    ]

[<ReactComponent>]
let FocusAreaSection (_: WelcomeProps) =
    Html.section [
        prop.className "py-16 sm:py-20 lg:py-32 px-4 sm:px-6 lg:px-8 bg-base-100 w-full"
        prop.children [
            Html.div [
                prop.className "max-w-6xl mx-auto"
                prop.children [
                    SectionCarousel {| 
                        Title = "Where I've Applied These Skills"
                        Items = [
                            {
                                IconElement = LucideIcon.HeartPulse "w-6 h-6"
                                Title       = "Healthcare & Regulated Systems"
                                Description = "Workflow-heavy applications, document processing, validation systems, and platform work in regulated environments."
                            }
                            {
                                IconElement = LucideIcon.ShoppingCart "w-6 h-6"
                                Title       = "E-Commerce Platforms"
                                Description = "Storefronts, product systems, fulfillment flows, and operational tooling for commerce-focused applications."
                            }
                            {
                                IconElement = LucideIcon.Briefcase "w-6 h-6"
                                Title       = "Internal Tools & Operations"
                                Description = "Systems that support intake, reporting, processing, review, and day-to-day operational workflows."
                            }
                            {
                                IconElement = LucideIcon.Activity "w-6 h-6"
                                Title       = "Real-Time Applications"
                                Description = "Live updates, interactive experiences, and state-aware workflows backed by responsive application logic."
                            }
                        ]
                    |}
                ]
            ]
        ]
    ]

// ---------- Root Welcome View ----------
[<ReactComponent>]
let Welcome (props: WelcomeProps) =
    Html.main [
        prop.className "w-full"
        prop.children [

            WelcomeHero props
            
            ExploreSection props
            
            ScrollReveal {|
                Variant   = ScaleUp
                Delay     = 0.08
                Threshold = 0.3
                Children = SkillsSection props
            |}
            
            ScrollReveal {|
                Variant   = FadeUp
                Delay     = 0.08
                Threshold = 0.45
                Children = FocusAreaSection props
            |}
        ]
    ]

[<ReactComponent>]
let View (dispatch: Msg -> unit) : ReactElement =
    Welcome {| dispatch = dispatch |}
