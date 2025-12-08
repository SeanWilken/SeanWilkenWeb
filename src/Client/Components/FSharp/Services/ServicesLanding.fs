module Components.FSharp.Services.Landing

open Feliz
open Shared
open Bindings.LucideIcon
open Client.Domain.SharedWebAppModels
open Client.Domain.SharedWebAppViewSections
open Components.FSharp.Layout.MultiContent

[<ReactComponent>]
let view (model: Client.Domain.SharedServices.Model) (dispatch: WebAppMsg -> unit) =
    React.useEffectOnce (fun () ->
        let el = Browser.Dom.document.getElementById("inner-main-content")
        if not (isNull el) 
        then el.scrollTop <- 0.0
    )

    Html.div [
        prop.children [
            
            if not (model.CurrentSection = ServicesLanding)
            then
                Html.button [
                    prop.className "
                        mb-10
                        inline-flex items-center gap-2
                        px-4 py-2
                        rounded-xl
                        backdrop-blur
                        bg-base-content/10
                        border border-base-content/20
                        text-base-content
                        shadow-sm
                        hover:shadow-md
                        hover:-translate-y-0.5
                        transition-all duration-200
                    "
                    prop.onClick (fun _ -> SwitchToOtherApp(ProfessionalServicesAppView ServicesLanding) |> dispatch)
                    prop.children [
                        Html.span [ prop.text "←" ]
                        Html.span [ prop.text "Back to Services" ]
                    ]
                ]


            match model.CurrentSection with
            | ServicesLanding ->
                Components.Layout.LayoutElements.SectionGrid {
                    Title = "🛠️ Services 🛠️"
                    Items = [
                        // WEBSITE
                        { Heading = "Web Development"; Icon = "🌐"; Description = "Building responsive and performant web applications."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView Website) |> dispatch }
                        { Heading = "UI/UX Design"; Icon = "🎨"; Description = "Designing clear, user-friendly interfaces and flows."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView Website) |> dispatch }
                        { Heading = "E-Commerce Sites"; Icon = "🛒"; Description = "Online stores and product pages that actually convert."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView Website) |> dispatch }

                        // SALES PLATFORM
                        { Heading = "Sales & CRM Platforms"; Icon = "📈"; Description = "CRM, pipelines, and automations that support your sales motion."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView SalesPlatform) |> dispatch }

                        // AI
                        { Heading = "AI Solutions"; Icon = "🤖"; Description = "AI agents and workflows embedded into your existing tools."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView AI) |> dispatch }
                        { Heading = "LLM Training & Tuning"; Icon = "📚"; Description = "Training and tuning LLMs around your data and processes."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView AI) |> dispatch }

                        // AUTOMATION
                        { Heading = "Automation & Workflows"; Icon = "⚙️"; Description = "Automating multi-step business processes across systems."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView Automation) |> dispatch }

                        // INTEGRATION
                        { Heading = "Software Integration"; Icon = "🔗"; Description = "Connecting CRMs, ERPs, EMRs, and other core systems."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView Integration) |> dispatch }
                        { Heading = "API Development"; Icon = "🔃"; Description = "Designing and implementing robust APIs for your platform."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView Integration) |> dispatch }

                        // DEVELOPMENT / PLATFORM
                        { Heading = "Cloud & Platform Delivery"; Icon = "☁️"; Description = "Deploying and running your applications in the cloud."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView Development) |> dispatch }
                        { Heading = "Analytics & Reporting"; Icon = "📊"; Description = "Dashboards and reporting for your product or operations."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView Development) |> dispatch }
                        { Heading = "Performance & Security"; Icon = "⚡"; Description = "Hardening, profiling, and tuning existing applications."; NavigateTo = fun _ -> SwitchToOtherApp(ProfessionalServicesAppView Development) |> dispatch }
                    ]
                }

            | AI             -> Components.FSharp.Service.AiSalesPage ()
            | Automation     -> Components.FSharp.Service.AutomationPage ()
            | Integration    -> Components.FSharp.Service.IntegrationPage ()
            | Website        -> Components.FSharp.Service.WebsitePage ()
            | SalesPlatform  -> Components.FSharp.Service.SalesPlatformPage ()
            | Development    -> Components.FSharp.Service.EngineeringPage ()

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