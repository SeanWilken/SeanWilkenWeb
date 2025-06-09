module Contact

open Feliz

type AnchorLink = {
    Hyperlink: string
    LinkIcon: string
    LinkTitle: string
}

let contactHeaderTitle = "Looking to tell me something?"
let contactHeaderBlurbs = [
    "Drop a line to the appropriate entity.."
    "Whatever it is, NO spam!"
]

let swContactEmailAnchor =
    [   
        "Got something to say?",
        Some { 
            Hyperlink = "mailto:sean.d.wilken@gmail.com"
            LinkIcon = "./imgs/icons/Mail.png"
            LinkTitle = "Sean.D.Wilken@GMail.com" 
        }
        "Credentials coming soon to resumes near you...",
        None
    ]

let xeContactEmailAnchor = 
    [ 
        "Complement or complaint...",
        Some { 
            Hyperlink = "mailto:xeroeffortclub@gmail.com"
            LinkIcon = "./imgs/icons/Mail.png"
            LinkTitle = "XeroEffortClub@GMail.com" 
        }
        "Coming very soon to social platforms near you...",
        None
    ]

let contactCard (title: string) (bullets: (string * AnchorLink option) list) =
    Html.div [
        prop.className "card bg-base-200 shadow-xl p-6 space-y-4 w-full"
        prop.children [
            Html.h2 [
                prop.className "text-xl font-bold text-primary"
                prop.text title
            ]
            for (text, maybeLink) in bullets do
                Html.div [
                    prop.className "space-y-1"
                    prop.children [
                        Html.p [ prop.text text ]
                        match maybeLink with
                        | Some link ->
                            Html.a [
                                prop.href link.Hyperlink
                                prop.className "flex items-center gap-2 text-accent hover:underline"
                                prop.children [
                                    Html.img [
                                        prop.src link.LinkIcon
                                        prop.className "w-6 h-6"
                                    ]
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
        prop.className "min-h-screen px-6 py-10 bg-base-100 text-base-content"
        prop.children [
            Html.div [
                prop.className "max-w-4xl mx-auto space-y-10"
                prop.children [
                    Html.div [
                        prop.className "text-center space-y-2"
                        prop.children [
                            Html.h1 [
                                prop.className "text-3xl font-bold text-primary"
                                prop.text contactHeaderTitle
                            ]
                            for blurb in contactHeaderBlurbs do
                                Html.p [ prop.text blurb ]
                        ]
                    ]
                    Html.div [
                        prop.className "grid grid-cols-1 md:grid-cols-2 gap-8"
                        prop.children [
                            contactCard "Sean Wilken" swContactEmailAnchor
                            contactCard "Xero Effort" xeContactEmailAnchor
                        ]
                    ]
                ]
            ]
        ]
    ]
