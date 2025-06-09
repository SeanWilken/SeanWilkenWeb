module FerroFluidWrapper

open Feliz
// open Feliz.Interop
open Fable.Core
open Fable.Core.JsInterop




[<ReactComponent>]
let FerroFluid () =
    Feliz.Interop.reactApi.createElement (importDefault "../JS/FerrofluidCanvas.tsx", [])

let view () =
    Html.div [
        FerroFluid ()
    ]