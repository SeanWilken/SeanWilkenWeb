module Components.FSharp.Contact

open Feliz
open Bindings.LucideIcon

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

                                            // LEFT: big serif heading + radios
                                            Html.div [
                                                prop.className "space-y-8"
                                                prop.children [
                                                    Html.h1 [
                                                        prop.className "serif text-4xl md:text-5xl lg:text-6xl font-light leading-tight"
                                                        prop.text "Looking to tell me something?"
                                                    ]

                                                    Html.p [
                                                        prop.className "text-sm md:text-base opacity-70 leading-relaxed max-w-md"
                                                        prop.text
                                                            "Drop a line to the appropriate persona. Whether it's engineering, creative, or brand inquiries, this is the front door."
                                                    ]

                                                    Html.div [
                                                        prop.className "space-y-4 text-sm"
                                                        prop.children [
                                                            Html.p [
                                                                prop.className "text-xs opacity-60"
                                                                prop.text "For reference, same person — choose the lane that matches the inquiry:"
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
                                                                        prop.text "Any creative or professional outreach → Sean Wilken"
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
                                                                        prop.text "Any creative or brand inquiries → Xero Effort"
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]

                                            // RIGHT: route-your-message column
                                            Html.div [
                                                prop.className "space-y-6"
                                                prop.children [
                                                    Html.p [
                                                        prop.className "text-[0.65rem] tracking-[0.2em] uppercase opacity-60"
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
                                                                                prop.className "w-10 h-10 rounded-full bg-gradient-to-br from-blue-400 to-purple-500 flex items-center justify-center text-white text-sm font-medium"
                                                                                prop.text "SW"
                                                                            ]
                                                                            Html.div [
                                                                                Html.h4 [
                                                                                    prop.className "serif text-base font-medium"
                                                                                    prop.text "Sean Wilken"
                                                                                ]
                                                                                Html.p [
                                                                                    prop.className "text-xs opacity-60"
                                                                                    prop.text "Engineering systems & professional outreach"
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
                                                        prop.className "persona-card block rounded-2xl border border-base-200 bg-base-100 px-4 py-4 lg:px-5 lg:py-4 hover:border-secondary/40 hover:-translate-y-[2px] transition-all"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-center justify-between gap-4"
                                                                prop.children [

                                                                    Html.div [
                                                                        prop.className "flex items-center gap-3"
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.className "w-10 h-10 rounded-full bg-gradient-to-br from-purple-400 to-pink-500 flex items-center justify-center text-white text-sm font-medium"
                                                                                prop.text "XE"
                                                                            ]
                                                                            Html.div [
                                                                                Html.h4 [
                                                                                    prop.className "serif text-base font-medium"
                                                                                    prop.text "Xero Effort"
                                                                                ]
                                                                                Html.p [
                                                                                    prop.className "text-xs opacity-60"
                                                                                    prop.text "Any creative work & brand inquiries"
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

                                                    Html.p [
                                                        prop.className "text-xs opacity-50 pt-2"
                                                        prop.text "Replies usually within ~24 hours."
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

            // HOW TO REACH OUT
            Html.section [
                prop.className "py-20 px-6 lg:px-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-6xl mx-auto"
                        prop.children [
                            Html.p [
                                prop.className "text-[0.65rem] tracking-[0.2em] uppercase opacity-60 text-center mb-10"
                                prop.text "How to reach out"
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
                                                        prop.className "serif text-2xl font-light"
                                                        prop.text "Dev / Resume"
                                                    ]
                                                ]
                                            ]
                                            Html.p [
                                                prop.className "text-sm opacity-70 mb-5 leading-relaxed"
                                                prop.text "Engineering, system reviews, contracting, or product help."
                                            ]
                                            Html.ul [
                                                prop.className "space-y-2 text-xs opacity-60"
                                                prop.children [
                                                    Html.li "• Share a quick overview of your project or role."
                                                    Html.li "• Include tech stack, timelines, and how you prefer to work."
                                                    Html.li "• Attach links: GitHub, job posting, brief if you have them."
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
                                                        prop.className "serif text-2xl font-light"
                                                        prop.text "Art / Brand"
                                                    ]
                                                ]
                                            ]
                                            Html.p [
                                                prop.className "text-sm opacity-70 mb-5 leading-relaxed"
                                                prop.text "Commissions, design, merch collab, or casual identity."
                                            ]
                                            Html.ul [
                                                prop.className "space-y-2 text-xs opacity-60"
                                                prop.children [
                                                    Html.li "• Tell me what you're making, or the vibe you're going for."
                                                    Html.li "• Mention format (digital, print, apparel, etc.)."
                                                    Html.li "• Share any links, references, or mood boards."
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
                                                        prop.className "serif text-2xl font-light"
                                                        prop.text "Something else"
                                                    ]
                                                ]
                                            ]
                                            Html.p [
                                                prop.className "text-sm opacity-70 mb-5 leading-relaxed"
                                                prop.text "Collabs, weird ideas, questions about the site, or code."
                                            ]
                                            Html.ul [
                                                prop.className "space-y-2 text-xs opacity-60"
                                                prop.children [
                                                    Html.li "• Let me know how you found me."
                                                    Html.li "• Give a bit of context and what you're hoping for."
                                                    Html.li "• If it's time-sensitive, mention that too (politely)."
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
                                        prop.className "text-[0.65rem] tracking-[0.2em] uppercase opacity-60 text-center mb-10"
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
                                                        prop.className "text-[0.7rem] tracking-[0.15em] uppercase opacity-60 mb-3"
                                                        prop.text "1. Received"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 leading-relaxed"
                                                        prop.text "Your message lands in the right inbox based on which email you choose."
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
                                                        prop.className "text-[0.7rem] tracking-[0.15em] uppercase opacity-60 mb-3"
                                                        prop.text "2. Reviewed"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 leading-relaxed"
                                                        prop.text "I scan for urgency, fit, and where I can actually be helpful."
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
                                                        prop.className "text-[0.7rem] tracking-[0.15em] uppercase opacity-60 mb-3"
                                                        prop.text "3. Replied"
                                                    ]
                                                    Html.p [
                                                        prop.className "text-sm opacity-70 leading-relaxed"
                                                        prop.text "Most messages get a thoughtful response within ~24 hours."
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
                                        prop.className "text-[0.65rem] tracking-[0.2em] uppercase opacity-60 text-center mb-10"
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
                                                                prop.className "serif text-2xl font-light"
                                                                prop.text "Best messages include..."
                                                            ]
                                                        ]
                                                    ]
                                                    Html.ul [
                                                        prop.className "text-sm opacity-70 space-y-2"
                                                        prop.children [
                                                            Html.li "• A bit of context about you or your company."
                                                            Html.li "• What you're hoping to explore or solve."
                                                            Html.li "• Any timelines, constraints, or links that help."
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
                                                                prop.className "serif text-2xl font-light"
                                                                prop.text "A few small disclaimers"
                                                            ]
                                                        ]
                                                    ]
                                                    Html.ul [
                                                        prop.className "text-sm opacity-70 space-y-2"
                                                        prop.children [
                                                            Html.li "• I can't take on every project, but I do read everything."
                                                            Html.li "• If your note is deeply personal or urgent, please say so."
                                                            Html.li "• Absolutely no spam or scripted marketing lists, please."
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
