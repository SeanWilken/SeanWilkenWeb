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
    static member inline Star     (classes: string) = icon "Star" classes
    static member inline Info     (classes: string) = icon "Info" classes
    static member inline ShoppingCart     (classes: string) = icon "ShoppingCart" classes
    static member inline BrainCircuit     (classes: string) = icon "BrainCircuit" classes
    static member inline MegaPhone     (classes: string) = icon "Megaphone" classes
    static member inline JoyStick     (classes: string) = icon "Joystick" classes
    static member inline BookOpenText     (classes: string) = icon "BookOpenText" classes
    static member inline Cog     (classes: string) = icon "Cog" classes
    static member inline Bot     (classes: string) = icon "Bot" classes
    static member inline CircleQuestionMark     (classes: string) = icon "CircleQuestionMark" classes
    static member inline HandPlatter     (classes: string) = icon "HandPlatter" classes
    static member inline Send     (classes: string) = icon "Send" classes
    static member inline Clock     (classes: string) = icon "Clock" classes
    static member inline AtSign     (classes: string) = icon "AtSign" classes
    static member inline ArrowRight     (classes: string) = icon "ArrowRight" classes
    static member inline Code2     (classes: string) = icon "Code2" classes
    static member inline MessageCircle     (classes: string) = icon "MessageCircle" classes
    static member inline Sparkles     (classes: string) = icon "Sparkles" classes
    static member inline Inbox     (classes: string) = icon "Inbox" classes
    static member inline Eye     (classes: string) = icon "Eye" classes
    static member inline HelpCircle     (classes: string) = icon "HelpCircle" classes
    static member inline Reply     (classes: string) = icon "Reply" classes
    static member inline HeartHandshake     (classes: string) = icon "HeartHandshake" classes
    static member inline Cloud     (classes: string) = icon "Cloud" classes
    static member inline Compass     (classes: string) = icon "Compass" classes