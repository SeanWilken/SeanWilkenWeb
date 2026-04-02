module Components.FSharp.Service

open Feliz
open Components.FSharp

type Msg =
    | NavigateTo of SharedViewModule.WebAppView.ProfessionalSkillsView

type SkillFeature = {
    Title: string
    Description: string
    Icon: string option
}

type SkillCapability = {
    Title: string
    Description: string
}

type SkillDomain = {
    Name: string
    Description: string
}

type SkillTechnologyGroup = {
    GroupName: string
    Items: string list
}

type SkillOutcome = {
    Label: string
    Value: string
    Context: string option
}

type RelatedSkillLink = {
    Name: string
    Route: SharedViewModule.WebAppView.ProfessionalSkillsView
    Description: string option
}

type SkillPageModel = {|
    Id: string
    Name: string

    HeroTitle: string
    HeroSubtitle: string
    HeroBadge: string option
    HeroGradientClass: string

    CoreSectionTitle: string
    CoreAreas: SkillFeature list

    FocusSectionTitle: string option
    FocusAreas: SkillCapability list

    DomainsSectionTitle: string option
    Domains: SkillDomain list

    TechnologiesSectionTitle: string option
    Technologies: SkillTechnologyGroup list

    OutcomesSectionTitle: string option
    Outcomes: SkillOutcome list

    RelatedSectionTitle: string option
    RelatedPages: RelatedSkillLink list
|}

let private capabilityCard (cap: SkillFeature) =
    Html.div [
        prop.className
            "rounded-2xl bg-base-100 border border-base-300/70 shadow-sm p-4 flex items-start gap-3 transition-transform duration-200 hover:-translate-y-0.5 hover:shadow-md"
        prop.children [
            match cap.Icon with
            | None -> Html.none
            | Some ico ->
                Html.div [
                    prop.className "w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center text-base"
                    prop.text ico
                ]
            Html.div [
                prop.children [
                    Html.h4 [
                        prop.className "text-sm font-semibold text-base-content"
                        prop.text cap.Title
                    ]
                    Html.p [
                        prop.className "mt-1 text-xs md:text-sm text-base-content/70 leading-relaxed"
                        prop.text cap.Description
                    ]
                ]
            ]
        ]
    ]


let private sectionHeader (title: string) (subtitle: string option) =
    Html.div [
        prop.className "text-center mb-10 sm:mb-12"
        prop.children [
            Html.h2 [
                prop.className "cormorant-font text-3xl sm:text-4xl lg:text-5xl font-light section-title inline-block"
                prop.text title
            ]
            match subtitle with
            | Some text when not (System.String.IsNullOrWhiteSpace text) ->
                Html.p [
                    prop.className "mt-4 text-sm sm:text-base text-base-content/70 max-w-3xl mx-auto leading-relaxed"
                    prop.text text
                ]
            | _ -> Html.none
        ]
    ]

