module MongoService

open System
open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes
open MongoDB.Driver


module Migration =

    type MigrationRecord =
        { 
            Version: string
            Name: string
            AppliedAt: DateTime 
        }

    module MigrationStore =

        let private collection (db: IMongoDatabase) =
            db.GetCollection<BsonDocument>("migrations")

        let getAppliedVersions (db: IMongoDatabase) =
            (collection db)
                .Find(FilterDefinition<BsonDocument>.Empty)
                .ToList()
            |> Seq.map (fun d -> d.["version"].AsString)
            |> Set.ofSeq

        let recordApplied (db: IMongoDatabase) (version: string) (name: string) =
            let doc =
                BsonDocument([
                    BsonElement("version", version)
                    BsonElement("name", name)
                    BsonElement("appliedAt", BsonDateTime(System.DateTime.UtcNow))
                ])

            (collection db)
                .InsertOne doc

    module Migration001 =
    
        let version = "001"
        let name = "init-drafts-collection"

        let up (db: IMongoDatabase) =
            let collections = db.ListCollectionNames().ToList()
            if not (collections.Contains("order_drafts"))
            then
                db.CreateCollection("order_drafts")

                let col = db.GetCollection<BsonDocument>("order_drafts")

                [ 
                    Builders.IndexKeys.Ascending("draftExternalId"), CreateIndexOptions(Unique = true, Name = "draftExternalId_unique")
                    Builders.IndexKeys.Ascending("stripePaymentIntentId"), CreateIndexOptions(Name = "stripe_pi_idx")
                    Builders.IndexKeys.Combine(Builders.IndexKeys.Ascending("customerEmail"), Builders.IndexKeys.Descending("createdAt")), CreateIndexOptions(Name = "customer_email_created_idx")
                    Builders.IndexKeys.Descending("createdAt"), CreateIndexOptions(Name = "createdAt_desc_idx")
                ]
                |> List.iter ( fun (keys, opts) -> col.Indexes.CreateOne(CreateIndexModel(keys, opts)) |> ignore )

    module Migration002 =
    
        let version = "002"
        let name = "init-products-collection"

        let up (db: IMongoDatabase) =
            let collections = db.ListCollectionNames().ToList()

            let newCollections = [ 
                "store_products", [("syncProductId", true); ("externalId", false); ("designKey", false); ("updatedAt", false)]
                "catalog_products", [ ("updatedAt", false) ]
                "store_mockups", []
                "users", [("email", true)]
                "art_gallery", [("designKey", true)]
            ]

            newCollections
            |> List.iter (fun (coll, indexes) ->

                if not (collections.Contains coll) 
                then db.CreateCollection coll
            
                let col = db.GetCollection<BsonDocument> coll
            
                indexes
                |> List.iter (
                    fun (idx, isUnique)->
                        let keys = Builders<BsonDocument>.IndexKeys.Ascending idx
                        let opts = 
                            if isUnique 
                            then CreateIndexOptions(Unique = isUnique, Name = $"{idx}_unique")
                            else CreateIndexOptions(Name = $"{idx}_idx")
                        col.Indexes.CreateOne(CreateIndexModel(keys, opts)) |> ignore
                )
            )


[<CLIMutable>]
type OrderDraftDocument = {
    [<BsonId>]
    [<BsonRepresentation(BsonType.String)>]
    Id: Guid

    [<BsonElement("draftExternalId")>]
    DraftExternalId: string

    [<BsonElement("status")>]
    Status: string

    [<BsonElement("printfulRequestJson")>]
    PrintfulRequestJson: string

    [<BsonElement("printfulResponseJson")>]
    PrintfulResponseJson: string option

    [<BsonElement("printfulOrderId")>]
    PrintfulOrderId: Nullable<int>

    [<BsonElement("stripePaymentIntentId")>]
    StripePaymentIntentId: string option

    [<BsonElement("stripePaymentStatus")>]
    StripePaymentStatus: string option

    [<BsonElement("paymentConfirmedAt")>]
    PaymentConfirmedAt: Nullable<DateTime>

    [<BsonElement("customerFirstName")>]
    CustomerFirstName: string option

    [<BsonElement("customerLastName")>]
    CustomerLastName: string option

    [<BsonElement("customerEmail")>]
    CustomerEmail: string option

    /// Currency of the retail total (e.g. "USD")
    [<BsonElement("orderCurrency")>]
    OrderCurrency: string option

    /// Retail order total in that currency
    [<BsonElement("orderTotal")>]
    OrderTotal: decimal

    [<BsonElement("createdAt")>]
    CreatedAt: DateTime

    [<BsonElement("updatedAt")>]
    UpdatedAt: DateTime
}

