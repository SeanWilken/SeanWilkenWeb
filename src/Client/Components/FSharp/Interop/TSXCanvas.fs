module Components.FSharp.Interop.TSXCanvas

open Feliz
open Fable.Core
open Fable.Core.JsInterop

// Define a F# record for props matching the TS interface
type TSXCanvasProps =
    {|
        text: string
        textColor: string 
    |} 

// ReactComponent wrapper accepting props
[<ReactComponent>]
let TSXCanvas (props: TSXCanvasProps) =
    Feliz.Interop.reactApi.createElement (
        importDefault "../../Typescript/CanvasBanner/TSXCanvas.tsx",
        !!props // cast props as JS object
    )

// Usage helper with default values if you want
let defaultView () =
    TSXCanvas
        {| 
            text = "TypeScript Components"
            textColor = "255, 255, 255" |}
