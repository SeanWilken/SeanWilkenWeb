namespace Shared

module StorePrintfulShippingApi =
        
    type QuantityAdjustment =
        | Increment
        | Decrement

    type DraftResult = { code: string }

    // Stub/placeholder types for API results
    // Replace with your real server shared types

    type CheckoutTax = { required: bool; rate: float; shipping_taxable: bool }

    type CheckoutShippingRate = { rate: float; name: string }

    type HttpError = string

    // ---- Internal types for Printful shipping-rates ----

    type ShippingRatesAddress = {
        // [<JsonPropertyName("address1")>]
        address1     : string option
        // [<JsonPropertyName("address2")>]
        address2     : string option
        // [<JsonPropertyName("city")>]
        city         : string option
        // [<JsonPropertyName("state_code")>]
        state_code   : string option
        // [<JsonPropertyName("country_code")>]
        country_code : string
        // [<JsonPropertyName("zip")>]
        zip          : string option
    }

    type CatalogShippingRateItem = {
        // [<JsonPropertyName("source")>]
        source            : string  // "catalog"
        // [<JsonPropertyName("quantity")>]
        quantity          : int
        // [<JsonPropertyName("catalog_variant_id")>]
        catalog_variant_id: int
    }

    type ShippingRatesInput = {
        // [<JsonPropertyName("recipient")>]
        recipient   : ShippingRatesAddress
        // [<JsonPropertyName("order_items")>]
        order_items : CatalogShippingRateItem array
        // [<JsonPropertyName("currency")>]
        currency    : string option
    }

    type ShippingRateDto = {
        // [<JsonPropertyName("shipping")>]
        shipping             : string
        // [<JsonPropertyName("shipping_method_name")>]
        shipping_method_name : string
        // [<JsonPropertyName("rate")>]
        rate                 : string
        // [<JsonPropertyName("currency")>]
        currency             : string
    }

    type ShippingRatesResponse = {
        // [<JsonPropertyName("data")>]
        data : ShippingRateDto array
    }
open StorePrintfulShippingApi

