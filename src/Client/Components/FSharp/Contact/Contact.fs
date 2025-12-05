module Components.FSharp.Contact

open Feliz
open Bindings.LucideIcon
open Fable.React

type AnchorLink = {
    Hyperlink: string
    LinkIcon: string
    LinkTitle: string
}

let contactHeaderTitle = "Looking to tell me something?"
let contactHeaderBlurbs = [
    "Drop a line to the appropriate entity..."
    "Whatever it is, PLEASE no spam!"
]

let inquiryReasons =
    [
        Html.div [
            prop.className "max-w-3xl mx-auto grid gap-8 md:grid-cols-2 items-start"
            prop.children [

                // Left: Why get in touch
                Html.div [
                    prop.className "rounded-2xl border border-base-300 bg-base-100/70 backdrop-blur p-6 shadow-sm"
                    prop.children [
                        Html.span [
                            prop.className "inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-semibold tracking-wide uppercase"
                            prop.children [
                                LucideIcon.MessageCircle "w-3 h-3"
                                Html.span "Why get in touch?"
                            ]
                        ]
                        Html.h2 [
                            prop.className "mt-3 text-xl font-semibold text-base-content"
                            prop.text "A single inbox won't cut it."
                        ]
                        Html.p [
                            prop.className "mt-2 text-sm text-base-content/80 leading-relaxed"
                            prop.text "You might want to reach out for freelance work, portfolio inquiries, hiring opportunities, or project collaboration. Whether you're scouting for engineering help or just curious about the code behind this site, you're in the right place."
                        ]
                    ]
                ]

                // Right: Which email to use
                Html.div [
                    prop.className "rounded-2xl border border-base-300 bg-base-100/70 backdrop-blur p-6 shadow-sm"
                    prop.children [
                        Html.span [
                            prop.className "inline-flex items-center gap-2 px-3 py-1 rounded-full bg-accent/10 text-accent text-xs font-semibold tracking-wide uppercase"
                            prop.children [
                                LucideIcon.AtSign "w-3 h-3"
                                Html.span "Which email should I use?"
                            ]
                        ]
                        Html.ul [
                            prop.className "mt-4 space-y-2 text-sm text-base-content/85"
                            prop.children [
                                Html.li "📬 For development, resume and/or professional outreach → Sean Wilken"
                                Html.li "🎨 For art, creative or brand inquiries → Xero Effort"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let swContactEmailAnchor =
    [
        "Got something to say?",
        Some {
            Hyperlink = "mailto:sean.d.wilken@gmail.com"
            LinkIcon = ""
            LinkTitle = "sean.d.wilken@gmail.com"
        }
        "Credentials and career-focused inquiries welcome.",
        None
    ]

let xeContactEmailAnchor =
    [
        "Compliment or creative complaint?",
        Some {
            Hyperlink = "mailto:xeroeffortclub@gmail.com"
            LinkIcon = ""
            LinkTitle = "xeroeffortclub@gmail.com"
        }
        "Reach out for art commissions, designs, or collabs.",
        None
    ]

let contactCard
    (title: string)
    (subtitle: string)
    (icon: ReactElement)
    (accentColorClass: string)   // e.g. "primary" or "secondary"
    (bullets: (string * AnchorLink option) list)
    =
    Html.div [
        // main card
        prop.className "relative w-full rounded-2xl border border-base-300 bg-base-100/80 backdrop-blur shadow-md overflow-hidden hover:shadow-xl hover:-translate-y-1 transition-all duration-200"
        prop.children [

            // accent stripe
            Html.div [
                prop.className (sprintf "absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-%s to-%s/70" accentColorClass accentColorClass)
            ]

            // content
            Html.div [
                prop.className "p-6 space-y-4"
                prop.children [

                    // header row
                    Html.div [
                        prop.className "flex items-center justify-between gap-3 border-b border-base-300 pb-3"
                        prop.children [
                            Html.div [
                                prop.className "flex items-center gap-3"
                                prop.children [
                                    Html.div [
                                        prop.className (sprintf "flex items-center justify-center w-9 h-9 rounded-full bg-%s/10 text-%s" accentColorClass accentColorClass)
                                        prop.children [ icon ]
                                    ]
                                    Html.div [
                                        Html.h2 [
                                            prop.className "text-lg font-semibold text-base-content"
                                            prop.text title
                                        ]
                                        Html.p [
                                            prop.className "text-xs text-base-content/70"
                                            prop.text subtitle
                                        ]
                                    ]
                                ]
                            ]
                            Html.span [
                                prop.className "hidden md:inline-flex items-center gap-1 px-2 py-1 rounded-full bg-base-200 text-[0.7rem] font-medium text-base-content/70"
                                prop.children [
                                    LucideIcon.Clock "w-3 h-3"
                                    Html.span "Typically replies within a day"
                                ]
                            ]
                        ]
                    ]

                    // body bullets
                    for (text, maybeLink) in bullets do
                        Html.div [
                            prop.className "space-y-1 pt-1"
                            prop.children [
                                Html.p [
                                    prop.className "text-sm text-base-content/90"
                                    prop.text text
                                ]
                                match maybeLink with
                                | Some link ->
                                    Html.a [
                                        prop.href link.Hyperlink
                                        prop.className "inline-flex items-center gap-2 text-accent hover:text-accent-focus text-sm font-medium"
                                        prop.children [
                                            LucideIcon.Mail "w-4 h-4"
                                            Html.span [ prop.text link.LinkTitle ]
                                        ]
                                    ]
                                | None -> Html.none
                            ]
                        ]
                ]
            ]
        ]
    ]

let view =
    Html.div [
        prop.className "relative min-h-screen bg-base-100 text-base-content overflow-hidden"
        prop.children [

            // soft background glow
            Html.div [
                prop.className "pointer-events-none absolute inset-0 opacity-60"
                prop.children [
                    Html.div [
                        prop.className "absolute -top-40 -left-40 w-80 h-80 bg-gradient-to-br from-primary/18 via-primary/5 to-transparent rounded-full blur-3xl"
                    ]
                    Html.div [
                        prop.className "absolute -bottom-40 -right-32 w-96 h-96 bg-gradient-to-tl from-secondary/18 via-secondary/5 to-transparent rounded-full blur-3xl"
                    ]
                ]
            ]

            Html.div [
                prop.className "absolute inset-0 bg-base-100/85"
            ]

            Html.div [
                prop.className "relative z-10 px-6 py-10 md:py-16"
                prop.children [
                    Html.div [
                        prop.className "max-w-5xl mx-auto space-y-10"
                        prop.children [

                            // HERO BANNER
                            Html.section [
                                prop.className "relative rounded-3xl border border-base-300 bg-gradient-to-br from-base-100 via-base-100 to-base-200/80 shadow-xl overflow-hidden px-6 py-8 md:px-10 md:py-10"
                                prop.children [

                                    // decorative line at top
                                    Html.div [
                                        prop.className "absolute inset-x-8 top-0 h-[2px] bg-gradient-to-r from-primary via-secondary to-accent"
                                    ]

                                    Html.div [
                                        prop.className "relative grid gap-8 md:grid-cols-[minmax(0,1.7fr)_minmax(0,1.3fr)] items-start"
                                        prop.children [

                                            // LEFT: hero text + main CTAs
                                            Html.div [
                                                prop.className "space-y-5"
                                                prop.children [
                                                    Html.span [
                                                        prop.className "inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-primary/10 text-primary text-xs font-semibold tracking-[0.2em] uppercase"
                                                        prop.children [
                                                            LucideIcon.Send "w-3 h-3"
                                                            Html.span "Contact"
                                                        ]
                                                    ]

                                                    Html.h1 [
                                                        prop.className "text-3xl md:text-4xl font-extrabold text-base-content"
                                                        prop.text contactHeaderTitle
                                                    ]

                                                    Html.p [
                                                        prop.className "text-sm md:text-base text-base-content/80 max-w-xl"
                                                        prop.text "Drop a line to the appropriate persona. Whether it's engineering, resumes, or creative work, this is the front door."
                                                    ]

                                                    // Html.div [
                                                    //     prop.className "flex flex-wrap gap-3 pt-1"
                                                    //     prop.children [
                                                    //         Html.a [
                                                    //             prop.href "mailto:sean.d.wilken@gmail.com"
                                                    //             prop.className "inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-primary text-primary-content text-sm font-semibold shadow-md hover:shadow-lg hover:-translate-y-0.5 transition-all"
                                                    //             prop.children [
                                                    //                 LucideIcon.UserCircle "w-4 h-4"
                                                    //                 Html.span "Email Sean (Dev / Resume)"
                                                    //             ]
                                                    //         ]
                                                    //         Html.a [
                                                    //             prop.href "mailto:xeroeffortclub@gmail.com"
                                                    //             prop.className "inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-base-200 text-base-content text-sm font-semibold border border-base-300 hover:bg-base-300/70 hover:-translate-y-0.5 transition-all"
                                                    //             prop.children [
                                                    //                 LucideIcon.PenTool "w-4 h-4"
                                                    //                 Html.span "Email Xero Effort (Art / Brand)"
                                                    //             ]
                                                    //         ]
                                                    //     ]
                                                    // ]
                                                    Html.div [
                                                        prop.className "space-y-1 pt-2 text-xs text-base-content/70 m-2"
                                                        prop.children [
                                                            Html.p "Two inboxes, same person — choose the lane that matches why you're reaching out."
                                                            Html.p [
                                                                prop.children [
                                                                    Html.span "📬 Dev, resume, or professional outreach → "
                                                                    Html.strong "Sean Wilken"
                                                                ]
                                                            ]
                                                            Html.p [
                                                                prop.children [
                                                                    Html.span "🎨 Art, creative, or brand inquiries → "
                                                                    Html.strong "Xero Effort"
                                                                ]
                                                            ]
                                                        ]
                                                    ]

                                                ]
                                            ]

                                            // RIGHT: route-your-message panel
                                       
                                            Html.div [
                                                prop.className "rounded-2xl border border-base-300 bg-base-100/90 backdrop-blur shadow-sm p-4 md:p-5 space-y-4"
                                                prop.children [

                                                    Html.div [
                                                        prop.className "flex items-center justify-between gap-3"
                                                        prop.children [
                                                            Html.div [
                                                                Html.h2 [
                                                                    prop.className "text-xs font-semibold tracking-wide text-base-content/80 uppercase"
                                                                    prop.text "Route your message"
                                                                ]
                                                                Html.p [
                                                                    prop.className "text-xs text-base-content/65"
                                                                    prop.text "Pick the lane that matches your intent. I'll take it from there."
                                                                ]
                                                            ]
                                                            Html.div [
                                                                prop.className "flex items-center gap-1 px-2 py-1 rounded-full bg-base-200 text-[0.7rem] text-base-content/70"
                                                                prop.children [
                                                                    LucideIcon.Clock "w-3 h-3"
                                                                    Html.span "Replies usually within 24 hours"
                                                                ]
                                                            ]
                                                        ]
                                                    ]

                                                    Html.div [
                                                        prop.className "space-y-2"
                                                        prop.children [

                                                            Html.a [
                                                                prop.href "mailto:sean.d.wilken@gmail.com"
                                                                prop.className "flex items-center justify-between gap-3 rounded-2xl border border-primary/25 bg-primary/5 px-3 py-2.5 hover:bg-primary/10 transition-colors"
                                                                prop.children [
                                                                    Html.div [
                                                                        prop.className "flex items-center gap-3"
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.className "flex items-center justify-center w-8 h-8 rounded-full bg-primary/15 text-primary"
                                                                                prop.children [ LucideIcon.Code2 "w-4 h-4" ]
                                                                            ]
                                                                            Html.div [
                                                                                Html.div [
                                                                                    prop.className "text-sm font-semibold text-base-content"
                                                                                    prop.text "Sean Wilken"
                                                                                ]
                                                                                Html.div [
                                                                                    prop.className "text-xs text-base-content/70"
                                                                                    prop.text "Engineering, resume & professional outreach"
                                                                                ]
                                                                            ]
                                                                        ]
                                                                    ]
                                                                    Html.div [
                                                                        prop.className "flex items-center gap-1 text-xs text-primary"
                                                                        prop.children [
                                                                            Html.span "Email"
                                                                            LucideIcon.ArrowRight "w-3 h-3"
                                                                        ]
                                                                    ]
                                                                ]
                                                            ]

                                                            Html.a [
                                                                prop.href "mailto:xeroeffortclub@gmail.com"
                                                                prop.className "flex items-center justify-between gap-3 rounded-2xl border border-secondary/25 bg-secondary/5 px-3 py-2.5 hover:bg-secondary/10 transition-colors"
                                                                prop.children [
                                                                    Html.div [
                                                                        prop.className "flex items-center gap-3"
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.className "flex items-center justify-center w-8 h-8 rounded-full bg-secondary/15 text-secondary"
                                                                                prop.children [ LucideIcon.Palette "w-4 h-4" ]
                                                                            ]
                                                                            Html.div [
                                                                                Html.div [
                                                                                    prop.className "text-sm font-semibold text-base-content"
                                                                                    prop.text "Xero Effort"
                                                                                ]
                                                                                Html.div [
                                                                                    prop.className "text-xs text-base-content/70"
                                                                                    prop.text "Art, creative work & brand inquiries"
                                                                                ]
                                                                            ]
                                                                        ]
                                                                    ]
                                                                    Html.div [
                                                                        prop.className "flex items-center gap-1 text-xs text-secondary"
                                                                        prop.children [
                                                                            Html.span "Email"
                                                                            LucideIcon.ArrowRight "w-3 h-3"
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
                            ]

                            // // MAIN CONTACT CARDS (deeper detail)
                            // Html.section [
                            //     prop.className "grid grid-cols-1 md:grid-cols-2 gap-6 pt-2"
                            //     prop.children [
                            //         contactCard
                            //             "Sean Wilken"
                            //             "Developer & Resume Contact"
                            //             (LucideIcon.UserCircle "w-5 h-5")
                            //             "primary"
                            //             swContactEmailAnchor

                            //         contactCard
                            //             "Xero Effort"
                            //             "Artistic / Brand Inquiries"
                            //             (LucideIcon.PenTool "w-5 h-5")
                            //             "secondary"
                            //             xeContactEmailAnchor
                            //     ]
                            // ]

                            // AFTER the hero banner section, replace the old contactCard grid with this:

                            // HOW TO REACH OUT – 3 guidance cards
                            Html.section [
                                prop.className "pt-6"
                                prop.children [
                                    Html.h2 [
                                        prop.className "text-sm font-semibold tracking-[0.25em] uppercase text-base-content/60 mb-3"
                                        prop.text "How to reach out"
                                    ]

                                    Html.div [
                                        prop.className "grid grid-cols-1 md:grid-cols-3 gap-4"
                                        prop.children [

                                            // Dev / Resume
                                            Html.div [
                                                prop.className "rounded-2xl border border-base-300 bg-base-100/90 backdrop-blur p-4 shadow-sm flex flex-col gap-2"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center gap-2"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-center justify-center w-8 h-8 rounded-full bg-primary/10 text-primary"
                                                                prop.children [ LucideIcon.UserCircle "w-4 h-4" ]
                                                            ]
                                                            Html.div [
                                                                Html.h3 [
                                                                    prop.className "text-sm font-semibold text-base-content"
                                                                    prop.text "Dev / Resume"
                                                                ]
                                                                Html.p [
                                                                    prop.className "text-xs text-base-content/70"
                                                                    prop.text "Engineering, resume review, contracting, or product help."
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                    Html.ul [
                                                        prop.className "mt-2 space-y-1 text-xs text-base-content/80 list-disc list-inside"
                                                        prop.children [
                                                            Html.li "Share a quick overview of your project or role."
                                                            Html.li "Include tech stack, timelines, and how you prefer to work."
                                                            Html.li "Attach links (GitHub, job posting, brief) if you have them."
                                                        ]
                                                    ]
                                                ]
                                            ]

                                            // Art / Brand
                                            Html.div [
                                                prop.className "rounded-2xl border border-base-300 bg-base-100/90 backdrop-blur p-4 shadow-sm flex flex-col gap-2"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center gap-2"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-center justify-center w-8 h-8 rounded-full bg-secondary/10 text-secondary"
                                                                prop.children [ LucideIcon.Palette "w-4 h-4" ]
                                                            ]
                                                            Html.div [
                                                                Html.h3 [
                                                                    prop.className "text-sm font-semibold text-base-content"
                                                                    prop.text "Art / Brand"
                                                                ]
                                                                Html.p [
                                                                    prop.className "text-xs text-base-content/70"
                                                                    prop.text "Commissions, design, merch ideas, or visual identity."
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                    Html.ul [
                                                        prop.className "mt-2 space-y-1 text-xs text-base-content/80 list-disc list-inside"
                                                        prop.children [
                                                            Html.li "Tell me what vibe or style you're going for."
                                                            Html.li "Mention format (digital, print, apparel, etc.)."
                                                            Html.li "Share any links, references, or mood boards."
                                                        ]
                                                    ]
                                                ]
                                            ]

                                            // Something else
                                            Html.div [
                                                prop.className "rounded-2xl border border-base-300 bg-base-100/90 backdrop-blur p-4 shadow-sm flex flex-col gap-2"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center gap-2"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-center justify-center w-8 h-8 rounded-full bg-accent/10 text-accent"
                                                                prop.children [ LucideIcon.Sparkles "w-4 h-4" ]
                                                            ]
                                                            Html.div [
                                                                Html.h3 [
                                                                    prop.className "text-sm font-semibold text-base-content"
                                                                    prop.text "Something else"
                                                                ]
                                                                Html.p [
                                                                    prop.className "text-xs text-base-content/70"
                                                                    prop.text "Collabs, weird ideas, questions about the site or code."
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                    Html.ul [
                                                        prop.className "mt-2 space-y-1 text-xs text-base-content/80 list-disc list-inside"
                                                        prop.children [
                                                            Html.li "Let me know how you found me."
                                                            Html.li "Give a bit of context and what you’re hoping for."
                                                            Html.li "If it’s time-sensitive, mention any deadlines."
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                            // WHAT TO EXPECT – mini timeline
                            Html.section [
                                prop.className "pt-8"
                                prop.children [
                                    Html.h2 [
                                        prop.className "text-sm font-semibold tracking-[0.25em] uppercase text-base-content/60 mb-3"
                                        prop.text "What to expect"
                                    ]

                                    Html.div [
                                        prop.className "rounded-2xl border border-base-300 bg-base-100/90 backdrop-blur p-5 shadow-sm"
                                        prop.children [
                                            Html.div [
                                                prop.className "grid gap-4 md:grid-cols-3"
                                                prop.children [

                                                    Html.div [
                                                        prop.className "flex flex-col gap-2"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-center gap-2"
                                                                prop.children [
                                                                    Html.div [
                                                                        prop.className "flex items-center justify-center w-8 h-8 rounded-full bg-base-200 text-base-content"
                                                                        prop.children [ LucideIcon.Inbox "w-4 h-4" ]
                                                                    ]
                                                                    Html.span [
                                                                        prop.className "text-xs font-semibold uppercase tracking-wide text-base-content/70"
                                                                        prop.text "1. Received"
                                                                    ]
                                                                ]
                                                            ]
                                                            Html.p [
                                                                prop.className "text-xs text-base-content/75"
                                                                prop.text "Your message lands in the right inbox based on which email you choose."
                                                            ]
                                                        ]
                                                    ]

                                                    Html.div [
                                                        prop.className "flex flex-col gap-2"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-center gap-2"
                                                                prop.children [
                                                                    Html.div [
                                                                        prop.className "flex items-center justify-center w-8 h-8 rounded-full bg-base-200 text-base-content"
                                                                        prop.children [ LucideIcon.Eye "w-4 h-4" ]
                                                                    ]
                                                                    Html.span [
                                                                        prop.className "text-xs font-semibold uppercase tracking-wide text-base-content/70"
                                                                        prop.text "2. Reviewed"
                                                                    ]
                                                                ]
                                                            ]
                                                            Html.p [
                                                                prop.className "text-xs text-base-content/75"
                                                                prop.text "I scan for fit, urgency, and where I can actually be helpful."
                                                            ]
                                                        ]
                                                    ]

                                                    Html.div [
                                                        prop.className "flex flex-col gap-2"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-center gap-2"
                                                                prop.children [
                                                                    Html.div [
                                                                        prop.className "flex items-center justify-center w-8 h-8 rounded-full bg-base-200 text-base-content"
                                                                        prop.children [ LucideIcon.Reply "w-4 h-4" ]
                                                                    ]
                                                                    Html.span [
                                                                        prop.className "text-xs font-semibold uppercase tracking-wide text-base-content/70"
                                                                        prop.text "3. Replied"
                                                                    ]
                                                                ]
                                                            ]
                                                            Html.p [
                                                                prop.className "text-xs text-base-content/75"
                                                                prop.text "Most messages get a thoughtful response within 1–2 days."
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                            // QUICK NOTES / FAQ
                            Html.section [
                                prop.className "pt-8 pb-4"
                                prop.children [
                                    Html.h2 [
                                        prop.className "text-sm font-semibold tracking-[0.25em] uppercase text-base-content/60 mb-3"
                                        prop.text "Quick notes"
                                    ]

                                    Html.div [
                                        prop.className "grid gap-4 md:grid-cols-2"
                                        prop.children [

                                            Html.div [
                                                prop.className "rounded-2xl border border-base-300 bg-base-100/90 backdrop-blur p-4 shadow-sm space-y-2"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center gap-2"
                                                        prop.children [
                                                            LucideIcon.Info "w-4 h-4 text-primary"
                                                            Html.h3 [
                                                                prop.className "text-sm font-semibold text-base-content"
                                                                prop.text "Best messages include..."
                                                            ]
                                                        ]
                                                    ]
                                                    Html.ul [
                                                        prop.className "text-xs text-base-content/80 space-y-1 list-disc list-inside"
                                                        prop.children [
                                                            Html.li "A bit of context about you or your company."
                                                            Html.li "What you’re hoping to explore or solve."
                                                            Html.li "Any timelines, constraints, or links that help."
                                                        ]
                                                    ]
                                                ]
                                            ]

                                            Html.div [
                                                prop.className "rounded-2xl border border-base-300 bg-base-100/90 backdrop-blur p-4 shadow-sm space-y-2"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center gap-2"
                                                        prop.children [
                                                            LucideIcon.HelpCircle "w-4 h-4 text-secondary"
                                                            Html.h3 [
                                                                prop.className "text-sm font-semibold text-base-content"
                                                                prop.text "A few small disclaimers"
                                                            ]
                                                        ]
                                                    ]
                                                    Html.ul [
                                                        prop.className "text-xs text-base-content/80 space-y-1 list-disc list-inside"
                                                        prop.children [
                                                            Html.li "I can’t take on every project, but I do read everything."
                                                            Html.li "If your note is deeply personal or urgent, please say so."
                                                            Html.li "Absolutely no spam or scraped mailing lists, please."
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
            ]
        ]
    ]
