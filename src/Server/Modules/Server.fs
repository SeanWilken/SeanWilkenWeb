module Server

open Saturn
open Giraffe
open PrintfulService.PrintfulApi
open ArtGalleryService
open ShopService
open EnvService.ArgLoader
open EnvService.EnvLoader
open Microsoft.AspNetCore.Http

let private isFileRequest (path: PathString) =
    let p = path.Value
    not (isNull p) && p.Contains(".")

let spaFallback : HttpHandler =
    fun next ctx ->
        if ctx.Request.Method <> "GET"
        then next ctx
        else
            let path = ctx.Request.Path
            // Don't hijack API routes
            if path.StartsWithSegments(PathString("/api"))
            then next ctx
            // Don't hijack assets (Vite build uses /assets/*)
            elif path.StartsWithSegments(PathString("/assets"))
            then next ctx
            // Don't hijack file requests (e.g. /favicon.ico, /robots.txt, /Resume.html)
            elif isFileRequest path
            then next ctx
            else htmlFile "public/index.html" next ctx


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