[<AutoOpen>]
module PrintfulCommon =

    /// Paging information
    type PagingInfoDTO = {
        total  : int
        offset : int
        limit  : int
    }

    let emptyPaging = {
        total  = 0
        offset = 0
        limit  = 20
    }

    /// Navigation links (HATEOAS from Printful)
    type LinksDTO = {
        self  : string
        next  : string option
        first : string option
        last  : string option
    }

    type HateoasLink = {
        href : string
    }

    /// Position of the image inside the print area (inches, per Printful docs)
    [<CLIMutable>]
    type PrintfulPosition = {
        Width  : float
        Height : float
        Left   : float
        Top    : float
    }

    /// Known layer option names we care about; fall back to Raw for future-proofing
    type PrintfulLayerOptionName =
        | ThreadColors          // "thread_colors"
        | YarnColors            // "yarn_colors"
        | CustomBorderColor     // "custom_border_color"
        | Raw of string         // any other option name

        member this.ToApiName() =
            match this with
            | ThreadColors      -> "thread_colors"
            | YarnColors        -> "yarn_colors"
            | CustomBorderColor -> "custom_border_color"
            | Raw n             -> n

    [<CLIMutable>]
    type PrintfulLayerOption = {
        Name   : PrintfulLayerOptionName
        Values : string list
    }

    /// Thin DTO used when actually building the JSON payload
    [<CLIMutable>]
    type PrintfulLayer = {
        Type         : string            // always "file" for now
        Url          : string
        LayerOptions : PrintfulLayerOption list
        Position     : PrintfulPosition option
    }

    /// One placement block in Printful's order item design data
    [<CLIMutable>]
    type PrintfulPlacement = {
        Placement : string              // e.g. "front", "back", "sleeve_left"
        Technique : string              // e.g. "dtg", "embroidery"
        Layers    : PrintfulLayer list
    }

    /// This is what you actually send as an item in the order
    type PrintfulOrderItem = {
        PreviewImage : string option
        catalog_variant_id : int
        source             : string
        quantity           : int
        placements         : PrintfulPlacement list
    }

    /// Logical “where” on the product that we support
    type DesignHitArea =
        | Front
        | Back
        | Pocket
        | LeftSleeve
        | RightSleeve
        | LeftLeg
        | RightLeg
        | LeftHalf
        | RightHalf
        | Center
        | OutsideLabel
        | Custom of string

        member this.ToPrintfulPlacement() =
            match this with
            | Front -> "front"
            | Back -> "back"
            | LeftSleeve -> "left_sleeve"
            | RightSleeve -> "right_sleeve"
            | Pocket -> "embroidery_pocket"          // “pocket” is typically front embroidery area in Printful UI
            | Center -> "default"
            | LeftLeg -> "left_leg"
            | RightLeg -> "right_leg"
            | LeftHalf -> "left_half"
            | RightHalf -> "right_half"
            | OutsideLabel -> "outside_label"
            | Custom name  -> name

    let designHitAreaFromPrintfulString str =
        match str with
        | "front"                  -> Front
        | "back"                   -> Back
        | "embroidery_pocket"      -> Pocket
        | "left_sleeve"            -> LeftSleeve
        | "right_sleeve"           -> RightSleeve 
        | "left_leg"               -> LeftLeg
        | "right_leg"              -> RightLeg
        | "left_half"              -> LeftHalf
        | "right_half"             -> RightHalf
        | "default"                -> Center 
        | "outside_label"          -> OutsideLabel 
        | name                     -> Custom name

    [<AutoOpen>]
    module PrintfulCatalog =
        /// Simplified type for displaying catalog products in your app
        type CatalogProduct = {
            id: int
            name: string
            thumbnailURL: string
            description: string option
            brand: string option
            model: string option
            variantCount: int
            isDiscontinued: bool
            sizes: string list
            colors: {| Color : string; ColorCodeOpt : string option |} list
        }

        type Filters = {
            Categories: int list
            Colors: string list
            Sizes: string list
            Placements: string list
            Techniques: string list
            OnlyNew: bool
            SellingRegion: string option
            DestinationCountry: string option
            SortDirection: string option
            SortType: string option
        }

        let defaultFilters : Filters = {
            Categories = []
            Colors = []
            Sizes = []
            Placements = []
            Techniques = []
            OnlyNew = false
            SellingRegion = None
            DestinationCountry = None
            SortDirection = None
            SortType = None
        }

    [<AutoOpen>]
    module ProductTemplate =

        // Fable-safe DTOs only
        type OptionData = {
            id    : string
            value : string array
        }

        type Color = {
            color_name  : string
            color_codes : string array
        }

        type PlacementOption = {
            Label   : string          // UI label
            HitArea : DesignHitArea
        }

        type PlacementOptionData = {
            ``type`` : string
            options  : string array
        }

        type ProductTemplate = {
            id                    : int
            product_id            : int
            external_product_id   : string option
            title                 : string
            available_variant_ids : int array
            option_data           : OptionData array
            colors                : Color array
            sizes                 : string array
            mockup_file_url       : string
            placements            : PlacementOption array
            created_at            : int64
            updated_at            : int64
            placement_option_data : PlacementOptionData array
            design_id             : string option
        }

        type ProductTemplatePaging = {
            total  : int
            limit  : int
            offset : int
        }

        type ProductTemplateResponse = {
            code   : int
            result : ProductTemplate list
            extra  : string array
            paging : PagingInfoDTO
        }

