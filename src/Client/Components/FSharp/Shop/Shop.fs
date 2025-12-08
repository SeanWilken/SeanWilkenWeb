module Components.FSharp.Shop

open System
open System
open Elmish
open Elmish.UrlParser
open Elmish.Navigation
open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Shared.SharedShopDomain
open Client.Domain.SharedShop
open Client.Domain


let sendMessage (_paypalOrderRef: string) : Cmd<ShopMsg> = Cmd.none

// let getAllProductTemplates (request: Shared.Api.Printful.CatalogProductRequest.CatalogProductsQuery) : Cmd<Shared.SharedShop.ShopMsg> =
//     Cmd.OfAsync.either
//         ( fun x -> Client.Api.productsApi.getProductTemplates x )
//         request
//         GotProductTemplates
//         FailedProductTemplates

// let getAllProducts (request: Shared.Api.Printful.CatalogProductRequest.CatalogProductsQuery) : Cmd<Shared.SharedShop.ShopMsg> =
//     Cmd.OfAsync.either
//         ( fun x -> Client.Api.productsApi.getProducts x )
//         request
//         GotAllProducts
//         FailedAllProducts

// let defaultProductsRequest : Shared.Api.Printful.CatalogProductRequest.CatalogProductsQuery = 
//     {
//         category_ids = Some [ 1 ]
//         colors = None
//         destination_country = None
//         limit = None
//         newOnly = None
//         offset = None
//         placements = None
//         selling_region_name = None
//         sort_direction = None
//         sort_type = None
//         techniques = None
//     }

// let getProductVariants (_variantId: int) : Cmd<Shared.SharedShop.ShopMsg> =
//     Cmd.OfAsync.either
//         Client.Api.productsApi.getProductVariants
//         _variantId
//         GotProductVariants
//         FailedProductVariants
        


// --------------------------------
// Init
// --------------------------------

// let init () : Shared.SharedShop.Model * Cmd<Shared.SharedShop.ShopMsg> =
//     {
//         section = ShopSection.ShopLanding
//         // customer = None
//         // productVariationOptionsSelected = (None, None)
//         validSyncVariantId = None
//         payPalOrderReference = None
//         // shoppingBag = []
//         checkoutTaxShipping = (None, None)
//         homeGif = ""
//         // customerSignUpForm = defaultCustomerSignUpForm
//         // customerAddressForm = defaultCustomerAddressForm
//         // validationResults = []
//         allProducts = None
//         // productVariants = None
//         productTemplates = []
//         productTemplate = None
//         buildYourOwn = None
//     }, getAllProducts defaultProductsRequest
    //  getHomeGif



let init shopSection =
    Client.Domain.SharedShop.getInitialModel shopSection
    , Cmd.none // getAllProducts defaultProductsRequest

// --------------------------------
// Helpers
// --------------------------------

// let updateCustomerField (model: Shared.SharedShop.Model) (userFormField: UserSignUpFormField) : Shared.SharedShop.Model =
//     let f = model.customerSignUpForm
//     match userFormField with
//     | FirstNameField v -> { model with customerSignUpForm = { f with firstName = v } }
//     | LastNameField v ->  { model with customerSignUpForm = { f with lastName = v } }
//     | UserNameField v ->  { model with customerSignUpForm = { f with userName = v } }
//     | PasswordField v ->  { model with customerSignUpForm = { f with password = v } }
//     | ConfirmPasswordField v -> { model with customerSignUpForm = { f with confirmPassword = v } }

// let updateAddressField (model: Shared.SharedShop.Model) (addressField: CustomerAddressFormField) : Shared.SharedShop.Model =
//     let a = model.customerAddressForm
//     match addressField with
//     | ShippingFirstNameField v -> { model with customerAddressForm = { a with firstName = v } }
//     | ShippingLastNameField v -> { model with customerAddressForm = { a with lastName = v } }
//     | ShippingStreetAddressField v -> { model with customerAddressForm = { a with streetAddress = v } }
//     | ShippingCityField v -> { model with customerAddressForm = { a with city = v } }
//     | ShippingStateCodeField v -> { model with customerAddressForm = { a with stateCode = v } }
//     | ShippingCountryCodeField v -> { model with customerAddressForm = { a with countryCode = v } }
//     | ShippingZipCodeField v -> { model with customerAddressForm = { a with zipCode = v } }

// // Refactor: Option.map2 + tryFind to check variant exists
// let checkProductSelectionsAreValidVariant (model: Shared.SharedShop.Model) (syncProduct: SyncProduct) : Shared.SharedShop.Model * Cmd<Shared.SharedShop.ShopMsg> =
//     let (mc, ms) = model.productVariationOptionsSelected
//     let maybeVariant =
//         Option.map2
//             (fun c s -> syncProduct.productVariations |> List.tryFind (fun v -> v.variantColor = c && v.variantSize = s))
//             mc ms
//         |> Option.bind id
//     let newModel = { model with validSyncVariantId = maybeVariant |> Option.map (fun v -> v.externalSyncVariantId) }
//     newModel, Cmd.none

// Add/merge shopping-bag quantities instead of duplicating line items
// let addVariantToShoppingBag (variant: SyncProductVariant) (bag: (SyncProductVariant*int) list) : (SyncProductVariant*int) list =
//     let eq v1 v2 = v1.externalSyncVariantId = v2.externalSyncVariantId
//     let existing, others = bag |> List.partition (fun (v,_) -> eq v variant)
//     match existing with
//     | (v, qty) :: _ -> (v, qty + 1) :: others
//     | [] -> (variant, 1) :: bag

// let adjustLineItemQuantity (adj: QuantityAdjustment) (itemVariant: SyncProductVariant) (bag: (SyncProductVariant*int) list) : (SyncProductVariant*int) list =
//     bag
//     |> List.map (fun (v, q) ->
//         if v = itemVariant then
//             match adj with
//             | Increment -> v, q + 1
//             | Decrement -> v, (if q - 1 <= 0 then 1 else q - 1)
//         else v, q)

// --------------------------------
// Update
// --------------------------------

