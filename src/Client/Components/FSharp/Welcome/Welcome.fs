module Components.FSharp.Welcome

open Feliz
open Shared
open SharedWelcome
open Bindings.LucideIcon

let indexTile (imgSrc: string) (webAppSection: SharedWebAppViewSections.AppView) (descrip: string) (dispatch: Msg -> unit) =
    let sectionButtonTitle = SharedWebAppViewSections.appSectionStringTitle webAppSection
    Html.div [
        prop.className "card w-80 bg-base-100 shadow-xl hover:scale-105 transition-transform duration-300"
        prop.children [
            Html.figure [
                Html.img [
                    prop.src imgSrc
                    prop.className "rounded-t-lg object-cover h-48 w-full"
                    prop.alt sectionButtonTitle
                ]
            ]
            Html.div [
                prop.className "card-body"
                prop.children [
                    Html.h2 [
                        prop.className "card-title"
                        prop.text sectionButtonTitle
                    ]
                    Html.p descrip
                    Html.div [
                        prop.className "card-actions"
                        prop.children [
                            Html.button [
                                prop.className "btn btn-primary w-full"
                                prop.text "Explore"
                                prop.onClick (fun _ -> dispatch (SwitchSection webAppSection))
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let viewMain (dispatch: Msg -> unit) =
    Html.div [
        prop.className "hero bg-gradient-to-br from-primary to-secondary text-base-100 py-16 rounded-xl shadow-xl"
        prop.children [
            Html.div [
                prop.className "hero-content flex-col lg:flex-row-reverse"
                prop.children [
                    Html.img [
                        prop.src "./img/SeanWilkenProfile.png"
                        prop.className "max-w-sm rounded-full shadow-2xl w-48 h-48 object-cover mb-6 lg:mb-0"
                        prop.alt "Profile Picture"
                    ]
                    Html.div [
                        prop.className "text-center lg:text-left"
                        prop.children [
                            Html.h1 [
                                prop.className "clash-font text-5xl font-bold"
                                prop.text "Welcome to the Functional Forge"
                            ]
                            Html.p [
                                prop.className "py-4 text-lg"
                                prop.text "Feel free to explore my site, check out some projects, mini-games, art or get in touch!"
                            ]
                            Html.button [
                                prop.className "btn btn-accent btn-lg"
                                prop.text "Start Exploring"
                                prop.onClick (fun _ -> dispatch (SwitchSection SharedWebAppViewSections.AboutAppView))
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let view2 (dispatch: Msg -> unit) =
    Html.div [
        prop.className "py-16 px-4 max-w-7xl mx-auto text-base-content"
        prop.children [
            Html.h2 [
                prop.className "text-4xl font-bold text-center mb-12"
                prop.text "Explore the Site"
            ]
            Html.div [
                prop.className "grid gap-8 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 justify-items-center"
                prop.children [
                    indexTile 
                        "./img/ales-nesetril-unsplash-dev.jpg" 
                        SharedWebAppViewSections.AboutAppView 
                        "About: Learn more about the site and its purpose." 
                        dispatch

                    indexTile 
                        "./img/walkator-unsplash-code.jpg" 
                        SharedWebAppViewSections.PortfolioAppCodeView 
                        "Code: Check out some mini games or code gists." 
                        dispatch

                    indexTile 
                        "./img/nikola-duza-unsplash-shop.jpg" 
                        SharedWebAppViewSections.PortfolioAppDesignView 
                        "Designs: Check out some drawings I've done recently." 
                        dispatch

                    indexTile
                        // "./img/mike-meyers-unsplash-contact.jpg" 
                        "./img/jakub-zerdzicki-unsplash-contact.jpg"
                        SharedWebAppViewSections.ContactAppView 
                        "Contact: Let's hear it already!" 
                        dispatch
                ]
            ]
        ]
    ]

open Shared.SharedWebAppModels
open Shared.SharedWebAppViewSections

let view (dispatch: Msg -> unit) =
    Html.div [
        prop.className "flex flex-col items-center space-y-3 p-6"
        prop.children [
            viewMain dispatch
            view2 dispatch
            Html.div [
                prop.className "w-full max-w-6xl mx-auto px-6"
                prop.children [

                    Components.Layout.LayoutElements.SectionGrid {
                        Title = "🛠️ Services 🛠️"
                        Items = [
                            // WEBSITE
                            { Heading = "Web Development"; Icon = "🌐"; Description = "Building responsive and performant web applications."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView Website) |> dispatch }
                            { Heading = "UI/UX Design"; Icon = "🎨"; Description = "Designing clear, user-friendly interfaces and flows."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView Website) |> dispatch }
                            { Heading = "E-Commerce Sites"; Icon = "🛒"; Description = "Online stores and product pages that actually convert."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView Website) |> dispatch }

                            // SALES PLATFORM
                            { Heading = "Sales & CRM Platforms"; Icon = "📈"; Description = "CRM, pipelines, and automations that support your sales motion."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView SalesPlatform) |> dispatch }

                            // AI
                            { Heading = "AI Solutions"; Icon = "🤖"; Description = "AI agents and workflows embedded into your existing tools."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView AI) |> dispatch }
                            { Heading = "LLM Training & Tuning"; Icon = "📚"; Description = "Training and tuning LLMs around your data and processes."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView AI) |> dispatch }

                            // AUTOMATION
                            { Heading = "Automation & Workflows"; Icon = "⚙️"; Description = "Automating multi-step business processes across systems."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView Automation) |> dispatch }

                            // INTEGRATION
                            { Heading = "Software Integration"; Icon = "🔗"; Description = "Connecting CRMs, ERPs, EMRs, and other core systems."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView Integration) |> dispatch }
                            { Heading = "API Development"; Icon = "🔃"; Description = "Designing and implementing robust APIs for your platform."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView Integration) |> dispatch }

                            // DEVELOPMENT / PLATFORM
                            { Heading = "Cloud & Platform Delivery"; Icon = "☁️"; Description = "Deploying and running your applications in the cloud."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView Development) |> dispatch }
                            { Heading = "Analytics & Reporting"; Icon = "📊"; Description = "Dashboards and reporting for your product or operations."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView Development) |> dispatch }
                            { Heading = "Performance & Security"; Icon = "⚡"; Description = "Hardening, profiling, and tuning existing applications."; NavigateTo = fun _ -> SwitchSection(ProfessionalServicesAppView Development) |> dispatch }
                        ]
                    }
                    
                    Components.Layout.LayoutElements.SectionList {
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

                    Components.Layout.LayoutElements.SectionCarousel {|
                        Title = "📰 Coming Soon 📰"
                        Items = [
                            {
                                IconElement = LucideIcon.BrainCircuit "w-6 h-6"
                                Title = "Site AI Chat"
                                Description = "Chatbot and AI Services to assist you to help you find what you are looking for here"
                            }
                            {
                                IconElement = LucideIcon.ShoppingCart "w-6 h-6"
                                Title = "Integration with Webshop"
                                Description = "Integration with webshop(s) to sell prints, digital art, and other merchandise"
                            }
                            {
                                IconElement = LucideIcon.JoyStick "w-6 h-6"
                                Title = "More Interactive Demos"
                                Description = "More interactive code demos and mini-games"
                            }
                            {
                                IconElement = LucideIcon.MegaPhone "w-6 h-6"
                                Title = "Testimonials"
                                Description = "User testimonials and success stories"
                            }
                            {
                                IconElement = LucideIcon.BookOpenText "w-6 h-6"
                                Title = "Blog"
                                Description = "Insights, tutorials, and updates on what's cooking"
                            }
                        ]
                    |}

                ]
            ]
        ]
    ]