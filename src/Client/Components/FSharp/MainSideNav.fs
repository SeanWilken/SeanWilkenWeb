module Components.NavigationMenu

open Feliz
open Bindings.LucideIcon
open Bindings.FramerMotion

let navIconFor label =
    match label with
    | "Home" -> LucideIcon.Home "w-6 h-6"
    | "Projects" -> LucideIcon.Briefcase "w-6 h-6"
    | "About" -> LucideIcon.UserCircle "w-6 h-6"
    | "Resume" -> LucideIcon.File "w-6 h-6"
    | "Contact" -> LucideIcon.Mail "w-6 h-6"
    | "Theme" -> LucideIcon.Palette "w-6 h-6"
    | _ -> Html.none

let navItem (label: string) icon isActive expandedMenu onSelect =
    Html.button [
        prop.className (
            "group w-full rounded-xl flex items-center justify-center px-2 lg:px-4 py-3 " +
            "transition-all duration-200 ease-in-out hover:bg-base-200 overflow-y-auto"
        )
        prop.onClick (fun _ -> onSelect label)
        prop.children [
            Html.div [
                prop.children [
                    Html.div [
                        prop.className (
                            "relative w-10 h-10 flex items-center justify-center shrink-0 " +
                            (if isActive then
                                "before:content-[''] before:absolute before:inset-0 before:bg-primary/20 " +
                                "before:-skew-x-12 before:z-0 before:rounded-md"
                            else "")
                        )
                        prop.children [
                            Html.div [
                                prop.className "w-6 h-6 flex items-center justify-center"
                                prop.children [ icon ]
                            ]
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.className "flex"
                prop.children [
                    Html.span [
                        prop.className (
                            "text-sm items-center font-medium transition-all duration-200 ease-in-out " +
                            (if expandedMenu then "opacity-100 scale-100" else "opacity-0 scale-95 w-0 overflow-hidden")
                        )
                        prop.text label
                    ]
                ]
            ]
        ]
    ]

type SubSection = {
    label: string
    href: string
}

type NavigationMenuProps = {|
    items: string[]
    menuOpen: bool
    activeItem: string
    onSelect: string -> unit
    subSections: SubSection[]
    currentSubSection: SubSection
|}

[<ReactComponent>]
let NavigationMenu (props: NavigationMenuProps) =
    let (isHovered, setHovered) = React.useState false

    React.useEffect (
        fun () ->
            if props.menuOpen
            then setHovered true
            else setHovered false
        , [| box props.menuOpen |]
    )

    Html.div
        [
            prop.className (
                "bg-base-300 shadow-lg flex flex-col py-8 px-4 h-screen " +
                // "transition-all duration-300 " +
                (if isHovered then "w-64" else "w-20")
            )
            prop.onMouseEnter (fun _ -> setHovered true)
            prop.onMouseLeave (fun _ -> setHovered false)
            prop.children [
                Html.div [
                    prop.className "mb-12 flex items-center text-base-content"
                    prop.children [
                        Html.div [ // wrap icon in a fixed-size box
                            prop.className "min-w-[3rem] min-h-[3rem] w-12 h-12 flex items-center justify-center bg-gradient-to-r from-indigo-500 to-purple-600 rounded-xl logo-pulse"
                            prop.children [ LucideIcon.Nucleus "w-6 h-6 text-white" ] // actual icon inside
                        ]
                        
                        Html.h1 [
                            prop.className (
                                "ml-4 text-xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent " +
                                "transition-all duration-300 ease-in-out whitespace-nowrap " +
                                (if isHovered then "opacity-100 translate-x-0 max-w-full" else "opacity-0 -translate-x-4 max-w-0 overflow-hidden")
                            )
                            prop.text "Sean Wilken"
                        ]
                    ]
                ]
                Html.nav [
                    prop.className "flex-1 space-y-1"
                    prop.children [
                        for item in props.items do
                            navItem item (navIconFor item) (item = props.activeItem) isHovered props.onSelect
                    ]
                ]
            ]
        ]
        