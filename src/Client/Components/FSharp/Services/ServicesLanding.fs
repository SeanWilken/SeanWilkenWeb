module Components.FSharp.Services.Landing

open Feliz
open Shared
open Bindings.LucideIcon
open Shared.SharedWebAppModels
open Shared.SharedWebAppViewSections
open Components.FSharp.Layout.MultiContent

[<ReactComponent>]
let view (model: Shared.SharedServices.Model) dispatch =
    Html.div [
        prop.className ""
        prop.children [

            // Components.Layout.LayoutElements.SectionGrid {
            //     Title = "🛠️ Services 🛠️"
            //     Items = [
            //         { Heading = "Web Development"; Icon = "🌐"; Description = "Building responsive and performant web applications." }
            //         { Heading = "UI/UX Design"; Icon = "🎨"; Description = "Creating user-friendly interfaces and experiences." }
            //         { Heading = "Software Integration"; Icon = "🔗"; Description = "Connecting different software systems and APIs." }
            //         { Heading = "E-Commerce"; Icon = "🛒"; Description = "Developing online stores and payment solutions." }
            //         { Heading = "AI Solutions"; Icon = "🤖"; Description = "Implementing AI and machine learning technologies." }
            //         { Heading = "LLM Training"; Icon = "📚"; Description = "Training large language models for specific tasks." }
            //         { Heading = "Cloud Deployment"; Icon = "☁️"; Description = "Deploying applications to cloud platforms." }
            //         { Heading = "Analytics"; Icon = "📊"; Description = "Managing and optimizing data analytics processes." }
            //         { Heading = "API Development"; Icon = "🔃"; Description = "Creating and maintaining APIs for applications." }
            //         { Heading = "Performance Optimization"; Icon = "⚡"; Description = "Improving application performance and speed." }
            //         { Heading = "Security Enhancements"; Icon = "🔒"; Description = "Implementing security best practices and measures." }
            //         { Heading = "Maintenance & Support"; Icon = "🛠️"; Description = "Providing ongoing maintenance and support services." }
            //     ]
            // }


            match model.CurrentSection with
            | AI ->
                Components.FSharp.Service.AiSalesPage () // (ServicesMsg >> dispatch)
            | _ ->
                Components.FSharp.Service.AiSalesPage () // (ServicesMsg >> dispatch)
                // Components.FSharp.Services.AIAutomation.AISalesDeck.AiSalesPage (ServicesMsg >> dispatch)

            Html.hr [ prop.className "my-16 border-primary/20" ]

            // Selection Row
            Html.div [
                prop.className "flex flex-col md:flex-row gap-6 mb-12"
                prop.children (

                    Components.Layout.LayoutElements.SectionCarousel {|
                        Title = "Other Professional Services"
                        Items = [
                            {
                                IconElement = LucideIcon.BrainCircuit "w-6 h-6"
                                Title = "AI Services"
                                Description = "Harness the power of AI and agentic networks to help propel your business well into the future"
                            }
                            {
                                IconElement = LucideIcon.ShoppingCart "w-6 h-6"
                                Title = "Integrations"
                                Description = "Connect systems together through API and custom middleware solutions"
                            }
                            {
                                IconElement = LucideIcon.JoyStick "w-6 h-6"
                                Title = "Web Development"
                                Description = "From custom websites to progressive web apps, there's something for everyone"
                            }
                            {
                                IconElement = LucideIcon.MegaPhone "w-6 h-6"
                                Title = "Sale Platform"
                                Description = "How would you like to maintain full creative control to match the dynamcy of your up and coming brand?"
                            }
                        ]
                    |}

                )
            ]

        ]
    ]