module OrderConfirmation

open System
open System.Globalization
open Feliz
open Browser.Dom

// If you're using a Lucide binding, adjust these opens/usages as needed.
// open Feliz.Lucide
// open Bindings.LucideIcon

// ---------- Domain Types ----------

type OrderItemDetails =
    { kind: string
      quantity: int
      externalProductId: string
      syncProductId: int
      syncVariantId: int option
      catalogProductId: string option
      catalogVariantId: string option
      templateId: string option }

type OrderItemLine =
    { item: OrderItemDetails
      unitPrice: decimal
      currency: string
      lineTotal: decimal
      isValid: bool
      error: string option }

type OrderCosts =
    { shippingName: string
      subtotal: decimal
      shipping: decimal
      tax: decimal
      total: decimal }

type Shipment =
    { carrier: string
      service: string
      trackingNumber: string
      trackingUrl: string option
      shipDate: string
      items: int list }

type OrderData =
    { orderID: int
      internalId: string
      status: string
      shippingServiceName: string
      shipments: Shipment list
      orderItems: OrderItemLine list
      costs: OrderCosts }

type OrderConfirmationProps =
    { orderData: Shared.Api.Checkout.ConfirmOrderResponse }

// ---------- Helpers ----------

let private statusColor (status: string) =
    match status.ToLowerInvariant() with
    | "confirmed"  -> "text-emerald-600"
    | "processing" -> "text-blue-600"
    | "shipped"    -> "text-purple-600"
    | "delivered"  -> "text-green-600"
    | _            -> "text-gray-600"

let private formatDate (dateString: string) =
    let dt = DateTime.Parse(dateString, CultureInfo.InvariantCulture)
    dt.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture)

let private money (value: decimal) =
    value.ToString("0.00", CultureInfo.InvariantCulture)

// ---------- Component ----------

