module Server

open Saturn
open Giraffe
open PrintfulService.PrintfulApi

let spaFallback : HttpHandler =
    fun next ctx ->
        // Only handle GETs (so POST/PUT still 404 properly if not matched)
        if ctx.Request.Method = "GET" then
            // Serve the built index.html from the public folder
            htmlFile "public/index.html" next ctx
        else
            next ctx


let app = application {
    use_router (
        choose [
            ProductAPI.handler
            CheckoutAPI.handler
            spaFallback
        ])
    memory_cache
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main _ =
    run app
    0