module Components.Layout.LayoutElements

open Feliz
open Fable.React
open Browser.Dom

// -------------------- SectionList --------------------

type SectionListProps = { Title: string; Items: string list }

[<ReactComponent>]
let SectionList (props: SectionListProps) =
    Html.div [
        prop.className "max-w-6xl mx-auto py-16 px-6"
        prop.children [
            // Title
            Html.div [
                prop.className "text-center mb-12 sm:mb-16 lg:mb-20"
                prop.children [
                    Html.div [
                        prop.className "flex items-center justify-center gap-3 mb-4"
                        prop.children [
                            Html.h2 [
                                prop.className "cormorant-font text-3xl sm:text-4xl lg:text-5xl font-light section-title"
                                prop.text props.Title
                            ]
                        ]
                    ]
                ]
            ]

            // Grid of tech items – matches .tech-item from mockup
            Html.div [
                prop.className "grid grid-cols-2 sm:grid-cols-4 gap-4 sm:gap-6 lg:gap-8 text-center"
                prop.children [
                    for item in props.Items do
                        Html.div [
                            prop.className
                                "tech-item text-center cormorant-font text-base sm:text-lg lg:text-xl font-medium"
                            prop.text item
                        ]
                ]
            ]
        ]
    ]

// -------------------- SectionGrid --------------------

type SectionGridItem = {
    Heading: string
    Icon: string
    Description: string
    NavigateTo: unit -> unit
}

type SectionGridProps = { Title: string; Items: SectionGridItem list }

[<ReactComponent>]
let SectionGrid (props: SectionGridProps) =
    Html.div [
        prop.className "max-w-6xl mx-auto py-16 px-6"
        prop.children [
            // Title
            Html.div [
                prop.className "text-center mb-12 sm:mb-16 lg:mb-20"
                prop.children [
                    Html.div [
                        prop.className "flex items-center justify-center gap-3 mb-4"
                        prop.children [
                            Html.h2 [
                                prop.className "cormorant-font text-3xl sm:text-4xl lg:text-5xl font-light section-title"
                                prop.text props.Title
                            ]
                        ]
                    ]
                ]
            ]

            // Service cards – match .service-card from mockup
            Html.div [
                prop.className "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6"
                prop.children [
                    for item in props.Items do
                        Html.div [
                            prop.className
                                "service-card cursor-pointer text-left flex flex-col h-full"
                            prop.onClick (fun _ -> item.NavigateTo())
                            prop.children [
                                Html.div [
                                    prop.className "text-3xl sm:text-4xl mb-4"
                                    prop.text item.Icon
                                ]
                                Html.h4 [
                                    prop.className "cormorant-font text-lg sm:text-xl font-medium mb-3"
                                    prop.text item.Heading
                                ]
                                Html.p [
                                    prop.className "text-xs sm:text-sm opacity-70"
                                    prop.text item.Description
                                ]
                            ]
                        ]
                ]
            ]
        ]
    ]

// -------------------- SectionCarousel --------------------

type CarouselItem = {
    IconElement: ReactElement
    Title: string
    Description: string
}

type SectionCarouselProps = {| Title: string; Items: CarouselItem list |}

