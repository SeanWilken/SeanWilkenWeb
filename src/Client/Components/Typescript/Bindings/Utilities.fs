module TSXUtilities

open Fable.Core.JsInterop
open Fable.Core

[<Import("getCssVar", "../Utils/Utilities.tsx")>]
let getCssVar (name: string) : string = jsNative


[<Import("convertOklchToHex", "../Utils/Utilities.tsx")>]

let convertOklchToHex (name: string) : string = jsNative