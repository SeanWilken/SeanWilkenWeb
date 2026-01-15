module EnvService

open System

[<RequireQualifiedAccess>]
module EnvConfig =
    let viteStripeKey = Environment.GetEnvironmentVariable("VITE_STRIPE_API_PK")
    let stripeKey = Environment.GetEnvironmentVariable("STRIPE_API_SK")
    let printfulKey = Environment.GetEnvironmentVariable("PRINTFUL_API_KEY")
    let printfulOAuthKey = Environment.GetEnvironmentVariable("PRINTFUL_OAUTH_KEY")
    let printfulStoreKey = Environment.GetEnvironmentVariable("PRINTFUL_STORE_ID")
    let mongoUrl = 
        match Environment.GetEnvironmentVariable("DATABASE_URL") with
        | null | "" -> "mongodb://localhost:27017/xeroeffort"
        | s -> s
    let elasticUrl = Environment.GetEnvironmentVariable("ELASTICSEARCH_URL")
    let gmailFrom = Environment.GetEnvironmentVariable "GMAIL_USERNAME"
    let gmailPass = Environment.GetEnvironmentVariable "GMAIL_APP_PASSWORD"
    let gmailFromName = Environment.GetEnvironmentVariable "GMAIL_FROM_NAME"
    let gmailSmtpHost = Environment.GetEnvironmentVariable "GMAIL_SMTP_HOST"
    let gmailSmtpPort = Environment.GetEnvironmentVariable "GMAIL_SMTP_PORT" |> int