let update (msg: ShopMsg) (model: Model) : Model * Cmd<ShopMsg> =
    match msg with
    | NavigateTo section ->
        // need to do url here

        { model with Section = section }, Cmd.none
        // Navigation.newUrl (toPath (Some Landing))

    | ShopMsg.ShopLanding msg ->
        match msg with
        | SharedShopV2Domain.ShopLandingMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)

    | ShopMsg.ShopTypeSelection msg ->
        match msg with
        | SharedShopV2Domain.ShopTypeSelectorMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)

    | ShopMsg.ShopBuildYourOwnWizard msg ->
        match model.Section, msg with
        | SharedShopV2.ShopSection.BuildYourOwnWizard _, SharedShopV2Domain.ShopBuildYourOwnProductWizardMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)
        | SharedShopV2.ShopSection.BuildYourOwnWizard byow, _ ->
            let newModel, childCmd = CreateYourOwnProduct.update msg byow
            { model with Section = SharedShopV2.ShopSection.BuildYourOwnWizard newModel },
            Cmd.map ShopMsg.ShopBuildYourOwnWizard childCmd

        | _ ->
            { model with Section = SharedShopV2.ShopSection.BuildYourOwnWizard (SharedShopV2.BuildYourOwnProductWizard.initialState ()) },
            Cmd.none

    | ShopMsg.ShopStoreProductTemplates msg ->

        match model.Section, msg with
        | SharedShopV2.ShopSection.ProductTemplateBrowser _, SharedShopV2Domain.ShopProductTemplatesMsg.SwitchSection section ->
            model, Cmd.ofMsg (NavigateTo section)
        | SharedShopV2.ShopSection.ProductTemplateBrowser ptb, _ ->
            let newModel, childCmd = Components.FSharp.Pages.ProductTemplateBrowser.update msg ptb
            { model with Section = SharedShopV2.ShopSection.ProductTemplateBrowser newModel },
            Cmd.map ShopMsg.ShopStoreProductTemplates childCmd
        | _ ->
            { model with Section = SharedShopV2.ShopSection.ProductTemplateBrowser (SharedShopV2.ProductTemplate.ProductTemplateBrowser.initialModel ()) },
            Cmd.none

    // | UpdateCustomerForm fld ->
    //     updateCustomerField model fld, Cmd.none

    // | UpdateAddressForm fld ->
    //     updateAddressField model fld, Cmd.none

    // | CheckUserSignUpForm ->
    //     model.customerSignUpForm
    //     |> checkUserFormFields
    //     |> checkCustomerSignUpFormValidationResponses model.customerSignUpForm
    //     |> fun res ->
    //         match res with
    //         | SignUpSuccess cust -> { model with customer = Some cust; validationResults = [ SuccessfulRequest "Created User" ] }, Cmd.none
    //         | SignUpFailed fails -> { model with validationResults = fails }, Cmd.none

    // | CheckAddressForm ->
    //     let addrResults = checkAddressFields model.customerAddressForm
    //     match model.customer with
    //     | Some cust ->
    //         // apply address to the *existing* customer when valid
    //         addrResults
    //         |> checkCustomerAddressFormValidationResponses cust model.customerAddressForm
    //         |> function
    //            | SignUpSuccess updatedCust -> { model with customer = Some updatedCust; validationResults = [ SuccessfulRequest "Address saved" ] }, Cmd.none
    //            | SignUpFailed fails -> { model with validationResults = fails }, Cmd.none
    //     | None ->
    //         // guest checkout: just record validation results; do not bind to missing customer
    //         let ok = addrResults |> List.forall checkValidationResult
    //         if ok then { model with validationResults = [ SuccessfulRequest "Address valid" ] }, Cmd.none
    //         else { model with validationResults = addrResults |> List.filter (checkValidationResult >> not) }, Cmd.none

    // | UpdateProductColor (syncProduct, color) ->
    //     let mc, ms = model.productVariationOptionsSelected
    //     let mc' = match mc with Some c when c = color -> None | _ -> Some color
    //     let m' = { model with productVariationOptionsSelected = (mc', ms) }
    //     checkProductSelectionsAreValidVariant m' syncProduct

    // | UpdateProductSize (syncProduct, size) ->
    //     let mc, ms = model.productVariationOptionsSelected
    //     let ms' = match ms with Some s when s = size -> None | _ -> Some size
    //     let m' = { model with productVariationOptionsSelected = (mc, ms') }
    //     checkProductSelectionsAreValidVariant m' syncProduct


    // | ShopMsg.RemoveProductFromBag variantId ->
    //     { model with shoppingBag = model.shoppingBag |> List.filter (fun (v, _) -> v.id <> variantId) }, Cmd.none

    // | AddVariantToShoppingBag variant ->
    //     { model with shoppingBag = addVariantToShoppingBag variant model.shoppingBag }, Cmd.none

    // | DeleteVariantFromShoppingBag variant ->
    //     let bag' = model.shoppingBag |> List.filter (fun (p,_) -> p.externalSyncVariantId <> variant.externalSyncVariantId)
    //     { model with shoppingBag = bag' }, Cmd.none

    // | AdjustLineItemQuantity (adj, bagLineItem) ->
    //     { model with shoppingBag = adjustLineItemQuantity adj bagLineItem model.shoppingBag }, Cmd.none

    // | GotResult result ->
    //     match result with
    //     | Ok response -> { model with homeGif = response }, Cmd.none
    //     | Error _ -> model, Cmd.none

    // | TestApiTaxRate -> model, testApiGetTaxRate model.customerAddressForm

    // | GotTaxRateResult result ->
    //     match result with
    //     | Ok response ->
    //         let taxValue = if response.taxRequired then Some response.taxRate else Some 0.0
    //         { model with checkoutTaxShipping = (taxValue, snd model.checkoutTaxShipping) }, Cmd.none
    //     | Error _ ->
    //         { model with validationResults = model.validationResults @ [ FailedRequest "We couldn’t retrieve tax information. Please try again." ] }, Cmd.none

    // | TestApiShippingRate -> model, testApiGetShippingRate model.customerAddressForm model.shoppingBag

    // | GotShippingRateResult result ->
    //     match result with
    //     | Ok (shipRates) ->
    //         match shipRates |> List.tryHead with
    //         | Some ship ->
    //             match Double.TryParse ship.shippingRate with
    //             | true, s -> { model with checkoutTaxShipping = (fst model.checkoutTaxShipping, Some s) }, Cmd.none
    //             | _ -> { model with validationResults = model.validationResults @ [ FailedRequest "Could not read shipping rate." ] }, Cmd.none
    //         | None -> { model with validationResults = model.validationResults @ [ FailedRequest "No shipping options returned." ] }, Cmd.none
    //     | Error _ -> { model with validationResults = model.validationResults @ [ FailedRequest "Shipping rate lookup failed." ] }, Cmd.none

    // | TestApiCustomerDraft customerDraft ->
    //     model, Cmd.batch [ testApiCreateDraftOrder customerDraft; Navigation.newUrl "/payment" ]

    // | GotCustomerOrderDraftResult result ->
    //     match result with
    //     | Ok response -> { model with payPalOrderReference = Some response.code }, Cmd.none
    //     | Error _ -> { model with validationResults = model.validationResults @ [ FailedRequest "Could not create order draft." ] }, Cmd.none

    // | Send ->
    //     let id = defaultArg model.payPalOrderReference ""
    //     model, sendMessage id

    // | GetProductTemplates -> 
    //     model, getAllProductTemplates defaultProductsRequest

    // | GotProductTemplates productResult ->
    //     { model with productTemplates = productResult.templateItems }, Cmd.none

    // | FailedProductTemplates ex ->
    //     Console.WriteLine $"ex: {ex.Message}"
    //     model, Cmd.none
        
    // | GetAllProducts -> 
    //     model, getAllProducts defaultProductsRequest

    // | GotAllProducts productResult ->
    //     { model with allProducts = Some productResult.products }, Cmd.none
    // | FailedAllProducts ex ->
    //     Console.WriteLine $"ex: {ex.Message}"
    //     model, Cmd.none
        
    // // | GetProductVariants variantId -> model, getProductVariants variantId
    // // | FailedProductVariants ex -> model, Cmd.none

    // // | GotProductVariants variantResult ->
    // //     { model with productVariants = Some variantResult }, Cmd.none

    // | ShopMsg.BuildYourOwnProductMsg msg ->
    //     let byoUpdated =
    //         match model.buildYourOwn with
    //         | None -> SharedShopV2.BuildYourOwnProductWizard.initialState (model.allProducts |> Option.defaultValue []) 
    //         | Some byo -> CreateYourOwnProduct.update msg byo
    //     let updatedModel = { model with buildYourOwn = Some byoUpdated; }
    //     // match msg with
    //     // | BuildYourOwnProductMsg.Msg.AddProductToBag product ->
    //     //     { updatedModel with shoppingBag = [ (product, 1) ] }, Cmd.none
    //     // | _ ->
    //     updatedModel, Cmd.none
            
open Feliz

let pathToTitleString (path: string) =
    path.Replace ("/", " ")

let headerLink (linkTitle: string) =
    Html.a [
        prop.href linkTitle
        prop.className "navigationLink satorshi-font"
        prop.text (pathToTitleString linkTitle)
    ]

