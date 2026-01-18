module Server

open Saturn
open Giraffe
open PrintfulService.PrintfulApi
open MongoDB.Driver



let spaFallback : HttpHandler =
    fun next ctx ->
        // Only handle GETs (so POST/PUT still 404 properly if not matched)
        if ctx.Request.Method = "GET" then
            // Serve the built index.html from the public folder
            htmlFile "public/index.html" next ctx
        else
            next ctx


module HealthAPI =
    let healthCheckHandler: HttpHandler =
        fun next ctx -> task {
            ctx.SetStatusCode 200
            ctx.WriteTextAsync "OK" |> Async.AwaitTask |> ignore
            return! next ctx
        }

    let handler =
        router {
            get "/api/health" healthCheckHandler
        }

let app = application {
    use_router (
        choose [
            ProductAPI.handler
            CheckoutAPI.handler
            HealthAPI.handler
            spaFallback
        ])
    memory_cache
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main args =
    match args with
    | [| "migrate" |] ->
        let conn = EnvService.EnvConfig.mongoUrl
        MigrationRunner.runMigrations conn
        exit 0
    | _ ->
        run app
        exit 0