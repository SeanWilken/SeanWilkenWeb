namespace Shared


module SharedShopDomain =
        
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

module PrintfulCommon =
    /// Paging information
    type PagingInfoDTO = {
        total: int
        offset: int
        limit: int
    }

    let emptyPaging = {
        total = 0
        offset = 0
        limit = 20
    }

    /// Navigation links (HATEOAS from Printful)
    type LinksDTO = {
        self: string
        next: string option
        first: string option
        last: string option
    }

    type HateoasLink = {
        href : string
    }

// Refactor Shop Types
module SharedShopV2 =

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

    open PrintfulCatalog

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
            placement               : string
            display_name            : string
            technique_key           : string
            technique_display_name  : string
            options                 : string array
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
            paging : PrintfulCommon.PagingInfoDTO
        }

    module Cart =

        /// A single variant in the cart (one size/color of a catalog product)
        type CartItem = {
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

        type CartTotals = {
            Subtotal : decimal
            Tax      : decimal
            Shipping : decimal
            Total    : decimal
            Currency : string
        }

        type CartState = {
            Items  : CartItem list
            Totals : CartTotals
        }

        /// Minimal representation for shipping/tax calls
        type ShippingLineItem = {
            CatalogVariantId : int
            Quantity         : int
        }

    module Checkout =

        /// A richer recipient address than TaxAddress – matches Printful "recipient"
        type RecipientAddress = {
            Name         : string
            Email        : string option
            Phone        : string option
            Address1     : string
            Address2     : string option
            City         : string
            StateCode    : string
            CountryCode  : string  // ISO 3166-1 alpha-2 (e.g. "US")
            Zip          : string
        }

        /// Shipping option returned from Printful /v2/shipping-rates
        type ShippingRate = {
            Code               : string   // "STANDARD", "EXPRESS", etc. (shipping)
            Name               : string   // shipping_method_name
            Rate               : decimal  // numeric "rate"
            Currency           : string
            MinDeliveryDays    : int option
            MaxDeliveryDays    : int option
            CustomsFeesPossible: bool
        }

        /// Request to get shipping options
        type ShippingQuoteRequest = {
            Recipient : RecipientAddress
            Items     : Cart.ShippingLineItem list
            Currency  : string
        }

        type ShippingQuoteResponse = {
            Options : ShippingRate list
        }

        /// What the client must send when it's ready to place an order (ignoring card payment for now)
        type DraftOrderRequest = {
            Recipient     : RecipientAddress
            Items         : Cart.CartItem list
            SelectedRate  : ShippingRate
            /// Totals as calculated client-side for display / sanity check
            ClientTotals  : Cart.CartTotals
        }

module SharedShopV2Domain =

    module ProductTemplateResponse =
        type ProductTemplatesResponse = {
            templateItems : SharedShopV2.ProductTemplate.ProductTemplate list
            paging : PrintfulCommon.PagingInfoDTO
        }

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

        let mapPrintfulProduct (p: CatalogProduct.PrintfulProduct) : SharedShopV2.PrintfulCatalog.CatalogProduct =
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
            products: SharedShopV2.PrintfulCatalog.CatalogProduct list
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


module Api =

    open System.Threading.Tasks

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

            let toApiQuery (paging: PrintfulCommon.PagingInfoDTO) (stateFilters: SharedShopV2.PrintfulCatalog.Filters) : CatalogProductsQuery =
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

    type ProductApi = {
        getProducts : 
            Printful.CatalogProductRequest.CatalogProductsQuery -> 
                Async<SharedShopV2Domain.CatalogProductResponse.CatalogProductsResponse>
        getProductTemplates : 
            Printful.CatalogProductRequest.CatalogProductsQuery -> 
                Async<SharedShopV2Domain.ProductTemplateResponse.ProductTemplatesResponse>
        // getProductVariants : int -> Async<SharedShopDomain.CatalogVariant list>
    }

    type PaymentApi = {
        getTaxRate : TaxAddress -> Async<SharedShopDomain.CheckoutTax>
        getShipping : ShippingRequest -> Async<decimal>
        createPayPalOrder : decimal -> Async<string>   // returns order id
        capturePayPalOrder : string -> Async<bool>     // capture by order id
    }

    type CheckoutApi = {
        /// Get shipping methods + rates for the given cart & address
        getShippingRates : SharedShopV2.Checkout.ShippingQuoteRequest
                        -> Async<SharedShopV2.Checkout.ShippingQuoteResponse>

        /// (Optional) if you want a server-driven "canonical" tax calc later
        getTaxEstimate   : SharedShopV2.Checkout.ShippingQuoteRequest
                        -> Async<SharedShopDomain.CheckoutTax>

        /// Create a Printful draft order (no payment capture here)
        createDraftOrder : SharedShopV2.Checkout.DraftOrderRequest
                        -> Async<SharedShopDomain.DraftResult>
    }

// Ensure that the Client and Server use same end-point
module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName
