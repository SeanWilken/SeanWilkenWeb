module Components.Layout.LayoutElements

open Feliz
open Fable.React
open Browser.Dom

type SectionListProps = { Title: string; Items: string list }

let SectionList (props: SectionListProps) =
    Html.div [
        prop.className "max-w-6xl mx-auto py-16 px-6"
        prop.children [
            Html.h3 [
                prop.className "clash-font text-3xl lg:text-4xl font-bold text-center text-primary mb-10"
                prop.text props.Title
            ]
            Html.div [
                prop.className "grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4  text-center"
                prop.children (
                    props.Items
                    |> List.map (fun item ->
                        Html.div [
                            prop.className "px-4 py-6 clash-font text-lg text-base-content"
                            prop.text item
                        ])
                )
            ]
        ]
    ]

type SectionGridItem = { Heading: string; Icon: string; Description: string; NavigateTo: unit -> unit  }
type SectionGridProps = { Title: string; Items: SectionGridItem list; }

let SectionGrid (props: SectionGridProps) =
    Html.div [
        prop.className "max-w-6xl mx-auto py-16 px-6"
        prop.children [
            Html.h3 [
                prop.className "clash-font text-3xl lg:text-4xl font-bold text-center text-secondary mb-12"
                prop.text props.Title
            ]
            Html.div [
                prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-10 text-center"
                prop.children (
                    props.Items
                    |> List.map (fun item ->
                        Html.div [
                            prop.className "satoshi-font bg-base-200 rounded-2xl p-8 shadow hover:shadow-lg transition"
                            prop.onClick (fun _ -> item.NavigateTo())
                            prop.children [
                                Html.div [
                                    prop.className "text-4xl mb-4"
                                    prop.text item.Icon
                                ]
                                Html.h4 [
                                    prop.className "text-xl font-semibold mb-2"
                                    prop.text item.Heading
                                ]
                                Html.p [
                                    prop.className "text-base-content/80"
                                    prop.text item.Description
                                ]
                            ]
                        ])
                )
            ]
        ]
    ]

type CarouselItem = { IconElement: ReactElement; Title: string; Description: string }
type SectionCarouselProps = {| Title: string; Items: CarouselItem list |}

[<ReactComponent>]
let SectionCarousel =
    React.functionComponent(fun (props: SectionCarouselProps) ->
        let total = props.Items.Length
        let (currentIndex, setCurrentIndex) = React.useState(0)
        let indexRef = React.useRef(0)
        let setIndex next =
            indexRef.current <- next
            setCurrentIndex(next)

        let (scrollDir, setScrollDir) = React.useState(1)
        let scrollDirRef = React.useRef(scrollDir)
        let (progress, setProgress) = React.useState(0.0)

        let stepMs = 5000
        let tickMs = 50

        let goPrev () =
            if total > 1 then
                setProgress(0.0)
                setIndex((indexRef.current - 1 + total) % total)
                setScrollDir(-1)

        let goNext () =
            if total > 1 then
                setProgress(0.0)
                setIndex((indexRef.current + 1) % total)
                setScrollDir(1)

        let getItem i = props.Items.[(i + total) % total]

        React.useEffect((fun () ->
            scrollDirRef.current <- scrollDir
        ), [| box scrollDir |])

        // Auto-scroll (runs once)
        React.useEffectOnce(fun () ->
            if total <= 1 then React.createDisposable(ignore)
            else
                let mutable elapsed = 0
                let timerId =
                    window.setInterval((fun () ->
                        elapsed <- elapsed + tickMs
                        setProgress(min 100.0 (float elapsed / float stepMs * 100.0))
                        if elapsed >= stepMs then
                            elapsed <- 0
                            setProgress(0.0)
                            let dir = scrollDirRef.current
                            let next = (indexRef.current + dir + total) % total
                            setIndex(next)
                        null), tickMs)
                React.createDisposable(fun () -> window.clearInterval(timerId))
        )

        Html.div [
            prop.className "max-w-6xl mx-auto py-16 px-6 space-y-10"
            prop.children [
                Html.h3 [
                    prop.className "clash-font text-3xl lg:text-4xl font-bold text-center text-primary"
                    prop.text props.Title
                ]
                Html.div [
                    prop.className "relative flex items-center justify-center overflow-hidden"
                    prop.children [

                        // Left Chevron
                        if total > 1 then
                            Html.button [
                                prop.style [ style.opacity 0.8 ]
                                prop.className (
                                    "absolute left-0 top-0 bottom-0 w-16 flex items-center justify-center" +
                                    " backdrop-blur-md bg-base-100/40 hover:bg-base-200/50 rounded-r-2xl z-10 overflow-visible"
                                )
                                prop.onClick (fun _ -> goPrev())
                                prop.onMouseEnter (fun _ -> setScrollDir(-1))
                                prop.children [
                                    Html.p [
                                        prop.className (
                                            "transition-colors duration-300" +
                                            if scrollDir = -1 then " text-primary" else ""
                                        )
                                        prop.text "❮"
                                    ]
                                ]
                            ]

                        // Carousel items
                        Html.div [
                            prop.className "flex items-center justify-center w-full gap-4 transition-all duration-500"
                            prop.children (
                                [ -1; 0; 1 ]
                                |> List.map (fun offset ->
                                    let item = getItem (currentIndex + offset)
                                    let scale = if offset = 0 then 1.0 else 0.85
                                    Html.div [
                                        prop.className "flex-shrink-0 rounded-2xl shadow bg-base-200 p-4 flex flex-col items-center justify-center transition-all duration-500 ease-in-out"
                                        prop.style [
                                            style.transform (transform.scale scale)
                                            style.width 280
                                            style.height 280
                                            if offset <> 0 then style.opacity 0.7
                                        ]
                                        prop.children [
                                            Html.div [ prop.className "text-6xl text-secondary mb-4"; prop.children [ item.IconElement ] ]
                                            Html.span [ prop.className "clash-font text-lg font-semibold text-center break-words text-secondary"; prop.text item.Title ]
                                            Html.p [ prop.className "satoshi-font mt-2 text-sm text-center text-base-content/80"; prop.text item.Description ]
                                        ]
                                    ]
                                )
                            )
                        ]

                        // Right Chevron
                        if total > 1 then
                            Html.button [
                                prop.style [ style.opacity 0.8 ]
                                prop.className (
                                    "absolute right-0 top-0 bottom-0 w-16 flex items-center justify-center " +
                                    "backdrop-blur-md bg-base-100/40 hover:bg-base-200/50 rounded-l-2xl z-10 overflow-hidden"
                                )
                                prop.onClick (fun _ -> goNext())
                                prop.onMouseEnter (fun _ -> setScrollDir(1))
                                prop.children [
                                    Html.p [
                                        prop.className (
                                            "transition-colors duration-300" +
                                            if scrollDir = 1 then " text-primary" else ""
                                        )
                                        prop.text "❯"
                                    ]
                                ]
                            ]
                    ]
                ]
                if total > 1 then
                    Html.div [
                        prop.className "w-full h-2 rounded-full overflow-hidden relative"
                        prop.children [
                            Html.progress [
                                prop.className "progress progress-primary w-full h-2"
                                prop.value progress
                                prop.max 100
                            ]
                        ]
                    ]
            ]
        ]
    )
