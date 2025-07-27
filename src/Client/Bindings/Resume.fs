module ResumeBindings

open Feliz
open Fable.Core.JsInterop

type ResumeSection = {|
    label: string
    items: string array
|}

// ReactComponent wrapper accepting props
[<ReactComponent>]
let ResumePage (props: {| sections: ResumeSection array |}) =
    Feliz.Interop.reactApi.createElement (
        importDefault "../Components/Typescript/Resume.tsx",
        !!props // cast props as JS object
    )