[<ReactComponent>]
let OrderConfirmation (props: OrderConfirmationProps) =
    let order = props.orderData

    let expandedShipment, setExpandedShipment = React.useState<int option>(None)
    let isVisible, setIsVisible = React.useState(false)

    // Run once on mount – for entry animation
    React.useEffect(
        (fun () -> setIsVisible true),
        [||]
    )

    let visibilityClass =
        if isVisible then "opacity-100 translate-y-0"
        else "opacity-0 translate-y-8"

    Html.div [
        // prop.className "min-h-screen bg-[#fafaf9]"

        prop.children [
            
            // Main content
            Html.div [
                // prop.className "max-w-5xl mx-auto px-6 py-16"

                // Success Header
                prop.children [
                    Html.div [
                        prop.className (
                            "flex items-center gap-6 mb-16 transition-all duration-1000 transform " +
                            visibilityClass
                        )
                        prop.children [
                            Html.div [
                                prop.className "flex-shrink-0 w-20 h-20 bg-linear-to-b from-emerald-400 to-emerald-600 flex items-center justify-center shadow-lg shadow-emerald-200 rounded-full"
                                prop.children [
                                    // Replace with your Lucide binding
                                    // Lucide.check [ Lucide.Size 40; Lucide.Class "text-white" ]
                                    Html.span [
                                        prop.className "w-10 h-10 text-white"
                                        prop.children [
                                            Html.i [ prop.className "lucide lucide-check w-10 h-10" ]
                                        ]
                                    ]
                                ]
                            ]
                            Html.div [
                                prop.children [
                                    Html.h1 [
                                        prop.className "text-5xl font-light tracking-tight text-gray-900 mb-2 cormorant-font"
                                        prop.text "Order Confirmed"
                                    ]
                                    Html.p [
                                        prop.className "text-gray-500 text-lg tracking-wide"
                                        prop.text "Thank you for your purchase"
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // Two-column layout
                    Html.div [
                        prop.className (
                            "grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8 transition-all duration-1000 delay-200 transform " +
                            visibilityClass
                        )

                        // Left column - Order details
                        prop.children [
                            Html.div [
                                prop.className "card bg-base-100 shadow-sm border border-gray-100 overflow-hidden"
                                prop.children [

                                    // Order Info Header
                                    Html.div [
                                        prop.className "px-8 py-6 border-b border-gray-100 bg-linear-to-b from-gray-50 to-white"
                                        prop.children [
                                            Html.div [
                                                prop.className "space-y-4"
                                                prop.children [

                                                    Html.div [
                                                        prop.children [
                                                            Html.p [
                                                                prop.className "text-xs tracking-wider text-gray-400 uppercase mb-1"
                                                                prop.text "Order Number"
                                                            ]
                                                            Html.p [
                                                                prop.className "text-lg font-medium text-gray-900"
                                                                prop.text (string order.OrderId)
                                                            ]
                                                        ]
                                                    ]

                                                    Html.div [
                                                        prop.children [
                                                            Html.p [
                                                                prop.className "text-xs tracking-wider text-gray-400 uppercase mb-1"
                                                                prop.text "Reference"
                                                            ]
                                                            Html.p [
                                                                prop.className "text-lg font-mono text-gray-700"
                                                                prop.text order.InternalId
                                                            ]
                                                        ]
                                                    ]

                                                    Html.div [
                                                        prop.children [
                                                            Html.p [
                                                                prop.className "text-xs tracking-wider text-gray-400 uppercase mb-1"
                                                                prop.text "Status"
                                                            ]
                                                            Html.p [
                                                                prop.className (
                                                                    "text-lg font-medium capitalize " +
                                                                    statusColor order.Status
                                                                )
                                                                prop.text order.Status
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]

                                    // Order Items
                                    Html.div [
                                        prop.className "px-8 py-8"
                                        prop.children [
                                            Html.h2 [
                                                prop.className "text-sm tracking-wider text-gray-400 uppercase mb-6"
                                                prop.text "Order Items"
                                            ]
                                            Html.div [
                                                prop.className "space-y-6"
                                                prop.children (
                                                    order.OrderItems
                                                    |> List.mapi (fun index line ->
                                                        Html.div [
                                                            prop.key (sprintf "order-item-%d" index)
                                                            prop.className "flex gap-6 pb-6 border-b border-gray-100 last:border-0 last:pb-0 group"
                                                            prop.children [
                                                                Html.div [
                                                                    prop.className "w-24 h-24 bg-gray-100 overflow-hidden flex-shrink-0 relative rounded-md"
                                                                    prop.children [
                                                                        Html.div [
                                                                            prop.className "absolute inset-0 bg-linear-to-b from-gray-200 to-gray-300 flex items-center justify-center"
                                                                            prop.children [
                                                                                // Replace with your Lucide binding
                                                                                Html.img [
                                                                                    prop.src line.Item.ThumbnailUrl
                                                                                    prop.alt line.Item.Name
                                                                                    prop.className " object-cover"
                                                                                ]
                                                                            ]
                                                                        ]
                                                                    ]
                                                                ]
                                                                Html.div [
                                                                    prop.className "flex-grow"
                                                                    prop.children [
                                                                        Html.div [
                                                                            prop.className "flex justify-between items-start mb-2"
                                                                            prop.children [
                                                                                Html.div [
                                                                                    prop.children [
                                                                                        Html.h3 [
                                                                                            prop.className "text-base font-medium text-gray-900 mb-1"
                                                                                            prop.text line.Item.Name
                                                                                        ]
                                                                                        Html.p [
                                                                                            prop.className "text-sm text-gray-500"
                                                                                            prop.text (sprintf "Quantity: %d" line.Item.Quantity)
                                                                                        ]
                                                                                        match line.Item.SyncVariantId with
                                                                                        | Some id ->
                                                                                            Html.p [
                                                                                                prop.className "text-xs text-gray-400 mt-1 font-mono"
                                                                                                prop.text (sprintf "Variant: %d" id)
                                                                                            ]
                                                                                        | None -> Html.none
                                                                                    ]
                                                                                ]
                                                                                Html.div [
                                                                                    prop.className "text-right"
                                                                                    prop.children [
                                                                                        Html.p [
                                                                                            prop.className "text-lg font-medium text-gray-900"
                                                                                            prop.text (
                                                                                                sprintf "%s %s" line.Currency (money line.LineTotal)
                                                                                            )
                                                                                        ]
                                                                                        Html.p [
                                                                                            prop.className "text-sm text-gray-400"
                                                                                            prop.text (
                                                                                                sprintf "%s %s each" line.Currency (money line.UnitPrice)
                                                                                            )
                                                                                        ]
                                                                                    ]
                                                                                ]
                                                                            ]
                                                                        ]

                                                                        if (not line.IsValid) && line.Error.IsSome then
                                                                            Html.div [
                                                                                prop.className "mt-2 px-3 py-2 bg-red-50 border border-red-100 rounded"
                                                                                prop.children [
                                                                                    Html.p [
                                                                                        prop.className "text-sm text-red-600"
                                                                                        prop.text line.Error.Value
                                                                                    ]
                                                                                ]
                                                                            ]
                                                                        else
                                                                            Html.none
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    )
                                                )
                                            ]
                                        ]
                                    ]

                                    // Order Totals
                                    Html.div [
                                        prop.className "px-8 py-6 bg-gray-50 border-t border-gray-100"
                                        prop.children [
                                            Html.div [
                                                prop.className "space-y-3 mb-4"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex justify-between text-sm"
                                                        prop.children [
                                                            Html.span [
                                                                prop.className "text-gray-600"
                                                                prop.text "Subtotal"
                                                            ]
                                                            Html.span [
                                                                prop.className "text-gray-900 font-medium"
                                                                prop.text (sprintf "$ %s" (money order.Costs.Subtotal))
                                                            ]
                                                        ]
                                                    ]
                                                    Html.div [
                                                        prop.className "flex justify-between text-sm"
                                                        prop.children [
                                                            Html.span [
                                                                prop.className "text-gray-600"
                                                                prop.text (sprintf "Shipping (%s)" order.Costs.ShippingName)
                                                            ]
                                                            Html.span [
                                                                prop.className "text-gray-900 font-medium"
                                                                prop.text (sprintf "$ %s" (money order.Costs.Shipping))
                                                            ]
                                                        ]
                                                    ]
                                                    Html.div [
                                                        prop.className "flex justify-between text-sm"
                                                        prop.children [
                                                            Html.span [
                                                                prop.className "text-gray-600"
                                                                prop.text "Tax"
                                                            ]
                                                            Html.span [
                                                                prop.className "text-gray-900 font-medium"
                                                                prop.text (sprintf "$ %s" (money order.Costs.Tax))
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]
                                            Html.div [
                                                prop.className "pt-4 border-t border-gray-200 flex justify-between items-center"
                                                prop.children [
                                                    Html.span [
                                                        prop.className "text-base font-medium text-gray-900"
                                                        prop.text "Total"
                                                    ]
                                                    Html.span [
                                                        prop.className "text-2xl font-medium text-gray-900"
                                                        prop.text (sprintf "$ %s" (money order.Costs.Total))
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]

                            // Right column - Shipments
                            if not (List.isEmpty order.Shipments) then
                                Html.div [
                                    prop.className "card bg-base-100 shadow-sm border border-gray-100 overflow-hidden"
                                    prop.children [
                                        Html.div [
                                            prop.className "px-8 py-6 border-b border-gray-100 bg-linear-to-b from-gray-50 to-white"
                                            prop.children [
                                                Html.div [
                                                    prop.className "flex items-center gap-3"
                                                    prop.children [
                                                        Html.i [
                                                            // Replace with your Lucide Truck binding
                                                            prop.className "lucide lucide-truck w-5 h-5 text-gray-400"
                                                        ]
                                                        Html.h2 [
                                                            prop.className "text-sm tracking-wider text-gray-400 uppercase"
                                                            prop.text "Shipping Information"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                        Html.div [
                                            prop.className "divide-y divide-gray-100"
                                            prop.children (
                                                order.Shipments
                                                |> List.mapi (fun index shipment ->
                                                    let isExpanded =
                                                        match expandedShipment with
                                                        | Some i when i = index -> true
                                                        | _ -> false

                                                    Html.div [
                                                        prop.key (sprintf "shipment-%d" index)
                                                        prop.className "px-8 py-6"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "space-y-4"
                                                                prop.children [

                                                                    Html.div [
                                                                        prop.className "flex justify-between items-start"
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.children [
                                                                                    Html.p [
                                                                                        prop.className "text-base font-medium text-gray-900 mb-1"
                                                                                        prop.text (sprintf "%s - %s" shipment.Carrier shipment.Service)
                                                                                    ]
                                                                                    Html.p [
                                                                                        prop.className "text-sm text-gray-500"
                                                                                        prop.text (sprintf "Shipped on %s" (formatDate shipment.ShipDate))
                                                                                    ]
                                                                                ]
                                                                            ]
                                                                            if not (List.isEmpty shipment.Items) then
                                                                                Html.button [
                                                                                    prop.onClick (fun _ ->
                                                                                        setExpandedShipment(
                                                                                            if isExpanded then None else Some index
                                                                                        )
                                                                                    )
                                                                                    prop.className "text-sm text-gray-500 hover:text-gray-900 transition-colors flex items-center gap-2"
                                                                                    prop.children [
                                                                                        Html.span [
                                                                                            prop.text (
                                                                                                sprintf "%d item%s"
                                                                                                    shipment.Items.Length
                                                                                                    (if shipment.Items.Length > 1 then "s" else "")
                                                                                            )
                                                                                        ]
                                                                                        Html.i [
                                                                                            prop.className (
                                                                                                if isExpanded then
                                                                                                    "lucide lucide-chevron-up w-4 h-4"
                                                                                                else
                                                                                                    "lucide lucide-chevron-down w-4 h-4"
                                                                                            )
                                                                                        ]
                                                                                    ]
                                                                                ]
                                                                            else
                                                                                Html.none
                                                                        ]
                                                                    ]

                                                                    // Tracking info
                                                                    Html.div [
                                                                        prop.className "flex flex-col gap-4 pt-4 border-t border-gray-100"
                                                                        prop.children [
                                                                            Html.div [
                                                                                prop.className "flex-grow"
                                                                                prop.children [
                                                                                    Html.p [
                                                                                        prop.className "text-xs tracking-wider text-gray-400 uppercase mb-1"
                                                                                        prop.text "Tracking Number"
                                                                                    ]
                                                                                    Html.p [
                                                                                        prop.className "text-sm font-mono text-gray-700 break-all"
                                                                                        prop.text shipment.TrackingNumber
                                                                                    ]
                                                                                ]
                                                                            ]
                                                                            // match shipment.TrackingUrl with
                                                                            // | Some url ->
                                                                            Html.a [
                                                                                prop.href shipment.TrackingUrl
                                                                                prop.target "_blank"
                                                                                prop.rel "noopener noreferrer"
                                                                                prop.className "btn btn-neutral w-full px-6 py-2.5 bg-gray-900 text-white text-sm tracking-wide hover:bg-gray-800 transition-colors flex items-center justify-center gap-2 group rounded-none"
                                                                                prop.children [
                                                                                    Html.span [ prop.text "TRACK PACKAGE" ]
                                                                                    Html.i [
                                                                                        prop.className "lucide lucide-arrow-right w-4 h-4 group-hover:translate-x-1 transition-transform"
                                                                                    ]
                                                                                ]
                                                                            ]
                                                                            // | None -> Html.none
                                                                        ]
                                                                    ]

                                                                    // Expanded items
                                                                    if isExpanded && not (List.isEmpty shipment.Items) then
                                                                        Html.div [
                                                                            prop.className "pt-4 border-t border-gray-100 space-y-2"
                                                                            prop.children [
                                                                                Html.p [
                                                                                    prop.className "text-xs tracking-wider text-gray-400 uppercase mb-3"
                                                                                    prop.text "Items in this shipment"
                                                                                ]
                                                                                for itemIndex in shipment.Items do
                                                                                    match order.OrderItems |> List.tryItem itemIndex with
                                                                                    | Some item ->
                                                                                        Html.div [
                                                                                            prop.key (sprintf "shipment-%d-item-%d" index itemIndex)
                                                                                            prop.className "text-sm text-gray-600 pl-4 border-l-2 border-gray-200"
                                                                                            prop.text (
                                                                                                sprintf "Item #%d - Qty: %d"
                                                                                                    (itemIndex + 1)
                                                                                                    item.Item.Quantity
                                                                                            )
                                                                                        ]
                                                                                    | None -> Html.none
                                                                            ]
                                                                        ]
                                                                    else
                                                                        Html.none
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                )
                                            )
                                        ]
                                    ]
                                ]
                            else
                                Html.none
                        ]
                    ]
                    
                ]
            ]

        ]
    ]

// ---------- Example usage with mock data ----------

[<ReactComponent>]
let View (orderResponse : Shared.Api.Checkout.ConfirmOrderResponse) =
    OrderConfirmation { orderData = orderResponse }
