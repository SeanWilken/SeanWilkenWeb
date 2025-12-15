namespace Client.Components.Shop

open Feliz
open Client.Components.Shop.Common.Ui
open Client.Domain.Store.ProductViewer
open Feliz.UseDeferred
open Elmish
module Product =
    open Client.Domain.Store.ProductViewer
    open Shared.StoreProductViewer

    // let tryPickVariantId (m: Model) (res: GetDetailsResponse) =
    //     match res.Details, m.SelectedSize, m.SelectedColor with
    //     | DetailsCatalog d, Some s, Some c ->
    //         d.Variants
    //         |> List.tryFind (fun v -> v.Size = Some s && v.Color = Some c)
    //         |> Option.map (fun v -> v.VariantId)
    //     | DetailsTemplate d, Some s, Some c ->
    //         d.Variants
    //         |> List.tryFind (fun v -> v.Size = Some s && v.Color = Some c)
    //         |> Option.map (fun v -> v.VariantId)
    //     | _ -> None

    let initFromSeed (seed: ProductSeed) (returnTo: ReturnTo) : Model * Cmd<Msg> =
        let model = {
            Key = keyFromSeed seed
            Seed = Some seed
            ReturnTo = returnTo
            Details = Deferred.HasNotStartedYet
            SelectedVariantId = None
            SelectedColor = None
            SelectedSize = None
        }

        // kick off detail fetch immediately
        model, Cmd.ofMsg LoadDetails

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | InitWith (key, seed, returnTo) ->
            initModel key seed returnTo
            , Cmd.ofMsg LoadDetails

        | LoadDetails ->
            let m1 = { model with Details = Deferred.InProgress }
            m1,
            Cmd.OfAsync.either
                (fun req -> Client.Api.productsApi.getProductDetails req)
                (detailsReq m1)
                GotDetails
                FailedDetails

        | GotDetails res ->
            { model with Details = Deferred.Resolved res }, Cmd.none

        | FailedDetails ex ->
            { model with Details = Deferred.Failed ex }, Cmd.none

        | SelectColor c ->
            // optionally clear variant on change
            let m1 = { model with SelectedColor = Some c; SelectedVariantId = None }
            m1, Cmd.ofMsg LoadDetails

        | SelectSize s ->
            let m1 = { model with SelectedSize = Some s; SelectedVariantId = None }
            m1, Cmd.ofMsg LoadDetails

        | SelectVariant vid ->
            let m1 = { model with SelectedVariantId = Some vid }
            // if you want pricing/image to reflect variant, reload details
            m1, Cmd.ofMsg LoadDetails

        | PrimaryAction ->
            // handled by Shop-level wrapper (Add-to-cart or Select-for-designer)
            model, Cmd.none

        | GoBack ->
            model, Cmd.none


    [<ReactComponent>]
    let view (props: Model) dispatch =
        Section.container [
            Html.div [
                prop.className "py-10 md:py-20 grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-16"
                prop.children [

                    // Media
                    Html.div [
                        prop.className "space-y-4"
                        prop.children [
                            Html.div [
                                prop.className "aspect-[3/4] bg-base-200 flex items-center justify-center text-[7rem] md:text-[9rem] font-light text-base-content/20 rounded-lg"
                                prop.text "1"
                            ]
                            Html.div [
                                prop.className "grid grid-cols-4 gap-3"
                                prop.children [
                                    for i in 1 .. 4 do
                                        Html.div [
                                            prop.key i
                                            prop.className "aspect-square bg-base-200 cursor-pointer hover:ring-2 ring-primary rounded-md flex items-center justify-center text-xl font-light text-base-content/30 transition-all"
                                            prop.text (string i)
                                        ]
                                ]
                            ]
                        ]
                    ]

                    // Details
                    Html.div [
                        prop.className "space-y-8 lg:sticky lg:top-28 h-fit"
                        prop.children [

                            // Title / rating / price
                            Html.div [
                                prop.className "space-y-3"
                                prop.children [
                                    Html.h1 [
                                        prop.className "text-3xl md:text-4xl font-light tracking-tight text-base-content"
                                        prop.text "props.Product.Name"
                                    ]
                                    Html.div [
                                        prop.className "flex items-center gap-3"
                                        prop.children [
                                            Html.div [
                                                prop.className "rating rating-sm"
                                                prop.children [
                                                    for i in 1 .. 5 do
                                                        Html.input [
                                                            prop.key i
                                                            prop.type' "radio"
                                                            prop.className "mask mask-star-2 bg-primary"
                                                            prop.readOnly true
                                                            if i = 5 then
                                                                prop.isChecked true
                                                        ]
                                                ]
                                            ]
                                            Html.span [
                                                prop.className "text-sm text-base-content/70"
                                                prop.text "({props.Product.ReviewCount} reviews)"
                                            ]
                                        ]
                                    ]
                                    Html.p [
                                        prop.className "text-3xl font-light"
                                        prop.text "${props.Product.Price}"
                                    ]
                                ]
                            ]

                            Html.p [
                                prop.className "text-base md:text-lg text-base-content/70 leading-relaxed"
                                prop.text "props.Product.Description"
                            ]

                            // Sizes
                            Html.div [
                                prop.className "space-y-2"
                                prop.children [
                                    Html.label [
                                        prop.className "text-xs font-medium uppercase tracking-[0.25em] text-base-content/60"
                                        prop.text "Select Size"
                                    ]
                                    Html.div [
                                        prop.className "flex flex-wrap gap-2"
                                        prop.children [
                                            for size in [ "props.Product.Sizes" ] do
                                                Html.button [
                                                    prop.key size
                                                    prop.className "btn btn-sm rounded-none border-base-300 hover:border-primary hover:bg-primary hover:text-primary-content"
                                                    prop.text size
                                                ]
                                        ]
                                    ]
                                ]
                            ]

                            // Colors
                            Html.div [
                                prop.className "space-y-2"
                                prop.children [
                                    Html.label [
                                        prop.className "text-xs font-medium uppercase tracking-[0.25em] text-base-content/60"
                                        prop.text "Select Color"
                                    ]
                                    Html.div [
                                        prop.className "flex flex-wrap gap-3"
                                        prop.children [
                                            for colorClass in ["props.Product.Colors"] do
                                                Html.button [
                                                    prop.key colorClass
                                                    prop.className (tw [
                                                        "w-10 h-10 rounded-full border border-base-300 hover:ring-2 ring-offset-2 ring-primary transition-all"
                                                        colorClass
                                                    ])
                                                ]
                                        ]
                                    ]
                                ]
                            ]

                            // Actions
                            Html.div [
                                prop.className "space-y-3 pt-4"
                                prop.children [
                                    Html.button [
                                        prop.className (Btn.primary [ "w-full rounded-none py-4" ])
                                        prop.text "Add to Cart"
                                        prop.onClick (fun _ -> () )
                                        // prop.onClick (fun _ -> dispatch (Msg.AddToCart "") )
                                    ]
                                    Html.button [
                                        prop.className (Btn.outline [ "w-full rounded-none py-4" ])
                                        prop.text "Add to Wishlist"
                                        prop.onClick (fun _ -> () )
                                        // prop.onClick (fun _ -> dispatch (Msg.AddToCart "") )
                                    ]
                                ]
                            ]

                            // Details accordions (DaisyUI collapse)
                            Html.div [
                                prop.className "border-t border-base-300 pt-6 space-y-3"
                                prop.children [
                                    Html.details [
                                        prop.className "collapse collapse-arrow bg-base-100"
                                        prop.children [
                                            Html.summary [
                                                prop.className "collapse-title text-xs font-medium uppercase tracking-[0.25em]"
                                                prop.text "Product Details"
                                            ]
                                            Html.div [
                                                prop.className "collapse-content text-sm text-base-content/70 leading-relaxed"
                                                prop.text "100% organic cotton, pre-shrunk, enzyme washed for softness."
                                            ]
                                        ]
                                    ]
                                    Html.details [
                                        prop.className "collapse collapse-arrow bg-base-100"
                                        prop.children [
                                            Html.summary [
                                                prop.className "collapse-title text-xs font-medium uppercase tracking-[0.25em]"
                                                prop.text "Shipping & Returns"
                                            ]
                                            Html.div [
                                                prop.className "collapse-content text-sm text-base-content/70 leading-relaxed"
                                                prop.text "Free shipping on orders over $100. 30-day return policy."
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
