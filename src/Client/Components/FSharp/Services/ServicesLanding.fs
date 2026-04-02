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
    | GoToSection section -> 
        Browser.Dom.console.log("Navigating to section:", section)
        { model with CurrentSection = section }, Cmd.none


let stackCard (title: string) (items: string list) =
    Html.div [
        prop.className "text-center"
        prop.children [
            Html.div [
                prop.className "cormorant-font text-lg font-light mb-3 opacity-80"
                prop.text title
            ]
            Html.div [
                prop.className "space-y-2 text-sm opacity-60"
                prop.children [
                    for i in items do Html.p i
                ]
            ]
        ]
    ]

type SkillCardContent = {|
    GoToSection: unit -> unit
    Icon: ReactElement
    Title: string
    Body: string
    Tags: string list
|}

[<ReactComponent>]
let FeaturedSkillCard (props: SkillCardContent) =
    Html.div [
        prop.className "flex flex-col featured-service cursor-pointer text-center items-center justify-center"
        prop.onClick (fun e -> e.stopPropagation(); props.GoToSection () )
        prop.children [
            // Html.div [
            //     prop.className "text-4xl mb-6 opacity-60"
            //     prop.text "🤖"
            // ]
            props.Icon
            Html.h3 [
                prop.className "cormorant-font text-2xl md:text-3xl font-light mb-4"
                prop.text props.Title
            ]
            Html.p [
                prop.className "text-sm opacity-70 mb-6 leading-relaxed"
                prop.text props.Body
            ]
            Html.div [
                prop.className "flex flex-wrap gap-2 mb-6 text-xs items-center justify-center"
                prop.children [
                    for tag in props.Tags do
                        Html.span [
                            prop.className "tech-badge"
                            prop.text tag
                        ]
                ]
            ]
            Html.button [
                prop.className "cta-btn text-[0.7rem]"
                prop.text "Learn More →"
                prop.onClick (fun e -> e.stopPropagation();  props.GoToSection () )
            ]
        ]
    ]