// let shoppingBagLink (bagItems: int) (linkTitle: string) =
//     Html.a [
//         prop.href linkTitle
//         prop.className "navigationLink satorshi-font"
//         prop.text $"{pathToTitleString linkTitle}: {bagItems}"
//     ]

// let headerNavigationLanding =
//     Html.a [
//         prop.href "/index.html"
//         prop.className "headerNavigationTitle clash-font"
//         prop.text "Xero Effort"
//     ]

// let header numBagItems dispatch =
//     Html.div [
//         prop.className "headerNavigation flex items-center justify-between p-4 bg-base-200 shadow-md"
//         prop.children [
//             Html.div [
//                 prop.className "navigationControls flex gap-4"
//                 prop.children [
//                     headerNavigationLanding
//                     headerLink "/home"
//                     headerLink "/shop"
//                     headerLink "/social"
//                     headerLink "/contact"
//                     headerLink "/signup"
//                     shoppingBagLink numBagItems "/shoppingBag"
//                 ]
//             ]
//         ]
//     ]

let spanOrControl (maybeControl: ReactElement option) : ReactElement =
    match maybeControl with
    | Some control -> control
    | None -> Html.span []

let contentHeader (sectionTitle: string) (contentNavigation: ReactElement option) : ReactElement =
    Html.div [ 
        prop.className "contentNavigation"
        prop.children [
            Html.div [ 
                prop.className "navigationControls"
                prop.children [
                    Html.div [ 
                        prop.className "headerContentTitle"
                        prop.children [
                            Html.text sectionTitle
                            spanOrControl contentNavigation
                        ]
                    ]
                ]
            ]
        ]
    ]

open Feliz
open Shared
open Bindings.LucideIcon

// For now, just hard-code your hero video path.
// Later we can make this configurable or pull from CMS.
let heroVideoUrl = "/media/xero-effort-hero.mp4"

// Optional: fallback image (poster) if video fails or while loading
let heroPosterUrl = "/img/xero-effort/hero-poster.jpg"

let private featuredDrops (homeGifUrls: string list) =
    let cards =
        homeGifUrls
        |> List.mapi (fun idx url ->
            Html.div [
                prop.key (string idx)
                prop.className
                    "rounded-3xl overflow-hidden shadow-lg border border-base-300/60 \
                     bg-base-100/90 hover:-translate-y-[3px] hover:shadow-xl transition-transform duration-200"
                prop.children [
                    Html.img [
                        prop.src url
                        prop.alt (sprintf "Featured drop %d" (idx + 1))
                        prop.className "w-full h-full object-cover"
                    ]
                ]
            ])

    Html.section [
        prop.className "max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 pt-10 pb-16 space-y-4"
        prop.children [
            Html.div [
                prop.className "flex items-center justify-between gap-3"
                prop.children [
                    Html.div [
                        Html.h2 [
                            prop.className "text-sm font-semibold tracking-wide uppercase"
                            prop.text "Featured drops"
                        ]
                        Html.p [
                            prop.className "text-[11px] text-base-content/60"
                            prop.text "Just a taste. Collections go deeper."
                        ]
                    ]
                ]
            ]
            Html.div [
                prop.className "grid gap-6 md:grid-cols-2"
                prop.children cards
            ]
        ]
    ]

// let hero dispatch =
//     Html.section [
//         prop.className
//             "relative w-full min-h-[75vh] flex flex-col justify-center items-center \
//              bg-black text-white overflow-hidden"

//         prop.children [

//             // subtle animated noise overlay (we'll add css below)
//             Html.div [ prop.className "absolute inset-0 pointer-events-none hero-noise" ]

//             Html.h1 [
//                 prop.className "text-6xl font-black tracking-tight text-center"
//                 prop.text "XERO EFFORT"
//             ]

//             Html.p [
//                 prop.className "mt-3 text-sm text-white/70 uppercase tracking-[0.15em]"
//                 prop.text "limited-time drops • never to be heard from again"
//             ]

//             Html.button [
//                 prop.className "mt-10 btn btn-sm px-6 bg-white text-black hover:bg-white/80"
//                 prop.onClick (fun _ -> dispatch (NavigateTo SharedShopV2.ShopSection.ShopTypeSelector))
//                 prop.text "Enter collections →"
//             ]
//         ]
//     ]


let hero dispatch =
    Html.section [
        prop.className
            "relative w-full min-h-[75vh] flex flex-col justify-center items-center \
             bg-black text-white overflow-hidden"

        prop.children [

            // fog, no PNG
            Html.div [ prop.className "fog-layer" ]

            Html.h1 [
                prop.className "text-6xl font-black tracking-tight text-center"
                prop.text "Xero Effort"
            ]
            Html.h5 [
                prop.className "mt-4 text-sm text-white/60 uppercase  text-center"
                prop.text  "Streetwear, prints, and digital mischief for people who took one step past the EXIT sign on purpose. Out with the old, in with the new—never to be heard from again."
            ]

            Html.p [
                prop.className "mt-4 text-sm text-white/60 uppercase tracking-[0.15em]"
                prop.children [
                    Html.strong [ prop.text "Limited-time drops" ]
                    Html.text " • "
                    Html.strong [ prop.text "No restocks" ]
                    Html.text " • "
                    Html.strong [ prop.text "Get it before it's gone" ]
                ]
                // prop.text "Limited-time drops • No restocks • Digital relics"
            ]

            Html.button [
                prop.className "mt-10 btn btn-sm px-6 bg-white text-black hover:bg-white/80"
                prop.onClick (fun _ -> dispatch (NavigateTo SharedShopV2.ShopSection.ShopTypeSelector))
                prop.text "Enter collections →"
            ]
        ]
    ]

let shopHero dispatch =
    Html.section [
        prop.className
            // "relative w-full min-h-[72vh] sm:min-h-[80vh] overflow-hidden flex items-center justify-center"
            "relative w-full min-h-[75vh] flex flex-col justify-center items-center \
             bg-black text-white overflow-hidden"
        prop.children [

            // 🎥 Background video
            Html.video [
                prop.className "absolute inset-0 w-full h-full object-cover hero-video"
                prop.src "/videos/xero-hero.mp4"      // ⬅️ put your mp4 here
                prop.autoPlay true
                prop.loop true
                prop.muted true
                prop.playsInline true
            ]

            // 🌫 theme-tinted scrim so text is readable in all themes
            Html.div [ prop.className "hero-scrim" ]

            // 🌫 fog + 👁‍🗨 film grain (both theme-tinted via CSS vars)
            Html.div [ prop.className "fog-layer" ]
            Html.div [ prop.className "hero-noise" ]

            // 🧊 foreground content (all using DaisyUI utility colors)
            Html.div [
                prop.className
                    "relative z-10 max-w-3xl px-6 py-10 text-center space-y-4 text-base-content"
                prop.children [

                    // main title
                    Html.h1 [
                        prop.className
                            "text-4xl sm:text-6xl md:text-7xl font-black leading-tight text-primary-content drop-shadow-lg"
                        prop.text "Xero Effort"
                    ]

                    Html.h5 [
                        prop.className "mt-4 text-sm text-white/60 uppercase  text-center"
                        prop.text  "Streetwear, prints, and digital mischief for people who took one step past the EXIT sign on purpose. Out with the old, in with the new—never to be heard from again."
                    ]
                    
                    Html.div [
                        prop.className "flex flex-wrap items-center justify-center gap-3 text-[11px]"
                        prop.children [
                            Html.div [
                                prop.className "flex flex-wrap gap-2"
                                prop.children [
                                    Html.span [
                                        prop.className
                                            "badge badge-sm badge-outline border-white/40 text-white/60"
                                        prop.text "Limited-run drops"
                                    ]
                                    Html.span [
                                        prop.className
                                            "badge badge-sm badge-outline border-white/40 text-white/60"
                                        prop.text "Get it before it's gone"
                                    ]
                                ]
                            ]
                        ]
                    ]

                    // CTA
                    Html.div [
                        prop.className "pt-4"
                        prop.children [
                            Html.button [
                                prop.className
                                    "btn btn-primary rounded-full px-8 gap-2 shadow-lg shadow-primary/40"
                                prop.text "Enter collections"
                                prop.onClick (fun _ ->
                                    dispatch (NavigateTo SharedShopV2.ShopSection.ShopTypeSelector))
                            ]
                        ]
                    ]

                    // tiny disclaimer line
                    Html.p [
                        prop.className "text-[11px]  text-white/60 uppercase pt-2"
                        prop.text "Limited runs. Once it's gone, it only lives in screenshots and regrets."
                    ]
                ]
            ]
        ]
    ]



