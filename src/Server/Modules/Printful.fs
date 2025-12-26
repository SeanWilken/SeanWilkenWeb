module PrintfulService

open Shared
open Shared.Api
open System.Net.Http
open System.Text.Json
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open System
open System.Net.Http.Headers
open Giraffe
open System.Text
open FSharp.Control
open Shared.Api.Checkout
open Shared.Api.Printful.SyncProduct
open StripeService
open EnvService
open MongoService
open System.Text.Json.Serialization

let storeHeaders =
    [ 
        "X-PF-Store-ID", "6302847"
    ]
    |> Map.ofList

let jsonOptions =
    JsonSerializerOptions(
        PropertyNameCaseInsensitive = true
    )

module PrintfulClient =

    let printfulHttpClient (version: string option) =
        new HttpClient(BaseAddress = 
            Uri (
                match version with
                | Some "v2" -> "https://api.printful.com/v2/"
                | _ -> "https://api.printful.com/"
            )
        )

    // this is what you already have, just parameterize the API key
    let configureClient (versionOpt: string option) (apiKey:string) (headers: Map<string, string>) =
        let client = printfulHttpClient versionOpt
        if not (client.DefaultRequestHeaders.Contains("X-PF-Store-ID")) then
            headers |> Map.iter (fun k v ->
                if not (client.DefaultRequestHeaders.Contains k) then
                    client.DefaultRequestHeaders.Add(k, v)
            )
        if isNull client.DefaultRequestHeaders.Authorization then
            client.DefaultRequestHeaders.Authorization <-
                AuthenticationHeaderValue("Bearer", apiKey)

        client

    let ensureSuccessOrFail (resp: HttpResponseMessage) (body: string) (context: string) =
        if not resp.IsSuccessStatusCode then
            // Printful often returns { code, result, error } even on failures
            failwith $"{context} failed ({int resp.StatusCode} {resp.ReasonPhrase}). Body: {body}"

        
    let qsParams (pairs: (string * string) list) =
        pairs
        |> List.map (fun (k,v) -> $"{k}={System.Uri.EscapeDataString(v)}")
        |> String.concat "&"
        |> fun s -> if System.String.IsNullOrWhiteSpace s then "" else "?" + s

    // review if multipurpose or not
    let productQueryString (q: Printful.CatalogProductRequest.CatalogProductsQuery) =
        [
            q.category_ids |> Option.map (fun ids -> "category_ids", String.concat "," (ids |> List.map string))
            q.colors |> Option.map (fun xs -> "colors", String.concat "," xs)
            q.limit |> Option.map (fun v -> "limit", string v)
            q.newOnly |> Option.map (fun v -> "new", if v then "true" else "false")
            q.offset |> Option.map (fun v -> "offset", string v)
            q.placements |> Option.map (fun xs -> "placements", String.concat "," xs)
            // q.selling_region_name |> Option.map (fun v -> "selling_region_name", v)
            // q.sort_direction |> Option.map (fun v -> "sort_direction", v)
            // q.sort_type |> Option.map (fun v -> "sort_type", v)
            q.techniques |> Option.map (fun xs -> "techniques", String.concat "," xs)
            q.destination_country |> Option.map (fun v -> "destination_country", v)
        ]
        |> List.choose id
        |> List.map (fun (k,v) -> $"{k}={System.Uri.EscapeDataString v}")
        |> String.concat "&"
        |> fun s -> 
            printfn $"Query String: {s}"
            if s = "" then "" else "?" + s


