module TSXNavBarWrapper

open Feliz
open Fable.Core
open Fable.Core.JsInterop

type TSXNavBarProps =
    {| 
        items: string[]
        activeItem: string
        onSelect: string -> unit 
    |}

[<ReactComponent>]
let TSXNavBarComponent (props: TSXNavBarProps) =
    let tsxNavBar: obj = importDefault "../JS/TSXNavBar.tsx"
    Interop.reactApi.createElement(tsxNavBar, props)

[<ReactComponent>]
let Gallery () =
    let (activeTab, setActiveTab) = React.useState("Canvas Header")

    Html.div [
        Html.div [
            prop.className "min-h-screen bg-base-100 text-white"
            prop.children [
                TSXNavBarComponent {|

                    items = [| "Canvas Header"; "Particles"; "Editor"; "UI Card" |]
                    activeItem = activeTab
                    onSelect = setActiveTab
                |}

                Html.div [
                    prop.className "p-6"
                    prop.text $"Currently showing: {activeTab}"
                ]
            ]
        ]
    ]
