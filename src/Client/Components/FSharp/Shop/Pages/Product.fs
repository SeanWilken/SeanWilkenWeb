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
            ProductDetails             : Deferred<Shared.ShopProductViewer.ShopProductDetails>
            SelectedColor       : string option
            SelectedSize        : string option
            SelectedImage        : string option
            SelectedVariantId   : int64 option
        }

    type Msg =
        | LoadProductDetails of int
        | GotProductDetails of Shared.ShopProductViewer.ShopProductDetails option
        | FailedProductDetails of exn

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
    let idFromSeed = function
        | Catalog id  -> id 
        | Template id -> id
        | Sync id -> id |> int

    let initModel (key: ProductKey) (seed: ProductSeed option) (returnTo: ReturnTo) : Model =
        {
            Key               = key
            Seed              = seed
            ReturnTo          = returnTo
            ProductDetails           = Deferred.HasNotStartedYet
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
            ProductDetails = Deferred.HasNotStartedYet
            SelectedVariantId = None
            SelectedColor = None
            SelectedSize = None
            SelectedImage = None
        }
        model, Cmd.ofMsg (LoadProductDetails (idFromSeed model.Key))

    let private distinctBy (f: 'a -> 'b) (xs: 'a list) =
        xs |> List.fold (fun acc x -> if acc |> List.exists (fun y -> f y = f x) then acc else acc @ [x]) []

    let private tryGetSelectedProductVariant (model: Model) (p: Shared.ShopProductViewer.ShopProductDetails) =
        let bySizeColor =
            match model.SelectedSize, model.SelectedColor with
            | Some s, Some c ->
                p.Variants
                |> List.filter (fun v -> 
                    match v.Availability with
                    | Some "active" -> true
                    | _ -> false
                )
                |> List.tryFind (fun v -> v.Size = s && v.Color = c)
            | None, Some c ->
                p.Variants
                |> List.filter (fun v -> 
                    match v.Availability with
                    | Some "active" -> true
                    | _ -> false
                )
                |> List.tryFind (fun v -> v.Color = c)
            | _ -> None

        let byId =
            model.SelectedVariantId
            |> Option.bind (fun id -> p.Variants |> List.tryFind (fun v -> v.SyncVariantId = id))

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

    let private derivedProductSizes (p: Shared.ShopProductViewer.ShopProductDetails) =
        p.Variants
        |> List.filter (fun v -> 
            match v.Availability with
            | Some "active" -> true
            | _ -> false
        )
        |> List.choose (fun v -> Some v.Size)
        |> List.distinct

    let private derivedProductColors (p: Shared.ShopProductViewer.ShopProductDetails) =
        p.Variants
        |> List.filter (fun v -> 
            match v.Availability with
            | Some "active" -> true
            | _ -> false
        )
        |> List.choose (fun v -> Some v.Color )
        |> List.distinct

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with


        | LoadProductDetails pd_id ->
            let m1 = { model with ProductDetails = Deferred.InProgress }
            m1,
            // Cmd.none
            Cmd.OfAsync.either
                Client.Api.shopApi.GetProductDetails
                pd_id
                GotProductDetails
                FailedProductDetails

        | GotProductDetails res ->
            { model with 
                ProductDetails =  
                    match res with
                    | None -> Deferred.Failed (exn "Could not locate product!")
                    | Some det -> Deferred.Resolved det
            }, Cmd.none

        | FailedProductDetails ex ->
            { model with ProductDetails = Deferred.Failed ex }, Cmd.none

        | SelectColor c ->
            // optionally clear variant on change
            let m1 = { model with SelectedColor = Some c }
            let updatedModel =
                match m1.ProductDetails  with
                | Deferred.Resolved d -> 
                    tryGetSelectedProductVariant m1 d
                    |> Option.map (fun v -> v.SyncVariantId, v.PreviewUrl)
                    |> function 
                        | Some (variantId, imgUrl) -> 
                            { m1 with SelectedVariantId = Some variantId; SelectedImage = imgUrl }
                        | None -> m1
                | _ -> m1
            updatedModel, Cmd.none

        | SelectSize s ->
            let m1 = { model with SelectedSize = Some s }
            let updatedModel =
                match model.ProductDetails  with
                | Deferred.Resolved d -> 
                    tryGetSelectedProductVariant m1 d
                    |> Option.map (fun v -> v.SyncVariantId)
                    |> fun variantId -> { m1 with SelectedVariantId = variantId }
                | _ -> m1
            updatedModel, Cmd.none
        
        | SelectImage imgUrl ->
            let m1 = { model with SelectedImage = Some imgUrl }
            m1, Cmd.none

        | SelectVariant vid ->
            let variantDetailsOpt =
                match model.ProductDetails with
                | Deferred.Resolved r ->
                    r.Variants
                    |> List.tryFind ( fun x -> x.SyncVariantId = vid )
                    |> Option.map (fun x -> x.Size, x.Color )
                | _ -> None

            let m1 = 
                match variantDetailsOpt with
                | Some (size, color) -> Some size, Some color
                | None -> None, None
                |> fun (s, c) -> { model with SelectedSize = s; SelectedColor = c; SelectedVariantId = Some vid }
            m1, Cmd.none

        // handled by Shop-level wrapper (Add-to-cart or Select-for-designer)
        | PrimaryAction -> model, Cmd.none
        | GoBack -> model, Cmd.none

    [<ReactComponent>]
    let View (props: Model) dispatch =
        let content =
            // match props.Details with
            match props.ProductDetails with
            | Deferred.HasNotStartedYet
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
                // let product = details.product
                // let selectedVariantOpt = tryGetSelectedVariant props details

                let selectedVariantOpt = tryGetSelectedProductVariant props details

                let heroImg =
                    match props.SelectedImage with
                    | Some img -> Some img
                    | None ->
                        match props.SelectedColor with
                        | None -> 
                            selectedVariantOpt
                            |> Option.bind (fun v -> v.PreviewUrl)
                            |> Option.orElse details.ThumbnailUrl
                        | Some clr ->
                            details.Variants
                            |> List.tryFind ( fun v -> v.Color = clr)
                            |> Option.bind (fun v -> v.PreviewUrl)
                            |> Option.orElse details.ThumbnailUrl

                // if discontinued or no available variants, show message instead

                let priceText =
                    match selectedVariantOpt with
                    | Some v -> formatMoney v.RetailPrice details.Currency
                    | None ->
                        // fallback: min retail price across variants
                        let minPrice =
                            details.Variants
                            |> List.choose (fun v -> v.RetailPrice)
                            |> List.sort
                            |> List.tryHead
                        formatMoney minPrice (details.Variants |> List.tryPick (fun v -> Some v.Currency))

                // let sizes = derivedSizes details
                // let colors = derivedColors details
                let sizes = derivedProductSizes details
                let colors = derivedProductColors details

                Html.div [
                    prop.className "py-10 md:py-20 grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-16"
                    prop.children [

                        // =============================================================================
                        // Media
                        // =============================================================================
                        Html.div [
                            prop.className "space-y-4"
                            prop.children [
                                Html.div [
                                    prop.className "aspect-3/4 bg-base-200 overflow-hidden rounded-lg flex items-center justify-center"
                                    prop.children [
                                        match heroImg with
                                        | Some url ->
                                            Html.img [
                                                prop.src url
                                                prop.alt details.Name
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
                                    details.Variants
                                    |> List.filter (fun x -> x.Availability = Some "active")
                                    |> List.choose (fun v -> v.PreviewUrl |> Option.map (fun u -> v.VariantId, u))
                                    |> distinctBy snd
                                    |> List.truncate 4

                                if thumbs |> List.isEmpty then Html.none
                                else
                                    Html.div [
                                        prop.className "grid grid-cols-4 gap-3"
                                        prop.children [
                                            for (vid, url) in thumbs do
                                                let isSelected =
                                                    props.SelectedImage = Some url
                                                    || (props.SelectedImage.IsNone && heroImg = Some url)

                                                Html.button [
                                                    prop.key (string vid)
                                                    prop.onClick (fun _ -> dispatch (SelectImage url))
                                                    prop.className (
                                                        Common.Ui.tw [
                                                            "aspect-square bg-base-200 rounded-md overflow-hidden transition-all"
                                                            "focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
                                                            if isSelected then "ring-2 ring-primary"
                                                            else "hover:ring-2 ring-primary/60"
                                                        ]
                                                    )
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

                        // =============================================================================
                        // Details
                        // =============================================================================
                        Html.div [
                            prop.className "space-y-8 lg:sticky lg:top-28 h-fit"
                            prop.children [

                                // Header (bring in meta + stock like newer version, but keep your look)
                                Html.div [
                                    prop.className "space-y-3"
                                    prop.children [
                                        Html.p [
                                            prop.className "text-xs font-medium uppercase tracking-[0.25em] text-base-content/50"
                                            prop.text $"Product #{details.SyncProductId}"
                                        ]

                                        Html.h1 [
                                            prop.className "text-3xl md:text-4xl font-light tracking-tight text-base-content"
                                            prop.text details.Name
                                        ]

                                        Html.div [
                                            prop.className "flex items-baseline gap-3"
                                            prop.children [
                                                Html.p [
                                                    prop.className "text-3xl font-light"
                                                    prop.text priceText
                                                ]

                                                match selectedVariantOpt with
                                                | Some v when v.Availability = Some "active" ->
                                                    Html.span [
                                                        prop.className "text-xs uppercase tracking-wider font-medium text-green-600"
                                                        prop.text "In Stock"
                                                    ]
                                                | _ -> Html.none
                                            ]
                                        ]
                                    ]
                                ]
                                
                                // Sizes
                                if not sizes.IsEmpty then
                                    Html.div [
                                        prop.className "space-y-4"
                                        prop.children [
                                            Html.label [
                                                prop.className "text-xs uppercase tracking-[0.25em] text-base-content/60 font-medium"
                                                prop.text "Select Size"
                                            ]
                                            Html.div [
                                                // better wrapping behavior than a hard 4-col grid
                                                prop.className "grid grid-cols-4 gap-2 sm:grid-cols-6"
                                                prop.children [
                                                    for s in sizes do
                                                        let isSelected = props.SelectedSize = Some s
                                                        Html.button [
                                                            prop.key s
                                                            prop.onClick (fun _ -> dispatch (SelectSize s))
                                                            prop.className (
                                                                tw [
                                                                    "py-3 text-sm border transition-all tracking-wider"
                                                                    "rounded-none"
                                                                    if isSelected then
                                                                        "border-base-content bg-base-content text-base-100"
                                                                    else
                                                                        "border-base-content/20 hover:border-base-content/40"
                                                                ]
                                                            )
                                                            prop.text s
                                                        ]
                                                ]
                                            ]
                                        ]
                                    ]

                                // Colors
                                if not colors.IsEmpty then
                                    Html.div [
                                        prop.className "space-y-4"
                                        prop.children [
                                            Html.div [
                                                prop.className "flex items-baseline justify-between"
                                                prop.children [
                                                    Html.label [
                                                        prop.className "text-xs uppercase tracking-[0.25em] text-base-content/60 font-medium"
                                                        prop.text "Select Color"
                                                    ]
                                                    match props.SelectedColor with
                                                    | Some c ->
                                                        Html.span [
                                                            prop.className "text-xs text-base-content/50"
                                                            prop.text c
                                                        ]
                                                    | None -> Html.none
                                                ]
                                            ]
                                            Html.div [
                                                prop.className "flex flex-wrap gap-3"
                                                prop.children [
                                                    for colorName in colors do
                                                        let isSelected = props.SelectedColor = Some colorName
                                                        Html.button [
                                                            prop.key colorName
                                                            prop.title colorName
                                                            prop.onClick (fun _ -> dispatch (SelectColor colorName))
                                                            prop.className (
                                                                tw [
                                                                    "w-11 h-11 rounded-full transition-all"
                                                                    "ring-offset-4 ring-offset-base-100"
                                                                    if isSelected then
                                                                        "ring-2 ring-base-content"
                                                                    else
                                                                        "ring-1 ring-base-content/15 hover:ring-2 hover:ring-base-content/30"
                                                                ]
                                                            )
                                                            prop.style [
                                                                match resolvePrintfulColor colorName with
                                                                | [| single |] -> style.backgroundColor single
                                                                | colors ->
                                                                    let stops =
                                                                        colors
                                                                        |> Array.mapi (fun i hex ->
                                                                            let start = (i * 100) / colors.Length
                                                                            let stop  = ((i + 1) * 100) / colors.Length
                                                                            $"{hex} {start}%%, {hex} {stop}%%"
                                                                        )
                                                                        |> String.concat ", "
                                                                    style.backgroundImage $"linear-gradient(90deg, {stops})"
                                                            ]
                                                        ]
                                                ]
                                            ]
                                        ]
                                    ]

                                // Actions (bring in disable logic from newer version)
                                let canAddToCart =
                                    props.SelectedSize.IsSome && props.SelectedColor.IsSome

                                Html.div [
                                    prop.className "space-y-3 pt-4"
                                    prop.children [
                                        Html.button [
                                            prop.className (
                                                Common.Ui.tw [
                                                    Btn.primary [ "w-full rounded-none py-4" ]
                                                    if not canAddToCart then "opacity-40 cursor-not-allowed"
                                                ]
                                            )
                                            prop.text "Add to Cart"
                                            prop.onClick (fun _ -> if canAddToCart then dispatch PrimaryAction)
                                            prop.disabled (not canAddToCart)
                                        ]
                                        Html.button [
                                            prop.className (Btn.outline [ "w-full rounded-none py-4" ])
                                            prop.text "Back"
                                            prop.onClick (fun _ -> dispatch GoBack)
                                        ]
                                    ]
                                ]

                                // Collapsibles (use your DaisyUI collapse, but upgrade content like newer version)
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
                                                    prop.className "collapse-content text-sm text-base-content/70 leading-relaxed space-y-3"
                                                    prop.children [
                                                        match details.Blank with
                                                        | Some blank ->
                                                            Html.p [ prop.text $"Made with premium {blank.Name}." ]
                                                            if not (SharedViewModule.Helpers.iNoWS blank.Description) then
                                                                Html.p [ prop.text blank.Description ]
                                                        | None ->
                                                            Html.p [ prop.text "Premium quality garment with professional printing." ]
                                                    ]
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
                                                    prop.className "collapse-content text-sm text-base-content/70 leading-relaxed space-y-3"
                                                    prop.children [
                                                        Html.p [ prop.text "Free shipping on orders over $100." ]
                                                        Html.p [ prop.text "30-day return policy. Items must be unworn and in original condition." ]
                                                        Html.p [ prop.text "Processing time: 2–7 business days." ]
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

        Section.container [ content ]
