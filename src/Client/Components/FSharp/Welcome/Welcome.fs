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
                        prop.src "./imgs/Harlot.jpeg"
                        prop.className "max-w-sm rounded-full shadow-2xl w-48 h-48 object-cover mb-6 lg:mb-0"
                        prop.alt "Profile Picture"
                    ]
                    Html.div [
                        prop.className "text-center lg:text-left"
                        prop.children [
                            Html.h1 [
                                prop.className "clash-font text-5xl font-bold"
                                prop.text "Welcome"
                            ]
                            Html.p [
                                prop.className "py-4 text-lg"
                                prop.text "Feel free to explore my site, check out some of my projects, art or get in touch!"
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
                        "/img/ales-nesetril-unsplash-dev.jpg" 
                        SharedWebAppViewSections.AboutAppView 
                        "About: Learn more about the site and its purpose." 
                        dispatch

                    indexTile 
                        "/img/walkator-unsplash-code.jpg" 
                        SharedWebAppViewSections.PortfolioAppCodeView 
                        "Code: Check out some mini games or code gists." 
                        dispatch

                    indexTile 
                        "/img/nikola-duza-unsplash-shop.jpg" 
                        SharedWebAppViewSections.PortfolioAppDesignView 
                        "Designs: Check out some drawings I've done recently." 
                        dispatch

                    indexTile
                        // "/img/mike-meyers-unsplash-contact.jpg" 
                        "/img/jakub-zerdzicki-unsplash-contact.jpg"
                        SharedWebAppViewSections.ContactAppView 
                        "Contact: Let's hear it already!" 
                        dispatch
                ]
            ]
        ]
    ]

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
                            { Heading = "Web Development"; Icon = "🌐"; Description = "Building responsive and performant web applications." }
                            { Heading = "UI/UX Design"; Icon = "🎨"; Description = "Creating user-friendly interfaces and experiences." }
                            { Heading = "Software Integration"; Icon = "🔗"; Description = "Connecting different software systems and APIs." }
                            { Heading = "E-Commerce"; Icon = "🛒"; Description = "Developing online stores and payment solutions." }
                            { Heading = "AI Solutions"; Icon = "🤖"; Description = "Implementing AI and machine learning technologies." }
                            { Heading = "LLM Training"; Icon = "📚"; Description = "Training large language models for specific tasks." }
                            { Heading = "Cloud Deployment"; Icon = "☁️"; Description = "Deploying applications to cloud platforms." }
                            { Heading = "Analytics"; Icon = "📊"; Description = "Managing and optimizing data analytics processes." }
                            { Heading = "API Development"; Icon = "🔃"; Description = "Creating and maintaining APIs for applications." }
                            { Heading = "Performance Optimization"; Icon = "⚡"; Description = "Improving application performance and speed." }
                            { Heading = "Security Enhancements"; Icon = "🔒"; Description = "Implementing security best practices and measures." }
                            { Heading = "Maintenance & Support"; Icon = "🛠️"; Description = "Providing ongoing maintenance and support services." }
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
                ]
            ]
        ]
    ]