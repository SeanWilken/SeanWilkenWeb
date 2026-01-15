module MongoService

open System
open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes

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
