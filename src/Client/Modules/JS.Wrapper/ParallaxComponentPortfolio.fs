module MyApp.Components.ProgrammingExamplesPage

open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Feliz

[<ImportDefault("path-to-tsx/ProgrammingExamplesPage")>]
let programmingExamplesPage: obj = jsNative

[<ReactComponent>]
let ParallaxComponentPortfolio () =
    Feliz.Interop.reactApi.createElement (importDefault "../JS/ParallaxComponentPortfolio.tsx", [])

[<ReactComponent>]
let view () =
    Html.div [
        ParallaxComponentPortfolio ()
    ]