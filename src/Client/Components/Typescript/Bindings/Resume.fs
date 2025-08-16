module ResumeBindings

open Feliz
open Fable.Core.JsInterop

type TimelineItem = {|
    company: string
    role: string
    summary: string
    startDateString: string
    endDateString: string
    responsibilities: string array
|}

type ResumeSection =
    | Section of label: string * items: string array
    | Timeline of label: string * items: TimelineItem array
    | TagCloud of label: string * items: string array

// ReactComponent wrapper accepting props

[<ReactComponent>]
let ResumePage (props: {| sections: obj array |}) =
    Feliz.Interop.reactApi.createElement (
        importDefault "../Resume/Resume.tsx",
        !!props // cast props as JS object
    )