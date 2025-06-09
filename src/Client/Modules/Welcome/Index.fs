module Welcome

open Feliz
open Shared
open SharedWelcome

let indexTile (imgSrc: string) (webAppSection: SharedWebAppViewSections.AppSection) (descrip: string) (dispatch: Msg -> unit) =
    let sectionButtonTitle = SharedWebAppViewSections.appSectionStringTitle webAppSection
    Html.div [
        prop.className "card w-72 bg-base-100 shadow-xl m-4 flex flex-col items-center"
        prop.children [
            Html.img [
                prop.src imgSrc
                prop.className "rounded-lg object-cover h-40 w-full"
                prop.alt sectionButtonTitle
            ]
            Html.div [
                prop.className "p-4 text-center text-base-content"
                prop.text descrip
            ]
            Html.button [
                prop.className "btn btn-primary mb-4"
                prop.text sectionButtonTitle
                prop.onClick (fun _ -> dispatch (SwitchSection webAppSection))
            ]
        ]
    ]

let viewMain (dispatch: Msg -> unit) =
    Html.div [
        prop.className "max-w-3xl mx-auto p-8 bg-base-200 rounded-lg shadow-lg text-center"
        prop.children [
            Html.img [
                prop.src "./imgs/Harlot.jpeg"
                prop.className "mx-auto rounded-full w-48 h-48 mb-6 object-cover"
                prop.alt "Profile Picture"
            ]
            Html.h1 [
                prop.className "text-5xl font-extrabold mb-2 text-primary"
                prop.text "Welcome."
            ]
            Html.h2 [
                prop.className "text-2xl mb-1"
                prop.text "This site's my name."
            ]
            Html.h2 [
                prop.className "text-2xl mb-6"
                prop.text "Web development is part of my game."
            ]
            Html.button [
                prop.className "btn btn-secondary btn-lg"
                prop.text "Check it out for yourself."
                prop.onClick (fun _ -> dispatch (SwitchSection SharedWebAppViewSections.AboutAppView))
            ]
        ]
    ]

let view2 (dispatch: Msg -> unit) =
    Html.div [
        prop.className "max-w-6xl mx-auto p-8 bg-base-300 rounded-lg shadow-lg text-white"
        prop.children [
            Html.h1 [
                prop.className "text-4xl font-bold mb-8"
                prop.text "Site Index"
            ]
            Html.div [
                prop.className "flex flex-wrap justify-center"
                prop.children [
                    indexTile "./imgs/Out for Blood.jpeg" SharedWebAppViewSections.AboutAppView "About: Learn more about the site and its purpose." dispatch
                    indexTile "./imgs/Bowing Bubbles.jpeg" SharedWebAppViewSections.PortfolioAppCodeView "Code: Check out some mini games or code gists." dispatch
                    indexTile "./imgs/Misfortune.jpeg" SharedWebAppViewSections.PortfolioAppDesignView "Designs: Check out some drawings I've done recently." dispatch
                    indexTile "./imgs/Backstabber.jpeg" SharedWebAppViewSections.ContactAppView "Contact: Let's hear it already!" dispatch
                ]
            ]
        ]
    ]

let view (dispatch: Msg -> unit) =
    Html.div [
        prop.className "flex flex-col items-center space-y-12 p-8"
        prop.children [
            viewMain dispatch
            view2 dispatch
        ]
    ]