module Store =

    [<AutoOpen>]

    module Cart =

        /// A single variant in the cart (one size/color of a catalog product)
        type CartItem = {
            VariantId : int                      // catalog_variant_id
            Placements : PrintfulPlacement list  // normalized placements/layers
            PreviewImage : string option         // for UI purposes
            /// Printful catalog product id (v2 /catalog-products)
            CatalogProductId   : int
            /// Printful catalog variant id (from /catalog-products/{id}/catalog-variants)
            CatalogVariantId   : int
            Name               : string
            ThumbnailUrl       : string
            /// e.g. "M", "L", "XL"
            Size               : string
            /// e.g. "Black"
            ColorName          : string
            /// e.g. "#000000"
            ColorCodeOpt       : string option
            Quantity           : int
            /// Store price you charge the customer (retail)
            UnitPrice          : decimal
            /// Optional flag for your custom-design flow
            IsCustomDesign     : bool
        }

        // These aren't for shopping really
        type TemplateCartItem = {
            VariantId : int
            Quantity  : int
            TemplateId : int
            CatalogProductId : int
            Name             : string
            Size             : string
            ColorName        : string
            ColorCodeOpt     : string option
            UnitPrice        : decimal
            PreviewImage     : string option
        }

        type SyncCartItem = {
            // ProductId : int
            // VariantId : int
            SyncProductId    : int64
            SyncVariantId    : int64
            ExternalId : string option
            CatalogVariantId : int option
            Quantity         : int
            Name             : string
            ThumbnailUrl     : string
            Size             : string
            ColorName        : string
            ColorCodeOpt     : string option
            UnitPrice        : decimal
            Currency         : string
        }

        type CartLineItem =
            | Template of TemplateCartItem
            | Sync of SyncCartItem
            | Custom of CartItem

        type CartTotals = {
            Subtotal : decimal
            Tax      : decimal
            Shipping : decimal
            Total    : decimal
            Currency : string
        }

        type CartState = {
            Items  : CartLineItem list
            Totals : CartTotals
        }

        let emptyTotals = {
            Subtotal = 0m
            Tax      = 0m
            Shipping = 0m
            Total    = 0m
            Currency = "USD"
        }

        let emptyCart : CartState = {
            Items  = []
            Totals = emptyTotals
        }

        let private designUpchargeFromPlacements (placements: PrintfulPlacement list) : decimal =
            placements
            |> List.sumBy (fun p ->
                let perDesign =
                    match p.Placement with
                    | "outside_label" -> 5m
                    | _ -> 10m

                perDesign * decimal p.Layers.Length
            )

        let private lineTotal (item: CartLineItem) : decimal =
            match item with
            | CartLineItem.Custom c ->
                let upcharge = designUpchargeFromPlacements c.Placements
                (c.UnitPrice + upcharge) * decimal c.Quantity
            | CartLineItem.Template t ->
                t.UnitPrice * decimal t.Quantity
            | CartLineItem.Sync s ->
                s.UnitPrice * decimal s.Quantity

        let private recomputeTotals (items: CartLineItem list) : CartTotals =
            let subtotal =
                items
                |> List.sumBy lineTotal

            // let taxRate  = 0.08m
            // let tax      = System.Math.Round(subtotal * taxRate, 2)
            // let shipping =
            //     if subtotal = 0m then 0m
            //     elif subtotal >= 100m then 0m
            //     else 12m

            // let total = subtotal + tax + shipping

            {
                Subtotal = subtotal
                Tax      = 0m
                Shipping = 0m
                Total    = subtotal
                Currency = "USD"
            }

        let withItems (items: CartLineItem list) : CartState =
            {
                Items  = items
                Totals = recomputeTotals items
            }

        let withRemovedItem item cartState =
            let updatedItems = cartState.Items |> List.filter ( fun x -> x <> item )
            { cartState with
                Items = updatedItems
                Totals = recomputeTotals updatedItems
            }

        let withUpdatedItemQuantity item qty cartState =
            let updatedItems = 
                cartState.Items 
                |> List.tryFind (fun it -> it = item)
                |> Option.map (fun x -> 
                    match x with
                    | CartLineItem.Custom cci -> { cci with Quantity = qty } |> CartLineItem.Custom
                    | CartLineItem.Template template -> { template with Quantity = qty } |> CartLineItem.Template
                    | CartLineItem.Sync sync -> { sync with Quantity = qty } |> CartLineItem.Sync
                )
                |> function
                    | Some citem ->
                        cartState.Items 
                        |> List.filter ( fun x -> x <> item )
                        |> fun citems -> citems @ [ citem ] 
                    | None -> cartState.Items
            { cartState with
                Items = updatedItems
                Totals = recomputeTotals updatedItems
            }

        /// Minimal representation for shipping/tax calls
        type ShippingLineItem = {
            CatalogVariantId : int
            Quantity         : int
        }


    [<AutoOpen>]
    module Checkout =

    //     type CheckoutPreviewRequest = {
    //         Items : CartLineItem list
    //     }

    //     type LineValidationError = {
    //         Index   : int
    //         Message : string
    //     }

    //     /// This mirrors your CartTotals, but we return it explicitly so the UI
    //     /// can trust the server's numbers.
        type PreviewTotals = {
            ShippingName: string
            Subtotal : decimal
            Tax      : decimal
            Shipping : decimal
            Total    : decimal
            Currency : string
        }

    //     type CheckoutPreviewResponse = {
    //         Items   : CartLineItem list     // canonicalized (prices, names, etc)
    //         Totals  : PreviewTotals
    //         Errors  : LineValidationError list
    //         IsValid : bool
    //     }

    //     /// Step 2: shipping/tax quote

        type Address = {
            FirstName : string
            LastName  : string
            Email     : string
            Phone     : string option
            Line1     : string
            Line2     : string option
            City      : string
            State     : string
            Zip       : string
            Country   : string
        }

        type CheckoutQuoteRequest = {
            Items   : CartLineItem list
            Address : Address
        }

