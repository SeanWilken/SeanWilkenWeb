module ThemePickerBindings

open Feliz
open Fable.Core.JsInterop

type ThemePickerProps = {|
    isOpen: bool
    onClose: unit -> unit
|}

// ReactComponent wrapper accepting props
[<ReactComponent>]
let ThemePicker (props: ThemePickerProps) =
    Feliz.Interop.reactApi.createElement (
        importDefault "../ThemePicker/ThemePicker.tsx",
        !!props
    )