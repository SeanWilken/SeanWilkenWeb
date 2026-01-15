module Components.FSharp.Portfolio.ArtGallery

open Elmish
open Feliz
open Browser
open Bindings.LucideIcon
open Client.Components.Shop.Common.Ui.Animations
open TSXDemos

type ArtPiece = {
    Id: string
    Title: string
    Description: string
    Year: string
    Medium: string
    Tags: string list
    ImageSrc: string
    ShiftDirection: RevealVariant
    DetailsSide: DetailsSide
}
and DetailsSide =
    | Left
    | Right

type Msg =
    | BackToPortfolio

type Model = {
    ArtPieces: ArtPiece list
}

let artworks : ArtPiece list = [
    {
        Id = "null-space"
        Title = "Null Space"
        Description = "An exploration of duality and transformation..."
        Year = "2026"
        Medium = "Digital Illustration"
        Tags = ["Portrait"; "Conceptual"; "Monochrome"]
        ImageSrc = "../../img/artwork/null-space.png"
        ShiftDirection = ScaleUp
        DetailsSide = Right
    }
    {
        Id = "fuming-beauty"
        Title = "Fuming Beauty"
        Description = "Blazing through life while it lasts."
        Year = "2026"
        Medium = "Digital Illustration"
        Tags = []
        ImageSrc = "../../img/artwork/fuming-beauty.png"
        ShiftDirection = Snap
        DetailsSide = Left
    }
    {
        Id = "this-is-quiet"
        Title = "This Is Quiet"
        Description = "A study in contradictions—the skull, surrounded by instruments of chaos and noise, declares silence..."
        Year = "2026"
        Medium = "Digital Illustration"
        Tags = ["Ironic"; "Statement"; "Detailed"]
        ImageSrc = "../../img/artwork/this-is-quiet.png"
        ShiftDirection = FadeIn
        DetailsSide = Right
    }
    {
        Id = "burning-blossom"
        Title = "Burning Blossom"
        Description = "Burning up cause it's cherry blossom season"
        Year = "2026"
        Medium = "Digital Illustration"
        Tags = ["Floral"; "Pattern"; "Bold"]
        ImageSrc = "../../img/artwork/burning-blossom.png"
        ShiftDirection = FadeUp
        DetailsSide = Left
    }
    {
        Id = "vices"
        Title = "Vices"
        Description = "Careful what you choose..."
        Year = "2026"
        Medium = "Digital Illustration"
        Tags = ["Chaotic"; "Colored";]
        ImageSrc = "../../img/artwork/vices.png"
        ShiftDirection = ScaleUp
        DetailsSide = Right
    }
    {
        Id = "forever-burning"
        Title = "Forever Burning"
        Description = "Skeletal hands cradle a burning rose—a meditation on passion..."
        Year = "2025"
        Medium = "Mixed Media Digital"
        Tags = ["Symbolic"; "Color"; "Narrative"]
        ImageSrc = "../../img/artwork/forever-burning.png"
        ShiftDirection = SlideRight
        DetailsSide = Left
    }
    {
        Id = "roses"
        Title = "Roses"
        Description = "A bold pattern exploring the duality of beauty and chaos..."
        Year = "2026"
        Medium = "Digital Pattern Design"
        Tags = ["Floral"; "Pattern"; "Bold"]
        ImageSrc = "../../img/artwork/roses.png"
        ShiftDirection = Snap
        DetailsSide = Right
    }
    {
        Id = "caution-very-hot"
        Title = "Caution: Very Hot"
        Description = "You know what they say?"
        Year = "2026"
        Medium = "Digital Pattern Design"
        Tags = [ "Shadows"; "Colored" ]
        ImageSrc = "../../img/artwork/caution-very-hot.png"
        ShiftDirection = Snap
        DetailsSide = Left
    }
    {
        Id = "emotion-flow"
        Title = "Emotion Flow"
        Description = "Going therough it all again..."
        Year = "2026"
        Medium = "Digital Art"
        Tags = [ "Flow"; "Composition" ]
        ImageSrc = "../../img/artwork/emotion-flow.png"
        ShiftDirection = ScaleUp
        DetailsSide = Right
    }
    {
        Id = "mistfortune"
        Title = "Mistfortune"
        Description = "It was never in the cards to begin with."
        Year = "2021"
        Medium = "Digital Illustration"
        Tags = []
        ImageSrc = "../../img/artwork/misfortune.png"
        ShiftDirection = SlideLeft
        DetailsSide = Left
    }
    {
        Id = "out-for-blood"
        Title = "Out for Blood"
        Description = "Blood lust runs high"
        Year = "2021"
        Medium = "Digital Illustration"
        Tags = ["Ironic"; "Statement"; "Detailed"]
        ImageSrc = "../../img/artwork/out-for-blood.png"
        ShiftDirection = FadeUp
        DetailsSide = Right
    }
    {
        Id = "bows-and-bubbles"
        Title = "Bows & Bubbles"
        Description = "What's poppin?"
        Year = "2022"
        Medium = "Digital Illustration"
        Tags = [ "Ironic" ]
        ImageSrc = "../../img/artwork/bows-and-bubbles.jpg"
        ShiftDirection = SlideRight
        DetailsSide = Left
    }
    {
        Id = "xray"
        Title = "Null XRay"
        Description = "Minimal contours to define form, not beauty."
        Year = "2026"
        Medium = "Digital Illustration"
        Tags = ["Portrait"; "Minimalist"; "Monochrome"]
        ImageSrc = "../../img/artwork/blurred-outline.png"
        ShiftDirection = FadeIn
        DetailsSide = Right
    }
]

