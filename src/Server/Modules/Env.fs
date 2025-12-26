module EnvService

open System

[<RequireQualifiedAccess>]
module EnvConfig =
    let stripeKey = Environment.GetEnvironmentVariable("STRIPE_API_SK_TEST")
    let printfulKey = Environment.GetEnvironmentVariable("PRINTFUL_API_KEY")
    let printfulOAuthKey = Environment.GetEnvironmentVariable("PRINTFUL_OAUTH_KEY")
    let mongoUrl = 
        match Environment.GetEnvironmentVariable("DATABASE_URL") with
        | null | "" -> "mongodb://localhost:27017/xeroeffort"
        | s -> s
    let elasticUrl = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL")