[<ReactComponent>]
let DomainSection (title: string) (subtitle: string option) (domains: SkillDomain list) =
    if List.isEmpty domains then
        Html.none
    else
        Html.section [
            prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
            prop.children [
                Html.div [
                    prop.className "max-w-7xl mx-auto"
                    prop.children [
                        sectionHeader title subtitle

                        Html.div [
                            prop.className "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
                            prop.children [
                                for domain in domains do
                                    Html.article [
                                        prop.key domain.Name
                                        prop.className "rounded-2xl bg-base-100 border border-base-300/70 shadow-sm p-6"
                                        prop.children [
                                            Html.h3 [
                                                prop.className "text-lg font-semibold text-base-content"
                                                prop.text domain.Name
                                            ]
                                            Html.p [
                                                prop.className "mt-3 text-sm leading-relaxed text-base-content/75"
                                                prop.text domain.Description
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
let TechnologiesSection (title: string) (subtitle: string option) (groups: SkillTechnologyGroup list) =
    if List.isEmpty groups then
        Html.none
    else
        Html.section [
            prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12 bg-base-200/50"
            prop.children [
                Html.div [
                    prop.className "max-w-7xl mx-auto"
                    prop.children [
                        sectionHeader title subtitle

                        Html.div [
                            prop.className "grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6"
                            prop.children [
                                for group in groups do
                                    Html.article [
                                        prop.key group.GroupName
                                        prop.className "rounded-2xl bg-base-100 border border-base-300/70 shadow-sm p-6"
                                        prop.children [
                                            Html.h3 [
                                                prop.className "text-lg font-semibold text-base-content"
                                                prop.text group.GroupName
                                            ]

                                            Html.div [
                                                prop.className "mt-4 flex flex-wrap gap-2"
                                                prop.children [
                                                    for item in group.Items do
                                                        Html.span [
                                                            prop.key (group.GroupName + "-" + item)
                                                            prop.className "px-3 py-1.5 rounded-full text-sm bg-base-200 border border-base-300/70 text-base-content/80"
                                                            prop.text item
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
let OutcomesSection (title: string) (subtitle: string option) (outcomes: SkillOutcome list) =
    if List.isEmpty outcomes then
        Html.none
    else
        Html.section [
            prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12"
            prop.children [
                Html.div [
                    prop.className "max-w-7xl mx-auto"
                    prop.children [
                        sectionHeader title subtitle

                        Html.div [
                            prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6"
                            prop.children [
                                for outcome in outcomes do
                                    Html.article [
                                        prop.key outcome.Label
                                        prop.className "rounded-2xl bg-base-100 border border-base-300/70 shadow-sm p-6"
                                        prop.children [
                                            Html.div [
                                                prop.className "text-3xl sm:text-4xl font-semibold text-base-content"
                                                prop.text outcome.Value
                                            ]
                                            Html.h3 [
                                                prop.className "mt-2 text-base font-medium text-base-content"
                                                prop.text outcome.Label
                                            ]
                                            match outcome.Context with
                                            | Some context when not (System.String.IsNullOrWhiteSpace context) ->
                                                Html.p [
                                                    prop.className "mt-3 text-sm leading-relaxed text-base-content/70"
                                                    prop.text context
                                                ]
                                            | _ -> Html.none
                                        ]
                                    ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

[<ReactComponent>]
let RelatedPagesSection (title: string) (subtitle: string option) (pages: RelatedSkillLink list) (relatedPageCallback: SharedViewModule.WebAppView.ProfessionalSkillsView -> unit) =
    if List.isEmpty pages then
        Html.none
    else
        Html.section [
            prop.className "py-16 md:py-20 px-4 sm:px-6 lg:px-12 bg-base-200/40"
            prop.children [
                Html.div [
                    prop.className "max-w-7xl mx-auto"
                    prop.children [
                        sectionHeader title subtitle

                        Html.div [
                            prop.className "grid grid-cols-1 md:grid-cols-2 gap-6"
                            prop.children [
                                for page in pages do
                                    Html.div [
                                        prop.key page.Name
                                        prop.onClick (fun _ -> relatedPageCallback page.Route)
                                        prop.className "group block cursor-pointer rounded-2xl bg-base-100 border border-base-300/70 shadow-sm p-6 transition-transform duration-200 hover:-translate-y-0.5 hover:shadow-md"
                                        prop.children [
                                            Html.div [
                                                prop.className "flex items-start justify-between gap-4"
                                                prop.children [
                                                    Html.div [
                                                        Html.h3 [
                                                            prop.className "text-lg font-semibold text-base-content group-hover:text-primary transition-colors"
                                                            prop.text page.Name
                                                        ]
                                                        match page.Description with
                                                        | Some desc when not (System.String.IsNullOrWhiteSpace desc) ->
                                                            Html.p [
                                                                prop.className "mt-3 text-sm leading-relaxed text-base-content/70"
                                                                prop.text desc
                                                            ]
                                                        | _ -> Html.none
                                                    ]

                                                    Html.span [
                                                        prop.className "text-base-content/40 group-hover:text-primary transition-colors"
                                                        prop.text "→"
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
let ServicePage (model: SkillPageModel) dispatch =
    Html.div [
        // Let the surrounding layout decide height; keep this page flexible
        prop.className "w-full text-base-content"
        prop.children [

            Html.div [
                prop.className "max-w-6xl mx-auto px-4 lg:px-8 py-12 md:py-16 space-y-14 md:space-y-18"
                prop.children [

                    // HERO
                    Html.section [
                        prop.className (
                            sprintf
                                "relative overflow-hidden rounded-3xl border border-base-300/70 bg-gradient-to-br %s p-7 md:p-10 lg:p-12 shadow-[0_24px_70px_rgba(15,23,42,0.40)]"
                                model.HeroGradientClass
                        )
                        prop.children [
                            // Soft blobs
                            Html.div [
                                prop.className
                                    "pointer-events-none absolute -top-24 -right-24 w-72 h-72 rounded-full bg-base-100/10 blur-3xl"
                            ]
                            Html.div [
                                prop.className
                                    "pointer-events-none absolute -bottom-32 -left-10 w-80 h-80 rounded-full bg-base-100/5 blur-3xl"
                            ]

                            Html.div [
                                prop.className "relative flex flex-col gap-10 md:flex-row md:items-center"
                                prop.children [

                                    // Copy
                                    Html.div [
                                        prop.className "flex-1 space-y-4 md:space-y-5"
                                        prop.children [
                                            match model.HeroBadge with
                                            | Some badge ->
                                                Html.div [
                                                    prop.className
                                                        "inline-flex items-center gap-2 rounded-full bg-base-100/15 border border-base-100/40 px-4 py-1 text-[0.68rem] font-semibold tracking-[0.2em] uppercase text-base-100 shadow-sm backdrop-blur"
                                                    prop.children [
                                                        Html.span [
                                                            prop.className
                                                                "w-2 h-2 rounded-full bg-accent animate-pulse"
                                                        ]
                                                        Html.span badge
                                                    ]
                                                ]
                                            | None -> Html.none

                                            Html.h1 [
                                                prop.className
                                                    "cormorant-font text-3xl sm:text-4xl md:text-5xl lg:text-6xl font-light leading-tight text-base-100"
                                                prop.text model.HeroTitle
                                            ]

                                            Html.p [
                                                prop.className
                                                    "mt-1 text-sm md:text-base lg:text-lg text-base-100/85 max-w-xl leading-relaxed"
                                                prop.text model.HeroSubtitle
                                            ]

                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    Html.section [
                        prop.children [
                            Html.div [
                                prop.className "flex items-center justify-between gap-4 mb-4"
                                prop.children [
                                    Html.div [
                                        prop.children [
                                            Html.span [
                                                prop.className
                                                    "text-[0.7rem] font-semibold tracking-[0.22em] uppercase text-base-content/55"
                                                prop.text "Engineering Focus"
                                            ]
                                            match model.FocusSectionTitle with
                                            | None -> Html.none
                                            | Some title ->
                                                Html.h2 [
                                                    prop.className
                                                        "mt-1 cormorant-font text-2xl md:text-3xl font-light text-base-content"
                                                    prop.text title
                                                ]
                                        ]
                                    ]
                                    Html.span [
                                        prop.className
                                            "hidden md:block text-xs md:text-sm text-right text-base-content/60"
                                        prop.text "key areas of expertise and focus"
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5"
                                prop.children [
                                    for f in model.FocusAreas do
                                        Html.div [
                                            prop.className
                                                "group relative rounded-2xl bg-base-100 border border-base-300/70 shadow-sm p-5 text-left transition-transform duration-200 hover:-translate-y-1 hover:shadow-lg"
                                            prop.children [
                                                Html.div [
                                                    prop.className
                                                        "absolute inset-0 rounded-2xl bg-gradient-to-br from-secondary/5 to-accent/10 opacity-0 group-hover:opacity-100 transition pointer-events-none"
                                                ]
                                                Html.h3 [
                                                    prop.className
                                                        "relative text-sm font-semibold text-base-content mb-1"
                                                    prop.text f.Title
                                                ]
                                                Html.p [
                                                    prop.className
                                                        "relative text-xs md:text-sm text-base-content/75 leading-relaxed"
                                                    prop.text f.Description
                                                ]
                                            ]
                                        ]
                                ]
                            ]
                        ]
                    ]

                    Html.hr [ prop.className "border-base-300/60" ]

                    // CAPABILITIES
                    match model.CoreSectionTitle, model.CoreAreas with
                    | title, _ :: _ ->
                        Html.section [
                            prop.children [
                                Html.span [
                                    prop.className
                                        "text-[0.7rem] font-semibold tracking-[0.22em] uppercase text-base-content/55"
                                    prop.text "Capabilities"
                                ]
                                Html.h2 [
                                    prop.className
                                        "mt-1 cormorant-font text-2xl md:text-3xl font-light text-base-content mb-4"
                                    prop.text title
                                ]
                                Html.div [
                                    prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-2 gap-4"
                                    prop.children [
                                        for cap in model.CoreAreas do
                                            capabilityCard cap
                                    ]
                                ]
                            ]
                        ]
                    | _ -> Html.none

                    Html.hr [ prop.className "border-base-300/60" ]

                    DomainSection
                        (defaultArg model.DomainsSectionTitle "Domains")
                        (Some "Where I've applied these skills professionally.")
                        model.Domains

                    Html.hr [ prop.className "border-base-300/60" ]

                    TechnologiesSection
                        (defaultArg model.TechnologiesSectionTitle "Languages, Skills & Tools")
                        (Some "A broader view of the languages, frameworks, platforms, and tools I've worked with across full-stack, backend, frontend, and platform-focused engineering work.")
                        model.Technologies
                    
                    Html.hr [ prop.className "border-base-300/60" ]

                    // OutcomesSection
                    //     (defaultArg model.OutcomesSectionTitle "Selected Outcomes")
                    //     (Some "A few representative outcomes tied to this area of work.")
                    //     model.Outcomes
                    
                    // Html.hr [ prop.className "border-base-300/60" ]

                    RelatedPagesSection
                        (defaultArg model.RelatedSectionTitle "Related Areas")
                        (Some "Other skill areas that connect naturally with this work.")
                        model.RelatedPages
                        (fun view -> dispatch (NavigateTo view))
                ]
            ]
        ]
    ]



let commonDomains: SkillDomain list = [
    {
        Name = "Healthcare & Regulated Systems"
        Description = "Built workflow-heavy applications, validation systems, and document processing pipelines for regulated healthcare environments with complex domain rules."
    }
    {
        Name = "E-Commerce Platforms"
        Description = "Worked across storefronts, product systems, fulfillment flows, and operational tooling for commerce-focused applications."
    }
    {
        Name = "Internal Tools & Operational Workflows"
        Description = "Built systems that support intake, processing, review, reporting, and day-to-day operational workflows."
    }
    {
        Name = "Real-Time & Interactive Systems"
        Description = "Developed systems with live updates, status tracking, and responsive client interactions backed by state-aware backend processing."
    }
]

let relatedSkillLinks: RelatedSkillLink list = [
    {
        Name = "Full-Stack Engineering"
        Route = SharedViewModule.WebAppView.ProfessionalSkillsView.FullStack
        Description = Some "Application architecture across frontend, backend, and delivery."
    }
    {
        Name = "Backend APIs & Systems"
        Route = SharedViewModule.WebAppView.ProfessionalSkillsView.Backend
        Description = Some "Custom servers, APIs, processing pipelines, and integrations."
    }
    {
        Name = "Frontend Development"
        Route = SharedViewModule.WebAppView.ProfessionalSkillsView.Frontend
        Description = Some "Responsive application UIs, component systems, and client-side architecture."
    }
    {
        Name = "Workflow Automation"
        Route = SharedViewModule.WebAppView.ProfessionalSkillsView.Automation
        Description = Some "Stateful workflows, validation, notifications, and operational tooling."
    }
    {
        Name = "AI & LLM Integrations"
        Route = SharedViewModule.WebAppView.ProfessionalSkillsView.AI
        Description = Some "AI-assisted features, retrieval workflows, and document processing."
    }
    {
        Name = "Leadership & Mentorship"
        Route = SharedViewModule.WebAppView.ProfessionalSkillsView.Leadership
        Description = Some "Hands-on technical leadership, mentorship, and implementation guidance."
    }
    {
        Name = "Cloud & Platform Delivery"
        Route = SharedViewModule.WebAppView.ProfessionalSkillsView.PlatformDelivery
        Description = Some "Deployment, containers, CI/CD, infrastructure, and platform reliability."
    }
]

let backendCoreAreas: SkillFeature list = [
    {
        Title = "Custom Backend Services"
        Description = "Designing and deploying custom servers and application services to support operational workflows and product features."
        Icon = None
    }
    {
        Title = "API Design & Contracts"
        Description = "Building backend endpoints and contracts that support validation, processing, submission, and frontend integration."
        Icon = None
    }
    {
        Title = "Document & Data Processing"
        Description = "Implementing ingestion, transformation, validation, and processing pipelines for structured and semi-structured workflows."
        Icon = None
    }
    {
        Title = "State-Aware Systems"
        Description = "Handling status transitions, duplicate prevention, retries, and review/resubmission flows in long-running processes."
        Icon = None
    }
]

let backendFocusAreas: SkillCapability list = [
    {
        Title = "Backend API Development"
        Description = "Building application backends for client access, data workflows, and domain-specific operations."
    }
    {
        Title = "Validation & Submission Pipelines"
        Description = "Designing systems that check data quality, surface discrepancies, and support reliable downstream submission."
    }
    {
        Title = "Real-Time Workflow State"
        Description = "Using WebSockets and state-safe processing patterns to expose progress and completion state to the client."
    }
    {
        Title = "Integration-Driven Backend Work"
        Description = "Connecting internal and third-party systems through APIs, webhooks, and controlled data movement."
    }
]

let backendTechnologies: SkillTechnologyGroup list = [
    {
        GroupName = "Languages"
        Items = [ "F#"; "TypeScript"; "Python"; "C#" ]
    }
    {
        GroupName = "Backend"
        Items = [ ".NET"; "Node.js"; "Express"; "REST APIs"; "GraphQL"; "WebSockets" ]
    }
    {
        GroupName = "Data"
        Items = [ "PostgreSQL"; "SQL Server"; "MongoDB"; "Redis" ]
    }
    {
        GroupName = "Infrastructure"
        Items = [ "Docker"; "Kubernetes"; "Azure"; "DigitalOcean" ]
    }
]

let backendOutcomes: SkillOutcome list = [
    {
        Label = "Chart Review Reduction"
        Value = "60%"
        Context = Some "Through automation and backend processing pipelines."
    }
    {
        Label = "Search & Workflow Performance"
        Value = "Seconds, not minutes"
        Context = Some "Introduced indexing, caching, background jobs, paging, and filtering to make large healthcare datasets practical for daily use."
    }
    {
        Label = "Submission Reliability"
        Value = "Improved"
        Context = Some "Validation and discrepancy-reporting workflows across trauma registry and healthcare reporting systems."
    }
]

let backendModel: SkillPageModel = {|
    Id = "backend-apis-systems"
    Name = "Backend APIs & Systems"
    HeroTitle = "Backend systems for APIs, processing pipelines, and workflow-heavy applications"
    HeroSubtitle = "Strong backend experience building custom servers, validation systems, state-aware processing flows, and client-facing APIs for operational software."
    HeroBadge = Some "Backend APIs & Systems"
    HeroGradientClass = "from-accent to-primary"

    CoreSectionTitle = "Core Areas"
    CoreAreas = backendCoreAreas

    FocusSectionTitle = Some "Technical Focus"
    FocusAreas = backendFocusAreas

    DomainsSectionTitle = Some "Domains"
    Domains = commonDomains

    TechnologiesSectionTitle = Some "Core Technologies"
    Technologies = backendTechnologies

    OutcomesSectionTitle = Some "Selected Outcomes"
    Outcomes = backendOutcomes

    RelatedSectionTitle = Some "Related Areas"
    RelatedPages = relatedSkillLinks
|}
let BackendPage (backButtonComponent: ReactElement) = ServicePage backendModel


let frontendCoreAreas: SkillFeature list = [
    {
        Title = "Application UI Development"
        Description = "Building responsive application interfaces that support complex workflows without overwhelming the user."
        Icon = None
    }
    {
        Title = "Component Systems"
        Description = "Creating reusable UI patterns and consistent structure that scale across pages, features, and states."
        Icon = None
    }
    {
        Title = "Typed Frontend Architecture"
        Description = "Using React, TypeScript, and F#-based client patterns to keep state and behavior predictable."
        Icon = None
    }
    {
        Title = "Responsive & Maintainable Design"
        Description = "Improving layout, usability, and structure while keeping the codebase clean and adaptable."
        Icon = None
    }
]

let frontendFocusAreas: SkillCapability list = [
    {
        Title = "React-Based Application Development"
        Description = "Building client-side experiences for product workflows, form-heavy applications, dashboards, and interactive tools."
    }
    {
        Title = "State Management & Client Logic"
        Description = "Structuring UI behavior and view state in a way that remains maintainable as applications grow."
    }
    {
        Title = "UI Implementation & Design Systems"
        Description = "Translating rough concepts, mocks, or evolving design direction into polished and usable interfaces."
    }
    {
        Title = "Frontend Performance & Usability"
        Description = "Improving perceived speed, responsiveness, and interaction quality across device sizes and application states."
    }
]

let frontendTechnologies: SkillTechnologyGroup list = [
    {
        GroupName = "Languages"
        Items = [ "TypeScript"; "JavaScript"; "F#" ]
    }
    {
        GroupName = "Frontend"
        Items = [ "React"; "JSX"; "TSX"; "Fable"; "Elmish"; "HTML"; "CSS" ]
    }
    {
        GroupName = "Styling"
        Items = [ "Tailwind CSS"; "DaisyUI"; "Bootstrap"; "Bulma" ]
    }
    {
        GroupName = "Supporting Work"
        Items = [ "REST APIs"; "GraphQL"; "Responsive Design"; "Component Architecture" ]
    }
]

let frontendOutcomes: SkillOutcome list = [
    {
        Label = "Regulated Frontend Workflows"
        Value = "Multi-State"
        Context = Some "Built dynamic frontend systems with state-specific regulatory logic and form behavior."
    }
    {
        Label = "Interactive Systems"
        Value = "60K+ Users"
        Context = Some "Frontend work supporting real-time and highly interactive user experiences."
    }
]

let frontendModel: SkillPageModel = {|
    Id = "frontend-development"
    Name = "Frontend Development"
    HeroTitle = "Frontend development focused on responsive applications and maintainable UI systems"
    HeroSubtitle = "Experience building product UIs, workflow-heavy interfaces, and component-driven frontends with React, TypeScript, F#, and modern styling systems."
    HeroBadge = Some "Frontend Development"
    HeroGradientClass = "from-primary to-secondary"

    CoreSectionTitle = "Core Areas"
    CoreAreas = frontendCoreAreas

    FocusSectionTitle = Some "Technical Focus"
    FocusAreas = frontendFocusAreas

    DomainsSectionTitle = Some "Domains"
    Domains = commonDomains

    TechnologiesSectionTitle = Some "Core Technologies"
    Technologies = frontendTechnologies

    OutcomesSectionTitle = Some "Selected Outcomes"
    Outcomes = frontendOutcomes

    RelatedSectionTitle = Some "Related Areas"
    RelatedPages = relatedSkillLinks
|}
let FrontendPage (backButtonComponent: ReactElement) = ServicePage frontendModel

let fullStackCoreAreas: SkillFeature list = [
    {
        Title = "Production Web Applications"
        Description = "Building maintainable applications with clear boundaries across frontend, backend, and shared domain logic."
        Icon = None
    }
    {
        Title = "End-to-End Feature Delivery"
        Description = "Owning features from data model and API design through UI implementation, testing, and deployment."
        Icon = None
    }
    {
        Title = "System Design & Architecture"
        Description = "Structuring applications to remain understandable, scalable, and adaptable as requirements evolve."
        Icon = None
    }
    {
        Title = "Cross-Stack Problem Solving"
        Description = "Working across client, server, database, and infrastructure layers to unblock delivery and improve reliability."
        Icon = None
    }
]

let fullStackFocusAreas: SkillCapability list = [
    {
        Title = "Typed Application Development"
        Description = "Using strongly typed languages and predictable patterns to reduce ambiguity and improve maintainability."
    }
    {
        Title = "Frontend / Backend Integration"
        Description = "Designing APIs and client contracts that support clean application flow and reliable feature delivery."
    }
    {
        Title = "Workflow-Heavy Systems"
        Description = "Building applications that manage intake, validation, processing, review, and user-facing status updates."
    }
    {
        Title = "Performance & Reliability"
        Description = "Improving responsiveness, reducing failure points, and tightening the feedback loop between product and engineering."
    }
]

let fullStackTechnologies: SkillTechnologyGroup list = [
    {
        GroupName = "Languages"
        Items = [ "F#"; "TypeScript"; "JavaScript"; "Python"; "C#" ]
    }
    {
        GroupName = "Frontend"
        Items = [ "React"; "Fable"; "Elmish"; "HTML"; "CSS"; "Tailwind CSS"; "DaisyUI" ]
    }
    {
        GroupName = "Backend"
        Items = [ ".NET"; "Node.js"; "Express"; "REST APIs"; "GraphQL" ]
    }
    {
        GroupName = "Data & Infrastructure"
        Items = [ "PostgreSQL"; "SQL Server"; "MongoDB"; "Docker"; "Kubernetes" ]
    }
]

let fullStackOutcomes: SkillOutcome list = [
    {
        Label = "Patient Records Processed"
        Value = "16M+"
        Context = Some "Diagnostic automation and healthcare workflow systems."
    }
    {
        Label = "Medical Facilities Supported"
        Value = "20+"
        Context = Some "Platform delivery across regulated healthcare environments."
    }
]

let fullStackModel: SkillPageModel = {|
    Id = "full-stack-engineering"
    Name = "Full-Stack Engineering"
    HeroTitle = "Full-stack engineering across product, platform, and workflow-heavy systems"
    HeroSubtitle = "Experience building production web applications across healthcare, e-commerce, and internal tools and platforms, with strength across frontend, backend, APIs, and delivery."
    HeroBadge = Some "Full-Stack Engineering"
    HeroGradientClass = "from-secondary to-accent"

    CoreSectionTitle = "Core Areas"
    CoreAreas = fullStackCoreAreas

    FocusSectionTitle = Some "Technical Focus"
    FocusAreas = fullStackFocusAreas

    DomainsSectionTitle = Some "Domains"
    Domains = commonDomains

    TechnologiesSectionTitle = Some "Core Technologies"
    Technologies = fullStackTechnologies

    OutcomesSectionTitle = Some "Selected Outcomes"
    Outcomes = fullStackOutcomes

    RelatedSectionTitle = Some "Related Areas"
    RelatedPages = relatedSkillLinks
|}
let FullstackPage (backButtonComponent: ReactElement) = ServicePage fullStackModel



let automationCoreAreas: SkillFeature list = [
    {
        Title = "Operational Workflow Design"
        Description = "Building systems that move work forward through intake, validation, processing, review, and completion."
        Icon = None
    }
    {
        Title = "Automation with Oversight"
        Description = "Automating repetitive work while keeping meaningful checkpoints for users and teams."
        Icon = None
    }
    {
        Title = "Notifications & Status Handling"
        Description = "Surfacing the right state, alerts, and completion signals so users know what happened and what comes next."
        Icon = None
    }
    {
        Title = "Internal Tooling"
        Description = "Creating focused tools and process layers around existing systems instead of forcing unnecessary platform replacement."
        Icon = None
    }
]

let automationFocusAreas: SkillCapability list = [
    {
        Title = "Workflow State Management"
        Description = "Designing systems that manage long-running operations, retries, status visibility, and safe transitions."
    }
    {
        Title = "Validation & Review Loops"
        Description = "Supporting discrepancy checks, review steps, and controlled resubmission in data-heavy processes."
    }
    {
        Title = "Operational Efficiency"
        Description = "Reducing manual effort by automating repeatable tasks while preserving reliability and visibility."
    }
    {
        Title = "Cross-System Process Automation"
        Description = "Connecting inputs, data transformations, and downstream actions across APIs and internal tools."
    }
]

let automationTechnologies: SkillTechnologyGroup list = [
    {
        GroupName = "Backend & Workflow"
        Items = [ ".NET"; "Node.js"; "REST APIs"; "WebSockets"; "Background Processing" ]
    }
    {
        GroupName = "Data"
        Items = [ "PostgreSQL"; "SQL Server"; "MongoDB"; "Redis" ]
    }
    {
        GroupName = "Automation & AI"
        Items = [ "OpenAI API"; "RAG Workflows"; "n8n"; "Zapier" ]
    }
]

let automationOutcomes: SkillOutcome list = [
    {
        Label = "Manual Review Reduction"
        Value = "60%"
        Context = Some "Workflow automation in healthcare processing systems."
    }
    {
        Label = "Registry Preparation Time"
        Value = "Reduced"
        Context = Some "Automated validation and XML generation pipelines replaced a largely manual submission process."
    }
]

let automationModel: SkillPageModel = {|
    Id = "workflow-automation"
    Name = "Workflow Automation"
    HeroTitle = "Workflow automation for validation, processing, and operational systems"
    HeroSubtitle = "Experience building systems that reduce repetitive work, track processing state, and support human review where reliability matters."
    HeroBadge = Some "Workflow Automation"
    HeroGradientClass = "from-info to-secondary"

    CoreSectionTitle = "Core Areas"
    CoreAreas = automationCoreAreas

    FocusSectionTitle = Some "Technical Focus"
    FocusAreas = automationFocusAreas

    DomainsSectionTitle = Some "Domains"
    Domains = commonDomains

    TechnologiesSectionTitle = Some "Core Technologies"
    Technologies = automationTechnologies

    OutcomesSectionTitle = Some "Selected Outcomes"
    Outcomes = automationOutcomes

    RelatedSectionTitle = Some "Related Areas"
    RelatedPages = relatedSkillLinks
|}
let AutomationPage (backButtonComponent: ReactElement) = ServicePage automationModel

let aiCoreAreas: SkillFeature list = [
    {
        Title = "AI-Assisted Product Features"
        Description = "Adding AI-backed features where they improve workflow quality, speed, or user assistance inside real products."
        Icon = None
    }
    {
        Title = "Document Analysis & Extraction"
        Description = "Using LLMs and structured processing to support analysis, extraction, and interpretation of document-driven workflows."
        Icon = None
    }
    {
        Title = "Retrieval-Based Workflows"
        Description = "Combining LLM behavior with retrieval and domain-specific context to keep outputs more grounded and useful."
        Icon = None
    }
    {
        Title = "Human-in-the-Loop Systems"
        Description = "Designing AI flows that support review, correction, and controlled use rather than treating automation as fully autonomous by default."
        Icon = None
    }
]

let aiFocusAreas: SkillCapability list = [
    {
        Title = "LLM Integration"
        Description = "Integrating OpenAI-powered workflows into applications, internal tools, and backend processes."
    }
    {
        Title = "RAG & Context-Aware Systems"
        Description = "Building retrieval-driven flows that improve output relevance and reduce unsupported responses."
    }
    {
        Title = "Document-Centric AI Work"
        Description = "Using AI to support analysis, coding suggestions, validation support, and user-facing workflow assistance."
    }
    {
        Title = "Practical AI Product Design"
        Description = "Applying AI where it supports the product experience and operational workflow instead of forcing it in as a gimmick."
    }
]

let aiTechnologies: SkillTechnologyGroup list = [
    {
        GroupName = "AI"
        Items = [ "OpenAI API"; "Claude"; "RAG Systems"; "Prompt Engineering" ]
    }
    {
        GroupName = "Application Stack"
        Items = [ "F#"; "C#"; "TypeScript"; "Python"; ".NET"; "Node.js"; "FastAPI" ]
    }
    {
        GroupName = "Data & Workflow"
        Items = [ "REST APIs"; "Document Processing"; "Validation Pipelines"; "Operational Tooling"; "pgvector"; "Pinecone" ]
    }
]

let aiOutcomes: SkillOutcome list = [
    {
        Label = "Document-Centric AI Work"
        Value = "Production"
        Context = Some "Applied in healthcare and operational workflows, including analysis, extraction, and support tooling."
    }
    {
        Label = "Retrieval-Based Systems"
        Value = "Multi-Tenant"
        Context = Some "Built RAG-enabled workflows with indexed documents, citation-backed responses, and auditable decision paths."
    }
]

let aiModel: SkillPageModel = {|
    Id = "ai-llm-integrations"
    Name = "AI & LLM Integrations"
    HeroTitle = "AI and LLM integrations grounded in real workflow and product needs"
    HeroSubtitle = "Experience integrating LLM-powered and retrieval-based tooling into applications for document analysis, workflow support, personalization, and product assistance."
    HeroBadge = Some "AI & LLM Integrations"
    HeroGradientClass = "from-fuchsia-500 to-primary"

    CoreSectionTitle = "Core Areas"
    CoreAreas = aiCoreAreas

    FocusSectionTitle = Some "Technical Focus"
    FocusAreas = aiFocusAreas

    DomainsSectionTitle = Some "Domains"
    Domains = commonDomains

    TechnologiesSectionTitle = Some "Core Technologies"
    Technologies = aiTechnologies

    OutcomesSectionTitle = Some "Selected Outcomes"
    Outcomes = aiOutcomes

    RelatedSectionTitle = Some "Related Areas"
    RelatedPages = relatedSkillLinks
|}
let AIPage (backButtonComponent: ReactElement) = ServicePage aiModel


let leadershipCoreAreas: SkillFeature list = [
    {
        Title = "Technical Leadership"
        Description = "Leading implementation across systems and teams by shaping architecture, guiding delivery, and keeping engineering work aligned with real product needs."
        Icon = None
    }
    {
        Title = "Mentorship & Developer Growth"
        Description = "Supporting engineers through code review, pairing, architectural guidance, and helping them build confidence in unfamiliar parts of the stack."
        Icon = None
    }
    {
        Title = "Cross-Functional Collaboration"
        Description = "Working closely with product, operations, and stakeholders to translate evolving requirements into practical software solutions."
        Icon = None
    }
    {
        Title = "Engineering Standards & Quality"
        Description = "Improving maintainability, clarity, and delivery quality through stronger technical patterns, better structure, and thoughtful review."
        Icon = None
    }
]

let leadershipFocusAreas: SkillCapability list = [
    {
        Title = "Architecture Guidance"
        Description = "Helping shape application structure, system boundaries, and implementation direction so projects remain scalable and understandable."
    }
    {
        Title = "Hands-On Leadership"
        Description = "Leading by building alongside the team, unblocking implementation, and taking ownership of difficult technical areas when needed."
    }
    {
        Title = "Code Review & Technical Coaching"
        Description = "Providing actionable feedback that improves code quality while helping other engineers grow in judgment, confidence, and execution."
    }
    {
        Title = "Process & Delivery Support"
        Description = "Improving how teams ship by reducing ambiguity, clarifying priorities, and creating workflows that support consistent progress."
    }
]

let leadershipDomains: SkillDomain list = [
    {
        Name = "Healthcare Platforms"
        Description = "Led engineering work across regulated systems where reliability, workflow clarity, and operational correctness mattered."
    }
    {
        Name = "Consulting & Product Teams"
        Description = "Worked across client engagements and product environments, helping teams make progress quickly while adapting to different technical and organizational constraints."
    }
    {
        Name = "Product & Platform Development"
        Description = "Balanced technical execution with longer-term product and platform needs across full-stack systems."
    }
]

let leadershipTechnologies: SkillTechnologyGroup list = [
    {
        GroupName = "Primary Stack"
        Items = [ "F#"; "TypeScript"; "React"; "Python"; "C#" ]
    }
    {
        GroupName = "Leadership in Practice"
        Items = [ "Architecture"; "Code Review"; "System Design"; "Technical Planning"; "Cross-Functional Delivery" ]
    }
    {
        GroupName = "Implementation Areas"
        Items = [ "Frontend"; "Backend APIs"; "Workflow Systems"; "Cloud Delivery"; "AI Integrations" ]
    }
]

let leadershipOutcomes: SkillOutcome list = [
    {
        Label = "Medical Facilities Supported"
        Value = "50+"
        Context = Some "Engineering leadership across a deployed healthcare platform."
    }
    {
        Label = "Patient Records Processed"
        Value = "16M+"
        Context = Some "Built and guided systems supporting large-scale healthcare workflows."
    }
    {
        Label = "Delivery Confidence"
        Value = "Improved"
        Context = Some "Helped teams ship more reliably through stronger technical structure, deployment practices, and implementation guidance."
    }
]

let leadershipModel: SkillPageModel = {|
    Id = "leadership-mentorship"
    Name = "Leadership & Mentorship"
    HeroTitle = "Technical leadership through hands-on engineering, mentorship, and delivery ownership"
    HeroSubtitle = "Experience leading implementation, mentoring engineers, and helping teams build maintainable systems across healthcare, client work, and full-stack product development."
    HeroBadge = Some "Leadership & Mentorship"
    HeroGradientClass = "from-warning to-primary"

    CoreSectionTitle = "Core Areas"
    CoreAreas = leadershipCoreAreas

    FocusSectionTitle = Some "Technical Focus"
    FocusAreas = leadershipFocusAreas

    DomainsSectionTitle = Some "Domains"
    Domains = leadershipDomains

    TechnologiesSectionTitle = Some "How I Lead"
    Technologies = leadershipTechnologies

    OutcomesSectionTitle = Some "Selected Outcomes"
    Outcomes = leadershipOutcomes

    RelatedSectionTitle = Some "Related Areas"
    RelatedPages = relatedSkillLinks
|}

let LeadershipPage (backButtonComponent: ReactElement) = ServicePage leadershipModel

let cloudCoreAreas: SkillFeature list = [
    {
        Title = "Containerized Delivery"
        Description = "Packaging and deploying applications with consistent environments across development and production."
        Icon = None
    }
    {
        Title = "Infrastructure & Environment Setup"
        Description = "Working through the practical details of environments, secrets, deployment configuration, and runtime behavior."
        Icon = None
    }
    {
        Title = "CI/CD & Release Flow"
        Description = "Improving delivery reliability and reducing friction around changes, deployment, and iteration."
        Icon = None
    }
    {
        Title = "Operational Stability"
        Description = "Supporting systems with an eye toward uptime, recoverability, and maintainable deployment patterns."
        Icon = None
    }
]

let cloudFocusAreas: SkillCapability list = [
    {
        Title = "Docker & Kubernetes"
        Description = "Using containers and orchestration to package, deploy, and operate applications more reliably."
    }
    {
        Title = "Deployment Workflow Design"
        Description = "Improving how applications move from development to production through repeatable release flow and environment management."
    }
    {
        Title = "Cloud Platform Work"
        Description = "Hands-on experience deploying and supporting systems on Azure and DigitalOcean."
    }
    {
        Title = "Reliability-Oriented Delivery"
        Description = "Reducing deployment failures and improving operational confidence through better delivery and runtime setup."
    }
]

let cloudTechnologies: SkillTechnologyGroup list = [
    {
        GroupName = "Cloud & Infra"
        Items = [ "Azure"; "DigitalOcean"; "Docker"; "Kubernetes"; "Terraform"; "Helm" ]
    }
    {
        GroupName = "Delivery"
        Items = [ "GitHub Actions"; "CI/CD"; "Environment Configuration"; "Deployment Pipelines" ]
    }
    {
        GroupName = "Supporting Stack"
        Items = [ ".NET"; "Node.js"; "F#"; "PostgreSQL"; "MongoDB" ]
    }
]

let cloudOutcomes: SkillOutcome list = [
    {
        Label = "Deployment Reliability"
        Value = "Improved"
        Context = Some "Introduced Docker, CI/CD, and repeatable deployment workflows across multiple teams and platforms."
    }
    {
        Label = "Operational Stability"
        Value = "90%+"
        Context = Some "Reduced downtime and maintenance burden through platform modernization and improved release workflows."
    }
]

let platformDeliveryModel: SkillPageModel = {|
    Id = "cloud-platform-delivery"
    Name = "Cloud & Platform Delivery"
    HeroTitle = "Cloud and platform delivery focused on deployment, reliability, and maintainable infrastructure"
    HeroSubtitle = "Experience shipping and supporting applications with containers, CI/CD pipelines, and cloud infrastructure across product and platform work."
    HeroBadge = Some "Cloud & Platform Delivery"
    HeroGradientClass = "from-sky-500 to-primary"

    CoreSectionTitle = "Core Areas"
    CoreAreas = cloudCoreAreas

    FocusSectionTitle = Some "Technical Focus"
    FocusAreas = cloudFocusAreas

    DomainsSectionTitle = Some "Domains"
    Domains = commonDomains

    TechnologiesSectionTitle = Some "Core Technologies"
    Technologies = cloudTechnologies

    OutcomesSectionTitle = Some "Selected Outcomes"
    Outcomes = cloudOutcomes

    RelatedSectionTitle = Some "Related Areas"
    RelatedPages = relatedSkillLinks
|}

let PlatformDeliveryPage (backButtonComponent: ReactElement) = ServicePage platformDeliveryModel

