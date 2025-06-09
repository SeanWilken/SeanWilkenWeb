module AboutSection

open Feliz
open Shared
open Elmish

// type Msg =
//     | ToggleModal of int
//     | SwitchModal of int
//     | SwitchSection of SharedWebAppViewSections.AppSection

type DirectoryTileDetails = {
    Header : string
    SubHeader : string
    Image : string option
}

type DirectoryButton = {
    ButtonTitle : string
    ButtonMsg : SharedAboutSection.Msg
}


// General Tile Level
// let aboutGeneralTileDetails = Some {
//     Header = "General"
//     SubHeader = "I wrote, designed, deployed and am hosting this website as a way to demonstrate some of my abilities & explain a little about me, in case you don't already know."
//     Image = None
// }
// let aboutGeneralTileDirectoryButton = Some {
//     ButtonTitle = "Read More"
//     ButtonMsg = ToggleModal 0
// }
// let aboutGeneralTileImage = Some "./imgs/Out for Blood.jpeg"

// // Personal Tile Level
// let aboutPersonalTileDetails = Some {
//     Header = "Personal"
//     SubHeader = "Who am I? Well I'm a person just like you, unless of course, you're a bot..."
//     Image = None
// }
// let aboutPersonalTileDirectoryButton = Some {
//     ButtonTitle = "Read More"
//     ButtonMsg = ToggleModal 2
// }
// // Personal Full Tile Level Images
// let aboutPersonalFullTileImages = Some [ 
//     "./imgs/Kcuf Em.jpeg"
//     "./imgs/Misfortune.jpeg"
// ]

// // Professional Tile Level
// let aboutProfessionalTileDetails = Some {
//     Header = "Industry"
//     SubHeader = "This is what it should look like when play time is over.."
//     Image = None
// }
// let aboutProfessionalTileDirectoryButton = Some {
//     ButtonTitle = "Read More"
//     ButtonMsg = ToggleModal 1
// }
// let aboutProfessionalTileImage = Some "./imgs/Caution Very Hot.jpeg"

// // Code Tile
// let codeGalleryDirectoryButtonDetails = Some {
//     Header = "Code Gallery"
//     SubHeader = "Check out some simple games and their code.."
//     Image = Some "./imgs/Harlot.jpeg"
// }
// let codeGalleryDirectoryButton = Some {
//     ButtonTitle = "Code"
//     ButtonMsg = SwitchSection ( SharedWebAppViewSections.PortfolioAppCodeView )
// }

// // Portfolio Landing Tile
// let portfolioDirectoryButtonDetails = Some {
//     Header = "Portfolio"
//     SubHeader = "Watch your back and check this out."
//     Image = Some "./imgs/Backstabber.jpeg"
// }
// let portfolioDirectoryButton = Some {
//     ButtonTitle = "Portfolio"
//     ButtonMsg = SwitchSection ( SharedWebAppViewSections.PortfolioAppLandingView )
// }

// // Art Gallery Tile
// let artGalleryDirectoryButtonDetails = Some {
//     Header = "Design Gallery"
//     SubHeader = "I draw things sometimes, some of which I actually kinda like."
//     Image = Some "./imgs/Bowing Bubbles.jpeg"
// }
// let artGalleryDirectoryButton = Some {
//     ButtonTitle = "Designs"
//     ButtonMsg = SwitchSection ( SharedWebAppViewSections.PortfolioAppDesignView )
// }

// // ADD IMAGES AND THINGS TO MAKE THIS BE A GENERIC LAYOUT PASSED THE CONTENT MODAL / VIEW
// // Switched to single string for now, kind of liked how it looked as list iterated and broken
// // down into sentences / points. (MAYBE USE BULLET POINTS)
// let generalModalContent = {
//     Title = "General"
//     MainContent =
//         """I wrote all the code from a basic SAFE stack boilerplate, which gets deployed to Azure via a FAKE script.
//           Check out the portfolio section for some example demo's, check out the source code that
//           comprises the different sections and the website itself, or take a peak at some drawings...
//           Check back frquently to see what's new, as I plan to update this with new features, games and content.
//           You can find all the code for the site on my Github if you want to review some sections in depth, get an idea
//           of how I leverage the technologies or solve some domain or logistical issues.
//           """
//         (*
//         *)
//     PreviousLabel = "Welcome"
//     NextLabel = "Industry"
// }
// let professionalModalContent = {
//     Title = "Industry"
//     MainContent =
//         (*
//         I've worked: with mid and small team sizes, working well with others or alone, with custom solutions, open source projects, many late nights tinkering, 
//         searching for and fixing bugs, deploying new code and putting out the fires, working with clients to come up with solutions for problems and bottlenecks being faced.
//         Source requirements, come up with timelines, architecture and logical solutions for such work and implemented the final custom solutions that get deployed 
//         into production environments.
//         In that time, I've been a: full-stack developer, tester, help-desk / support, 
//         requirement gatherer, custom integration specialist and a lot more..
//         I've professionally developed, implemented and maintained things like custom data processors
//         / data integrations / themes / websites / projects & solutions, and created many more personal hobby projects (such as this site, Unity projects, etc..) and scripts.
//         *)
//         """As a mainly self-taught computer programmer, I am constantly looking for new and interesting aspects of technology. 
//         I've worked in a variety of positions and dealt with a number of duites that a full stack developer or software engineer may have to go through. 
//         With daily duties ranging from addressing clients directly to source requirements or troubleshoot issues, to implementing custom integrations and solutions.
//         I enjoy expanding my skill set by learning new languages, practices and design patterns and having the freedom to solve complex issues by thinking critically and creatively."""
//     PreviousLabel = "General"
//     NextLabel = "Personal"
// }
// let personalModalContent = {
//     Title = "Personal"
//     MainContent =
//         """I'm pretty laid back and enjoy living life in the momement, learning and experiencing new things. I like being challenged and adapting
//         to problems that present themselves along the way. Fun fact: I've sailed across the Carribean Sea back to the states and driven across the United States cross twice..."""
//     PreviousLabel = "Industry"
//     NextLabel = "Portfolio"
// }

