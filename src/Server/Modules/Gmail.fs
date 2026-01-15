module Gmail

open System
open System.Net
open System.Net.Mail

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



// let sendConfirmationEmailHtml
    
//     brandLogoUrl
//     brandName
//     orderDate
//     customerFirstName
//     heroImageUrl
    
//     appOrderId
//     printfulOrderId
//     stripePaymentId
//     orderLookupUrl
    
//     itemCount
//     itemRowsHtml
    
//     currencySymbol
//     subtotal
//     shipping
//     tax
//     total
    
//     shipName
//     shipLine1
//     shipLine2
//     shipCity
//     shipState
//     shipZip
//     shipCountry
//     shipEmail
    
//     supportEmail
//     brandTagline
//     brandSiteUrl









// <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="margin:0 0 12px 0;">
//   <tr>
//     <td style="width:76px; vertical-align:top;">
//       <img src="{{ITEM_IMAGE_URL}}"
//            alt="{{ITEM_NAME}}"
//            width="68"
//            height="68"
//            style="display:block; width:68px; height:68px; object-fit:cover; border-radius:10px; border:1px solid #ededed;" />
//     </td>
//     <td style="vertical-align:top; padding-left:12px;">
//       <div style="font-size:14px; font-weight:700; line-height:1.2;">{{ITEM_NAME}}</div>
//       <div style="font-size:12px; color:#666; margin-top:6px; line-height:1.4;">
//         {{ITEM_COLOR}} • {{ITEM_SIZE}} • Qty {{ITEM_QTY}}
//       </div>
//       <div style="font-size:12px; color:#111; margin-top:6px;">
//         {{CURRENCY_SYMBOL}}{{ITEM_LINE_TOTAL}}
//       </div>
//     </td>
//   </tr>
// </table>







// <!doctype html>
// <html>
// <head>
//     <meta charset="utf-8" />
//     <meta name="viewport" content="width=device-width" />
//     <title>Order Confirmation</title>
// </head>

// <body style="margin:0; padding:0; background:#f6f6f6; font-family: Arial, Helvetica, sans-serif; color:#111;">
//     <!-- Outer wrapper -->
//     <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="background:#f6f6f6; padding:24px 0;">
//     <tr>
//         <td align="center" style="padding: 0 16px;">
//         <!-- Card -->
//         <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="600"
//             style="width:100%; max-width:600px; background:#ffffff; border:1px solid #eaeaea; border-radius:16px; overflow:hidden;">

//             <!-- Header / Brand -->
//             <tr>
//             <td style="background:#0b0b0b; color:#fff; padding:24px;">
//                 <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0">
//                 <tr>
//                     <td align="left" style="vertical-align:middle;">
//                     <!-- Logo image (optional) -->
//                     <!-- If you don't have a logo URL, you can remove this img -->
//                     <img src="{{BRAND_LOGO_URL}}"
//                         alt="{{BRAND_NAME}}"
//                         width="120"
//                         style="display:block; border:0; outline:none; text-decoration:none; max-width:120px; height:auto;" />
//                     </td>
//                     <td align="right" style="vertical-align:middle;">
//                     <div style="font-size:12px; letter-spacing:0.12em; text-transform:uppercase; opacity:0.85;">
//                         Order confirmation
//                     </div>
//                     <div style="font-size:12px; opacity:0.75; margin-top:4px;">
//                         {{ORDER_DATE}}
//                     </div>
//                     </td>
//                 </tr>
//                 </table>

//                 <div style="margin-top:18px;">
//                 <div style="font-size:26px; font-weight:700; line-height:1.2;">
//                     Thanks, {{CUSTOMER_FIRSTNAME}}.
//                 </div>
//                 <div style="font-size:14px; opacity:0.85; margin-top:8px; line-height:1.5;">
//                     We've received your order and it's queued for fulfillment. You'll get another email when it ships.
//                 </div>
//                 </div>

//                 <!-- Hero / banner image (optional) -->
//                 <div style="margin-top:18px;">
//                 <img src="{{HERO_IMAGE_URL}}"
//                     alt="Order banner"
//                     width="552"
//                     style="display:block; width:100%; max-width:552px; height:auto; border-radius:12px; border:0;" />
//                 </div>
//             </td>
//             </tr>

//             <!-- Order Summary -->
//             <tr>
//             <td style="padding:24px;">
//                 <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0">
//                 <tr>
//                     <td align="left" style="vertical-align:top;">
//                     <div style="font-size:14px; font-weight:700; letter-spacing:0.08em; text-transform:uppercase;">
//                         Order summary
//                     </div>
//                     <div style="font-size:12px; color:#666; margin-top:6px;">
//                         Order ID: <strong style="color:#111;">{{APP_ORDER_ID}}</strong><br/>
//                         Printful: <strong style="color:#111;">{{PRINTFUL_ORDER_ID}}</strong><br/>
//                         Payment: <strong style="color:#111;">{{STRIPE_PAYMENT_ID}}</strong>
//                     </div>
//                     </td>
//                     <td align="right" style="vertical-align:top;">
//                     <a href="{{ORDER_LOOKUP_URL}}"
//                         style="display:inline-block; padding:10px 14px; background:#111; color:#fff; text-decoration:none; border-radius:10px; font-size:12px; letter-spacing:0.08em; text-transform:uppercase;">
//                         Track order
//                     </a>
//                     </td>
//                 </tr>
//                 </table>

