module Bindings.LucideIcon

open Fable.Core
open Fable.Core.JsInterop
open Fable.React

let inline private icon (name: string) (classes: string) =
    Feliz.Interop.reactApi.createElement(import name "lucide-react", createObj [ "className" ==> classes ])

[<Erase>]
type LucideIcon =
    static member inline Home          (classes: string) = icon "Home" classes
    static member inline UserCircle    (classes: string) = icon "UserCircle" classes
    static member inline FileText      (classes: string) = icon "FileText" classes
    static member inline FolderOpen    (classes: string) = icon "FolderOpen" classes
    static member inline PenLine       (classes: string) = icon "PenLine" classes
    static member inline Mail          (classes: string) = icon "Mail" classes
    static member inline Cpu           (classes: string) = icon "Cpu" classes
    static member inline CodeSandbox         (classes: string) = icon "codesandbox" classes
    static member inline CalendarRange (classes: string) = icon "CalendarRange" classes
    static member inline PlayCircle    (classes: string) = icon "PlayCircle" classes
    static member inline File          (classes: string) = icon "File" classes
    static member inline ExternalLink  (classes: string) = icon "ExternalLink" classes
    static member inline Settings      (classes: string) = icon "Settings" classes
    static member inline Github        (classes: string) = icon "Github" classes
    static member inline Linkedin      (classes: string) = icon "Linkedin" classes
    static member inline Nucleus          (classes: string) = icon "Atom" classes
    static member inline Briefcase     (classes: string) = icon "Briefcase" classes
    static member inline BookOpen     (classes: string) = icon "BookOpen" classes
    static member inline Menu     (classes: string) = icon "Menu" classes
    static member inline Palette     (classes: string) = icon "Palette" classes
    static member inline Instagram     (classes: string) = icon "Instagram" classes
    static member inline X     (classes: string) = icon "X" classes
    static member inline ChevronLeft     (classes: string) = icon "ChevronLeft" classes
    static member inline PenTool     (classes: string) = icon "PenTool" classes