module Server

open Saturn
open Giraffe
open PrintfulService.PrintfulApi
open ArtGalleryService
open ShopService
open EnvService.ArgLoader
open EnvService.EnvLoader

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
            PrintfulProductApi.handler
            CheckoutAPI.handler
            ArtGalleryAPI.handler
            ShopAPI.handler
            HealthAPI.handler
            spaFallback
        ])
    memory_cache
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main args =
    let argsList = args |> Array.toList

    getArg argsList "--env-file"
    |> Option.iter (fun path ->
        printfn $"[startup] Loading env: {path}"
        loadDotEnv path
    )

    argsList |> List.iter (fun x -> System.Console.WriteLine $"ARG: {x}")

    let pf1 = System.Environment.GetEnvironmentVariable("PRINTFUL_API_KEY")
    let pf2 = System.Environment.GetEnvironmentVariable("PRINTFUL_STORE_ID")

    printfn $"PRINTFUL_API_KEY len: {if isNull pf1 then -1 else pf1.Length}"
    printfn $"PRINTFUL_STORE_ID len: {if isNull pf2 then -1 else pf2.Length}"

    match argsList with
    | "sync-runner" :: _ ->
        SyncRunner.runSync ()
        exit 0
    | "migrate" :: _ ->
        MigrationRunner.runMigrations ()
        exit 0
    | _ ->
        run app
        exit 0