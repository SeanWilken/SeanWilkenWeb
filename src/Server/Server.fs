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

module Api =
    let http = new HttpClient()

    module Product =

        module Printful =
            open System.Net.Http.Headers

            let private printfulHttpClient =
                    new HttpClient(BaseAddress = System.Uri("https://api.printful.com/v2/"))

            do
                // Authentication (use your API key)
                printfulHttpClient.DefaultRequestHeaders.Authorization <-
                    // let apiKey =
                    //     match System.Environment.GetEnvironmentVariable("PRINTFUL_API_KEY") with
                    //     | a when a = null -> "<YOUR_API_KEY>"
                    //     | str ->  str
                    // AuthenticationHeaderValue("Bearer", apiKey)
                    AuthenticationHeaderValue("Bearer", "CtxfvCP31MacEZMVh8th76TwH2IflQBoFN6viKrE")

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
                let fetchProducts (queryParams: Printful.CatalogProductRequest.CatalogProductsQuery) : Async<Printful.CatalogProduct.CatalogResponseDTO> = async {
                    let url = queryString queryParams

                    System.Console.WriteLine $"URL: {url}"

                    let! response = printfulHttpClient.GetAsync("catalog-products" + url) |> Async.AwaitTask

                    System.Console.WriteLine $"RESPONSE: {response}"

                    // response.EnsureSuccessStatusCode() |> ignore

                    let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                    
                    // Deserialize into raw Printful schema
                    let raw = JsonSerializer.Deserialize<Printful.CatalogProduct.PrintfulCatalogProductResponse>(body)

                    // Map to your clean domain model
                    return Printful.CatalogProduct.mapPrintfulResponse raw
                }
            
            module CatalogVariant =
                let fetchVariant (variantId:int) : Async<SharedShopDomain.CatalogVariant list> = async {
                    let url = $"products/variants/%d{variantId}"
                    let! response = printfulHttpClient.GetAsync(url) |> Async.AwaitTask
                    response.EnsureSuccessStatusCode() |> ignore

                    let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                    let parsed = JsonSerializer.Deserialize<Api.Printful.ProductVariant.PrintfulVariantsResponse>(body)
                    return Api.Printful.ProductVariant.mapVariants parsed
                }

        let private productApi : ProductApi = {
            getProducts = Printful.CatalogProduct.fetchProducts
            getProductVariants = Printful.CatalogVariant.fetchVariant
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