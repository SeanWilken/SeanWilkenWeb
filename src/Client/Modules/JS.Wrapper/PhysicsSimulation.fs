module PhysicsPlayground

open Feliz
open Fable.Core.JsInterop

[<ReactComponent>]
let PhysicsSimulation () =
    Feliz.Interop.reactApi.createElement (importDefault "../JS/PhysicsSimulation.tsx", [])

let view () =
    Html.div [
        PhysicsSimulation ()
    ]