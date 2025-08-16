module Components.Layout.PageLayout

open Feliz
open Bindings.LucideIcon
open NavigationMenu
open Shared.SharedWebAppModels
open Shared.SharedWebAppViewSections

type PageLayoutProps = {|
    children: ReactElement
    model: WebAppModel
    dispatch: WebAppMsg -> unit
|}

[<ReactComponent>]
let PageLayout (props: PageLayoutProps) =
    let activeItem, setActiveItem = React.useState "Home"
    let currentSub, setCurrentSub = React.useState { label = "Intro"; href = "intro" }
    let isOpen, setIsOpen = React.useState false
    let themePickerOpen, setThemePickerOpen = React.useState false

    React.useEffect (
        fun _ ->
            setActiveItem (
                match props.model.CurrentAreaModel with
                | Welcome -> "Home"
                | About _ -> "About"
                | Portfolio _ -> "Projects"
                | Resume -> "Resume"
                | Contact -> "Contact"
                | _ -> "Home"
            )

       , [| box props.model.CurrentAreaModel |]
    )

    Html.div [
        prop.className "flex h-screen"
        prop.children [

            ThemePickerBindings.ThemePicker {|
                isOpen = themePickerOpen
                onClose = fun _ -> setThemePickerOpen false
            |}

            // Sidebar
            Html.div [
                prop.className (
                    "fixed lg:static z-40 top-0 left-0 h-full bg-white shadow-lg transform transition-transform duration-300 ease-in-out " +
                    (if isOpen then "translate-x-0" else "-translate-x-full lg:translate-x-0")
                )
                prop.children [
                    NavigationMenu {|
                        items = [| "Home"; "About"; "Projects"; "Resume"; "Contact"; "Theme" |]
                        activeItem =

                            match props.model.CurrentAreaModel with
                            | Welcome -> "Home"
                            | About _ -> "About"
                            | Portfolio _ -> "Projects"
                            | Resume -> "Resume"
                            | Contact -> "Contact"
                            | _ -> "Home"
                        
                        onSelect = fun item ->

                            match item with
                            | "Home" -> props.dispatch (SwitchToOtherApp WelcomeAppView)
                            | "About" -> props.dispatch (SwitchToOtherApp AboutAppView)
                            | "Projects" -> props.dispatch (SwitchToOtherApp PortfolioAppLandingView)
                            | "Resume" -> props.dispatch (SwitchToOtherApp ResumeAppView)
                            | "Contact" -> props.dispatch (SwitchToOtherApp ContactAppView)
                            | "Theme" -> setThemePickerOpen true
                            | _ -> ()

                            setIsOpen false
                        menuOpen = isOpen
                        subSections = [|
                            { label = "Intro"; href = "intro" }
                            { label = "Details"; href = "details" }
                        |]
                        currentSubSection = currentSub
                    |}
                ]
            ]

            // Mobile overlay
            Html.div [
                prop.className (
                    "fixed inset-0 bg-black bg-opacity-40 z-30 lg:hidden transition-opacity duration-300 " +
                    (if isOpen then "opacity-100 pointer-events-auto" else "opacity-0 pointer-events-none")
                )
                prop.onClick (fun _ -> setIsOpen(false))
            ]

            // Main content
            Html.div [
                prop.className "flex flex-col flex-1 overflow-y-auto"
                prop.children [

                    // Mobile top nav
                    Html.header [
                        prop.className "lg:hidden flex items-center justify-between bg-base-300 shadow px-4 py-3 sticky top-0 z-20"
                        prop.children [
                            Html.div [
                                prop.className "flex flex-row space-x-2 text-primary"
                                prop.children [
                                    navIconFor activeItem
                                    Html.p activeItem
                                ]
                            ]
                            Html.h1 [
                                // prop.className "text-lg font-semibold manrope-x-heavy text-primary"
                                prop.className "clash-font text-4xl bg-gradient-to-r from-primary to-secondary bg-clip-text text-transparent"
                                prop.text "Sean Wilken"
                            ]

                            Html.button [
                                prop.className "p-2"
                                prop.onClick (fun _ -> setIsOpen(not isOpen))
                                prop.children [ LucideIcon.Menu "w-6 h-6 bg-gradient-to-r from-primary to-secondary bg-clip-text text-secondary" ]
                            ]
                        ]
                    ]

                    // Wrapped page content
                    Html.main [
                        prop.className "p-4"
                        prop.children [ props.children ]
                    ]
                ]
            ]
        ]
    ]