module TSXHeaderCanvasWrapper

open Feliz
open Fable.Core
open Fable.Core.JsInterop

// Define a F# record for props matching the TS interface
type TSXHeaderCanvasProps =
    {|
        text: string
        textColor: string 
    |} 

// ReactComponent wrapper accepting props
[<ReactComponent>]
let TSXHeaderCanvasComponent (props: TSXHeaderCanvasProps) =
    Feliz.Interop.reactApi.createElement (
        importDefault "../JS/TSXHeaderCanvas.tsx",
        !!props // cast props as JS object
    )

// Usage helper with default values if you want
let defaultView () =
    TSXHeaderCanvasComponent
        {| 
            text = "TypeScript Components"
            textColor = "255, 255, 255" |}
