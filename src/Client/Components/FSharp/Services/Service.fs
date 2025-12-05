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


[<ReactComponent>]
let ServicePage (model: ServicePageModel) =
    Html.div [
        prop.className "bg-base-200 min-h-screen text-base-content"
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


// AI Sales & Operations Automation Page Model

let aiFeatures : ServiceFeature list = [
    { Title = "Receptionist Agent"
      Description = "24/7 AI calls & chat, instant lead capture" }
    { Title = "Scheduler & Dispatch Agent"
      Description = "Auto-booking, route planning, crew balancing" }
    { Title = "Follow-Up Agent"
      Description = "Automated quotes, reminders, upsells & review requests" }
    { Title = "Analytics Dashboard"
      Description = "Real-time KPIs, custom reports & ROI tracking" }
]

let aiTiers : ServiceTier list = [
    { Name = "Basic Integration"
      Items = [ "Calendar & CRM sync"; "AI-driven Receptionist" ] }
    { Name = "Advanced Automation"
      Items = [ "Full crew dispatch"; "Invoicing reminders"; "Simple RAG KB" ] }
    { Name = "Fully Managed Turnkey"
      Items = [ "Platform handover"; "Training & docs"; "Optional retainer" ] }
]

let aiPricing : ServicePricingPlan list = [
    { Name = "Basic Integration";      Setup = "$2,000";  Monthly = "$500/mo" }
    { Name = "Advanced Automation";    Setup = "$3,500";  Monthly = "$1,000/mo" }
    { Name = "Fully Managed Turnkey";  Setup = "$15,000"; Monthly = "TBD (support retainer)" }
]

let aiStats : ServiceStat list = [
    { Label = "Missed Calls"
      Value = "30%"
      Trend = Down }

    { Label = "Lead Conversion"
      Value = "20%"
      Trend = Up }

    { Label = "Admin Hours"
      Value = "25%"
      Trend = Down }

    { Label = "Customer Satisfaction"
      Value = "60%"
      Trend = Up }

    { Label = "Ops Errors"
      Value = "40%"
      Trend = Down }

    { Label = "Owner Focus Time"
      Value = "50%"
      Trend = Up }
]


let aiSalesModel : ServicePageModel =
    { Id = "ai-sales-automation"
      Name = "AI Sales & Operations Automation"
      HeroTitle = "Scale Your Business with AI-Driven Automation"
      HeroSubtitle = "Never miss a lead, eliminate scheduling headaches, and automate follow-ups — all in one unified platform."
      HeroBadge = Some "AI Automation"
      HeroGradientClass = "bg-gradient-to-br from-primary to-secondary"
      CoreSectionTitle = "Core Modules"
      CoreFeatures = aiFeatures
      TierSectionTitle = "Integration & Automation Tiers"
      Tiers = aiTiers
      PricingSectionTitle = "Pricing Plans"
      PricingPlans = aiPricing
      StatsSectionTitle = "Typical First-Month Impact"
      Stats = aiStats
      CtaText = "Book Your 30-Minute Demo" }

[<ReactComponent>]
let AiSalesPage () =
    ServicePage aiSalesModel