// let aboutModalContentSections = [ generalModalContent; professionalModalContent; personalModalContent ]

// // Update lifecycle ---------

// // fulma timeline to walk through timeline of events
let toggleModal (model: SharedAboutSection.Model) index =
    let activeModal = if ( index <> model.ActiveModalIndex ) then { model with ActiveModalIndex = index } else model
    { activeModal with ModalIsActive = not model.ModalIsActive }

let update msg model : SharedAboutSection.Model * Elmish.Cmd<SharedAboutSection.Msg> =
    match msg with
    | SharedAboutSection.ToggleModal int ->
        let toggled = toggleModal model int
        toggled, Cmd.none
    | SharedAboutSection.SwitchModal int ->
        match int with
        | 1 -> { model with ActiveModalIndex = model.ActiveModalIndex + 1 }, Cmd.none
        | -1 -> { model with ActiveModalIndex = model.ActiveModalIndex - 1}, Cmd.none
        | _ -> model, Cmd.none
    | _ -> model, Cmd.none


// Modal content records as in your original code (omitted here for brevity)
open Shared.SharedAboutSection

let aboutModalCard (modalContent: Shared.SharedAboutSection.ModalContent) =
    Html.div [
        prop.className "generalContentCard p-4 bg-base-200 rounded-lg shadow-md"
        prop.children [
            Html.p [ prop.text modalContent.MainContent ]
        ]
    ]


let aboutModalContentSections = [
    {
        Title = "General"
        MainContent = """
            I wrote all the code from a basic SAFE stack boilerplate, which gets deployed to Azure via a FAKE script.
            Check out the portfolio section for some example demo's, check out the source code that
            comprises the different sections and the website itself, or take a peek at some drawings...
            Check back frequently to see what's new, as I plan to update this with new features, games and content.
            You can find all the code for the site on my Github if you want to review some sections in depth, get an idea
            of how I leverage the technologies or solve some domain or logistical issues.
        """
        PreviousLabel = "Welcome"
        NextLabel = "Industry"
    }
    {
        Title = "Industry"
        MainContent = """
            As a mainly self-taught computer programmer, I am constantly looking for new and interesting aspects of technology. 
            I've worked in a variety of positions and dealt with a number of duties that a full stack developer or software engineer may have to go through. 
            With daily duties ranging from addressing clients directly to sourcing requirements or troubleshooting issues, to implementing custom integrations and solutions.
            I enjoy expanding my skill set by learning new languages, practices and design patterns and having the freedom to solve complex issues by thinking critically and creatively.
        """
        PreviousLabel = "General"
        NextLabel = "Personal"
    }
    {
        Title = "Personal"
        MainContent = """
            I'm pretty laid back and enjoy living life in the moment, learning and experiencing new things. I like being challenged and adapting
            to problems that present themselves along the way. Fun fact: I've sailed across the Caribbean Sea back to the states and driven across the United States four times...
        """
        PreviousLabel = "Industry"
        NextLabel = "Portfolio"
    }
]


let aboutModal (model: Shared.SharedAboutSection.Model) modalContent dispatch =
    SharedViewModule.sharedViewModal
        model.ModalIsActive
        ( SharedViewModule.sharedModalHeader
            (aboutModalContentSections.Item(model.ActiveModalIndex).Title)
            (ToggleModal model.ActiveModalIndex)
            dispatch )
        (aboutModalCard modalContent)
        (Html.span [])

