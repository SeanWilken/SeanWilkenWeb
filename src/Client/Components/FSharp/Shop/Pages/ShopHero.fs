namespace Client.Components.Shop.ShopHero

open Feliz
open Client.Components.Shop.Common

module Hero =

    /// Props so parent can wire into routing / ShopSection
    type Props = {
        OnShopCollection : unit -> unit
        OnExploreMore    : unit -> unit
    }

    let view (props: Props) =
        Html.section [
            prop.className "relative min-h-[calc(100vh-4rem)] bg-base-100"
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

                // Content
                Html.div [
                    prop.className "relative h-full flex items-center justify-center px-4 sm:px-6 lg:px-8"
                    prop.children [
                        Html.div [
                            prop.className "max-w-4xl text-center space-y-8"
                            prop.children [
                                Html.h1 [
                                    prop.className "font-light text-5xl md:text-7xl lg:text-8xl text-base-content tracking-tight leading-none"
                                    prop.children [
                                        Html.span "REDEFINE"
                                        Html.br []
                                        Html.span [
                                            prop.className "font-bold"
                                            prop.text "XERO EFFORT"
                                        ]
                                    ]
                                ]

                                Html.p [
                                    prop.className "text-lg md:text-xl text-base-content/70 max-w-2xl mx-auto font-light"
                                    prop.text "Discover where minimal effort meets maximum style. Our curated collection of essentials is designed for those who value simplicity without compromising on quality."
                                ]

                                Html.div [
                                    prop.className "flex flex-wrap gap-4 justify-center pt-6"
                                    prop.children [
                                        Html.button [
                                            prop.className "rounded-none px-8 md:px-10 py-3 md:py-4 btn-primary"
                                            prop.text "Shop Collection"
                                            prop.onClick (fun _ -> props.OnShopCollection())
                                        ]
                                        Html.button [
                                            prop.className "rounded-none px-8 md:px-10 py-3 md:py-4 btn-outline"
                                            prop.text "Explore More"
                                            prop.onClick (fun _ -> props.OnExploreMore())
                                        ]
                                    ]
                                ]
                            ]
                        ]
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