module MongoDBClient =

    open MongoDB.Driver
    open EnvService

    let private mongoUrl  = 
        EnvConfig.getConfiguredEnvironment ()
        |> fun envConfig -> MongoUrl envConfig.MongoDbConnectionString

    let private client    = 
        EnvConfig.getConfiguredEnvironment ()
        |> fun envConfig -> new MongoClient(envConfig.MongoDbConnectionString)

    let private dbName    =
        if String.IsNullOrWhiteSpace mongoUrl.DatabaseName then "xeroeffort"
        else mongoUrl.DatabaseName

    let db = client.GetDatabase dbName
        

module OrderDraftStorage =
    open MongoDBClient

    let private drafts =
        db.GetCollection<OrderDraftDocument>("order_drafts")

    // -------------------------------------------------------------
    // INSERT
    // -------------------------------------------------------------

    /// Existing helper – still works, but with empty customer/payment fields.
    /// This is what your current createDraftOrder uses.
    let insertDraft
        (draftExternalId: string)
        (currency: string option)
        (orderTotal: decimal)
        (printfulRequestJson: string)
        : Async<OrderDraftDocument> =
        async {
            System.Console.WriteLine $"[Mongo] insertDraft draftExternalId={draftExternalId}"

            let now = DateTime.UtcNow

            let doc: OrderDraftDocument =
                { Id = Guid.NewGuid()
                  DraftExternalId = draftExternalId
                  Status = "draft-requested"
                  PrintfulRequestJson = printfulRequestJson
                  PrintfulResponseJson = None
                  PrintfulOrderId = Nullable()
                  OrderCurrency = currency
                  OrderTotal = orderTotal
                  StripePaymentIntentId = None
                  StripePaymentStatus = None
                  PaymentConfirmedAt = Nullable()
                  CustomerFirstName = None
                  CustomerLastName = None
                  CustomerEmail = None
                  CreatedAt = now
                  UpdatedAt = now }

            do! drafts.InsertOneAsync(doc) |> Async.AwaitTask
            return doc
        }

    /// New helper – same as insertDraft but lets you attach customer info up front.
    /// You can switch your createDraftOrder to this later if you want.
    let insertDraftWithCustomer
        (draftExternalId: string)
        (printfulRequestJson: string)
        (currency: string option)
        (orderTotal: decimal)
        (customerFirstName: string option)
        (customerLastName: string option)
        (customerEmail: string option)
        : Async<OrderDraftDocument> =
        async {
            System.Console.WriteLine $"[Mongo] insertDraftWithCustomer draftExternalId={draftExternalId}"

            let now = DateTime.UtcNow

            let doc: OrderDraftDocument =
                { Id = Guid.NewGuid()
                  DraftExternalId = draftExternalId
                  Status = "draft-requested"
                  PrintfulRequestJson = printfulRequestJson
                  PrintfulResponseJson = None
                  PrintfulOrderId = Nullable()
                  StripePaymentIntentId = None
                  StripePaymentStatus = None
                  PaymentConfirmedAt = Nullable()
                  OrderCurrency = currency
                  OrderTotal = orderTotal
                  CustomerFirstName = customerFirstName
                  CustomerLastName = customerLastName
                  CustomerEmail = customerEmail
                  CreatedAt = now
                  UpdatedAt = now }

            do! drafts.InsertOneAsync(doc) |> Async.AwaitTask
            return doc
        }

    // -------------------------------------------------------------
    // PRINTFUL RESULT
    // -------------------------------------------------------------

    /// Update the draft after Printful responds (success or failure)
    let setDraftPrintfulResult
        (draftExternalId: string)
        (status: string)
        (responseJson: string option)
        (printfulOrderId: int option)
        : Async<bool> =
        async {
            let filter =
                Builders<OrderDraftDocument>.Filter
                    .Eq((fun d -> d.DraftExternalId), draftExternalId)

            let update =
                Builders<OrderDraftDocument>.Update
                    .Set((fun d -> d.Status), status)
                    .Set((fun d -> d.PrintfulResponseJson), responseJson)
                    .Set((fun d -> d.UpdatedAt), DateTime.UtcNow)
                    .Set(
                        (fun d -> d.PrintfulOrderId),
                        match printfulOrderId with
                        | Some id -> Nullable id
                        | None -> Nullable()
                    )

            let! result =
                drafts.UpdateOneAsync(filter, update)
                |> Async.AwaitTask

            return result.ModifiedCount = 1L
        }

    // -------------------------------------------------------------
    // STRIPE PAYMENT INTENT ATTACHMENT
    // -------------------------------------------------------------

    /// When Stripe succeeds in creating the PaymentIntent, attach its id
    let setStripePaymentIntent
        (draftExternalId: string)
        (paymentIntentId: string)
        : Async<unit> =
        async {
            let filter =
                Builders<OrderDraftDocument>.Filter
                    .Eq((fun d -> d.DraftExternalId), draftExternalId)

            let updates =
                Builders<OrderDraftDocument>.Update
                    .Set((fun d -> d.StripePaymentIntentId), Some paymentIntentId)
                    .Set((fun d -> d.UpdatedAt), DateTime.UtcNow)

            let! _ = drafts.UpdateOneAsync(filter, updates) |> Async.AwaitTask
            return ()
        }

    // -------------------------------------------------------------
    // STRIPE PAYMENT CONFIRMATION (confirmPayment flow)
    // -------------------------------------------------------------

    /// Update status based on Stripe payment element callback (by PaymentIntent id).
    /// Typically called from your confirmPayment endpoint when the client
    /// reports that the PaymentIntent succeeded.
    let setStripePaymentStatusByIntentId
        (paymentIntentId: string)
        (stripeStatus: string)
        : Async<unit> =
        async {
            let filter =
                Builders<OrderDraftDocument>.Filter
                    .Eq((fun d -> d.StripePaymentIntentId), Some paymentIntentId)

            let now = DateTime.UtcNow

            let baseUpdate =
                Builders<OrderDraftDocument>.Update
                    .Set((fun d -> d.StripePaymentStatus), Some stripeStatus)
                    .Set((fun d -> d.UpdatedAt), now)

            // If succeeded, also mark our high-level status and set PaymentConfirmedAt
            let updates =
                match stripeStatus with
                | "succeeded"
                | "requires_action"  // you can adjust this mapping as you like
                | "processing" ->
                    baseUpdate
                        .Set((fun d -> d.Status), "payment-succeeded")
                        .Set((fun d -> d.PaymentConfirmedAt), Nullable now)
                | "canceled"
                | "requires_payment_method" ->
                    baseUpdate
                        .Set((fun d -> d.Status), "payment-failed")
                        .Set((fun d -> d.PaymentConfirmedAt), Nullable())
                | _ ->
                    // unknown / neutral status – don't touch PaymentConfirmedAt
                    baseUpdate

            let! _ = drafts.UpdateOneAsync(filter, updates) |> Async.AwaitTask
            return ()
        }

    /// Sometimes you may also want to set status by draft id (e.g. internal usage).
    let setStripePaymentStatusByDraftId
        (draftExternalId: string)
        (stripeStatus: string)
        : Async<unit> =
        async {
            let filter =
                Builders<OrderDraftDocument>.Filter
                    .Eq((fun d -> d.DraftExternalId), draftExternalId)

            let now = DateTime.UtcNow

            let baseUpdate =
                Builders<OrderDraftDocument>.Update
                    .Set((fun d -> d.StripePaymentStatus), Some stripeStatus)
                    .Set((fun d -> d.UpdatedAt), now)

            let updates =
                match stripeStatus with
                | "succeeded" ->
                    baseUpdate
                        .Set((fun d -> d.Status), "payment-succeeded")
                        .Set((fun d -> d.PaymentConfirmedAt), Nullable now)
                | "canceled"
                | "requires_payment_method" ->
                    baseUpdate
                        .Set((fun d -> d.Status), "payment-failed")
                        .Set((fun d -> d.PaymentConfirmedAt), Nullable())
                | _ ->
                    baseUpdate

            let! _ = drafts.UpdateOneAsync(filter, updates) |> Async.AwaitTask
            return ()
        }

    // -------------------------------------------------------------
    // ORDER CONFIRM (Printful /orders/{id}/confirm)
    // -------------------------------------------------------------

    /// After you've hit Printful's confirm endpoint successfully,
    /// you can mark the draft as "order-confirmed" and store the real Printful order id.
    let setOrderConfirmed
        (draftExternalId: string)
        (printfulOrderId: int)
        (status: string)
        : Async<unit> =
        async {
            let filter =
                Builders<OrderDraftDocument>.Filter
                    .Eq((fun d -> d.DraftExternalId), draftExternalId)

            let updates =
                Builders<OrderDraftDocument>.Update
                    .Set((fun d -> d.PrintfulOrderId), Nullable printfulOrderId)
                    .Set((fun d -> d.Status), status)  // e.g. "order-confirmed"
                    .Set((fun d -> d.UpdatedAt), DateTime.UtcNow)

            let! _ = drafts.UpdateOneAsync(filter, updates) |> Async.AwaitTask
            return ()
        }

    // -------------------------------------------------------------
    // LOOKUPS
    // -------------------------------------------------------------

    /// Convenience lookup by your draft external id
    let tryGetByDraftExternalId
        (draftExternalId: string)
        : Async<OrderDraftDocument option> =
        async {
            let filter =
                Builders<OrderDraftDocument>.Filter
                    .Eq((fun d -> d.DraftExternalId), draftExternalId)

            let! cursor = drafts.FindAsync(filter) |> Async.AwaitTask
            let! results = cursor.ToListAsync() |> Async.AwaitTask
            return results |> Seq.tryHead
        }

    /// Lookup by Stripe PaymentIntent id (useful for webhooks or confirmPayment)
    let tryGetByPaymentIntentId
        (paymentIntentId: string)
        : Async<OrderDraftDocument option> =
        async {
            let filter =
                Builders<OrderDraftDocument>.Filter
                    .Eq((fun d -> d.StripePaymentIntentId), Some paymentIntentId)

            let! cursor = drafts.FindAsync(filter) |> Async.AwaitTask
            let! results = cursor.ToListAsync() |> Async.AwaitTask
            return results |> Seq.tryHead
        }

    /// Find orders by customer email, optionally narrowing by "order id".
    /// You can define "order id" however you like – here we check:
    ///   - DraftExternalId
    ///   - StripePaymentIntentId
    let findByEmailAndOptionalOrderId
        (email: string)
        (orderId: string option)
        : Async<OrderDraftDocument list> =
        async {
            let builder = Builders<OrderDraftDocument>.Filter

            let emailFilter =
                builder.Eq((fun d -> d.CustomerEmail), Some email)

            let filter =
                match orderId with
                | None ->
                    emailFilter
                | Some oid ->
                    let matchDraftId =
                        builder.Eq((fun d -> d.DraftExternalId), oid)
                    let matchStripePi =
                        builder.Eq((fun d -> d.StripePaymentIntentId), Some oid)

                    builder.And(
                        emailFilter,
                        builder.Or(matchDraftId, matchStripePi)
                    )

            let! cursor =
                drafts.Find(filter)
                    .Sort(Builders<OrderDraftDocument>.Sort.Descending("createdAt"))
                    .ToCursorAsync()
                |> Async.AwaitTask

            let! results = cursor.ToListAsync() |> Async.AwaitTask
            return results |> Seq.toList
        }

