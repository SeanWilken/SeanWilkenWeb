module Server

open Saturn
open Giraffe
open PrintfulService.PrintfulApi

let app = application {
    use_router (
        choose [
            ProductAPI.handler
            CheckoutAPI.handler
        ])
    memory_cache
    use_static "public"
    use_gzip
}

[<EntryPoint>]
let main _ =
    run app
    0