module Components.Layout.PageLayout

open Feliz
open Client.Domain
open SharedWebAppModels
open SharedWebAppViewSections
open Components.Layout.UnifiedNavigation

type PageLayoutProps = {| 
    children: ReactElement
    model: WebAppModel
    dispatch: WebAppMsg -> unit 
|}

[<ReactComponent>]
let PageLayout (props: PageLayoutProps) =
    let activeItem, setActiveItem = React.useState "Home"
    let themePickerOpen, setThemePickerOpen = React.useState false

    // Derive activeItem from the model
    React.useEffect(
        fun _ ->
            setActiveItem (
                match props.model.CurrentAreaModel with
                | Welcome     -> "Home"
                | About _     -> "About"
                | Portfolio _ -> "Projects"
                | Resume      -> "Resume"
                | Services _  -> "Services"
                | Contact     -> "Contact"
                | Shop _      -> "Shop"
                | Landing     -> ""
                | Help        -> "Help"
                | Settings    -> "Settings"
            )
        , [| box props.model.CurrentAreaModel |]
    )

    let isShopBrand =
        match props.model.CurrentAreaModel with
        | Shop _ -> true
        | _      -> false

    let handleNavSelect (item: string) =
        match item with
        | "Home" ->
            props.dispatch (SwitchToOtherApp WelcomeAppView)
        | "About" ->
            props.dispatch (SwitchToOtherApp AboutAppView)
        | "Projects" ->
            props.dispatch (SwitchToOtherApp PortfolioAppLandingView)
        | "Resume" ->
            props.dispatch (SwitchToOtherApp ResumeAppView)
        | "Contact" ->
            props.dispatch (SwitchToOtherApp ContactAppView)
        | "Services" ->
            props.dispatch (SwitchToOtherApp (ProfessionalServicesAppView ServicesLanding))
        | "Shop" ->
            props.dispatch (SwitchToOtherApp ShopAppView)
        | _ -> ()

    Html.div [
        prop.className "flex flex-col min-h-screen bg-base-100 text-base-content cormorant-font"
        prop.children [

            // Theme picker modal
            ThemePickerBindings.ThemePicker {| 
                isOpen = themePickerOpen
                onClose = fun _ -> setThemePickerOpen false 
            |}

            // Unified top nav
            UnifiedNavigation {| 
                activeItem   = activeItem
                onSelect     = handleNavSelect
                isShopBrand  = isShopBrand
                cartCount    = 2          // TODO: wire to real cart count later
                onCartClick  = fun () -> props.dispatch (SwitchToOtherApp ShopAppView)
                onThemeClick = fun () -> setThemePickerOpen (not themePickerOpen)
            |}

            // Main content area
            Html.main [
                prop.className "flex-1 mt-16"
                prop.id "inner-main-content"
                prop.children [ props.children ]
            ]
        ]
    ]
