module ArtGalleryService

module ArtGalleryAPI =
    open Giraffe
    open Fable.Remoting.Server
    open Fable.Remoting.Giraffe
    open MongoService


    let private artGalleryApi : Shared.Api.ArtGalleryApi = {
        GetGallery = ArtGalleryStorage.getArtGalleryCollection
    }

    let handler : HttpHandler =
        Remoting.createApi()
        |> Remoting.withRouteBuilder (fun typeName methodName ->
                sprintf "/api/art/%s" methodName) 
        |> Remoting.fromValue artGalleryApi
        |> Remoting.buildHttpHandler
