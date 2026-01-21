module Components.FSharp.Portfolio.ArtGallery

open Elmish
open Feliz
open Browser
open Bindings.LucideIcon
open Client.Components.Shop.Common.Ui.Animations
open TSXDemos
open Feliz.UseDeferred
open Client.Api

type GalleryArtPiece = {
    ArtPiece: Shared.ArtGalleryViewer.ArtPiece
    ShiftDirection: RevealVariant
    DetailsSide: DetailsSide
}
and DetailsSide =
    | Left
    | Right

type Msg =
    | BackToPortfolio
    | LoadGalleryPieces
    | LoadedGalleryPieces of Shared.ArtGalleryViewer.ArtPiece list
    | FailedToLoadGalleryPieces of exn

type Model = {
    ArtPieces: Deferred<GalleryArtPiece list>
}

let loadGalleryCmd =
    Cmd.OfAsync.either
        artGalleryApi.GetGallery
        ()
        LoadedGalleryPieces
        FailedToLoadGalleryPieces

let initialModel = { ArtPieces = Deferred.HasNotStartedYet } 

let init () : Model * Cmd<Msg> =
    initialModel, Cmd.ofMsg LoadGalleryPieces

let update (msg: Msg) (model: Model) =
    match msg with
    | BackToPortfolio -> model, Cmd.none
    | LoadGalleryPieces -> model, loadGalleryCmd
    | LoadedGalleryPieces artPieces ->
        { model with 
            ArtPieces =
                artPieces
                |> List.mapi (fun idx artPiece ->
                    let side = if idx % 2 = 0 then Left else Right
                    let direction =
                        match idx % 4 with
                        | 0 -> SlideRight
                        | 1 -> FadeIn
                        | 2 -> Snap
                        | _ -> ScaleUp

                    {
                        ArtPiece = artPiece
                        DetailsSide = side
                        ShiftDirection = direction
                    }
                )
                |> Deferred.Resolved
        }, Cmd.none
    | FailedToLoadGalleryPieces ex ->
        { model with ArtPieces = Deferred.Failed ex }, Cmd.none

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
let ArtPieceCard (art: GalleryArtPiece) =
    let isLeft = art.DetailsSide = Left

    let detailsColOrder = if isLeft then "lg:order-1" else "lg:order-2"
    let imageColOrder   = if isLeft then "lg:order-2" else "lg:order-1"

    let overlap =
        if isLeft then "lg:-mr-10 xl:-mr-16"
        else "lg:-ml-10 xl:-ml-16"

    Html.section [
        prop.key art.ArtPiece.DesignKey
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
                                                                                        prop.text $"{art.ArtPiece.CreatedAt}"
                                                                                    ]
                                                                                ]
                                                                            ]

                                                                            Html.h2 [
                                                                                prop.className "font-serif text-3xl sm:text-4xl leading-tight"
                                                                                prop.text art.ArtPiece.Title
                                                                            ]

                                                                            Html.p [
                                                                                prop.className "text-sm sm:text-base leading-relaxed opacity-75"
                                                                                prop.text art.ArtPiece.Description
                                                                            ]

                                                                            if not art.ArtPiece.Tags.IsEmpty then
                                                                                Html.div [
                                                                                    prop.className "pt-1 flex flex-wrap gap-2"
                                                                                    prop.children [
                                                                                        art.ArtPiece.Tags
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
                                                                        prop.src art.ArtPiece.ImageUrl
                                                                        prop.alt art.ArtPiece.Title
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
                    match model.ArtPieces with
                    | Deferred.HasNotStartedYet
                    | Deferred.InProgress ->
                        Html.div [
                            prop.className "w-full py-20 flex justify-center"
                            prop.children [
                                Html.div [
                                    prop.className "loading-spinner loading-lg loading"
                                ]
                            ]
                        ]
                    | Deferred.Failed ex ->
                        Html.div [
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
                                            prop.src "/img/artwork/this-is-quiet.png"
                                            prop.alt "Errawr!!!!"
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
                                Html.div [
                                    prop.className "w-full py-20 text-center text-red-500"
                                    prop.text $"Oh no! we failed to load the art gallery"
                                ]
                            ]
                        ]
                    | Deferred.Resolved artPieces ->
                        artPieces
                        |> List.map ArtPieceCard
                        |> React.fragment
                ]
            ]

            GalleryFooter()
        ]
    ]
