module Welcome

open Feliz
open Shared
open SharedWelcome

let indexTile (imgSrc: string) (webAppSection: SharedWebAppViewSections.AppView) (descrip: string) (dispatch: Msg -> unit) =
    let sectionButtonTitle = SharedWebAppViewSections.appSectionStringTitle webAppSection
    Html.div [
        prop.className "card w-80 bg-base-100 shadow-xl hover:scale-105 transition-transform duration-300"
        prop.children [
            Html.figure [
                Html.img [
                    prop.src imgSrc
                    prop.className "rounded-t-lg object-cover h-48 w-full"
                    prop.alt sectionButtonTitle
                ]
            ]
            Html.div [
                prop.className "card-body"
                prop.children [
                    Html.h2 [
                        prop.className "card-title"
                        prop.text sectionButtonTitle
                    ]
                    Html.p descrip
                    Html.div [
                        prop.className "card-actions justify-end"
                        prop.children [
                            Html.button [
                                prop.className "btn btn-primary"
                                prop.text "Explore"
                                prop.onClick (fun _ -> dispatch (SwitchSection webAppSection))
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let viewMain (dispatch: Msg -> unit) =
    Html.div [
        prop.className "hero bg-gradient-to-br from-primary to-secondary text-base-100 py-16 rounded-xl shadow-xl"
        prop.children [
            Html.div [
                prop.className "hero-content flex-col lg:flex-row-reverse"
                prop.children [
                    Html.img [
                        prop.src "./imgs/Harlot.jpeg"
                        prop.className "max-w-sm rounded-full shadow-2xl w-48 h-48 object-cover mb-6 lg:mb-0"
                        prop.alt "Profile Picture"
                    ]
                    Html.div [
                        prop.className "text-center lg:text-left"
                        prop.children [
                            Html.h1 [
                                prop.className "text-5xl font-bold"
                                prop.text "Welcome."
                            ]
                            Html.p [
                                prop.className "py-4 text-lg"
                                prop.text "This site's my name. Web development is part of my game."
                            ]
                            Html.button [
                                prop.className "btn btn-accent btn-lg"
                                prop.text "Start Exploring"
                                prop.onClick (fun _ -> dispatch (SwitchSection SharedWebAppViewSections.AboutAppView))
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let view2 (dispatch: Msg -> unit) =
    Html.div [
        prop.className "py-16 px-4 max-w-7xl mx-auto text-base-content"
        prop.children [
            Html.h2 [
                prop.className "text-4xl font-bold text-center mb-12"
                prop.text "Explore the Site"
            ]
            Html.div [
                prop.className "grid gap-8 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 xl:grid-cols-4 justify-items-center"
                prop.children [
                    indexTile 
                        "/img/ales-nesetril-unsplash-dev.jpg" 
                        SharedWebAppViewSections.AboutAppView 
                        "About: Learn more about the site and its purpose." 
                        dispatch

                    indexTile 
                        "/img/walkator-unsplash-code.jpg" 
                        SharedWebAppViewSections.PortfolioAppCodeView 
                        "Code: Check out some mini games or code gists." 
                        dispatch

                    indexTile 
                        "/img/nikola-duza-unsplash-shop.jpg" 
                        SharedWebAppViewSections.PortfolioAppDesignView 
                        "Designs: Check out some drawings I've done recently." 
                        dispatch

                    indexTile
                        // "/img/mike-meyers-unsplash-contact.jpg" 
                        "/img/jakub-zerdzicki-unsplash-contact.jpg"
                        SharedWebAppViewSections.ContactAppView 
                        "Contact: Let's hear it already!" 
                        dispatch
                ]
            ]
        ]
    ]

/// 🔮 Placeholder section to inspire ideas
let placeholderSections =
    Html.div [
        prop.className "w-full max-w-6xl mx-auto py-16 px-4 text-base-content space-y-12"
        prop.children [
            Html.div [
                prop.className "prose lg:prose-xl mx-auto text-center"
                prop.children [
                    Html.h3 "💬 Testimonials"
                    Html.p "Coming soon: Quotes from collaborators, mentors, and other developers."
                ]
            ]
            Html.div [
                prop.className "prose lg:prose-xl mx-auto text-center"
                prop.children [
                    Html.h3 "🧰 Tech Stack"
                    Html.p "This site uses F#, Fable, Tailwind, DaisyUI, and more. Full stack details to be added here."
                ]
            ]
            Html.div [
                prop.className "prose lg:prose-xl mx-auto text-center"
                prop.children [
                    Html.h3 "📰 Coming soon"
                    Html.p "Blogs, snippets from upcoming thoughts on code, art, and personal growth, along with a shop to purchase some of the art found here."
                ]
            ]
        ]
    ]

let view (dispatch: Msg -> unit) =
    Html.div [
        prop.className "flex flex-col items-center space-y-24 p-6"
        prop.children [
            viewMain dispatch
            placeholderSections
            view2 dispatch
        ]
    ]