[<ReactComponent>]
let SkillsLandingSection (dispatch: Msg -> unit) =
    // Html.div [
    Html.section [
        prop.className "py-16 md:py-20 px-6 md:px-8 lg:px-12 mb-16 md:mb-24"
        prop.children [
            Html.div [
                prop.className "max-w-6xl mx-auto"
                prop.children [
                    
                    // HERO
                    Client.Components.Shop.Common.Ui.Section.headerTagArea
                        (LucideIcon.Palette "w-4 h-4 opacity-60")
                        "SKILLS"

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

                    Html.p [
                        prop.className "text-base md:text-lg text-base-content/70 max-w-3xl mx-auto leading-8 text-center"
                        prop.text "My background spans full-stack application development, backend APIs, workflow-heavy systems, cloud delivery, and AI-enabled product features. Strongest in F#, React, TypeScript, Python, and C#."
                    ]

                    Html.section [
                        prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
                        prop.children [
                            Html.div [
                                prop.className "max-w-6xl mx-auto"
                                prop.children [
                                    Html.p [
                                        prop.className "cormorant-font text-3xl md:text-4xl lg:text-5xl font-light text-center mb-12"
                                        prop.text "Core Areas"
                                    ]

                                    Html.div [
                                        prop.className "grid lg:grid-cols-2 gap-8 mb-16"

                                        prop.children [
                                            
                                            FeaturedSkillCard {|
                                                GoToSection = (fun () -> GoToSection AI |> dispatch)
                                                Icon = LucideIcon.Bot "text-4xl mb-6 opacity-60"
                                                Title = "AI & LLM Integrations"
                                                Body = "Integrating AI-assisted features into real products and workflows, especially for document analysis, retrieval-driven tooling, and human-in-the-loop systems that benefit from contextual assistance."
                                                Tags = [ "OpenAI API"; "RAG"; "Document Analysis"; "Prompt Design" ]
                                            |}

                                            FeaturedSkillCard {|
                                                GoToSection = (fun () -> GoToSection Automation |> dispatch)
                                                Icon = LucideIcon.Cog "text-4xl mb-6 opacity-60"
                                                Title = "Workflow Automation"
                                                Body = "Creating workflow-heavy systems that reduce repetitive work, improve visibility into process state, and support review, correction, and downstream actions where reliability matters."
                                                Tags = [ "State Handling"; "Background Jobs"; "Notifications"; "Validation"; "Operational tooling" ]
                                            |}

                                            FeaturedSkillCard {|
                                                GoToSection = (fun () -> GoToSection Backend |> dispatch)
                                                Icon = LucideIcon.Zap "text-4xl mb-6 opacity-60"
                                                Title = "Backend APIs & Systems"
                                                Body = "Designing custom servers, backend APIs, validation pipelines, and document-processing workflows that support real operational needs. Strong focus on reliability, state handling, and clear system boundaries."
                                                Tags = [ "Custom Servers"; "REST APIs"; "WebSockets"; "Validation"; "Document Pipelines" ]
                                            |}

                                            FeaturedSkillCard {|
                                                GoToSection = (fun () -> GoToSection Frontend |> dispatch)
                                                Icon = LucideIcon.HandPlatter "text-4xl mb-6 opacity-60"
                                                Title = "Frontend Development"
                                                Body = "Building responsive application interfaces with strong client architecture, predictable state management, and maintainable component systems. Focused on product UIs that are both polished and practical."
                                                Tags = [ "React"; "TypeScript"; "Fable"; "Elmish"; "Responsive Design"; "Tailwind CSS" ]
                                            |}

                                            FeaturedSkillCard {|
                                                GoToSection = (fun () -> GoToSection FullStack |> dispatch)
                                                Icon = LucideIcon.Cpu "text-4xl mb-6 opacity-60"
                                                Title = "Full-Stack Engineering"
                                                Body = "Building production applications across frontend, backend, APIs, and deployment. This includes shaping architecture, implementing features end to end, and keeping systems maintainable as they grow."
                                                Tags = [ "F#"; "TypeScript"; "React"; ".NET"; "PostgreSQL"; "Node.js"; "Python" ]
                                            |}

                                            FeaturedSkillCard {|
                                                GoToSection = (fun () -> GoToSection Leadership |> dispatch)
                                                Icon = LucideIcon.Compass "text-4xl mb-6 opacity-60"
                                                Title = "Leadership & Mentorship"
                                                Body = "Leading through hands-on implementation, architecture guidance, code review, and helping other engineers build confidence and clarity across unfamiliar or complex parts of the stack."
                                                Tags = [ "Architecture"; "Code Review"; "System Design"; "Technical Guidance"; "Mentorship" ]
                                            |}

                                            FeaturedSkillCard {|
                                                GoToSection = (fun () -> GoToSection PlatformDelivery |> dispatch)
                                                Icon = LucideIcon.Cloud "text-4xl mb-6 opacity-60"
                                                Title = "Cloud & Platform Delivery"
                                                Body = "Shipping and supporting applications with containers, CI/CD pipelines, cloud infrastructure, and environment-aware deployment workflows that improve reliability and reduce friction."
                                                Tags = [ "Docker"; "Kubernetes"; "CI/CD"; "Azure"; "Terraform"; "DigitalOcean" ] 
                                            |}

                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                    
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
                                            let approachCard (title: string) (body: string) =
                                                Html.div [
                                                    prop.className "approach-card"
                                                    prop.children [
                                                        Html.h4 [
                                                            prop.className "cormorant-font text-2xl md:text-3xl font-light mb-4"
                                                            prop.text title
                                                        ]
                                                        Html.p [
                                                            prop.className "text-sm text-base-content/70 leading-7"
                                                            prop.text body
                                                        ]
                                                    ]
                                                ]

                                            approachCard
                                                "Typed systems"
                                                "I gravitate toward strongly typed systems and predictable application structure because they make complex behavior easier to reason about, maintain, and extend over time."

                                            approachCard
                                                "APIs and integrations"
                                                "A lot of my work involves connecting systems cleanly through APIs, custom endpoints, validation flows, and integration layers that support real product and operational workflows."

                                            approachCard
                                                "Workflow automation"
                                                "I enjoy building systems that move work forward through intake, validation, processing, review, and completion without losing transparency or control along the way."
                                            
                                            approachCard
                                                "Responsive applications"
                                                "On the frontend, I focus on responsive application interfaces that remain clear and usable even when the underlying workflow or state model is complex."
                                            
                                            approachCard
                                                "Deployment and infrastructure"
                                                "I care about how software gets shipped, configured, and supported in practice, not just how it looks in development. Deployment and reliability are part of the product."

                                            approachCard
                                                "Maintainability & Refactoring"
                                                "I value codebases that stay understandable as they grow. That usually means tightening naming, boundaries, duplication, and structure before complexity becomes expensive."

                                            approachCard
                                                "Technical Leadership"
                                                "I tend to contribute by bringing structure to ambiguous work, unblocking implementation, and helping teams move forward with cleaner technical direction."

                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // TECHNOLOGY STACKS
                    ScrollReveal {|
                        Variant   = SlideLeft
                        Delay     = 0.08
                        Threshold = 0.15
                        Children = 
                            Html.section [
                                prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
                                prop.children [
                                    Html.div [
                                        prop.className "max-w-6xl mx-auto"
                                        prop.children [
                                            Html.h2 [
                                                prop.className "cormorant-font text-3xl md:text-4xl lg:text-5xl font-light text-center mb-12"
                                                prop.text "Languages, Skills & Tools"
                                            ]
                                            Html.p [
                                                prop.className "text-md opacity-60 text-center max-w-3xl mx-auto mb-16 leading-loose"
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
                                                prop.className "cormorant-font text-3xl md:text-4xl lg:text-5xl font-light text-center mb-12"
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
