namespace Client.Components.Shop.ShopHero

open Feliz
open Client.Components.Shop.Common.Ui.Animations

module Sections =

    // we don't want these types
    type Product = {
        Name  : string
        Price : string
        Glyph : string
    }

    let private products : Product list = [
        { Name = "Minimal Tee";       Price = "$48";  Glyph = "◆" }
        { Name = "Essential Hoodie";  Price = "$98";  Glyph = "◇" }
        { Name = "Classic Denim";     Price = "$128"; Glyph = "○" }
        { Name = "Leather Sneaker";   Price = "$168"; Glyph = "◈" }
    ]

    [<ReactComponent>]
    let FeaturedProductsSection () =
        // this should call API for 4 items
        Html.section [
            prop.className "relative bg-base-100 py-16 sm:py-24 px-4 sm:px-6 lg:px-10"
            prop.children [

                ScrollReveal {|
                    Variant   = FadeUp
                    Delay     = 0.0
                    Threshold = 0.3
                    Children =
                        Html.div [
                            prop.className "max-w-3xl mx-auto text-center mb-12 sm:mb-16"
                            prop.children [
                                Html.div [
                                    prop.className "text-[11px] tracking-[0.3em] uppercase text-base-content/60 mb-3"
                                    prop.text "Featured"
                                ]
                                Html.h2 [
                                    prop.className "font-light text-3xl sm:text-4xl md:text-5xl text-base-content mb-4"
                                    prop.text "Essential Pieces"
                                ]
                                Html.p [
                                    prop.className "text-sm sm:text-base text-base-content/70"
                                    prop.text "Each item is carefully selected for its versatility, quality, and timeless appeal."
                                ]
                            ]
                        ]
                |}

                Html.div [
                    prop.className "grid gap-6 sm:gap-8 md:grid-cols-2 lg:grid-cols-4 max-w-6xl mx-auto"
                    prop.children [
                        for idx, product in products |> List.indexed do
                            ScrollReveal {|
                                Variant   = Snap
                                Delay     = float idx * 0.1
                                Threshold = 0.2
                                Children =
                                    Html.div [
                                        prop.className "product-card bg-base-200/20 border border-base-300/60 px-6 py-10 text-center transition-transform duration-500 hover:-translate-y-1 hover:bg-base-200/40"
                                        prop.children [
                                            Html.div [
                                                prop.className "text-5xl mb-4 text-base-content/20"
                                                prop.text product.Glyph
                                            ]
                                            Html.h3 [
                                                prop.className "tracking-[0.12em] uppercase text-sm text-base-content mb-1"
                                                prop.text product.Name
                                            ]
                                            Html.p [
                                                prop.className "text-xs text-base-content/60"
                                                prop.text product.Price
                                            ]
                                        ]
                                    ]
                            |}
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    let FinalCtaSection (onExploreCollection: unit -> unit) =
        Html.section [
            prop.className "relative bg-base-100 py-20 sm:py-24 px-4 sm:px-6 lg:px-10"
            prop.children [
                ScrollReveal {|

                    Variant   = ScaleUp
                    Delay     = 0.0
                    Threshold = 0.3
                    Children =
                        Html.div [
                            prop.className "max-w-2xl mx-auto text-center space-y-6"
                            prop.children [
                                Html.h2 [
                                    prop.className "font-light text-3xl sm:text-4xl md:text-5xl text-base-content"
                                    prop.text "Ready to simplify your style?"
                                ]
                                Html.p [
                                    prop.className "text-sm sm:text-base text-base-content/70 mb-4"
                                    prop.text "Browse our collection and discover pieces that work effortlessly with your lifestyle."
                                ]
                                Html.button [
                                    prop.className "btn btn-primary rounded-none px-10 py-3 tracking-[0.18em] uppercase text-xs"
                                    prop.text "Explore Collection"
                                    prop.onClick (fun _ -> onExploreCollection ())
                                ]
                            ]
                        ]
                |}
            ]
        ]

    [<ReactComponent>]
    let StoryProgressiveSection () =
        Html.section [
            prop.className "relative bg-neutral text-base-100 py-24 sm:py-32 px-4 sm:px-6 lg:px-10"
            prop.children [
                ProgressiveReveal {
                    Children =
                        Html.div [
                            prop.className "max-w-3xl mx-auto text-center space-y-6"
                            prop.children [
                                Html.h2 [
                                    prop.className "font-light text-3xl sm:text-4xl md:text-5xl"
                                    prop.text "Designed for those who appreciate the finer details"
                                ]
                                Html.p [
                                    prop.className "text-base sm:text-lg text-base-100/70"
                                    prop.text "Every stitch, every seam, every choice of fabric is made with intention. This is fashion that respects your time, your values, and your style."
                                ]
                            ]
                        ]
                }
            ]
        ]

    type Feature = {
        Icon        : string
        Title       : string
        Description : string
    }

    let private features : Feature list = [
        {
            Icon = "◆"
            Title = "Effortless Quality"
            Description = "Premium materials that feel as good as they look."
        }
        {
            Icon = "◇"
            Title = "Timeless Design"
            Description = "Pieces that transcend seasonal trends."
        }
        {
            Icon = "○"
            Title = "Conscious Craft"
            Description = "Sustainably sourced, ethically produced."
        }
    ]

    [<ReactComponent>]
    let PhilosophySection () =
        Html.section [
            prop.className "relative bg-base-100 py-20 sm:py-28 px-4 sm:px-6 lg:px-10"
            prop.children [

                ScrollReveal {|
                    Variant   = FadeUp
                    Delay     = 0.0
                    Threshold = 0.25
                    Children =
                        Html.div [
                            prop.className "max-w-2xl mx-auto text-center mb-12"
                            prop.children [
                                Html.div [
                                    prop.className "text-[11px] tracking-[0.3em] uppercase text-base-content/60 mb-3"
                                    prop.text "Philosophy"
                                ]
                                Html.h2 [
                                    prop.className "font-light text-3xl sm:text-4xl md:text-5xl text-base-content"
                                    prop.text "Why Xero Effort"
                                ]
                            ]
                        ]
                |}

                Html.div [
                    prop.className "grid gap-10 md:grid-cols-3 max-w-5xl mx-auto"
                    prop.children [
                        for idx, f in features |> List.indexed do

                            ScrollReveal {|
                                Variant   = FadeIn
                                Delay     = float idx * 0.15
                                Threshold = 0.2
                                Children =
                                    Html.div [
                                        prop.className "text-center space-y-4"
                                        prop.children [
                                            Html.div [
                                                prop.className "w-20 h-20 mx-auto rounded-full border border-base-300 flex items-center justify-center text-3xl text-base-content/30 bg-base-200/20"
                                                prop.text f.Icon
                                            ]
                                            Html.h3 [
                                                prop.className "text-sm tracking-[0.18em] uppercase text-base-content"
                                                prop.text f.Title
                                            ]
                                            Html.p [
                                                prop.className "text-sm text-base-content/70 leading-relaxed"
                                                prop.text f.Description
                                            ]
                                        ]
                                    ]
                            |}
                    ]
                ]
            ]
        ]

module Hero =

    /// Props so parent can wire into routing / ShopSection
    type Props = {
        OnShopCollection : unit -> unit
        OnExploreMore    : unit -> unit
    }

    [<ReactComponent>]
    let View (props: Props) =
        Html.section [
            prop.className "relative min-h-[calc(100vh-4rem)] bg-base-100 content-center"
            prop.children [

                // Background gradient / texture using theme colors
                Html.div [
                    prop.className "pointer-events-none absolute inset-0 bg-gradient-to-br from-neutral via-base-300/40 to-base-100"
                    prop.children [
                        Html.div [
                            prop.className "absolute inset-0 opacity-20 bg-[radial-gradient(circle_at_top,_#ffffff22,_transparent_60%)]"
                        ]
                    ]
                ]

                // Content with scroll reveal
                Html.div [
                    prop.className "relative h-full flex items-center justify-center px-4 sm:px-6 lg:px-8"
                    prop.children [
                        ScrollReveal {|
                            Variant   = FadeUp
                            Delay     = 0.1
                            Threshold = 0.3
                            Children =
                                Html.div [
                                    prop.children [
                                        Html.h1 [
                                            prop.className "font-light text-5xl md:text-7xl lg:text-8xl text-base-content tracking-tight leading-none"
                                            prop.children [
                                                Html.div [
                                                    prop.className "hero-eyebrow"
                                                    prop.text "REDEFINE"
                                                ]
                                                Html.span [
                                                    prop.className "hero-title"
                                                    prop.children [
                                                        Html.div [
                                                            prop.className "hero-title-highlight"
                                                            prop.text "XERO EFFORT"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]

                                        Html.p [
                                            prop.className "hero-description"
                                            prop.text "Discover where minimal effort meets maximum style. Our curated collection of essentials is designed for those who value simplicity without compromising on quality."
                                        ]

                                        Html.div [
                                            prop.className "hero-cta-group"
                                            prop.children [
                                                Html.button [
                                                    prop.className "btn btn-primary rounded-none px-10 py-3 tracking-[0.18em] uppercase text-xs"
                                                    prop.text "Shop Collection"
                                                    prop.onClick (fun _ -> props.OnShopCollection())
                                                ]
                                                // Html.button [
                                                //     prop.className "cta-btn cta-btn-secondary"
                                                //     prop.text "Explore More"
                                                //     prop.onClick (fun _ -> props.OnExploreMore())
                                                // ]
                                            ]
                                        ]
                                    ]
                                ]
                        |}
                    ]
                ]

                // Scroll indicator
                Html.div [
                    prop.className "absolute bottom-8 left-1/2 -translate-x-1/2 animate-bounce"
                    prop.children [
                        Html.div [
                            prop.className "w-7 h-12 border-2 border-base-content rounded-full flex justify-center pt-2"
                            prop.children [
                                Html.div [
                                    prop.className "w-1.5 h-2.5 bg-base-content rounded-full"
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

