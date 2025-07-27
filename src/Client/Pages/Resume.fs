module Pages.Resume

open Feliz
open ResumeBindings

let resumeSections : ResumeSection array =
    [|
        {|
            label = "Skills"
            items = [|
                "Full-stack development"; "API integration and design"; "Microservices"; "SaaS platforms"; "Agile (Scrum, Kanban)";
                "Requirement gathering"; "Team leadership"; "CI/CD and DevOps principles"; "F#, TypeScript, .NET Core, React";
                "Readable coding"; "Code as documentation"; "Cloud-native development"; "SQL and NoSQL databases"; "AI workflows";
                "ETL processes"; "Azure"; "Docker"; "Kubernetes"; "Git"; "Unit testing and TDD"; "Performance optimization";
                "Cross-functional collaboration"; "Problem-solving"; "Adaptability"; "Continuous learning"
            |]
        |}
        {|
            label = "Summary"
            items = [|
                "Experienced Full Stack Developer with 9+ years in healthcare, e-commerce, and emerging tech."
                "Built and led development of an AI-powered EMR platform in use across multiple states."
                "Skilled in backend-first architecture with strong frontend capabilities."
            |]
        |}
        {|
            label = "Experience"
            items = [|
                "Co-Founder at Antidote AI: Led AI-driven diagnostic tools and integrated EMR workflows (2021-Present)"
                "Contract Developer at Scully Leather: Upgraded Magento, created CI/CD playbooks, handled payment and shipping modules (2017-2019)"
                "Developer at Supotsu: Built real-time betting engine with live data integration and optimized UX under load (2018-2019)"
                "Developer at RepSpark: Delivered rule-based logic for retail platforms and improved DevOps stability (2016-2018)"
            |]
        |}
        {|
            label = "Projects"
            items = [|
                "Healthcare EMR: Built secure AI-based EMR for hospitals with dynamic forms and telehealth support."
                "Magento Overhaul: Migrated legacy e-commerce platform, adding custom modules and DevOps workflows."
                "Sports Betting System: Developed real-time betting engine with kiosk and live integration."
                "2D Unity Physics Engine and Concept Game: Developed 2D physics engine and prototype game using Unity."
                "Portfolio Website: Created a personal portfolio site showcasing projects and skills."
                "Custom Website interfacing with Printful to create a storefront for some of my designs."
            |]
        |}
        {|
            label = "Education"
            items = [|
                "SUNY Orange, Middletown NY - AAS in Computer Science (2014)"
                "Dean's List - Honor Roll"
                "Science Research Program & AP Coursework"
            |]
        |}
    |]

[<ReactComponent>]
let ResumePage () =
    ResumePage {| sections = resumeSections |}
