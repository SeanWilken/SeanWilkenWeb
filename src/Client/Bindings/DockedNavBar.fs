module DockedNavBar

open Fable.Core
open Fable.Core.JsInterop
open Feliz

type SubSection = {
    label: string
    href: string
}

[<ImportDefault("../Components/Typescript/DockedNavBar.tsx")>] // adjust path!
let dockedNavBar: obj = jsNative

[<Erase>]
type DockedNavBarProps =
    {|
        items: string array
        activeItem: string
        onSelect: string -> unit
        subSections: SubSection array
        currentSubSection: SubSection
    |}

let inline DockedNavBar (props: DockedNavBarProps) =
    Interop.reactApi.createElement(dockedNavBar, props)