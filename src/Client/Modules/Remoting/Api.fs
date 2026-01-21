module Client.Api

open Fable.Remoting.Client
open Shared

let checkoutApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun typeName methodName -> sprintf "/api/%s/%s" typeName methodName)
    |> Remoting.buildProxy<Api.CheckoutApi>

let printfulProductApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun _ methodName -> sprintf "/api/printful/%s" methodName)
    |> Remoting.buildProxy<Api.PrintfulProductApi>

let artGalleryApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun _ methodName -> sprintf "/api/art/%s" methodName)
    |> Remoting.buildProxy<Api.ArtGalleryApi>

let shopApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun _ methodName -> sprintf "/api/shop/%s" methodName)
    |> Remoting.buildProxy<Api.ShopApi>