let homeView (homeGifUrls: string list) dispatch =
    Html.div [
        prop.className "space-y-0" // hero + content stack
        prop.children [
            // hero dispatch
            shopHero dispatch
            featuredDrops homeGifUrls
        ]
    ]

// let viewInput (viewType: string) (placeholder: string) (inputValue: string) toMsg dispatch =
//     Html.div [
//         Html.input [
//             prop.className "input input-bordered w-full max-w-xs satorshi-font"
//             prop.type' viewType
//             prop.placeholder placeholder
//             prop.value inputValue
//             prop.onChange (fun (v: string) -> dispatch (toMsg v))
//         ]
//     ]

// let signUpView (customerSignUpForm: CustomerSignUpForm) (validationResults: RequestResponse list) dispatch =
//     Html.div [
//         prop.className "p-6 flex flex-col gap-4"
//         prop.children [
//             Html.div [
//                 prop.className "contentNavigation clash-font text-2xl"
//                 prop.text "JOIN THE DEGENERATE SIDE"
//             ]
//             Html.div [
//                 prop.className "flex flex-col gap-2"
//                 prop.children [
//                     viewInput "text" "First Name" customerSignUpForm.firstName (fun x -> UpdateCustomerForm (FirstNameField x)) dispatch
//                     viewInput "text" "Last Name" customerSignUpForm.lastName (fun x -> UpdateCustomerForm (LastNameField x)) dispatch
//                     viewInput "text" "User Name" customerSignUpForm.userName (fun x -> UpdateCustomerForm (UserNameField x)) dispatch
//                     viewInput "password" "Password" customerSignUpForm.password (fun x -> UpdateCustomerForm (PasswordField x)) dispatch
//                     viewInput "password" "Confirm Password" customerSignUpForm.confirmPassword (fun x -> UpdateCustomerForm (ConfirmPasswordField x)) dispatch
//                     // Validation results mapped here (same as your Elm)
//                     Html.div [
//                         match validationResults with
//                         | [SuccessfulRequest _] -> Html.p [ prop.className "text-green-500"; prop.text "Ok, no errors." ]
//                         | results ->
//                             Html.ul [
//                                 prop.className "text-red-500 list-disc list-inside"
//                                 prop.children [
//                                     for r in results do
//                                         match r with
//                                         | SuccessfulRequest _ -> Html.none
//                                         | FailedRequest fail -> Html.li [ prop.text fail ]
//                                 ]
//                             ]
//                     ]
//                 ]
//             ]
//             Html.button [
//                 prop.className "btn btn-secondary"
//                 prop.onClick (fun _ -> dispatch CheckUserSignUpForm)
//                 prop.text "Check User Sign Up Form"
//             ]
//         ]
//     ]

// let productColorVariationsView dispatch (product: SyncProduct) (selectedColor: ProductColor option) (variations: ProductColor list) =
//     Html.div [
//         prop.className "configurationOptionContainer"
//         prop.children [
//             Html.text "-- Colors --"
//             Html.div [
//                 prop.children (
//                     variations |> List.map (fun color ->
//                         let isActive = selectedColor = Some color
//                         Html.button [
//                             prop.classes [ "variationSelection"; if isActive then "activeVariation" ]
//                             prop.onClick (fun _ -> UpdateProductColor (product, color) |> dispatch)
//                             prop.children [ Html.text (productColorToString color) ]
//                         ]
//                     )
//                 )
//             ]
//         ]
//     ]

// let productSizeVariationsView dispatch (product: SyncProduct) (selectedSize: ProductSize option) (variations: ProductSize list) =
//     Html.div [
//         prop.className "configurationOptionContainer"
//         prop.children [
//             Html.text "-- Sizes --"
//             Html.div [
//                 prop.children (
//                     variations |> List.map (fun size ->
//                         let isActive = selectedSize = Some size
//                         Html.button [
//                             prop.classes [ "variationSelection"; if isActive then "activeVariation" ]
//                             prop.onClick (fun _ -> UpdateProductSize (product, size) |> dispatch)
//                             prop.children [ Html.text (productSizeToString size) ]
//                         ]
//                     )
//                 )
//             ]
//         ]
//     ]

// let productValidSelectionAddToCart dispatch (validSyncVariantId: string option) (product: SyncProduct) (inBag: bool) =
//     Html.div [
//         prop.className "addToCartContainer"
//         prop.children [
//             Html.text "-- Price --"
//             match validSyncVariantId with
//             | Some id ->
//                 product.productVariations
//                 |> List.tryFind (fun v -> v.externalSyncVariantId = id)
//                 |> function
//                     | Some variant ->
//                         if inBag then
//                             Html.button [
//                                 prop.classes [ "variationSelection"; "existsInCart" ]
//                                 prop.children [ Html.text "Added to Bag" ]
//                             ]
//                         else
//                             Html.button [
//                                 prop.classes [ "variationSelection"; "addToCartButton" ]
//                                 prop.onClick (fun _ -> AddVariantToShoppingBag variant |> dispatch)
//                                 prop.children [ Html.text (string variant.variantPrice) ]
//                             ]
//                     | None -> Html.span []
//             | None -> Html.span []
//         ]
//     ]

// let productDetailConfiguration 
//     dispatch
//     (product: SyncProduct)
//     (selected: ProductColor option * ProductSize option)
//     (price: ReactElement)
//     (colors: ProductColor list)
//     (sizes: ProductSize list) =
//     let selectedColor, selectedSize = selected
//     Html.div [
//         prop.className "productConfigurationDetails"
//         prop.children [
//             Html.div [
//                 prop.children [
//                     Html.p [ prop.children [ Html.text "- Some kind of short description about the item goes here.." ] ]
//                     Html.p [ prop.children [ Html.text "- Some kind of short description about the item goes here.." ] ]
//                     Html.p [ prop.children [ Html.text "- Some kind of short description about the item goes here.." ] ]
//                 ]
//             ]
//             Html.div [
//                 prop.className "navigationControls"
//                 prop.children [
//                     productColorVariationsView dispatch product selectedColor colors
//                     productSizeVariationsView dispatch product selectedSize sizes
//                     price
//                 ]
//             ]
//         ]
//     ]

// 404 Not Found page
let notFoundView =
    Html.div [
        prop.className "contentBackground"
        prop.children [
            Html.div [ prop.text "Who invited you here?! There's nothing to be seen here, so GTFO!" ]
        ]
    ]

// let productView dispatch (syncProductId: int) (model: Model) collection =
//     let syncProductOpt =
//         // match model.allProducts with
//         // | None -> None
//         // | Some products ->
//         //     products
//             collection.products
//             |> List.tryFind (fun p -> p.syncProductId = syncProductId)

//     match syncProductOpt with
//     | None -> notFoundView
//     | Some product ->
//         let variationOptions = productVariationOptions product.productVariations
//         let selectedOptions = model.productVariationOptionsSelected
//         let colors, sizes = variationOptions

