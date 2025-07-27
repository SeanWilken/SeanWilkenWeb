module SubNavbarBindings

open Feliz
open Fable.Core.JsInterop

type SubSection = {|
    label: string
    href: string
|}

type SubNavbarProps = {|
    CurrentSubSection: SubSection
    SubSections: SubSection array
|}

// ReactComponent wrapper accepting props
[<ReactComponent>]
let SubNavbar (props: SubNavbarProps) =
    Feliz.Interop.reactApi.createElement (
        importDefault "../Components/Typescript/SubNavbar.tsx",
        !!props // cast props as JS object
    )