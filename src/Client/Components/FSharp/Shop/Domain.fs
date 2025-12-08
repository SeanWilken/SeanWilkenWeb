namespace Client.Shop

open Elmish
open Shared
open Shared.SharedShopV2
open Shared.SharedShopV2Domain
open Shared.SharedShopDomain
open Client.Domain

module Domain =


    // --- Cart sub-domain ---

    module Cart =
        open Client.Domain.Store

        type Model = {
            Items: CartItem list
        }

        type Msg =
            | AddTemplate of ProductTemplate.ProductTemplate
            // | AddCustom   of BuildYourOwnProductWizard.Model
            | RemoveItem  of CartItem
            | AdjustQty   of CartItem * QuantityAdjustment
            | Clear

        let init () = { Items = [] }

        let private adjustQty (item: CartItem) (adj: QuantityAdjustment) : CartItem =
            match item, adj with
            | Template (tpl, qty), Increment -> Template (tpl, qty + 1)
            | Template (tpl, qty), Decrement -> Template (tpl, max 1 (qty - 1))
            | Custom (byo, qty), Increment   -> Custom (byo, qty + 1)
            | Custom (byo, qty), Decrement   -> Custom (byo, max 1 (qty - 1))

        let update (msg: Msg) (model: Model) : Model =
            match msg with
            | AddTemplate tpl ->
                // naive: always add as new line; you can merge by id later
                { model with Items = Template (tpl, 1) :: model.Items }

            // | AddCustom byo ->
            //     { model with Items = Custom (byo, 1) :: model.Items }

            | RemoveItem item ->
                { model with Items = model.Items |> List.except [ item ] }

            | AdjustQty (item, adj) ->
                let items' =
                    model.Items
                    |> List.map (fun i -> if obj.ReferenceEquals(i, item) then adjustQty i adj else i)
                { model with Items = items' }

            | Clear ->
                { model with Items = [] }
