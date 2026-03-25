module Components.FSharp.Skills.Landing

open Feliz
open Bindings.LucideIcon
open SharedViewModule.WebAppView
open Elmish
open Client.Components.Shop.Common.Ui.Animations
open Components.Layout.LayoutElements


type Msg =
    | LoadApp of AppView
    | SkillMsg of Components.FSharp.Service.Msg
    | GoToSection of ProfessionalSkillsView
    
type Model = {
    CurrentSection: ProfessionalSkillsView
}

let getInitialModel section = { CurrentSection = section }

let update msg model =
    match msg with
    | LoadApp _ -> model, Cmd.none
    | SkillMsg msg ->
        match msg with
        | Components.FSharp.Service.Msg.NavigateTo view -> model, Cmd.ofMsg (GoToSection view)
    | GoToSection section -> { model with CurrentSection = section }, Cmd.none


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

[<ReactComponent>]
let SkillsLandingSection (dispatch: Msg -> unit) =
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
                                "SKILLS"
                        ]
                    ]
                ]
            ]


            Html.div [
                prop.className "hero-image mb-16 md:mb-24 rounded-2xl overflow-hidden"
                prop.children [
                    Html.img [
                        prop.src ("https://seanwilken.com/img/daria-nepriakhina-planning-unsplash.jpg")
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
                                    "My background spans full-stack application development, backend APIs, workflow-heavy systems, cloud delivery, and AI-enabled product features. Strongest in F#, React, TypeScript, Python, and C#."
                            ]
                        ]
                    ]
                ]
            ]

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
                                        prop.text "CORE AREAS"
                                    ]

                                    Html.div [
                                        prop.className "grid lg:grid-cols-2 gap-8 mb-16"

                                        prop.children [

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
                                                        prop.text "AI & LLM Integrations"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                                                        prop.text
                                                            "Integrating AI-assisted features into real products and workflows, especially for document analysis, retrieval-driven tooling, and human-in-the-loop systems that benefit from contextual assistance."
                                                    ]
                                                    Html.div [
                                                        prop.className "flex flex-wrap gap-2 mb-6 text-xs"
                                                        prop.children [
                                                            for tag in
                                                                [ 
                                                                    "OpenAI API"
                                                                    "RAG"
                                                                    "Document Analysis"
                                                                    "Prompt Design" ] do
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
                                                        prop.text "Workflow Automation"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                                                        prop.text
                                                            "Creating workflow-heavy systems that reduce repetitive work, improve visibility into process state, and support review, correction, and downstream actions where reliability matters."
                                                    ]
                                                    Html.div [
                                                        prop.className "flex flex-wrap gap-2 mb-6 text-xs"
                                                        prop.children [
                                                            for tag in
                                                                [ 
                                                                    "State Handling"
                                                                    "Background Jobs"
                                                                    "Notifications"
                                                                    "Validation"
                                                                    "Operational tooling" ] do
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

                                            Html.div [
                                                prop.className "featured-service cursor-pointer"
                                                prop.onClick (fun e -> e.stopPropagation(); GoToSection Backend |> dispatch)
                                                prop.children [
                                                    Html.div [
                                                        prop.className "text-4xl mb-6 opacity-60"
                                                        prop.text "⚡"
                                                    ]
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-2xl md:text-3xl font-light mb-4"
                                                        prop.text "Backend Engineering"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                                                        prop.text
                                                            "Designing custom servers, backend APIs, validation pipelines, and document-processing workflows that support real operational needs. Strong focus on reliability, state handling, and clear system boundaries."
                                                    ]
                                                    Html.div [
                                                        prop.className "flex flex-wrap gap-2 mb-6 text-xs"
                                                        prop.children [
                                                            for tag in
                                                                [ 
                                                                    "Custom Servers"
                                                                    "REST APIs"
                                                                    "WebSockets"
                                                                    "Validation"
                                                                    "Document Pipelines" ] do
                                                                Html.span [
                                                                    prop.className "tech-badge"
                                                                    prop.text tag
                                                                ]
                                                        ]
                                                    ]
                                                    Html.button [
                                                        prop.className "cta-btn text-[0.7rem]"
                                                        prop.text "Learn More →"
                                                        prop.onClick (fun e -> e.stopPropagation(); GoToSection Backend |> dispatch)
                                                    ]
                                                ]
                                            ]

                                            Html.div [
                                                prop.className "featured-service cursor-pointer"
                                                prop.onClick (fun e -> e.stopPropagation(); GoToSection Frontend |> dispatch)
                                                prop.children [
                                                    Html.div [
                                                        prop.className "text-4xl mb-6 opacity-60"
                                                        prop.text "⚙️"
                                                    ]
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-2xl md:text-3xl font-light mb-4"
                                                        prop.text "Frontend Development"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                                                        prop.text
                                                            "Building responsive application interfaces with strong client architecture, predictable state management, and maintainable component systems. Focused on product UIs that are both polished and practical."
                                                    ]
                                                    Html.div [
                                                        prop.className "flex flex-wrap gap-2 mb-6 text-xs"
                                                        prop.children [
                                                            for tag in
                                                                [ 
                                                                    "React"
                                                                    "Typescript"
                                                                    "Fable"
                                                                    "Elmish"
                                                                    "Tailwind CSS" ] do
                                                                Html.span [
                                                                    prop.className "tech-badge"
                                                                    prop.text tag
                                                                ]
                                                        ]
                                                    ]
                                                    Html.button [
                                                        prop.className "cta-btn text-[0.7rem]"
                                                        prop.text "Learn More →"
                                                        prop.onClick (fun e -> e.stopPropagation(); GoToSection Frontend |> dispatch)
                                                    ]
                                                ]
                                            ]

                                            Html.div [
                                                prop.className "featured-service cursor-pointer"
                                                prop.onClick (fun e -> e.stopPropagation(); GoToSection FullStack |> dispatch)
                                                prop.children [
                                                    Html.div [
                                                        prop.className "text-4xl mb-6 opacity-60"
                                                        prop.text "🦾"
                                                    ]
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-2xl md:text-3xl font-light mb-4"
                                                        prop.text "Full-Stack Engineering"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                                                        prop.text
                                                            "Building production applications across frontend, backend, APIs, and deployment. This includes shaping architecture, implementing features end to end, and keeping systems maintainable as they grow."
                                                    ]
                                                    Html.div [
                                                        prop.className "flex flex-wrap gap-2 mb-6 text-xs"
                                                        prop.children [
                                                            for tag in
                                                                [ 
                                                                    "F#"
                                                                    "Typescript"
                                                                    "React"
                                                                    ".NET"
                                                                    "PostgresSQL" ] do
                                                                Html.span [
                                                                    prop.className "tech-badge"
                                                                    prop.text tag
                                                                ]
                                                        ]
                                                    ]
                                                    Html.button [
                                                        prop.className "cta-btn text-[0.7rem]"
                                                        prop.text "Learn More →"
                                                        prop.onClick (fun e -> e.stopPropagation(); GoToSection FullStack |> dispatch)
                                                    ]
                                                ]
                                            ]

                                            
                                            Html.div [
                                                prop.className "featured-service cursor-pointer"
                                                prop.onClick (fun e -> e.stopPropagation(); GoToSection Leadership |> dispatch)
                                                prop.children [
                                                    Html.div [
                                                        prop.className "text-4xl mb-6 opacity-60"
                                                        prop.text "⚡"
                                                    ]
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-2xl md:text-3xl font-light mb-4"
                                                        prop.text "Leadership & Mentorship"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                                                        prop.text
                                                            "Leading through hands-on implementation, architecture guidance, code review, and helping other engineers build confidence and clarity across unfamiliar or complex parts of the stack."
                                                    ]
                                                    Html.div [
                                                        prop.className "flex flex-wrap gap-2 mb-6 text-xs"
                                                        prop.children [
                                                            for tag in
                                                                [ 
                                                                    "Architecture"
                                                                    "Code Review"
                                                                    "System Design"
                                                                    "Technical Guidance"
                                                                    "Team Management" ] do
                                                                Html.span [
                                                                    prop.className "tech-badge"
                                                                    prop.text tag
                                                                ]
                                                        ]
                                                    ]
                                                    Html.button [
                                                        prop.className "cta-btn text-[0.7rem]"
                                                        prop.text "Learn More →"
                                                        prop.onClick (fun e -> e.stopPropagation(); GoToSection Leadership |> dispatch)
                                                    ]
                                                ]
                                            ]
                                            
                                            Html.div [
                                                prop.className "featured-service cursor-pointer"
                                                prop.onClick (fun e -> e.stopPropagation(); GoToSection PlatformDelivery |> dispatch)
                                                prop.children [
                                                    Html.div [
                                                        prop.className "text-4xl mb-6 opacity-60"
                                                        prop.text "⚡"
                                                    ]
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-2xl md:text-3xl font-light mb-4"
                                                        prop.text "Cloud & Platform Delivery"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                                                        prop.text
                                                            "Shipping and supporting applications with containers, CI/CD pipelines, cloud infrastructure, and environment-aware deployment workflows that improve reliability and reduce friction."
                                                    ]
                                                    Html.div [
                                                        prop.className "flex flex-wrap gap-2 mb-6 text-xs"
                                                        prop.children [
                                                            for tag in
                                                                [ 
                                                                    "Docker"
                                                                    "Kubernetes"
                                                                    "CI/CD"
                                                                    "Azure"
                                                                    "DigitalOcean" ] do
                                                                Html.span [
                                                                    prop.className "tech-badge"
                                                                    prop.text tag
                                                                ]
                                                        ]
                                                    ]
                                                    Html.button [
                                                        prop.className "cta-btn text-[0.7rem]"
                                                        prop.text "Learn More →"
                                                        prop.onClick (fun e -> e.stopPropagation(); GoToSection PlatformDelivery |> dispatch)
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
            
            Html.section [
                prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto"
                        prop.children [
                            Html.h2 [
                                prop.className "cormorant-font text-3xl md:text-4xl lg:text-5xl font-light text-center mb-12"
                                prop.text "Engineering Focus"
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
                                        "Typed systems"
                                        "I gravitate toward strongly typed systems and predictable application structure because they make complex behavior easier to reason about, maintain, and extend over time."
                                        [ ]

                                    approachCard
                                        "APIs and integrations"
                                        "A lot of my work involves connecting systems cleanly through APIs, custom endpoints, validation flows, and integration layers that support real product and operational workflows."
                                        [ ]

                                    approachCard
                                        "Workflow automation"
                                        "I enjoy building systems that move work forward through intake, validation, processing, review, and completion without losing transparency or control along the way."
                                        [ ]
                                    
                                    approachCard
                                        "Responsive applications"
                                        "On the frontend, I focus on responsive application interfaces that remain clear and usable even when the underlying workflow or state model is complex."
                                        [ ]
                                    
                                    approachCard
                                        "Deployment and infrastructure"
                                        "I care about how software gets shipped, configured, and supported in practice, not just how it looks in development. Deployment and reliability are part of the product."
                                        [ ]
                                    approachCard
                                        "Maintainability & Refactoring"
                                        "I value codebases that stay understandable as they grow. That usually means tightening naming, boundaries, duplication, and structure before complexity becomes expensive."
                                        [ ]
                                    approachCard
                                        "Technical Leadership"
                                        "I tend to contribute by bringing structure to ambiguous work, unblocking implementation, and helping teams move forward with cleaner technical direction."
                                        [ ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // TECHNOLOGY STACKS
            ScrollReveal {|
                Variant   = SlideRight
                Delay     = 0.08
                Threshold = 0.45
                Children = 
                    Html.section [
                        prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
                        prop.children [
                            Html.div [
                                prop.className "max-w-6xl mx-auto"
                                prop.children [
                                    Html.h2 [
                                        prop.className "cormorant-font text-3xl md:text-4xl lg:text-5xl font-light text-center mb-12"
                                        prop.text "Languages, Frameworks, & Tools"
                                    ]
                                    Html.p [
                                        prop.className "text-sm opacity-60 text-center max-w-3xl mx-auto mb-16 leading-loose"
                                        prop.text
                                            "A broader view of the languages, frameworks, platforms, and tools I've worked with across full-stack, backend, frontend, and platform-focused engineering work."
                                    ]

                                    Html.div [
                                        prop.className "grid sm:grid-cols-2 lg:grid-cols-3 gap-6"
                                        prop.children [
                                            stackCard "Languages" [ "F#"; "TypeScript"; "JavaScript"; "Python"; "C#"; "Rust"; "Kotlin"; "Swift"; "SQL" ]
                                            stackCard "Frontend" [ "React"; "JSX / TSX"; "Fable"; "Elmish"; "HTML"; "CSS"; "Tailwind CSS"; "DaisyUI"; "Bootstrap"; "Bulma" ]
                                            stackCard "Backend & APIs" [ ".NET"; "Node.js"; "Express"; "REST APIs"; "GraphQL"; "WebSockets"; "Azure Functions" ]
                                            stackCard "Data & Storage" [ "PostgreSQL"; "SQL Server"; "MongoDB"; "Pinecone"; "Redis" ]
                                            stackCard "Cloud, DevOps & Delivery" [ "Docker"; "Kubernetes"; "Terraform"; "Helm"; "GitHub Actions"; "CI/CD"; "Azure"; "DigitalOcean" ]
                                            stackCard "AI & Workflow Tooling" [ "OpenAI API"; "LangChain"; "n8n"; "Zapier"; "Retrieval-Augmented Generation"; "Prompt Design"; "State Handling"; "Background Jobs"; "Operational Tooling" ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
            |}

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
                                        prop.text "Interested in my experience?"
                                    ]
                                    Html.div [
                                        prop.className "flex flex-col sm:flex-row gap-6 justify-center"
                                        prop.children [
                                            Html.button [
                                                prop.className "cta-btn"
                                                prop.text "View Resume"
                                                prop.onClick (fun _ ->
                                                    LoadApp ResumeAppView |> dispatch
                                                )
                                            ]
                                            Html.button [
                                                prop.className "cta-btn"
                                                prop.text "Get in touch"
                                                prop.onClick (fun _ ->
                                                    // adjust target view if you prefer a different contact entry
                                                    LoadApp ContactAppView |> dispatch
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
let SkillsLandingButton (dispatch: Msg -> unit) =
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
                    GoToSection SkillsLanding |> dispatch
                )
                prop.children [
                    Html.span [ prop.text "←" ]
                    Html.span [ prop.text "Back to Skills" ]
                ]
            ]
        ]
    ]
[<ReactComponent>]
let View (model: Model) (dispatch: Msg -> unit) =
    React.useMemo ((
        fun () ->
            let el = Browser.Dom.document.getElementById("inner-main-content")
            if not (isNull el) then
                el.scrollTop <- 0.0
        ), 
        [| box model.CurrentSection |]
    )
    Html.main [
        prop.className "w-full"
        prop.children [
            match model.CurrentSection with
            | SkillsLanding -> SkillsLandingSection dispatch
            | AI            -> Components.FSharp.Service.AIPage (SkillsLandingButton dispatch) (SkillMsg >> dispatch)
            | Automation    -> Components.FSharp.Service.AutomationPage (SkillsLandingButton dispatch) (SkillMsg >> dispatch)
            | Backend   -> Components.FSharp.Service.BackendPage (SkillsLandingButton dispatch) (SkillMsg >> dispatch)
            | Frontend       -> Components.FSharp.Service.FrontendPage (SkillsLandingButton dispatch) (SkillMsg >> dispatch)
            | FullStack -> Components.FSharp.Service.FullstackPage (SkillsLandingButton dispatch) (SkillMsg >> dispatch)
            | Leadership   -> Components.FSharp.Service.LeadershipPage (SkillsLandingButton dispatch) (SkillMsg >> dispatch)
            | PlatformDelivery   -> Components.FSharp.Service.PlatformDeliveryPage (SkillsLandingButton dispatch) (SkillMsg >> dispatch)
        ]
    ]
