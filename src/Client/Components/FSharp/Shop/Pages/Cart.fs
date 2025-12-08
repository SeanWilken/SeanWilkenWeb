namespace Client.Components.Shop.Cart

open Feliz
open Shared
open Client.Components.Shop.Common
open Client.Components.Shop.Common.Types

module Cart =

    type CartItem = {
        Id           : int
        Name         : string
        Price        : decimal
        Quantity     : int
        Color        : string
        Size         : string
        ImageLabel   : string
        CustomDesign : bool
    }

    type Model = {
        Items : CartItem list
    }

    type Msg =
        | IncrementQty of itemId:int
        | DecrementQty of itemId:int
        | RemoveItem   of itemId:int
        | GoToCollection
        | GoToCheckout

    let private totals (items: CartItem list) =
        let subtotal =
            items
            |> List.sumBy (fun i -> i.Price * decimal i.Quantity)

        let shippingCost =
            if subtotal > 100m then 0m else 12m

        let tax = subtotal * 0.08m
        let total = subtotal + tax + shippingCost
        subtotal, shippingCost, tax, total

    let view (model: Model) (dispatch: Msg -> unit) =
        let subtotal, shippingCost, tax, total = totals model.Items

        Html.section [
            prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 md:py-20"
            prop.children [
                if model.Items.IsEmpty then
                    Html.div [
                        prop.className "text-center py-24 md:py-32"
                        prop.children [
                            Html.div [
                                prop.className "w-16 h-16 mx-auto mb-6 rounded-full bg-base-200 flex items-center justify-center text-3xl"
                                prop.text "🛍️"
                            ]
                            Html.h2 [
                                prop.className "text-2xl md:text-3xl font-light mb-3"
                                prop.text "Your cart is empty"
                            ]
                            Html.p [
                                prop.className "text-sm md:text-base text-base-content/60 mb-6"
                                prop.text "Add some items to get started."
                            ]
                            Html.button [
                                prop.className "btn btn-primary rounded-none px-8"
                                prop.text "Continue Shopping"
                                prop.onClick (fun _ -> dispatch GoToCollection)
                            ]
                        ]
                    ]
                else
                    Html.div [
                        prop.className "grid grid-cols-1 lg:grid-cols-3 gap-10 lg:gap-12"
                        prop.children [

                            // LEFT: items
                            Html.div [
                                prop.className "lg:col-span-2 space-y-8"
                                prop.children [
                                    Html.div [
                                        prop.className "flex items-center justify-between border-b border-base-300 pb-4 md:pb-6"
                                        prop.children [
                                            Html.h1 [
                                                prop.className "text-3xl md:text-4xl font-light"
                                                prop.text "Shopping Cart"
                                            ]
                                            Html.span [
                                                prop.className "text-sm text-base-content/60"
                                                prop.text (
                                                    let count = model.Items.Length
                                                    if count = 1 then "1 item"
                                                    else $"{count} items"
                                                )
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "space-y-6"
                                        prop.children [
                                            for item in model.Items do
                                                Html.div [
                                                    prop.key item.Id
                                                    prop.className "flex gap-4 md:gap-6 pb-5 border-b border-base-300 group"
                                                    prop.children [

                                                        // Thumbnail
                                                        Html.div [
                                                            prop.className "w-24 h-24 md:w-32 md:h-32 flex-shrink-0 bg-base-200 flex items-center justify-center text-3xl md:text-4xl font-light text-base-content/30 relative overflow-hidden rounded-md"
                                                            prop.children [
                                                                Html.span item.ImageLabel
                                                                if item.CustomDesign then
                                                                    Html.div [
                                                                        prop.className "absolute top-1.5 right-1.5 bg-neutral text-neutral-content text-[0.6rem] px-2 py-0.5 rounded-full uppercase tracking-[0.15em]"
                                                                        prop.text "Custom"
                                                                    ]
                                                            ]
                                                        ]

                                                        // Details + quantity
                                                        Html.div [
                                                            prop.className "flex-1 space-y-3"
                                                            prop.children [
                                                                Html.div [
                                                                    prop.className "flex items-start justify-between gap-3"
                                                                    prop.children [
                                                                        Html.div [
                                                                            Html.h3 [
                                                                                prop.className "text-base md:text-lg font-medium mb-1"
                                                                                prop.text item.Name
                                                                            ]
                                                                            Html.div [
                                                                                prop.className "text-xs md:text-sm text-base-content/60 space-y-0.5"
                                                                                prop.children [
                                                                                    Html.p [ prop.text $"Color: {item.Color}" ]
                                                                                    Html.p [ prop.text $"Size: {item.Size}" ]
                                                                                ]
                                                                            ]
                                                                        ]
                                                                        Html.button [
                                                                            prop.className "text-base-content/40 hover:text-base-content/80 transition-opacity opacity-0 group-hover:opacity-100"
                                                                            prop.onClick (fun _ -> dispatch (RemoveItem item.Id))
                                                                            prop.children [
                                                                                Html.span "✕"
                                                                            ]
                                                                        ]
                                                                    ]
                                                                ]

                                                                Html.div [
                                                                    prop.className "flex items-center justify-between pt-1"
                                                                    prop.children [
                                                                        Html.div [
                                                                            prop.className "flex items-center gap-2 md:gap-3"
                                                                            prop.children [
                                                                                Html.button [
                                                                                    prop.className "btn btn-xs btn-outline rounded-none"
                                                                                    prop.text "−"
                                                                                    prop.onClick (fun _ -> dispatch (DecrementQty item.Id))
                                                                                ]
                                                                                Html.span [
                                                                                    prop.className "w-10 text-center font-medium"
                                                                                    prop.text (string item.Quantity)
                                                                                ]
                                                                                Html.button [
                                                                                    prop.className "btn btn-xs btn-outline rounded-none"
                                                                                    prop.text "+"
                                                                                    prop.onClick (fun _ -> dispatch (IncrementQty item.Id))
                                                                                ]
                                                                            ]
                                                                        ]
                                                                        Html.p [
                                                                            prop.className "text-lg font-light"
                                                                            prop.text (sprintf "$%.2f" (item.Price * decimal item.Quantity))
                                                                        ]
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                        ]
                                    ]

                                    Html.button [
                                        prop.className "inline-flex items-center gap-2 text-xs md:text-sm font-medium uppercase tracking-[0.25em] hover:gap-3 transition-all"
                                        prop.onClick (fun _ -> dispatch GoToCollection)
                                        prop.children [
                                            Html.span [
                                                prop.className "inline-block rotate-180"
                                                prop.text "➜"
                                            ]
                                            Html.span "Continue Shopping"
                                        ]
                                    ]
                                ]
                            ]

                            // RIGHT: summary
                            Html.div [
                                prop.className "lg:col-span-1"
                                prop.children [
                                    Html.div [
                                        prop.className "sticky top-28 space-y-6 border border-base-300 rounded-lg p-6 md:p-8 bg-base-100"
                                        prop.children [
                                            Html.h2 [
                                                prop.className "text-xl md:text-2xl font-light pb-4 border-b border-base-300"
                                                prop.text "Order Summary"
                                            ]

                                            Html.div [
                                                prop.className "space-y-3 text-sm"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center justify-between text-base-content/70"
                                                        prop.children [
                                                            Html.span "Subtotal"
                                                            Html.span (sprintf "$%.2f" subtotal)
                                                        ]
                                                    ]
                                                    Html.div [
                                                        prop.className "flex items-center justify-between text-base-content/70"
                                                        prop.children [
                                                            Html.span "Shipping"
                                                            Html.span (
                                                                if shippingCost = 0m then "FREE"
                                                                else sprintf "$%.2f" shippingCost
                                                            )
                                                        ]
                                                    ]
                                                    if shippingCost = 12m && subtotal < 100m then
                                                        Html.div [
                                                            prop.className "text-[0.7rem] text-base-content/60 bg-base-200/80 p-3 rounded"
                                                            prop.text (sprintf "Add $%.2f more for free shipping." (100m - subtotal))
                                                        ]
                                                    Html.div [
                                                        prop.className "flex items-center justify-between text-base-content/70"
                                                        prop.children [
                                                            Html.span "Tax (8%)"
                                                            Html.span (sprintf "$%.2f" tax)
                                                        ]
                                                    ]
                                                    Html.div [
                                                        prop.className "pt-4 border-t border-base-300 flex items-center justify-between text-lg"
                                                        prop.children [
                                                            Html.span [ prop.className "font-medium"; prop.text "Total" ]
                                                            Html.span [ prop.className "font-light"; prop.text (sprintf "$%.2f" total) ]
                                                        ]
                                                    ]
                                                ]
                                            ]

                                            Html.button [
                                                prop.className "btn btn-primary w-full rounded-none mt-2"
                                                prop.text "Proceed to Checkout"
                                                prop.onClick (fun _ -> dispatch GoToCheckout)
                                            ]

                                            Html.div [
                                                prop.className "space-y-2 pt-4 border-t border-base-300 text-xs md:text-sm text-base-content/70"
                                                prop.children [
                                                    Html.div [
                                                        prop.className "flex items-center gap-2"
                                                        prop.children [
                                                            Html.span "✓"
                                                            Html.span "Free returns within 30 days"
                                                        ]
                                                    ]
                                                    Html.div [
                                                        prop.className "flex items-center gap-2"
                                                        prop.children [
                                                            Html.span "✓"
                                                            Html.span "Secure checkout with encryption"
                                                        ]
                                                    ]
                                                    Html.div [
                                                        prop.className "flex items-center gap-2"
                                                        prop.children [
                                                            Html.span "✓"
                                                            Html.span "Customer support 24/7"
                                                        ]
                                                    ]
                                                ]
                                            ]

                                            Html.div [
                                                prop.className "border border-base-200 rounded-lg p-4 mt-2 space-y-3"
                                                prop.children [
                                                    Html.h3 [
                                                        prop.className "text-[0.7rem] font-semibold uppercase tracking-[0.25em]"
                                                        prop.text "Have a promo code?"
                                                    ]
                                                    Html.div [
                                                        prop.className "flex gap-2"
                                                        prop.children [
                                                            Html.input [
                                                                prop.className "input input-bordered input-sm rounded-none flex-1"
                                                                prop.placeholder "Enter code"
                                                            ]
                                                            Html.button [
                                                                prop.className "btn btn-sm rounded-none"
                                                                prop.text "Apply"
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]

                                            Html.div [
                                                prop.className "bg-base-200/60 rounded-lg p-4 text-center text-xs md:text-sm text-base-content/70"
                                                prop.children [
                                                    Html.span "Need help? "
                                                    Html.button [
                                                        prop.className "underline hover:text-base-content"
                                                        prop.text "Contact us"
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
            ]
        ]
