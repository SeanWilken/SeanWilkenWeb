module Bindings.FramerMotion

open Fable.Core
open Fable.Core.JsInterop
open Feliz

// Basic binding for Framer Motion component
[<Import("motion", from="framer-motion")>]
let motion: obj = jsNative

let inline MotionButton props =
    Interop.reactApi.createElement(motion?button, createObj !!props)

let inline MotionSpan props =
    Interop.reactApi.createElement(motion?span, createObj !!props)