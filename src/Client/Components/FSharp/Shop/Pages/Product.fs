namespace Client.Components.Shop

open Feliz
open Client.Components.Shop.Common.Ui
open Client.Domain.Store.ProductViewer
open Feliz.UseDeferred
open Elmish
open Shared.StoreProductViewer

module Product =


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

    let private distinctBy (f: 'a -> 'b) (xs: 'a list) =
        xs |> List.fold (fun acc x -> if acc |> List.exists (fun y -> f y = f x) then acc else acc @ [x]) []

    let private tryGetSelectedVariant (model: Model) (p: SyncProduct.SyncProduct) =
        let byId =
            model.SelectedVariantId
            |> Option.bind (fun id -> p.Variants |> List.tryFind (fun v -> v.Id = id))

        let bySizeColor =
            match model.SelectedSize, model.SelectedColor with
            | Some s, Some c ->
                p.Variants
                |> List.tryFind (fun v -> v.Size = Some s && v.Color = Some c)
            | _ -> None

        byId |> Option.orElse bySizeColor |> Option.orElse (p.Variants |> List.tryHead)

    let private formatMoney (amount: decimal option) (currency: string option) =
        match amount with
        | None -> "—"
        | Some a ->
            let c = currency |> Option.defaultValue "USD"
            // keep it simple for now; you can do real currency formatting later
            if c = "USD" 
            then $"${a}" 
            else $"{a} {c}"

    let private derivedSizes (p: SyncProduct.SyncProduct) =
        p.Variants
        |> List.filter (fun v -> 
            match v.Availability with
            | Some "discontinued" -> false
            | Some "out of stock" -> false
            | _ -> true
        )
        |> List.choose (fun v -> v.Size)
        |> List.distinct

    let private derivedColors (p: SyncProduct.SyncProduct) =
        p.Variants
        |> List.filter (fun v -> 
            match v.Availability with
            | Some "discontinued" -> false
            | Some "out of stock" -> false
            | _ -> true
        )
        |> List.choose (fun v ->
            v.Color |> Option.map (fun name -> name, v.Color))
        |> distinctBy fst
    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | InitWith (key, seed, returnTo) ->
            initModel key seed returnTo
            , Cmd.ofMsg LoadDetails

        | LoadDetails ->
            let m1 = { model with Details = Deferred.InProgress }
            m1,
            // Cmd.none
            Cmd.OfAsync.either
                (fun req -> Client.Api.productsApi.getSyncProductVariantDetails req)
                (detailsReq m1)
                GotDetails
                FailedDetails

        | GotDetails res ->
            { model with Details = Deferred.Resolved res }, Cmd.none

        | FailedDetails ex ->
            { model with Details = Deferred.Failed ex }, Cmd.none

        | SelectColor c ->
            // optionally clear variant on change
            let updatedModel =
                match model.Details  with
                | Deferred.Resolved d -> 
                    tryGetSelectedVariant model d.product
                    |> Option.map (fun v -> v.Id)
                    |> fun variantId -> { model with SelectedVariantId = variantId }
                | _ -> model
            let m1 = { updatedModel with SelectedColor = Some c }
            m1, Cmd.none

        | SelectSize s ->
            let updatedModel =
                match model.Details  with
                | Deferred.Resolved d -> 
                    tryGetSelectedVariant model d.product
                    |> Option.map (fun v -> v.Id)
                    |> fun variantId -> { model with SelectedVariantId = variantId }
                | _ -> model
            let m1 = { updatedModel with SelectedSize = Some s }
            m1, Cmd.none // Cmd.ofMsg LoadDetails

        | SelectVariant vid ->
            let m1 = { model with SelectedVariantId = Some vid }
            // if you want pricing/image to reflect variant, reload details
            m1, Cmd.none // Cmd.ofMsg LoadDetails

        | PrimaryAction ->
            // handled by Shop-level wrapper (Add-to-cart or Select-for-designer)
            model, Cmd.none

        | GoBack ->
            model, Cmd.none

    [<ReactComponent>]
    let View (props: Model) dispatch =
        let content =
            match props.Details with
            | Deferred.HasNotStartedYet ->
                // Html.div [ prop.className "py-16 text-base-content/60"; prop.text "Loading product…" ]
                Html.button [  prop.onClick (fun _ -> dispatch LoadDetails); prop.text "Load Product" ]

            | Deferred.InProgress ->
                Html.div [ prop.className "py-16 text-base-content/60"; prop.text "Loading product…" ]

            | Deferred.Failed err ->
                Html.div [
                    prop.className "py-16 space-y-4"
                    prop.children [
                        Html.div [ prop.className "alert alert-error"; prop.text $"Failed to load product: {err.Message}" ]
                        Html.button [
                            prop.className (Btn.outline [ "rounded-none" ])
                            prop.text "Back"
                            prop.onClick (fun _ -> dispatch GoBack)
                        ]
                    ]
                ]

            | Deferred.Resolved details ->
                let product = details.product

                let selectedVariantOpt = tryGetSelectedVariant props product

                let heroImg =
                    selectedVariantOpt
                    |> Option.bind (fun v -> v.PreviewUrl)
                    |> Option.orElse product.ThumbnailUrl

                // if discontinued or no available variants, show message instead

                let priceText =
                    match selectedVariantOpt with
                    | Some v -> formatMoney v.RetailPrice v.Currency
                    | None ->
                        // fallback: min retail price across variants
                        let minPrice =
                            product.Variants
                            |> List.choose (fun v -> v.RetailPrice)
                            |> List.sort
                            |> List.tryHead
                        formatMoney minPrice (product.Variants |> List.tryPick (fun v -> v.Currency))

                let sizes = derivedSizes product
                let colors = derivedColors product

                Html.div [
                    prop.className "py-10 md:py-20 grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-16"
                    prop.children [

                        // Media
                        Html.div [
                            prop.className "space-y-4"
                            prop.children [
                                Html.div [
                                    prop.className "aspect-[3/4] bg-base-200 overflow-hidden rounded-lg flex items-center justify-center"
                                    prop.children [
                                        match heroImg with
                                        | Some url ->
                                            Html.img [
                                                prop.src url
                                                prop.alt product.Name
                                                prop.className "w-full h-full object-cover"
                                            ]
                                        | None ->
                                            Html.div [
                                                prop.className "text-[7rem] md:text-[9rem] font-light text-base-content/20"
                                                prop.text "—"
                                            ]
                                    ]
                                ]

                                // Thumbnail strip (optional) – show first 4 variant images
                                let thumbs =
                                    product.Variants
                                    |> List.choose (fun v -> v.PreviewUrl |> Option.map (fun u -> v.VariantId, u))
                                    |> distinctBy snd
                                    |> List.truncate 4

                                if thumbs |> List.isEmpty then Html.none
                                else
                                    Html.div [
                                        prop.className "grid grid-cols-4 gap-3"
                                        prop.children [
                                            for (vid, url) in thumbs do
                                                Html.button [
                                                    prop.key (string vid)
                                                    prop.className "aspect-square bg-base-200 rounded-md overflow-hidden hover:ring-2 ring-primary transition-all"
                                                    prop.onClick (fun _ -> dispatch (SelectVariant vid))
                                                    prop.children [
                                                        Html.img [
                                                            prop.src url
                                                            prop.className "w-full h-full object-cover"
                                                        ]
                                                    ]
                                                ]
                                        ]
                                    ]
                            ]
                        ]

                        // Details
                        Html.div [
                            prop.className "space-y-8 lg:sticky lg:top-28 h-fit"
                            prop.children [

                                Html.div [
                                    prop.className "space-y-3"
                                    prop.children [
                                        Html.h1 [
                                            prop.className "text-3xl md:text-4xl font-light tracking-tight text-base-content"
                                            prop.text product.Name
                                        ]
                                        Html.p [
                                            prop.className "text-3xl font-light"
                                            prop.text priceText
                                        ]
                                        match product.ExternalId with
                                        | Some eid ->
                                            Html.p [
                                                prop.className "text-xs text-base-content/60"
                                                prop.text $"#{eid}"
                                            ]
                                        | None -> Html.none
                                    ]
                                ]

                                // Sizes (derived)
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
                                                for s in sizes do
                                                    let isSelected = props.SelectedSize = Some s
                                                    Html.button [
                                                        prop.key s
                                                        prop.className (
                                                            if isSelected then
                                                                "btn btn-sm rounded-none btn-primary"
                                                            else
                                                                "btn btn-sm rounded-none border-base-300 hover:border-primary hover:bg-primary hover:text-primary-content"
                                                        )
                                                        prop.text s
                                                        prop.onClick (fun _ -> dispatch (SelectSize s))
                                                    ]
                                            ]
                                        ]
                                    ]
                                ]

                                // Colors (derived)
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
                                                for (name, codeOpt) in colors do
                                                    let isSelected = props.SelectedColor = Some name
                                                    Html.button [
                                                        prop.key name
                                                        prop.title name
                                                        prop.className (
                                                            Common.Ui.tw [
                                                                "w-10 h-10 rounded-full border transition-all"
                                                                if isSelected then "border-primary ring-2 ring-primary ring-offset-2"
                                                                else "border-base-300 hover:ring-2 ring-offset-2 ring-primary"
                                                            ]
                                                        )
                                                        prop.style [
                                                            match codeOpt with
                                                            | Some hex -> style.backgroundColor hex
                                                            | None -> style.backgroundColor "#999999"
                                                        ]
                                                        prop.onClick (fun _ -> dispatch (SelectColor name))
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
                                            prop.onClick (fun _ -> dispatch PrimaryAction)
                                        ]
                                        Html.button [
                                            prop.className (Btn.outline [ "w-full rounded-none py-4" ])
                                            prop.text "Back"
                                            prop.onClick (fun _ -> dispatch GoBack)
                                        ]
                                    ]
                                ]

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
                                                    prop.text "Details coming soon."
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

        Section.container [ content ]
