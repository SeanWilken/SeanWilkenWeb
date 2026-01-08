namespace Client.Components.Shop

open Feliz
open Client.Components.Shop.Common.Ui
open Feliz.UseDeferred
open Elmish
open Shared.StoreProductViewer
open TSXUtilities

module Product =

    type Model =
        {
            Key                 : ProductKey
            Seed                : ProductSeed option
            ReturnTo            : ReturnTo
            // Details             : Deferred<GetDetailsResponse>
            Details             : Deferred<SyncProduct.SyncProductDetailsResponse>
            SelectedColor       : string option
            SelectedSize        : string option
            SelectedImage        : string option
            SelectedVariantId   : int64 option
        }

    type Msg =
        // | InitWith of key:ProductKey * seed:ProductSeed option * returnTo:ReturnTo
        | LoadDetails
        | GotDetails of SyncProduct.SyncProductDetailsResponse
        | FailedDetails of exn

        | SelectColor of string
        | SelectSize  of string
        | SelectVariant of int64
        | SelectImage of string

        // “primary CTA”
        | PrimaryAction

        // navigation hooks
        | GoBack

    let keyFromSeed = function
        | SeedCatalog p  -> Catalog p.id
        | SeedSync p  -> Sync p.Id
        | SeedTemplate t -> Template t.id

    let initModel (key: ProductKey) (seed: ProductSeed option) (returnTo: ReturnTo) : Model =
        {
            Key               = key
            Seed              = seed
            ReturnTo          = returnTo
            Details           = Deferred.HasNotStartedYet
            SelectedColor     = None
            SelectedSize      = None
            SelectedImage      = None
            SelectedVariantId = None }

    let detailsReq (m: Model) : Shared.Api.Printful.SyncProduct.GetSyncProductDetailsRequest =
        {
            selectedColor  = m.SelectedColor
            selectedSize   = m.SelectedSize
            syncProductId =
                match m.Key with
                | Catalog id -> id
                | Sync id -> int id
                | Template id -> id
        }

    let initFromSeed (seed: ProductSeed) (returnTo: ReturnTo) : Model * Cmd<Msg> =
        let model = {
            Key = keyFromSeed seed
            Seed = Some seed
            ReturnTo = returnTo
            Details = Deferred.HasNotStartedYet
            SelectedVariantId = None
            SelectedColor = None
            SelectedSize = None
            SelectedImage = None
        }
        model, Cmd.ofMsg LoadDetails

    let private distinctBy (f: 'a -> 'b) (xs: 'a list) =
        xs |> List.fold (fun acc x -> if acc |> List.exists (fun y -> f y = f x) then acc else acc @ [x]) []

    let private tryGetSelectedVariant (model: Model) (p: SyncProduct.SyncProduct) =

        let bySizeColor =
            match model.SelectedSize, model.SelectedColor with
            | Some s, Some c ->
                p.Variants
                |> List.filter (fun v -> 
                    match v.Availability with
                    | Some "active" -> true
                    | _ -> false
                )
                |> List.tryFind (fun v -> v.Size = Some s && v.Color = Some c)
            | None, Some c ->
                p.Variants
                |> List.filter (fun v -> 
                    match v.Availability with
                    | Some "active" -> true
                    | _ -> false
                )
                |> List.tryFind (fun v -> v.Color = Some c)
            | _ -> None

        let byId =
            model.SelectedVariantId
            |> Option.bind (fun id -> p.Variants |> List.tryFind (fun v -> v.Id = id))

        bySizeColor |> Option.orElse byId |> Option.orElse  (p.Variants |> List.tryHead)

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
            | Some "active" -> true
            | _ -> false
        )
        |> List.choose (fun v -> v.Size)
        |> List.distinct

    let private derivedColors (p: SyncProduct.SyncProduct) =
        p.Variants
        |> List.filter (fun v -> 
            match v.Availability with
            | Some "active" -> true
            | _ -> false
        )
        |> List.choose (fun v ->
            v.Color |> Option.map (fun name -> name, v.Color))
        |> distinctBy fst

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        // | InitWith (key, seed, returnTo) ->
        //     initModel key seed returnTo
        //     , Cmd.ofMsg LoadDetails

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
            let m1 = { model with SelectedColor = Some c }
            let updatedModel =
                match m1.Details  with
                | Deferred.Resolved d -> 
                    tryGetSelectedVariant m1 d.product
                    |> Option.map (fun v -> v.Id, v.PreviewUrl)
                    |> function 
                        | Some (variantId, imgUrl) -> 
                            { m1 with SelectedVariantId = Some variantId; SelectedImage = imgUrl }
                        | None -> m1
                | _ -> m1
            updatedModel, Cmd.none

        | SelectSize s ->
            let m1 = { model with SelectedSize = Some s }
            let updatedModel =
                match model.Details  with
                | Deferred.Resolved d -> 
                    tryGetSelectedVariant m1 d.product
                    |> Option.map (fun v -> v.Id)
                    |> fun variantId -> { m1 with SelectedVariantId = variantId }
                | _ -> m1
            updatedModel, Cmd.none
        
        | SelectImage imgUrl ->
            let m1 = { model with SelectedImage = Some imgUrl }
            m1, Cmd.none

        | SelectVariant vid ->
            let variantDetailsOpt =
                match model.Details with
                | Deferred.Resolved r ->
                    r.product.Variants
                    |> List.tryFind ( fun x -> x.Id = vid )
                    |> Option.map (fun x -> x.Size, x.Color )
                | _ -> None

            let m1 = 
                match variantDetailsOpt with
                | Some (size, color) -> size, color
                | None -> None, None
                |> fun (s, c) -> { model with SelectedSize = s; SelectedColor = c; SelectedVariantId = Some vid }
            m1, Cmd.none

        // handled by Shop-level wrapper (Add-to-cart or Select-for-designer)
        | PrimaryAction -> model, Cmd.none
        | GoBack -> model, Cmd.none

    [<ReactComponent>]
    let View (props: Model) dispatch =
        let content =
            match props.Details with
            | Deferred.HasNotStartedYet ->
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
                    match props.SelectedImage with
                    | Some img -> Some img
                    | None ->
                        match props.SelectedColor with
                        | None -> 
                            selectedVariantOpt
                            |> Option.bind (fun v -> v.PreviewUrl)
                            |> Option.orElse product.ThumbnailUrl
                        | Some clr ->
                            product.Variants
                            |> List.tryFind ( fun v -> v.Color = Some clr)
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
                                    |> List.filter ( fun x -> x.Availability = Some "active"  )
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
                                                    prop.onClick (fun _ -> dispatch (SelectImage url))
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
                                                    printfn $"SELECTED size: {props.SelectedSize}"
                                                    printfn $"Size: {s}"
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
                                                for (colorName, _) in colors do
                                                    let isSelected = props.SelectedColor = Some colorName
                                                    printfn $"COLOR NAME: {colorName}"
                                                    Html.button [
                                                        prop.key colorName
                                                        prop.title colorName
                                                        prop.className (
                                                            Common.Ui.tw [
                                                                "w-10 h-10 rounded-full border transition-all"
                                                                if isSelected then "border-primary ring-2 ring-primary ring-offset-2"
                                                                else "border-base-300 hover:ring-2 ring-offset-2 ring-primary"
                                                            ]
                                                        )
                                                        prop.style [
                                                            match resolvePrintfulColor colorName with
                                                            | [| single |] -> style.backgroundColor single
                                                            | colors ->
                                                                // build gradient
                                                                let stops =
                                                                    colors
                                                                    |> Array.mapi (fun i hex ->
                                                                        let start = (i * 100) / colors.Length
                                                                        let stop  = ((i + 1) * 100) / colors.Length
                                                                        $"{hex} {start}" + "%, " + $"{hex} {stop}" + "%"
                                                                    )
                                                                    |> String.concat ", "

                                                                style.backgroundImage $"linear-gradient(90deg, {stops})"
                                                        ]
                                                        prop.onClick (fun _ -> dispatch (SelectColor colorName))
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