//         let selectedInCart =
//             match model.validSyncVariantId with
//             | Some id ->
//                 model.shoppingBag
//                 |> List.tryFind (fun (v, _) -> v.id.ToString() = id)
//                 // |> List.tryFind (fun (v, _) -> v.externalSyncVariantId = id)
//                 |> Option.isSome
//             | None -> false

//         let priceHtml = productValidSelectionAddToCart dispatch model.validSyncVariantId product selectedInCart

//         Html.div [
//             prop.children [
//                 contentHeader product.name (Some (headerLink ("/shop/" + product.collectionTag.toString())))
//                 Html.div [
//                     prop.className "contentBackground"
//                     prop.children [
//                         Html.div [
//                             prop.className "productViewContainer"
//                             prop.children [
//                                 Html.div [
//                                     prop.className "productImageContainer"
//                                     prop.children (
//                                         product.syncProductAltImagePaths
//                                         |> List.map (fun path ->
//                                             Html.div [
//                                                 prop.children [
//                                                     Html.img [
//                                                         prop.className "altImage"
//                                                         prop.src ("../" + path)
//                                                     ]
//                                                 ]
//                                             ]
//                                         )
//                                         |> fun x -> x @ [ Html.img [ prop.className "productImage"; prop.src ("../" + product.syncProductHeroImagePath) ] ]
//                                         |> React.fragment
//                                     )
//                                 ]
//                                 productDetailConfiguration dispatch product selectedOptions priceHtml colors sizes
//                             ]
//                         ]
//                     ]
//                 ]
//             ]
//         ]



// let shoppingBagItem dispatch (variant, qty: int) =
//     Html.div [ 
//         prop.className "navigationControls"
//         prop.children [
//             Html.div [ 
//                 prop.className "shoppingBagLineItemControl" 
//                 prop.children [
//                     Html.text variant.variantName
//                 ]  
//             ]
//             Html.div [ 
//                 prop.className "shoppingBagLineItemControl"
//                 prop.children [ 
//                     Html.img [ 
//                         prop.className "thumbnail"
//                         prop.src variant.variantHeroImagePath
//                     ]
//                 ]
//             ]
//             Html.div [ 
//                 prop.className "shoppingBagLineItemControl" 
//                 prop.children [
//                     Html.text (productColorToString variant.variantColor)
//                 ]  
//             ]
//             Html.div [ 
//                 prop.className "shoppingBagLineItemControl" 
//                 prop.children [
//                     Html.text (productSizeToString variant.variantSize)
//                 ]  
//             ]
//             Html.div [ 
//                 prop.className "shoppingBagLineItemControl" 
//                 prop.children [
//                     Html.text (string variant.variantPrice)
//                 ]  
//             ]
//             Html.div [ 
//                     prop.className "shoppingBagLineItemControl"
//                     prop.children [
//                         Html.button [ 
//                             prop.onClick (fun _ -> AdjustLineItemQuantity (Decrement, variant) |> dispatch)
//                             prop.children [ Html.text "-" ]
//                         ]
//                         Html.text (string qty)
//                         Html.button [ 
//                             prop.onClick (fun _ -> AdjustLineItemQuantity (Increment, variant) |> dispatch)
//                             prop.children [ Html.text "+" ]
//                         ]
//                     ]
//             ]
//             Html.div [ 
//                 prop.className "shoppingBagLineItemControl" 
//                 prop.children [
//                     Html.button [ 
//                         prop.className "removeFromBag"
//                         prop.onClick (fun _ -> DeleteVariantFromShoppingBag variant |> dispatch)
//                         prop.children [ Html.text "remove" ]
//                     ]
//                 ]
//             ]
//         ]
//     ]

// let shoppingBagView (model: Model) dispatch =
//     let orderTotal =
//         model.shoppingBag
//         // |> List.map (fun (v, q) -> v.variantPrice * float q)
//         |> List.map (fun (v, q) -> 20.0 * float q)
//         |> List.sum

//     let checkoutButton =
//         if orderTotal = 0.0 then None else Some (headerLink "/shop/checkout")

//     Html.div [
//         contentHeader "Shopping Bag" (Some (headerLink "/shop"))
//         Html.div [ 
//             prop.className "contentBackground" 
//             prop.children [
//                 // (model.shoppingBag |> List.map (shoppingBagItem dispatch) |> React.fragment)
//                 // (model.shoppingBag |> List.map (shoppingBagItem dispatch) |> React.fragment)
//                 contentHeader (sprintf "Order Bag Total: %.2f" orderTotal) checkoutButton
//             ]
//         ]
//     ]


// ----------------------------
// DOMAIN TYPES
// ----------------------------




// ----------------------------
// HELPERS
// ----------------------------

let roundOrderTotal (total: float) : float option =
    let parts = total.ToString().Split('.') |> List.ofArray
    match parts with
    | [] -> Some 0.0
    | dollars :: rest ->
        let cents =
            rest
            |> List.tryHead
            |> Option.map (fun c -> if c.Length > 2 then c.Substring(0,2) else c)
            |> Option.defaultValue "00"
        let roundedString = dollars + "." + cents
        match System.Double.TryParse roundedString with
        | true, value -> Some value
        | _ -> None

// let calculateOrderBagTotal (shoppingBag: (SyncProductVariant * int) list) : float =
//     shoppingBag
//     |> List.map (fun (variant, qty) -> variant.variantPrice * float qty)
//     |> List.sum
//     |> roundOrderTotal
//     |> Option.defaultValue 0.0

let floatToString (f: float) = f.ToString()

let roundedGrandTotal (bag: float) (tax: float) (ship: float) : string =
    let taxAmount = bag * tax
    let total = bag + taxAmount + ship
    match roundOrderTotal total with
    | Some g -> g.ToString()
    | None -> ""

// let createCustomerDraftOrderDetails (model: Model)
//                                     (subTotal: string)
//                                     (shippingTotal: string)
//                                     (taxRate: string)
//                                     (taxTotal: string)
//                                     (orderTotal: string) : CustomerDraftOrder =
//     let orderItems =
//         model.shoppingBag
//         |> List.map (fun (variant, qty) -> {
//             externalVariantId = variant.id.ToString()
//             // externalVariantId = variant.externalSyncVariantId
//             itemQuantity = qty
//             itemRetailPrice = floatToString 28.0 // variant.variantPrice
//         })

//     {
//         recipient = model.customerAddressForm
//         orderItems = orderItems
//         orderCosts = {
//             orderSubTotal = subTotal
//             orderShipping = shippingTotal
//             orderTaxRate = taxRate
//             orderTax = taxTotal
//             orderTotal = orderTotal
//         }
//     }

// ----------------------------
// VIEWS
// ----------------------------

// let customerAddressFormView (form: CustomerAddressForm) (validation: RequestResponse list) dispatch =
//     Html.div [
//         prop.children [
//             contentHeader "Shipping Address" None
//             Html.div [
//                 prop.className "contentBackground"
//                 prop.children [
//                     viewInput "text" "First Name" form.firstName (fun x -> ShippingFirstNameField x |> UpdateAddressForm) dispatch
//                     viewInput "text" "Last Name" form.lastName (fun x -> ShippingLastNameField x |> UpdateAddressForm) dispatch
//                     viewInput "text" "Street Address" form.streetAddress (fun x -> ShippingStreetAddressField x |> UpdateAddressForm) dispatch
//                     viewInput "text" "City" form.city (fun x -> ShippingCityField x |> UpdateAddressForm) dispatch
//                     viewInput "text" "State" form.stateCode (fun x -> ShippingStateCodeField x |> UpdateAddressForm) dispatch
//                     viewInput "text" "Country" form.countryCode (fun x -> ShippingCountryCodeField x |> UpdateAddressForm) dispatch
//                     viewInput "text" "Zip Code" form.zipCode (fun x -> ShippingZipCodeField x |> UpdateAddressForm) dispatch
//                     // viewValidation validation
//                     Html.button [ prop.onClick (fun _ -> dispatch CheckAddressForm); prop.text "Check User Sign Up Form" ]
//                     Html.button [ prop.onClick (fun _ -> dispatch TestApiTaxRate); prop.text "Test Tax Rate" ]
//                     Html.button [ prop.onClick (fun _ -> dispatch TestApiShippingRate); prop.text "Test Shipping Rate" ]
//                 ]
//             ]
//         ]
//     ]

