module Components.FSharp.Services.Landing

open Feliz
open Bindings.LucideIcon
open SharedViewModule.WebAppView
open Elmish
open Client.Components.Shop.Common.Ui.Animations

type Industry =
    | Contractor
    | Lawyer
    | Doctor
    | InsuranceAgency
    | RealEstate
    | Retail
    | Restaurant
    | SmallBusiness
    | Other

type Msg =
    | LoadApp of AppView
    | GoToSection of ProfessionalServicesView
    
type Model = {
    CurrentSection: ProfessionalServicesView
}

let getInitialModel section = { CurrentSection = section }

let update msg model =
    match msg with
    | LoadApp _ -> model, Cmd.none
    | GoToSection section -> { model with CurrentSection = section }, Cmd.none



[<ReactComponent>]
let ServicesLanding (dispatch: Msg -> unit) =
    Html.div [
        prop.children [
            
            // HERO
            Html.section [
                prop.className "pt-28 pb-20 px-6 md:px-8 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto text-center"
                        prop.children [
                            Client.Components.Shop.Common.Ui.Section.headerTagArea
                                (LucideIcon.CircleDollarSign "w-4 h-4 opacity-60")
                                "SERVICES"
                        ]
                    ]
                ]
            ]


            Html.div [
                prop.className "hero-image mb-16 md:mb-24 rounded-2xl overflow-hidden"
                prop.children [
                    Html.img [
                        prop.src ("../../img/daria-nepriakhina-planning-unsplash.jpg")
                        prop.alt "About hero"
                        prop.className "w-full h-80 md:h-96 object-cover"
                    ]
                ]
            ]

            
            Html.section [
                prop.className "pt-28 pb-20 px-6 md:px-8 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto text-center"
                        prop.children [
                            Html.p [
                                prop.className "text-sm opacity-60 max-w-3xl mx-auto leading-loose"
                                prop.text
                                    "From full-stack development to AI-powered automation, I help teams build systems that work. Whether you need a complete product built from scratch or want to optimize existing workflows, I bring a pragmatic approach focused on shipping quality software that solves real problems."
                            ]
                        ]
                    ]
                ]
            ]

            // FEATURED SERVICES
            ProgressiveReveal {
                Children =
                    Html.section [
                        prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
                        prop.children [
                            Html.div [
                                prop.className "max-w-6xl mx-auto"
                                prop.children [
                                    Html.p [
                                        prop.className "section-label text-center mb-12"
                                        prop.text "FEATURED OFFERINGS"
                                    ]

                                    Html.div [
                                        prop.className "grid lg:grid-cols-2 gap-8 mb-16"

                                        prop.children [

                                            // Featured: AI Solutions
                                            Html.div [
                                                prop.className "featured-service cursor-pointer"
                                                prop.onClick (fun e -> e.stopPropagation(); GoToSection AI |> dispatch)
                                                prop.children [
                                                    Html.div [
                                                        prop.className "text-4xl mb-6 opacity-60"
                                                        prop.text "🤖"
                                                    ]
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-2xl md:text-3xl font-light mb-4"
                                                        prop.text "AI Solutions & LLM Integration"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                                                        prop.text
                                                            "Embed AI agents and workflows into your existing tools. From ChatGPT integrations to custom-trained models, I help you leverage AI where it actually makes sense—automating repetitive tasks, enhancing user experiences, and unlocking insights from your data."
                                                    ]
                                                    Html.div [
                                                        prop.className "flex flex-wrap gap-2 mb-6 text-xs"
                                                        prop.children [
                                                            for tag in
                                                                [ 
                                                                    "OpenAI / Anthropic APIs"
                                                                    "LangChain"
                                                                    "Vector Databases"
                                                                    "Fine-tuning"
                                                                    "RAG Systems" ] do
                                                                Html.span [
                                                                    prop.className "tech-badge"
                                                                    prop.text tag
                                                                ]
                                                        ]
                                                    ]
                                                    Html.button [
                                                        prop.className "cta-btn text-[0.7rem]"
                                                        prop.text "Learn More →"
                                                        prop.onClick (fun e -> e.stopPropagation(); GoToSection AI |> dispatch)
                                                    ]
                                                ]
                                            ]

                                            // Featured: Automation
                                            Html.div [
                                                prop.className "featured-service cursor-pointer"
                                                prop.onClick (fun e -> e.stopPropagation(); GoToSection Automation |> dispatch)
                                                prop.children [
                                                    Html.div [
                                                        prop.className "text-4xl mb-6 opacity-60"
                                                        prop.text "⚙️"
                                                    ]
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-2xl md:text-3xl font-light mb-4"
                                                        prop.text "Automation & Workflows"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                                                        prop.text
                                                            "Codify your best processes into automations so your team can focus on exceptions, not routine. I build cross-system workflows that handle everything from data sync to approval chains, reducing manual clicks by 40–70% and cutting error rates in half."
                                                    ]
                                                    Html.div [
                                                        prop.className "flex flex-wrap gap-2 mb-6 text-xs"
                                                        prop.children [
                                                            for tag in
                                                                [ 
                                                                    "Zapier / Make"
                                                                    "n8n"
                                                                    "Custom APIs"
                                                                    "ETL Pipelines"
                                                                    "Event-Driven" ] do
                                                                Html.span [
                                                                    prop.className "tech-badge"
                                                                    prop.text tag
                                                                ]
                                                        ]
                                                    ]
                                                    Html.button [
                                                        prop.className "cta-btn text-[0.7rem]"
                                                        prop.text "Learn More →"
                                                        prop.onClick (fun e -> e.stopPropagation(); GoToSection Automation |> dispatch)
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
            }
            

            // HOW I WORK
            Html.section [
                prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto"
                        prop.children [
                            Html.p [
                                prop.className "section-label text-center mb-8"
                                prop.text "APPROACH"
                            ]
                            Html.h2 [
                                prop.className "cormorant-font text-3xl md:text-4xl lg:text-5xl font-light text-center mb-12"
                                prop.text "How I Engage"
                            ]

                            Html.div [
                                prop.className "grid lg:grid-cols-3 gap-8"
                                prop.children [

                                    let approachCard (title: string) (body: string) bullets =
                                        Html.div [
                                            prop.className "approach-card"
                                            prop.children [
                                                Html.h4 [
                                                    prop.className "cormorant-font text-2xl font-light mb-4"
                                                    prop.text title
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text body
                                                ]
                                                Html.ul [
                                                    prop.className "space-y-2 text-xs opacity-50"
                                                    prop.children [
                                                        for b in bullets do Html.li ("• " + b)
                                                    ]
                                                ]
                                            ]
                                        ]

                                    approachCard
                                        "Discovery & Design"
                                        "We start by mapping your current process and identifying opportunities. I deliver a clear implementation plan with realistic timelines and costs."
                                        [ 
                                            "Current process mapping"
                                            "Automation opportunity report"
                                            "Prioritized roadmap" ]

                                    approachCard
                                        "Build & Iterate"
                                        "I build in focused sprints, delivering working software every 1-2 weeks. You see progress early and often, with room to course-correct as we go."
                                        [ 
                                            "Workflow design & implementation"
                                            "System integrations & data sync"
                                            "Testing & refinement" ]

                                    approachCard
                                        "Support & Optimize"
                                        "After launch, I provide ongoing support to ensure everything runs smoothly. We monitor performance and continuously optimize for better results."
                                        [ 
                                            "Ongoing tuning & monitoring"
                                            "New workflow rollouts"
                                            "Quarterly optimization reviews" ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            

            // ALL SERVICES GRID
            Html.section [
                prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto"
                        prop.children [
                            Html.p [
                                prop.className "section-label text-center mb-12"
                                prop.text "ALL CAPABILITIES"
                            ]

                            ProgressiveReveal {
                                Children =
                                Html.div [
                                    prop.className "grid sm:grid-cols-2 lg:grid-cols-3 gap-8"
                                    prop.children [

                                        

                                        // Web Development
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection Website |> dispatch)
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "🌐"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "Web Development"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "Building responsive and performant web applications."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "React"
                                                        Html.span "•"
                                                        Html.span "TypeScript"
                                                        Html.span "•"
                                                        Html.span "F#"
                                                        Html.span "•"
                                                        Html.span "Tailwind"
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // UI/UX Design → also Website
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection Website |> dispatch)
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "🎨"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "UI/UX Design"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "Designing clear, user-friendly interfaces and flows."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "Figma"
                                                        Html.span "•"
                                                        Html.span "Design Systems"
                                                        Html.span "•"
                                                        Html.span "Prototyping"
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // E-Commerce Sites → Website
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection Website |> dispatch)
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "🛒"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "E-Commerce Sites"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "Online stores and product pages that actually convert."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "Shopify"
                                                        Html.span "•"
                                                        Html.span "Stripe"
                                                        Html.span "•"
                                                        Html.span "Printful"
                                                        Html.span "•"
                                                        Html.span "CS-Cart"
                                                        Html.span "•"
                                                        Html.span "Magento"
                                                        Html.span "•"
                                                        Html.span "Custom"
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // Sales & CRM Platforms → SalesPlatform
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection SalesPlatform |> dispatch)

                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "📊"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "Sales & CRM Platforms"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "CRM, pipelines, and automations that support your sales motion."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "HubSpot"
                                                        Html.span "•"
                                                        Html.span "Salesforce"
                                                        Html.span "•"
                                                        Html.span "Custom CRM"
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // LLM Training & Tuning → AI
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection AI |> dispatch)
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "📚"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "LLM Training & Tuning"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "Training and tuning LLMs around your data and processes."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "Fine-tuning"
                                                        Html.span "•"
                                                        Html.span "Prompt Engineering"
                                                        Html.span "•"
                                                        Html.span "RAG"
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // Software Integration → Integration
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection Integration |> dispatch)
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "🔗"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "Software Integration"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "Connecting CRMs, ERPs, EMRs, and other core systems."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "REST APIs"
                                                        Html.span "•"
                                                        Html.span "Webhooks"
                                                        Html.span "•"
                                                        Html.span "OAuth"
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // API Development → Integration
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection Integration |> dispatch)
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "🔌"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "API Development"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "Designing and implementing robust APIs for your platform."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "GraphQL"
                                                        Html.span "•"
                                                        Html.span "REST"
                                                        Html.span "•"
                                                        Html.span "gRPC"
                                                        Html.span "•"
                                                        Html.span "OpenAPI"
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // Cloud & Platform Delivery → Development
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection Development |> dispatch)
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "☁️"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "Cloud & Platform Delivery"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "Deploying and running your applications in the cloud."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "Azure"
                                                        Html.span "•"
                                                        Html.span "AWS"
                                                        Html.span "•"
                                                        Html.span "Docker"
                                                        Html.span "•"
                                                        Html.span "K8s"
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // Analytics & Reporting → Development
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection Development |> dispatch)
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "📊"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "Analytics & Reporting"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "Dashboards and reporting for your product or operations."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "PowerBI"
                                                        Html.span "•"
                                                        Html.span "Custom Dashboards"
                                                        Html.span "•"
                                                        Html.span "SQL"
                                                    ]
                                                ]
                                            ]
                                        ]

                                        // Performance & Security → Development
                                        Html.div [
                                            prop.className "service-card"
                                            prop.onClick (fun _ -> GoToSection Development |> dispatch)
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-3xl mb-4 opacity-60"
                                                    prop.text "⚡"
                                                ]
                                                Html.h3 [
                                                    prop.className "cormorant-font text-xl font-light mb-3"
                                                    prop.text "Performance & Security"
                                                ]
                                                Html.p [
                                                    prop.className "text-xs opacity-60 mb-4 leading-relaxed"
                                                    prop.text "Hardening, profiling, and tuning existing applications."
                                                ]
                                                Html.div [
                                                    prop.className "flex flex-wrap gap-2 text-xs opacity-50"
                                                    prop.children [
                                                        Html.span "Load Testing"
                                                        Html.span "•"
                                                        Html.span "Security Audits"
                                                        Html.span "•"
                                                        Html.span "Optimization"
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            }
                        ]
                    ]
                ]
            ]

            // TECHNOLOGY STACK
            Html.section [
                prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto"
                        prop.children [
                            Html.p [
                                prop.className "section-label text-center mb-8"
                                prop.text "TECHNOLOGY STACK"
                            ]
                            Html.h2 [
                                prop.className "cormorant-font text-3xl md:text-4xl lg:text-5xl font-light text-center mb-12"
                                prop.text "Built with Modern, Proven Tools"
                            ]
                            Html.p [
                                prop.className "text-sm opacity-60 text-center max-w-3xl mx-auto mb-16 leading-loose"
                                prop.text
                                    "I believe in using the right tool for the job. Here's what I typically reach for when building systems that need to be fast, maintainable, and scalable."
                            ]

                            Html.div [
                                prop.className "grid sm:grid-cols-2 lg:grid-cols-4 gap-6"
                                prop.children [

                                    let stackCard (title: string) (items: string list) =
                                        Html.div [
                                            prop.className "approach-card text-center"
                                            prop.children [
                                                Html.h4 [
                                                    prop.className "cormorant-font text-lg font-light mb-3 opacity-80"
                                                    prop.text title
                                                ]
                                                Html.div [
                                                    prop.className "space-y-2 text-xs opacity-60"
                                                    prop.children [
                                                        for i in items do Html.p i
                                                    ]
                                                ]
                                            ]
                                        ]

                                    stackCard "Frontend" [ "React / Feliz"; "TypeScript"; "Tailwind CSS"; "DaisyUI" ]
                                    stackCard "Backend" [ "F# / Fable"; "Node.js"; "Python"; "PostgreSQL" ]
                                    stackCard "Cloud & DevOps" [ "Azure"; "Digital Ocean"; "Docker"; "Kubernetes" ]
                                    stackCard "AI & Automation" [ "OpenAI API"; "LangChain"; "n8n"; "Zapier" ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // CTA
            ProgressiveReveal {
                Children = 
                    Html.section [
                        prop.className "py-20 px-4 sm:px-6 lg:px-12 mb-24"
                        prop.children [
                            Html.div [
                                prop.className "max-w-4xl mx-auto text-center"
                                prop.children [
                                    Html.h2 [
                                        prop.className "cormorant-font text-3xl md:text-4xl lg:text-5xl font-light mb-8"
                                        prop.text "Ready to start building?"
                                    ]
                                    Html.p [
                                        prop.className "text-sm opacity-60 mb-12 leading-loose"
                                        prop.text
                                            "Let's talk about your project. I offer free initial consultations to discuss your needs and see if we're a good fit."
                                    ]
                                    Html.div [
                                        prop.className "flex flex-col sm:flex-row gap-6 justify-center"
                                        prop.children [
                                            Html.button [
                                                prop.className "cta-btn"
                                                prop.text "Schedule a Consultation"
                                                prop.onClick (fun _ ->
                                                    // adjust target view if you prefer a different contact entry
                                                    LoadApp ContactAppView |> dispatch
                                                )
                                            ]
                                            Html.button [
                                                prop.className "cta-btn"
                                                prop.text "View Past Projects"
                                                prop.onClick (fun _ ->
                                                    LoadApp PortfolioAppLandingView |> dispatch
                                                )
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
            }
        ]
    ]

[<ReactComponent>]
let View (model: Model) (dispatch: Msg -> unit) =
    React.useEffectOnce (fun () ->
        let el = Browser.Dom.document.getElementById("inner-main-content")
        if not (isNull el) then
            el.scrollTop <- 0.0
    )

    Html.main [
        prop.className "w-full"
        prop.children [

            match model.CurrentSection with
            | ServicesLanding -> ServicesLanding dispatch
            | _ ->
                // Back button aligned with service details
                Html.div [
                    prop.className "max-w-6xl mx-auto px-4 lg:px-8 pt-6 md:pt-8 pb-4"
                    prop.children [
                        Html.button [
                            prop.className "
                                inline-flex items-center gap-2
                                px-4 py-2
                                rounded-xl
                                backdrop-blur
                                bg-base-content/5
                                border border-base-content/15
                                text-xs md:text-sm
                                uppercase tracking-[0.18em]
                                text-base-content/80
                                shadow-sm
                                hover:shadow-md
                                hover:-translate-y-0.5
                                transition-all duration-200
                                mb-6
                            "
                            prop.onClick (fun _ ->
                                GoToSection ProfessionalServicesView.ServicesLanding |> dispatch
                            )
                            prop.children [
                                Html.span [ prop.text "←" ]
                                Html.span [ prop.text "Back to Services" ]
                            ]
                        ]
                    ]
                ]

                // Detail page content
                match model.CurrentSection with
                | AI            -> Components.FSharp.Service.AiSalesPage ()
                | Automation    -> Components.FSharp.Service.AutomationPage ()
                | Integration   -> Components.FSharp.Service.IntegrationPage ()
                | Website       -> Components.FSharp.Service.WebsitePage ()
                | SalesPlatform -> Components.FSharp.Service.SalesPlatformPage ()
                | Development   -> Components.FSharp.Service.EngineeringPage ()
                | ServicesLanding -> Html.none
        ]
    ]
