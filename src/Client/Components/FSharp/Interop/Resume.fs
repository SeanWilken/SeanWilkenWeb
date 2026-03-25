module Components.FSharp.Interop.Resume

open Feliz
open ResumeBindings
open Fable.Core.JsInterop

// F# side: structured sections that we normalize into the TS ResumePage props
let resumeSections : ResumeSection array =
    [|
        Section(
            "Summary",
            [|
                "Senior Software Engineer with 10+ years of full-stack experience building production systems across healthcare, e-commerce, and AI platforms. Proven track record owning complex technical initiatives end-to-end — from architecture through deployment. Deep expertise in LLM integration, HIPAA compliant clinical systems, and distributed micro-services. Comfortable leading cross-functional engineering teams and driving delivery in fast-moving environments."
            |]
        )

        Timeline(
            "Experience",
            [|
                {|
                    company = "Antidote AI"
                    role = "Technical Co-Founder & Lead Engineer"
                    summary = ""
                    startDateString = "January 2021"
                    endDateString = "Present · Orange, CA"
                    responsibilities =
                        [|
                            "Lead technical strategy and full-stack architecture for an AI-powered healthcare platform deployed across 50+ medical facilities in multiple states."
                            "Built diagnostic automation pipelines processing 2M+ patient records at 95%+ accuracy, reducing manual chart review time by 60% while maintaining strict HIPAA compliance."
                            "Designed and shipped secure F# + Azure Functions data pipelines, cutting coordination overhead by 50% and improving clinical team throughput with up-to-date information."
                            "Architected telehealth workflows and multi-state form systems in React/TypeScript with dynamic, state-specific regulatory logic."
                            "Integrated OpenAI API and custom RAG pipelines for intelligent document analysis, coding suggestions, and clinical decision-support features."
                            "Engineered a real-time communication system with live WebSocket feeds and atomic operations."
                        |]
                |}

                {|
                    company = "SRC Inc. (Various Client Projects)"
                    role = "Software Engineering Consultant"
                    summary = ""
                    startDateString = "July 2016"
                    endDateString = "December 2021 · Southern California"
                    responsibilities =
                        [|
                            "Delivered full-stack solutions for e-commerce, sports betting, and retail technology clients; specialized in high-performance Node.js backends and React frontends."
                            "Upgraded enterprise e-commerce platform with custom inventory management and disaster-recovery protocols, reducing downtime and maintenance by 95% from their old legacy system."
                            "and 3D interactive prototypes supporting 50K+ concurrent users."
                            "Built enterprise retail integrations with dynamic pricing engines, catalog management, and bulk import pipelines processing 100K+ SKUs."
                            "Improved CI/CD reliability across multiple client platforms using Docker and Azure, reducing deployment failures by 60%."
                        |]
                |}

                {|
                    company = "Juniper Health Solutions · Remindfy AI · Monster Reviews"
                    role = "Senior Software Engineer (Contract)"
                    summary = ""
                    startDateString = "July 2024"
                    endDateString = "December 2025 · Remote"
                    responsibilities =
                        [|
                            "Architect and implement multi-state trauma registry submission systems supporting data checking and reporting discrepancies to the user to ensure high data accuracy."
                            "Built national trauma data standards validation and submission pipelines ensuring regulatory compliance and high-quality data ingestion."
                            "Implement paging, filtering and caching mechanisms to speed up client features and interactions."
                            "Orchestrate work flows for autonomous operation of client intake and on-boarding."
                        |]
                |}
            |]
        )

        Section(
            "Technical Skills",
            [|
                "Languages: F#, TypeScript, JavaScript, C#, Python, Rust, Elm, Kotlin, Swift"
                "Frontend: React, Elmish, Bulma, Bootstrap, Tailwind CSS, DaisyUI, HTML/CSS"
                "Backend: Node.js, .NET, Azure Functions, Express, REST APIs, GraphQL"
                "Cloud & DevOps: Azure, Digital Ocean, Docker, Kubernetes, Terraform, Helm, CI/CD, GitHub Actions"
                "Databases: PostgreSQL, SQL Server, MongoDB, Redis, Vector DBs"
                "AI / ML: OpenAI API, LangChain, RAG Systems, Fine-tuning, Prompt Engineering"
            |]
        )

        Section(
            "Education",
            [|
                "Associate of Applied Science, Computer Science — 2014"
                "SUNY Orange"
            |]
        )

        Section(
            "Notable Projects",
            [|
                "XERO EFFORT - Luxury E-Commerce Platform"
                "Built a premium e-commerce experience on the F# SAFE stack (Suave, Azure, Fable, Elmish) with Printful API v2 integration for on-demand fulfillment."
                "Implemented glassmorphism UI, scroll-based animations, and a dynamic theme system with Tailwind CSS and DaisyUI for a luxury brand aesthetic."
                "Architected a type-safe frontend in Fable + Elmish ensuring predictable state management and zero runtime errors."
                ""
                "Karma Keepers - Unity Rogue-like"
                "Unity based C# physics and procedural generated third person rogue-like that works through engines to produce level layout, loot, environment terrain, difficulty and meta-progression."
            |]
        )
    |]

// Normalizer stays the same: turn DU into JS objects the TS ResumePage expects
let normalizeSection (section: ResumeSection) =
    match section with
    | Section(label, items) ->
        createObj [
            "label" ==> label
            "kind" ==> "section"
            "items" ==> items
        ]
    | Timeline(label, items) ->
        createObj [
            "label" ==> label
            "kind" ==> "timeline"
            "items" ==> items
        ]
    | TagCloud(label, items) ->
        createObj [
            "label" ==> label
            "kind" ==> "tagcloud"
            "items" ==> items
        ]

[<ReactComponent>]
let ResumePage () =
    let normalizedSections = resumeSections |> Array.map normalizeSection
    ResumeBindings.ResumePage {| sections = normalizedSections |}
