module Bindings.Stripe

open Fable.Core
open Fable.Core.JsInterop

// ---------- Core Stripe types ----------

module StripePayment = 

    [<AllowNullLiteral>]
    type StripeError =
        abstract message: string option
        abstract ``type``: string

    [<AllowNullLiteral>]
    type PaymentIntent =
        abstract id: string
        abstract status: string

    [<AllowNullLiteral>]
    type IStripeElement =
        /// Mounts the element into a DOM element. You can pass either a selector string ("#card-element")
        /// or a raw HTMLElement; we keep it simple and use the selector overload.
        abstract mount: domSelector: string -> unit
        abstract mount: element: Browser.Types.HTMLElement -> unit
        abstract destroy: unit -> unit

    [<AllowNullLiteral>]
    type IElements =
        abstract create: elementType: string * ?options: obj -> IStripeElement
        abstract getElement: elementType: string -> IStripeElement
        abstract submit: unit -> JS.Promise<obj>   // <-- add this

    [<AllowNullLiteral>]
    type StripeElementsOptions =
        abstract clientSecret: string option with get, set
        abstract appearance: obj option with get, set

    [<AllowNullLiteral>]
    type ConfirmCardPaymentMethod =
        abstract card: IStripeElement with get, set
        abstract billing_details: obj option with get, set

    [<AllowNullLiteral>]
    type ConfirmCardPaymentParams =
        abstract payment_method: ConfirmCardPaymentMethod with get, set

    [<AllowNullLiteral>]
    type ConfirmCardPaymentResult =
        abstract error: StripeError option
        abstract paymentIntent: PaymentIntent option

    [<AllowNullLiteral>]
    type ConfirmPaymentParams =
        abstract elements: IElements with get, set
        abstract clientSecret: string with get, set
        abstract confirmParams: obj option with get, set
        abstract redirect: string option with get, set

    [<AllowNullLiteral>]
    type ConfirmPaymentResult =
        abstract error: StripeError option
        abstract paymentIntent: PaymentIntent option

    [<AllowNullLiteral>]
    type IStripe =
        abstract elements: StripeElementsOptions -> IElements
        abstract confirmCardPayment:
            clientSecret: string *
            confirmParams: ConfirmCardPaymentParams ->
                JS.Promise<ConfirmCardPaymentResult>
        abstract confirmPayment:
            ConfirmPaymentParams ->
                JS.Promise<ConfirmPaymentResult>


    // ---------- loadStripe wrapper ----------

    // Uses the official helper from @stripe/stripe-js
    [<Import("loadStripe", "@stripe/stripe-js")>]
    let private loadStripeImpl (publishableKey: string) : JS.Promise<IStripe> = jsNative

    /// F#-friendly wrapper; you’ll usually call this once, e.g. from init cmd.
    let loadStripe (publishableKey: string) : JS.Promise<IStripe> =
        loadStripeImpl publishableKey