module ArtGalleryStorage =
    open MongoDBClient
    
    [<CLIMutable>]
    type ArtGalleryDoc = { 
        [<BsonId; BsonRepresentation(BsonType.String)>]
        Id: Guid

        [<BsonElement("title")>]
        Title: string

        [<BsonElement("description")>]
        Description: string

        [<BsonElement("designKey")>]
        DesignKey: string

        [<BsonElement("imageUrl")>]
        ImageUrl: string

        [<BsonElement("cdnUrls")>]
        CdnUrls: string[]

        [<BsonElement("tags")>]
        Tags: string[]

        // explicit links (optional)
        [<BsonElement("linkedSyncProductIds")>]
        LinkedSyncProductIds: int[]

        [<BsonElement("createdAt")>]
        CreatedAt: DateTime

        [<BsonElement("updatedAt")>]
        UpdatedAt: DateTime
    }

    let getArtGalleryCollection () : Async<Shared.ArtGalleryViewer.ArtPiece list> =
        async {
            try
                let collection = db.GetCollection<ArtGalleryDoc>("art_gallery")

                let! galleryPieces =
                    collection
                        .Find(FilterDefinition<ArtGalleryDoc>.Empty)
                        .SortBy(fun d -> d.CreatedAt)
                        .ToListAsync()
                    |> Async.AwaitTask
                
                return
                    galleryPieces
                    |> Seq.toList
                    |> List.map (fun galleryDoc -> 
                        {
                            Id = galleryDoc.Id
                            DesignKey = galleryDoc.DesignKey
                            Title = galleryDoc.Title
                            Description = galleryDoc.Description
                            ImageUrl = galleryDoc.ImageUrl
                            CdnUrls = galleryDoc.CdnUrls |> Array.toList
                            Tags = galleryDoc.Tags |> Array.toList
                            LinkedSyncProductIds = galleryDoc.LinkedSyncProductIds |> Array.toList
                            CreatedAt = galleryDoc.CreatedAt.ToString("o")
                        }
                    )
            with ex ->
                System.Console.WriteLine $"[Mongo] Error fetching art gallery collection: {ex.Message}"
                return failwith ex.Message
        }

