namespace Client.Components.Shop.Cart

open Feliz
open Shared
open Store.Cart
open Bindings.LucideIcon

module Cart =

    type Props = {
        Cart : CartState
        OnRemove          : CartLineItem -> unit
        OnUpdateQuantity  : CartLineItem * int -> unit
        OnContinueShopping: unit -> unit
        OnCheckout        : unit -> unit
    }

    let private money (v: decimal) =
        v.ToString("0.00")

    [<ReactComponent>]
    let View (props: Props) =
        let items  = props.Cart.Items
        let totals = props.Cart.Totals

        let cartItemsCount = items.Length

        // For the free-shipping banner, mirror your TSX behaviour
        let freeShippingThreshold = 100m
        let standardShippingFlat  = 12m

        Html.div [
            prop.className "max-w-7xl mx-auto px-6 py-20"

            prop.children [
                if cartItemsCount = 0 then
                    Html.div [
                        prop.className "text-center py-32"
                        prop.children [
                            LucideIcon.ShoppingBag "w-16 h-16 mx-auto text-base-300 mb-6"
                            Html.h2 [
                                prop.className "text-3xl font-light mb-4"
                                prop.text "Your cart is empty"
                            ]
                            Html.p [
                                prop.className "text-base-content/60 mb-8"
                                prop.text "Add some items to get started"
                            ]
                            Html.button [
                                prop.className
                                    "btn btn-sm sm:btn-md rounded-none bg-base-content text-base-100 px-10 py-4 \
                                    text-sm font-medium uppercase tracking-[0.2em] hover:bg-base-content/90 transition-colors"
                                prop.onClick (fun _ -> props.OnContinueShopping())
                                prop.text "Start shopping"
                            ]
                        ]
                    ]
                else
                    Html.div [
                        prop.className "grid grid-cols-1 lg:grid-cols-3 gap-12"
                        prop.children [

                            // LEFT: items
                            Html.div [
                                prop.className "lg:col-span-2 space-y-8"
                                prop.children [

                                    Html.button [
                                        prop.className
                                            "flex items-center gap-2 text-sm font-medium uppercase tracking-[0.2em] \
                                            hover:gap-3 transition-all"
                                        prop.onClick (fun _ -> props.OnContinueShopping())
                                        prop.children [
                                            LucideIcon.ChevronRight "w-4 h-4 rotate-180"
                                            Html.span [ prop.text "Continue shopping" ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "flex items-center justify-between border-b border-base-300 pb-6"
                                        prop.children [
                                            Html.h1 [
                                                prop.className "text-4xl font-light"
                                                prop.text "Shopping Cart"
                                            ]
                                            Html.span [
                                                prop.className "text-base-content/60 text-sm"
                                                prop.text (
                                                    sprintf "%d item%s"
                                                        cartItemsCount
                                                        (if cartItemsCount = 1 then "" else "s")
                                                )
                                            ]
                                        ]
                                    ]

                                    Html.div [
                                        prop.className "space-y-6"
                                        prop.children [
                                            for item in items do
                                                let itemId =
                                                    match item with
                                                    | CartLineItem.Custom c -> 
                                                        // c.CatalogProductId, c.CatalogVariantId
                                                        $"{c.CatalogProductId}-{c.CatalogVariantId}" 
                                                    | CartLineItem.Template t -> 
                                                        // t.CatalogProductId, t.VariantId
                                                        $"{t.CatalogProductId}-{t.VariantId}" 
                                                    | CartLineItem.Sync s -> 
                                                        $"{s.SyncProductId}-{s.SyncVariantId}" 
                                                let itemDetails =
                                                    match item with
                                                    | CartLineItem.Custom c -> 
                                                        {|
                                                            thumbnail = c.PreviewImage
                                                            name = c.Name
                                                            isCustom = c.IsCustomDesign // always true?
                                                            color = c.ColorName
                                                            size = c.Size
                                                            unitPrice = c.UnitPrice
                                                            quantity = c.Quantity
                                                        |}
                                                    | CartLineItem.Template t -> 
                                                        {|
                                                            thumbnail = t.PreviewImage
                                                            name = t.Name
                                                            isCustom = false // ??
                                                            color = t.ColorName
                                                            size = t.Size
                                                            unitPrice = t.UnitPrice
                                                            quantity = t.Quantity
                                                        |}
                                                    | CartLineItem.Sync s -> 
                                                        {|
                                                            thumbnail = Some s.ThumbnailUrl
                                                            name = s.Name.Split("/") |> Array.tryHead |> Option.defaultValue ""
                                                            isCustom = false // ??
                                                            color = s.ColorName
                                                            size = s.Size
                                                            unitPrice = s.UnitPrice
                                                            quantity = s.Quantity
                                                        |}

                                                    

                                                Html.div [
                                                    prop.key itemId
                                                    prop.className "flex gap-6 pb-6 border-b border-base-200 group"
                                                    prop.children [

                                                        // Thumbnail
                                                        Html.div [
                                                            prop.className
                                                                "w-32 h-32 flex-shrink-0 bg-base-200 flex items-center justify-center text-4xl font-light text-base-content/20 relative overflow-hidden rounded-lg"
                                                            prop.children [
                                                                match itemDetails.thumbnail with
                                                                | Some tnail ->
                                                                    Html.img [
                                                                        prop.src tnail
                                                                        prop.alt itemDetails.name
                                                                        prop.className " object-cover"
                                                                    ]
                                                                | None ->
                                                                    Html.span [
                                                                        prop.className "text-3xl"
                                                                        prop.text (itemDetails.name.[0].ToString().ToUpper())
                                                                    ]

                                                                if itemDetails.isCustom then
                                                                    Html.div [
                                                                        prop.className
                                                                            "absolute top-2 right-2 bg-base-content text-base-100 \
                                                                            text-[10px] px-2 py-1 rounded"
                                                                        prop.text "Custom"
                                                                    ]
                                                            ]
                                                        ]

                                                        // Text + controls
                                                        Html.div [
                                                            prop.className "flex-1 space-y-3"
                                                            prop.children [

                                                                // Title + remove
                                                                Html.div [
                                                                    prop.className "flex items-start justify-between"
                                                                    prop.children [
                                                                        Html.div [
                                                                            Html.h3 [
                                                                                prop.className "text-lg font-medium mb-1"
                                                                                prop.text itemDetails.name
                                                                            ]
                                                                            Html.div [
                                                                                prop.className "text-sm text-base-content/60 space-y-1"
                                                                                prop.children [
                                                                                    Html.p [
                                                                                        prop.text (
                                                                                            sprintf "Color: %s"
                                                                                                (itemDetails.color)
                                                                                        )
                                                                                    ]
                                                                                    Html.p [
                                                                                        prop.text (
                                                                                            sprintf "Size: %s"
                                                                                                (itemDetails.size)
                                                                                        )
                                                                                    ]
                                                                                ]
                                                                            ]
                                                                        ]
                                                                        Html.button [
                                                                            prop.onClick (fun _ -> props.OnRemove item)
                                                                            prop.className
                                                                                "text-base-content/40 hover:text-base-content \
                                                                                transition-colors opacity-0 group-hover:opacity-100"
                                                                            prop.children [
                                                                                LucideIcon.X "w-5 h-5"
                                                                            ]
                                                                        ]
                                                                    ]
                                                                ]

                                                                // Qty + line total
                                                                Html.div [
                                                                    prop.className "flex items-center justify-between pt-2"
                                                                    prop.children [
                                                                        Html.div [
                                                                            prop.className "flex items-center gap-3"
                                                                            prop.children [
                                                                                Html.button [
                                                                                    prop.className
                                                                                        "w-8 h-8 border border-base-300 flex items-center \
                                                                                        justify-center hover:border-base-content \
                                                                                        transition-colors"
                                                                                    prop.onClick (fun _ ->
                                                                                        let newQty = max 1 (itemDetails.quantity - 1)
                                                                                        props.OnUpdateQuantity (item, newQty)
                                                                                    )
                                                                                    prop.children [
                                                                                        LucideIcon.Minus "w-4 h-4"
                                                                                    ]
                                                                                ]
                                                                                Html.span [
                                                                                    prop.className "w-12 text-center font-medium"
                                                                                    prop.text (string itemDetails.quantity)
                                                                                ]
                                                                                Html.button [
                                                                                    prop.className
                                                                                        "w-8 h-8 border border-base-300 flex items-center \
                                                                                        justify-center hover:border-base-content \
                                                                                        transition-colors"
                                                                                    prop.onClick (fun _ ->
                                                                                        let newQty = itemDetails.quantity + 1
                                                                                        props.OnUpdateQuantity (item, newQty)
                                                                                    )
                                                                                    prop.children [
                                                                                        LucideIcon.Plus "w-4 h-4"
                                                                                    ]
                                                                                ]
                                                                            ]
                                                                        ]
                                                                        Html.p [
                                                                            prop.className "text-lg font-light"
                                                                            let lineTotal = itemDetails.unitPrice * decimal itemDetails.quantity
                                                                            prop.text ("$" + money lineTotal)
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

                            // RIGHT: summary
                            Html.div [
                                prop.className "lg:col-span-1"
                                prop.children [
                                    Html.div [
                                        prop.className "sticky top-32 space-y-8"
                                        prop.children [

                                            // Promo code
                                            // Html.div [
                                            //     prop.className "border border-base-300 p-6 bg-base-100"
                                            //     prop.children [
                                            //         Html.h3 [
                                            //             prop.className "text-sm font-medium uppercase tracking-[0.2em] mb-4"
                                            //             prop.text "Have a promo code?"
                                            //         ]
                                            //         Html.div [
                                            //             prop.className "flex gap-2"
                                            //             prop.children [
                                            //                 Html.input [
                                            //                     prop.className
                                            //                         "flex-1 px-4 py-3 border border-base-300 text-sm \
                                            //                         focus:outline-none focus:border-base-content transition-colors"
                                            //                     prop.placeholder "Enter code"
                                            //                 ]
                                            //                 Html.button [
                                            //                     prop.className
                                            //                         "px-6 py-3 bg-base-200 text-sm font-medium uppercase \
                                            //                         tracking-[0.2em] hover:bg-base-300 transition-colors"
                                            //                     prop.text "Apply"
                                            //                 ]
                                            //             ]
                                            //         ]
                                            //     ]
                                            // ]

                                            // Order summary card
                                            Html.div [
                                                prop.className "border border-base-300 p-8 space-y-6 bg-base-100"
                                                prop.children [
                                                    Html.h2 [
                                                        prop.className "text-2xl font-light pb-4 border-b border-base-200"
                                                        prop.text "Order Summary"
                                                    ]

                                                    Html.div [
                                                        prop.className "space-y-4 text-sm"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-center justify-between text-base-content/70"
                                                                prop.children [
                                                                    Html.span [ prop.text "Subtotal" ]
                                                                    Html.span [ prop.text ("$ " + money totals.Subtotal) ]
                                                                ]
                                                            ]
                                                            Html.div [
                                                                prop.className "pt-4 border-t border-base-200 flex items-center justify-between text-xl"
                                                                prop.children [
                                                                    Html.span [ prop.className "font-medium"; prop.text "Cart Total" ]
                                                                    Html.span [
                                                                        prop.className "font-light"
                                                                        prop.text ("$ " + money totals.Total)
                                                                    ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]

                                                    Html.button [
                                                        prop.className
                                                            "w-full bg-base-content text-base-100 py-5 text-sm font-medium \
                                                            uppercase tracking-[0.2em] hover:bg-base-content/90 transition-colors"
                                                        prop.onClick (fun _ -> props.OnCheckout())
                                                        prop.text "Proceed to shipping"
                                                    ]

                                                    Html.div [
                                                        prop.className "space-y-3 pt-4 border-t border-base-200 text-sm"
                                                        prop.children [
                                                            Html.div [
                                                                prop.className "flex items-center gap-3 text-base-content/70"
                                                                prop.children [
                                                                    LucideIcon.Check "w-5 h-5 flex-shrink-0"
                                                                    Html.span [ prop.text "Free returns within 30 days" ]
                                                                ]
                                                            ]
                                                            Html.div [
                                                                prop.className "flex items-center gap-3 text-base-content/70"
                                                                prop.children [
                                                                    LucideIcon.Check "w-5 h-5 flex-shrink-0"
                                                                    Html.span [ prop.text "Secure checkout with encryption" ]
                                                                ]
                                                            ]
                                                            Html.div [
                                                                prop.className "flex items-center gap-3 text-base-content/70"
                                                                prop.children [
                                                                    LucideIcon.Check "w-5 h-5 flex-shrink-0"
                                                                    Html.span [ prop.text "Customer support 24/7" ]
                                                                ]
                                                            ]
                                                        ]
                                                    ]
                                                ]
                                            ]


                                            // Tiny footer
                                            Html.div [
                                                prop.className "bg-base-200/60 p-6 text-center text-sm"
                                                prop.children [
                                                    Html.p [
                                                        prop.className "text-base-content/70"
                                                        prop.children [
                                                            Html.text "Need help? "
                                                            Html.button [
                                                                prop.className "underline hover:text-base-content transition-colors"
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
            ]
        ]