module ArtGalleryViewer =
    type ArtPiece = {
        Id: System.Guid
        DesignKey: string
        Title: string
        Description: string
        ImageUrl: string
        CdnUrls: string list
        Tags: string list
        LinkedSyncProductIds: int list
        CreatedAt: string
    }


open Store

module StoreProductViewer =

    module SyncProduct =
        /// A lightweight list item for Collection (fast)
        type SyncProductSummary = {
            Id : int64
            ExternalId    : string option
            Name          : string
            ThumbnailUrl  : string option
            VariantCount  : int
        }

        type SyncVariant = {
            Id : int64
            ExternalId    : string
            SyncProductId : int64
            VariantId     : int

            VariantProductId     : int
            VariantProductVariantId: int

            Name          : string option
            Size          : string option
            Color         : string option
            ImageUrl      : string option
            PreviewUrl    : string option
            RetailPrice   : decimal option
            Currency      : string option
            Availability  : string option
        }

        type SyncProduct = {
            SyncProductId : int
            ExternalId    : string option
            Name          : string
            ThumbnailUrl  : string option
            VariantCount  : int
            Variants      : SyncVariant list
        }

        type SyncProductDetailsResponse = {
            product : SyncProduct
        }

    type ProductKey =
        | Template of templateId:int
        | Sync of syncId:int64
        | Catalog  of catalogProductId:int

    // ---------------------------------------
    // “Seed” data so the page can render fast
    // (what you already have in lists)
    // ---------------------------------------
    type ProductSeed =
        | SeedTemplate of ProductTemplate
        | SeedSync of SyncProduct.SyncProductSummary
        | SeedCatalog  of CatalogProduct

    // ---------------------------------------
    // Where to go back to
    // ---------------------------------------
    type ReturnTo =
        | BackToCollection
        | BackToDesignerBaseSelect
    
    // ---------------------------------------
    // What the UI can do with the item
    // ---------------------------------------
    type ProductAction =
        | CanAddTemplateToCart
        | CanSelectCatalogForDesigner

    // ---------------------------------------
    // Details from server (shape you control)
    // ---------------------------------------
    type Money = { Amount: decimal; Currency: string }

    type CatalogVariantSummary =
        { VariantId : int
          Size      : string option
          Color     : string option
          Price     : Money option
          InStock   : bool option
          ImageUrl  : string option }

    type TemplateVariantSummary =
        { VariantId : int
          Size      : string option
          Color     : string option
          Price     : Money option
          InStock   : bool option
          ImageUrl  : string option }

    type CatalogDetails =
        { ProductId     : int
          Name          : string
          Description   : string option
          ThumbnailUrl  : string option
          Brand         : string option
          Model         : string option
          Sizes         : string list
          Colors        : {| Color : string; ColorCodeOpt : string option |} list
          Placements    : string list
          Techniques    : string list
          Variants      : CatalogVariantSummary list }

    type TemplateDetails =
        { TemplateId    : int
          Title         : string
          MockupUrl     : string option
          Description   : string option
          Sizes         : string list
          Colors        : Color list
          Placements    : PlacementOption list
          Techniques    : string list
          Variants      : TemplateVariantSummary list }

    type ProductDetails =
        | DetailsTemplate of TemplateDetails
        | DetailsCatalog  of CatalogDetails


    // ---------------------------------------
    // Request/Response payloads
    // ---------------------------------------
    type GetDetailsRequest =
        { Key               : ProductKey
          // optional selection hints to fetch pricing/image for “current selection”
          SelectedColorOpt  : string option
          SelectedSizeOpt   : string option
          SelectedVariantId : int option }

    type GetDetailsResponse =
        { Key     : ProductKey
          Details : ProductDetails }

