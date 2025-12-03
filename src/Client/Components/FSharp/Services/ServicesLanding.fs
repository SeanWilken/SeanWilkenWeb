module Components.FSharp.Services.Landing

open Feliz
open Shared
open Bindings.LucideIcon
open Shared.SharedWebAppModels
open Shared.SharedWebAppViewSections
open Components.FSharp.Layout.MultiContent

let tileContents: TileContent list = [
    {
        Title = "Website"
        Summary = "Discover the story behind this site, the technology stack, and the philosophy that drives its design."
        Details = """
            This site is a hands-on showcase of my approach to modern web development. Built from the ground up using the SAFE stack (Saturn, Azure, Fable, Elmish), it demonstrates how functional programming and strong typing can create robust, maintainable applications.

            There is a mix of F# and Typescript components loaded throughout the website, to demonstrate the power of leveraging both languages complementary to one another through bindings.
            
            The deployment pipeline uses FAKE for automated builds and Azure for cloud hosting, ensuring reliability and scalability. Custom layouts, Elmish update loops, and a modular architecture make the site both flexible and easy to extend.
            
            Beyond the technical, this site reflects my commitment to clarity, usability, and developer experience. Every page is crafted to be both informative and interactive, with real code experiments and demos you can launch and explore.
        """
        Icon = LucideIcon.BookOpen "w-6 h-6"
        Image = Some "./img/josh-boak-unsplash-overview.jpg"
    }
    {
        Title = "Industry"
        Summary = "A look at my professional journey, the problems I've solved, and the impact I've made across teams and projects."
        Details = """
            My career spans full-stack development, architecture, and technical leadership. I've built tools that empower teams, designed systems that scale, and shipped products that solve real business challenges.
            
            From collaborating with clients to define requirements, to integrating complex backend services, my work is rooted in understanding both the technical and human sides of software. I thrive in environments where learning is constant and where the right solution matters more than the easy one.
            
            Whether it's mentoring junior developers, leading code reviews, or architecting new features, I bring a focus on quality, communication, and long-term value. My experience covers web, cloud, and desktop, with a passion for automation and developer tooling.
        """
        Icon = LucideIcon.Briefcase "w-6 h-6"
        Image = Some "./img/bernd-dittrich-unsplash-office.jpg"
    }
    {
        Title = "Personal"
        Summary = "Get to know the person behind the code: my interests, adventures, and what drives me outside of work."
        Details = """
            Curiosity and adaptability are at the heart of who I am. I love learning by doing, whether that's sailing across the Caribbean, driving coast-to-coast, or picking up a new technology just for fun.
            
            I believe in the value of challenge: every problem is an opportunity to grow, and every experience adds a new perspective. Outside of coding, you'll find me exploring the world, tinkering with side projects, or kicking back with my friends and family.
            
            My personal philosophy is simple: stay curious, help others, and never stop improving. Life is best lived with a sense of adventure and a willingness to try new things.
        """
        Icon = LucideIcon.UserCircle "w-6 h-6"
        Image = Some "./img/sailing-1.JPG"
    }
]

[<ReactComponent>]
let view (model: Shared.SharedServices.Model) dispatch =
    Html.div [
        prop.className ""
        prop.children [
            // Html.h1 [
            //     prop.className "clash-font text-4xl text-center mb-12 text-primary"
            //     prop.text "Services"
            // ]

            // Selection Row
            Html.div [
                prop.className "flex flex-col md:flex-row gap-6 mb-12"
                prop.children (

                    Components.Layout.LayoutElements.SectionCarousel {|
                        Title = "Professional Services"
                        Items = [
                            {
                                IconElement = LucideIcon.BrainCircuit "w-6 h-6"
                                Title = "AI Services"
                                Description = "Harness the power of AI and agentic networks to help propel your business well into the future"
                            }
                            {
                                IconElement = LucideIcon.ShoppingCart "w-6 h-6"
                                Title = "Integrations"
                                Description = "Connect systems together through API and custom middleware solutions"
                            }
                            {
                                IconElement = LucideIcon.JoyStick "w-6 h-6"
                                Title = "Web Development"
                                Description = "From custom websites to progressive web apps, there's something for everyone"
                            }
                            {
                                IconElement = LucideIcon.MegaPhone "w-6 h-6"
                                Title = "Sale Platform"
                                Description = "How would you like to maintain full creative control to match the dynamcy of your up and coming brand?"
                            }
                        ]
                    |}

                )
            ]

            match model.CurrentSection with
            | AI ->
                Components.FSharp.Services.AIAutomation.AISalesDeck.AiSalesPage (ServicesMsg >> dispatch)
            | _ ->
                Components.FSharp.Services.AIAutomation.AISalesDeck.AiSalesPage (ServicesMsg >> dispatch)


            // Content / Fallback Section
            // match model. with
            // | Some idx when idx < tileContents.Length ->
            //     Html.div [
            //         prop.className "mt-16"
            //         prop.children [
            //             selectedContentDisplay (Some tileContents[idx])
            //         ]
            //     ]
            // | _ ->
            //     Html.div [
            //         prop.className "mt-16 bg-base-200 p-10 rounded-xl shadow-inner text-center space-y-8"
            //         prop.children [
            //             Html.h2 [
            //                 prop.className "text-3xl font-extrabold text-primary"
            //                 prop.text "Want to see what I've built?"
            //             ]

            //             Html.p [
            //                 prop.className "text-base-content/80 max-w-xl mx-auto"
            //                 prop.text "Explore my projects, demos, and source code, from interactive games to full-stack tools."
            //             ]

            //             Html.div [
            //                 prop.className "flex flex-wrap gap-4 justify-center items-center pt-4"
            //                 prop.children [
            //                     Html.img [
            //                         prop.src "./img/project-1-thumb.jpg"
            //                         prop.className "rounded-lg max-h-24 w-auto object-cover shadow"
            //                         prop.alt "Project preview"
            //                     ]
            //                     Html.img [
            //                         prop.src "./img/project-2-thumb.jpg"
            //                         prop.className "rounded-lg max-h-24 w-auto object-cover shadow"
            //                         prop.alt "Project preview"
            //                     ]
            //                 ]
            //             ]

            //             Html.button [
            //                 prop.className "btn btn-primary btn-lg"
            //                 prop.text "Explore Projects"
            //                 prop.onClick (fun _ -> dispatch (Shared.SharedWebAppModels.WebAppMsg.SwitchToOtherApp SharedWebAppViewSections.PortfolioAppLandingView))
            //             ]
            //         ]
            //     ]
        ]
    ]