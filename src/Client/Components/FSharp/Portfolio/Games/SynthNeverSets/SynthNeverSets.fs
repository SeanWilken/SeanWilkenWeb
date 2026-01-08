module Components.FSharp.Portfolio.Games.SynthNeverSets

open Feliz
open SharedViewModule.SharedMicroGames


type Msg =
    // round messages
    | Ignore // I really don't like this, how to assign NO message..need separate funcs
    | ResetRound // resets the board and player round details
    | EndRound // you crashed and will be brought to game over screen
    | ExitGameLoop // Call to ensure no window intervals running when game is exited
    | QuitGame // Leave this game and return to the code gallery

let instructionLines = [
    "- 🖱 MOVE MOUSE to STEER."
    "- ⚠ FAST MOVEMENTS = OVERSTEER."
    "- 🛡 COLLECT SHIELDS for PROTECTION."
    "- 🎯 CENTER OVER POTHOLES to AVOID DAMAGE🎯 CENTER OVER POTHOLES to AVOID DAMAGE."
    "- 💀 ONE HIT = GAME OVER."
]

let View (model: unit) (dispatch: Msg -> unit) (quitMsg: 'parentMsg) (dispatchParent: 'parentMsg -> unit) =
    // let overlay : ReactElement option =
    //     match model.GameState with
    //     | Won ->
    //         Some (
    //             WinOverlay 0
    //                 (fun () -> dispatch ResetRound)
    //                 (Some (fun () -> dispatch (LoadRound (model.LevelIndex + 1))))
    //         )
    //     | _ -> None

    CyberShellResponsive {
        Left = 
            Html.div 
                [
                    TitlePanel "Synth Never Sets"
                    InstructionsPanel 
                        "HOW TO PLAY" 
                        instructionLines
                        ""
                        (fun () -> ())
                ]
        Board = BoardPanel ( TSXDemos.SynthNeverSets() )
        Overlay = None // overlay
        OnQuit = (fun () -> dispatchParent quitMsg)
    }