// TODO: Still need above types until cart / checkout review and refactor
module ShopProductViewer =
    
    type ShopProductListItem = { 
        SyncProductId : int
        DesignKey  : string option
        Name          : string
        ThumbnailUrl  : string option

        PriceMin      : decimal option
        PriceMax      : decimal option
        Currency      : string option

        Colors        : string list
        Sizes         : string list

        BlankName     : string option
        BlankBrand    : string option
        BlankModel    : string option
        BlankImage    : string option 
    }

    type ShopVariant = { 
        SyncVariantId     : int64
        ExternalId        : string
        VariantId         : int

        CatalogProductId  : int
        CatalogVariantId  : int

        Name              : string
        Size              : string
        Color             : string
        Availability      : string option

        Currency          : string
        RetailPrice       : decimal option

        Sku               : string option
        ImageUrl          : string option
        PreviewUrl        : string option
        FileUrls          : string list 
    }

    type ShopCatalogColor = { 
        Name : string
        Value : string option
    }

    type ShopCatalogProduct = { 
        Id            : int
        Name          : string
        Brand         : string option
        Model         : string option
        Image         : string
        Description   : string
        Sizes         : string list
        Colors        : ShopCatalogColor list
        Techniques    : string list
        Placements    : string list
        ProductOptions: string list
    }

    type ShopProductDetails = {
        SyncProductId : int
        DesignKey  : string option
        Name          : string
        ThumbnailUrl  : string option
        Tags          : string list

        PriceMin      : decimal option
        PriceMax      : decimal option
        Currency      : string option

        Colors        : string list
        Sizes         : string list

        Variants      : ShopVariant list
        Blank         : ShopCatalogProduct option
    }

open StoreProductViewer

// This is the responses from the API
module PrintfulStoreDomain =

    module CatalogProductResponse =
        
        module CatalogProduct =

            open PrintfulCommon

            type ProductLinks = {
                self  : HateoasLink
                next  : HateoasLink option
                first : HateoasLink option
                last  : HateoasLink option
            }

            type Color = {
                name : string
                value : string option
                // image : string option
            }

            type Technique = {
                key : string
                display_name : string
            }

            type DesignPlacement = {
                placement : string
                display_name : string
            }

            type CatalogOption = {
                id : string
                title : string
                type' : string
            }

            type PrintfulProduct = {
                id              : int
                main_category_id: int
                ``type``        : string
                name            : string
                brand           : string option
                model           : string option
                image           : string
                variant_count   : int
                is_discontinued : bool
                description     : string
                sizes           : string array
                colors          : Color array
                techniques      : Technique array
                placements      : DesignPlacement array
                product_options : CatalogOption array
                _links          : ProductLinks
            }

            type PrintfulCatalogProductResponse = {
                data   : PrintfulProduct array
                paging : PrintfulCommon.PagingInfoDTO
                _links : ProductLinks
            }

        let mapPrintfulProduct (p: CatalogProduct.PrintfulProduct) : PrintfulCatalog.CatalogProduct =
            System.Console.WriteLine($"Mapping product: {p.name} with id {p.id}")
            p.colors |> Array.iter (fun (c: CatalogProduct.Color) -> 
                System.Console.WriteLine $"COLOR: {c.name} with code {c.value}"
            )

            { 
                id = p.id
                name = p.name
                thumbnailURL = p.image
                description = Some p.description
                brand = p.brand
                model = p.model
                variantCount = p.variant_count
                isDiscontinued = p.is_discontinued
                sizes = p.sizes |> List.ofArray
                colors = p.colors |> Array.map (fun c -> {| Color = c.name; ColorCodeOpt = c.value |} ) |> Array.toList
            }
        
        /// API response shaped for the client
        type CatalogProductsResponse = {
            products: PrintfulCatalog.CatalogProduct list
            paging: PrintfulCommon.PagingInfoDTO
            links: PrintfulCommon.LinksDTO
        }

        let mapPrintfulResponse (r: CatalogProduct.PrintfulCatalogProductResponse) : CatalogProductsResponse =
            { 
                products = 
                    r.data
                    |> Array.map mapPrintfulProduct
                    |> List.ofArray
                paging = r.paging
                links =
                    { 
                        self = r._links.self.href
                        next = r._links.next |> Option.map (fun l -> l.href)
                        first = r._links.first |> Option.map (fun l -> l.href)
                        last = r._links.last |> Option.map (fun l -> l.href)
                    } 
            }


