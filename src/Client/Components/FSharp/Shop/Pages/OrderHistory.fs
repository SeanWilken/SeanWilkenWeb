namespace Client.Components.Shop

open Feliz
open Fable.Core
open System
open Shared.Api.Checkout
open Client.Domain.Store.OrderHistory
open Elmish

module OrderHistory =
    open Feliz.UseDeferred

    let statusToColor = 
        function
        | AwaitingPayment
        | Pending -> "text-yellow-500"
        | InProduction
        | Processing -> "text-blue-500"
        | Shipped -> "text-purple-500"
        | Paid -> "text-green-500"
        | Cancelled -> "text-red-500"
        | Refunded -> "text-orange-500"
        | Other _ -> "text-info-500" // ?



    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | SetEmail email ->
            { model with Email = email; Error = None }, Cmd.none
            
        | SetOrderId orderId ->
            { model with OrderId = orderId; Error = None }, Cmd.none
            
        | SearchOrders ->
            if String.IsNullOrWhiteSpace(model.Email) then
                { model with Error = Some "Email is required" }, Cmd.none
            else
                let request = {
                    Email = model.Email.Trim()
                    OrderId = 
                        if String.IsNullOrWhiteSpace(model.OrderId) then None
                        else Some (model.OrderId.Trim())
                }
                
                let cmd = 
                    Cmd.OfAsync.either
                        Client.Api.checkoutApi.LookupOrder
                        request
                        OrdersLoaded
                        (fun ex -> SearchFailed ex.Message)
                
                { model with IsSearching = true; Error = None }, cmd
                
        | OrdersLoaded orders ->
            { model with 
                Orders = Deferred.Resolved orders.Orders
                IsSearching = false
                Error = if orders.Orders.IsEmpty then Some "No orders found" else None
            }, Cmd.none
            
        | SearchFailed error ->
            { model with 
                IsSearching = false
                Error = Some error
            }, Cmd.none
            
        | ClearResults ->
            initModel(), Cmd.none

    // View Components
    let private inputField (label: string) (placeholder: string) (value: string) (onChange: string -> unit) =
        Html.div [
            prop.className "space-y-2"
            prop.children [
                Html.label [
                    prop.className "text-xs tracking-widest uppercase text-base-content/60 font-light"
                    prop.text label
                ]
                Html.input [
                    prop.type' "text"
                    prop.placeholder placeholder
                    prop.value value
                    prop.onChange onChange
                    prop.className "w-full px-4 py-3 bg-transparent border border-base-content/10 
                        text-sm font-light tracking-wide transition-all duration-300
                        focus:border-base-content/30 focus:outline-none
                        hover:border-base-content/20"
                ]
            ]
        ]

    let private orderCard (order: OrderSummary) =
        Html.div [
            prop.className "p-6 bg-base-100/50 border border-base-content/8 
                transition-all duration-500 hover:border-base-content/15 
                hover:shadow-lg group"
            prop.children [
                // Header
                Html.div [
                    prop.className "flex items-start justify-between mb-4"
                    prop.children [
                        Html.div [
                            prop.className "space-y-1"
                            prop.children [
                                Html.div [
                                    prop.className "font-light text-xs tracking-wider uppercase text-base-content/50"
                                    prop.text "Order ID"
                                ]
                                Html.div [
                                    prop.className "font-mono text-sm text-base-content"
                                    prop.text order.OrderId
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "text-right space-y-1"
                            prop.children [
                                Html.div [
                                    prop.className (sprintf "text-xs tracking-wider uppercase font-medium %s" 
                                        (statusToColor order.Status))
                                    prop.text (OrderStatus.toString order.Status)
                                ]
                                if order.PrintfulOrderId.IsSome then
                                    Html.div [
                                        prop.className "font-mono text-xs text-base-content/50"
                                        prop.text (sprintf "Printful #%d" order.PrintfulOrderId.Value)
                                    ]
                            ]
                        ]
                    ]
                ]
                
                // Divider
                Html.div [
                    prop.className "h-px bg-base-content/5 mb-4"
                ]
                
                // Details
                Html.div [
                    prop.className "grid grid-cols-2 gap-4 text-sm"
                    prop.children [
                        Html.div [
                            prop.className "space-y-1"
                            prop.children [
                                Html.div [
                                    prop.className "text-xs text-base-content/50"
                                    prop.text "Date"
                                ]
                                Html.div [
                                    prop.className "font-light text-base-content/80"
                                    prop.text (order.CreatedAt.ToString("MMM dd, yyyy"))
                                ]
                            ]
                        ]
                        Html.div [
                            prop.className "space-y-1 text-right"
                            prop.children [
                                Html.div [
                                    prop.className "text-xs text-base-content/50"
                                    prop.text "Total"
                                ]
                                Html.div [
                                    prop.className "font-light text-base-content"
                                    prop.text (sprintf "%s %.2f" order.Currency order.Total)
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]

    let private emptyState =
        Html.div [
            prop.className "text-center py-16 px-8"
            prop.children [
                Html.div [
                    prop.className "w-16 h-16 mx-auto mb-6 rounded-full bg-base-content/5 
                        flex items-center justify-center"
                    prop.children [
                        // Html.svg [
                        //     prop.className "w-8 h-8 text-base-content/30"
                        //     prop.fill "none"
                        //     prop.stroke "currentColor"
                        //     prop.viewBox (0, 0, 24, 24)
                        //     prop.children [
                        //         Html.path [
                        //             prop.strokeLinecap "round"
                        //             prop.strokeLinejoin "round"
                        //             prop.strokeWidth 1.5
                        //             prop.d "M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                        //         ]
                        //     ]
                        // ]
                    ]
                ]
                Html.p [
                    prop.className "text-sm text-base-content/60 font-light"
                    prop.text "Enter your email to search for orders"
                ]
            ]
        ]

    // Main View
    let view (model: Model) (dispatch: Msg -> unit) =
        Html.div [
            prop.className "space-y-8"
            prop.children [
                // Header
                Html.div [
                    prop.className "text-center pb-8 border-b border-base-content/10"
                    prop.children [
                        Html.h2 [
                            prop.className "text-2xl font-light tracking-tight mb-2"
                            prop.style [ style.fontFamily "'Cormorant Garamond', serif" ]
                            prop.text "Track Your Order"
                        ]
                        Html.p [
                            prop.className "text-sm text-base-content/60 font-light"
                            prop.text "Enter your email and optional order ID to view order status"
                        ]
                    ]
                ]
                
                // Search Form
                Html.div [
                    prop.className "space-y-6"
                    prop.children [
                        // Email Input (Required)
                        inputField 
                            "Email Address *" 
                            "your@email.com"
                            model.Email
                            (SetEmail >> dispatch)
                        
                        // Order ID Input (Optional)
                        inputField 
                            "Order ID (Optional)" 
                            "Leave blank to see all orders"
                            model.OrderId
                            (SetOrderId >> dispatch)
                        
                        // Error Message
                        if model.Error.IsSome then
                            Html.div [
                                prop.className "p-4 border border-red-500/20 bg-red-500/5 text-red-500"
                                prop.children [
                                    Html.p [
                                        prop.className "text-sm font-light"
                                        prop.text model.Error.Value
                                    ]
                                ]
                            ]
                        
                        // Action Buttons
                        Html.div [
                            prop.className "flex gap-4"
                            prop.children [
                                Html.button [
                                    prop.onClick (fun _ -> dispatch SearchOrders)
                                    prop.disabled model.IsSearching
                                    prop.className "flex-1 px-6 py-3.5 bg-base-content text-base-100 
                                        text-xs tracking-widest uppercase font-light
                                        hover:opacity-90 transition-all duration-300
                                        disabled:opacity-50 disabled:cursor-not-allowed
                                        hover:-translate-y-1 hover:shadow-xl"
                                    prop.children [
                                        if model.IsSearching then
                                            Html.span [
                                                prop.className "flex items-center justify-center gap-2"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "w-4 h-4 border-2 border-base-100/30 
                                                            border-t-base-100 rounded-full animate-spin"
                                                    ]
                                                    Html.text "Searching..."
                                                ]
                                            ]
                                        else
                                            Html.text "Search Orders"
                                    ]
                                ]
                                
                                match model.Orders with
                                | Deferred.Resolved _ ->
                                    Html.button [
                                        prop.onClick (fun _ -> dispatch ClearResults)
                                        prop.className "px-6 py-3.5 border border-base-content/20 
                                            text-xs tracking-widest uppercase font-light
                                            hover:bg-base-content/5 transition-all duration-300"
                                        prop.text "Clear"
                                    ]
                                | _ -> Html.none
                            ]
                        ]
                    ]
                ]
                
                // Results
                Html.div [
                    prop.className "space-y-6"
                    prop.children [
                        match model.Orders with
                        | Deferred.Resolved orders ->
                            
                            Html.div [
                                prop.className "space-y-4"
                                prop.children [
                                    if not orders.IsEmpty then
                                        Html.div [
                                            prop.className "flex items-center justify-between pb-4 
                                                border-b border-base-content/10"
                                            prop.children [
                                                Html.h3 [
                                                    prop.className "text-xs tracking-widest uppercase 
                                                        text-base-content/60 font-light"
                                                    prop.text (sprintf "Found %d Order(s)" orders.Length)
                                                ]
                                            ]
                                        ]
                                    
                                    for order in orders do
                                        orderCard order
                                ]
                            ]
                        | _ ->
                            if model.Error.IsNone && not model.IsSearching 
                            then emptyState
                    ]
                ]
            ]
        ]

    // React Component (if using React interop)
    [<ReactComponent>]
    let OrderLookupComponent model dispatch =
        view model dispatch