let initialModel = { ArtPieces = artworks } 

let init () : Model * Cmd<Msg> =
    initialModel, Cmd.none

let update (msg: Msg) (model: Model) =
    match msg with
    | BackToPortfolio -> model, Cmd.none

type RightHeaderIcon = {
    icon: ReactElement
    label: string
    externalLink: string option
    externalAlt: string option
}

type GalleryHeaderProps = {
    onClose: unit -> unit
    rightIcon: RightHeaderIcon option
}

let galleryHeaderControls (props: GalleryHeaderProps) =
    Html.div [
        prop.className "flex items-center justify-between"
        prop.children [
            Html.button [
                prop.className "btn btn-ghost btn-sm gap-2"
                prop.onClick (fun _ -> props.onClose())
                prop.children [
                    LucideIcon.ChevronLeft "w-5 h-5"
                    Html.span [
                        prop.className "hidden sm:inline text-xs tracking-widest uppercase opacity-70"
                        prop.text "Back to portfolio"
                    ]
                ]
            ]

            match props.rightIcon with
            | Some icon ->
                Html.a [
                    match icon.externalLink with
                    | Some href -> prop.href href
                    | None -> ()
                    prop.target "_blank"
                    prop.className "btn btn-ghost btn-sm gap-2"
                    prop.title (icon.externalAlt |> Option.defaultValue icon.label)
                    prop.children [
                        icon.icon
                        Html.span [
                            prop.className "text-xs tracking-widest uppercase opacity-70"
                            prop.text icon.label
                        ]
                    ]
                ]
            | None ->
                Html.div [ prop.className "w-1 h-1 opacity-0" ]
        ]
    ]

let tagPill (tag: string) =
    Html.span [
        // Daisy badge gives you theme awareness automatically
        prop.className "tag tag-outline text-xs tracking-widest uppercase px-3 py-3 opacity-80 hover:opacity-100"
        prop.text tag
    ]

