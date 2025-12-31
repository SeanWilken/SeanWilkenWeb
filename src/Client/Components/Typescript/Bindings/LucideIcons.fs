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
    static member inline ChevronRight     (classes: string) = icon "ChevronRight" classes
    static member inline PenTool     (classes: string) = icon "PenTool" classes
    static member inline Star     (classes: string) = icon "Star" classes
    static member inline Info     (classes: string) = icon "Info" classes
    static member inline ShoppingCart     (classes: string) = icon "ShoppingCart" classes
    static member inline ShoppingBag     (classes: string) = icon "ShoppingBag" classes
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
    static member inline Rocket     (classes: string) = icon "Rocket" classes
    static member inline FlaskConical     (classes: string) = icon "FlaskConical" classes
    static member inline Image     (classes: string) = icon "Image" classes
    static member inline Gamepad2     (classes: string) = icon "Gamepad2" classes
    static member inline Heart     (classes: string) = icon "Heart" classes
    static member inline GitBranch     (classes: string) = icon "GitBranch" classes
    static member inline MousePointerClick     (classes: string) = icon "MousePointerClick" classes
    static member inline PanelLeft     (classes: string) = icon "PanelLeft" classes
    static member inline Target     (classes: string) = icon "Target" classes
    static member inline Play     (classes: string) = icon "Play" classes
    static member inline Store     (classes: string) = icon "Store" classes
    static member inline ZoomIn     (classes: string) = icon "ZoomIn" classes
    static member inline RotateCw     (classes: string) = icon "RotateCw" classes
    static member inline RotateCcw     (classes: string) = icon "RotateCcw" classes
    static member inline Move     (classes: string) = icon "Move" classes
    static member inline Layers     (classes: string) = icon "Layers" classes
    static member inline Plus     (classes: string) = icon "Plus" classes
    static member inline Minus     (classes: string) = icon "Minus" classes
    static member inline Check     (classes: string) = icon "Check" classes
    static member inline CheckCircle2     (classes: string) = icon "CheckCircle2" classes
    static member inline AlertCircle     (classes: string) = icon "AlertCircle" classes
    static member inline Truck     (classes: string) = icon "Truck" classes
    static member inline CircleDollarSign     (classes: string) = icon "CircleDollarSign" classes
    static member inline PackageCheck     (classes: string) = icon "PackageCheck" classes
    static member inline ArrowLeft     (classes: string) = icon "ArrowLeft" classes
    static member inline CirclePlus     (classes: string) = icon "CirclePlus" classes
    static member inline LayoutTemplate     (classes: string) = icon "LayoutTemplate" classes
    static member inline MessageSquare     (classes: string) = icon "MessageSquare" classes
    static member inline FileCode2     (classes: string) = icon "FileCode2" classes
    static member inline Wrench     (classes: string) = icon "Wrench" classes
    static member inline CircleStar     (classes: string) = icon "CircleStar" classes
    static member inline FileClock     (classes: string) = icon "FileClock" classes
    static member inline PackageSearch     (classes: string) = icon "PackageSearch" classes