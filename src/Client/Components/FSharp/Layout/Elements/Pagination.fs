module Components.FSharp.Layout.Elements.Pagination

open Feliz
open Feliz.DaisyUI

type Props = {
    total: int
    limit: int
    offset: int
    onPageChange: int -> unit // offset -> unit
}

[<ReactComponent>]
let Pagination (props: Props) =
    let currentPage = props.offset / props.limit + 1
    let totalPages = (props.total + props.limit - 1) / props.limit
    let isDisabled = currentPage = 1
    Daisy.join [
        Daisy.button.button [
            prop.text "Prev"
            button.outline
            prop.disabled isDisabled
            prop.onClick (fun _ -> props.onPageChange (props.offset - props.limit))
        ]
        Daisy.button.button [
            prop.text $"{currentPage} / {totalPages}"
            button.ghost
        ]
        Daisy.button.button [
            prop.text "Next"
            button.outline
            prop.disabled (currentPage >= totalPages)
            prop.onClick (fun _ -> props.onPageChange (props.offset + props.limit))
        ]
    ]