[<ReactComponent>]
let ArtPieceCard (art: ArtPiece) =
    let isLeft = art.DetailsSide = Left

    let detailsColOrder = if isLeft then "lg:order-1" else "lg:order-2"
    let imageColOrder   = if isLeft then "lg:order-2" else "lg:order-1"

    let overlap =
        if isLeft then "lg:-mr-10 xl:-mr-16"
        else "lg:-ml-10 xl:-ml-16"

    Html.section [
        prop.key art.Id
        prop.className "min-h-[130vh] sm:min-h-[140vh] lg:min-h-[155vh] flex items-center"
        prop.children [
            Html.div [
                prop.className "w-full"
                prop.children [
                    Html.div [
                        prop.className "sticky top-16 h-[calc(100vh-4rem)] flex items-center"
                        prop.children [
                            Html.div [
                                prop.className "mx-auto w-full max-w-[1400px] px-4 sm:px-6 lg:px-8"
                                prop.children [

                                    ScrollReveal {|
                                        Variant   = art.ShiftDirection
                                        Delay     = 0.08
                                        Threshold = 0.45
                                        Children =
                                            Html.div [
                                                prop.className "grid grid-cols-1 lg:grid-cols-12 items-center gap-6 lg:gap-10 min-w-0"
                                                prop.children [

                                                    // DETAILS
                                                    Html.div [
                                                        prop.className
                                                            $"lg:col-span-4 {detailsColOrder} relative z-10 min-w-0 {overlap}"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className
                                                                    "card bg-base-100/80 backdrop-blur-xl border border-base-content/10 shadow-xl rounded-2xl"
                                                                prop.children [
                                                                    Html.div [
                                                                        prop.className "card-body p-6 sm:p-8 gap-4"
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.className "flex items-center justify-between gap-3"
                                                                                prop.children [
                                                                                    Html.p [
                                                                                        prop.className "text-[11px] tracking-[0.28em] uppercase opacity-60"
                                                                                        prop.text $"{art.Year} • {art.Medium}"
                                                                                    ]
                                                                                ]
                                                                            ]

                                                                            Html.h2 [
                                                                                prop.className "font-serif text-3xl sm:text-4xl leading-tight"
                                                                                prop.text art.Title
                                                                            ]

                                                                            Html.p [
                                                                                prop.className "text-sm sm:text-base leading-relaxed opacity-75"
                                                                                prop.text art.Description
                                                                            ]

                                                                            if not art.Tags.IsEmpty then
                                                                                Html.div [
                                                                                    prop.className "pt-1 flex flex-wrap gap-2"
                                                                                    prop.children [
                                                                                        art.Tags
                                                                                        |> List.map tagPill
                                                                                        |> React.fragment
                                                                                    ]
                                                                                ]
                                                                        ]
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]

                                                    // IMAGE (with shader background inside the card)
                                                    Html.div [
                                                        prop.className $"lg:col-span-8 {imageColOrder} min-w-0"
                                                        prop.children [
                                                            Html.div [
                                                                // "card" container for image + shader, contained and theme aware
                                                                prop.className
                                                                    "relative overflow-hidden rounded-2xl border border-base-content/10 shadow-2xl"

                                                                prop.children [

                                                                    // Shader background layer (contained to this card)
                                                                    Html.div [
                                                                        prop.className "absolute inset-0"
                                                                        prop.children [
                                                                            ShaderGradientBackground {|
                                                                                className = Some "opacity-80"
                                                                                intensity = Some 0.55
                                                                            |}

                                                                            // readability/contrast veil so art stays readable
                                                                            Html.div [
                                                                                prop.className "absolute inset-0 bg-base-100/30"
                                                                            ]
                                                                        ]
                                                                    ]

                                                                    // Image (above shader)
                                                                    Html.img [
                                                                        prop.src art.ImageSrc
                                                                        prop.alt art.Title
                                                                        // prop.loading.lazy
                                                                        prop.className
                                                                            "relative z-10 w-full h-auto object-contain"
                                                                    ]

                                                                    // Subtle frame ring (above everything)
                                                                    Html.div [
                                                                        prop.className
                                                                            "pointer-events-none absolute inset-0 rounded-2xl ring-1 ring-base-content/10 z-20"
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]
                                    |}
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]



[<ReactComponent>]
let GalleryHero () =
    Html.header [
        prop.className "min-h-screen flex items-center border-b border-base-content/10"
        prop.children [
            Html.div [
                prop.className "mx-auto w-full max-w-6xl px-4 sm:px-6 lg:px-8 text-center space-y-4"
                prop.children [
                    Html.h1 [
                        prop.className "cormorant-font text-5xl sm:text-7xl lg:text-8xl font-light tracking-wide"
                        prop.text "Gallery"
                    ]
                    Html.p [
                        prop.className "text-xs tracking-[0.35em] uppercase opacity-60"
                        prop.text "Selected Works"
                    ]

                    // Scroll indicator, theme-friendly
                    Html.div [
                        prop.className "pt-10 flex justify-center"
                        prop.children [
                            Html.div [
                                prop.className "w-7 h-12 rounded-full border border-base-content/20 flex justify-center pt-2 opacity-60 animate-bounce"
                                prop.children [
                                    Html.div [ prop.className "w-1.5 h-2.5 rounded-full bg-base-content/70" ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

[<ReactComponent>]
let GalleryFooter () =
    Html.footer [
        prop.className "min-h-[40vh] flex items-center border-t border-base-content/10"
        prop.children [
            Html.div [
                prop.className "mx-auto w-full max-w-6xl px-4 sm:px-6 lg:px-8 text-center"
                prop.children [
                    Html.p [
                        prop.className "text-xs tracking-[0.25em] uppercase opacity-50"
                        prop.text "End of Gallery"
                    ]
                ]
            ]
        ]
    ]

[<ReactComponent>]
let ArtGalleryPage (model: Model) dispatch =
    Html.section [
        prop.className "w-full"
        prop.children [
            Html.div [
                prop.className "mx-auto w-full max-w-6xl px-4 sm:px-6 lg:px-8 pt-6"
                prop.children [
                    galleryHeaderControls {
                        onClose = fun () -> BackToPortfolio |> dispatch
                        rightIcon = Some {
                            icon = LucideIcon.Instagram "w-5 h-5"
                            label = "Instagram"
                            externalLink = Some "https://www.instagram.com/xeroeffort/"
                            externalAlt = Some "View artwork on Instagram"
                        }
                    }
                ]
            ]

            GalleryHero()

            Html.div [
                prop.className "space-y-0"
                prop.children [
                    for art in model.ArtPieces do
                        ArtPieceCard art
                ]
            ]

            GalleryFooter()
        ]
    ]