module Types =

    module Common = 

        // Server-only: raw types with JsonElement
        type RawOptionData = {
            id    : string
            value : JsonElement
        }

        type RawColor = {
            color_name  : string
            color_codes : string array
        }

        type RawPlacementOption = {
            placement               : string
            display_name            : string
            technique_key           : string
            technique_display_name  : string
            options                 : JsonElement array
        }

        type RawPlacementOptionData = {
            ``type`` : string
            options  : JsonElement array
        }

    // Printful Product Catalog (Base Items)
    module CatalogProduct =

        type RawStoreVariant = {
            id            : int
            variant_id    : int option
            size          : string option
            color         : string option
            color_name    : string option
            color_code    : string option
            color_hex     : string option
            retail_price  : decimal option
            price         : decimal option
            currency      : string option
            currency_code : string option
        }

        type RawStoreVariantResponse = {
            code   : int
            result : RawStoreVariant
        }

    // Store Products imported from Printful
    module Sync =

        module SyncProductSummary =

            type RawSyncProductSummary = {
                id          : int
                external_id : string option
                name        : string
                thumbnail_url : string option
                synced      : int
                variants    : int
                is_ignored : bool
            }

            type RawSyncProductResponse = {
                code : int
                result  : RawSyncProductSummary array
                paging : PagingInfoDTO
                extra    : string array

            }
            
            type RawSyncProductDetails = {
                product_id : int
                variant_id : int
                external_id   : string option
                name          : string
                thumbnail_url : string option
            }

            type RawSyncVariant = {
                id            : int
                sync_product_id : int option
                name          : string option
                variant_id : int option
                size          : string option
                color         : string option
                color_code    : string option
                image         : string option
                retail_price  : string option   // Printful sometimes sends prices as string
                currency      : string option
            }

            type RawSyncProductDetailsResult = {
                sync_product : RawSyncProductDetails
                sync_variants: RawSyncVariant array
            }


            type RawSyncProductDetailsResponse = {
                code   : int
                result : RawSyncProductDetailsResult
            }

            module Mapping =
                
                let parsePrice (s: string option) =
                    match s with
                    | None -> None
                    | Some t ->
                        match System.Decimal.TryParse(t) with
                        | true, v -> Some v
                        | _ -> None

                // -------------------------
                // MAPPERS (Raw -> Shared)
                // -------------------------

                let mapSummary (r: RawSyncProductSummary) : StoreProductViewer.SyncProduct.SyncProductSummary =
                    {
                        Id = r.id
                        ExternalId    = r.external_id
                        Name          = r.name
                        ThumbnailUrl  = r.thumbnail_url
                        VariantCount  = r.variants
                    }


        module SyncProductVariant =



            [<CLIMutable>]
            type RawSyncProduct = {
                [<JsonPropertyName("id")>]
                id : int
                [<JsonPropertyName("external_id")>]
                external_id : string
                [<JsonPropertyName("name")>]
                name : string
                [<JsonPropertyName("variants")>]
                variants : int
                [<JsonPropertyName("thumbnail_url")>]
                thumbnail_url : string
                [<JsonPropertyName("synced")>]
                synced : int
                [<JsonPropertyName("is_ignored")>]
                is_ignored : bool
            }

            [<CLIMutable>]
            type RawVariantProductInfo = {
                [<JsonPropertyName("variant_id")>]
                variant_id : int
                [<JsonPropertyName("product_id")>]
                product_id : int
                [<JsonPropertyName("image")>]
                image : string
                [<JsonPropertyName("name")>]
                name : string
            }

            [<CLIMutable>]
            type RawSyncFile = {
                [<JsonPropertyName("type")>]
                ``type`` : string
                [<JsonPropertyName("preview_url")>]
                preview_url : string
            }

            [<CLIMutable>]
            type RawSyncOption = {
                [<JsonPropertyName("id")>]
                id : string
                [<JsonPropertyName("value")>]
                value : string
            }

            [<CLIMutable>]
            type RawSyncVariant = {
                [<JsonPropertyName("id")>]
                id : int64
                [<JsonPropertyName("external_id")>]
                external_id : string
                [<JsonPropertyName("sync_product_id")>]
                sync_product_id : int64
                [<JsonPropertyName("name")>]
                name : string
                [<JsonPropertyName("synced")>]
                synced : bool
                [<JsonPropertyName("variant_id")>]
                variant_id : int
                [<JsonPropertyName("retail_price")>]
                retail_price : string
                [<JsonPropertyName("currency")>]
                currency : string
                [<JsonPropertyName("is_ignored")>]
                is_ignored : bool
                [<JsonPropertyName("sku")>]
                sku : string option
                [<JsonPropertyName("product")>]
                product : RawVariantProductInfo
                [<JsonPropertyName("files")>]
                files : RawSyncFile array
                // [<JsonPropertyName("options")>]
                // options : RawSyncOption array
                [<JsonPropertyName("main_category_id")>]
                main_category_id : int option
                [<JsonPropertyName("warehouse_product_id")>]
                warehouse_product_id : int option
                [<JsonPropertyName("warehouse_product_variant_id")>]
                warehouse_product_variant_id : int64 option
                [<JsonPropertyName("size")>]
                size : string
                [<JsonPropertyName("color")>]
                color : string
                [<JsonPropertyName("availability_status")>]
                availability_status : string
            }

            [<CLIMutable>]
            type RawSyncProductDetailsResult = {
                [<JsonPropertyName("sync_product")>]
                sync_product : RawSyncProduct
                [<JsonPropertyName("sync_variants")>]
                sync_variants : RawSyncVariant array
            }

            [<CLIMutable>]
            type RawSyncProductDetailsResponse = {
                [<JsonPropertyName("code")>]
                code : int
                [<JsonPropertyName("result")>]
                result : RawSyncProductDetailsResult
                [<JsonPropertyName("extra")>]
                extra : JsonElement array
            }

            [<CLIMutable>]
            type RawSingleSyncProductDetailsResponse = {
                [<JsonPropertyName("code")>]
                code : int
                [<JsonPropertyName("result")>]
                result : RawSyncVariant
            }

            open Shared.StoreProductViewer.SyncProduct



            module Mapping =

                let private tryParseDecimal (s: string) : decimal option =
                    match Decimal.TryParse(s, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture) with
                    | true, d -> Some d
                    | _ -> None

                let private pickPreviewUrl (files: RawSyncFile array) : string option =
                    files
                    |> Array.tryFind (fun f -> String.Equals(f.``type``, "preview", StringComparison.OrdinalIgnoreCase))
                    |> Option.map (fun f -> f.preview_url)
                    |> Option.orElse (
                        files
                        |> Array.tryHead
                        |> Option.map (fun f -> f.preview_url)
                    )
                    |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))

                let toDetailsResponse (raw: RawSyncProductDetailsResponse) : SyncProductDetailsResponse =
                    let sp = raw.result.sync_product
                    let variants =
                        raw.result.sync_variants
                        |> Array.toList
                        |> List.map (fun variant ->
                            {
                                Id    = variant.id
                                SyncProductId    = variant.sync_product_id
                                VariantId = variant.variant_id
                                ExternalId = variant.external_id // think this is what we want
                                VariantProductId = variant.product.product_id
                                VariantProductVariantId = variant.product.variant_id
                                Name             = if String.IsNullOrWhiteSpace variant.name then None else Some variant.name
                                Size             = if String.IsNullOrWhiteSpace variant.size then None else Some variant.size
                                Color            = if String.IsNullOrWhiteSpace variant.color then None else Some variant.color
                                ImageUrl         = if isNull variant.product.image then None else Some variant.product.image |> Option.filter (fun x -> not (String.IsNullOrWhiteSpace x))
                                PreviewUrl       = pickPreviewUrl variant.files
                                RetailPrice      = tryParseDecimal variant.retail_price
                                Currency         = if String.IsNullOrWhiteSpace variant.currency then None else Some variant.currency
                                Availability     = if String.IsNullOrWhiteSpace variant.availability_status then None else Some variant.availability_status
                            }
                        )

                    System.Console.WriteLine $"EXTERNAL ID: {sp.external_id}"

                    {
                        product =
                            {
                                SyncProductId = sp.id
                                ExternalId    = if String.IsNullOrWhiteSpace sp.external_id then None else Some sp.external_id
                                Name          = sp.name
                                ThumbnailUrl  = if String.IsNullOrWhiteSpace sp.thumbnail_url then None else Some sp.thumbnail_url
                                VariantCount  = sp.variants
                                Variants      = variants
                            }
                    }

        module Order =

            [<CLIMutable>]    
            type OrderCosts = { 
                [<JsonPropertyName("currency")>]
                currency : string
                [<JsonPropertyName("subtotal")>]
                subtotal : decimal 
                [<JsonPropertyName("discount")>]
                discount : decimal
                [<JsonPropertyName("shipping")>]
                shipping : decimal
                [<JsonPropertyName("digitization")>]
                digitization : decimal
                [<JsonPropertyName("additional_fee")>]
                additional_fee : decimal
                [<JsonPropertyName("fulfillment_fee")>]
                fulfillment_fee : decimal
                [<JsonPropertyName("retail_delivery_fee")>]
                retail_delivery_fee : string
                [<JsonPropertyName("tax")>]
                tax : string
                [<JsonPropertyName("vat")>]
                vat : decimal
                [<JsonPropertyName("total")>]
                total : decimal
            }

            [<CLIMutable>]    
            type OrderRetailCosts = { 
                [<JsonPropertyName("currency")>]
                currency : string
                [<JsonPropertyName("subtotal")>]
                subtotal : decimal 
                [<JsonPropertyName("discount")>]
                discount : decimal
                [<JsonPropertyName("shipping")>]
                shipping : decimal
                [<JsonPropertyName("tax")>]
                tax : decimal option
                [<JsonPropertyName("vat")>]
                vat : decimal option
                [<JsonPropertyName("total")>]
                total : decimal
            }

            [<CLIMutable>]    
            type OrderItem = { 
                [<JsonPropertyName("id")>]
                id : int // line item id
                [<JsonPropertyName("external_id")>]
                external_id : string  // line item id from internal
                [<JsonPropertyName("variant_id")>]
                variant_id : int64
                [<JsonPropertyName("sync_variant_id")>]
                sync_variant_id : int64
                [<JsonPropertyName("external_variant_id")>]
                external_variant_id : string  // line item id from internal
                [<JsonPropertyName("product_template_id")>]
                product_template_id : int
                [<JsonPropertyName("quantity")>]
                quantity : int
                [<JsonPropertyName("price")>]
                price : decimal
                [<JsonPropertyName("retail_price")>]
                retail_price : string
                [<JsonPropertyName("name")>]
                name : string
                [<JsonPropertyName("product")>]
                product : SyncProductVariant.RawVariantProductInfo
                [<JsonPropertyName("discontinued")>]
                discontinued : bool
                [<JsonPropertyName("out_of_stock")>]
                out_of_stock : bool

            }

            [<CLIMutable>]    
            type OrderResult = { 
                [<JsonPropertyName("id")>]
                id : int
                [<JsonPropertyName("external_id")>]
                external_id : string 
                [<JsonPropertyName("status")>]
                status : string
                [<JsonPropertyName("shipping")>]
                shipping : string
                [<JsonPropertyName("shipping_service_name")>]
                shipping_service_name : string
                [<JsonPropertyName("items")>]
                items : OrderItem array
                [<JsonPropertyName("costs")>]
                costs : OrderCosts
                [<JsonPropertyName("retail_costs")>]
                retail_costs : OrderRetailCosts
            }

            [<CLIMutable>]    
            type OrderResponse = { 
                [<JsonPropertyName("code")>]
                code : int 
                [<JsonPropertyName("result")>]
                result : OrderResult
            }



    // module Checkout =
        
        // module Shipping =

        //     type Recipient = {
        //         phone        : string option
        //         address1     : string
        //         address2     : string option
        //         city         : string
        //         state_code   : string
        //         country_code : string
        //         zip          : string
        //     }

        //     type Item = {
        //         variant_id      : string option
        //         external_variant_id: string option
        //         warehouse_product_variant_id: string option
        //         quantity        : int
        //         value : string option
        //     }

        //     type RatesRequest = {
        //         recipient : Recipient
        //         items     : Item list
        //         currency : string
        //         locale : string
        //     }

        //     open System.Text.Json.Serialization

        //     [<CLIMutable>]       
        //     type Rate = {
        //         [<JsonPropertyName("id")>]
        //         id                 : string
        //         [<JsonPropertyName("name")>]
        //         name               : string
        //         [<JsonPropertyName("rate")>]
        //         rate               : string
        //         [<JsonPropertyName("currency")>]
        //         currency           : string
        //         [<JsonPropertyName("minDeliveryDays")>]
        //         minDeliveryDays  : int
        //         [<JsonPropertyName("maxDeliveryDays")>]
        //         maxDeliveryDays  : int
        //     }

        //     [<CLIMutable>]    
        //     type RatesResponse = {
        //         [<JsonPropertyName("code")>]
        //         code : int
        //         [<JsonPropertyName("result")>]
        //         result : Rate array
        //         [<JsonPropertyName("extra")>]
        //         extra : obj array
        //     }

  
        //     module Mapping =
        //         let toPrintfulRatesRequest (req: CheckoutQuoteRequest) : RatesRequest =
        //             let r = req.ShippingAddress

        //             let recipient : Recipient = {
        //                 phone        = r.Phone
        //                 address1     = r.Line1
        //                 address2     = r.Line2
        //                 city         = r.City
        //                 state_code   = r.State
        //                 country_code = r.CountryCode
        //                 zip          = r.PostalCode
        //             }

        //             let items =
        //                 req.Items
        //                 |> List.map (
        //                     fun ci ->
        //                         { 
        //                             variant_id      =
        //                                 match ci.CatalogVariantId with
        //                                 | None -> None
        //                                 | Some cvid -> string cvid |> Some
        //                             external_variant_id      =
        //                                 match ci.CatalogProductId with
        //                                 | None -> None
        //                                 | Some spid -> string spid |> Some
        //                             warehouse_product_variant_id      = None
        //                             quantity        = ci.Quantity
        //                             value = None
        //                         }
        //                 )

        //             { recipient = recipient; items = items ; currency = "usd"; locale = "en_US" }

