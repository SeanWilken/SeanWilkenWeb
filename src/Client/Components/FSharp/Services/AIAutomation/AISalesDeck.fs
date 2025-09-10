module Components.FSharp.Services.AIAutomation.AISalesDeck

open Feliz
open Elmish
open Shared.SharedServices

let services = [
    "Receptionist Agent", "24/7 AI calls & chat, instant lead capture"
    "Scheduler & Dispatch Agent", "Auto-booking, route planning, crew balancing"
    "Follow-Up Agent", "Automated quotes, reminders, upsells & review requests"
    "Analytics Dashboard", "Real-time KPIs, custom reports & ROI tracking"
]

let integrationLevels = [
    "Basic Integration", ["Calendar & CRM sync"; "AI-driven Receptionist"]
    "Advanced Automation", ["Full crew dispatch"; "Invoicing reminders"; "Simple RAG KB"]
    "Fully Managed Turnkey", ["Platform handover"; "Training & docs"; "Optional retainer"]
]

let pricingPlans = [
    "Basic Integration", "$2,000", "$500/mo"
    "Advanced Automation", "$3,500", "$1,000/mo"
    "Fully Managed Turnkey", "$15,000", "TBD (support retainer)"
]

let statCards = [
    "Missed Calls ↓", "30%"
    "Lead Conversion ↑", "20%"
    "Admin Hours ↓", "25%"
    "Customer Service ↑", "60%"
    "Operation Errors ↓", "40%"
    "Business Focus ↑", "50%"
    "Employee Delegation ↑", "30%"
]

[<ReactComponent>]
let AiSalesPage = React.functionComponent(fun (dispatch: Msg -> unit) ->
    Html.div [
        prop.className "bg-base-100 min-h-screen p-8 space-y-16"

        // Hero
        prop.children [
            Html.section [
                prop.className "hero bg-gradient-to-br from-primary to-secondary text-base-100 shadow-lg rounded-lg p-12 text-center"
                prop.children [
                    Html.h1 [ prop.className "text-6xl font-bold text-secondary"; prop.text "Scale Your Business with AI-Driven Automation" ]
                    Html.p [ prop.className "mt-4 text-lg text-white-700"
                             prop.text "Never miss a lead, eliminate scheduling headaches, and automate follow-ups — all in one unified platform." ]
                ]
            ]

            // Features
            Html.section [
                prop.children [
                    Html.h2 [ prop.className "text-3xl font-semibold text-secondary text-center mb-6"; prop.text "Core Modules" ]
                    Html.div [
                        prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6"
                        prop.children (
                            services
                            |> List.map (fun (title, desc) ->
                                Html.div [
                                    prop.className "card bg-hero-gradient shadow-md p-6"
                                    prop.children [
                                        Html.h3 [ prop.className "text-xl text-secondary font-medium mb-2"; prop.text title ]
                                        Html.p [ prop.className "text-primary"; prop.text desc ]
                                    ]
                                ])
                        )
                    ]
                ]
            ]

            // Integration Levels
            Html.section [
                prop.children [
                    Html.h2 [ prop.className "text-3xl font-semibold text-secondary text-center mb-6"; prop.text "Integration & Automation Tiers" ]
                    Html.div [
                        prop.className "grid grid-cols-1 md:grid-cols-3 gap-8"
                        prop.children (
                            integrationLevels
                            |> List.map (fun (tier, items) ->
                                Html.div [
                                    prop.className "card bg-hero-gradient shadow-md p-6"
                                    prop.children [
                                        Html.h3 [ prop.className "text-xl text-secondary font-medium mb-4"; prop.text tier ]
                                        Html.ul [
                                            prop.className "list-disc pl-5 text-primary space-y-2"
                                            prop.children (items |> List.map (fun i -> Html.li i))
                                        ]
                                    ]
                                ])
                        )
                    ]
                ]
            ]

            // Pricing
            Html.section [
                prop.children [
                    Html.h2 [ prop.className "text-3xl font-semibold text-secondary text-center mb-6"; prop.text "Pricing Plans" ]
                    Html.div [
                        prop.className "overflow-x-auto"
                        prop.children [
                            Html.table [
                                prop.className "table w-full bg-hero-gradient shadow-md text-primary"
                                prop.children [
                                    Html.thead [
                                        prop.className "text-secondary"
                                        prop.children [
                                            Html.tr [
                                                Html.th "Plan"
                                                Html.th "Setup Fee"
                                                Html.th "Monthly Fee"
                                            ]
                                        ]
                                    ]
                                    Html.tbody [
                                        for (plan, setup, monthly) in pricingPlans do
                                            Html.tr [
                                                Html.td plan
                                                Html.td setup
                                                Html.td monthly
                                            ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // ROI Stats
            Html.section [
                prop.children [
                    Html.h2 [ prop.className "text-3xl font-semibold text-secondary text-center mb-6"; prop.text "Typical First-Month Impact" ]
                    Html.div [
                        prop.className "stats bg-hero-gradient stats-vertical lg:stats-horizontal shadow"
                        prop.children (
                            statCards
                            |> List.map (fun (label, value) ->
                                Html.div [
                                    prop.className "stat"
                                    prop.children  [
                                        Html.div [ prop.className "stat-title text-secondary"; prop.text label ]
                                        Html.div [ prop.className "stat-value text-primary"; prop.text value ]
                                    ]
                                ])
                        )
                    ]
                ]
            ]

            // CTA
            Html.section [
                prop.className "text-center"
                prop.children [
                    Html.button [
                        prop.className "btn btn-primary btn-lg"
                        prop.text "Book Your 30-Minute Demo"
                    ]
                ]
            ]
        ]
    ]
)
