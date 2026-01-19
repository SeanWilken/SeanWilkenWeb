module Gmail

open System
open System.Net
open System.Net.Mail
open System.Globalization

type GmailConfig = {
    Host     : string
    Port     : int
    Username : string
    Password : string
    FromName : string
    FromAddr : string
}

let loadConfig () =
    {
        Host     = Environment.GetEnvironmentVariable("GMAIL_SMTP_HOST")
        Port     = Environment.GetEnvironmentVariable("GMAIL_SMTP_PORT") |> int
        Username = Environment.GetEnvironmentVariable("GMAIL_USERNAME")
        Password = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD")
        FromName = Environment.GetEnvironmentVariable("GMAIL_FROM_NAME")
        FromAddr = Environment.GetEnvironmentVariable("GMAIL_FROM_ADDRESS")
    }

let sendEmail
    (toAddress : string)
    (subject   : string)
    (htmlBody  : string)
    =
    async {
        let cfg = loadConfig()

        use msg = new MailMessage()
        msg.From <- MailAddress(cfg.FromAddr, cfg.FromName)
        msg.To.Add(toAddress)
        msg.Subject <- subject
        msg.Body <- htmlBody
        msg.IsBodyHtml <- true

        use client = new SmtpClient(cfg.Host, cfg.Port)
        client.EnableSsl <- true
        client.UseDefaultCredentials <- false
        client.Credentials <- NetworkCredential(cfg.Username, cfg.Password)

        try
            do! client.SendMailAsync(msg) |> Async.AwaitTask
        with ex ->
            printfn "❌ Email failed: %s" ex.Message
            // raise ex
            ()
    }

type OrderEmailVars = {
    BrandLogoUrl: string option
    BrandName: string
    BrandTagline: string
    BrandSiteUrl: string
    HeroImageUrl: string option
    SupportEmail: string

    OrderDate: string
    CustomerFirstname: string

    AppOrderId: string
    PrintfulOrderId: string
    StripePaymentId: string
    OrderLookupUrl: string

    ItemCount: int
    ItemRowsHtml: string

    CurrencySymbol: string
    Subtotal: string
    Shipping: string
    Tax: string
    Total: string

    ShipName: string
    ShipLine1: string
    ShipLine2: string
    ShipCity: string
    ShipState: string
    ShipZip: string
    ShipCountry: string
    ShipEmail: string 
}

let htmlEncode (s: string) =
    System.Net.WebUtility.HtmlEncode(s)

let currencySymbol = function
    | "USD" -> "$"
    | "EUR" -> "€"
    | "GBP" -> "£"
    | c -> c + " "

let fmtDecimal (d: decimal) =
    d.ToString("0.00", CultureInfo.InvariantCulture)

let money (currencySymbol: string) (v: decimal) =
    // keep simple/consistent; you can format by culture later
    $"{currencySymbol}{fmtDecimal v}"

let replaceToken (token: string) (value: string) (html: string) =
    html.Replace("{{" + token + "}}", value)

let replaceTokenOpt token (valueOpt: string option) (html: string) =
    let value = valueOpt |> Option.defaultValue ""
    replaceToken token value html

let renderOrderConfirmationTemplate (templateHtml: string) (v: OrderEmailVars) =
    templateHtml
    |> replaceTokenOpt "BRAND_LOGO_URL" v.BrandLogoUrl
    |> replaceToken "BRAND_NAME" v.BrandName
    |> replaceToken "BRAND_TAGLINE" v.BrandTagline
    |> replaceToken "BRAND_SITE_URL" v.BrandSiteUrl
    |> replaceTokenOpt "HERO_IMAGE_URL" v.HeroImageUrl
    |> replaceToken "SUPPORT_EMAIL" v.SupportEmail
    |> replaceToken "ORDER_DATE" v.OrderDate
    |> replaceToken "CUSTOMER_FIRSTNAME" v.CustomerFirstname
    |> replaceToken "APP_ORDER_ID" v.AppOrderId
    |> replaceToken "PRINTFUL_ORDER_ID" v.PrintfulOrderId
    |> replaceToken "STRIPE_PAYMENT_ID" v.StripePaymentId
    |> replaceToken "ORDER_LOOKUP_URL" v.OrderLookupUrl
    |> replaceToken "ITEM_COUNT" (string v.ItemCount)
    |> replaceToken "ITEM_ROWS_HTML" v.ItemRowsHtml
    |> replaceToken "CURRENCY_SYMBOL" v.CurrencySymbol
    |> replaceToken "SUBTOTAL" v.Subtotal
    |> replaceToken "SHIPPING" v.Shipping
    |> replaceToken "TAX" v.Tax
    |> replaceToken "TOTAL" v.Total
    |> replaceToken "SHIP_NAME" v.ShipName
    |> replaceToken "SHIP_LINE1" v.ShipLine1
    |> replaceToken "SHIP_LINE2" v.ShipLine2
    |> replaceToken "SHIP_CITY" v.ShipCity
    |> replaceToken "SHIP_STATE" v.ShipState
    |> replaceToken "SHIP_ZIP" v.ShipZip
    |> replaceToken "SHIP_COUNTRY" v.ShipCountry
    |> replaceToken "SHIP_EMAIL" v.ShipEmail

let formatOrderDate (dt: System.DateTime) =
    // keep it readable; adjust timezone if you want
    dt.ToString("MMM d, yyyy")

