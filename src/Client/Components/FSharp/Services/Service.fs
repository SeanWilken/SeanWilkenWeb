module Components.FSharp.Service

open Feliz
open Shared.SharedServices


let private heroMetricCard (stat: ServiceStat) =
    let arrow, colorClass =
        match stat.Trend with
        | Up   -> "↑", "text-accent"
        | Down -> "↓", "text-error"

    Html.div [
        prop.className "rounded-2xl bg-base-100/10 border border-base-100/40 p-4 backdrop-blur shadow-md flex items-center gap-3"
        prop.children [
            // big arrow on the left
            Html.div [
                prop.className "w-12 h-12 rounded-full bg-base-100/25 flex items-center justify-center"
                prop.children [
                    Html.span [
                        prop.className ("text-3xl font-bold " + colorClass)
                        prop.text arrow
                    ]
                ]
            ]

            // label + value on the right
            Html.div [
                prop.className "flex-1 text-center"
                prop.children [
                    Html.div [
                        prop.className "text-[0.65rem] font-semibold uppercase tracking-wide text-base-100/75"
                        prop.text stat.Label
                    ]
                    Html.div [
                        prop.className ("mt-1 text-2xl font-extrabold " + colorClass)
                        prop.text stat.Value
                    ]
                ]
            ]
        ]
    ]

let private capabilityCard (cap: ServiceCapability) =
    Html.div [
        prop.className "rounded-2xl bg-base-100/95 border border-base-300 shadow-sm p-4 flex items-start gap-3"
        prop.children [
            Html.div [
                prop.className "w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center text-lg"
                prop.text cap.Icon
            ]
            Html.div [
                prop.children [
                    Html.h4 [
                        prop.className "text-sm font-semibold text-primary"
                        prop.text cap.Heading
                    ]
                    Html.p [
                        prop.className "text-xs md:text-sm text-base-content/70"
                        prop.text cap.Description
                    ]
                ]
            ]
        ]
    ]

let private industryCard (ind: ServiceIndustry) =
    Html.div [
        prop.className "rounded-2xl bg-base-100/95 border border-base-300 shadow-sm p-5 text-left"
        prop.children [
            Html.h3 [
                prop.className "text-base font-semibold text-secondary mb-1"
                prop.text ind.Name
            ]
            Html.p [
                prop.className "text-xs md:text-sm text-base-content/70 mb-2"
                prop.text ind.Summary
            ]
            Html.ul [
                prop.className "list-disc pl-4 text-xs md:text-sm text-base-content/75 space-y-1"
                prop.children [
                    for o in ind.Outcomes do
                        Html.li o
                ]
            ]
        ]
    ]


