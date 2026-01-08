module TSXDemos

open Fable.Core
open Feliz

// ===========================
// Demo components (no props)
// ===========================

[<Import("SynthNeverSets", "../Demos/SynthNeverSets.tsx")>]
let SynthNeverSets () : ReactElement = jsNative

[<Import("ParallaxScrollDemo", "../Demos/Animations.tsx")>]
let ParallaxScrollDemo () : ReactElement = jsNative

[<Import("MouseFollowDemo", "../Demos/Animations.tsx")>]
let MouseFollowDemo () : ReactElement = jsNative

[<Import("GPU3DTransformDemo", "../Demos/Animations.tsx")>]
let GPU3DTransformDemo () : ReactElement = jsNative

[<Import("ParticleSystemDemo", "../Demos/Animations.tsx")>]
let ParticleSystemDemo () : ReactElement = jsNative

[<Import("MagneticElementsDemo", "../Demos/Animations.tsx")>]
let MagneticElementsDemo () : ReactElement = jsNative

[<Import("ShaderGradientDemo", "../Demos/Animations.tsx")>]
let ShaderGradientDemo () : ReactElement = jsNative


// ===========================
// Core / reusable pieces
// (props object style)
// ===========================

type MagneticProps = {|
    children: ReactElement
    className: string option
    strength: float option
|}
    // abstract children: ReactElement with get, set
    // abstract className: string option with get, set
    // abstract strength: float option with get, set

[<Import("Magnetic", "../Demos/Animations.tsx")>]
let Magnetic (props: MagneticProps) : ReactElement = jsNative


type MagneticButtonProps = {|
    children: ReactElement
    className: string option
    gradientClassName: string option
|}
    // abstract children: ReactElement with get, set
    // abstract className: string option with get, set
    // abstract gradientClassName: string option with get, set

[<Import("MagneticButton", "../Demos/Animations.tsx")>]
let MagneticButton (props: MagneticButtonProps) : ReactElement = jsNative


type TiltCardProps = {|
    children: ReactElement
    className: string option
    maxTilt: int option
    perspective: int option
|}
    // abstract children: ReactElement with get, set
    // abstract className: string option with get, set
    // abstract maxTilt: int option with get, set
    // abstract perspective: int option with get, set

[<Import("TiltCard", "../Demos/Animations.tsx")>]
let TiltCard (props: TiltCardProps) : ReactElement = jsNative


type ParticleFieldBackgroundProps = {|
    className: string option
    count: int option
    interactionRadius: int option
    linkDistance: int option
    trailAlpha: float option
|}
    // abstract className: string option with get, set
    // abstract count: int option with get, set
    // abstract interactionRadius: int option with get, set
    // abstract linkDistance: int option with get, set
    // abstract trailAlpha: float option with get, set

[<Import("ParticleFieldBackground", "../Demos/Animations.tsx")>]
let ParticleFieldBackground (props: ParticleFieldBackgroundProps) : ReactElement = jsNative


type ShaderGradientBackgroundProps = {|
    className: string option
    intensity: float option
|}
    // abstract className: string option with get, set
    // abstract intensity: float option with get, set

[<Import("ShaderGradientBackground", "../Demos/Animations.tsx")>]
let ShaderGradientBackground (props: ShaderGradientBackgroundProps) : ReactElement = jsNative


type MouseFollowBackgroundProps = {|
    className: string option
    blobSize: int option
|}
    // abstract className: string option with get, set
    // abstract blobSize: int option with get, set

[<Import("MouseFollowBackground", "../Demos/Animations.tsx")>]
let MouseFollowBackground (props: MouseFollowBackgroundProps) : ReactElement = jsNative


// ===========================
// Demo showcase (default export)
// includes back callback
// ===========================

module AnimationDemo =

    type Msg =
        // round messages
        | QuitDemo // Leave this game and return to the code gallery

    type DemoShowcaseProps = {|
        onBack: (unit -> unit) option
        initialIndex: int option 
    |}
        // abstract onBack: (unit -> unit) option with get, set
        // abstract initialIndex: int option with get, set

    [<ImportDefault("../Demos/Animations.tsx")>]
    let DemoShowcase (props: DemoShowcaseProps) : ReactElement = jsNative
