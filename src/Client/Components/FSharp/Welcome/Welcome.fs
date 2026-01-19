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
}

type WelcomeProps = {| dispatch: Msg -> unit |}

// ---------- Explore tile (card) ----------

[<ReactComponent>]
let ExploreTile (props: ExploreTileProps) =
    let title = WebAppView.appSectionStringTitle props.Section

    Html.div [
        // scroll-fade classes hook into the IntersectionObserver CSS from your mockup
        prop.className "explore-card scroll-fade"
        prop.children [
            Html.div [
                prop.className "aspect-[4/3] overflow-hidden bg-neutral-100"
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
                        prop.className "text-xs sm:text-sm opacity-60 mb-6 leading-relaxed"
                        prop.text props.Description
                    ]
                    Html.button [
                        prop.className "card-btn"
                        prop.text "Explore"
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
        prop.className "gradient-dark-to-light min-h-[70vh] flex items-center justify-center pt-24 px-4 sm:px-6 lg:px-8 w-full"
        prop.children [
            Html.div [
                prop.className "max-w-4xl mx-auto text-center w-full"
                prop.children [
                    Html.p [
                        prop.className "text-xs sm:text-sm tracking-[0.2em] uppercase opacity-60 mb-6"
                        prop.text "SEAN WILKEN"
                    ]
                    Html.h1 [
                        prop.className "cormorant-font text-4xl sm:text-5xl md:text-6xl lg:text-7xl xl:text-8xl font-light mb-6 sm:mb-8 leading-tight"
                        prop.text "FUNCTIONAL FORGE"
                    ]
                    Html.p [
                        prop.className "text-sm sm:text-base lg:text-lg opacity-80 mb-8 sm:mb-12 max-w-2xl mx-auto leading-relaxed px-4"
                        prop.text "Discover where minimal effort meets maximum style. A curated collection of projects and services designed for those who value simplicity without compromising on quality."
                    ]
                    Html.div [
                        prop.className "flex flex-col sm:flex-row gap-4 justify-center px-4"
                        prop.children [
                            Html.button [
                                prop.className "premium-btn"
                                prop.text "Explore Work"
                                // Send them into the Explore section – About is a good “first stop”
                                prop.onClick (fun _ ->
                                    props.dispatch (SwitchSection WebAppView.AppView.PortfolioAppLandingView)
                                )
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
        prop.className "py-16 sm:py-20 lg:py-32 px-4 sm:px-6 lg:px-8 bg-base-200 w-full"
        prop.children [
            Html.div [
                prop.className "max-w-7xl mx-auto"
                prop.children [

                    // Section title
                    Html.div [
                        prop.className "text-center mb-12 sm:mb-16 lg:mb-20"
                        prop.children [
                            Html.h2 [
                                prop.className "cormorant-font text-3xl sm:text-4xl lg:text-5xl font-light section-title inline-block"
                                prop.text "Explore the Site"
                            ]
                        ]
                    ]

                    // Cards grid
                    Html.div [
                        prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 lg:gap-8"
                        prop.children [

                            ExploreTile {
                                ImgSrc      = "./img/ales-nesetril-unsplash-dev.jpg"
                                Section     = WebAppView.AppView.AboutAppView
                                Description = "About: Learn more about the site and its purpose."
                                Dispatch    = props.dispatch
                            }

                            ExploreTile {
                                ImgSrc      = "./img/ian-schneider-services-unsplash.jpg"
                                Section     = WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.ServicesLanding
                                Description = "Services: Learn more about what you can expect of me."
                                Dispatch    = props.dispatch
                            }

                            ExploreTile {
                                ImgSrc      = "./img/walkator-unsplash-code.jpg"
                                Section     = WebAppView.AppView.PortfolioAppCodeView
                                Description = "Code: Check out some mini games or code gists."
                                Dispatch    = props.dispatch
                            }

                            ExploreTile {
                                ImgSrc      = "./img/ann-artroom-unsplash.jpg"
                                Section     = WebAppView.AppView.PortfolioAppDesignView
                                Description = "Designs: Check out some drawings I've done recently."
                                Dispatch    = props.dispatch
                            }

                            ExploreTile {
                                ImgSrc      = "./img/nikola-duza-unsplash-shop.jpg"
                                Section     = WebAppView.AppView.ShopAppView
                                Description = "Shop: Designs + Product = Store."
                                Dispatch    = props.dispatch
                            }

                            ExploreTile {
                                ImgSrc      = "./img/joao-ferrao-resume-unsplash.jpg"
                                Section     = WebAppView.AppView.ResumeAppView
                                Description = "Resume: See what I've been up to professionally."
                                Dispatch    = props.dispatch
                            }

                        ]
                    ]
                ]
            ]
        ]
    ]

// ---------- Services / Stack / Coming Soon sections ----------

[<ReactComponent>]
let ServicesSection (props: WelcomeProps) =
    Html.section [
        prop.className "py-16 sm:py-20 lg:py-32 px-4 sm:px-6 lg:px-8 bg-base-100 w-full"
        prop.children [
            Html.div [
                prop.className "max-w-7xl mx-auto"
                prop.children [

                    // Use your existing SectionGrid component for the cards
                    SectionGrid {
                        Title = "🛠️ Services 🛠️"
                        Items = [
                            // WEBSITE
                            { Heading = "Web Development"; Icon = "🌐"; Description = "Building responsive and performant web applications."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.Website) |> props.dispatch }
                            { Heading = "UI/UX Design";   Icon = "🎨"; Description = "Designing clear, user-friendly interfaces and flows."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.Website) |> props.dispatch }
                            { Heading = "E-Commerce Sites"; Icon = "🛒"; Description = "Online stores and product pages that actually convert."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.Website) |> props.dispatch }

                            // SALES PLATFORM
                            { Heading = "Sales & CRM Platforms"; Icon = "📈"; Description = "CRM, pipelines, and automations that support your sales motion."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.SalesPlatform) |> props.dispatch }

                            // AI
                            { Heading = "AI Solutions"; Icon = "🤖"; Description = "AI agents and workflows embedded into your existing tools."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.AI) |> props.dispatch }
                            { Heading = "LLM Training & Tuning"; Icon = "📚"; Description = "Training and tuning LLMs around your data and processes."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.AI) |> props.dispatch }

                            // AUTOMATION
                            { Heading = "Automation & Workflows"; Icon = "⚙️"; Description = "Automating multi-step business processes across systems."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.Automation) |> props.dispatch }

                            // INTEGRATION
                            { Heading = "Software Integration"; Icon = "🔗"; Description = "Connecting CRMs, ERPs, EMRs, and other core systems."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.Integration) |> props.dispatch }
                            { Heading = "API Development";      Icon = "🔃"; Description = "Designing and implementing robust APIs for your platform."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.Integration) |> props.dispatch }

                            // DEVELOPMENT / PLATFORM
                            { Heading = "Cloud & Platform Delivery"; Icon = "☁️"; Description = "Deploying and running your applications in the cloud."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.Development) |> props.dispatch }
                            { Heading = "Analytics & Reporting";      Icon = "📊"; Description = "Dashboards and reporting for your product or operations."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.Development) |> props.dispatch }
                            { Heading = "Performance & Security";     Icon = "⚡"; Description = "Hardening, profiling, and tuning existing applications."; NavigateTo = fun _ -> SwitchSection(WebAppView.AppView.ProfessionalServicesAppView WebAppView.ProfessionalServicesView.Development) |> props.dispatch }
                        ]
                    }
                ]
            ]
        ]
    ]

