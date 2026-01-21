module SyncRunner

open System
open System.Threading
open MongoDB.Driver
open MongoService.StoreProductStorage
open PrintfulService

type SyncArgs = { 
    Limit: int option
    ProductId: int option
}

let runPageParallel
    (maxConcurrency: int)
    (items: _ array)
    (work: _ -> Async<unit>)
    : Async<unit> =
    async {
        // simple semaphore to throttle parallelism
        use sem = new SemaphoreSlim(maxConcurrency)

        let! _ =
            items
            |> Array.map (fun item ->
                async {
                    do! sem.WaitAsync() |> Async.AwaitTask
                    try
                        do! work item
                    finally
                        sem.Release() |> ignore
                })
            |> Async.Parallel

        return ()
    }


let runSyncProducts (args: SyncArgs) (db: IMongoDatabase)  =
    async {
        let products = db.GetCollection<StoreProductDoc>("store_products")

        match args.ProductId with
        | Some pid ->
                let! raw = 
                    PrintfulApi.SyncProduct.fetchRawSyncProductDetails pid
                do! MongoHelpers.upsertFromRawDetails products raw
                printfn $"[sync-runner] Upserted product {pid}"
        | None ->
            let pageSize = 50
            let mutable offset = 0
            let mutable processed = 0
            let maxCount = args.Limit

            let keepGoing () =
                match maxCount with
                | None -> true
                | Some m -> processed < m

            while keepGoing() do
                let! page = 
                    PrintfulApi.SyncProduct.fetchRawSyncProductsPage 
                        None
                        None
                if page.result.Length = 0
                then
                    printfn "[sync-runner] Done (no more results)."
                    // break
                    offset <- Int32.MaxValue
                else
                    let maxConcurrency = 2  // tune: 4–8 is usually safe for Printful
                    let batch = page.result

                    // If you have a hard overall limit, slice the batch first
                    let batch =
                        match maxCount with
                        | None -> batch
                        | Some m ->
                            let remaining = max 0 (m - processed)
                            batch |> Array.truncate remaining

                    do!
                        runPageParallel maxConcurrency batch (fun s ->
                            async {
                                let! raw = PrintfulApi.SyncProduct.fetchRawSyncProductDetails s.id
                                do! MongoHelpers.upsertFromRawDetails products raw

                                let newCount = Interlocked.Increment(&processed)
                                printfn $"[sync-runner] Upserted {s.id} ({s.name}) (processed={newCount})"
                            })

                    offset <- offset + page.result.Length
    }

// TODO: args for non-full sync
let runSync () =
    let envConfig = EnvService.EnvConfig.getConfiguredEnvironment ()
    let client = new MongoClient(envConfig.MongoDbConnectionString)
    client.GetDatabase envConfig.MongoDbName
    |> runSyncProducts { Limit = None; ProductId = None }
    |> Async.RunSynchronously
    
    client.Dispose()
