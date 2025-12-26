module StripeService

open Stripe

module StripePayments =

    let createPaymentIntent (totalCents : int) (draftOrderId : string) =
        task {
            StripeConfiguration.ApiKey <- EnvService.EnvConfig.stripeKey

            let service = PaymentIntentService()

            let options = PaymentIntentCreateOptions(
                Amount = totalCents,
                Currency = "usd",
                Metadata = 
                    System.Collections.Generic.Dictionary<string, string>( dict [ "draft_order_id", draftOrderId ] )
            )

            let! intent = service.CreateAsync(options)
            return intent.ClientSecret, intent.Id
        }


