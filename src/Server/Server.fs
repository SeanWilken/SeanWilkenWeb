module Server

open SAFE
open Saturn
open Shared

open Shared.Api
open System.Net.Http
open System.Text.Json
open System.Text
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Giraffe


// CtxfvCP31MacEZMVh8th76TwH2IflQBoFN6viKrE
// 6302847
module Api =
    let http = new HttpClient()

    module Product =

        module Printful =

            // Server-only: raw types with JsonElement
            open System.Text.Json
            open System.Threading

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

            type RawProductTemplate = {
                id                    : int
                product_id            : int
                external_product_id   : string option
                title                 : string
                available_variant_ids : int array
                option_data           : RawOptionData array
                colors                : RawColor array
                sizes                 : string array
                mockup_file_url       : string
                placements            : RawPlacementOption array
                created_at            : int64
                updated_at            : int64
                placement_option_data : RawPlacementOptionData array
                design_id             : string option
            }

            type RawProductTemplateResult = {
                items : RawProductTemplate array
            }

            type RawStoreVariantResponse = {
                code   : int
                result : RawStoreVariant
            }

            type RawProductTemplateResponse = {
                code   : int
                result : RawProductTemplateResult
                extra  : JsonElement array
                paging : PrintfulCommon.PagingInfoDTO
            }


            // --- Shared DTOs (reuse types from client) ---
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

            type ProductTemplateResult = {
                items : ProductTemplate array
            }

            type ProductTemplateResponse = {
                code   : int
                result : ProductTemplateResult
                extra  : string array
                paging : PrintfulCommon.PagingInfoDTO
            }


            // --- Mapping functions ---
            let private normalizeOptionData (raw: RawOptionData) : ProductTemplate.OptionData =
                let values =
                    match raw.value.ValueKind with
                    | JsonValueKind.String -> [| raw.value.GetString() |]
                    | JsonValueKind.Array ->
                        raw.value.EnumerateArray()
                        |> Seq.choose (fun v ->
                            if v.ValueKind = JsonValueKind.String then Some (v.GetString())
                            else None
                        )
                        |> Seq.toArray
                    | _ -> [||]
                { id = raw.id; value = values }

            let private normalizePlacementOption (raw: RawPlacementOption) : ProductTemplate.PlacementOption =
                let opts =
                    raw.options
                    |> Array.choose (fun v -> if v.ValueKind = JsonValueKind.String then Some (v.GetString()) else None)
                { 
                    // placement = raw.placement
                    // display_name = raw.display_name
                    // technique_key = raw.technique_key
                    // technique_display_name = raw.technique_display_name
                    // options = opts
                    Label = raw.display_name
                    HitArea = PrintfulCommon.designHitAreaFromPrintfulString raw.placement
                }

            let private normalizePlacementOptionData (raw: RawPlacementOptionData) : ProductTemplate.PlacementOptionData =
                let opts =
                    raw.options
                    |> Array.choose (fun v -> if v.ValueKind = JsonValueKind.String then Some (v.GetString()) else None)
                { ``type`` = raw.``type``; options = opts }

            module StoreCardMapping =
                open Shared.Api
                open Shared.PrintfulStoreDomain.ProductTemplateResponse

                let private variantColorName (v: RawStoreVariant) =
                    v.color |> Option.orElse v.color_name

                let private variantColorCode (v: RawStoreVariant) =
                    v.color_code |> Option.orElse v.color_hex

                let private variantPrice (v: RawStoreVariant) =
                    v.retail_price |> Option.orElse v.price

                let private variantCurrency (v: RawStoreVariant) =
                    v.currency |> Option.orElse v.currency_code

                let private distinctSorted (xs: string list) =
                    xs |> List.distinct |> List.sort

                let private minMax (xs: decimal list) =
                    match xs with
                    | [] -> None, None
                    | _  -> Some (List.min xs), Some (List.max xs)

                let toStoreCard (tpl: RawProductTemplate) (variants: RawStoreVariant list) : StoreCard =
                    let prices = variants |> List.choose variantPrice
                    let pmin, pmax = minMax prices

                    let currencyOpt =
                        variants |> List.choose variantCurrency |> List.tryHead

                    let sizes =
                        variants |> List.choose (fun v -> v.size) |> distinctSorted

                    let colors =
                        variants
                        |> List.choose (fun v ->
                            variantColorName v |> Option.map (fun name -> name, variantColorCode v, v.id)
                        )
                        |> List.groupBy (fun (name, codeOpt, _vid) -> name, codeOpt)
                        |> List.map (fun ((name, codeOpt), rows) ->
                            {
                                Name = name
                                CodeOpt = codeOpt
                                VariantIds = rows |> List.map (fun (_,_,vid) -> vid) |> List.distinct |> List.sort
                            }
                        )
                        |> List.sortBy (fun c -> c.Name)

                    {
                        TemplateId       = tpl.id
                        PriceMin         = pmin
                        PriceMax         = pmax
                        CurrencyOpt      = currencyOpt
                        Colors           = colors
                        Sizes            = sizes
                        DefaultVariantId = tpl.available_variant_ids |> Array.tryHead
                    }

            let private parallelThrottled (maxParallel:int) (jobs: Async<'a> list) : Async<'a list> = async {
                use sem = new SemaphoreSlim(maxParallel)
                let wrap job = async {
                    do! sem.WaitAsync() |> Async.AwaitTask
                    try return! job
                    finally sem.Release() |> ignore
                }
                let! results = jobs |> List.map wrap |> Async.Parallel
                return results |> Array.toList
            }


            let mapProductTemplate (raw: RawProductTemplate) : ProductTemplate.ProductTemplate =
                { 
                    id = raw.id
                    product_id = raw.product_id
                    external_product_id = raw.external_product_id
                    title = raw.title
                    available_variant_ids = raw.available_variant_ids
                    option_data = raw.option_data |> Array.map normalizeOptionData
                    colors = raw.colors |> Array.map (fun c -> { color_name = c.color_name; color_codes = c.color_codes })
                    sizes = raw.sizes
                    mockup_file_url = raw.mockup_file_url
                    placements = raw.placements |> Array.map normalizePlacementOption
                    created_at = raw.created_at
                    updated_at = raw.updated_at
                    placement_option_data = raw.placement_option_data |> Array.map normalizePlacementOptionData
                    design_id = raw.design_id 
                }

            let mapPrintfulTemplateResponse (r: RawProductTemplateResponse) : PrintfulStoreDomain.ProductTemplateResponse.ProductTemplatesResponse =
                { 
                    templateItems = r.result.items |> Array.map mapProductTemplate |> Array.toList
                    storeCards = []
                    paging = r.paging 
                }

            open System.Net.Http.Headers

            let printfulV1HttpClient =
                new HttpClient(BaseAddress = System.Uri("https://api.printful.com/"))

            let printfulV2HttpClient =
                new HttpClient(BaseAddress = System.Uri("https://api.printful.com/v2/"))
            
            // {"id":6302847,"name":"Xero Effort","type":"native"}
            let storeHeaders =
                [ 
                    "X-PF-Store-ID", "6302847"
                ]
                |> Map.ofList

            let configureClient (client: HttpClient) (headers: Map<string, string>) =
                if not (client.DefaultRequestHeaders.Contains("X-PF-Store-ID")) then
                    headers |> Map.iter (fun k v -> client.DefaultRequestHeaders.Add(k, v))
                if client.DefaultRequestHeaders.Authorization = null then
                    client.DefaultRequestHeaders.Authorization <-
                        AuthenticationHeaderValue("Bearer", "GIitSq2Yt71FffXuLTMlMDJ439bngntVemR4QIFG")


            module CatalogProduct = 
                let private queryString (q: Printful.CatalogProductRequest.CatalogProductsQuery) =
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

                // let fetchStoreVariant (variantId:int) : Async<RawStoreVariant> = async {
                //     configureClient printfulV1HttpClient storeHeaders
                //     let! resp = printfulV1HttpClient.GetAsync($"store/variants/%d{variantId}") |> Async.AwaitTask
                //     // resp.EnsureSuccessStatusCode() |> ignore
                //     let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
                //     let raw = JsonSerializer.Deserialize<RawStoreVariantResponse>(body)
                //     return raw.result
                // }

                let fetchStoreVariant (variantId:int) : Async<RawStoreVariant> = async {
                    configureClient printfulV1HttpClient storeHeaders

                    let url = $"store/variants/%d{variantId}"
                    System.Console.WriteLine $"[Printful][StoreVariant] GET {url}"

                    let! resp =
                        printfulV1HttpClient.GetAsync(url)
                        |> Async.AwaitTask

                    System.Console.WriteLine $"[Printful][StoreVariant] Status {resp.StatusCode}"

                    let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
                    System.Console.WriteLine $"[Printful][StoreVariant] Body:\n{body}"

                    if not resp.IsSuccessStatusCode then
                        failwith $"Store variant fetch failed ({variantId})"

                    let raw =
                        try
                            JsonSerializer.Deserialize<RawStoreVariantResponse>(body)
                        with ex ->
                            System.Console.WriteLine $"[Printful][StoreVariant] DESERIALIZE ERROR: {ex.Message}"
                            raise ex

                    System.Console.WriteLine $"[Printful][StoreVariant] Parsed variant {raw.result.id}"
                    return raw.result
                }

                type RawSingleCatalogProductResponse = {
                    data : PrintfulStoreDomain.CatalogProductResponse.CatalogProduct.PrintfulProduct
                }

                /// Fetch a single Printful catalog product (v2)
                let fetchCatalogProductById
                    (catalogProductId: int)
                    : Async<PrintfulCatalog.CatalogProduct> =
                    async {

                        configureClient printfulV2HttpClient storeHeaders

                        let url = $"catalog-products/{catalogProductId}"
                        System.Console.WriteLine $"[Printful][CatalogProduct] GET {url}"

                        let! resp =
                            printfulV2HttpClient.GetAsync(url)
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
                // V1 only
                let fetchProductTemplates (queryParams: Printful.CatalogProductRequest.CatalogProductsQuery) = async {
                    let url = queryString queryParams
                    configureClient printfulV1HttpClient storeHeaders
                    let! response = printfulV1HttpClient.GetAsync("product-templates/" + url) |> Async.AwaitTask
                    // + url
                    // "product-templates/"
                    // configureClient printfulV2HttpClient storeHeaders
                    // let! response = printfulV2HttpClient.GetAsync( "product-templates/33880554" ) |> Async.AwaitTask
                    // response.EnsureSuccessStatusCode() |> ignore
                    System.Console.WriteLine $"RESPONSE: {response}"
                    let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                    System.Console.WriteLine $"BODY: {body}"
                    let raw = JsonSerializer.Deserialize<RawProductTemplateResponse>(body)
                    return mapPrintfulTemplateResponse raw
                    // let raw = JsonSerializer.Deserialize<RawProductTemplateResponseV2>(body)
                    // return mapPrintfulTemplateResponseV2 raw
                }

                module ProductDetails =
                    open Shared.StoreProductViewer

                    let fetchCatalogDetails
                        (productId: int)
                        : Async<CatalogDetails> =
                        async {

                            // reuse existing catalog product fetch + mapping
                            let! product = fetchCatalogProductById productId

                            // fetch variants if needed (later)
                            return {
                                ProductId    = product.id
                                Name         = product.name
                                Description  = product.description
                                ThumbnailUrl = Some product.thumbnailURL
                                Brand        = product.brand
                                Model        = product.model
                                Sizes        = product.sizes
                                Colors       = product.colors
                                Placements   = []        // from product.placements if needed
                                Techniques   = []        // from product.techniques if needed
                                Variants     = []        // optional for now
                            }
                        }



                    let fetchTemplateDetails
                        (templateId: int)
                        : Async<TemplateDetails> =
                        async {

                            // 1. Fetch the single template (reuse existing call)
                            let! templates =
                                fetchProductTemplates {
                                    category_ids = None
                                    colors = None
                                    limit = None
                                    newOnly = None
                                    offset = None
                                    selling_region_name = None
                                    sort_direction = None
                                    sort_type = None
                                    placements = None
                                    techniques = None
                                    destination_country = None
                                }

                            let template =
                                templates.templateItems
                                |> List.find (fun t -> t.id = templateId)

                            // 2. Fetch store variants for pricing
                            let! variants =
                                template.available_variant_ids
                                |> Array.toList
                                |> List.map fetchStoreVariant
                                |> Async.Parallel

                            return {
                                TemplateId   = template.id
                                Title        = template.title
                                MockupUrl    = Some template.mockup_file_url
                                Description  = None
                                Sizes        = template.sizes |> Array.toList
                                Colors       = template.colors |> Array.toList
                                Placements   = template.placements |> Array.toList
                                Techniques =
                                    template.placement_option_data
                                    |> Array.map (fun p -> p.``type``)
                                    |> Array.distinct
                                    |> Array.toList
                                Variants =
                                    variants
                                    |> Array.map (fun v ->
                                        {
                                            VariantId        = v.id
                                            // CatalogVariantId = v.variant_id
                                            Size             = v.size
                                            Color            = v.color_name
                                            Price            = 
                                                match v.retail_price with
                                                | Some p -> Some { Amount = p; Currency = v.currency |> Option.defaultValue "USD" }
                                                | None   -> None
                                            InStock = None
                                            ImageUrl        = None
                                            // Currency         = v.currency
                                        }
                                    )
                                    |> Array.toList
                            }
                        }

                    let getProductDetails (req: GetDetailsRequest) : Async<GetDetailsResponse> =
                        async {
                            match req.Key with
                            | Template templateId ->
                                let! details = fetchTemplateDetails templateId
                                return {
                                    Key = req.Key
                                    Details = DetailsTemplate details
                                }

                            | Catalog catalogId ->
                                let! details = fetchCatalogDetails catalogId
                                return {
                                    Key = req.Key
                                    Details = DetailsCatalog details
                                }
                        }

                // V2 only
                // let fetchProducts (queryParams: Printful.CatalogProductRequest.CatalogProductsQuery) = async {
                //     let url = queryString queryParams
                //     configureClient printfulV2HttpClient storeHeaders
                //     let! response = printfulV2HttpClient.GetAsync("catalog-products" + url) |> Async.AwaitTask
                //     response.EnsureSuccessStatusCode() |> ignore
                //     let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                //     let raw = JsonSerializer.Deserialize<Printful.CatalogProduct.PrintfulCatalogProductResponse>(body)
                //     return Printful.CatalogProduct.mapPrintfulResponse raw
                // }
                // let fetchProductTemplates (queryParams: Printful.CatalogProductRequest.CatalogProductsQuery) : Async<Printful.CatalogProduct.CatalogResponseDTO> = async {
                //     let url = queryString queryParams

                //     configureClient storeHeaders

                //     System.Console.WriteLine $"URL: {url}"
                //     // 291981984 - Handsy

                //     // let! response = printfulHttpClient.GetAsync("product-templates/" + url) |> Async.AwaitTask
                //     let! response = printfulHttpClient.GetAsync("product-templates/?limit=10" ) |> Async.AwaitTask
                //     // let! response = printfulHttpClient.GetAsync("store/products") |> Async.AwaitTask
                //     System.Console.WriteLine $"RESPONSE: {response}"

                //     response.EnsureSuccessStatusCode() |> ignore

                //     let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                //     System.Console.WriteLine $"BODY: {body}"
                    
                //     let raw = JsonSerializer.Deserialize<Printful.CatalogProduct.PrintfulCatalogProductResponse>(body)
                //     return Printful.CatalogProduct.mapPrintfulResponse raw

                //     // Deserialize into raw Printful schema
                //     // let raw = JsonSerializer.Deserialize<Printful.CatalogProduct.PrintfulStoreProductResponse>(body)
                //     // Map to your clean domain model
                //     // return Printful.CatalogProduct.mapPrintfulStoreResponse raw
                // }
                

                // Fetch products (paginated)
                let fetchProducts (queryParams: Printful.CatalogProductRequest.CatalogProductsQuery) : Async<PrintfulStoreDomain.CatalogProductResponse.CatalogProductsResponse> = async {
                    let url = queryString queryParams

                    // configureClient storeHeaders
                    configureClient printfulV2HttpClient storeHeaders

                    System.Console.WriteLine $"URL: {url}"
                    // + url
                    let! response = printfulV2HttpClient.GetAsync("catalog-products/" + url) |> Async.AwaitTask
                    // 291981984 - Handsy
                    System.Console.WriteLine $"RESPONSE: {response}"

                    response.EnsureSuccessStatusCode() |> ignore

                    let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                    System.Console.WriteLine $"BODY: {body}"
                    
                    let raw = JsonSerializer.Deserialize<PrintfulStoreDomain.CatalogProductResponse.CatalogProduct.PrintfulCatalogProductResponse>(body)
                    return PrintfulStoreDomain.CatalogProductResponse.mapPrintfulResponse raw

                }

        let private productApi : ProductApi = {
            getProducts = Printful.CatalogProduct.fetchProducts
            getProductTemplates = Printful.CatalogProduct.fetchProductTemplates
            getProductDetails = Printful.CatalogProduct.ProductDetails.getProductDetails
        }
        let handler =
            Remoting.createApi()
            |> Remoting.withRouteBuilder (fun typeName methodName ->
                sprintf "/api/products/%s" methodName)
            |> Remoting.fromValue productApi
            |> Remoting.buildHttpHandler

    module Checkout =
        
        let private checkoutApi : CheckoutApi = {
            getShippingRates = fun req -> async {
                // TODO: map req -> Printful /v2/shipping-rates body,
                // call HTTP, parse response, map into SharedShopV2.Checkout.ShippingRate list
                // (use req.Recipient + req.Items + req.Currency)
                return { Options = [] }  // stub for now
            }

            getTaxEstimate = fun req -> async {
                // For now you can reuse your existing simple tax logic,
                // or call Printful order-estimation if you want more accuracy later.
                return { required = true; rate = 8.0; shipping_taxable = true }
            }

            createDraftOrder = fun draftReq -> async {
                // TODO: map draftReq -> Printful /v2/orders (draft)
                // * recipient  -> "recipient"
                // * items      -> [ { quantity; catalog_variant_id; source="catalog"; placements=[...later] } ]
                // * shipping   -> shipping settings from SelectedRate
                // return { code = orderIdFromPrintful }
                return { code = "DRAFT-PLACEHOLDER" }
            }
        }
        

        let handler =
            Remoting.createApi()
            |> Remoting.withRouteBuilder Shared.Route.builder
            |> Remoting.fromValue checkoutApi
            |> Remoting.buildHttpHandler



    module Payment =
        let private paymentApi : PaymentApi = {
            getTaxRate = fun addr -> async {
                let body = JsonSerializer.Serialize(addr)
                use content = new StringContent(body, Encoding.UTF8, "application/json")
                let! resp = http.PostAsync("https://api.printful.com/tax/rates", content) |> Async.AwaitTask
                let! body = resp.Content.ReadAsStringAsync() |> Async.AwaitTask
                // parse body → extract tax rate
                return { rate = 5.99; required = true; shipping_taxable = true}
            }

            getShipping = fun req -> async {
                // call Printful shipping API
                return 5.99m
            }

            createPayPalOrder = fun amount -> async {
                // call PayPal with stored token
                return "ORDER123"
            }

            capturePayPalOrder = fun orderId -> async {
                // call PayPal capture endpoint
                return true
            }
        }

        let handler =
            Remoting.createApi()
            |> Remoting.withRouteBuilder Shared.Route.builder 
            |> Remoting.fromValue paymentApi
            |> Remoting.buildHttpHandler




let app = application {
    use_router (
        choose [
            Api.Payment.handler
            Api.Product.handler
            Api.Checkout.handler
        ])
    memory_cache
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main _ =
    run app
    0