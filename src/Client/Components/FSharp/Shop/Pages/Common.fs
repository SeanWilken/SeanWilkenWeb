namespace Client.Components.Shop.Common

open Feliz


module Types =
    type MockProduct = {
        Id       : int
        Name     : string
        Price    : decimal
        Category : string
        Colors   : int
        Tag      : string option
    }

module Ui =
    /// Simple helper to concatenate tailwind classes cleanly
    let inline tw (classes: string list) =
        classes
        |> List.filter (System.String.IsNullOrWhiteSpace >> not)
        |> String.concat " "

    /// DaisyUI button variants
    module Btn =
        let primary extra =
            tw ("btn btn-primary btn-lg tracking-[0.25em]" :: extra)

        let outline extra =
            tw ("btn btn-outline btn-lg tracking-[0.25em]" :: extra)

        let ghost extra =
            tw ("btn btn-ghost" :: extra)

    module Section =
        let container (children: ReactElement list) =
            Html.section [
                prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8"
                prop.children children
            ]
