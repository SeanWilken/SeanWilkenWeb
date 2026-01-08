module Client.Domain

open System
open Feliz.UseDeferred
open Components.FSharp.About
open Components.FSharp.Welcome
open Components.FSharp.Contact
open Components.FSharp.Portfolio
open Components.FSharp.Shop

module WebAppModels =

    type Theme =
        | Light
        | Dark
        | Cupcake
        | Bumblebee
        | Emerald
        | Corporate
        | Synthwave
        | Retro
        | Cyberpunk
        | Valentine
        | Halloween
        | Garden
        | Forest
        | Aqua
        | Lofi
        | Pastel
        | Fantasy
        | Wireframe
        | Black
        | Luxury
        | Dracula
        | Cmyk
        | Autumn
        | Business
        | Acid
        | Lemonade
        | Night
        | Coffee
        | Winter
        | Dim
        | Nord
        | Sunset

        member this.AsString =
            match this with
            | Light -> "light"
            | Dark -> "dark"
            | Cupcake -> "cupcake"
            | Bumblebee -> "bumblebee"
            | Emerald -> "emerald"
            | Corporate -> "corporate"
            | Synthwave -> "synthwave"
            | Retro -> "retro"
            | Cyberpunk -> "cyberpunk"
            | Valentine -> "valentine"
            | Halloween -> "halloween"
            | Garden -> "garden"
            | Forest -> "forest"
            | Aqua -> "aqua"
            | Lofi -> "lofi"
            | Pastel -> "pastel"
            | Fantasy -> "fantasy"
            | Wireframe -> "wireframe"
            | Black -> "black"
            | Luxury -> "luxury"
            | Dracula -> "dracula"
            | Cmyk -> "cmyk"
            | Autumn -> "autumn"
            | Business -> "business"
            | Acid -> "acid"
            | Lemonade -> "lemonade"
            | Night -> "night"
            | Coffee -> "coffee"
            | Winter -> "winter"
            | Dim -> "dim"
            | Nord -> "nord"
            | Sunset -> "sunset"

    type Model =
        | About of Components.FSharp.About.Model
        | Contact
        | Help
        | Settings
        | Shop of Components.FSharp.Shop.Model
        | Services of Components.FSharp.Services.Landing.Model
        | Portfolio of Components.FSharp.PortfolioLanding.Model
        | Resume
        | Welcome

    type WebAppModel = {
        CurrentAreaModel: Model
        // cart needs and checkout at this level?
        Theme: Theme
    }

    type WebAppMsg =
        | WelcomeMsg of Components.FSharp.Welcome.Msg
        | AboutMsg of Components.FSharp.About.Msg
        | PortfolioMsg of Components.FSharp.PortfolioLanding.Msg
        | ShopMsg of Components.FSharp.Shop.ShopMsg
        | ServicesMsg of Components.FSharp.Services.Landing.Msg
        | ErrorMsg of exn // WIP?
        | ChangeTheme of Theme
        // Don't need both?
        | SwitchToOtherApp of SharedViewModule.WebAppView.AppView
        | NavigatePage of SharedViewModule.WebAppView.Page