// let createDraftOrderButton (draft: CustomerDraftOrder) (dispatch: SharedShop.ShopMsg -> unit) =
//     Html.button [
//         prop.onClick (fun _ -> dispatch (TestApiCustomerDraft draft))
//         prop.text "test"
//     ]

// let checkoutView (model: Model) (dispatch: SharedShop.ShopMsg -> unit) =
//     // let bagTotal = calculateOrderBagTotal model.shoppingBag
//     let bagTotal = 100.0
//     let taxOpt, shippingOpt = model.checkoutTaxShipping

//     let submitOrderButton =
//         if bagTotal = 0.0 || shippingOpt.IsNone || not (checkIfAnyValidationErrors model.validationResults) then
//             Html.none
//         else
//             match taxOpt, shippingOpt with
//             | Some tax, Some shipping ->
//                 let grandTotalString = roundedGrandTotal bagTotal tax shipping
//                 if grandTotalString = "" then Html.none else
//                     let draft =
//                         createCustomerDraftOrderDetails
//                             model
//                             (bagTotal.ToString())
//                             (shipping.ToString())
//                             (tax.ToString())
//                             (tax.ToString())
//                             grandTotalString
//                     contentHeader ($"Order Bag Total: {bagTotal} + Tax:{tax} + Ship:{shipping}") (Some (createDraftOrderButton draft dispatch))

//             | None, Some shipping ->
//                 let grandTotalString = roundedGrandTotal bagTotal 0.0 shipping
//                 if grandTotalString = "" then Html.none
//                 else contentHeader ($"Order Bag Total: {grandTotalString}") (Some (headerLink "/payNow"))
//             | _ -> Html.none

//     Html.div [
//         prop.children [
//             contentHeader "Checkout" (Some (headerLink "/shoppingBag"))
//             Html.div [ customerAddressFormView model.customerAddressForm model.validationResults dispatch ]
//             submitOrderButton
//         ]
//     ]


// // Payment page
// let paymentView (model: Model) (dispatch: SharedShop.ShopMsg -> unit) =
//     Html.div [
//         contentHeader "Payment" None
//         if model.payPalOrderReference.IsNone then
//             Html.span []
//         else
//             Html.div [
//                 prop.id "paypal-button-container"
//                 prop.children [
//                     Html.button [
//                         prop.text "Proceed to PayPal"
//                         prop.onClick (fun _ -> dispatch Send)
//                         prop.className "btn btn-primary"
//                     ]
//                 ]
//             ]
//     ]

// Social page
let socialView (dispatch: Client.Domain.SharedShop.ShopMsg -> unit) =
    Html.div [
        contentHeader "SOCIAL SHIT SHOW" None
        Html.div [
            prop.className "homeContent"
            prop.children [
                Html.p [ prop.text "You just HAD to look into that abyss didn't yah..." ]
                Html.p [ prop.text "Well, it's too late now. It's looking right back at you." ]
                Html.p [ prop.text "It seems to be watching..waiting..for your next move.." ]
            ]
        ]
        Html.div [
            prop.className "navigationControls"
            prop.children [
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "Check out what kind of non-sense we are getting ourselves into..." ]
                        Html.a [ prop.href "https://www.instagram.com/xeroeffort"; prop.text "Xero Effort Instagram" ]
                    ]
                ]
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "I hope you have low expectations..." ]
                        Html.a [ prop.href "https://www.instagram.com/xeroeffort"; prop.text "Xero Effort Twitter" ]
                    ]
                ]
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "Feeling a bit lost and alone? Good, so is our discord server. Join up or get left behind." ]
                        Html.a [ prop.href "https://www.instagram.com/xeroeffort"; prop.text "Xero Effort Discord" ]
                    ]
                ]
            ]
        ]
    ]

// Contact page
let contactView =
    Html.div [
        contentHeader "ALL HANDS ON DECK" None
        Html.div [
            prop.className "homeContent"
            prop.children [ Html.p [ prop.text "Dying to get something off your chest?" ] ]
        ]
        Html.div [
            prop.className "navigationControls"
            prop.children [
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "Got questions, comments or concerns?" ]
                        Html.a [ prop.href "mailto:xeroeffortclub@gmail.com"; prop.text "General Inquiries" ]
                    ]
                ]
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "What's that boy? Your order is stuck in a well?..." ]
                        Html.a [ prop.href "mailto:xeroeffortclub@gmail.com"; prop.text "Order Inquiries / Issues" ]
                    ]
                ]
                Html.div [
                    prop.className "homeContent"
                    prop.children [
                        Html.p [ prop.text "Think you got it all figured out?" ]
                        Html.a [ prop.href "mailto:xeroeffortclub@gmail.com"; prop.text "Business Inquiries" ]
                    ]
                ]
            ]
        ]
    ]



// The overall app state

// View dispatcher: select page content based on Model.CurrentPage
// let view (model: Model) (dispatch: Msg -> unit) =
//     Html.div [
//         Components.Navbar.view model dispatch  // your shared nav
//         match model.CurrentPage with
//         | Home ->
//             Html.h1 "Welcome to the Shop" // TODO: expand into a full homeView
//         | Products ->
//             ProductsPage.view model.Products dispatch
//         | Cart ->
//             CartPage.view model.Cart dispatch
//         | Checkout ->
//             CheckoutViews.checkoutView model dispatch
//         | Payment ->
//             CheckoutViews.paymentView model dispatch
//         | Contact ->
//             CheckoutViews.contactView model dispatch
//         | Social ->
//             CheckoutViews.socialView model dispatch
//         | NotFound ->
//             CheckoutViews.notFoundView
//         Components.Footer.view model dispatch
//     ]

// let shopCatalogLink2 (product: CatalogProduct) =
//     let title, category =
//         if String.IsNullOrWhiteSpace product.name
//         then "", ""
//         else
//             product.name.Split " - "
//             |> fun details ->
//                 let t = Array.tryItem 0 details |> Option.defaultValue "TITLE MISSING"
//                 let c = if details.Length > 2 then Array.tryItem 2 details |> Option.defaultValue "CATEGORY MISSING" else ""
//                 t, c

//     Html.div [
//         prop.className "catalogProductTile"
//         prop.children [
//             Html.a [
//                 prop.href ("/shop/" + category + "/" + string product.id)
//                 prop.children [
//                     Html.div [
//                         prop.children [
//                             Html.img [
//                                 prop.className "catalogProductImage"
//                                 prop.src product.thumbnailURL
//                             ]
//                         ]
//                     ]
//                     Html.div [
//                         prop.className "navigationLink"
//                         prop.children [ Html.text title ]
//                     ]
//                 ]
//             ]
//         ]
//     ]

