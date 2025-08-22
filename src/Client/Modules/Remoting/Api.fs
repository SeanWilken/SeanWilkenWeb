module Client.Api

open Fable.Remoting.Client
open Shared

let paypalApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun typeName methodName -> sprintf "/api/%s/%s" typeName methodName)
    |> Remoting.buildProxy<Api.PaymentApi>

let productsApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun _ methodName -> sprintf "/api/products/%s" methodName)
    |> Remoting.buildProxy<Api.ProductApi>