//                 <!-- Divider -->
//                 <div style="height:1px; background:#ededed; margin:18px 0;"></div>

//                 <!-- Items -->
//                 <div style="font-size:12px; color:#666; margin-bottom:10px;">
//                 Items ({{ITEM_COUNT}})
//                 </div>

//                 {{ITEM_ROWS_HTML}}

//                 <!-- Divider -->
//                 <div style="height:1px; background:#ededed; margin:18px 0;"></div>

//                 <!-- Totals -->
//                 <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="font-size:14px;">
//                 <tr>
//                     <td style="color:#666; padding:4px 0;">Subtotal</td>
//                     <td align="right" style="padding:4px 0;">{{CURRENCY_SYMBOL}}{{SUBTOTAL}}</td>
//                 </tr>
//                 <tr>
//                     <td style="color:#666; padding:4px 0;">Shipping</td>
//                     <td align="right" style="padding:4px 0;">{{CURRENCY_SYMBOL}}{{SHIPPING}}</td>
//                 </tr>
//                 <tr>
//                     <td style="color:#666; padding:4px 0;">Tax</td>
//                     <td align="right" style="padding:4px 0;">{{CURRENCY_SYMBOL}}{{TAX}}</td>
//                 </tr>
//                 <tr>
//                     <td style="padding:10px 0; font-weight:700; border-top:1px solid #ededed;">Total</td>
//                     <td align="right" style="padding:10px 0; font-weight:700; border-top:1px solid #ededed;">
//                     {{CURRENCY_SYMBOL}}{{TOTAL}}
//                     </td>
//                 </tr>
//                 </table>
//             </td>
//             </tr>

//             <!-- Shipping address / support -->
//             <tr>
//             <td style="padding:0 24px 24px 24px;">
//                 <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0"
//                     style="background:#fafafa; border:1px solid #ededed; border-radius:12px;">
//                 <tr>
//                     <td style="padding:16px;">
//                     <div style="font-size:12px; font-weight:700; letter-spacing:0.08em; text-transform:uppercase;">
//                         Shipping to
//                     </div>
//                     <div style="font-size:13px; color:#333; margin-top:8px; line-height:1.5;">
//                         {{SHIP_NAME}}<br/>
//                         {{SHIP_LINE1}}<br/>
//                         {{SHIP_LINE2}}<br/>
//                         {{SHIP_CITY}}, {{SHIP_STATE}} {{SHIP_ZIP}}<br/>
//                         {{SHIP_COUNTRY}}<br/>
//                         <span style="color:#666;">{{SHIP_EMAIL}}</span>
//                     </div>
//                     </td>
//                     <td style="padding:16px; vertical-align:top;" align="right">
//                     <div style="font-size:12px; font-weight:700; letter-spacing:0.08em; text-transform:uppercase;">
//                         Need help?
//                     </div>
//                     <div style="font-size:13px; color:#333; margin-top:8px; line-height:1.5;">
//                         Reply to this email or contact us:<br/>
//                         <a href="mailto:{{SUPPORT_EMAIL}}" style="color:#111; text-decoration:underline;">{{SUPPORT_EMAIL}}</a>
//                     </div>
//                     </td>
//                 </tr>
//                 </table>

//                 <div style="font-size:11px; color:#666; margin-top:14px; line-height:1.5;">
//                 Keep this email for your records. To look up your order later, use:
//                 <strong>{{APP_ORDER_ID}}</strong> + your email address.
//                 </div>
//             </td>
//             </tr>

//             <!-- Footer -->
//             <tr>
//             <td style="padding:18px 24px; background:#ffffff; border-top:1px solid #ededed;">
//                 <div style="font-size:11px; color:#777; line-height:1.5;">
//                 {{BRAND_NAME}} • {{BRAND_TAGLINE}}<br/>
//                 <a href="{{BRAND_SITE_URL}}" style="color:#111; text-decoration:underline;">{{BRAND_SITE_URL}}</a>
//                 </div>
//             </td>
//             </tr>

//         </table>
//         <!-- /Card -->
//         </td>
//     </tr>
//     </table>
// </body>
// </html>

let orderConfirmationEmail
    (orderId : string)
    (total   : decimal)
    (currency: string)
    =
    $"""
    <div style="font-family: sans-serif; max-width: 600px;">
        <h2>Thank you for your order!</h2>

        <p>Your order <strong>{orderId}</strong> has been confirmed.</p>

        <p>
            <strong>Total:</strong> {currency} {total:F2}
        </p>

        <p>
            You can track your order anytime using your email address.
        </p>

        <hr />
        <p style="font-size: 12px; color: #666;">
            Xero Effort
        </p>
    </div>
    """

