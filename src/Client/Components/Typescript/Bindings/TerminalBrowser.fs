module TerminalBrowserBindings

open Feliz
open Fable.Core.JsInterop

type TerminalBrowserProps = {|
    onNavigate: string -> unit
    onOpenViewer: string -> unit
    onOpenAsset: string -> unit
    className: string option
|}

// ReactComponent wrapper accepting props

[<ReactComponent>]
let TerminalBrowser (props: TerminalBrowserProps) =
    Feliz.Interop.reactApi.createElement (
        importDefault "../Demos/TerminalBrowser.tsx",
        !!props // cast props as JS object
    )