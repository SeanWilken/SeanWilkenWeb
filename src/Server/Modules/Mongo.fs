module MongoService

open System
open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes


[<CLIMutable>]
type OrderDraftDocument = {
    [<BsonId>]
    [<BsonRepresentation(BsonType.String)>]
    Id: Guid

    /// Your GUID external_id that you also send to Printful
    [<BsonElement("draftExternalId")>]
    DraftExternalId: string

    /// e.g. "draft-requested", "draft-created", "draft-failed"
    [<BsonElement("status")>]
    Status: string

    /// JSON payload you sent to Printful (for debugging / replay)
    [<BsonElement("printfulRequestJson")>]
    PrintfulRequestJson: string

    /// Raw Printful response JSON (draft order or error)
    [<BsonElement("printfulResponseJson")>]
    PrintfulResponseJson: string option

    /// Optional Printful order id, once created
    [<BsonElement("printfulOrderId")>]
    PrintfulOrderId: Nullable<int>

    /// Optional Stripe PaymentIntent id
    [<BsonElement("stripePaymentIntentId")>]
    StripePaymentIntentId: string option

    [<BsonElement("createdAt")>]
    CreatedAt: DateTime

    [<BsonElement("updatedAt")>]
    UpdatedAt: DateTime
}


open MongoDB.Driver
open EnvService

module OrderDraftStorage =

   

    let private mongoUrl  = MongoUrl EnvConfig.mongoUrl
    let private client    = new MongoClient(EnvConfig.mongoUrl)
    let private dbName    =
        if String.IsNullOrWhiteSpace mongoUrl.DatabaseName then "xeroeffort"
        else mongoUrl.DatabaseName

    let private db =
        client.GetDatabase(dbName)

    let private drafts =
        db.GetCollection<OrderDraftDocument>("order_drafts")

    /// Insert a new draft record when you’re about to call Printful
    let insertDraft
        (draftExternalId: string)
        (printfulRequestJson: string)
        : Async<OrderDraftDocument> =
        async {
            System.Console.WriteLine $"Connection String: {EnvConfig.mongoUrl}"
            let now = DateTime.UtcNow
            let doc =
                { Id = Guid.NewGuid()
                  DraftExternalId = draftExternalId
                  Status = "draft-requested"
                  PrintfulRequestJson = printfulRequestJson
                  PrintfulResponseJson = None
                  PrintfulOrderId = Nullable()
                  StripePaymentIntentId = None
                  CreatedAt = now
                  UpdatedAt = now }

            do! drafts.InsertOneAsync(doc) |> Async.AwaitTask
            
            return doc
        }

    /// Update the draft after Printful responds (success or failure)
    let setDraftPrintfulResult
        (draftExternalId: string)
        (status: string)
        (responseJson: string)
        (printfulOrderId: int option)
        : Async<unit> =
        async {
            let filter =
                Builders<OrderDraftDocument>.Filter
                    .Eq((fun d -> d.DraftExternalId), draftExternalId)

            let updates =
                Builders<OrderDraftDocument>.Update
                    .Set((fun d -> d.Status), status)
                    .Set((fun d -> d.PrintfulResponseJson), Some responseJson)
                    .Set((fun d -> d.UpdatedAt), DateTime.UtcNow)
                    .Set(
                        (fun d -> d.PrintfulOrderId),
                        match printfulOrderId with
                        | Some id -> Nullable id
                        | None -> Nullable()
                    )

            let! _ = drafts.UpdateOneAsync(filter, updates) |> Async.AwaitTask

            return ()
        }

    /// When Stripe succeeds, attach the PaymentIntent id so you can
    /// find the draft from webhooks, etc.
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
