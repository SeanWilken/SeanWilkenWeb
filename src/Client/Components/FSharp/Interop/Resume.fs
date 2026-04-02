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
                    role = "Lead Engineer & Co-Founder"
                    summary = ""
                    startDateString = "January 2021"
                    endDateString = "Present"
                    responsibilities =
                        [|
                            "Owned the company's core platform from prototype through production, helping move the business from pre-seed to paying customers and profitability."
                            "Built backend services in F#, .NET, Node.js, and Python for APIs, AI workflows, and background processing."
                            "Created a document ingestion pipeline that processed 16M+ patient records, reducing manual chart review while maintaining HIPAA compliance."
                            "Built a WebSocket-based recovery system that preserved progress during long-running reviews and significantly improved reliability."
                            "Replaced hard-coded customer logic with a rules engine and state-machine workflow system, allowing new states, payers, and customers to be onboarded without branching the application."
                            "Built a document ingestion and analysis pipeline that processed more than 16M patient records, reducing manual chart review effort while maintaining HIPAA compliance."
                            "Migrated the platform from Azure to Docker and Kubernetes, enabling reliable zero-downtime releases."
                        |]
                |}

                {|
                    company = "SRC Inc. | LogiSolve"
                    role = "Software Engineer & Consultant"
                    summary = ""
                    startDateString = "March 2020"
                    endDateString = "December 2025"
                    responsibilities =
                        [|
                            "Replaced a largely manual trauma registry submission process with an automated validation and XML generation pipeline, reducing the time required to prepare records for state and national submission while making the process more reliable."
                            "Identified that large healthcare datasets had become effectively unusable due to multi-minute load times, then introduced indexing, caching, background jobs, server-side paging, and filtering to reduce searches to seconds and make the product practical for daily use."
                            "Designed and implemented a real-time sports betting platform using live data feeds and WebSocket updates, creating an experience that supported groups of users without requiring page refreshes."
                            "Introduced Docker, CI/CD, and automated deployment pipelines across multiple teams, shortening release cycles and reducing deployment failures while giving smaller teams more confidence shipping changes."                    
                        |]
                |}

                {|
                    company = "Scully Leather"
                    role = "Product & Software Engineer"
                    summary = ""
                    startDateString = "July 2018"
                    endDateString = "March 2020"
                    responsibilities =
                        [|
                            "Helped migrate the business from a legacy e-commerce platform to a modern solution by integrating inventory, warehouse, fulfillment, and third-party systems into a single workflow."
                            "Built the custom extensions, integrations, pricing logic, and business-specific functionality needed to support the company's existing processes without disrupting day-to-day operations."
                            "Reduced downtime and maintenance effort by more than 90% while improving the reliability and speed of releases."
                            "Worked directly with the client and internal stakeholders to identify operational pain points, prioritize new functionality, and ensure the platform matched existing business workflows."
                            "Collaborated with the client's DevOps engineer to containerize the platform and create a deployment process that could be handed off internally after the project was complete."
                        |]
                |}

                {|
                    company = "RepSpark"
                    role = "Implementation & Software Engineer"
                    summary = ""
                    startDateString = "July 2016"
                    endDateString = "December 2018"
                    responsibilities =
                        [|
                            "Worked directly with customers to understand sales, pricing, and workflow requirements, then translated those needs into custom integrations and product improvements."
                            "Implemented new product features and pricing workflows that supported more complex customer use cases and reduced the need for manual workarounds."
                            "Supported releases and customer rollouts by testing new functionality, troubleshooting production issues, and ensuring implementations were delivered successfully."
                        |]
                |}
            |]
        )

        Section(
            "Technical Skills",
            [|
                "Languages: F#, TypeScript, JavaScript, C#, Python, Rust, Elm, Kotlin, Swift"
                "Frontend: React, Elmish, Bulma, Bootstrap, Tailwind CSS, DaisyUI, HTML/CSS"
                "Backend: Node.js, .NET, Azure Functions, Django, Express, REST APIs, GraphQL"
                "Cloud & DevOps: Azure, Digital Ocean, Docker, Kubernetes, Terraform, Helm, CI/CD, GitHub Actions"
                "Databases: PostgreSQL, SQL Server, MongoDB, Redis, Vector DBs"
                "AI / ML: OpenAI API, LangChain, Integration, RAG Systems, Fine-tuning, Prompt Engineering"
            |]
        )

        Section(
            "Education",
            [|
                "Associate of Applied Science, Computer Science — 2014"
                "SUNY Orange"
            |]
        )

        Timeline(
            "Projects",
            [|
                {|
                    company = "Portfolio & E-Commerce Platform"
                    role = "SeanWilken / Xero Effort"
                    summary = ""
                    startDateString = ""
                    endDateString = ""
                    responsibilities =
                        [|
                            "Built a complete portfolio and print-on-demand e-commerce platform using F#, Fable, Elmish, React, Stripe, and Printful integrations."
                            "Designed a reusable component system, custom theme engine, and strongly typed frontend architecture using Tailwind CSS and DaisyUI, making it easier to add new features and maintain consistency."
                            "Built and deployed the platform using Docker, Kubernetes, Terraform, and DigitalOcean infrastructure, mirroring the same production workflows used in client and startup work."
                        |]
                |}
                {|
                    company = "Engagement and automation Platform for SaaS"
                    role = "Remindify"
                    summary = ""
                    startDateString = ""
                    endDateString = ""
                    responsibilities =
                        [|
                            "Built customer-facing dashboards, onboarding flows, and internal tools using React, TypeScript, and Node.js, helping move the product from manual workflows to a complete SaaS experience."
                            "Designed automation workflows that connected lead capture, messaging, trials, billing, and analytics into a single product, reducing manual work and improving visibility into customer behavior."
                            "Added AI-powered personalization using OpenAI APIs to tailor messaging based on user behavior, improving trial-to-paid conversion and engagement."
                            "Implemented Stripe billing, webhook processing, reporting, and analytics features, giving the team better visibility into funnel performance and retention."
                            "Planning to make the codebase public after removing sensitive data and cleaning up repository history."
                        |]
                |}
                {|
                    company = "Unity Rogue-like Game (Unreleased)"
                    role = "Karma Keepers"
                    summary = ""
                    startDateString = ""
                    endDateString = ""
                    responsibilities =
                        [|
                            "Designed gameplay systems, progression loops, UI flows, and content pipelines for a third-person rogue-like game built in Unity and C#."
                            "Built reusable systems for inventory, card-based upgrades, player progression, and modular UI to support rapid iteration and future expansion."
                            "Created supporting tools and workflows that improved balancing, iteration speed, and long-term maintainability of the project."
                        |]
                |}
                {|
                    company = "AI assisted Development Team for individuals and startups"
                    role = "AI Training Ground"
                    summary = ""
                    startDateString = ""
                    endDateString = ""
                    responsibilities =
                        [|
                            "Architected a multi-tenant AI orchestration platform using FastAPI, PostgreSQL, TypeScript, and React, allowing teams to collaborate in shared workspaces while keeping data isolated between organizations."
                            "Built a retrieval pipeline using pgvector and RAG techniques that cleaned and indexed uploaded documents, then generated citation-backed responses with lower cost and better answer quality."
                            "Designed role- and policy-based access controls (RBAC / ABAC), event logging, and traceable decision workflows so that every action and AI-generated response could be audited."
                        |]
                |}
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
