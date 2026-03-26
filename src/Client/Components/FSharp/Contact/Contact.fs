module Components.FSharp.Contact

open Feliz
open Bindings.LucideIcon
open Client.Components.Shop.Common.Ui.Animations

[<ReactComponent>]
let View () =
    Html.main [
        prop.className "w-full bg-base-100 text-base-content"
        prop.children [

            // HERO SECTION
            Html.section [
                prop.className "pt-28 pb-20 px-6 md:px-8 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto"
                        prop.children [

                            Client.Components.Shop.Common.Ui.Section.headerTagArea
                                (LucideIcon.Mail "w-4 h-4 opacity-60")
                                "Contact"

                            // Hero card
                            Html.div [
                                prop.className "hero-card px-6 py-10 lg:px-12 lg:py-14"
                                prop.children [

                                    Html.div [
                                        prop.className "grid gap-10 lg:grid-cols-2 items-start"
                                        prop.children [

                                            // LEFT: big cormorant-font heading + radios
                                            ScrollReveal {|
                                                Variant   = SlideRight
                                                Delay     = 0.08
                                                Threshold = 0.45
                                                Children = 
                                                    
                                                    Html.div [
                                                        prop.className "space-y-8"
                                                        prop.children [
                                                            Html.h1 [
                                                                prop.className "cormorant-font text-4xl md:text-5xl lg:text-6xl font-light leading-tight"
                                                                prop.text "Get in touch"
                                                            ]

                                                            Html.p [
                                                                prop.className "text-md md:text-base opacity-70 leading-relaxed max-w-md"
                                                                prop.text
                                                                    "For professional opportunities, project discussions, creative inquiries, or general questions, feel free to reach out. Sean Wilken is the primary contact for engineering and professional outreach, while Xero Effort is available for brand or creative inquiries."
                                                            ]

                                                            Html.div [
                                                                prop.className "space-y-4"
                                                                prop.children [
                                                                    Html.p [
                                                                        prop.className "cormorant-font text-sm opacity-60"
                                                                        prop.text "Choose the best contact path"
                                                                    ]

                                                                    // Sean option (selected)
                                                                    Html.div [
                                                                        prop.className "flex items-center gap-3"
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.className "custom-radio checked w-5 h-5 rounded-full border-2 border-primary relative"
                                                                                prop.children [
                                                                                    Html.div [
                                                                                        prop.className "absolute inset-1 rounded-full bg-primary"
                                                                                    ]
                                                                                ]
                                                                            ]
                                                                            Html.p [
                                                                                prop.className "text-sm"
                                                                                prop.text "Sean Wilken is the main contact for engineering, software roles, and professional outreach."
                                                                            ]
                                                                        ]
                                                                    ]

                                                                    // Xero option
                                                                    Html.div [
                                                                        prop.className "flex items-center gap-3"
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.className "custom-radio checked w-5 h-5 rounded-full border-2 border-secondary relative"
                                                                                prop.children [
                                                                                    Html.div [
                                                                                        prop.className "absolute inset-1 rounded-full bg-secondary"
                                                                                    ]
                                                                                ]
                                                                            ]
                                                                            Html.p [
                                                                                prop.className "text-sm opacity-80"
                                                                                prop.text "Xero Effort is for creative or brand-related inquiries."
                                                                            ]
                                                                        ]
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                            |}
                                            // RIGHT: route-your-message column
                                            ScrollReveal {|
                                                Variant   = SlideLeft
                                                Delay     = 0.08
                                                Threshold = 0.45
                                                Children = 
                                                    Html.div [
                                                        prop.className "space-y-6"
                                                        prop.children [
                                                            Html.p [
                                                                prop.className "cormorant-font text-[0.65rem] tracking-[0.2em] uppercase opacity-60"
                                                                prop.text "Route your message"
                                                            ]

                                                            // Sean card
                                                            Html.a [
                                                                prop.href "mailto:sean.d.wilken@gmail.com"
                                                                prop.className "persona-card block rounded-2xl border border-base-200 bg-base-100 px-4 py-4 lg:px-5 lg:py-4 hover:border-primary/40 hover:-translate-y-[2px] transition-all"
                                                                prop.children [
                                                                    Html.div [
                                                                        prop.className "flex items-center justify-between gap-4"
                                                                        prop.children [

                                                                            Html.div [
                                                                                prop.className "flex items-center gap-3"
                                                                                prop.children [
                                                                                    Html.div [
                                                                                        prop.className "cormorant-font p-3 rounded-full bg-linear-to-br from-blue-400 to-purple-500 flex items-center justify-center text-white text-sm font-medium"
                                                                                        prop.text "SW"
                                                                                    ]
                                                                                    Html.div [
                                                                                        Html.h4 [
                                                                                            prop.className "cormorant-font text-base font-medium"
                                                                                            prop.text "Sean Wilken"
                                                                                        ]
                                                                                        Html.p [
                                                                                            prop.className "text-xs opacity-60"
                                                                                            prop.text "Software engineering, technical opportunities, and professional outreach"
                                                                                        ]
                                                                                    ]
                                                                                ]
                                                                            ]

                                                                            Html.div [
                                                                                prop.className "flex items-center gap-2 text-[0.7rem] tracking-[0.18em] uppercase"
                                                                                prop.children [
                                                                                    Html.span [ prop.text "Email" ]
                                                                                    LucideIcon.ArrowRight "w-3 h-3"
                                                                                ]
                                                                            ]
                                                                        ]
                                                                    ]
                                                                ]
                                                            ]

                                                            // Xero card
                                                            Html.a [
                                                                prop.href "mailto:xeroeffortclub@gmail.com"
                                                                prop.className "persona-card block rounded-2xl border border-base-200 bg-base-100 px-4 py-4 lg:px-5 lg:py-4 hover:border-secondary/40 hover:-translate-y-0.5 transition-all"
                                                                prop.children [
                                                                    Html.div [
                                                                        prop.className "flex items-center justify-between gap-4"
                                                                        prop.children [

                                                                            Html.div [
                                                                                prop.className "flex items-center gap-3"
                                                                                prop.children [
                                                                                    Html.div [
                                                                                        prop.className "cormorant-font p-3 rounded-full bg-linear-to-br from-purple-400 to-pink-500 flex items-center justify-center text-white text-sm font-medium"
                                                                                        prop.text "XE"
                                                                                    ]
                                                                                    Html.div [
                                                                                        Html.h4 [
                                                                                            prop.className "cormorant-font text-base font-medium"
                                                                                            prop.text "Xero Effort"
                                                                                        ]
                                                                                        Html.p [
                                                                                            prop.className "text-xs opacity-60"
                                                                                            prop.text "Creative, design, and brand-related inquiries"
                                                                                        ]
                                                                                    ]
                                                                                ]
                                                                            ]

                                                                            Html.div [
                                                                                prop.className "flex items-center gap-2 text-[0.7rem] tracking-[0.18em] uppercase"
                                                                                prop.children [
                                                                                    Html.span [ prop.text "Email" ]
                                                                                    LucideIcon.ArrowRight "w-3 h-3"
                                                                                ]
                                                                            ]
                                                                        ]
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                            |}
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            // HOW TO REACH OUT
            Html.section [
                prop.className "py-20 px-6 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto"
                        prop.children [
                            Html.p [
                                prop.className "cormorant-font text-lg tracking-[0.2em] uppercase opacity-60 text-center mb-10"
                                prop.text "What to include"
                            ]

                            Html.div [
                                prop.className "grid gap-8 lg:grid-cols-3"
                                prop.children [

                                    // Dev / Resume
                                    Html.div [
                                        prop.className "contact-card rounded-2xl border border-base-200 bg-base-100 px-6 py-8"
                                        prop.children [
                                            Html.div [
                                                prop.className "flex items-center gap-3 mb-4"
                                                prop.children [
                                                    LucideIcon.FileCode2 "w-7 h-7 opacity-70"
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-xl font-light"
                                                        prop.text "Software / Roles"
                                                    ]
                                                ]
                                            ]
                                            Html.p [
                                                prop.className "text-sm opacity-70 mb-5 leading-relaxed"
                                                prop.text "Software engineering roles, product discussions, consulting inquiries, or technical collaboration."
                                            ]
                                            Html.ul [
                                                prop.className "space-y-2 text-sm opacity-60"
                                                prop.children [
                                                    Html.li "• A brief overview of the role, project, or opportunity."
                                                    Html.li "• Relevant context such as stack, scope, timeline, or team setup."
                                                    Html.li "• Links to a job post, repo, product, or supporting materials if available."
                                                ]
                                            ]
                                        ]
                                    ]

                                    // Art / Brand
                                    Html.div [
                                        prop.className "contact-card rounded-2xl border border-base-200 bg-base-100 px-6 py-8"
                                        prop.children [
                                            Html.div [
                                                prop.className "flex items-center gap-3 mb-4"
                                                prop.children [
                                                    LucideIcon.PenTool "w-7 h-7 opacity-70"
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-xl font-light"
                                                        prop.text "Creative / Brand"
                                                    ]
                                                ]
                                            ]
                                            Html.p [
                                                prop.className "text-sm opacity-70 mb-5 leading-relaxed"
                                                prop.text "Design, merchandise, visual collaboration, or brand-related outreach."
                                            ]
                                            Html.ul [
                                                prop.className "space-y-2 text-sm opacity-60"
                                                prop.children [
                                                    Html.li "• A short description of what you are making or exploring."
                                                    Html.li "• Any preferred format, medium, or deliverable."
                                                    Html.li "• References, examples, or visual direction if helpful."
                                                ]
                                            ]
                                        ]
                                    ]

                                    // Something else
                                    Html.div [
                                        prop.className "contact-card rounded-2xl border border-base-200 bg-base-100 px-6 py-8"
                                        prop.children [
                                            Html.div [
                                                prop.className "flex items-center gap-3 mb-4"
                                                prop.children [
                                                    LucideIcon.MessageSquare "w-7 h-7 opacity-70"
                                                    Html.h3 [
                                                        prop.className "cormorant-font text-xl font-light"
                                                        prop.text "General Inquiries"
                                                    ]
                                                ]
                                            ]
                                            Html.p [
                                                prop.className "text-sm opacity-70 mb-5 leading-relaxed"
                                                prop.text "Questions, collaborations, site-related notes, or anything else that does not fit the categories above."
                                            ]
                                            Html.ul [
                                                prop.className "space-y-2 text-sm opacity-60"
                                                prop.children [
                                                    Html.li "• A bit of context about why you are reaching out."
                                                    Html.li "• What you are hoping to discuss or learn more about."
                                                    Html.li "• Any timing considerations if the message is time-sensitive."
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

            // WHAT TO EXPECT
            Client.Components.Shop.Common.Ui.Animations.ProgressiveReveal {
                Children =
                    Html.section [
                        prop.className "py-20 px-6 lg:px-12"
                        prop.children [
                            Html.div [
                                prop.className "max-w-6xl mx-auto"
                                prop.children [
                                    Html.p [
                                        prop.className "cormorant-font text-lg tracking-[0.2em] uppercase opacity-60 text-center mb-10"
                                        prop.text "What to expect"
                                    ]

                                    Html.div [
                                        prop.className "grid gap-8 lg:grid-cols-3"
                                        prop.children [

                                            Html.div [
                                                prop.className "timeline-card rounded-2xl border border-base-200 bg-base-100 px-6 py-8 text-center"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex justify-center mb-5"
                                                        prop.children [ LucideIcon.Inbox "w-9 h-9 opacity-70" ]
                                                    ]
                                                    Html.p [
                                                        prop.className "text-md tracking-[0.15em] uppercase opacity-60 mb-3"
                                                        prop.text "1. Received"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 leading-relaxed"
                                                        prop.text "Your message goes to the appropriate inbox based on the contact path you choose."
                                                    ]
                                                ]
                                            ]

                                            Html.div [
                                                prop.className "timeline-card rounded-2xl border border-base-200 bg-base-100 px-6 py-8 text-center"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex justify-center mb-5"
                                                        prop.children [ LucideIcon.Eye "w-9 h-9 opacity-70" ]
                                                    ]
                                                    Html.p [
                                                        prop.className "text-md tracking-[0.15em] uppercase opacity-60 mb-3"
                                                        prop.text "2. Reviewed"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 leading-relaxed"
                                                        prop.text "I review for fit, context, and how I can respond most helpfully."
                                                    ]
                                                ]
                                            ]

                                            Html.div [
                                                prop.className "timeline-card rounded-2xl border border-base-200 bg-base-100 px-6 py-8 text-center"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex justify-center mb-5"
                                                        prop.children [ LucideIcon.Reply "w-9 h-9 opacity-70" ]
                                                    ]
                                                    Html.p [
                                                        prop.className "text-md tracking-[0.15em] uppercase opacity-60 mb-3"
                                                        prop.text "3. Replied"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 leading-relaxed"
                                                        prop.text "Most messages receive a response within 24 hours."
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

            }
            
            // QUICK NOTES
            Client.Components.Shop.Common.Ui.Animations.ProgressiveReveal {
                Children =
                    Html.section [
                        prop.className "py-20 px-6 lg:px-12 mb-24"
                        prop.children [
                            Html.div [
                                prop.className "max-w-6xl mx-auto"
                                prop.children [
                                    Html.p [
                                        prop.className "cormorant-font text-lg tracking-[0.2em] uppercase opacity-60 text-center mb-10"
                                        prop.text "Quick notes"
                                    ]

                                    Html.div [
                                        prop.className "grid gap-8 lg:grid-cols-2"
                                        prop.children [

                                            Html.div [
                                                prop.className "rounded-2xl border border-base-200 bg-base-100 px-6 py-8 shadow-sm space-y-3"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center gap-2 mb-2"
                                                        prop.children [
                                                            LucideIcon.CheckCircle2 "w-5 h-5 text-primary"
                                                            Html.h3 [
                                                                prop.className "cormorant-font text-lg font-light"
                                                                prop.text "Helpful context"
                                                            ]
                                                        ]
                                                    ]
                                                    Html.ul [
                                                        prop.className "text-sm opacity-70 space-y-2"
                                                        prop.children [
                                                            Html.li "• A bit of background about you, your team, or your company."
                                                            Html.li "• What you are hoping to explore, build, or discuss."
                                                            Html.li "• Any relevant links, constraints, or timing information."
                                                        ]
                                                    ]
                                                ]
                                            ]

                                            Html.div [
                                                prop.className "rounded-2xl border border-base-200 bg-base-100 px-6 py-8 shadow-sm space-y-3"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center gap-2 mb-2"
                                                        prop.children [
                                                            LucideIcon.Info "w-5 h-5 text-secondary"
                                                            Html.h3 [
                                                                prop.className "cormorant-font text-lg font-light"
                                                                prop.text "Good to know"
                                                            ]
                                                        ]
                                                    ]
                                                    Html.ul [
                                                        prop.className "text-sm opacity-70 space-y-2"
                                                        prop.children [
                                                            Html.li "• I read everything, even if I cannot take on every opportunity."
                                                            Html.li "• If your message is time-sensitive, please mention that clearly."
                                                            Html.li "• Please avoid unsolicited marketing or bulk outreach."
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
            }
        ]
    ]
