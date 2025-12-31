module Components.Layout.UnifiedNavigation

open Feliz

type UnifiedNavProps = {| 
    activeItem: string
    onSelect: string -> unit
    isShopBrand: bool
    cartCount: int
    onCartClick: unit -> unit
    onThemeClick: unit -> unit
|}

[<ReactComponent>]
let UnifiedNavigation (props: UnifiedNavProps) =
    let isMobileOpen, setIsMobileOpen = React.useState false

    let brandName =
        if props.isShopBrand then "XERO EFFORT"
        else "SEAN WILKEN"

    let mainNavItems =
        [| {| id = "Home";     label = "Home";     icon = "◆" |}
           {| id = "About";    label = "About";    icon = "○" |}
           {| id = "Contact";  label = "Contact";  icon = "◌" |}
           {| id = "Projects"; label = "Projects"; icon = "◈" |}
           {| id = "Resume";   label = "Resume";   icon = "▢" |}
           {| id = "Services"; label = "Services"; icon = "◇" |}
           {| id = "Shop";     label = "Shop";     icon = "⊙" |} 
        |]

    // If we later want Lucide icons here, we can swap out `icon` strings.
    let utilityButtons =
        [| ("account", "◉", "Account", None) |]

    Html.div [
        prop.className "nav-container cormorant-font"
        prop.children [
            Html.div [ prop.className "nav-backdrop" ]

            Html.div [
                prop.className "nav-content"
                prop.children [

                    // Top bar
                    Html.nav [
                        prop.className "nav-main"
                        prop.children [

                            // Left: logo / brand
                            Html.button [
                                prop.className "logo"
                                prop.onClick (fun _ ->
                                    if props.isShopBrand then props.onSelect "Shop"
                                    else props.onSelect "Home"
                                )
                                prop.text brandName
                            ]

                            // Center: main nav items (desktop only)
                            Html.div [
                                prop.className "hidden md:flex flex-1 justify-center gap-1"
                                prop.children [
                                    for item in mainNavItems do
                                        Html.button [
                                            prop.key item.id
                                            prop.className (
                                                "nav-item " +
                                                (if props.activeItem = item.id then "active" else "")
                                            )
                                            prop.custom ("data-icon", item.icon)
                                            prop.onClick (fun _ -> props.onSelect item.id)
                                            prop.children [
                                                Html.span [ prop.text item.label ]
                                                Html.div [ prop.className "nav-item-indicator" ]
                                            ]
                                        ]
                                ]
                            ]

                            // Right: utility buttons (desktop only)
                            Html.div [
                                prop.className "hidden md:flex items-center gap-2"
                                prop.children [
                                    Html.button [
                                        prop.key "theme"
                                        prop.className "utility-btn"
                                        prop.ariaLabel "Theme"
                                        prop.onClick (fun _ -> props.onThemeClick ())
                                        prop.children [
                                            Html.span [ prop.text "◐" ]
                                        ]
                                    ]
                                ]
                            ]

                            // Mobile: right-side controls
                            Html.div [
                                prop.className "flex items-center gap-2 md:hidden"
                                prop.children [

                                    // Cart
                                    // Html.button [
                                    //     prop.key "m-cart"
                                    //     prop.className "utility-btn"
                                    //     prop.ariaLabel "Cart"
                                    //     prop.onClick (fun _ -> props.onCartClick ())
                                    //     prop.children [
                                    //         Html.span [ prop.text "⊙" ]
                                    //         if props.cartCount > 0 then
                                    //             Html.div [
                                    //                 prop.className "badge"
                                    //                 prop.text (string props.cartCount)
                                    //             ]
                                    //     ]
                                    // ]

                                    // Theme
                                    Html.button [
                                        prop.key "m-theme"
                                        prop.className "utility-btn"
                                        prop.ariaLabel "Theme"
                                        prop.onClick (fun _ -> props.onThemeClick ())
                                        prop.children [
                                            Html.span [ prop.text "◐" ]
                                        ]
                                    ]

                                    // Hamburger
                                    Html.button [
                                        prop.key "m-menu"
                                        prop.className "utility-btn"
                                        prop.ariaLabel "Menu"
                                        prop.custom ("aria-expanded", isMobileOpen)
                                        prop.custom ("aria-controls", "mobile-nav")
                                        prop.onClick (fun _ -> setIsMobileOpen (not isMobileOpen))
                                        prop.children [
                                            Html.span [
                                                prop.className "text-lg leading-none"
                                                prop.text (if isMobileOpen then "×" else "≡")
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Mobile dropdown nav
                    Html.div [
                        prop.id "mobile-nav"
                        prop.className (
                            "m-2 md:hidden bg-base-300/95 border-b border-base-200 " +
                            "overflow-hidden transition-[max-height,opacity] duration-300 " +
                            (if isMobileOpen then "max-h-96 opacity-100" else "max-h-0 opacity-0")
                        )
                        prop.children [
                            Html.div [
                                prop.className "px-4 py-3 flex flex-col gap-1"
                                prop.children [
                                    for item in mainNavItems do
                                        Html.button [
                                            prop.key ("m-" + item.id)
                                            prop.onClick (fun _ ->
                                                props.onSelect item.id
                                                setIsMobileOpen false
                                            )
                                            prop.className (
                                                "w-full flex items-center justify-between px-3 py-2 rounded-lg " +
                                                "text-[12px] tracking-[0.18em] uppercase " +
                                                (if props.activeItem = item.id then
                                                    "bg-base-100 text-primary"
                                                 else
                                                    "text-base-content/80 hover:bg-base-200")
                                            )
                                            prop.children [
                                                Html.span [ prop.text item.label ]
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