// open Types.Checkout.Shipping


module PrintfulApi =

    module SyncProduct =

        open Types.Sync.SyncProductSummary
        open Shared.Api.Printful.SyncProduct
        open Types.Sync.SyncProductVariant
        open Types.Sync.SyncProductVariant.Mapping
        open Types.Sync.SyncProductSummary.Mapping
        open Shared.StoreProductViewer.SyncProduct

        let fetchSyncProducts
            (req    : GetSyncProductsRequest)
            : Async<SyncProductsResponse> =
            async {

                let printfulClient = PrintfulClient.configureClient None EnvConfig.printfulKey storeHeaders

                let url =
                    "store/products" +
                    PrintfulClient.qsParams [
                        match req.limit with  Some v -> ("limit", string v)  | None -> ()
                        match req.offset with Some v -> ("offset", string v) | None -> ()
                    ]

                Console.WriteLine $"[Printful][SyncProduct] BaseAddress={printfulClient.BaseAddress} + URL={url}"

                let! resp = printfulClient.GetAsync(url) |> Async.AwaitTask
                let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
                System.Console.WriteLine $"[Printful][SyncProducts] RESP BODY {body}"
                
                PrintfulClient.ensureSuccessOrFail resp body "[Printful][SyncProducts]"

                try
                    System.Console.WriteLine $"[Printful][SyncProducts] DESERIALIZE"
                    let raw = JsonSerializer.Deserialize<RawSyncProductResponse>(body, jsonOptions)
                    System.Console.WriteLine $"[Printful][SyncProducts] RAW {raw.code} items={raw.result.Length}"

                    return {
                        items  = raw.result |> Array.toList |> List.map mapSummary
                        paging = { total = raw.paging.total; offset = raw.paging.offset; limit = raw.paging.limit }
                    }
                with e ->
                    System.Console.WriteLine $"[Printful][SyncProducts] DESERIALIZE ERROR: {e.Message}"
                    return failwith $"[Printful][SyncProducts] DESERIALIZE ERROR: {e.Message}"
            } 


        /// “Single product details” includes variants in the response (best for Product page)
        let fetchSyncProductDetails
            // (http   : HttpClient)
            (syncProductId : Printful.SyncProduct.GetSyncProductDetailsRequest)
            : Async<SyncProductDetailsResponse> =
            async {
                let printfulClient = PrintfulClient.configureClient None EnvConfig.printfulKey storeHeaders

                let url = $"store/products/{syncProductId.syncProductId}"
                System.Console.WriteLine $"[Printful][SyncProductDetails] FETCH DETAILS"
                let! resp = printfulClient.GetAsync(url) |> Async.AwaitTask
                System.Console.WriteLine $"[Printful][SyncProductDetails] GOT RESPONSE"
                let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask

                PrintfulClient.ensureSuccessOrFail resp body "[Printful][SyncProductDetails]"
                
                try

                    System.Console.WriteLine $"[Printful][SyncProductDetails] DESERIALIZE {body}"
                    let raw = JsonSerializer.Deserialize<RawSyncProductDetailsResponse>(body, jsonOptions)
                    System.Console.WriteLine $"[Printful][SyncProductDetails] RAW: {raw}"
                    return Mapping.toDetailsResponse raw
                with e ->
                    System.Console.WriteLine $"[Printful][SyncProductDetails] DESERIALIZE ERROR: {e.Message}"
                    return failwith $"[Printful][SyncProductDetails] DESERIALIZE ERROR: {e.Message}"
            }

        let fetchSyncProductVariantDetails
            (externalSyncVariantId : string)
            : Async<RawSingleSyncProductDetailsResponse option> =
            async {
                let printfulClient = PrintfulClient.configureClient None EnvConfig.printfulKey storeHeaders

                let url = $"store/variants/@{externalSyncVariantId}"
                System.Console.WriteLine $"[Printful][SyncSingleProductDetails] FETCH DETAILS"
                let! resp = printfulClient.GetAsync(url) |> Async.AwaitTask
                System.Console.WriteLine $"[Printful][SyncSingleProductDetails] GOT RESPONSE"
                let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask

                PrintfulClient.ensureSuccessOrFail resp body "[Printful][SyncSingleProductDetails]"
                
                try
                    System.Console.WriteLine $"[Printful][SyncSingleProductDetails] DESERIALIZE {body}"
                    let raw = JsonSerializer.Deserialize<RawSingleSyncProductDetailsResponse>(body, jsonOptions)
                    System.Console.WriteLine $"[Printful][SyncSingleProductDetails] RAW: {raw}"
                    return Some raw
                with e ->
                    System.Console.WriteLine $"[Printful][SyncSingleProductDetails] DESERIALIZE ERROR: {e.Message}"
                    return None
            }
    open SyncProduct


    module CatalogProduct = 

        type RawSingleCatalogProductResponse = {
            data : PrintfulStoreDomain.CatalogProductResponse.CatalogProduct.PrintfulProduct
        }

        /// Fetch a single Printful catalog product (v2)
        let fetchCatalogProductById
            (catalogProductId: int)
            : Async<PrintfulCatalog.CatalogProduct> =
            async {

                let printfulClient = PrintfulClient.configureClient (Some "v2") EnvConfig.printfulKey storeHeaders


                let url = $"catalog-products/{catalogProductId}"
                System.Console.WriteLine $"[Printful][CatalogProduct] GET {url}"

                let! resp =
                    printfulClient.GetAsync(url)
                    |> Async.AwaitTask

                System.Console.WriteLine $"[Printful][CatalogProduct] Status {resp.StatusCode}"

                let! body =
                    resp.Content.ReadAsStringAsync()
                    |> Async.AwaitTask

                System.Console.WriteLine $"[Printful][CatalogProduct] Body:\n{body}"

                if not resp.IsSuccessStatusCode then
                    failwith $"Printful catalog product fetch failed ({catalogProductId})"

                let raw =
                    try
                        JsonSerializer.Deserialize<RawSingleCatalogProductResponse>(body)
                    with ex ->
                        System.Console.WriteLine $"[Printful][CatalogProduct] DESERIALIZE ERROR: {ex.Message}"
                        raise ex

                return Shared.PrintfulStoreDomain.CatalogProductResponse.mapPrintfulProduct raw.data
            }


        // Fetch products (paginated)
        let fetchProducts (queryParams: Printful.CatalogProductRequest.CatalogProductsQuery) : Async<PrintfulStoreDomain.CatalogProductResponse.CatalogProductsResponse> = async {
            let url = PrintfulClient.productQueryString queryParams

            // configureClient storeHeaders
            let printfulClient = PrintfulClient.configureClient (Some "v2") EnvConfig.printfulKey storeHeaders

            System.Console.WriteLine $"URL: {url}"
            // + url
            let! response = printfulClient.GetAsync("catalog-products/" + url) |> Async.AwaitTask
            // 291981984 - Handsy
            System.Console.WriteLine $"RESPONSE: {response}"

            response.EnsureSuccessStatusCode() |> ignore

            let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            System.Console.WriteLine $"BODY: {body}"
            
            let raw = JsonSerializer.Deserialize<PrintfulStoreDomain.CatalogProductResponse.CatalogProduct.PrintfulCatalogProductResponse>(body)
            return PrintfulStoreDomain.CatalogProductResponse.mapPrintfulResponse raw

        }




    module Checkout =

        // ---------------- helpers ----------------

        // remove this
        // let estimateTax (countryCode: string) (state: string) (subtotal: decimal) =
        //     if countryCode = "US" then
        //         subtotal * 0.08m
        //     else
        //         subtotal * 0.15m

        // // /// repricing for sync-based items using Printful sync product details
        // let private repriceSyncItem
        //     (item : CheckoutCartItem)
        //     : Async<CheckoutPreviewLine option> =
        //     async {
        //         match item.ExternalProductId, item.SyncVariantId with
        //         | Some externalId, Some syncVariantId ->
        //             try
        //                 Console.WriteLine $"product id: {externalId}"
        //                 Console.WriteLine $"sync variant id: {syncVariantId}"
        //                 let! details = 
        //                     SyncProduct.fetchSyncProductVariantDetails externalId
             
        //                 match details with
        //                 | None ->
        //                     return None
        //                 | Some v ->
        //                     let unitPrice = v.result.retail_price |> Decimal.TryParse |> function (false, _) -> 0m | true, price -> price 
        //                     let currency  = v.result.currency
        //                     let lineTotal = unitPrice * decimal item.Quantity

        //                     return Some {
        //                         Item      = item
        //                         UnitPrice = unitPrice
        //                         Currency  = currency
        //                         LineTotal = lineTotal
        //                         IsValid   = true
        //                         Error     = None
        //                     }
        //             with e ->
        //                 Console.WriteLine $"EXCEPTION REPRICING: {e.Message}"
        //                 return failwith e.Message
        //         | _ ->
        //             return None
        //     }

        // // Template/custom – stub for now
        // let private repriceTemplateOrCustomItem (item: CheckoutCartItem) : Async<CheckoutPreviewLine option> =
        //     async {
        //         return None
        //     }

        // let repriceItem
        //     (item : CheckoutCartItem)
        //     : Async<CheckoutPreviewLine option> =
        //     match item.Kind with
        //     | CartItemKind.Sync     -> repriceSyncItem item
        //     | CartItemKind.Template -> repriceTemplateOrCustomItem item
        //     | CartItemKind.Custom   -> repriceTemplateOrCustomItem item

        // // ---------- Printful shipping DTOs (server-only) ----------

        // module Shipping =
        //     let fetchShippingRates
        //         (pfReq    : RatesRequest)
        //         : Async<RatesResponse> =
        //         async {
        //             let printfulClient = PrintfulClient.configureClient None EnvConfig.printfulKey storeHeaders

        //             System.Console.WriteLine $"[Printful][ShippingRates] Request: {pfReq}"
        //             let url = "shipping/rates"
        //             let body =
        //                 JsonSerializer.Serialize(
        //                     {| 
        //                         recipient =
        //                             {| 
        //                                 phone        = pfReq.recipient.phone |> Option.toObj
        //                                 address1     = pfReq.recipient.address1
        //                                 address2     = pfReq.recipient.address2 |> Option.toObj
        //                                 city         = pfReq.recipient.city
        //                                 state_code   = pfReq.recipient.state_code
        //                                 country_code = pfReq.recipient.country_code
        //                                 zip          = pfReq.recipient.zip
        //                             |}
        //                         items =
        //                             pfReq.items
        //                             |> List.map (fun i ->
        //                                 {| 
        //                                     variant_id =  i.variant_id |> Option.toObj
        //                                     external_variant_id = None |> Option.toObj
        //                                     warehouse_product_variant_id = None |> Option.toObj
        //                                     quantity = i.quantity
        //                                     value = None |> Option.toObj
        //                                 |})
        //                             |> Array.ofList
        //                         currency = pfReq.currency
        //                         locale   = pfReq.locale 
        //                     |}
        //                 )

        //             System.Console.WriteLine $"[Printful][ShippingRates] POST Request Body: {body}"
        //             let content = new StringContent(body, Encoding.UTF8, "application/json")

        //             System.Console.WriteLine $"[Printful][ShippingRates] POST {url}"

        //             let! resp = printfulClient.PostAsync(url, content) |> Async.AwaitTask
        //             let! respBody = resp.Content.ReadAsStringAsync() |> Async.AwaitTask

        //             System.Console.WriteLine $"[Printful][ShippingRates] RESPONSE BODY {respBody}"

        //             if not resp.IsSuccessStatusCode then
        //                 System.Console.WriteLine $"[Printful][ShippingRates] FAILED {resp.StatusCode}: {respBody}"
        //                 failwith $"[Printful][ShippingRates] {resp.StatusCode}: {respBody}"


        //             let parsed = 
        //                 try JsonSerializer.Deserialize<RatesResponse>(respBody)
        //                 with e -> 
        //                     System.Console.WriteLine $"[Printful][ShippingRates] EXCEPTION PARSING {e.Message}: {respBody}"
        //                     failwith e.Message

        //             return parsed
        //         }

        // module Tax =
        //     open StripeService.Types

        //     let fetchTaxRates
        //         (taxAddress    : TaxAddress)
        //         (total    : decimal)
        //         : Async<TaxResponse> =
        //         async {
        //             use printfulClient = PrintfulClient.configureClient None EnvConfig.printfulKey storeHeaders

        //             let url = "tax/rates"

        //             let taxRateReq : TaxRequest = {
        //                 recipient = taxAddress
        //                 amount = total
        //             }

        //             System.Console.WriteLine $"[Printful][TaxRates] tax rate req {taxRateReq}"
        //             let json =
        //                 JsonSerializer.Serialize(
        //                     taxRateReq,
        //                     JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        //                 )
        //             System.Console.WriteLine $"[Printful][TaxRates] JSON {json}"

        //             let content = new StringContent(json, Encoding.UTF8, "application/json")

        //             try
        //                 let! response = printfulClient.PostAsync(url, content) |> Async.AwaitTask

        //                 System.Console.WriteLine $"[Printful][TaxRates] response {response.IsSuccessStatusCode}"

        //                 if not response.IsSuccessStatusCode 
        //                 then failwith $"[Printful][TaxRate] {response.StatusCode}"
                        
        //                 let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        //                 System.Console.WriteLine $"[Printful][TaxRates] BODY {body}"
        //                 let parsed =
        //                     JsonSerializer.Deserialize<TaxResponse>(
        //                         body,
        //                         JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        //                     )

        //                 return parsed
        //             with e ->
        //                 System.Console.WriteLine $"[Printful][TaxRates] EXCEPTION {e.Message}; INNER: {e.InnerException.Message}"
        //                 return failwith e.Message
        //         }

        // -------- Stripe (stubbed) --------
        // think about how this actually fits in, supposed to be validation
        // let getPreviewCart (req: CheckoutPreviewRequest) : Async<CheckoutPreviewResponse> =
        //     async {
        //         let! linesArr =
        //             req.Items
        //             |> List.map repriceItem
        //             |> Async.Parallel

        //         let lines = 
        //             linesArr 
        //             |> Array.toList
        //             |> List.choose id

        //         let subtotal =
        //             lines
        //             |> List.filter (fun l -> l.IsValid)
        //             |> List.sumBy (fun l -> l.LineTotal)

        //         let shippingEstimate = 0m

        //         // For preview we can assume US unless you decide to pass address
        //         let taxEstimate = estimateTax "US" "" subtotal

        //         return {
        //             Lines            = lines
        //             Subtotal         = subtotal
        //             ShippingEstimate = shippingEstimate
        //             TaxEstimate      = taxEstimate
        //             TotalEstimate    = subtotal + shippingEstimate + taxEstimate
        //         }
        //     }

        // let getQuote (req: CheckoutQuoteRequest) : Async<CheckoutQuoteResponse> =
        //     async {
        //         System.Console.WriteLine $"Request: {req}"
        //         // 1. Reprice like preview
        //         let! linesArr =
        //             req.Items
        //             |> List.map repriceItem
        //             |> Async.Parallel

        //         let lines =
        //             linesArr 
        //             |> Array.toList
        //             |> List.choose id

        //         let subtotal =
        //             lines
        //             |> List.filter (fun l -> l.IsValid)
        //             |> List.sumBy (fun l -> l.LineTotal)

        //         // 2. Printful shipping rates
        //         let pfReq = Types.Checkout.Shipping.Mapping.toPrintfulRatesRequest req

        //         try

        //             // todo: storeHeaders
        //             let! ratesResp = Shipping.fetchShippingRates pfReq

        //             System.Console.WriteLine $"WE GOT THE RATES RESPONSE"

        //             let shippingOptions : ShippingOption list =
        //                 ratesResp.result
        //                 |> Array.map (fun r ->
        //                     {
        //                         Id       = r.id
        //                         Name     = r.name
        //                         Price    = decimal r.rate
        //                         Currency = r.currency
        //                         MinDays  = r.minDeliveryDays
        //                         MaxDays  = r.maxDeliveryDays
        //                     })
        //                 |> List.ofArray

        //             // 3. Totals per option
        //             let totals : QuoteTotals list =
        //                 shippingOptions
        //                 |> List.map (fun opt ->
        //                     let tax =
        //                         StripeTaxHelper.fetchTaxRatesAsync
        //                             req.ShippingAddress.CountryCode
        //                             req.ShippingAddress.State
        //                             req.ShippingAddress.City
        //                             req.ShippingAddress.PostalCode
        //                             (subtotal + opt.Price)
        //                         |> Async.RunSynchronously

        //                     Console.WriteLine $"Subtotal: {subtotal}"
        //                     Console.WriteLine $"Opt Prices: {opt.Price}"
        //                     Console.WriteLine $"Int64 Tax Cents: {tax.taxCents}"
        //                     Console.WriteLine $"Decimal Tax Cents: { decimal tax.taxCents}"
        //                     Console.WriteLine $"Output: {subtotal + opt.Price + ( decimal tax.taxCents / 100m )}"

        //                     {
        //                         ShippingOptionId = opt.Id
        //                         Subtotal         = subtotal
        //                         Shipping         = opt.Price
        //                         Tax              = StripeService.StripeTaxHelper.fromCentsToDollars (decimal tax.taxCents)
        //                         Total            = subtotal + opt.Price + ( decimal tax.taxCents / 100m )
        //                     })


        //             return {
        //                 Lines           = lines
        //                 ShippingOptions = shippingOptions
        //                 Totals          = totals
        //             }
        //         with e ->
        //             System.Console.WriteLine $"EXCEPTION: {e.Message}; INNER: {e.InnerException.Message}"
        //             return failwith e.Message
        //     }

        
        let stringToDecimal (str: string) =
            match Decimal.TryParse str with
            | false, _ -> failwith "unable to parse decimal"
            | true, dec -> dec


        let mapOrderItemToPreviewLine currency (orderItem: Types.Sync.Order.OrderItem) =
            {
                Item = {
                    Kind           = CartItemKind.Sync
                    Quantity       = orderItem.quantity
                    // Sync-based items (Printful "store/sync" world)
                    ExternalProductId  = Some orderItem.external_variant_id
                    SyncProductId  = Some orderItem.sync_variant_id
                    SyncVariantId = Some orderItem.variant_id
                    // Catalog/template based (if you still want to support later)
                    CatalogProductId = Some orderItem.product.product_id
                    CatalogVariantId  = Some orderItem.product.variant_id
                    TemplateId       = Some orderItem.product_template_id
                }
                UnitPrice     = stringToDecimal orderItem.retail_price
                Currency      = currency
                LineTotal     = (stringToDecimal orderItem.retail_price) * decimal orderItem.quantity
                IsValid       = not (orderItem.discontinued || orderItem.out_of_stock)
                Error         =
                    if orderItem.discontinued then Some "Item discontinued"
                    elif orderItem.out_of_stock then Some "Item out of stock"
                    else None
            }

        let createDraftOrder (req : CreateDraftOrderRequest) =
            async {

                let printfulClient = 
                    PrintfulClient.configureClient 
                        None 
                        EnvConfig.printfulKey
                        storeHeaders

                let intenalOrderId = Guid.NewGuid().ToString()

                let payload =
                    {| 
                        externalId = intenalOrderId // we need to store this IF final
                        items =
                            req.items
                            |> List.map (fun i ->
                                {| 
                                    sync_variant_id = i.productId
                                    quantity = i.quantity
                                |}
                            )
                        recipient =
                            {| 
                                name = req.address.name
                                address1 = req.address.address1
                                city = req.address.city
                                state_code = req.address.state
                                country_code = req.address.countryCode
                                zip = req.address.postalCode
                            |}
                        shipping = req.shippingOptionId
                    |}

                let url = "orders"

                try
                    let json = JsonSerializer.Serialize(payload)
                    use content = new StringContent(json, Encoding.UTF8, "application/json")

                    System.Console.WriteLine $"[Printful][DRAFT] JSON {json}"

                    let! response = printfulClient.PostAsync(url, content) |> Async.AwaitTask
                    System.Console.WriteLine $"[Printful][DRAFT] RESPONSE {response}"

                    // response.EnsureSuccessStatusCode() |> ignore
                    let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                    System.Console.WriteLine $"[Printful][DRAFT] BODY {body}"
                    let parsed = JsonSerializer.Deserialize<Types.Sync.Order.OrderResponse>(body)
                    System.Console.WriteLine $"[Printful][DRAFT] PARSED {parsed}"

                    let previewLines =
                        parsed.result.items
                        |> Array.toList
                        |> List.map (mapOrderItemToPreviewLine "USD")

                    let orderTotals =
                        {
                            ShippingName = parsed.result.shipping_service_name
                            Subtotal         = parsed.result.retail_costs.subtotal
                            Shipping         = parsed.result.retail_costs.shipping
                            Tax              = stringToDecimal parsed.result.costs.tax
                            Total            = parsed.result.retail_costs.total
                        }

                    if req.isTemp
                    then
                        // need to delete the draft
                        return CreatedTemp {
                            PreviewLines = previewLines
                            DraftOrderTotals = orderTotals
                        }
                    else
                        let draftId = (string parsed.result.id)

                        let! _ =
                            OrderDraftStorage.insertDraft
                                draftId
                                json

                        let! clientSecret, paymentId =
                            StripePayments.createPaymentIntent
                                (int (orderTotals.Total * 100m))
                                draftId
                            |> Async.AwaitTask

                        let! _ = 
                            OrderDraftStorage.setStripePaymentIntent
                                draftId
                                paymentId

                        return CreatedFinal {
                            OrderLines = previewLines
                            OrderTotals = orderTotals
                            DraftOrderId = draftId
                            StripeSecret = clientSecret
                            StripePaymentIntentId = paymentId
                        }
                with e ->
                    System.Console.WriteLine $"[Printful][DRAFT] EXCEPTION PARSING {e.Message}"
                    return failwith e.Message
            }

        let lookupOrder (req: OrderLookupRequest) : Async<OrderLookupResponse> =
            async {
                // let! orders = env.OrderRepository.FindByEmailAndOptionalOrderId(req.Email, req.OrderId)
                return { Orders =  [] } // orders }
            }

    module CheckoutAPI =
        open Checkout

        let private checkoutApi : Shared.Api.CheckoutApi =
            {
                // GetPreviewCart        = getPreviewCart
                // GetQuote              = getQuote
                CreateDraftOrder      = createDraftOrder
                LookupOrder           = lookupOrder
            }

        let handler : HttpHandler =
            Remoting.createApi()
            |> Remoting.withRouteBuilder Shared.Route.builder 
            |> Remoting.fromValue checkoutApi
            |> Remoting.buildHttpHandler


    module ProductAPI =

        let private productApi : ProductApi = {
            getProducts = CatalogProduct.fetchProducts
            getSyncProducts = SyncProduct.fetchSyncProducts
            getSyncProductVariantDetails = SyncProduct.fetchSyncProductDetails
        }

        let handler : HttpHandler =
            Remoting.createApi()
            |> Remoting.withRouteBuilder (fun typeName methodName ->
                sprintf "/api/products/%s" methodName)
            |> Remoting.fromValue productApi
            |> Remoting.buildHttpHandler