module StoreProductStorage =
    
    [<CLIMutable>]
    type StoreVariantDoc = {

        // ID's
        [<BsonElement("syncVariantId")>]
        SyncVariantId: int64

        [<BsonElement("externalId")>]
        ExternalId: string

        [<BsonElement("variantId")>]
        VariantId: int

        [<BsonElement("catalogProductId")>]
        CatalogProductId: int

        [<BsonElement("catalogVariantId")>]
        CatalogVariantId: int

        // Product Name
        [<BsonElement("name")>]
        Name: string

        // Product Details
        [<BsonElement("size")>]
        Size: string

        [<BsonElement("color")>]
        Color: string

        [<BsonElement("availability")>]
        Availability: string option

        [<BsonElement("sku")>]
        Sku: string option

        [<BsonElement("currency")>]
        Currency: string

        [<BsonElement("retailPrice")>]
        RetailPrice: decimal option

        [<BsonElement("imageUrl")>]
        ImageUrl: string option

        [<BsonElement("previewUrl")>]
        PreviewUrl: string option

        [<BsonElement("fileUrls")>]
        FileUrls: string[]
    }

    [<CLIMutable>]
    type StoreProductSummaryDoc = { 
        [<BsonElement("priceMin")>]
        PriceMin: decimal option
        
        [<BsonElement("priceMax")>]
        PriceMax: decimal option
        
        [<BsonElement("colors")>]
        Colors: string[]
        
        [<BsonElement("sizes")>]
        Sizes: string[]
        
        [<BsonElement("primaryCatalogProductId")>]
        PrimaryCatalogProductId: int option
        
        [<BsonElement("blankName")>] 
        BlankName: string option
        
        [<BsonElement("blankBrand")>]
        BlankBrand: string option

        [<BsonElement("blankModel")>]
        BlankModel: string option
        
        [<BsonElement("blankImage")>]
        BlankImage: string option
    }

    [<CLIMutable>]
    type StoreProductDoc = {
        [<BsonId; BsonRepresentation(BsonType.String)>]
        Id: Guid

        [<BsonElement("syncProductId")>]
        SyncProductId: int

        [<BsonElement("externalId")>]
        ExternalId: string option

        [<BsonElement("name")>]
        Name: string

        [<BsonElement("thumbnailUrl")>]
        ThumbnailUrl: string option

        [<BsonElement("synced")>] // ??
        Synced: int option

        [<BsonElement("variantCount")>] // ??
        VariantCount: int

        [<BsonElement("isIgnored")>]
        IsIgnored: bool option

        // For art/gallery linking
        [<BsonElement("designKey")>]
        DesignKey: string option

        [<BsonElement("tags")>]
        Tags: string[]

        [<BsonElement("catalogProductIds")>]
        CatalogProductIds: int[]

        [<BsonElement("summary")>]
        Summary: StoreProductSummaryDoc

        // full details
        [<BsonElement("variants")>]
        Variants: StoreVariantDoc[]

        [<BsonElement("createdAt")>]
        CreatedAt: DateTime

        [<BsonElement("updatedAt")>]
        UpdatedAt: DateTime
    }

    [<CLIMutable>]
    type CatalogColorDoc = { 
        [<BsonElement("name")>]
        Name: string
        
        [<BsonElement("value")>]
        Value: string option
    }   

    [<CLIMutable>]
    type CatalogTechniqueDoc = { 
        [<BsonElement("key")>]
        Key: string
        
        [<BsonElement("displayName")>]
        DisplayName: string option
    }

    [<CLIMutable>]
    type CatalogPlacementDoc = {
        [<BsonElement("placement")>]
        Placement: string

        [<BsonElement("displayName")>]
        DisplayName: string option
    }

    [<CLIMutable>]
    type CatalogOptionDoc =
        { 
            [<BsonElement("id")>]
            Id: string
            
            [<BsonElement("title")>]
            Title: string option
            
            [<BsonElement("type")>]
            Type: string option
        }

    [<CLIMutable>]
    type CatalogProductDoc = { 
        [<BsonId>]
        [<BsonElement("_id")>] 
        Id: int

        [<BsonElement("mainCategoryId")>]
        MainCategoryId: int
        
        [<BsonElement("type")>]
        Type: string

        [<BsonElement("name")>]
        Name: string
        
        [<BsonElement("brand")>]
        Brand: string option
        
        [<BsonElement("model")>]
        Model: string option

        [<BsonElement("image")>]
        Image: string

        [<BsonElement("variantCount")>]
        VariantCount: int
        
        [<BsonElement("isDiscontinued")>]
        IsDiscontinued: bool

        [<BsonElement("description")>]
        Description: string

        [<BsonElement("sizes")>]
        Sizes: string[]
        
        [<BsonElement("colors")>]
        Colors: CatalogColorDoc[]

        [<BsonElement("techniques")>]
        Techniques: CatalogTechniqueDoc[]
        
        [<BsonElement("placements")>]
        Placements: CatalogPlacementDoc[]
        
        [<BsonElement("productOptions")>]
        ProductOptions: CatalogOptionDoc[]

        [<BsonElement("updatedAt")>]
        UpdatedAt: DateTime 
    }



    module ShopMapping =

        open Shared.Api
        open Shared.ShopProductViewer

        let mapListItem (d: StoreProductDoc) : ShopProductListItem =
            // currency: pick the most common non-empty currency from variants, else None
            let currencyOpt =
                d.Variants
                |> Array.map (fun v -> v.Currency)
                |> Array.tryFind (fun c -> not (String.IsNullOrWhiteSpace c))

            { 
                SyncProductId = d.SyncProductId
                DesignKey = d.DesignKey
                Name = d.Name
                ThumbnailUrl = d.ThumbnailUrl

                PriceMin = d.Summary.PriceMin
                PriceMax = d.Summary.PriceMax
                Currency = currencyOpt

                Colors = d.Summary.Colors |> Array.toList
                Sizes = d.Summary.Sizes |> Array.toList

                BlankName = d.Summary.BlankName
                BlankBrand = d.Summary.BlankBrand
                BlankModel = d.Summary.BlankModel
                BlankImage = d.Summary.BlankImage
            }

        let mapVariant (v: StoreVariantDoc) : ShopVariant =
            { 
                SyncVariantId = v.SyncVariantId
                ExternalId = v.ExternalId
                VariantId = v.VariantId

                CatalogProductId = v.CatalogProductId
                CatalogVariantId = v.CatalogVariantId

                Name = v.Name
                Size = v.Size
                Color = v.Color
                Availability = v.Availability
                Sku = v.Sku

                Currency = v.Currency
                RetailPrice = v.RetailPrice

                ImageUrl = v.ImageUrl
                PreviewUrl = v.PreviewUrl
                FileUrls = v.FileUrls |> Array.toList
            }

        let mapCatalog (c: CatalogProductDoc) : ShopCatalogProduct =
            { 
                Id = c.Id
                Name = c.Name
                Brand = c.Brand
                Model = c.Model
                Image = c.Image
                Description = c.Description
                Sizes = c.Sizes |> Array.toList
                Colors = c.Colors |> Array.toList |> List.map (fun x -> { Name = x.Name; Value = x.Value })
                Techniques = c.Techniques |> Array.toList |> List.choose (fun t -> t.DisplayName) // or Key
                Placements = c.Placements |> Array.toList |> List.choose (fun p -> p.DisplayName) // or Placement
                ProductOptions =
                    c.ProductOptions
                    |> Array.toList
                    |> List.map (fun o ->
                        match o.Title, o.Type with
                        | Some t, Some ty -> $"{t} ({ty})"
                        | Some t, None -> t
                        | None, Some ty -> ty
                        | None, None -> o.Id) 
            }