let aboutTileDetailView (tileDetails: DirectoryTileDetails option) =
    match tileDetails with
    | Some details ->
        Html.div [
            prop.className "generalContentCardTextBackground p-6 rounded-lg bg-base-100 shadow"
            prop.children [
                Html.div [
                    Html.h1 [
                        prop.className "text-5xl font-bold mb-2"
                        prop.text details.Header
                    ]
                    Html.h2 [
                        prop.className "text-xl font-exo font-normal mb-4"
                        prop.text details.SubHeader
                    ]
                    match details.Image with
                    | Some imgSrc -> Html.img [ prop.className "rounded-lg shadow-lg max-w-full"; prop.src imgSrc ]
                    | None -> Html.none
                ]
            ]
        ]
    | None -> Html.none

let aboutTileButtonView (tileButton: DirectoryButton option) dispatch =
    match tileButton with
    | Some btn ->
        SharedViewModule.sharedSwitchSectionButton btn.ButtonMsg btn.ButtonTitle dispatch
    | None -> Html.none

let aboutTileImageView (tileImage: string option) =
    match tileImage with
    | Some imgSrc ->
        Html.div [
            prop.className "tile-child w-1/3 p-4"
            prop.children [
                Html.img [ prop.className "rounded-lg shadow-md max-w-full"; prop.src imgSrc ]
            ]
        ]
    | None -> Html.none

let aboutTileImagesFullView (tileImages: string list option) =
    match tileImages with
    | Some images ->
        Html.div [
            prop.className "container mx-auto p-4 flex flex-wrap justify-center gap-4"
            prop.children (
                images
                |> List.map (fun imgSrc ->
                    Html.img [ prop.className "rounded-lg shadow-md max-w-xs"; prop.src imgSrc ]
                )
            )
        ]
    | None -> Html.none

let aboutTileDetailsView (tileDetails: DirectoryTileDetails option) (tileButton: DirectoryButton option) dispatch =
    if tileDetails.IsNone && tileButton.IsNone then Html.none
    else
        Html.div [
            prop.className "tile-child w-2/3 mx-auto p-6"
            prop.children [
                aboutTileDetailView tileDetails
                aboutTileButtonView tileButton dispatch
            ]
        ]

let aboutTileDetailsLevel tileDetails tileButton tileImage dispatch =
    Html.div [
        prop.className "generalContentCard p-4 bg-base-200 rounded-lg shadow-lg mb-6"
        prop.children [
            Html.div [
                prop.className "flex items-center justify-center space-x-8"
                prop.children [
                    Html.div [ prop.className "flex-1" ]
                    aboutTileImageView tileImage
                    Html.div [ prop.className "flex-1" ]
                    aboutTileDetailsView tileDetails tileButton dispatch
                    Html.div [ prop.className "flex-1" ]
                ]
            ]
        ]
    ]

let aboutTileDetailsFullView tileDetails tileButton tileImages dispatch =
    Html.div [
        prop.className "generalContentCard p-4 bg-base-200 rounded-lg shadow-lg mb-6"
        prop.children [
            Html.div [
                prop.className "flex flex-col items-center justify-center space-y-6"
                prop.children [
                    Html.div [
                        prop.className "w-full text-center"
                        prop.children [
                            aboutTileDetailView tileDetails
                            aboutTileButtonView tileButton dispatch
                        ]
                    ]
                    aboutTileImagesFullView tileImages
                ]
            ]
        ]
    ]


let aboutDirectory dispatch =
    Html.div [
        prop.className "tile-ancestor p-6"
        prop.children [
            Html.div [
                prop.className "tile-parent flex justify-around space-x-4"
                prop.children [
                    aboutTileDetailsView 
                        (Some { Header = "General"; SubHeader = "I wrote, designed, deployed and am hosting this website as a way to demonstrate some of my abilities & explain a little about me, in case you don't already know."; Image = None } )
                        (Some { ButtonTitle = "Read more"; ButtonMsg = ToggleModal 0 })
                        dispatch
                    aboutTileDetailsView
                        (Some { Header = "Industry"; SubHeader = "This is what it should look like when play time is over.."; Image = None } )
                        (Some { ButtonTitle = "Read more"; ButtonMsg = ToggleModal 1 })
                        dispatch
                    aboutTileDetailsView
                        (Some { Header = "Personal"; SubHeader = "Who am I? Well I'm a person just like you, unless of course, you're a bot..."; Image = None } )
                        (Some { ButtonTitle = "Read more"; ButtonMsg = ToggleModal 2 })
                        dispatch
                    // aboutTileDetailsView artGalleryDirectoryButtonDetails artGalleryDirectoryButton dispatch
                ]
            ]
        ]
    ]

let view model dispatch =
    Html.div [
        aboutModal model (aboutModalContentSections.Item(model.ActiveModalIndex)) dispatch
        // aboutTileDetailsLevel aboutGeneralTileDetails aboutGeneralTileDirectoryButton aboutGeneralTileImage dispatch
        // aboutTileDetailsFullView aboutPersonalTileDetails aboutPersonalTileDirectoryButton aboutPersonalFullTileImages dispatch
        // aboutTileDetailsLevel aboutProfessionalTileDetails aboutProfessionalTileDirectoryButton aboutProfessionalTileImage dispatch
        aboutDirectory dispatch
    ]
