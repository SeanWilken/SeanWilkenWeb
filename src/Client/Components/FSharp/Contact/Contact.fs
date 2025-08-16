module Components.FSharp.Contact

open Feliz
open Bindings.LucideIcon

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

let inquiryReasons = [
    Html.div [
        prop.className "prose max-w-2xl mx-auto text-base-content/90 text-center"
        prop.children [
            Html.h2 [ prop.className "text-2xl text-secondary font-semibold mb-2"; prop.text "Why get in touch?" ]
            Html.p "You might want to reach out for freelance work, portfolio inquiries, hiring opportunities, or project collaboration. Whether you're scouting for design help or curious about the code behind this site,  you're in the right place."
        ]
    ]
    Html.div [
        prop.className "prose max-w-2xl mx-auto text-base-content/80 text-center"
        prop.children [
            Html.h2 [ prop.className "text-xl text-accent font-medium"; prop.text "Which email should I use?" ]
            Html.ul [
                Html.li "📬 For development, resume and/or professional outreach → Sean Wilken"
                Html.li "🎨 For art, creative or brand inquiries → Xero Effort"
            ]
        ]
    ]
]

let swContactEmailAnchor =
    [
        "Got something to say?",
        Some {
            Hyperlink = "mailto:sean.d.wilken@gmail.com"
            LinkIcon = "" // Lucide used instead
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

let contactCard (title: string) (subtitle: string) (icon: ReactElement) (bgColor) (bullets: (string * AnchorLink option) list) =
    Html.div [
        prop.className "card bg-base-200/70 backdrop-blur border border-base-300 shadow-xl p-6 space-y-4 w-full hover:shadow-2xl hover:scale-[1.015] transition-all"
        prop.children [
            Html.div [
                prop.className "flex items-center gap-3 border-b border-base-300 pb-3"
                prop.children [
                    icon
                    Html.div [
                        Html.h2 [
                            prop.className $"text-xl font-bold text-{bgColor}"
                            prop.text title
                        ]
                        Html.p [ 
                            prop.className "text-sm text-base-content/70"
                            prop.text subtitle 
                        ]
                    ]
                ]
            ]
            for (text, maybeLink) in bullets do
                Html.div [
                    prop.className "space-y-1"
                    prop.children [
                        Html.p [ prop.className "text-base-content"; prop.text text ]
                        match maybeLink with
                        | Some link ->
                            Html.a [
                                prop.href link.Hyperlink
                                prop.className "flex items-center gap-2 text-accent hover:underline"
                                prop.children [
                                    LucideIcon.Mail "w-5 h-5"
                                    Html.span [ prop.text link.LinkTitle ]
                                ]
                            ]
                        | None -> Html.none
                    ]
                ]
        ]
    ]

let view =
    Html.div [
        prop.className "relative min-h-screen bg-base-100 text-base-content overflow-hidden"
        prop.children [

            // Background image (optional)
            Html.div [
                prop.className "absolute inset-0 bg-cover bg-center opacity-20"
                prop.style [
                    style.backgroundImage "url('/imgs/contact-bg.jpg')"
                ]
            ]

            Html.div [
                prop.className "absolute inset-0 bg-gradient-to-b from-base-100/70 via-base-100/80 to-base-100"
            ]

            Html.div [
                prop.className "relative z-10 px-6 py-12"
                prop.children [
                    Html.div [
                        prop.className "max-w-5xl mx-auto space-y-12"
                        prop.children [

                            // Header
                            Html.div [
                                prop.className "text-center space-y-2"
                                prop.children [
                                    Html.h1 [
                                        prop.className "text-4xl font-bold text-primary"
                                        prop.text contactHeaderTitle
                                    ]
                                    for blurb in contactHeaderBlurbs do
                                        Html.p [ 
                                            prop.className "text-lg text-base-content/80"
                                            prop.text blurb 
                                        ]
                                ]
                            ]

                            // Why inquire
                            yield! inquiryReasons

                            // Cards
                            Html.div [
                                prop.className "grid grid-cols-1 md:grid-cols-2 gap-8 pt-4"
                                prop.children [
                                    contactCard 
                                        "Sean Wilken" 
                                        "Developer & Resume Contact" 
                                        (LucideIcon.UserCircle "w-6 h-6 text-primary") 
                                        "primary"
                                        swContactEmailAnchor

                                    contactCard 
                                        "Xero Effort"
                                        "Artistic / Brand Inquiries" 
                                        (LucideIcon.PenTool "w-6 h-6 text-secondary") 
                                        "secondary"
                                        xeContactEmailAnchor
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]