//dispatch
// let catalogView2 collectionString (model: Model) dispatch =
//     match model.allProducts with
//     | Some products ->
//         Html.div [
//             prop.children [
//                 // Optional header, uncomment if needed
//                 // contentHeader ((collectionString) + " collection") (Some (headerLink "/shop"))
//                 Html.h1 [
//                     prop.className "clash-font text-4xl text-center mb-12 text-primary"
//                     prop.text (collectionString + " Collection")
//                 ]
//                 Components.FSharp.Layout.Elements.ProductGrid.ProductGrid products (fun catalogProduct -> ShopMsg.GetProductVariants catalogProduct.id |> dispatch)
//                 // Html.div [
//                 //     prop.className "productCatalogGrid"
//                 //     prop.children (products |> List.map )
//                 //     // prop.children (products |> List.map shopCatalogLink2)
//                 // ]
//             ]
//         ]
//     | None ->
//         Html.div [
//             prop.children [ Html.text "No Products found" ]
//         ]

// let shopCategoryLink (collectionTag: CollectionTag) dispatch =
//     let collectionString = collectionTag.toString()
//     Html.div [
//         prop.className "contentBackground"
//         prop.children [
//             Html.button [
//                 // prop.href ("/shop/" + collectionString)
//                 // prop.href ("/shop/" + collectionString)
//                 prop.onClick ( fun _ -> (NavigateTo (ShopSection.Catalog collectionString)) |> dispatch )
//                 prop.children [
//                     Html.div [
//                         prop.className "navigationLink"
//                         prop.children [ Html.text (collectionString + " collection") ]
//                     ]
//                     // Html.div [
//                     //     prop.children [
//                     //         Html.img [
//                     //             prop.src ("../public/images/" + collectionString + ".png")
//                     //         ]
//                     //     ]
//                 ]
//             ]
//         ]
//     ]

// let categoryHeroCard imageUrl collectionName (description: string) dispatch =
//     Html.div [ 
//         prop.className "card bg-base-200 shadow-xl image-full w-full max-w-md"  
//         prop.children [
//             Html.figure [ Html.img [ prop.src imageUrl; prop.alt collectionName ] ]
//             Html.div [ 
//                 prop.className "card-body" 
//                 prop.children [
//                     Html.h2 [ prop.className "card-title text-white"; prop.text collectionName ]
//                     Html.p [ prop.className "text-white"; prop.text description ]
//                     Html.div [ 
//                         prop.className "card-actions"
//                         prop.children [
//                             Html.button [ 
//                                 prop.className "btn btn-primary"
//                                 prop.text "Shop Now" 
//                                 prop.onClick ( fun _ -> (NavigateTo (ShopSection.Catalog collectionName)) |> dispatch )
//                             ]
//                         ]
//                     ]
//                 ]
//             ]
//         ]
//     ]
        
// let collectionView dispatch =
//     Html.div [
//         prop.children [
//             // contentHeader "BROWSE BY COLLECTION" (Some (headerLink "/shop/landing"))
//             Html.h1 "Collections"
//             Html.div [
//                 prop.children [
//                     categoryHeroCard "" (Limited.toString()) "Get 'em while they're hot" dispatch
//                     categoryHeroCard "" (Unlimited.toString()) "Find your next favorite thing" dispatch
//                 ]
//             ]
//         ]
//     ]

// module SharedShopV2.Views

// open Feliz
open SharedShopV2
open SharedShopV2Domain
open Client.Domain.SharedShopV2Domain
open Client.Domain.SharedShopV2

let shopTypeSelectorView (dispatch: ShopTypeSelectorMsg -> unit) =
    Html.div [
        prop.className "flex flex-col items-center justify-center gap-8 p-8"
        prop.children [
            Html.h2 [
                prop.className "text-2xl font-bold"
                prop.text "Choose how you want to shop"
            ]

            Html.div [
                prop.className "grid grid-cols-1 gap-6 w-full max-w-3xl"
                prop.children [

                    Html.div [
                        prop.className "p-6 border rounded-2xl shadow hover:shadow-lg cursor-pointer transition"
                        prop.onClick (fun _ -> dispatch (ShopTypeSelectorMsg.SwitchSection (ShopSection.BuildYourOwnWizard (BuildYourOwnProductWizard.initialState ()))))
                        prop.children [
                            Html.h3 [ prop.className "text-xl font-semibold"; prop.text "📅 We'll be right back!" ]
                            // Html.hr [ prop.className "my-4" ]
                            Html.p [ prop.className "text-md opacity-70 text-center p-2"; prop.text "Currently disabled, while we upgrade to API v2!" ]
                        ]
                    ]

                    // Html.div [
                    //     prop.className "p-6 border rounded-2xl shadow hover:shadow-lg cursor-pointer transition"
                    //     // prop.onClick (fun _ -> dispatch (ShopTypeSelectorMsg.SwitchSection (ShopSection.BuildYourOwnWizard (BuildYourOwnProductWizard.initialState ()))))
                    //     prop.children [
                    //         Html.h3 [ prop.className "text-xl font-semibold text-left"; prop.text "🛠️ Build Your Own" ]
                    //         Html.p [ prop.className "text-md opacity-70 text-center p-2"; prop.text "Customize products step by step with your own designs." ]
                    //     ]
                    // ]

                    // Html.div [
                    //     prop.className "p-6 border rounded-2xl shadow hover:shadow-lg cursor-pointer transition"
                    //     // prop.onClick (fun _ -> dispatch (ShopTypeSelectorMsg.SwitchSection (ShopSection.ProductTemplateBrowser (ProductTemplate.ProductTemplateBrowser.initialModel()))))
                    //     prop.children [
                    //         Html.h3 [ prop.className "text-xl font-semibold text-left"; prop.text "🛍️ Browse Store Templates" ]
                    //         Html.p [ prop.className "text-md opacity-70 text-center p-2"; prop.text "Pick from pre-made product templates and order quickly." ]
                    //     ]
                    // ]
                ]
            ]
        ]
    ]


open Feliz
open Client.Components.Shop
open Client.Components.Shop.Common
open Client.Components.Shop.Common.Types
open SharedShopV2
open Client.Components.Shop.Collection
open Client.Components.Shop.ShopHero

module LuxuryMockup =
    open Client.Shop.Domain
    open Client.Domain.SharedShopV2

    type Tab =
        | Hero
        | Collection
        | Designer
        | Product
        | Cart
        | Checkout

    [<ReactComponent>]
    let View (dispatch: Client.Domain.SharedShop.ShopMsg -> unit) =
        let (tab, setTab) = React.useState Tab.Hero

        let productDetails : Product.ProductDetails =
            {
                Name        = "Essential Crew Tee"
                Price       = 45m
                Description = "Premium cotton construction with a modern fit. Designed for everyday wear with exceptional comfort and durability. Sustainably produced."
                ReviewCount = 128
                Sizes       = [ "XS"; "S"; "M"; "L"; "XL"; "XXL" ]
                Colors      = [ "bg-neutral"; "bg-base-100 border"; "bg-base-300"; "bg-primary" ]
            }

        // Minimal designer model for now
        let wizardModel =
            BuildYourOwnProductWizard.initialState ()

        Html.div [
            prop.className "min-h-screen bg-base-100 text-base-content"
            prop.children [

                // Top tab bar
                Html.div [
                    prop.className "sticky top-0 z-40 bg-base-100/90 backdrop-blur border-b border-base-300"
                    prop.children [
                        Ui.Section.container [
                            Html.div [
                                prop.className "flex gap-6 sm:gap-8 py-3"
                                prop.children [
                                    let tabBtn t (label: string) =
                                        Html.button [
                                            prop.key label
                                            prop.className (
                                                Ui.tw [
                                                    "text-xs sm:text-sm font-medium uppercase tracking-[0.2em] pb-1 transition-all border-b-2 border-transparent"
                                                    if tab = t then "text-base-content border-base-content"
                                                    else "text-base-content/50 hover:text-base-content"
                                                ]
                                            )
                                            prop.text label
                                            prop.onClick (fun _ -> setTab t)
                                        ]

                                    tabBtn Tab.Hero       "hero"
                                    tabBtn Tab.Collection "collection"
                                    tabBtn Tab.Designer   "designer"
                                    tabBtn Tab.Product    "product"
                                    tabBtn Tab.Cart       "cart"
                                    tabBtn Tab.Checkout   "checkout"
                                ]
                            ]
                        ]
                    ]
                ]

                // Active body
                match tab with
                | Tab.Hero ->
                    Hero.view {
                        OnShopCollection = (fun () -> setTab Tab.Collection)
                        OnExploreMore    = (fun () -> setTab Tab.Collection)
                    }

                | Tab.Collection ->
                    Collection.collectionView
                        Collection.State.initModel
                        (fun msg -> ())


                | Tab.Product ->
                    Product.view {
                        Product     = productDetails
                        OnAddToCart = ignore
                        OnAddToWish = ignore
                    }

                | Tab.Designer ->
                    Designer.view {
                        Model    = wizardModel
                        Dispatch = ignore
                    }
                | Tab.Cart ->
                    Cart.Cart.view
                        { Items   = []; }
                        (fun _ -> ())

                | Tab.Checkout ->
                    Checkout.Checkout.view 
                        {
                            Step           = Checkout.Checkout.CheckoutStep.Shipping
                            ShippingInfo   =
                                {
                                    Email     = ""
                                    FirstName = ""
                                    LastName  = ""
                                    Address   = ""
                                    Apartment = ""
                                    City      = ""
                                    State     = ""
                                    ZipCode   = ""
                                    Country   = ""
                                    Phone     = ""
                                }
                            ShippingMethod  = Checkout.Checkout.ShippingMethod.Standard
                            PaymentMethod  = Checkout.Checkout.PaymentMethod.Card
                            Items          = []
                        } 
                        (fun _ -> ())
            ]
        ]




