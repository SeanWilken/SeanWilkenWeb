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

let storeHeaders () =
    let envConfig = EnvConfig.getConfiguredEnvironment ()
    [ 
        "X-PF-Store-ID", envConfig.PrintfulStoreId
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
    let configureClient (versionOpt: string option) (headers: Map<string, string>) =
        let envConfig = EnvConfig.getConfiguredEnvironment ()
        let client = printfulHttpClient versionOpt
        if not (client.DefaultRequestHeaders.Contains("X-PF-Store-ID")) then
            headers |> Map.iter (fun k v ->
                if not (client.DefaultRequestHeaders.Contains k) then
                    client.DefaultRequestHeaders.Add(k, v)
            )
        if isNull client.DefaultRequestHeaders.Authorization then
            client.DefaultRequestHeaders.Authorization <-
                AuthenticationHeaderValue("Bearer", envConfig.PrintfulKey)

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

    // Printful Product Catalog (Base Items)
    // THIS IS V2
    module CatalogProduct =

        type CatalogVariant = {
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

        type CatalogVariantResponse = {
            code   : int
            result : CatalogVariant
        }

    // Store Products imported from Printful
    // THESE ARE WHAT WE ARE USING
    module SyncProduct =

        [<CLIMutable>]
        type SyncProductSummary = {
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

        type SyncProductResponse = {
            code : int
            result  : SyncProductSummary array
            paging : PagingInfoDTO
            extra    : string array
        }
        
        // do we need this? This is the list response shape?
        type SyncProductDetails = {
            product_id : int
            variant_id : int
            external_id   : string option
            name          : string
            thumbnail_url : string option
        }

        [<CLIMutable>]
        type VariantProductInfo = {
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
        type SyncFile = {
            [<JsonPropertyName("type")>]
            ``type`` : string
            [<JsonPropertyName("preview_url")>]
            preview_url : string
        }

        // [<CLIMutable>]
        // type SyncOption = {
        //     [<JsonPropertyName("id")>]
        //     id : string
        //     [<JsonPropertyName("value")>]
        //     value : string
        // }

        [<CLIMutable>]
        type SyncVariant = {
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
            product : VariantProductInfo
            [<JsonPropertyName("files")>]
            files : SyncFile array
            // [<JsonPropertyName("options")>]
            // options : SyncOption array
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
            sync_product : SyncProductSummary // ??
            [<JsonPropertyName("sync_variants")>]
            sync_variants : SyncVariant array
        }

        [<CLIMutable>]
        type SyncProductDetailsResponse = {
            [<JsonPropertyName("code")>]
            code : int
            [<JsonPropertyName("result")>]
            result : RawSyncProductDetailsResult
            [<JsonPropertyName("extra")>]
            extra : JsonElement array
        }

        [<CLIMutable>]
        type SingleSyncProductDetailsResponse = {
            [<JsonPropertyName("code")>]
            code : int
            [<JsonPropertyName("result")>]
            result : SyncVariant
        }


        module Mapping =

            let tryParseDecimal (s: string) : decimal option =
                match Decimal.TryParse(s, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture) with
                | true, d -> Some d
                | _ -> None

            let private pickPreviewUrl (files: SyncFile array) : string option =
                files
                |> Array.tryFind (fun f -> String.Equals(f.``type``, "preview", StringComparison.OrdinalIgnoreCase))
                |> Option.map (fun f -> f.preview_url)
                |> Option.orElse (
                    files
                    |> Array.tryHead
                    |> Option.map (fun f -> f.preview_url)
                )
                |> Option.filter (fun s -> not (String.IsNullOrWhiteSpace s))

            module Response =

                let mapSummary (r: SyncProductSummary) : StoreProductViewer.SyncProduct.SyncProductSummary =
                    {
                        Id = r.id
                        Name          = r.name
                        VariantCount  = r.variants
                        ExternalId    = if String.IsNullOrWhiteSpace r.external_id then None else Some r.external_id
                        ThumbnailUrl  = if String.IsNullOrWhiteSpace r.thumbnail_url then None else Some r.thumbnail_url
                    }

                let toDetailsResponse (raw: SyncProductDetailsResponse) : StoreProductViewer.SyncProduct.SyncProductDetailsResponse =
                    let sp = raw.result.sync_product
                    let variants =
                        raw.result.sync_variants
                        |> Array.toList
                        |> List.map (
                            fun variant ->
                                { 
                                    Id    = variant.id
                                    ExternalId = variant.external_id
                                    SyncProductId    = variant.sync_product_id
                                    VariantId = variant.variant_id
                                    VariantProductId = variant.product.product_id
                                    VariantProductVariantId = variant.product.variant_id
                                    Name             = if String.IsNullOrWhiteSpace variant.name then None else Some variant.name
                                    Size             = if String.IsNullOrWhiteSpace variant.size then None else Some variant.size
                                    Color            = if String.IsNullOrWhiteSpace variant.color then None else Some variant.color
                                    ImageUrl         = 
                                        if isNull variant.product.image 
                                        then None 
                                        else Some variant.product.image |> Option.filter (fun x -> not (String.IsNullOrWhiteSpace x))
                                    PreviewUrl       = pickPreviewUrl variant.files
                                    RetailPrice      = tryParseDecimal variant.retail_price
                                    Currency         = 
                                        if String.IsNullOrWhiteSpace variant.currency 
                                        then None
                                        else Some variant.currency
                                    Availability     = 
                                        if String.IsNullOrWhiteSpace variant.availability_status 
                                        then None 
                                        else Some variant.availability_status
                                } : StoreProductViewer.SyncProduct.SyncVariant
                        )

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

            module MongoDocument =

                open StoreProductStorage
                
                let mapVariant (v: SyncVariant) : StoreVariantDoc =
                    { 
                        CatalogProductId = v.product.product_id
                        CatalogVariantId = v.product.variant_id
                        SyncVariantId = v.id
                        ExternalId = v.external_id
                        VariantId = v.variant_id
                        Name = v.name
                        Size = v.size
                        Color = v.color
                        Availability = if String.IsNullOrWhiteSpace v.availability_status then None else Some v.availability_status
                        Sku = v.sku
                        Currency = v.currency
                        RetailPrice = tryParseDecimal v.retail_price
                        ImageUrl = if String.IsNullOrWhiteSpace v.product.image then None else Some v.product.image
                        PreviewUrl = pickPreviewUrl v.files
                        FileUrls = v.files |> Array.map (fun f -> f.preview_url)
                    }

                let computeProductSummary (variants: StoreVariantDoc[]) : StoreProductSummaryDoc =
                    let prices = variants |> Array.choose (fun v -> v.RetailPrice)
                    let priceMin = if prices.Length = 0 then None else Some (Array.min prices)
                    let priceMax = if prices.Length = 0 then None else Some (Array.max prices)

                    let colors =
                        variants
                        |> Array.map (fun v -> v.Color)
                        |> Array.distinct
                        |> Array.sort

                    let sizes =
                        variants
                        |> Array.map (fun v -> v.Size)
                        |> Array.distinct
                        |> Array.sort

                    {
                        PriceMin = priceMin
                        PriceMax = priceMax
                        Colors = colors
                        Sizes = sizes
                        PrimaryCatalogProductId = None
                        BlankName = None
                        BlankBrand = None
                        BlankModel = None
                        BlankImage = None
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
                product : VariantProductInfo
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

            [<CLIMutable>]    
            type RecipientAddress = { 
                [<JsonPropertyName("name")>]
                name : string 
                [<JsonPropertyName("address1")>]
                address1 : string
                [<JsonPropertyName("address2")>]
                address2 : string
                [<JsonPropertyName("city")>]
                city : string
                [<JsonPropertyName("state_code")>]
                state_code : string
                [<JsonPropertyName("state_name")>]
                state_name : string
                [<JsonPropertyName("country_code")>]
                country_code : string
                [<JsonPropertyName("country_name")>]
                country_name : string
                [<JsonPropertyName("zip")>]
                zip : string
                [<JsonPropertyName("phone")>]
                phone : string
                [<JsonPropertyName("email")>]
                email : string
            }

            [<CLIMutable>]    
            type ShipmentItem = { 
                [<JsonPropertyName("item_id")>]
                item_id : int 
                [<JsonPropertyName("quantity")>]
                quantity : int 
                [<JsonPropertyName("picked")>]
                picked : int
                [<JsonPropertyName("printed")>]
                printed : int
            }

            [<CLIMutable>]    
            type Shipment = { 
                [<JsonPropertyName("id")>]
                id : string 
                [<JsonPropertyName("carrier")>]
                carrier : string 
                [<JsonPropertyName("service")>]
                service : string
                [<JsonPropertyName("tracking_number")>]
                tracking_number : string
                [<JsonPropertyName("tracking_url")>]
                tracking_url : string
                [<JsonPropertyName("created")>]
                created : string
                [<JsonPropertyName("ship_date")>]
                ship_date : string
                [<JsonPropertyName("shipped_at")>]
                shipped_at : string
                [<JsonPropertyName("items")>]
                items : ShipmentItem array
                [<JsonPropertyName("zip")>]
                zip : string
                [<JsonPropertyName("phone")>]
                phone : string
                [<JsonPropertyName("email")>]
                email : string
            }

            [<CLIMutable>]    
            type ConfirmOrderResult = { 
                [<JsonPropertyName("id")>]
                id : int
                [<JsonPropertyName("external_id")>]
                external_id : string 
                [<JsonPropertyName("store")>]
                store : int64
                [<JsonPropertyName("status")>]
                status : string
                [<JsonPropertyName("shipping")>]
                shipping : string
                [<JsonPropertyName("shipping_service_name")>]
                shipping_service_name : string
                [<JsonPropertyName("shipments")>]
                shipments : Shipment array
                [<JsonPropertyName("recipient")>]
                recipient : RecipientAddress
                [<JsonPropertyName("items")>]
                items : OrderItem array
                [<JsonPropertyName("costs")>]
                costs : OrderCosts
                [<JsonPropertyName("retail_costs")>]
                retail_costs : OrderRetailCosts
            }

            [<CLIMutable>]    
            type PrintfulConfirmOrderResponse = { 
                [<JsonPropertyName("code")>]
                code : int 
                [<JsonPropertyName("result")>]
                result : ConfirmOrderResult
            }
open Types.SyncProduct

module PrintfulApi =

    open System.Net

    let private tryGetRetryAfterSeconds (resp: HttpResponseMessage) =
        // Prefer Retry-After header if present
        if resp.Headers.RetryAfter <> null then
            if resp.Headers.RetryAfter.Delta.HasValue then
                int (resp.Headers.RetryAfter.Delta.Value.TotalSeconds)
                |> Some
            elif resp.Headers.RetryAfter.Date.HasValue then
                let seconds = int (resp.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow).TotalSeconds
                Some (max 1 seconds)
            else None
        else None


    module SyncProduct =

        open Types.SyncProduct
        open Shared.StoreProductViewer.SyncProduct

        /// Raw list (paged) - keep all fields for DB sync
        let fetchRawSyncProductsPage (limit: int option) (offset: int option) : Async<SyncProductResponse> =
            async {
                let printfulClient = PrintfulClient.configureClient None (storeHeaders())
                let url =
                    "store/products" +
                    PrintfulClient.qsParams [
                        match limit with  Some v -> ("limit", string v)  | None -> ()
                        match offset with Some v -> ("offset", string v) | None -> ()
                    ]

                let! resp = printfulClient.GetAsync(url) |> Async.AwaitTask
                let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
                PrintfulClient.ensureSuccessOrFail resp body "[Printful][RawSyncProductsPage]"
                return JsonSerializer.Deserialize<SyncProductResponse>(body, jsonOptions)
            }

        /// Raw details for one sync product - includes variants + files, etc.
        let fetchRawSyncProductDetails (syncProductId: int) : Async<Types.SyncProduct.SyncProductDetailsResponse> =
            async {
                let printfulClient = PrintfulClient.configureClient None (storeHeaders())

                let url = $"store/products/{syncProductId}"

                let rec attempt (n: int) : Async<Types.SyncProduct.SyncProductDetailsResponse> =
                    async {
                        let! resp = printfulClient.GetAsync(url) |> Async.AwaitTask
                        let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask

                        if resp.StatusCode = HttpStatusCode.TooManyRequests
                        then
                            let retryAfter = tryGetRetryAfterSeconds resp |> Option.defaultValue 60
                            let jitterMs = Random().Next(0, 750)
                            let waitMs = retryAfter * 1000 + jitterMs

                            if n >= 5
                            then return failwith $"[Printful][RawSyncProductDetails] 429 after {n} retries. Body: {body}"
                            else
                                printfn $"[Printful] 429 rate limited. Waiting {waitMs}ms then retrying (attempt {n+1}/5)..."
                                do! Async.Sleep waitMs
                                return! attempt (n + 1)
                        else
                            PrintfulClient.ensureSuccessOrFail resp body "[Printful][RawSyncProductDetails]"
                            return JsonSerializer.Deserialize<Types.SyncProduct.SyncProductDetailsResponse>(body, jsonOptions)
                    }

                return! attempt 0
            }


        let fetchSyncProducts
            (req    : GetSyncProductsRequest)
            : Async<SyncProductsResponse> =
            async {

                let printfulClient = PrintfulClient.configureClient None (storeHeaders())

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
                    let raw = JsonSerializer.Deserialize<SyncProductResponse>(body, jsonOptions)
                    System.Console.WriteLine $"[Printful][SyncProducts] RAW {raw.code} items={raw.result.Length}"

                    return {
                        items  = raw.result |> Array.toList |> List.map Mapping.Response.mapSummary
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
            : Async<StoreProductViewer.SyncProduct.SyncProductDetailsResponse> =
            async {
                let printfulClient = PrintfulClient.configureClient None (storeHeaders())

                let url = $"store/products/{syncProductId.syncProductId}"
                System.Console.WriteLine $"[Printful][SyncProductDetails] FETCH DETAILS"
                let! resp = printfulClient.GetAsync(url) |> Async.AwaitTask
                System.Console.WriteLine $"[Printful][SyncProductDetails] GOT RESPONSE"
                let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask

                PrintfulClient.ensureSuccessOrFail resp body "[Printful][SyncProductDetails]"
                
                try

                    System.Console.WriteLine $"[Printful][SyncProductDetails] DESERIALIZE {body}"
                    let raw = JsonSerializer.Deserialize<Types.SyncProduct.SyncProductDetailsResponse>(body, jsonOptions)
                    System.Console.WriteLine $"[Printful][SyncProductDetails] RAW: {raw}"
                    return Mapping.Response.toDetailsResponse raw
                with e ->
                    System.Console.WriteLine $"[Printful][SyncProductDetails] DESERIALIZE ERROR: {e.Message}"
                    return failwith $"[Printful][SyncProductDetails] DESERIALIZE ERROR: {e.Message}"
            }

        let fetchSyncProductVariantDetails
            (externalSyncVariantId : string)
            : Async<SingleSyncProductDetailsResponse option> =
            async {
                let printfulClient = PrintfulClient.configureClient None (storeHeaders())

                let url = $"store/variants/@{externalSyncVariantId}"
                System.Console.WriteLine $"[Printful][SyncSingleProductDetails] FETCH DETAILS"
                let! resp = printfulClient.GetAsync(url) |> Async.AwaitTask
                System.Console.WriteLine $"[Printful][SyncSingleProductDetails] GOT RESPONSE"
                let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask

                PrintfulClient.ensureSuccessOrFail resp body "[Printful][SyncSingleProductDetails]"
                
                try
                    System.Console.WriteLine $"[Printful][SyncSingleProductDetails] DESERIALIZE {body}"
                    let raw = JsonSerializer.Deserialize<SingleSyncProductDetailsResponse>(body, jsonOptions)
                    System.Console.WriteLine $"[Printful][SyncSingleProductDetails] RAW: {raw}"
                    return Some raw
                with e ->
                    System.Console.WriteLine $"[Printful][SyncSingleProductDetails] DESERIALIZE ERROR: {e.Message}"
                    return None
            }


























































    module CatalogProduct =

        let mapCatalogProductToDoc
            (p: PrintfulStoreDomain.CatalogProductResponse.CatalogProduct.PrintfulProduct)
            : StoreProductStorage.CatalogProductDoc = 
                {
                    Id = p.id
                    MainCategoryId = p.main_category_id
                    Type = p.``type``
                    Name = p.name
                    Brand = p.brand
                    Model = p.model
                    Image = p.image
                    VariantCount = p.variant_count
                    IsDiscontinued = p.is_discontinued
                    Description = p.description
                    Sizes = p.sizes
                    Colors =
                        p.colors
                        |> Array.map (fun c ->
                            { 
                                StoreProductStorage.CatalogColorDoc.Name = c.name
                                Value = c.value
                            }
                        )
                    Techniques =
                        p.techniques
                        |> Array.map (fun t ->
                            { 
                                StoreProductStorage.CatalogTechniqueDoc.Key = t.key
                                DisplayName = Some t.display_name 
                            }
                        )
                    Placements =
                        p.placements
                        |> Array.map (fun pl ->
                            { 
                                StoreProductStorage.CatalogPlacementDoc.Placement = pl.placement
                                DisplayName = Some pl.display_name 
                            }
                        )
                    ProductOptions =
                        p.product_options
                        |> Array.map (fun o ->
                            { 
                                StoreProductStorage.CatalogOptionDoc.Id = o.id
                                Title = Some o.title
                                Type = Some o.type'
                            }
                        )
                    UpdatedAt = DateTime.UtcNow
                }


        type RawSingleCatalogProductResponse = {
            data : PrintfulStoreDomain.CatalogProductResponse.CatalogProduct.PrintfulProduct
        }


        /// Fetch a single Printful catalog product (v2)
        let fetchCatalogProductById
            (catalogProductId: int)
            : Async<PrintfulCatalog.CatalogProduct> =
            async {

                let printfulClient = PrintfulClient.configureClient (Some "v2") (storeHeaders())


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
            let printfulClient = PrintfulClient.configureClient (Some "v2") (storeHeaders())

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
        open MongoService.OrderDraftStorage

        module OrderConfirmationEmail =

            open Gmail
            open Types.SyncProduct.Order
            
            let renderItemRowsHtml (currencySymbol: string) (items: OrderItem array) =
                let sb = StringBuilder()

                for it in items do
                    let name = htmlEncode it.name
                    let qty  = it.quantity
                    let unit = htmlEncode it.retail_price

                    sb.AppendLine("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"margin:0 0 10px 0;\">") |> ignore
                    sb.AppendLine("  <tr>") |> ignore
                    sb.AppendLine("    <td style=\"font-size:14px; color:#111; padding:8px 0;\">") |> ignore
                    sb.AppendLine($"      <div style=\"font-weight:700;\">{name}</div>") |> ignore
                    sb.AppendLine("      <div style=\"font-size:12px; color:#666; margin-top:2px;\">") |> ignore
                    sb.AppendLine($"        Qty {qty}") |> ignore
                    sb.AppendLine("      </div>") |> ignore
                    sb.AppendLine("    </td>") |> ignore
                    sb.AppendLine("    <td align=\"right\" style=\"font-size:14px; color:#111; padding:8px 0; white-space:nowrap;\">") |> ignore
                    sb.AppendLine($"{currencySymbol}{unit}") |> ignore
                    sb.AppendLine("    </td>") |> ignore
                    sb.AppendLine("  </tr>") |> ignore
                    sb.AppendLine("</table>") |> ignore

                sb.ToString()

            
            let mkVars
                (brandName: string)
                (brandSiteUrl: string)
                (brandTagline: string)
                (supportEmail: string)
                (orderLookupUrl: string)
                (stripePaymentIntentId: string)
                (doc: OrderDraftDocument option)
                (confirm: PrintfulConfirmOrderResponse)
                =
                    let firstName, paymentConfirmation, stripeId, draftId =
                        match doc with
                        | None ->
                            confirm.result.recipient.name, DateTime.UtcNow, stripePaymentIntentId, confirm.result.external_id
                        | Some d ->
                            let fn = d.CustomerFirstName |> Option.defaultValue "Friend"
                            let pc =
                                if d.PaymentConfirmedAt.HasValue then d.PaymentConfirmedAt.Value
                                else d.CreatedAt
                            let sid = d.StripePaymentIntentId |> Option.defaultValue stripePaymentIntentId 
                            let did = d.DraftExternalId
                            fn, pc, sid, did
                    let r = confirm.result
                    let rc = r.retail_costs
                    let symbol = currencySymbol rc.currency

                    let ship = r.recipient
                    { 
                        BrandLogoUrl = Some "https://xeroeffort.com/img/xe_ico.png"
                        BrandName = brandName
                        BrandTagline = brandTagline
                        BrandSiteUrl = brandSiteUrl
                        HeroImageUrl = Some "https://xeroeffort.com/img/artwork/roses.png"
                        SupportEmail = supportEmail

                        OrderDate = formatOrderDate paymentConfirmation
                        CustomerFirstname = htmlEncode firstName

                        AppOrderId = htmlEncode draftId
                        PrintfulOrderId = htmlEncode (string r.id)
                        StripePaymentId = htmlEncode stripeId 
                        OrderLookupUrl = orderLookupUrl

                        ItemCount = r.items.Length
                        ItemRowsHtml = renderItemRowsHtml symbol r.items
                        CurrencySymbol = symbol
                        Subtotal = $"{fmtDecimal rc.subtotal}"
                        Shipping = "" // $"{rc.shipping:0.00}"
                        Tax =
                            match rc.tax with
                            | Some t -> $"{fmtDecimal t}"
                            | None -> "0.00"
                        Total = $"{fmtDecimal rc.total}"
                        ShipName = htmlEncode ship.name
                        ShipLine1 = htmlEncode ship.address1
                        ShipLine2 = 
                            if String.IsNullOrWhiteSpace ship.address2 then "" else htmlEncode ship.address2
                        ShipCity = htmlEncode ship.city
                        ShipState = htmlEncode ship.state_code
                        ShipZip = htmlEncode ship.zip
                        ShipCountry = htmlEncode ship.country_name
                        ShipEmail = htmlEncode ship.email 
                    }

            let orderConfirmationEmail orderResponse orderDraft stripePaymentIntentId = 
                renderOrderConfirmationTemplate
                    (ServerUtilities.EmbeddedResources.loadEmbedded "Server.Template.Email.OrderConfirmation.html")
                    (mkVars
                        "Xero Effort"
                        "xeroeffort.com"
                        "All things come from xero"
                        "xeroeffortclub@gmail.com"
                        "https://xeroeffort.com/shop/orders"
                        stripePaymentIntentId
                        orderDraft
                        orderResponse)




        // ---------------- helpers ----------------

        let stringToDecimal (str: string) =
            match Decimal.TryParse str with
            | false, _ -> failwith "unable to parse decimal"
            | true, dec -> dec


        let mapOrderItemToPreviewLine currency (orderItem: Types.SyncProduct.Order.OrderItem) =
            {
                Item = {
                    Name          = orderItem.name
                    ThumbnailUrl = orderItem.product.image
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

                let printfulClient = PrintfulClient.configureClient None (storeHeaders())

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
                    let parsed = JsonSerializer.Deserialize<Types.SyncProduct.Order.OrderResponse>(body)
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
                            OrderDraftStorage.insertDraftWithCustomer
                                draftId
                                json
                                (Some "usd")
                                (decimal req.totalsCents * 100m)
                                (req.address.name.Split " " |> Array.tryHead)
                                (req.address.name.Split " " |> Array.rev |> Array.tryHead )
                                (Some req.address.email)

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

        let markOrderConfirmed draftExternalId printfulJson printfulOrderId =
            setDraftPrintfulResult
                draftExternalId
                "order_confirmed"
                (Some printfulJson)
                (Some printfulOrderId)


        let confirmOrder (req : ConfirmOrderRequest) : Async<ConfirmOrderResponse> =
            async {

                let printfulClient = PrintfulClient.configureClient None (storeHeaders())

                let urlParam =
                    match Guid.TryParse req.OrderDraftId with
                    | false, _ -> req.OrderDraftId
                    | true, g -> "@" + g.ToString()

                let url = $"orders/{urlParam}/confirm"

                try
                    use content = new StringContent("", Encoding.UTF8, "application/json")

                    let! _ = 
                        setStripePaymentIntent
                            req.OrderDraftId
                            req.StripeConfirmation

                    System.Console.WriteLine $"[Printful][CONFIRM] JSON {json}"

                    let! response = printfulClient.PostAsync(url, content) |> Async.AwaitTask
                    System.Console.WriteLine $"[Printful][CONFIRM] RESPONSE {response}"

                    response.EnsureSuccessStatusCode() |> ignore
                    let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                    System.Console.WriteLine $"[Printful][CONFIRM] BODY {body}"

                    let parsed : Types.SyncProduct.Order.PrintfulConfirmOrderResponse = 
                        JsonSerializer.Deserialize<Types.SyncProduct.Order.PrintfulConfirmOrderResponse>(body)
                    System.Console.WriteLine $"[Printful][CONFIRM] PARSED {parsed}"
                    
                    let! _ = markOrderConfirmed req.OrderDraftId body parsed.result.id
                        
                    let previewLines =
                        parsed.result.items
                        |> Array.toList
                        |> List.map (mapOrderItemToPreviewLine "USD")

                    let orderTotals =
                        {
                            ShippingName = parsed.result.shipping
                            Subtotal         = parsed.result.retail_costs.subtotal
                            Shipping         = parsed.result.retail_costs.shipping
                            Tax              = parsed.result.retail_costs.tax |> Option.defaultValue 0m
                            Total            = parsed.result.retail_costs.total
                        }

                    let! docOpt =
                        tryGetByDraftExternalId req.OrderDraftId

                    let! _ = 
                        Gmail.sendEmail
                            req.CustomerInfo.Email
                            $"Xero Effort Order Confirmation: {req.OrderDraftId}"
                            (OrderConfirmationEmail.orderConfirmationEmail parsed docOpt req.StripeConfirmation)

                    return {
                        OrderId = parsed.result.id
                        InternalId = parsed.result.external_id
                        Status = parsed.result.status
                        ShippingServiceName = parsed.result.shipping_service_name
                        Shipments =
                            parsed.result.shipments
                            |> Array.map ( fun x ->
                                {
                                    Carrier = x.carrier
                                    Service = x.service
                                    TrackingNumber = x.tracking_number
                                    TrackingUrl = x.tracking_url
                                    ShipDate = x.ship_date
                                    Items = 
                                        x.items 
                                        |> Array.map ( fun it -> it.item_id )
                                        |> Array.toList
                                }
                            )
                            |> Array.toList
                            
                        OrderItems = previewLines
                        Costs = orderTotals
                    }

                with e ->
                    System.Console.WriteLine $"[Printful][DRAFT] EXCEPTION PARSING {e.Message}"
                    return failwith e.Message
            }

        let lookupOrder (req: OrderLookupRequest) : Async<OrderLookupResponse> =
            async {

                let! drafts =
                    OrderDraftStorage.findByEmailAndOptionalOrderId
                        req.Email
                        req.OrderId

                // Map OrderDraftDocument -> your DTO shape
                let orders =
                    drafts
                    |> List.map (fun d ->
                        {
                            OrderId = d.DraftExternalId
                            Email = d.CustomerEmail |> Option.defaultValue ""
                            CreatedAt = d.CreatedAt
                            Status =
                                d.Status |> OrderStatus.fromString
                            Total = d.OrderTotal
                            Currency = d.OrderCurrency |> Option.defaultValue "usd"
                            PrintfulOrderId =
                                if d.PrintfulOrderId.HasValue
                                then Some d.PrintfulOrderId.Value
                                else None
                        }
                    )

                return { Orders = orders }
            }


    module CheckoutAPI =
        open Checkout

        let private checkoutApi : Shared.Api.CheckoutApi =
            {
                CreateDraftOrder      = createDraftOrder
                ConfirmOrder          = confirmOrder
                LookupOrder           = lookupOrder
            }

        let handler : HttpHandler =
            Remoting.createApi()
            |> Remoting.withRouteBuilder Shared.Route.builder 
            |> Remoting.fromValue checkoutApi
            |> Remoting.buildHttpHandler


    module PrintfulProductApi =

        let private printfulProductApi : PrintfulProductApi = {
            getProducts = CatalogProduct.fetchProducts
            getSyncProducts = SyncProduct.fetchSyncProducts
            getSyncProductVariantDetails = SyncProduct.fetchSyncProductDetails
        }

        let handler : HttpHandler =
            Remoting.createApi()
            |> Remoting.withRouteBuilder (fun typeName methodName ->
                sprintf "/api/printful/%s" methodName)
            |> Remoting.fromValue printfulProductApi
            |> Remoting.buildHttpHandler


module MongoHelpers =
    open MongoDB.Driver
    open MongoService.StoreProductStorage

    let upsertFromRawDetails
        (collection: IMongoCollection<StoreProductDoc>)
        (raw: SyncProductDetailsResponse)
        =
        async {
            let sp = raw.result.sync_product
            let now = DateTime.UtcNow

            let filter =
                Builders<StoreProductDoc>.Filter.Eq((fun p -> p.SyncProductId), sp.id)

            let! existing =
                collection.Find(filter).FirstOrDefaultAsync()
                |> Async.AwaitTask

            let variants =
                raw.result.sync_variants
                |> Array.map Mapping.MongoDocument.mapVariant

            let summary = Mapping.MongoDocument.computeProductSummary variants

            let doc =
                if isNull (box existing)
                then
                    { 
                        Id = Guid.NewGuid()
                        SyncProductId = sp.id
                        ExternalId = if String.IsNullOrWhiteSpace sp.external_id then None else Some sp.external_id
                        Name = sp.name
                        ThumbnailUrl = if String.IsNullOrWhiteSpace sp.thumbnail_url then None else Some sp.thumbnail_url
                        Synced = Some sp.synced
                        VariantCount = sp.variants
                        IsIgnored = Some sp.is_ignored
                        DesignKey = None
                        Tags = [||]
                        Variants = variants
                        CatalogProductIds = [||]
                        Summary = summary
                        CreatedAt = now
                        UpdatedAt = now 
                    }
                else
                    { existing with
                        ExternalId = if String.IsNullOrWhiteSpace sp.external_id then None else Some sp.external_id
                        Name = sp.name
                        ThumbnailUrl = if String.IsNullOrWhiteSpace sp.thumbnail_url then None else Some sp.thumbnail_url
                        Synced = Some sp.synced
                        VariantCount = sp.variants
                        IsIgnored = Some sp.is_ignored
                        Variants = variants
                        Summary = { 
                            existing.Summary with
                                PriceMin = summary.PriceMin 
                                PriceMax = summary.PriceMax 
                                Colors = summary.Colors 
                                Sizes = summary.Sizes 
                        }
                        UpdatedAt = now
                    }

            do!
                collection.ReplaceOneAsync(filter, doc, ReplaceOptions(IsUpsert = true))
                |> Async.AwaitTask
                |> Async.Ignore
        }