[<ReactComponent>]
let SectionCarousel (props: SectionCarouselProps) =
    let total = props.Items.Length

    let currentIndex, setCurrentIndex = React.useState(0)
    let indexRef = React.useRef(0)

    let setIndex next =
        indexRef.current <- next
        setCurrentIndex(next)

    let scrollDir, setScrollDir = React.useState(1)
    let scrollDirRef = React.useRef(scrollDir)

    let progress, setProgress = React.useState(0.0)

    let stepMs = 5000
    let tickMs = 50

    let goPrev () =
        if total > 1 then
            setProgress(0.0)
            setIndex ((indexRef.current - 1 + total) % total)
            setScrollDir(-1)

    let goNext () =
        if total > 1 then
            setProgress(0.0)
            setIndex ((indexRef.current + 1) % total)
            setScrollDir(1)

    let getItem i = props.Items.[(i + total) % total]

    // Keep scrollDirRef in sync
    React.useEffect(
        (fun () -> scrollDirRef.current <- scrollDir),
        [| box scrollDir |]
    )

    // Auto-scroll
    React.useEffectOnce(fun () ->
        if total <= 1 then
            React.createDisposable ignore
        else
            let mutable elapsed = 0
            let timerId =
                window.setInterval(
                    (fun _ ->
                        elapsed <- elapsed + tickMs
                        setProgress (min 100.0 (float elapsed / float stepMs * 100.0))

                        if elapsed >= stepMs then
                            elapsed <- 0
                            setProgress 0.0
                            let dir = scrollDirRef.current
                            let next = (indexRef.current + dir + total) % total
                            setIndex next

                        null
                    ),
                    tickMs
                )

            React.createDisposable(fun () -> window.clearInterval timerId)
    )

    Html.div [
        prop.className "max-w-6xl mx-auto py-16 px-6 space-y-10"
        prop.children [

            // Title
            Html.div [
                prop.className "text-center mb-12 sm:mb-16 lg:mb-20"
                prop.children [
                    Html.div [
                        prop.className "flex items-center justify-center gap-3 mb-4"
                        prop.children [
                            Html.h2 [
                                prop.className "cormorant-font text-3xl sm:text-4xl lg:text-5xl font-light section-title"
                                prop.text props.Title
                            ]
                        ]
                    ]
                ]
            ]

            // Carousel core
            Html.div [
                prop.className "relative flex items-center justify-center overflow-hidden"
                prop.children [

                    // Left chevron
                    if total > 1 then
                        Html.button [
                            prop.style [ style.opacity 0.8 ]
                            prop.className
                                "absolute left-0 top-0 bottom-0 w-16 flex items-center justify-center \
                                 backdrop-blur-md bg-base-100/40 hover:bg-base-200/50 rounded-r-2xl z-10"
                            prop.onClick (fun _ -> goPrev())
                            prop.onMouseEnter (fun _ -> setScrollDir(-1))
                            prop.children [
                                Html.p [
                                    prop.className (
                                        "transition-colors duration-300" +
                                        if scrollDir = -1 then " text-primary" else ""
                                    )
                                    prop.text "<"
                                ]
                            ]
                        ]

                    // Items
                    Html.div [
                        prop.className "flex items-center justify-center w-full gap-4 transition-all duration-500"
                        prop.children (
                            [ -1; 0; 1 ]
                            |> List.map (fun offset ->
                                let item = getItem (currentIndex + offset)
                                let scale = if offset = 0 then 1.0 else 0.85

                                Html.div [
                                    prop.className
                                        "flex-shrink-0 coming-soon-card text-center \
                                         flex flex-col items-center justify-center transition-all duration-500 ease-in-out"
                                    prop.style [
                                        style.transform (transform.scale scale)
                                        style.width 280
                                        style.height 280
                                        if offset <> 0 then style.opacity 0.7
                                    ]
                                    prop.children [
                                        Html.div [
                                            prop.className "text-3xl sm:text-4xl mb-4"
                                            prop.children [ item.IconElement ]
                                        ]
                                        Html.span [
                                            prop.className "cormorant-font text-lg font-medium text-center mb-2"
                                            prop.text item.Title
                                        ]
                                        Html.p [
                                            prop.className "text-xs sm:text-sm opacity-70 text-center"
                                            prop.text item.Description
                                        ]
                                    ]
                                ]
                            )
                        )
                    ]

                    // Right chevron
                    if total > 1 then
                        Html.button [
                            prop.style [ style.opacity 0.8 ]
                            prop.className
                                "absolute right-0 top-0 bottom-0 w-16 flex items-center justify-center \
                                 backdrop-blur-md bg-base-100/40 hover:bg-base-200/50 rounded-l-2xl z-10"
                            prop.onClick (fun _ -> goNext())
                            prop.onMouseEnter (fun _ -> setScrollDir(1))
                            prop.children [
                                Html.p [
                                    prop.className (
                                        "transition-colors duration-300" +
                                        if scrollDir = 1 then " text-primary" else ""
                                    )
                                    prop.text ">"
                                ]
                            ]
                        ]
                ]
            ]

            // Progress bar
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