[<ReactComponent>]
let SiteStackSection (_: WelcomeProps) =
    Html.section [
        prop.className "py-16 sm:py-20 lg:py-32 px-4 sm:px-6 lg:px-8 bg-base-200 w-full"
        prop.children [
            Html.div [
                prop.className "max-w-6xl mx-auto"
                prop.children [

                    SectionList {
                        Title = "🖥️ Site Stack 🖥️"
                        Items = [
                            "F#"
                            "Fable"
                            "TypeScript"
                            "Tailwind"
                            "DaisyUI"
                            "Azure"
                            "Digital Ocean"
                            "Kubernetes"
                        ]
                    }
                ]
            ]
        ]
    ]

[<ReactComponent>]
let ComingSoonSection (_: WelcomeProps) =
    Html.section [
        prop.className "py-16 sm:py-20 lg:py-32 px-4 sm:px-6 lg:px-8 bg-base-100 w-full"
        prop.children [
            Html.div [
                prop.className "max-w-6xl mx-auto"
                prop.children [
                    SectionCarousel {| 
                        Title = "📰 Coming Soon 📰"
                        Items = [
                            {
                                IconElement = LucideIcon.BrainCircuit "w-6 h-6"
                                Title       = "Site AI Chat"
                                Description = "Chatbot and AI services to assist you in finding what you're looking for."
                            }
                            {
                                IconElement = LucideIcon.ShoppingCart "w-6 h-6"
                                Title       = "Integration with Webshop"
                                Description = "Integration with webshops to sell prints, digital art, and other merchandise."
                            }
                            {
                                IconElement = LucideIcon.JoyStick "w-6 h-6"
                                Title       = "More Interactive Demos"
                                Description = "More interactive code demos and mini-games."
                            }
                            {
                                IconElement = LucideIcon.MegaPhone "w-6 h-6"
                                Title       = "Testimonials"
                                Description = "User testimonials and success stories."
                            }
                            {
                                IconElement = LucideIcon.BookOpenText "w-6 h-6"
                                Title       = "Blog"
                                Description = "Insights, tutorials, and updates on what's cooking."
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
    Html.div [
        // Let each section control its own width/background; this keeps layout clean & responsive
        prop.className "flex flex-col w-full"
        prop.children [
            WelcomeHero props
            
            ScrollReveal {|
                Variant   = FadeIn
                Delay     = 0.08
                Threshold = 0.45
                Children = ExploreSection props
            |}
            ScrollReveal {|
                Variant   = ScaleUp
                Delay     = 0.08
                Threshold = 0.45
                Children = ServicesSection props
            |}
            ScrollReveal {|
                Variant   = SlideRight
                Delay     = 0.08
                Threshold = 0.45
                Children = SiteStackSection props
            |}
            ScrollReveal {|
                Variant   = FadeUp
                Delay     = 0.08
                Threshold = 0.45
                Children = ComingSoonSection props
            |}
        ]
    ]

// Keep backwards-compatible entry point for Elmish-style calls
[<ReactComponent>]
let View (dispatch: Msg -> unit) : ReactElement =
    Welcome {| dispatch = dispatch |}