// | ** API ** |
module Api =

    type TaxAddress = {
        Country : string
        State   : string
        City    : string
        Zip     : string
    }

    type ShippingRequest = {
        Address : TaxAddress
        Items   : string list
    }

    type Product = {
        Id : string
        Name : string
        Price : decimal
    }

    module Printful =

        module SyncProduct =
           
            type GetSyncProductsRequest = {
                limit  : int option
                offset : int option
            }

            type SyncProductsResponse = {
                items  : SyncProduct.SyncProductSummary list
                paging : PagingInfoDTO
            }


            type GetSyncProductDetailsRequest = {
                syncProductId : int64
                /// optional: if you want server to “preselect” the best variant/price/image later
                selectedColor : string option
                selectedSize  : string option
            }

        module CatalogProductRequest =

            type CatalogProductsQuery = {
                category_ids: int list option
                colors: string list option
                limit: int option
                newOnly: bool option
                offset: int option
                placements: string list option
                selling_region_name: string option
                sort_direction: string option
                sort_type: string option
                techniques: string list option
                destination_country: string option
            }

            let toApiQuery (paging: PrintfulCommon.PagingInfoDTO) (stateFilters: PrintfulCatalog.Filters) : CatalogProductsQuery =
                {
                    category_ids = if stateFilters.Categories |> List.isEmpty then None else Some stateFilters.Categories
                    colors = if stateFilters.Colors |> List.isEmpty then None else Some stateFilters.Colors
                    limit = Some paging.limit
                    offset = Some paging.offset
                    newOnly = Some stateFilters.OnlyNew
                    placements = if stateFilters.Placements |> List.isEmpty then None else Some stateFilters.Placements
                    selling_region_name = stateFilters.SellingRegion
                    sort_direction = stateFilters.SortDirection
                    sort_type = stateFilters.SortType
                    techniques = if stateFilters.Techniques |> List.isEmpty then None else Some stateFilters.Techniques
                    destination_country = stateFilters.DestinationCountry
                }

    module Checkout =
        open System
    
        type Address = {
            Name        : string option
            Email       : string
            Phone       : string option
            Line1       : string
            Line2       : string option
            City        : string
            State       : string
            PostalCode  : string
            CountryCode : string  // "US", "CA", etc.
        }

        // How client expresses what's in the cart.
        // We keep it "Printful-ready" instead of mirroring full CartLineItem.
        type CartItemKind =
            | Template
            | Sync
            | Custom

        type CheckoutCartItem = {
            Name           : string
            ThumbnailUrl   : string
            Kind           : CartItemKind
            Quantity       : int
            // Sync-based items (Printful "store/sync" world)
            ExternalProductId  : string option
            SyncProductId  : int64 option
            SyncVariantId  : int64 option   // keep as int64, don't truncate
            // Catalog/template based (if you still want to support later)
            CatalogProductId : int option
            CatalogVariantId : int option
            TemplateId       : int option
        }

        // ---------- 4.1 Preview ----------

        type CheckoutPreviewLine = {
            Item          : CheckoutCartItem
            UnitPrice     : decimal
            Currency      : string
            LineTotal     : decimal
            IsValid       : bool
            Error         : string option
        }

        type LineItem = {
            externalId : string
            productId : int64
            variantId : int
            quantity  : int
        }

        type ShippingAddress = {
            name        : string
            address1    : string
            city        : string
            state       : string
            countryCode : string
            postalCode  : string
            email : string
            phone :string option
        }

        type CreateDraftOrderRequest = {
            items           : LineItem list
            shippingOptionId: string
            totalsCents     : int
            address         : ShippingAddress
            isTemp : bool
        }

        type OrderTotals = {
            ShippingName : string
            Subtotal         : decimal
            Shipping         : decimal
            Tax              : decimal
            Total            : decimal
        }

        type CreateTempDraftOrderResponse = {
            PreviewLines : CheckoutPreviewLine list
            DraftOrderTotals: OrderTotals
        }

        type CreateFinalDraftOrderResponse = {
            OrderLines : CheckoutPreviewLine list
            OrderTotals: OrderTotals
            DraftOrderId : string
            StripeSecret : string
            StripePaymentIntentId : string
        }

        type CreateDraftOrderResponse =
            | CreatedTemp of CreateTempDraftOrderResponse
            | CreatedFinal of CreateFinalDraftOrderResponse

        type CustomerOrderDetails = {
            FirstName: string
            LastName: string
            Email: string
            Phone: string option
        }

        type ConfirmOrderRequest = {
            CustomerInfo : CustomerOrderDetails
            StripeConfirmation: string
            OrderDraftId     : string // should be able to find by this, but we'll send the email as well
            IsSuccess: bool
        }

        type OrderShipment = {
            Carrier: string
            Service: string
            TrackingNumber: string
            TrackingUrl: string
            ShipDate: string
            Items: int list // look them up in order items
        }

        type ConfirmOrderResponse = {
            OrderId : int
            InternalId : string
            Status: string
            ShippingServiceName: string
            Shipments: OrderShipment list
            OrderItems : CheckoutPreviewLine list
            Costs: OrderTotals
        }

        // ---------- 4.5 Order Lookup ----------

        type OrderStatus =
            | Pending          // created locally, not yet paid
            | Processing          // created locally, not yet paid
            | AwaitingPayment  // draft in Printful, waiting on Stripe
            | Paid             // payment succeeded, printful confirm pending
            | Refunded          // created locally, not yet paid
            | InProduction     // confirmed with Printful
            | Shipped
            | Cancelled
            | Other of string

            static member fromString (os: string) =
                match os.ToLowerInvariant() with
                | "draft-requested"
                | "draft-created"
                | "pending" -> Pending
                | "awaitingpayment" -> AwaitingPayment
                | "payment-succeeded" -> Processing
                | "paid" -> Paid
                | "inproduction" -> InProduction
                | "order-confirmed" 
                | "shipped" -> Shipped
                | "payment-failed"
                | "canceled" 
                | "cancelled" -> Cancelled
                | "refunded" -> Refunded
                | _ -> Other os

            static member toString =
                function
                | Pending -> "pending"
                | Processing -> "processing"
                | Refunded -> "refunded"
                | AwaitingPayment -> "awaitingpayment"
                | Paid -> "paid" 
                | InProduction -> "inproduction" 
                | Shipped -> "shipped" 
                | Cancelled -> "cancelled"
                | Other os -> os 

        type OrderSummary = {
            OrderId        : string
            Email          : string
            CreatedAt      : DateTime
            Status         : OrderStatus
            Total          : decimal
            Currency       : string
            PrintfulOrderId: int64 option
        }

        type OrderLookupRequest = {
            Email   : string
            OrderId : string option
        }

        type OrderLookupResponse = {
            Orders : OrderSummary list
        }
        
    open Printful.SyncProduct

    type PrintfulProductApi = {
        getProducts : Printful.CatalogProductRequest.CatalogProductsQuery -> Async<PrintfulStoreDomain.CatalogProductResponse.CatalogProductsResponse>
        getSyncProducts : GetSyncProductsRequest -> Async<SyncProductsResponse>
        getSyncProductVariantDetails : GetSyncProductDetailsRequest -> Async<SyncProduct.SyncProductDetailsResponse>
    }

    type PaymentApi = {
        getTaxRate : TaxAddress -> Async<CheckoutTax>
        getShipping : ShippingRequest -> Async<decimal>
        createPayPalOrder : decimal -> Async<string>   // returns order id
        capturePayPalOrder : string -> Async<bool>     // capture by order id
    }

    type CheckoutApi = {
        CreateDraftOrder : Checkout.CreateDraftOrderRequest -> Async<Checkout.CreateDraftOrderResponse>
        ConfirmOrder : Checkout.ConfirmOrderRequest -> Async<Checkout.ConfirmOrderResponse>
        LookupOrder    : Checkout.OrderLookupRequest     -> Async<Checkout.OrderLookupResponse>
    }

    type ArtGalleryApi = {
        GetGallery: unit -> Async<ArtGalleryViewer.ArtPiece list>
    }

    type ShopApi = { 
        GetProducts : unit -> Async<ShopProductViewer.ShopProductListItem list>
        GetProductDetails : int -> Async<ShopProductViewer.ShopProductDetails option> 
    }

// Ensure that the Client and Server use same end-point
module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName
