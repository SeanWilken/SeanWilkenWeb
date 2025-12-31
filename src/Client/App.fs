module App

open Elmish
open Elmish.Navigation
open Elmish.React
open PageRouter

#if DEBUG
// open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram Index.init Index.update Index.View
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "wilken-website"
// #if DEBUG
// // |> Program.withDebugger
// #endif
|> Program.toNavigable PageRouter.urlParser PageRouter.urlUpdate
|> Program.run