[<ReactComponent>]
let ServicePage (model: ServicePageModel) =
    Html.div [
        prop.className "min-h-screen text-base-content"
        prop.children [

            Html.div [
                prop.className "max-w-6xl mx-auto px-4 lg:px-8 py-10 space-y-16"
                prop.children [

                    // HERO
                    Html.section [
                        prop.className
                            (sprintf "relative overflow-hidden rounded-3xl shadow-[0_25px_80px_rgba(15,23,42,0.35)] border border-base-300/60 bg-gradient-to-br %s p-8 md:p-12"
                                 model.HeroGradientClass)
                        prop.children [
                            Html.div [
                                prop.className "pointer-events-none absolute -top-24 -right-24 w-72 h-72 rounded-full bg-white/15 blur-3xl"
                            ]
                            Html.div [
                                prop.className "pointer-events-none absolute -bottom-32 -left-10 w-80 h-80 rounded-full bg-secondary/25 blur-3xl"
                            ]

                            Html.div [
                                prop.className "relative flex flex-col gap-10 md:flex-row md:items-center"
                                prop.children [

                                    // Copy
                                    Html.div [
                                        prop.className "flex-1 space-y-4"
                                        prop.children [
                                            match model.HeroBadge with
                                            | Some badge ->
                                                Html.div [
                                                    prop.className "inline-flex items-center gap-2 rounded-full bg-base-100/15 border border-base-100/40 px-4 py-1 text-[0.68rem] font-semibold tracking-[0.2em] uppercase text-base-100 shadow-sm backdrop-blur"
                                                    prop.children [
                                                        Html.span [ prop.className "w-2 h-2 rounded-full bg-accent animate-pulse" ]
                                                        Html.span badge
                                                    ]
                                                ]
                                            | None -> Html.none

                                            Html.h1 [
                                                prop.className "text-4xl md:text-5xl lg:text-6xl font-extrabold leading-tight text-base-100"
                                                prop.text model.HeroTitle
                                            ]

                                            Html.p [
                                                prop.className "mt-2 text-base md:text-lg text-base-100/85 max-w-xl"
                                                prop.text model.HeroSubtitle
                                            ]

                                            Html.div [
                                                prop.className "mt-5 flex flex-wrap gap-3 items-center"
                                                prop.children [
                                                    Html.button [
                                                        prop.className "btn btn-secondary btn-md shadow-lg shadow-secondary/40"
                                                        prop.text model.CtaText
                                                    ]
                                                    Html.div [
                                                        prop.className "inline-flex items-center gap-2 text-xs md:text-sm text-base-100/80"
                                                        prop.children [
                                                            Html.span "Avg first-month ROI: "
                                                            Html.span [
                                                                prop.className "font-semibold text-accent"
                                                                prop.text "3-5x"
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]

                                    // Metrics (top 4 stats)
                                    Html.div [
                                        prop.className "flex-1 md:pl-8"
                                        prop.children [
                                            Html.div [
                                                prop.className "grid grid-cols-2 gap-3"
                                                prop.children [
                                                    for s in model.Stats |> List.truncate 6 do
                                                        heroMetricCard s
                                                        // Html.div [
                                                        //     prop.className "rounded-2xl bg-base-100/12 border border-base-100/40 p-4 backdrop-blur shadow-md"
                                                        //     prop.children [
                                                        //         Html.div [
                                                        //             prop.className "text-[0.65rem] font-semibold text-xs uppercase flex justify-between tracking-wide text-base-100/70"
                                                        //             prop.children [
                                                        //                 // Html.span [ prop.text s.Label ]
                                                        //                 // Html.span [ prop.text s.Value ]
                                                        //             ]
                                                        //         ]
                                                        //         // Html.div [
                                                        //         //     prop.className "mt-1 text-2xl font-bold text-accent text-right"
                                                        //         // ]
                                                        //     ]
                                                        // ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // CORE MODULES
                    Html.section [
                        prop.children [
                            Html.div [
                                prop.className "flex items-center justify-between mb-3 gap-4"
                                prop.children [
                                    Html.div [
                                        prop.children [
                                            Html.span [
                                                prop.className "text-[0.7rem] font-semibold tracking-[0.22em] uppercase text-base-content/50"
                                                prop.text "Product Modules"
                                            ]
                                            Html.h2 [
                                                prop.className "mt-1 text-2xl md:text-3xl font-semibold text-primary"
                                                prop.text model.CoreSectionTitle
                                            ]
                                        ]
                                    ]
                                    Html.span [
                                        prop.className "hidden md:block text-xs md:text-sm text-right text-base-content/60"
                                        prop.text "Everything you need to capture, qualify, and close."
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6"
                                prop.children (
                                    model.CoreFeatures
                                    |> List.map (fun f ->
                                        Html.div [
                                            prop.className "group relative rounded-2xl bg-base-100/95 border border-base-300 shadow-sm p-6 text-center transition transform hover:-translate-y-1 hover:shadow-xl hover:border-secondary/70"
                                            prop.children [
                                                Html.div [
                                                    prop.className "absolute inset-0 rounded-2xl bg-gradient-to-br from-secondary/5 to-accent/10 opacity-0 group-hover:opacity-100 transition pointer-events-none"
                                                ]
                                                Html.h3 [
                                                    prop.className "relative text-base font-semibold text-secondary mb-1"
                                                    prop.text f.Title
                                                ]
                                                Html.p [
                                                    prop.className "relative text-xs md:text-sm text-base-content/80"
                                                    prop.text f.Description
                                                ]
                                            ]
                                        ])
                                )
                            ]
                        ]
                    ]

                    // thin divider
                    Html.hr [ prop.className "border-base-300/60" ]

                    // INTEGRATION TIERS
                    Html.section [
                        prop.children [
                            Html.div [
                                prop.className "flex items-center justify-between mb-3 gap-4"
                                prop.children [
                                    Html.div [
                                        prop.children [
                                            Html.span [
                                                prop.className "text-[0.7rem] font-semibold tracking-[0.22em] uppercase text-base-content/50"
                                                prop.text "Implementation Levels"
                                            ]
                                            Html.h2 [
                                                prop.className "mt-1 text-2xl md:text-3xl font-semibold text-primary"
                                                prop.text model.TierSectionTitle
                                            ]
                                        ]
                                    ]
                                    Html.span [
                                        prop.className "hidden md:block text-xs md:text-sm text-right text-base-content/60"
                                        prop.text "Choose the level of automation that fits your team today."
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.className "grid grid-cols-1 md:grid-cols-3 gap-6"
                                prop.children (
                                    model.Tiers
                                    |> List.mapi (fun idx tier ->
                                        let isFeatured = idx = 1
                                        Html.div [
                                            prop.className (
                                                "relative rounded-2xl p-6 bg-base-100/95 border shadow-sm transition transform hover:-translate-y-1 hover:shadow-2xl " +
                                                (if isFeatured then "border-secondary/70 ring-2 ring-secondary/25" else "border-base-300")
                                            )
                                            prop.children [
                                                if isFeatured then
                                                    Html.div [
                                                        prop.className "absolute -top-3 -right-3 px-3 py-1 rounded-full bg-secondary text-[0.7rem] font-semibold text-base-100 shadow-sm"
                                                        prop.text "Most Popular"
                                                    ]

                                                Html.h3 [
                                                    prop.className "mt-3 text-lg font-semibold text-secondary mb-2"
                                                    prop.text tier.Name
                                                ]
                                                Html.ul [
                                                    prop.className "list-disc pl-5 text-xs md:text-sm text-base-content/80 space-y-1"
                                                    prop.children (
                                                        tier.Items
                                                        |> List.map (fun i -> Html.li i)
                                                    )
                                                ]
                                            ]
                                        ])
                                )
                            ]
                        ]
                    ]

                    Html.hr [ prop.className "border-base-300/60" ]

                    // WHO THIS IS FOR (Industries)
                    match model.IndustriesSectionTitle, model.Industries with
                    | Some title, _ :: _ ->
                        Html.section [
                            prop.children [
                                Html.span [
                                    prop.className "text-[0.7rem] font-semibold tracking-[0.22em] uppercase text-base-content/50"
                                    prop.text "Industries"
                                ]
                                Html.h2 [
                                    prop.className "mt-1 text-2xl md:text-3xl font-semibold text-primary mb-4"
                                    prop.text title
                                ]
                                Html.div [
                                    prop.className "grid grid-cols-1 md:grid-cols-2 gap-5"
                                    prop.children (
                                        model.Industries
                                        |> List.map industryCard
                                    )
                                ]
                            ]
                        ]
                    | _ -> Html.none

                    // ADDITIONAL CAPABILITIES
                    match model.CapabilitiesSectionTitle, model.Capabilities with
                    | Some title, _ :: _ ->
                        Html.section [
                            prop.children [
                                Html.span [
                                    prop.className "text-[0.7rem] font-semibold tracking-[0.22em] uppercase text-base-content/50"
                                    prop.text "Capabilities"
                                ]
                                Html.h2 [
                                    prop.className "mt-1 text-2xl md:text-3xl font-semibold text-primary mb-4"
                                    prop.text title
                                ]
                                Html.div [
                                    prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4"
                                    prop.children (
                                        model.Capabilities
                                        |> List.map capabilityCard
                                    )
                                ]
                            ]
                        ]
                    | _ -> Html.none

                    Html.hr [ prop.className "border-base-300/60" ]

                    // PRICING
                    Html.section [
                        prop.children [
                            Html.div [
                                prop.className "flex items-center justify-between mb-3 gap-4"
                                prop.children [
                                    Html.div [
                                        prop.children [
                                            Html.span [
                                                prop.className "text-[0.7rem] font-semibold tracking-[0.22em] uppercase text-base-content/50"
                                                prop.text "Pricing"
                                            ]
                                            Html.h2 [
                                                prop.className "mt-1 text-2xl md:text-3xl font-semibold text-primary"
                                                prop.text model.PricingSectionTitle
                                            ]
                                        ]
                                    ]
                                    Html.span [
                                        prop.className "hidden md:block text-xs md:text-sm text-right text-base-content/60"
                                        prop.text "Transparent, implementation-first pricing. No surprise fees."
                                    ]
                                ]
                            ]

                            Html.div [
                                prop.className "grid grid-cols-1 md:grid-cols-3 gap-6"
                                prop.children (
                                    model.PricingPlans
                                    |> List.mapi (fun idx plan ->
                                        let isFeatured = idx = 1
                                        Html.div [
                                            prop.className (
                                                "flex flex-col rounded-2xl bg-base-100/95 border shadow-sm p-6 text-left transition transform hover:-translate-y-1 hover:shadow-2xl " +
                                                (if isFeatured then "border-secondary/70 ring-2 ring-secondary/25" else "border-base-300")
                                            )
                                            prop.children [
                                                Html.h3 [
                                                    prop.className "text-base md:text-lg font-semibold text-secondary"
                                                    prop.text plan.Name
                                                ]
                                                Html.div [
                                                    prop.className "mt-3 flex items-baseline gap-2"
                                                    prop.children [
                                                        Html.span [
                                                            prop.className "text-2xl font-bold text-primary"
                                                            prop.text plan.Setup
                                                        ]
                                                        Html.span [
                                                            prop.className "text-[0.7rem] uppercase tracking-wide text-base-content/60"
                                                            prop.text "Setup"
                                                        ]
                                                    ]
                                                ]
                                                Html.div [
                                                    prop.className "mt-1 text-xs md:text-sm text-base-content/70"
                                                    prop.text ("Monthly: " + plan.Monthly)
                                                ]
                                                Html.div [
                                                    prop.className "mt-4 text-[0.7rem] md:text-xs text-base-content/60"
                                                    prop.text "Includes configuration, QA, and go-live support."
                                                ]
                                            ]
                                        ])
                                )
                            ]
                        ]
                    ]

                    Html.hr [ prop.className "border-base-300/60" ]

                    // STATS / ROI
                    Html.section [
                        prop.children [
                            Html.span [
                                prop.className "text-[0.7rem] font-semibold tracking-[0.22em] uppercase text-base-content/50"
                                prop.text "Impact"
                            ]
                            Html.h2 [
                                prop.className "mt-1 text-2xl md:text-3xl font-semibold text-primary mb-4"
                                prop.text model.StatsSectionTitle
                            ]
                            Html.div [
                                prop.className "grid grid-cols-2 md:grid-cols-3 gap-4"
                                prop.children (
                                    model.Stats
                                    |> List.map (fun s ->
                                        Html.div [
                                            prop.className "rounded-2xl bg-base-100/95 border border-base-300 shadow-sm p-4 text-left"
                                            prop.children [
                                                Html.div [
                                                    prop.className "text-[0.7rem] font-semibold uppercase tracking-wide text-base-content/60"
                                                    prop.text s.Label
                                                ]
                                                Html.div [
                                                    prop.className "mt-1 text-2xl font-bold text-accent"
                                                    prop.text s.Value
                                                ]
                                            ]
                                        ])
                                )
                            ]
                        ]
                    ]

                    // CTA FOOTER
                    Html.section [
                        prop.className "pt-2"
                        prop.children [
                            Html.div [
                                prop.className "rounded-2xl bg-gradient-to-r from-primary to-secondary text-base-100 px-6 md:px-8 py-6 flex flex-col md:flex-row items-center justify-between gap-4 shadow-[0_18px_45px_rgba(15,23,42,0.55)]"
                                prop.children [
                                    Html.div [
                                        prop.className "text-left space-y-1"
                                        prop.children [
                                            Html.h3 [
                                                prop.className "text-lg md:text-2xl font-semibold"
                                                prop.text "Ready to see this in your business?"
                                            ]
                                            Html.p [
                                                prop.className "text-xs md:text-sm text-base-100/85 max-w-xl"
                                                prop.text "Walk through your current process, identify quick wins, and leave with an implementation plan tailored to your operations."
                                            ]
                                        ]
                                    ]
                                    Html.button [
                                        prop.className "btn btn-lg md:btn-md bg-base-100 text-primary border-none shadow-lg hover:bg-base-200"
                                        prop.text model.CtaText
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

// --- AI SALES & OPS ---

let aiFeatures : ServiceFeature list = [
    { Title = "Receptionist Agent"
      Description = "24/7 AI calls & chat, instant lead capture and routing." }
    { Title = "Scheduler & Dispatch Agent"
      Description = "Auto-booking, route planning, and crew balancing across calendars." }
    { Title = "Follow-Up Agent"
      Description = "Automated quotes, reminders, upsells, reviews, and nurture sequences." }
    { Title = "Analytics & Insights"
      Description = "Real-time KPIs, attribution, and ROI reporting in one dashboard." }
]

let aiTiers : ServiceTier list = [
    { Name = "Basic Integration"
      Items = [ "Calendar & CRM sync"; "AI-driven Receptionist"; "Lead capture + tagging" ] }
    { Name = "Advanced Automation"
      Items = [ "Full crew dispatch"; "Invoicing reminders"; "Simple RAG knowledge base" ] }
    { Name = "Fully Managed Turnkey"
      Items = [ "End-to-end funnel automation"; "Training & docs for your team"; "Optional ongoing optimization retainer" ] }
]

let aiPricing : ServicePricingPlan list = [
    { Name = "Basic Integration";      Setup = "$2,000";  Monthly = "$500/mo" }
    { Name = "Advanced Automation";    Setup = "$3,500";  Monthly = "$1,000/mo" }
    { Name = "Fully Managed Turnkey";  Setup = "$15,000"; Monthly = "TBD (support retainer)" }
]

let aiStats : ServiceStat list = [
    { Label = "Missed Calls";          Value = "30%"; Trend = Down }
    { Label = "Lead Conversion";       Value = "20%"; Trend = Up }
    { Label = "Admin Hours";           Value = "25%"; Trend = Down }
    { Label = "Customer Satisfaction"; Value = "60%"; Trend = Up }
    { Label = "Ops Errors";            Value = "40%"; Trend = Down }
    { Label = "Owner Focus Time";      Value = "50%"; Trend = Up }
]

// Industries this service is tailored for
let aiIndustries : ServiceIndustry list = [
    { Name = "Home Services & Contractors"
      Summary = "Perfect for HVAC, plumbing, electrical, and other trades with inbound calls and dispatch."
      Outcomes = [
        "Auto-answers and qualifies every inbound call."
        "Reminders and follow-ups when bids go quiet."
        "Route-balanced schedules for techs and crews."
      ] }
    { Name = "Healthcare & Clinics"
      Summary = "Reduce front-desk load while keeping patient communication personal."
      Outcomes = [
        "Intake, reminders, and reschedules handled by agents."
        "Triage questions routed to the right staff."
        "Post-visit surveys and review requests automated."
      ] }
    { Name = "Legal & Professional Services"
      Summary = "Never miss a lead while you’re in court or with clients."
      Outcomes = [
        "Call screening and intake forms without extra staff."
        "Smart follow-ups to reduce no-shows and ghosting."
        "Automated reminders around key dates and documents."
      ] }
    { Name = "Retail, Salons & Memberships"
      Summary = "Drive repeat visits, memberships, and reviews without manual outreach."
      Outcomes = [
        "Abandoned inquiry follow-ups and reactivation campaigns."
        "Review and referral requests after each visit."
        "Membership and package renewal nudges."
      ] }
]

// AI / LLM-specific capabilities
let aiCapabilities : ServiceCapability list = [
    { Heading = "LLM-Powered Phone & Chat Agents"
      Icon = "🤖"
      Description = "Voice and chat agents tailored to your scripts, playbooks, and compliance needs." }
    { Heading = "RAG Knowledge Bases"
      Icon = "📚"
      Description = "Ground models in your SOPs, PDFs, contracts, and EMR/CRM data instead of generic internet text." }
    { Heading = "Generative Outreach & Follow-Up"
      Icon = "✉️"
      Description = "Context-aware emails, SMS, and messages that match your brand and tone—no templates required." }
    { Heading = "LLM Workflow Orchestration"
      Icon = "🧠"
      Description = "Chain multiple agents and tools together to complete multi-step workflows end-to-end." }
    { Heading = "Model & Prompt Tuning"
      Icon = "🎯"
      Description = "Prompt, system message, and parameter tuning so responses stay on-brand, safe, and reliable." }
    { Heading = "Analytics & Guardrails"
      Icon = "📊"
      Description = "Monitor performance, catches, and errors with human-in-the-loop controls where needed." }
]

let aiSalesModel : ServicePageModel =
    { Id = "ai-sales-automation"
      Name = "AI Sales & Operations Automation"
      HeroTitle = "Scale Your Business with AI-Driven Automation"
      HeroSubtitle = "Never miss a lead, eliminate scheduling headaches, and automate follow-ups — all in one unified platform."
      HeroBadge = Some "AI Automation"
      HeroGradientClass = "from-primary to-secondary"

      CoreSectionTitle = "Core Modules"
      CoreFeatures = aiFeatures

      TierSectionTitle = "Integration & Automation Tiers"
      Tiers = aiTiers

      PricingSectionTitle = "Pricing Plans"
      PricingPlans = aiPricing

      StatsSectionTitle = "Typical First-Month Impact"
      Stats = aiStats

      IndustriesSectionTitle = Some "Who This Service Works Best For"
      Industries = aiIndustries

      CapabilitiesSectionTitle = Some "AI & LLM Capabilities"
      Capabilities = aiCapabilities

      CtaText = "Book Your 30-Minute Demo" }


[<ReactComponent>]
let AiSalesPage () =
    ServicePage aiSalesModel

let engineeringCapabilities : ServiceCapability list = [
    { Heading = "Web Development";         Icon = "🌐"; Description = "Responsive, performant SPAs and full-stack web apps." }
    { Heading = "UI/UX Design";            Icon = "🎨"; Description = "Wireframes, design systems, and polished front-ends." }
    { Heading = "Software Integration";    Icon = "🔗"; Description = "Connecting CRMs, ERPs, EMRs, and third-party tools." }
    { Heading = "E-Commerce";              Icon = "🛒"; Description = "Online stores, product catalogs, and custom checkout flows." }
    { Heading = "API Development";         Icon = "🔃"; Description = "REST/GraphQL APIs with clear contracts and docs." }
    { Heading = "Performance Optimization";Icon = "⚡"; Description = "Profiling, query tuning, and front-end performance passes." }
    { Heading = "Security Enhancements";   Icon = "🔒"; Description = "Hardening auth, RBAC, and data access paths." }
    { Heading = "Cloud Deployment";        Icon = "☁️"; Description = "CI/CD, containers, and cloud-native deployments." }
    { Heading = "Maintenance & Support";   Icon = "🛠️"; Description = "Retainers for fixes, small features, and upgrades." }
]

let engineeringFeatures : ServiceFeature list = [
    { Title = "Modern Web Apps"
      Description = "F#, TypeScript, and React-based applications that stay maintainable over time." }
    { Title = "Design-to-Code"
      Description = "Partner with designers or take your rough sketches to polished interfaces." }
    { Title = "Platform Glue"
      Description = "Bridge legacy systems and new tooling without rewriting everything at once." }
    { Title = "Data & Reporting"
      Description = "Dashboards and exports that give teams the numbers they actually care about." }
]

let engineeringTiers : ServiceTier list = [
    { Name = "Project Foundations"
      Items = [ "Architecture & technical plan"; "Design system + component library"; "Initial implementation" ] }
    { Name = "Product Delivery"
      Items = [ "End-to-end feature delivery"; "Backend + frontend integration"; "QA, testing, and rollout" ] }
    { Name = "Platform Partnership"
      Items = [ "Ongoing roadmap support"; "Performance and reliability passes"; "On-call for product and engineering" ] }
]

let engineeringPricing : ServicePricingPlan list = [
    { Name = "Project Foundations";  Setup = "$4,000+"; Monthly = "N/A (one-time)" }
    { Name = "Product Delivery";     Setup = "$8,000+"; Monthly = "Scoped per engagement" }
    { Name = "Platform Partnership"; Setup = "$3,000+"; Monthly = "Retainer-based" }
]

let engineeringStats : ServiceStat list = [
    { Label = "Delivery Speed"; Value = "2–3×"; Trend = Up }
    { Label = "Production Incidents"; Value = "50%"; Trend = Down }
    { Label = "Developer Happiness"; Value = "40%"; Trend = Up }
]

let engineeringIndustries : ServiceIndustry list = [
    { Name = "SaaS & B2B Platforms"
      Summary = "For teams that need a senior engineer to own complex features or integrations."
      Outcomes = [
        "Ship complex features without ballooning team size."
        "Have a sounding board for architecture decisions."
        "Unblock frontend–backend integration quickly."
      ] }
    { Name = "Healthcare & Regulated"
      Summary = "Great fit when you need careful handling of PHI or domain-heavy logic."
      Outcomes = [
        "Map messy domain concepts into clear types and workflows."
        "Integrate with registries, clearinghouses, or vendor APIs."
        "Improve reliability and observability around critical flows."
      ] }
]

let engineeringModel : ServicePageModel =
    { Id = "engineering-services"
      Name = "Engineering & Integration Services"
      HeroTitle = "Ship the Right Features, the Right Way"
      HeroSubtitle = "From greenfield builds to legacy integrations, get a senior engineer who can own the messy middle and deliver."
      HeroBadge = Some "Engineering & Integration"
      HeroGradientClass = "from-secondary to-accent"

      CoreSectionTitle = "Ways We Can Help"
      CoreFeatures = engineeringFeatures

      TierSectionTitle = "Engagement Models"
      Tiers = engineeringTiers

      PricingSectionTitle = "Typical Investment"
      PricingPlans = engineeringPricing

      StatsSectionTitle = "Impact on Your Team"
      Stats = engineeringStats

      IndustriesSectionTitle = Some "Who This Service Is a Great Fit For"
      Industries = engineeringIndustries

      CapabilitiesSectionTitle = Some "Capabilities"
      Capabilities = engineeringCapabilities

      CtaText = "Talk About Your Roadmap" }

[<ReactComponent>]
let EngineeringPage () =
    ServicePage engineeringModel

    // ----------------------
// AUTOMATION
// ----------------------

let automationFeatures : ServiceFeature list = [
    { Title = "Workflow Automation"
      Description = "Automate cross-app workflows so teams stop living in spreadsheets and email." }
    { Title = "Internal Tools & Dashboards"
      Description = "Lightweight internal apps that wrap your existing systems instead of replacing them." }
    { Title = "Notification & Alerting"
      Description = "Smart alerts for SLAs, approvals, and exceptions that actually matter." }
    { Title = "Backoffice Bots"
      Description = "Bots that handle repetitive admin tasks while your team handles edge cases." }
]

let automationTiers : ServiceTier list = [
    { Name = "Discovery & Design"
      Items = [
        "Current process mapping"
        "Automation opportunity report"
        "Prioritized roadmap"
      ] }
    { Name = "Automation Build-Out"
      Items = [
        "Workflow design & implementation"
        "Integrations & data sync"
        "Monitoring & logging"
      ] }
    { Name = "Automation Care Plan"
      Items = [
        "Ongoing tuning & monitoring"
        "New workflow rollouts"
        "Quarterly optimization review"
      ] }
]

let automationPricing : ServicePricingPlan list = [
    { Name = "Discovery & Design";   Setup = "$2,000";  Monthly = "N/A (one-time)" }
    { Name = "Automation Build-Out"; Setup = "$5,000+"; Monthly = "Scoped per workflow set" }
    { Name = "Automation Care Plan"; Setup = "$1,000";  Monthly = "$1,000+/mo" }
]

let automationStats : ServiceStat list = [
    { Label = "Manual Clicks Removed"; Value = "40–70%"; Trend = Down }
    { Label = "Process Throughput";    Value = "2–4×";   Trend = Up }
    { Label = "Error Rate";            Value = "30–60%"; Trend = Down }
]

// Industries where this shines
let automationIndustries : ServiceIndustry list = [
    { Name = "Home Services & Field Ops"
      Summary = "For teams where scheduling, dispatch, and follow-up are still handled manually."
      Outcomes = [
        "Reduce time spent on scheduling and dispatch."
        "Standardize follow-up sequences so no work order gets dropped."
        "Give owners a clear view of workload and bottlenecks."
      ] }
    { Name = "Healthcare & Clinics"
      Summary = "Automate the repetitive coordination around appointments, reminders, and paperwork."
      Outcomes = [
        "Reduce front-desk load without losing human touch."
        "Automate reminders, reschedules, and simple pre-visit intake."
        "Flag exceptions and edge cases for staff instead of handling every case manually."
      ] }
    { Name = "Backoffice & Accounting"
      Summary = "Great fit for finance and ops teams drowning in checklists and spreadsheets."
      Outcomes = [
        "Automate status changes and document routing."
        "Ensure approvals and reviews follow a consistent path."
        "Improve auditability with clear logs and traces."
      ] }
]

// Capabilities (for the capabilities section)
let automationCapabilities : ServiceCapability list = [
    { Heading = "Process Mapping & Design"
      Icon = "🧭"
      Description = "Map your current workflows, then design automated versions that keep the right approvals in place." }
    { Heading = "Task & Ticket Automation"
      Icon = "📌"
      Description = "Create, update, and route tasks in tools like Jira, Trello, Asana, or custom systems." }
    { Heading = "Approval Flows"
      Icon = "✅"
      Description = "Automate approvals while keeping human review for high-risk or high-value decisions." }
    { Heading = "Backoffice Bots"
      Icon = "🤖"
      Description = "Bots that handle repetitive admin work like status updates, reminders, and document checks." }
    { Heading = "Monitoring & Guardrails"
      Icon = "🛡️"
      Description = "Dashboards and alerts so you can see what automations are doing and step in when needed." }
    { Heading = "Human-in-the-Loop Design"
      Icon = "🧑‍💻"
      Description = "Keep people in control of exceptions, escalations, and edge cases instead of trying to automate everything." }
]

let automationModel : ServicePageModel =
    { Id = "automation-services"
      Name = "Business & Ops Automation"
      HeroTitle = "Automate the Busywork, Keep the Control"
      HeroSubtitle = "Codify your best processes into automations so your team can focus on the exceptions, not the routine."
      HeroBadge = Some "Automation"
      HeroGradientClass = "from-primary to-secondary"
      CoreSectionTitle = "Automation Building Blocks"
      CoreFeatures = automationFeatures
      TierSectionTitle = "How We Engage"
      Tiers = automationTiers
      PricingSectionTitle = "Implementation Packages"
      PricingPlans = automationPricing
      StatsSectionTitle = "Typical Operations Impact"
      Stats = automationStats
      IndustriesSectionTitle = Some "Who This Service Works Best For"
      Industries = automationIndustries
      CapabilitiesSectionTitle = Some "Automation Capabilities"
      Capabilities = automationCapabilities
      CtaText = "Schedule an Automation Review" }

[<ReactComponent>]
let AutomationPage () =
    ServicePage automationModel


// ----------------------
// INTEGRATION
// ----------------------

let integrationFeatures : ServiceFeature list = [
    { Title = "API & Webhook Integrations"
      Description = "Connect CRMs, ERPs, EMRs, and SaaS tools with robust, monitored integrations." }
    { Title = "Data Sync & ETL"
      Description = "Move data reliably between systems with proper mapping, validation, and logging." }
    { Title = "Event-Driven Workflows"
      Description = "Trigger downstream actions when the right events happen in your systems." }
    { Title = "Vendor & Partner Platforms"
      Description = "Implement vendor APIs and specs so your team doesn’t have to reverse-engineer them." }
]

let integrationTiers : ServiceTier list = [
    { Name = "Single Integration"
      Items = [
        "One source ↔ destination"
        "Field mapping & validation"
        "Monitoring & alerts"
      ] }
    { Name = "Integration Bundle"
      Items = [
        "Multiple systems connected"
        "Shared logging & observability"
        "Error handling playbooks"
      ] }
    { Name = "Integration Platform"
      Items = [
        "Reusable integration layer"
        "Docs & onboarding for your devs"
        "Optional ongoing support"
      ] }
]

let integrationPricing : ServicePricingPlan list = [
    { Name = "Single Integration";   Setup = "$3,000+";  Monthly = "Optional support retainer" }
    { Name = "Integration Bundle";   Setup = "$7,500+";  Monthly = "Scoped per bundle" }
    { Name = "Integration Platform"; Setup = "$12,000+"; Monthly = "Custom" }
]

let integrationStats : ServiceStat list = [
    { Label = "Manual Data Entry";         Value = "70%";  Trend = Down }
    { Label = "Sync Reliability";          Value = "99%+"; Trend = Up }
    { Label = "Time to Onboard New Tools"; Value = "50%";  Trend = Down }
]

let integrationIndustries : ServiceIndustry list = [
    { Name = "SaaS & B2B Platforms"
      Summary = "When customers demand integrations with the tools they already use."
      Outcomes = [
        "Ship integrations without burning out your core product team."
        "Standardize how you connect to CRMs, billing, and data warehouses."
        "Improve retention by making your product fit into existing stacks."
      ] }
    { Name = "Healthcare & Regulated Data"
      Summary = "For EMRs, registries, and clearinghouse-style integrations that require extra care."
      Outcomes = [
        "Respect compliance requirements while still moving quickly."
        "Create clear mapping between systems so data is trustworthy."
        "Own logging and error handling instead of relying on vendor UI." 
      ] }
    { Name = "Operations & Logistics"
      Summary = "Perfect when you rely on multiple vendors, carriers, or systems to move work along."
      Outcomes = [
        "Get real-time updates between systems without CSV imports."
        "Reduce mismatches between order, inventory, and delivery data."
        "Give operations a single, consistent view of the state of work."
      ] }
]

let integrationCapabilities : ServiceCapability list = [
    { Heading = "API Design & Integration"
      Icon = "🔗"
      Description = "Build and consume REST or GraphQL APIs with clear contracts, auth, and versioning." }
    { Heading = "Webhooks & Event Buses"
      Icon = "📡"
      Description = "Trigger downstream workflows reliably when important events occur in your systems." }
    { Heading = "ETL & Data Pipelines"
      Icon = "📊"
      Description = "Extract, transform, and load data into CRMs, warehouses, or analytics tools with validation." }
    { Heading = "Schema Mapping"
      Icon = "🧬"
      Description = "Map messy, real-world data into sane shapes with explicit rules and safeguards." }
    { Heading = "Monitoring & Observability"
      Icon = "👀"
      Description = "Logs, dashboards, and alerts so you know when integrations are misbehaving before customers do." }
    { Heading = "Vendor Collaboration"
      Icon = "🤝"
      Description = "Work directly with vendor teams and docs so your internal team doesn’t have to." }
]

let integrationModel : ServicePageModel =
    { Id = "integration-services"
      Name = "API & Systems Integration"
      HeroTitle = "Make Your Tools Actually Talk to Each Other"
      HeroSubtitle = "Connect the platforms you already use so data flows cleanly without brittle scripts and manual exports."
      HeroBadge = Some "Integration"
      HeroGradientClass = "from-accent to-primary"
      CoreSectionTitle = "Integration Capabilities"
      CoreFeatures = integrationFeatures
      TierSectionTitle = "Integration Packages"
      Tiers = integrationTiers
      PricingSectionTitle = "Typical Investment"
      PricingPlans = integrationPricing
      StatsSectionTitle = "Integration Outcomes"
      Stats = integrationStats
      IndustriesSectionTitle = Some "Who This Is a Great Fit For"
      Industries = integrationIndustries
      CapabilitiesSectionTitle = Some "Integration Capabilities"
      Capabilities = integrationCapabilities
      CtaText = "Map Your Integrations" }

[<ReactComponent>]
let IntegrationPage () =
    ServicePage integrationModel

// ----------------------
// WEBSITE / FRONTEND
// ----------------------

let websiteFeatures : ServiceFeature list = [
    { Title = "Marketing & Brochure Sites"
      Description = "Fast, modern sites that clearly explain what you do and why it matters." }
    { Title = "Product & App Shells"
      Description = "Frontends that make complex products feel simple and approachable." }
    { Title = "Design Systems"
      Description = "Reusable components and styles across marketing and app surfaces." }
    { Title = "Performance & Accessibility"
      Description = "Core Web Vitals, accessibility, and SEO-conscious structure baked in." }
]

let websiteTiers : ServiceTier list = [
    { Name = "Launch Site"
      Items = [
        "1–3 core pages"
        "Simple CMS or static"
        "Contact / lead capture"
      ] }
    { Name = "Growth Site"
      Items = [
        "Multi-page marketing site"
        "Blog or resources"
        "Analytics & experimentation ready"
      ] }
    { Name = "Design System"
      Items = [
        "Component library"
        "Docs and usage guidelines"
        "Design-to-code workflow"
      ] }
]

let websitePricing : ServicePricingPlan list = [
    { Name = "Launch Site";   Setup = "$2,500+"; Monthly = "Hosting + light care plan optional" }
    { Name = "Growth Site";   Setup = "$5,000+"; Monthly = "Optional optimization retainer" }
    { Name = "Design System"; Setup = "$6,500+"; Monthly = "Project-based" }
]

let websiteStats : ServiceStat list = [
    { Label = "Page Load Time";         Value = "50–70%"; Trend = Down }
    { Label = "Demo / Lead Conversion"; Value = "15–30%"; Trend = Up }
    { Label = "Design Debt";            Value = "40%";    Trend = Down }
]

let websiteIndustries : ServiceIndustry list = [
    { Name = "Consultants & Solo Founders"
      Summary = "When your current site undersells what you can actually do."
      Outcomes = [
        "Tell a clear story about what you offer."
        "Make it easy for the right clients to book a call."
        "Have a site you’re proud to send people to."
      ] }
    { Name = "SaaS & B2B Products"
      Summary = "Marketing sites that match the polish and sophistication of your product."
      Outcomes = [
        "Clarify positioning and who the product is for."
        "Support demo, trial, and onboarding flows."
        "Create consistency between marketing and in-app UI."
      ] }
    { Name = "Service Businesses"
      Summary = "Local or niche services that win when the site feels trustworthy and modern."
      Outcomes = [
        "Make it painless to contact or book."
        "Highlight reviews, outcomes, and social proof."
        "Stand out from generic template sites in your space."
      ] }
]

let websiteCapabilities : ServiceCapability list = [
    { Heading = "Responsive Layouts"
      Icon = "📱"
      Description = "Layouts that look and feel right across desktop, tablet, and mobile." }
    { Heading = "Component Libraries"
      Icon = "🧩"
      Description = "Reusable components wired into your design tokens and theme." }
    { Heading = "Content-First Structure"
      Icon = "✍️"
      Description = "Sections and flows built around the story you want to tell, not just visuals." }
    { Heading = "Performance & SEO"
      Icon = "⚡"
      Description = "Good Core Web Vitals and markup that search engines can actually understand." }
    { Heading = "Analytics & Experimentation"
      Icon = "📊"
      Description = "Hook in analytics, event tracking, and simple A/B tests from day one." }
    { Heading = "Design Handoff"
      Icon = "🎨"
      Description = "Work directly from Figma or sketches with tight design–dev feedback loops." }
]

let websiteModel : ServicePageModel =
    { Id = "website-services"
      Name = "Websites & Frontend Experience"
      HeroTitle = "Give Your Product the Frontend It Deserves"
      HeroSubtitle = "From quick marketing sites to full design systems, make your product look as good as it works."
      HeroBadge = Some "Web Development"
      HeroGradientClass = "from-primary to-pink-500"
      CoreSectionTitle = "What We Build"
      CoreFeatures = websiteFeatures
      TierSectionTitle = "Site Packages"
      Tiers = websiteTiers
      PricingSectionTitle = "Typical Investment"
      PricingPlans = websitePricing
      StatsSectionTitle = "Frontend Impact"
      Stats = websiteStats
      IndustriesSectionTitle = Some "Who This Is For"
      Industries = websiteIndustries
      CapabilitiesSectionTitle = Some "Web & Frontend Capabilities"
      Capabilities = websiteCapabilities
      CtaText = "Plan Your Next Release" }

[<ReactComponent>]
let WebsitePage () =
    ServicePage websiteModel


// ----------------------
// SALES PLATFORM / REVOPS
// ----------------------

let salesFeatures : ServiceFeature list = [
    { Title = "CRM & Pipeline Setup"
      Description = "Clean pipelines, fields, and views so reps know exactly what to do next." }
    { Title = "Playbooks & Sequences"
      Description = "Codify your best outreach into reusable, trackable sequences." }
    { Title = "Reporting & RevOps"
      Description = "Dashboards that highlight what actually moves revenue, not vanity metrics." }
    { Title = "AI Sales Assist"
      Description = "AI helpers for research, call notes, and follow-up drafting." }
]

let salesTiers : ServiceTier list = [
    { Name = "CRM Cleanup"
      Items = [
        "Pipeline & stage design"
        "Field audit & simplification"
        "Basic dashboards"
      ] }
    { Name = "Playbooks + Automation"
      Items = [
        "Inbound/outbound playbooks"
        "Sequences & triggers"
        "Handoff workflows"
      ] }
    { Name = "RevOps Partnership"
      Items = [
        "Monthly reporting & analysis"
        "Experiment design"
        "Ongoing tuning & support"
      ] }
]

let salesPricing : ServicePricingPlan list = [
    { Name = "CRM Cleanup";            Setup = "$2,500";  Monthly = "N/A (one-time)" }
    { Name = "Playbooks + Automation"; Setup = "$4,000+"; Monthly = "$1,000+/mo" }
    { Name = "RevOps Partnership";     Setup = "$3,000";  Monthly = "$2,000+/mo" }
]

let salesStats : ServiceStat list = [
    { Label = "Time to First Response";   Value = "60%";   Trend = Down }
    { Label = "Qualified Opportunities";  Value = "30–50%";Trend = Up }
    { Label = "Owner / Leader Visibility";Value = "80%";   Trend = Up }
]

let salesIndustries : ServiceIndustry list = [
    { Name = "B2B Sales Teams"
      Summary = "When deals slip through the cracks because the stack and process aren’t aligned."
      Outcomes = [
        "Cleaner pipelines and next steps for every deal."
        "Consistent follow-up cadences that match your brand."
        "Reporting that lets leaders coach and forecast, not guess."
      ] }
    { Name = "Agencies & Service Businesses"
      Summary = "Align sales and delivery so you stop overpromising or under-scoping."
      Outcomes = [
        "Lead intake that captures the details delivery actually needs."
        "Visibility from first touch through active projects and renewals."
        "Automated nudges around renewals, upsells, and referrals."
      ] }
    { Name = "Founder-Led Sales"
      Summary = "Perfect when the founder or small team is still doing most of the selling."
      Outcomes = [
        "Document your best sales motion into playbooks."
        "Get out of inbox-only chaos into a clear system."
        "Know which channels and messages actually close deals."
      ] }
]

let salesCapabilities : ServiceCapability list = [
    { Heading = "CRM Architecture"
      Icon = "📂"
      Description = "Design pipelines, fields, and objects that match how you actually sell." }
    { Heading = "Playbooks & Sequences"
      Icon = "📨"
      Description = "Email, SMS, and task sequences wired to your stages and personas." }
    { Heading = "AI Sales Assist"
      Icon = "🤖"
      Description = "Call summaries, follow-up drafts, and research helpers tuned to your voice." }
    { Heading = "RevOps Dashboards"
      Icon = "📈"
      Description = "Dashboards and reports for owners, leaders, and reps that cut through noise." }
    { Heading = "Handoff & Delivery Flows"
      Icon = "🔁"
      Description = "Structured process when a deal closes so delivery starts off on the right foot." }
    { Heading = "Experimentation & Tuning"
      Icon = "🧪"
      Description = "Iterate on messaging, channels, and sequences with real data, not guesses." }
]

let salesModel : ServicePageModel =
    { Id = "sales-platform-services"
      Name = "Sales & Customer Platforms"
      HeroTitle = "Turn Your Sales Stack into a Revenue Engine"
      HeroSubtitle = "Align CRM, automations, and analytics so every lead gets the right next step—without manual wrangling."
      HeroBadge = Some "Sales Platform"
      HeroGradientClass = "from-fuchsia-500 to-primary"
      CoreSectionTitle = "Sales Platform Building Blocks"
      CoreFeatures = salesFeatures
      TierSectionTitle = "Engagement Options"
      Tiers = salesTiers
      PricingSectionTitle = "Typical Investment"
      PricingPlans = salesPricing
      StatsSectionTitle = "Sales & RevOps Outcomes"
      Stats = salesStats
      IndustriesSectionTitle = Some "Who This Is Ideal For"
      Industries = salesIndustries
      CapabilitiesSectionTitle = Some "Sales & RevOps Capabilities"
      Capabilities = salesCapabilities
      CtaText = "Tune Your Sales Platform" }

[<ReactComponent>]
let SalesPlatformPage () =
    ServicePage salesModel

