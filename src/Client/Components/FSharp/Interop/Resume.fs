module Components.FSharp.Interop.Resume

open Feliz
open ResumeBindings
open Fable.Core.JsInterop

let resumeSections : ResumeSection array =
    [|
        TagCloud (
            "Skills",
            [|
                "Full-stack development"; "API integration and design"; "Microservices"; "SaaS platforms"; "Agile (Scrum, Kanban)";
                "Requirement gathering"; "Team leadership"; "CI/CD and DevOps principles"; "F#, TypeScript, .NET Core, React";
                "Readable coding"; "Code as documentation"; "Cloud-native development"; "SQL and NoSQL databases"; "AI workflows";
                "ETL processes"; "Azure"; "Docker"; "Kubernetes"; "Git"; "Unit testing and TDD"; "Performance optimization";
                "Cross-functional collaboration"; "Problem-solving"; "Adaptability"; "Continuous learning"
            |]
        );
        Section (
            "Summary",
            [|
                "Experienced Full Stack Developer with 9+ years in healthcare, e-commerce, and emerging tech.";
                "Built and led development of an AI-powered EMR platform in use across multiple states.";
                "Skilled in backend-first architecture with strong frontend capabilities."
            |]
        );
        Timeline (
            "Experience",
            [|
                {|
                    company = "LogiSolve"
                    role = "Senior Developer (Full Stack)"
                    summary = "Led full-stack development for client-facing platforms, driving technical strategy and delivering scalable solutions."
                    startDateString = "2025"
                    endDateString = "Current"
                    responsibilities = [|
                        "Architected and implemented scalable web applications using F#, .NET Core, and React."
                        "Collaborated with cross-functional teams to gather requirements and deliver tailored solutions."
                        "Mentored junior developers and conducted code reviews to ensure maintainability and performance."
                        "Integrated third-party APIs and optimized backend workflows for speed and reliability."
                        "Led DevOps initiatives including CI/CD pipelines and container orchestration with Docker and Kubernetes."
                    |]
                |}
                {|
                    company = "Antidote AI"
                    role = "Co-Founder"
                    summary = "Built and scaled AI-powered healthcare tools, integrating diagnostic intelligence into EMR workflows."
                    startDateString = "2021"
                    endDateString = "2025"
                    responsibilities = [|
                        "Designed and developed AI-driven diagnostic modules for clinical decision support."
                        "Integrated EMR systems with custom APIs and secure data pipelines."
                        "Led product development from concept to deployment across multiple healthcare networks."
                        "Managed infrastructure and cloud-native deployments on Azure."
                        "Collaborated with medical professionals to align technical solutions with clinical needs."
                    |]
                |}
                {|
                    company = "Scully Leather"
                    role = "Contract Developer"
                    summary = "Modernized legacy e-commerce systems and improved operational workflows."
                    startDateString = "2017"
                    endDateString = "2019"
                    responsibilities = [|
                        "Upgraded Magento platform and implemented custom modules for inventory and shipping."
                        "Created CI/CD playbooks to streamline deployment and reduce downtime."
                        "Troubleshot customer-facing issues and improved UX across storefront components."
                        "Integrated payment gateways and ensured PCI compliance."
                        "Provided technical support and training for internal staff."
                    |]
                |}
                {|
                    company = "Supotsu"
                    role = "Developer"
                    summary = "Contributed to the development of a real-time sports betting engine with live data integration."
                    startDateString = "2018"
                    endDateString = "2019"
                    responsibilities = [|
                        "Built real-time betting engine with live odds and event tracking."
                        "Optimized frontend performance under high traffic conditions."
                        "Implemented user authentication and session management."
                        "Collaborated with designers to enhance user experience and accessibility."
                        "Resolved bugs and deployed incremental feature updates."
                    |]
                |}
                {|
                    company = "RepSpark"
                    role = "Developer"
                    summary = "Developed rule-based logic and supported backend systems for retail platforms."
                    startDateString = "2016"
                    endDateString = "2018"
                    responsibilities = [|
                        "Implemented business logic for retail workflows and product configurations."
                        "Improved backend performance and database query efficiency."
                        "Participated in sprint planning and agile ceremonies."
                        "Assisted QA and support teams with bug resolution and feature validation."
                        "Maintained legacy systems while contributing to modernization efforts."
                    |]
                |}
            |]
        );
        Section (
            "Projects",
            [|
                "Healthcare EMR: Built secure AI-based EMR for hospitals with dynamic forms and telehealth support.";
                "Magento Overhaul: Migrated legacy e-commerce platform, adding custom modules and DevOps workflows.";
                "Sports Betting System: Developed real-time betting engine with kiosk and live integration.";
                "2D Unity Physics Engine and Concept Game: Developed 2D physics engine and prototype game using Unity.";
                "Portfolio Website: Created a personal portfolio site showcasing projects and skills.";
                "Custom Website interfacing with Printful to create a storefront for some of my designs."
            |]
        );
        Section (
            "Education",
            [|
                "SUNY Orange, Middletown NY - AAS in Computer Science (2014)"
                ", Middletown NY - AAS in Computer Science (2014)"
            |]
        )
    |]

let normalizeSection (section: ResumeSection) =
    match section with
    | Section (label, items) ->
        createObj [
            "label" ==> label
            "kind" ==> "section"
            "items" ==> items
        ]
    | Timeline (label, items) ->
        createObj [
            "label" ==> label
            "kind" ==> "timeline"
            "items" ==> items
        ]
    | TagCloud (label, items) ->
        createObj [
            "label" ==> label
            "kind" ==> "tagcloud"
            "items" ==> items
        ]


[<ReactComponent>]
let ResumePage () =
    let normalizedSections = resumeSections |> Array.map normalizeSection
    ResumeBindings.ResumePage {| sections = normalizedSections |}