// View dispatcher: select page content based on Model.CurrentPage
let view (model: Client.Domain.SharedShop.Model) (dispatch: Client.Domain.SharedShop.ShopMsg -> unit) =
    // React.useEffectOnce ( 
    //     fun _ -> 
    //         dispatch GetAllProducts
    //         dispatch GetProductTemplates
    //     // , [| |] 
    // )
    Html.div [
        match model.Section with
        | _ ->
            LuxuryMockup.View (dispatch)
        // | SharedShopV2.ShopLanding ->
        //     homeView 
        //         [   
        //             "https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExN3czeTFpdnYxczhiMWRqaGkxdHU4bHgybm51bWp2Znh2N2ZobnM5aiZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/HtfFneOxp0fx6nUvYn/giphy.gif"
        //             "https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExMXFsaXJvN2JrNzA2dHAxeTVtOWZxampyZDNtejdlczJlc2V3N3c2ayZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/d2W70ynZhKMyWTMQ/giphy.gif"
        //             // "https://media1.giphy.com/media/v1.Y2lkPTc5MGI3NjExZXhkdm5teWs4cHRrYW53bmZ5bnZxenV4eXFkM2s3dzdva2swcDFwbSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/l3q2QXWNglhiBC4KI/giphy.gif"
        //             // "https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExOGducmk3MmpzaHF6aGNlcncwbnY3MTBmaTE4MmlkbzJ2MXJqcWU0NiZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/4EEy1K5SgmYcxFGCII/giphy.gif"
        //             // "https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExa2J5bXN0cmpzeXZhaTI0MzgxaXRqYXkwazl1cXJua3djMXM1NGdmMSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/yb4GOQLoRkTJFnaJam/giphy.gif"
        //             // "https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExODQwZWs2eTRnM2VxMDRqMGh4MXA2cHhrb3hvanQyZjI3NmdnMjM0eSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/vwT1f00PU44QnUgY2l/giphy.gif"
        //             // "https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExaXA2ZnduMGV1dHJnZzBycG9nOXg1bm5tcG95NmFwMjA5ZDg4MGVraCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/l46C82yRUnonqFM9q/giphy.gif"
        //             // "https://media1.giphy.com/media/v1.Y2lkPTc5MGI3NjExNXN0bzUzaTZ2bTRtN2d4NWdkaGp6NmZic3Q3NWY1cGludGpsM3B6aCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/kv5ek9HvmdqpOYiYxh/giphy.gif"
        //             // "https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExMXFsZXNzNGg4NXRqOXgybTlhcGp4a3AzbTFmaTUwM3hzcTNtOGMzOCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/C8OSeOz7PY6FzId1lQ/giphy.gif"
        //             // "https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExODJ3a2FydHoydDQ2bDg4cHl1ZW45c3dudnJocTlremh1cm5jZHdiOSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/3o7ZezIWW1cKlqFpCM/giphy.gif"
        //             // "https://media0.giphy.com/media/v1.Y2lkPTc5MGI3NjExZjVwNXpucjhnNTF5M3Azb3ptNGxqZnZkdGpzMXl4aWhuOXgyd2U0dCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/3oEjHDPbHx75Oo5ec8/giphy.gif"
        //             // "https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExamk1dDR1ZXZsazdpdHB5eHY4cjA1MmxkZGRkamJrMnkzYm90amE5bSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/jorJeXKNM6rxIEV7jr/giphy.gif"
        //             // I LIKE BELOW A LOT
        //             // "https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExd20xdDB3eWNpbnYxemo3cWF0Z3cwbmRmOHRjZGJ2cjYwcWRnbnhkeCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/V3rEdkdScV7mo/giphy.gif"
        //         ] 
        //         dispatch
        // | SharedShopV2.ShopTypeSelector ->
        //     // collectionView dispatch
        //     shopTypeSelectorView (ShopMsg.ShopTypeSelection >> dispatch)
        // | SharedShopV2.BuildYourOwnWizard byow ->
        //     // collectionView dispatch
        //     CreateYourOwnProduct.render byow (ShopMsg.ShopBuildYourOwnWizard >> dispatch)
        // | SharedShopV2.ProductTemplateBrowser ptb ->
        //     // collectionView dispatch
        //     Components.FSharp.Pages.ProductTemplateBrowser.ProductTemplateBrowser ptb (ShopMsg.ShopStoreProductTemplates >> dispatch)
        // | Catalog catalogName ->
        //     // catalogView2 catalogName model dispatch
        //     match model.productTemplates with
        //     | None ->
        //         Html.div "Loading..."
        //     | Some pt ->
        //         Components.FSharp.Pages.ProductTemplateBrowser.ProductTemplateBrowser {
        //             templates = pt.result.items |> Array.toList
        //             total = Array.length pt.result.items
        //             limit = Array.length pt.result.items
        //             offset = 0
        //             loadTemplate = fun x -> printfn "Load product: %i" x
        //             setPage = fun x -> printfn "Load Page: %i" x
        //         }
        //     // CreateYourOwnProduct.render 
        //     //     (match model.buildYourOwn with
        //     //     | None -> BuildYourOwnProduct.initialState (model.allProducts |> Option.defaultValue []) 
        //     //     | Some byo -> byo)
        //     //     (BuildYourOwnProductMsg >> dispatch) 
        // | Product (productName, productId) ->
        //     productView (fun msg -> dispatch msg ) productId model shirtCollection
        // | ShoppingBag ->
        //     // shoppingBagView model dispatch
        //     Html.div "SHOPPING BAG"
        // | Checkout ->
        //     // checkoutView model dispatch
        //     Html.div "CHECKOUT"
        // | Payment ->
        //     Html.div "PAYMENT"
        //     // paymentView model dispatch
        // | Contact ->
        //     contactView
        // | Social ->
        //     socialView dispatch
        // | NotFound ->
        //     notFoundView
        // Components.Footer.view model dispatch
    ]