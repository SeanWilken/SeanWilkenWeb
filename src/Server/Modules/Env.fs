module EnvService

open System
open System.IO

[<RequireQualifiedAccess>]
module EnvConfig =

    type EnvConfig = {
        ViteStripeKey : string
        StripeKey : string
        PrintfulKey : string
        PrintfulOAuthKey : string
        PrintfulStoreId : string
        MongoDbName : string
        MongoDbConnectionString : string
        GmailFrom : string
        GmailPass : string
        GmailFromName : string
        GmailSmtpHost : string
        GmailSmtpPort : int
    }

    let getConfiguredEnvironment () =
        {
            ViteStripeKey = Environment.GetEnvironmentVariable("VITE_STRIPE_API_PK")
            StripeKey = Environment.GetEnvironmentVariable("STRIPE_API_SK")
            PrintfulKey = Environment.GetEnvironmentVariable("PRINTFUL_API_KEY")
            PrintfulOAuthKey = Environment.GetEnvironmentVariable("PRINTFUL_OAUTH_KEY")
            PrintfulStoreId = Environment.GetEnvironmentVariable("PRINTFUL_STORE_ID")
            MongoDbName = 
                match Environment.GetEnvironmentVariable("MONGO_DB") with
                | null | "" -> "xeroeffort"
                | s -> s
            MongoDbConnectionString =
                match Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") with
                | null | "" -> "mongodb://xero:dev123@localhost:27017/"
                | s -> s
            GmailFrom = Environment.GetEnvironmentVariable "GMAIL_USERNAME"
            GmailPass = Environment.GetEnvironmentVariable "GMAIL_APP_PASSWORD"
            GmailFromName = Environment.GetEnvironmentVariable "GMAIL_FROM_NAME"
            GmailSmtpHost = Environment.GetEnvironmentVariable "GMAIL_SMTP_HOST"
            GmailSmtpPort =
                match Environment.GetEnvironmentVariable "GMAIL_SMTP_PORT" with
                | null | "" -> 587 // default
                | s -> int s
        }

module ArgLoader =

    let getArg args name =
        args
        |> List.tryFindIndex ((=) name)
        |> Option.bind (fun i -> args |> List.tryItem (i + 1))

module EnvLoader =

    let loadDotEnv (path: string) =
        if not (File.Exists path) then
            failwith $"Env file not found: {path}"

        File.ReadAllLines path
        |> Array.iter (fun line ->
            let line = line.Trim()
            if line = "" || line.StartsWith "#" then () else
            match line.Split('=', 2) with
            | [| key; value |] ->
                Environment.SetEnvironmentVariable(key.Trim(), value.Trim())
            | _ -> ()
        )


