module Bindings.FramerMotion

open Fable.Core
open Fable.Core.JsInterop
open Feliz

[<Import("motion", from="framer-motion")>]
let motion: obj = jsNative

// Generic: motionEl "div" [ ... ]
let inline motionEl (tag: string) (props: IReactProperty list) =
    Interop.reactApi.createElement(motion?(tag), createObj !!props)

// Common shorthands
let inline MotionDiv (props: IReactProperty list) =
    Interop.reactApi.createElement(motion?div, createObj !!props)

let inline MotionSection (props: IReactProperty list) =
    Interop.reactApi.createElement(motion?section, createObj !!props)

let inline MotionNav (props: IReactProperty list) =
    Interop.reactApi.createElement(motion?nav, createObj !!props)

let inline MotionHeader (props: IReactProperty list) =
    Interop.reactApi.createElement(motion?header, createObj !!props)

let inline MotionButton (props: IReactProperty list) =
    Interop.reactApi.createElement(motion?button, createObj !!props)

let inline MotionSpan (props: IReactProperty list) =
    Interop.reactApi.createElement(motion?span, createObj !!props)
