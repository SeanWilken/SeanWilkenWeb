module Components.FSharp.Interop.Resume

open Feliz
open ResumeBindings
open Fable.Core.JsInterop

// F# side: structured sections that we normalize into the TS ResumePage props
let resumeSections : ResumeSection array =
    [|
        // Tag cloud of skills / tech stack
        TagCloud(
            "Skills",
            [|
                // Core dev
                "Full-stack development"
                "Functional programming (F#)"
                "API integration and design"
                "Microservices"
                "SaaS platforms"
                "Backend-first architecture"
                "Frontend engineering (React/Next.js)"

                // Languages & frameworks
                "F#"
                "C#"
                "TypeScript"
                "JavaScript"
                "Rust"
                "Python"
                "SQL"
                ".NET Core / ASP.NET Core"
                "Fable"
                "Elmish"
                "React"
                "Next.js"
                "Node.js"

                // Cloud / DevOps / Infra
                "Azure"
                "DigitalOcean"
                "Docker"
                "Kubernetes"
                "GitHub Actions"
                "GitLab CI"
                "CI/CD pipelines"
                "Nginx"
                "Redis"
                "Elasticsearch"
                "Microsoft Fabric"
                "Grafana"
                "Metabase"

                // Data / automation / AI
                "ETL processes"
                "Data validation & XSD-driven pipelines"
                "AI workflows"
                "LLM integrations"
                "Automation tooling"

                // Practices & soft skills
                "Secure coding practices"
                "Performance optimization"
                "Unit testing and TDD"
                "Readable code & documentation"
                "Requirement gathering"
                "Agile (Scrum, Kanban)"
                "Team leadership"
                "Cross-functional collaboration"
                "Problem-solving"
                "Adaptability"
                "Continuous learning"
            |]
        )

        // Professional summary, aligned with the HTML resume
        Section(
            "Professional Summary",
            [|
                "Full Stack Developer with 10+ years of experience designing scalable systems across healthcare, e-commerce, and emerging technologies. Highly effective in team collaboration, adaptable to evolving project needs, and committed to delivering high-quality, impactful results."
                "Specialized in functional programming (F#, Rust) and in building AI-driven and automation-focused solutions. Creator of production healthcare platforms used across multiple states, and actively developing custom automation tools, AI integrations, and LLM-powered workflows that streamline operations and enhance productivity."
            |]
        )

        // Accomplishments section
        Section(
            "Accomplishments",
            [|
                "Achieved national trauma-registry submission accreditation by engineering a custom XSD schema validator and rules engine for accurate, compliant medical data."
                "Built AI-powered form-analysis and medical-record review agents that extract key clinical insights and assign patients to appropriate cohorts for improved care coordination."
                "Enabled small businesses to streamline operations by delivering automation pipelines, custom applications, and AI-integrated tools that reduce manual work and accelerate growth."
            |]
        )

        // Experience timeline – matches the updated resume
        Timeline(
            "Experience",
            [|
                // Juniper Health Solutions
                {|
                    company = "Juniper Health Solutions"
                    role = "Senior Software Engineer (Contract)"
                    summary =
                        "Built and maintained F# applications powering statewide and national trauma-registry submissions and the full validation pipeline."
                    startDateString = "July 2025"
                    endDateString = "December 2025 · Remote"
                    responsibilities =
                        [|
                            "Built and maintained a suite of F# applications powering statewide and national trauma-registry submissions for hospital systems."
                            "Designed and implemented the full submission and validation pipeline (ITDX, NTDS, TQIP), including schema-driven validation, XML generation, and automated error mapping."
                            "Delivered major performance improvements across ingestion, transformation, and large-volume batch processing workflows."
                            "Implemented indexed searching, optimized data-retrieval paths, and facility-specific UI/data overrides for different hospital partners."
                            "Built REST API endpoints for submission orchestration, validation, status tracking, and downstream integrations."
                            "Integrated with external APIs for validation, registry workflows, and automated reporting while collaborating with clinical and technical stakeholders to ensure compliance with evolving standards."
                        |]
                |}

                // Antidote AI
                {|
                    company = "Antidote AI"
                    role = "Full Stack Developer (Co-Founder)"
                    summary =
                        "Partnered with healthcare systems to build AI-driven diagnostic tools and workflow automation for clinical operations."
                    startDateString = "January 2021"
                    endDateString = "Current · Orange, CA"
                    responsibilities =
                        [|
                            "Partnered with healthcare systems and medical professionals to build AI-driven diagnostic and workflow automation tools."
                            "Developed dynamic forms, telehealth workflows, and secure data pipelines that reduced manual coordination by ~40%."
                            "Led architecture decisions using F#, TypeScript, and cloud-native systems; mentored engineers and ensured high-quality, scalable code."
                            "Created custom automation tools, AI integrations, and LLM workflows to streamline clinical and operational processes."
                        |]
                |}

                // RemindifyAI
                {|
                    company = "RemindifyAI"
                    role = "Full Stack Developer"
                    summary =
                        "Built an interactive portal and analytics stack for text reminders and AI-powered engagement workflows."
                    startDateString = "August 2025"
                    endDateString = "December 2025 · Orange, CA"
                    responsibilities =
                        [|
                            "Developed an interactive web portal with analytics dashboards for managing reminder text messages and personalized user updates."
                            "Integrated with an existing leads spreadsheet to create a dynamic, reactive pipeline that automatically engages new sign-ups and manages trial-to-paid transitions."
                            "Implemented payment reminder scheduling and engagement workflows triggered as users approached the end of trial periods."
                            "Connected the platform to AI/LLM services to power intelligent messaging, personalized automations, and high-efficiency operational workflows."
                        |]
                |}

                // Scullyleather
                {|
                    company = "Scullyleather"
                    role = "Contract Web Developer"
                    summary =
                        "Modernized Magento e-commerce operations and improved deployment reliability for a legacy retail stack."
                    startDateString = "January 2017"
                    endDateString = "December 2019 · Oxnard, CA"
                    responsibilities =
                        [|
                            "Upgraded a Magento e-commerce platform with custom components and management features tailored to operational needs."
                            "Integrated warehouse and inventory systems for automated product synchronization and real-time stock visibility."
                            "Authored deployment and disaster-recovery playbooks and improved CI/CD reliability for long-term operational stability."
                        |]
                |}

                // Supotsu
                {|
                    company = "Supotsu"
                    role = "Full Stack Software Developer"
                    summary =
                        "Built real-time sports betting and interactive 3D prototypes to align stakeholders around product direction."
                    startDateString = "March 2018"
                    endDateString = "September 2019 · Orange, CA"
                    responsibilities =
                        [|
                            "Engineered a real-time sports betting system for deployment in-home and at kiosks, integrating backend services with live sports data feeds."
                            "Designed interactive 3D prototypes to align stakeholders and guide product direction."
                            "Optimized performance to maintain smooth user interactions under active load conditions."
                        |]
                |}

                // RepSpark
                {|
                    company = "RepSpark"
                    role = "Full Stack Web Developer"
                    summary =
                        "Worked on enterprise retail integrations, pricing engines, and performance improvements."
                    startDateString = "July 2016"
                    endDateString = "March 2018 · Costa Mesa, CA"
                    responsibilities =
                        [|
                            "Developed enterprise retail integrations, rule-based pricing engines, and core platform features for large product catalogs."
                            "Improved CI/CD reliability and frontend performance, enhancing overall user experience on web platforms."
                        |]
                |}
            |]
        )

        // Education
        Section(
            "Education",
            [|
                "Associate of Applied Science, Computer Science – SUNY Orange · January 2014"
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
