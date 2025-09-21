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
open SharedShopV2Domain


// CtxfvCP31MacEZMVh8th76TwH2IflQBoFN6viKrE
// 6302847
module Api =
    let http = new HttpClient()

    module Product =

        module Printful =

            // Server-only: raw types with JsonElement
            open System.Text.Json

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
            let private normalizeOptionData (raw: RawOptionData) : SharedShopV2.ProductTemplate.OptionData =
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

            let private normalizePlacementOption (raw: RawPlacementOption) : SharedShopV2.ProductTemplate.PlacementOption =
                let opts =
                    raw.options
                    |> Array.choose (fun v -> if v.ValueKind = JsonValueKind.String then Some (v.GetString()) else None)
                { 
                    placement = raw.placement
                    display_name = raw.display_name
                    technique_key = raw.technique_key
                    technique_display_name = raw.technique_display_name
                    options = opts 
                }

            let private normalizePlacementOptionData (raw: RawPlacementOptionData) : SharedShopV2.ProductTemplate.PlacementOptionData =
                let opts =
                    raw.options
                    |> Array.choose (fun v -> if v.ValueKind = JsonValueKind.String then Some (v.GetString()) else None)
                { ``type`` = raw.``type``; options = opts }

            let mapProductTemplate (raw: RawProductTemplate) : SharedShopV2.ProductTemplate.ProductTemplate =
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

            let mapPrintfulTemplateResponse (r: RawProductTemplateResponse) : ProductTemplateResponse.ProductTemplatesResponse =
                { 
                    templateItems = r.result.items |> Array.map mapProductTemplate |> Array.toList
                    paging = r.paging 
                }

            open System.Net.Http.Headers

            // let private printfulHttpClient =
            //     new HttpClient(BaseAddress = System.Uri("https://api.printful.com/"))
            //     // new HttpClient(BaseAddress = System.Uri("https://api.printful.com/v2/"))
            //     // Authentication (use your API key)

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
                // if not (client.DefaultRequestHeaders.Contains("X-PF-Store-ID")) then
                //     headers |> Map.iter (fun k v -> client.DefaultRequestHeaders.Add(k, v))
                if client.DefaultRequestHeaders.Authorization = null then
                    client.DefaultRequestHeaders.Authorization <-
                        // AuthenticationHeaderValue("Bearer", "CtxfvCP31MacEZMVh8th76TwH2IflQBoFN6viKrE")
                        AuthenticationHeaderValue("Bearer", "GIitSq2Yt71FffXuLTMlMDJ439bngntVemR4QIFG")

            // let configureClient(headers: Map<string, string>) =
            // // do
            //     // headers |> Map.iter (fun k v -> printfulHttpClient.DefaultRequestHeaders.Add(k, v))
            //     printfulHttpClient.DefaultRequestHeaders.Add("X-PF-Store-ID", "6302847")
            //     printfulHttpClient.DefaultRequestHeaders.Authorization <-
            //         AuthenticationHeaderValue("Bearer", "TOKEN_HERE")
                    // let apiKey =
                    //     match System.Environment.GetEnvironmentVariable("PRINTFUL_API_KEY") with
                    //     | a when a = null -> "<YOUR_API_KEY>"
                    //     | str ->  str
                    // AuthenticationHeaderValue("Bearer", apiKey)

            module CatalogProduct = 
                open System.Threading.Tasks

                let private queryString (q: Printful.CatalogProductRequest.CatalogProductsQuery) =
                    [
                        q.category_ids |> Option.map (fun ids -> "category_ids", String.concat "," (ids |> List.map string))
                        q.colors |> Option.map (fun xs -> "colors", String.concat "," xs)
                        q.limit |> Option.map (fun v -> "limit", string v)
                        q.newOnly |> Option.map (fun v -> "new", if v then "true" else "false")
                        q.offset |> Option.map (fun v -> "offset", string v)
                        q.placements |> Option.map (fun xs -> "placements", String.concat "," xs)
                        q.selling_region_name |> Option.map (fun v -> "selling_region_name", v)
                        q.sort_direction |> Option.map (fun v -> "sort_direction", v)
                        q.sort_type |> Option.map (fun v -> "sort_type", v)
                        q.techniques |> Option.map (fun xs -> "techniques", String.concat "," xs)
                        q.destination_country |> Option.map (fun v -> "destination_country", v)
                    ]
                    |> List.choose id
                    |> List.map (fun (k,v) -> $"{k}={System.Uri.EscapeDataString v}")
                    |> String.concat "&"
                    |> fun s -> if s = "" then "" else "?" + s

                // Fetch products (paginated)
                // V1 only
                let fetchProductTemplates (queryParams: Printful.CatalogProductRequest.CatalogProductsQuery) = async {
                    let url = queryString queryParams
                    configureClient printfulV1HttpClient storeHeaders
                    // let! response = printfulV1HttpClient.GetAsync("product-templates/" + url) |> Async.AwaitTask
                    let! response = printfulV1HttpClient.GetAsync("product-templates/" + url) |> Async.AwaitTask
                    // response.EnsureSuccessStatusCode() |> ignore
                    System.Console.WriteLine $"RESPONSE: {response}"
                    let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                    System.Console.WriteLine $"BODY: {body}"
                    let raw = JsonSerializer.Deserialize<RawProductTemplateResponse>(body)
                    return mapPrintfulTemplateResponse raw
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
                let fetchProducts (queryParams: Printful.CatalogProductRequest.CatalogProductsQuery) : Async<CatalogProductResponse.CatalogProductsResponse> = async {
                    let url = queryString queryParams

                    // configureClient storeHeaders
                    configureClient printfulV2HttpClient storeHeaders

                    System.Console.WriteLine $"URL: {url}"
// + url
                    let! response = printfulV2HttpClient.GetAsync("catalog-products/" ) |> Async.AwaitTask
                    // 291981984 - Handsy
                    System.Console.WriteLine $"RESPONSE: {response}"

                    response.EnsureSuccessStatusCode() |> ignore

                    let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                    System.Console.WriteLine $"BODY: {body}"
                    
                    let raw = JsonSerializer.Deserialize<CatalogProductResponse.CatalogProduct.PrintfulCatalogProductResponse>(body)
                    return CatalogProductResponse.mapPrintfulResponse raw

                    // Deserialize into raw Printful schema
                    // let raw = JsonSerializer.Deserialize<Printful.CatalogProduct.PrintfulStoreProductResponse>(body)
                    // Map to your clean domain model
                    // return Printful.CatalogProduct.mapPrintfulStoreResponse raw
                }
            
            // module CatalogVariant =
            //     let fetchVariant (variantId:int) : Async<SharedShopDomain.CatalogVariant list> = async {
            //         // let url = $"store/variants/%d{variantId}"
            //         let url = $"store/products/variants/637a618f21ce44"
            //         // let url = $"store/products/637a618f21ce44"
            //         System.Console.WriteLine url
            //         let! response = printfulV1HttpClient.GetAsync(url) |> Async.AwaitTask
            //         System.Console.WriteLine $"RESPONSE: {response}"
            //         // response.EnsureSuccessStatusCode() |> ignore
            //         let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            //         System.Console.WriteLine $"BODY: {body}"
            //         let parsed = JsonSerializer.Deserialize<Api.Printful.ProductVariant.PrintfulVariantsResponse>(body)
            //         // let parsed = JsonSerializer.Deserialize<Api.Printful.ProductVariant.PrintfulVariantsResponse>(body)
            //         return Api.Printful.ProductVariant.mapVariants parsed
            //     }

        let private productApi : ProductApi = {
            getProducts = Printful.CatalogProduct.fetchProducts
            getProductTemplates = Printful.CatalogProduct.fetchProductTemplates
            // getProductVariants = Printful.CatalogVariant.fetchVariant
        }
        let handler =
            Remoting.createApi()
            |> Remoting.withRouteBuilder (fun typeName methodName ->
                sprintf "/api/products/%s" methodName)
            |> Remoting.fromValue productApi
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
        ])
    memory_cache
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main _ =
    run app
    0