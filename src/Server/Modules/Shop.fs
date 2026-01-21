module ShopService

open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open MongoDB.Driver
open Shared.Api
open MongoService
open MongoService.StoreProductStorage
open Shared.ShopProductViewer

module ShopAPI =

    let fetchStoreProducts () : Async<ShopProductListItem list> =
        async {
            let db = MongoDBClient.db
            let col = db.GetCollection<StoreProductDoc>("store_products")

            let! docs =
                col.Find(FilterDefinition<StoreProductDoc>.Empty)
                   .SortByDescending(fun d -> d.UpdatedAt)
                   .ToListAsync()
                |> Async.AwaitTask

            return
                docs
                |> Seq.map ShopMapping.mapListItem
                |> Seq.toList
        }

    let fetchStoreProductDetails (syncProductId: int) : Async<ShopProductDetails option> =
        async {
            let db = MongoDBClient.db
            let store = db.GetCollection<StoreProductDoc>("store_products")
            let cats = db.GetCollection<CatalogProductDoc>("catalog_products")

            let filter = Builders<StoreProductDoc>.Filter.Eq((fun p -> p.SyncProductId), syncProductId)
            let! doc = store.Find(filter).FirstOrDefaultAsync() |> Async.AwaitTask

            if isNull (box doc) then return None
            else
                let currency =
                    doc.Variants
                    |> Array.map (fun v -> v.Currency)
                    |> Array.tryFind (fun c -> not (System.String.IsNullOrWhiteSpace c))

                // map variants
                let variants =
                    doc.Variants
                    |> Array.toList
                    |> List.map ShopMapping.mapVariant

                // optional: pull blank doc
                let! blankOpt =
                    match doc.Summary.PrimaryCatalogProductId with
                    | None -> async { return None }
                    | Some pid ->
                        async {
                            let f = Builders<CatalogProductDoc>.Filter.Eq((fun c -> c.Id), pid)
                            let! c = cats.Find(f).FirstOrDefaultAsync() |> Async.AwaitTask
                            if isNull (box c) then return None
                            else
                                return 
                                    ShopMapping.mapCatalog c
                                    |> Some
                        }

                return Some {
                    SyncProductId = doc.SyncProductId
                    Name = doc.Name
                    ThumbnailUrl = doc.ThumbnailUrl
                    DesignKey = doc.DesignKey
                    Tags = doc.Tags |> Array.toList
                    Currency = currency

                    PriceMin = doc.Summary.PriceMin
                    PriceMax = doc.Summary.PriceMax
                    Colors = doc.Summary.Colors |> Array.toList
                    Sizes = doc.Summary.Sizes |> Array.toList

                    Variants = variants
                    Blank = blankOpt
                }
        }

    let private shopApi : ShopApi =
        { 
            GetProducts = fetchStoreProducts
            GetProductDetails = fetchStoreProductDetails
        }

    let handler : HttpHandler =
        Remoting.createApi()
        |> Remoting.withRouteBuilder (fun _ methodName -> sprintf "/api/shop/%s" methodName)
        |> Remoting.fromValue shopApi
        |> Remoting.buildHttpHandler
