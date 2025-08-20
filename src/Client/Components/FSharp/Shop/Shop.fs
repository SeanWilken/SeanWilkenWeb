module Components.FSharp.Shop

open System
open System
open Elmish
open Elmish.UrlParser
open Elmish.Navigation
open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Shared.SharedShop

// ------------------- HELPERS -------------------

let checkIfJustValue (value: 'a option) : bool =
    match value with
    | Some a -> true
    | None -> false

let checkValidationResultHead (resultHead: RequestResponse option) =
    match resultHead with
    | Some result -> result
    | None -> FailedRequest "Result was not found."

let checkValidationResult = function
    | SuccessfulRequest _ -> true
    | FailedRequest _ -> false

let checkRequestAgainstConditionFunction conditionFunction request =
    match request with
    | SuccessfulRequest succ -> conditionFunction succ
    | FailedRequest fail -> FailedRequest fail

let checkIfFieldIsBlank fieldTitle fieldValue =
    if String.IsNullOrEmpty fieldValue |> not then
        if fieldTitle <> fieldValue then
            SuccessfulRequest fieldValue
        else
            FailedRequest $"{fieldTitle} cannot be default. \n"
    else
        FailedRequest $"{fieldTitle} cannot be empty. \n"

let checkPasswordMinimumLengthRequirement (password: string) =
    if password.Length >= 8 then
        SuccessfulRequest password
    else
        FailedRequest "Password does not meet the minimal length requirement of 8 characters."

let checkPasswordContainsUpperCharacter password =
    if password |> Seq.exists Char.IsUpper
    then SuccessfulRequest password
    else FailedRequest "Password does contain any upper-case characters."

let checkPasswordContainsLowerCharacter password =
    if password |> Seq.exists Char.IsLower
    then SuccessfulRequest password
    else FailedRequest "Password does contain any lower-case characters."

let checkPasswordContainsNumericCharacter password =
    if password |> Seq.exists Char.IsDigit
    then SuccessfulRequest password
    else FailedRequest "Password does contain any numeric characters."

let checkPasswordIsConfirmed password confirmPassword =
    if password = confirmPassword
    then SuccessfulRequest password
    else FailedRequest "Confirm password does not match original password."

let checkIfPasswordIsVerified password confirmedPassword =
    checkIfFieldIsBlank "Password" password
    |> checkRequestAgainstConditionFunction checkPasswordContainsLowerCharacter
    |> checkRequestAgainstConditionFunction checkPasswordContainsUpperCharacter
    |> checkRequestAgainstConditionFunction checkPasswordContainsNumericCharacter
    |> checkRequestAgainstConditionFunction checkPasswordMinimumLengthRequirement
    |> checkRequestAgainstConditionFunction (checkPasswordIsConfirmed confirmedPassword)

let checkUserFormFields (customerSignUpForm: CustomerSignUpForm) =
    [ checkIfFieldIsBlank "First Name" customerSignUpForm.firstName
      checkIfFieldIsBlank "Last Name" customerSignUpForm.lastName
      checkIfFieldIsBlank "User Name" customerSignUpForm.userName
      checkIfPasswordIsVerified customerSignUpForm.password customerSignUpForm.confirmPassword ]

let createCustomerFromSignUpForm (customerForm: CustomerSignUpForm) : Customer =
    { firstName = customerForm.firstName
      lastName = customerForm.lastName
      userName = customerForm.userName
      password = customerForm.password
      savedShippingAddress = None
      orders = [] }

let checkIfAnyValidationErrors validationResults =
    validationResults
    |> List.filter (checkValidationResult >> not)
    |> List.isEmpty

let checkIfCanCreateUser customerForm validationResults successFlag =
    if successFlag then
        SignUpSuccess (createCustomerFromSignUpForm customerForm)
    else
        SignUpFailed validationResults

let checkCustomerSignUpFormValidationResponses customerForm validationResults =
    validationResults
    |> List.filter (checkValidationResult >> not)
    |> List.isEmpty
    |> checkIfCanCreateUser customerForm validationResults


// ------------------- LOOKUPS -------------------

let stateCodeStringToStateCode (stateCodeValue: string) =
    match stateCodeValue.ToUpper() with
    | "NY" | "NEW YORK" -> NY
    | "CA" | "CALIFORNIA" -> CA
    | "FL" | "FLORIDA" -> FL
    | "TX" | "TEXAS" -> TX
    | _ -> NO_STATE

let countryCodeStringToCountryCode (countryCodeValue: string) =
    match countryCodeValue.ToUpper() with
    | "US" | "UNITED STATES" -> US
    | "MEX" | "MEXICO" -> MEX
    | "CAN" | "CANADA" -> CAN
    | _ -> NO_COUNTRY

let checkIfFieldIsValidFieldCode fieldTitle fieldValue =
    match fieldTitle with
    | "State Code" ->
        match stateCodeStringToStateCode fieldValue with
        | NY | CA | FL | TX -> SuccessfulRequest fieldValue
        | _ -> FailedRequest $"No State Code found with look up: {fieldValue}"
    | "Country Code" ->
        match countryCodeStringToCountryCode fieldValue with
        | US | MEX | CAN -> SuccessfulRequest fieldValue
        | NO_COUNTRY -> FailedRequest $"No Country Code found with look up: {fieldValue}"
    | _ -> FailedRequest $"Unknown field request: {fieldTitle}"

let zipStringCodeToZipIntCode (zipCode: string) =
    match Int32.TryParse zipCode with
    | true, zip -> zip
    | _ -> 0

let checkIfZipIsValid (zipCode: string) =
    if zipCode.Length = 5 then
        match Int32.TryParse zipCode with
        | true, _ -> SuccessfulRequest zipCode
        | false, _ -> FailedRequest "Could not convert Zip Code input to integer."
    else
        FailedRequest "Zip Code not correct length of 5 digits."

// module Shop.AddressAndProducts


// ------------------- ADDRESS VALIDATION -------------------

let checkAddressFields (customerAddressForm: CustomerAddressForm) : RequestResponse list =
    [ checkIfFieldIsBlank "First Name" customerAddressForm.firstName
      checkIfFieldIsBlank "Last Name" customerAddressForm.lastName
      checkIfFieldIsBlank "Street Address" customerAddressForm.streetAddress
      checkIfFieldIsBlank "City" customerAddressForm.city
      checkIfFieldIsValidFieldCode "State Code" customerAddressForm.stateCode
      checkIfFieldIsValidFieldCode "Country Code" customerAddressForm.countryCode
      checkIfZipIsValid customerAddressForm.zipCode ]

let createCustomerWithAddressSignUpForm (customer: Customer) (addressForm: CustomerAddressForm) : Customer =
    { customer with
        savedShippingAddress =
            Some {
                firstName = addressForm.firstName
                lastName = addressForm.lastName
                streetAddress = addressForm.streetAddress
                city = addressForm.city
                stateCode = stateCodeStringToStateCode addressForm.stateCode
                countryCode = countryCodeStringToCountryCode addressForm.countryCode
                zipCode = zipStringCodeToZipIntCode addressForm.zipCode
            } }

let checkIfCanCreateCustomerAddress customer customerAddressForm validationResults successFlag =
    if successFlag then
        SignUpSuccess (createCustomerWithAddressSignUpForm customer customerAddressForm)
    else
        SignUpFailed validationResults

let checkCustomerAddressFormValidationResponses customer customerAddressForm validationResults =
    validationResults
    |> List.filter (checkValidationResult >> not)
    |> List.isEmpty
    |> checkIfCanCreateCustomerAddress customer customerAddressForm validationResults


// ------------------- PRODUCT SYNC -------------------


// ------------------- PRODUCT COLOR/SIZE HELPERS -------------------

let variantColorStringToProductColor color =
    match color with
    | "White" -> White
    | "Black" -> Black
    | "Ash" -> Ash
    | "Asphalt" -> Asphalt
    | "Aqua" -> Aqua
    | "Charcoal Gray" -> CharcoalGray
    | "Gold" -> Gold
    | "Maroon" -> Maroon
    | "Mustard" -> Mustard
    | "Navy" -> Navy
    | "Red" -> Red
    | "Silver" -> Silver
    | "Athletic Heather" -> AthleticHeather
    | "Black Heather" -> BlackHeather
    | "Dark Grey Heather" -> DarkGreyHeather
    | "Deep Heather" -> DeepHeather
    | "Heather Dark Gray" -> HeatherDarkGray
    | "Heather Olive" -> HeatherOlive
    | "Heather Blue" -> HeatherBlue
    | "Heather Navy" -> HeatherNavy
    | "Dark Heather" -> DarkHeather
    | "Heather Raspberry" -> HeatherRaspberry
    | "Heather Dust" -> HeatherDust
    | "Heather Deep Teal" -> HeatherDeepTeal
    | x -> NoColor($"No Color: {x}")

let productColorToString = function
    | White -> "White"
    | Black -> "Black"
    | Ash -> "Ash"
    | Asphalt -> "Asphalt"
    | Aqua -> "Aqua"
    | CharcoalGray -> "Charcoal Gray"
    | Gold -> "Gold"
    | Maroon -> "Maroon"
    | Mustard -> "Mustard"
    | Navy -> "Navy"
    | Red -> "Red"
    | Silver -> "Silver"
    | AthleticHeather -> "Athletic Heather"
    | BlackHeather -> "Black Heather"
    | DarkGreyHeather -> "Dark Grey Heather"
    | DeepHeather -> "Deep Heather"
    | HeatherDarkGray -> "Heather Dark Gray"
    | HeatherOlive -> "Heather Olive"
    | HeatherBlue -> "Heather Blue"
    | HeatherNavy -> "Heather Navy"
    | DarkHeather -> "Dark Heather"
    | HeatherRaspberry -> "Heather Raspberry"
    | HeatherDust -> "Heather Dust"
    | HeatherDeepTeal -> "Heather Deep Teal"
    | NoColor col -> $"No Color: {col}"

let variantSizeStringToProductSize size =
    match size with
    | "XS" | "X-Small" -> XS
    | "S" | "Small" -> S
    | "XS/S" -> XS_S
    | "M" | "Medium" -> M
    | "L" | "Large" -> L
    | "M/L" -> M_L
    | "XL" | "X-Large" -> XL
    | "XXL" | "2XL" | "XX-Large" -> XXL
    | "XXXL" | "3XL" | "XXX-Large" -> XXXL
    | "XXXXL" | "4XL" | "XXXX-Large" -> XXXXL
    | "OSFA" | "One Size Fits All" -> OSFA
    | x -> NoSize($"No Size: {x}")

let productSizeToString = function
    | XS -> "XS"
    | S -> "S"
    | XS_S -> "XS/S"
    | M -> "M"
    | L -> "L"
    | M_L -> "M/L"
    | XL -> "XL"
    | XXL -> "2XL"
    | XXXL -> "3XL"
    | XXXXL -> "4XL"
    | OSFA -> "One Size Fits All"
    | NoSize siz -> $"No Size: {siz}"


// ------------------- SKU HELPERS -------------------

let skuOptionStringListToProductColor listLength (options: string list) =
    options
    |> List.take listLength
    |> List.map (fun s -> " " + s)
    |> String.concat ""
    |> fun s -> s.Trim()
    |> variantColorStringToProductColor

let skuOptionStringListToProductSize options =
    match options |> List.rev |> List.tryHead with
    | Some size -> variantSizeStringToProductSize size
    | None -> NoSize "Size element was empty."

let variantSkuToVariantOptions (variantSku: string) : ProductColor * ProductSize =
    match variantSku.Split '_' |> Array.toList |> List.rev |> List.tryHead with
    | Some opts ->
        let optionValues = opts.Split '-' |> Array.toList
        let optionListLength = optionValues.Length
        ( skuOptionStringListToProductColor (optionListLength - 1) optionValues,
          skuOptionStringListToProductSize optionValues )
    | None -> (NoColor "No color data", NoSize "No size data")


// ------------------- IMAGE HELPERS -------------------

let syncVariantProductImagePathCreator productName (color: string option) =
    match color with
    | Some c -> $"./public/images/products/{productName} - {c}.jpg"
    | None -> $"./public/images/products/{productName}.jpg"


// ------------------- VARIANT VALIDATION -------------------

let checkForVariationColor color variant =
    if variant.variantColor = color then
        SuccessfulRequest "Color was found."
    else
        FailedRequest "Color doesn't exist."

let checkForVariationSize size variant =
    if variant.variantSize = size then
        SuccessfulRequest "Size was found."
    else
        FailedRequest "Size doesn't exist"

let checkVariantsInGroup color size variant =
    (checkForVariationColor color variant |> checkValidationResult)
    && (checkForVariationSize size variant |> checkValidationResult)

let checkGroupByVariationFunction variationFunc variationValue productVariations =
    productVariations
    |> List.map (variationFunc variationValue)
    |> List.filter checkValidationResult
    |> List.isEmpty

let createSyncProductVariant (colorOption: ProductColor option) (sizeOption: ProductSize option) (syncProduct: SyncProduct) =
    match colorOption, sizeOption with
    | None, Some _ -> Failed "Please select a color option"
    | Some _, None -> Failed "Please select a size option"
    | Some color, Some size ->
        match syncProduct.productVariations |> List.tryFind (checkVariantsInGroup color size) with
        | Some syncVariant -> Successful syncVariant
        | None ->
            match ( checkGroupByVariationFunction checkForVariationSize size syncProduct.productVariations,
                    checkGroupByVariationFunction checkForVariationColor color syncProduct.productVariations ) with
            | true, true -> Failed "Neither color, nor size options exist."
            | true, false -> Failed "Selected size does not exist."
            | false, true -> Failed "Selected color does not exist."
            | false, false -> Failed "Something went wrong with variant lookup process."
    | None, None -> Failed "Please select a configuration size and color."


// ------------------- VARIANT CONSTRUCTOR -------------------

let syncProductVariantGenerator variantId variantName variantSize variantColor variantPrice heroImagePath altImagePaths =
    { externalSyncVariantId = variantId
      variantName = variantName
      variantSize = variantSize
      variantColor = variantColor
      variantPrice = variantPrice
      variantHeroImagePath = heroImagePath
      variantAltImagePaths = altImagePaths }

// outforBloodShirt : SyncProduct
// let outforBloodShirt =
//     { 
//         name = outForBloodShirtProductName
//         collectionTag = Unlimited
//         syncProductHeroImagePath = "./public/images/products/Out for Blood/Out for Blood - Unisex Shirt.jpg"
//         syncProductAltImagePaths = []
//         syncProductId = 1
//         productVariations =
//             [ smallGray; mediumGray; largeGray; xLargeGray ]
//     }

let outForBloodShirtProductName = "Out for Blood - Unisex Shirt"
let outForBloodHoodieProductName = "Out for Blood - Unisex Hoodie"
let inescapableStareProductName = "Inescapable Stare - Unisex Hoodie"

// Variant definitions
let smallGray = syncProductVariantGenerator "ufb-shirt-small-gray" "Out for Blood Shirt" S CharcoalGray 25.0 "/images/products/Out for Blood/Out for Blood - Unisex Shirt.jpg" []
let mediumGray = syncProductVariantGenerator "ufb-shirt-medium-gray" "Out for Blood Shirt" M CharcoalGray 25.0 "/images/products/Out for Blood/Out for Blood - Unisex Shirt.jpg" []
let largeGray = syncProductVariantGenerator "ufb-shirt-large-gray" "Out for Blood Shirt" L CharcoalGray 25.0 "/images/products/Out for Blood/Out for Blood - Unisex Shirt.jpg" []
let xLargeGray = syncProductVariantGenerator "ufb-shirt-xl-gray" "Out for Blood Shirt" XL CharcoalGray 25.0 "/images/products/Out for Blood/Out for Blood - Unisex Shirt.jpg" []
let smallWhite = syncProductVariantGenerator "ufb-hoodie-small-white" "Out for Blood Hoodie" S White 45.0 "/images/products/Out for Blood/Out for Blood - Unisex Hoodie.jpg" []
let mediumWhite = syncProductVariantGenerator "ufb-hoodie-medium-white" "Out for Blood Hoodie" M White 45.0 "/images/products/Out for Blood/Out for Blood - Unisex Hoodie.jpg" []
let largeWhite = syncProductVariantGenerator "ufb-hoodie-large-white" "Out for Blood Hoodie" L White 45.0 "/images/products/Out for Blood/Out for Blood - Unisex Hoodie.jpg" []
let xLargeWhite = syncProductVariantGenerator "ufb-hoodie-xl-white" "Out for Blood Hoodie" XL White 45.0 "/images/products/Out for Blood/Out for Blood - Unisex Hoodie.jpg" []
let smallNavy = syncProductVariantGenerator "inescapable-hoodie-small-navy" "Inescapable Stare Hoodie" S Navy 50.0 "/images/products/Inescapable Stare/Inescapable Stare - Unisex Hoodie.jpg" []
let mediumNavy = syncProductVariantGenerator "inescapable-hoodie-medium-navy" "Inescapable Stare Hoodie" M Navy 50.0 "/images/products/Inescapable Stare/Inescapable Stare - Unisex Hoodie.jpg" []
let largeNavy = syncProductVariantGenerator "inescapable-hoodie-large-navy" "Inescapable Stare Hoodie" L Navy 50.0 "/images/products/Inescapable Stare/Inescapable Stare - Unisex Hoodie.jpg" []
let xLargeNavy = syncProductVariantGenerator "inescapable-hoodie-xl-navy" "Inescapable Stare Hoodie" XL Navy 50.0 "/images/products/Inescapable Stare/Inescapable Stare - Unisex Hoodie.jpg" []

let outforBloodShirt : SyncProduct =
    { name = outForBloodShirtProductName
      collectionTag = Unlimited
      syncProductHeroImagePath = "/images/products/Out for Blood/Out for Blood - Unisex Shirt.jpg"
      syncProductAltImagePaths = []
      syncProductId = 1
      productVariations = [ smallGray; mediumGray; largeGray; xLargeGray ] }


// Hoodie products
let outforBloodHoodie : SyncProduct =
    { name = outForBloodHoodieProductName
      collectionTag = Unlimited
      syncProductHeroImagePath = "/images/products/Out for Blood/Out for Blood - Unisex Hoodie.jpg"
      syncProductAltImagePaths = []
      syncProductId = 2
      productVariations = [ smallWhite; mediumWhite; largeWhite; xLargeWhite ] }

let inescapableStareHoodie : SyncProduct =
    { name = inescapableStareProductName
      collectionTag = Unlimited
      syncProductHeroImagePath = "/images/products/Inescapable Stare/Inescapable Stare - Unisex Hoodie.jpg"
      syncProductAltImagePaths = []
      syncProductId = 3
      productVariations = [ smallNavy; mediumNavy; largeNavy; xLargeNavy ] }

// Collection
let shirtCollection : ProductCollection =
    { collectionName = "Shirt Collection"
      collectionTag = Unlimited
      products = [ outforBloodShirt; ] }

let hoodieCollection : ProductCollection =
    { collectionName = "Hoodie Collection"
      collectionTag = Unlimited
      products = [ outforBloodHoodie; inescapableStareHoodie; ] }

let productColorOptionsToString (colors: ProductColor list) : string list =
    List.map productColorToString colors

let productColorStringToProductColor (colors: string list) : ProductColor list =
    List.map variantColorStringToProductColor colors

let productSizeOptionsToString (sizes: ProductSize list) : string list =
    List.map productSizeToString sizes

let productSizeStringToProductSize (sizes: string list) : ProductSize list =
    List.map variantSizeStringToProductSize sizes

let productVariationOptions (variations: SyncProductVariant list) : ProductColor list * ProductSize list =
    let colors =
        variations
        |> List.map (fun v -> v.variantColor)
        |> productColorOptionsToString
        |> Set.ofList
        |> Set.toList
        |> productColorStringToProductColor

    let sizes =
        variations
        |> List.map (fun v -> v.variantSize)
        |> productSizeOptionsToString
        |> Set.ofList
        |> Set.toList
        |> productSizeStringToProductSize

    colors, sizes


// --------------------------------
// Domain shared with Elm code
// --------------------------------


// --------------------------------
// Commands (Stubs)
// Replace these with your real HTTP commands (Thoth.Fetch/Elmish.Cmd.OfAsync/etc.)
// --------------------------------

let getHomeGif : Cmd<Shared.SharedShop.ShopMsg> = Cmd.none

let testApiGetTaxRate (_addr: CustomerAddressForm) : Cmd<Shared.SharedShop.ShopMsg> = Cmd.none

let testApiGetShippingRate (_addr: CustomerAddressForm) (_bag: (SyncProductVariant * int) list) : Cmd<Shared.SharedShop.ShopMsg> = Cmd.none

let testApiCreateDraftOrder (_draft: CustomerDraftOrder) : Cmd<Shared.SharedShop.ShopMsg> = Cmd.none

let sendMessage (_paypalOrderRef: string) : Cmd<Shared.SharedShop.ShopMsg> = Cmd.none

let getAllProducts : Cmd<Shared.SharedShop.ShopMsg> = Cmd.none

let getProductVariants (_variantId: int) : Cmd<Shared.SharedShop.ShopMsg> = Cmd.none

// --------------------------------
// Init
// --------------------------------

let init () : Shared.SharedShop.Model * Cmd<Shared.SharedShop.ShopMsg> =
    {
        section = ShopLanding
        customer = None
        productVariationOptionsSelected = (None, None)
        validSyncVariantId = None
        payPalOrderReference = None
        shoppingBag = []
        checkoutTaxShipping = (None, None)
        homeGif = ""
        customerSignUpForm = defaultCustomerSignUpForm
        customerAddressForm = defaultCustomerAddressForm
        validationResults = []
        allProducts = None
        productVariants = None 
    }, Cmd.batch [ getAllProducts; getHomeGif ]

// --------------------------------
// Helpers
// --------------------------------

let updateCustomerField (model: Shared.SharedShop.Model) (userFormField: UserSignUpFormField) : Shared.SharedShop.Model =
    let f = model.customerSignUpForm
    match userFormField with
    | FirstNameField v -> { model with customerSignUpForm = { f with firstName = v } }
    | LastNameField v ->  { model with customerSignUpForm = { f with lastName = v } }
    | UserNameField v ->  { model with customerSignUpForm = { f with userName = v } }
    | PasswordField v ->  { model with customerSignUpForm = { f with password = v } }
    | ConfirmPasswordField v -> { model with customerSignUpForm = { f with confirmPassword = v } }

let updateAddressField (model: Shared.SharedShop.Model) (addressField: CustomerAddressFormField) : Shared.SharedShop.Model =
    let a = model.customerAddressForm
    match addressField with
    | ShippingFirstNameField v -> { model with customerAddressForm = { a with firstName = v } }
    | ShippingLastNameField v -> { model with customerAddressForm = { a with lastName = v } }
    | ShippingStreetAddressField v -> { model with customerAddressForm = { a with streetAddress = v } }
    | ShippingCityField v -> { model with customerAddressForm = { a with city = v } }
    | ShippingStateCodeField v -> { model with customerAddressForm = { a with stateCode = v } }
    | ShippingCountryCodeField v -> { model with customerAddressForm = { a with countryCode = v } }
    | ShippingZipCodeField v -> { model with customerAddressForm = { a with zipCode = v } }

// Refactor: Option.map2 + tryFind to check variant exists
let checkProductSelectionsAreValidVariant (model: Shared.SharedShop.Model) (syncProduct: SyncProduct) : Shared.SharedShop.Model * Cmd<Shared.SharedShop.ShopMsg> =
    let (mc, ms) = model.productVariationOptionsSelected
    let maybeVariant =
        Option.map2
            (fun c s -> syncProduct.productVariations |> List.tryFind (fun v -> v.variantColor = c && v.variantSize = s))
            mc ms
        |> Option.bind id
    let newModel = { model with validSyncVariantId = maybeVariant |> Option.map (fun v -> v.externalSyncVariantId) }
    newModel, Cmd.none

// Add/merge shopping-bag quantities instead of duplicating line items
let addVariantToShoppingBag (variant: SyncProductVariant) (bag: (SyncProductVariant*int) list) : (SyncProductVariant*int) list =
    let eq v1 v2 = v1.externalSyncVariantId = v2.externalSyncVariantId
    let existing, others = bag |> List.partition (fun (v,_) -> eq v variant)
    match existing with
    | (v, qty) :: _ -> (v, qty + 1) :: others
    | [] -> (variant, 1) :: bag

let adjustLineItemQuantity (adj: QuantityAdjustment) (itemVariant: SyncProductVariant) (bag: (SyncProductVariant*int) list) : (SyncProductVariant*int) list =
    bag
    |> List.map (fun (v, q) ->
        if v = itemVariant then
            match adj with
            | Increment -> v, q + 1
            | Decrement -> v, (if q - 1 <= 0 then 1 else q - 1)
        else v, q)

// --------------------------------
// Update
// --------------------------------

let update (msg: Shared.SharedShop.ShopMsg) (model: Model) : Model * Cmd<Shared.SharedShop.ShopMsg> =
    match msg with

    | UpdateCustomerForm fld ->
        updateCustomerField model fld, Cmd.none

    | UpdateAddressForm fld ->
        updateAddressField model fld, Cmd.none

    | CheckUserSignUpForm ->
        model.customerSignUpForm
        |> checkUserFormFields
        |> checkCustomerSignUpFormValidationResponses model.customerSignUpForm
        |> fun res ->
            match res with
            | SignUpSuccess cust -> { model with customer = Some cust; validationResults = [ SuccessfulRequest "Created User" ] }, Cmd.none
            | SignUpFailed fails -> { model with validationResults = fails }, Cmd.none

    | CheckAddressForm ->
        let addrResults = checkAddressFields model.customerAddressForm
        match model.customer with
        | Some cust ->
            // apply address to the *existing* customer when valid
            addrResults
            |> checkCustomerAddressFormValidationResponses cust model.customerAddressForm
            |> function
               | SignUpSuccess updatedCust -> { model with customer = Some updatedCust; validationResults = [ SuccessfulRequest "Address saved" ] }, Cmd.none
               | SignUpFailed fails -> { model with validationResults = fails }, Cmd.none
        | None ->
            // guest checkout: just record validation results; do not bind to missing customer
            let ok = addrResults |> List.forall checkValidationResult
            if ok then { model with validationResults = [ SuccessfulRequest "Address valid" ] }, Cmd.none
            else { model with validationResults = addrResults |> List.filter (checkValidationResult >> not) }, Cmd.none

    | UpdateProductColor (syncProduct, color) ->
        let mc, ms = model.productVariationOptionsSelected
        let mc' = match mc with Some c when c = color -> None | _ -> Some color
        let m' = { model with productVariationOptionsSelected = (mc', ms) }
        checkProductSelectionsAreValidVariant m' syncProduct

    | UpdateProductSize (syncProduct, size) ->
        let mc, ms = model.productVariationOptionsSelected
        let ms' = match ms with Some s when s = size -> None | _ -> Some size
        let m' = { model with productVariationOptionsSelected = (mc, ms') }
        checkProductSelectionsAreValidVariant m' syncProduct

    | AddVariantToShoppingBag variant ->
        { model with shoppingBag = addVariantToShoppingBag variant model.shoppingBag }, Cmd.none

    | DeleteVariantFromShoppingBag variant ->
        let bag' = model.shoppingBag |> List.filter (fun (p,_) -> p.externalSyncVariantId <> variant.externalSyncVariantId)
        { model with shoppingBag = bag' }, Cmd.none

    | AdjustLineItemQuantity (adj, bagLineItem) ->
        { model with shoppingBag = adjustLineItemQuantity adj bagLineItem model.shoppingBag }, Cmd.none

    | GotResult result ->
        match result with
        | Ok response -> { model with homeGif = response }, Cmd.none
        | Error _ -> model, Cmd.none

    | TestApiTaxRate -> model, testApiGetTaxRate model.customerAddressForm

    | GotTaxRateResult result ->
        match result with
        | Ok response ->
            let taxValue = if response.taxRequired then Some response.taxRate else Some 0.0
            { model with checkoutTaxShipping = (taxValue, snd model.checkoutTaxShipping) }, Cmd.none
        | Error _ ->
            { model with validationResults = model.validationResults @ [ FailedRequest "We couldn’t retrieve tax information. Please try again." ] }, Cmd.none

    | TestApiShippingRate -> model, testApiGetShippingRate model.customerAddressForm model.shoppingBag

    | GotShippingRateResult result ->
        match result with
        | Ok (shipRates) ->
            match shipRates |> List.tryHead with
            | Some ship ->
                match Double.TryParse ship.shippingRate with
                | true, s -> { model with checkoutTaxShipping = (fst model.checkoutTaxShipping, Some s) }, Cmd.none
                | _ -> { model with validationResults = model.validationResults @ [ FailedRequest "Could not read shipping rate." ] }, Cmd.none
            | None -> { model with validationResults = model.validationResults @ [ FailedRequest "No shipping options returned." ] }, Cmd.none
        | Error _ -> { model with validationResults = model.validationResults @ [ FailedRequest "Shipping rate lookup failed." ] }, Cmd.none

    | TestApiCustomerDraft customerDraft ->
        model, Cmd.batch [ testApiCreateDraftOrder customerDraft; Navigation.newUrl "/payment" ]

    | GotCustomerOrderDraftResult result ->
        match result with
        | Ok response -> { model with payPalOrderReference = Some response.code }, Cmd.none
        | Error _ -> { model with validationResults = model.validationResults @ [ FailedRequest "Could not create order draft." ] }, Cmd.none

    | Send ->
        let id = defaultArg model.payPalOrderReference ""
        model, sendMessage id

    | GetAllProducts -> model, getAllProducts

    | GotAllProducts productResult ->
        match productResult with
        | Ok products -> { model with allProducts = Some products }, Cmd.none
        | Error _ -> { model with allProducts = None }, Cmd.none

    | GetProductVariants variantId -> model, getProductVariants variantId

    | GotProductVariants variantResult ->
        match variantResult with
        | Ok variants -> { model with productVariants = Some variants }, Cmd.none
        | Error _ -> { model with productVariants = None }, Cmd.none

open Feliz

let pathToTitleString (path: string) =
    path.Replace ("/", " ")

let headerLink (linkTitle: string) =
    Html.a [
        prop.href linkTitle
        prop.className "navigationLink satorshi-font"
        prop.text (pathToTitleString linkTitle)
    ]

let shoppingBagLink (bagItems: int) (linkTitle: string) =
    Html.a [
        prop.href linkTitle
        prop.className "navigationLink satorshi-font"
        prop.text $"{pathToTitleString linkTitle}: {bagItems}"
    ]

let headerNavigationLanding =
    Html.a [
        prop.href "/index.html"
        prop.className "headerNavigationTitle clash-font"
        prop.text "Xero Effort"
    ]

let header numBagItems dispatch =
    Html.div [
        prop.className "headerNavigation flex items-center justify-between p-4 bg-base-200 shadow-md"
        prop.children [
            Html.div [
                prop.className "navigationControls flex gap-4"
                prop.children [
                    headerNavigationLanding
                    headerLink "/home"
                    headerLink "/shop"
                    headerLink "/social"
                    headerLink "/contact"
                    headerLink "/signup"
                    shoppingBagLink numBagItems "/shoppingBag"
                ]
            ]
        ]
    ]

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

let homeView (homeGif: string) dispatch =
    let gifView =
        Html.img [
            prop.className "homeGif rounded-lg shadow-md"
            prop.src "https://media.giphy.com/media/3o85xxSZvFZgD4wXde/giphy.gif"
        ]

    Html.div [
        prop.className "centeredView flex flex-col items-center gap-6 p-6"
        prop.children [
            Html.div [
                prop.className "contentNavigation clash-font text-xl"
                prop.text "HOLD ONTO YOUR BEERS, INITIATING LAUNCH...."
            ]
            Html.div [
                prop.className "gifContainer grid grid-cols-2 gap-4"
                prop.children [gifView; gifView; gifView; gifView]
            ]
            Html.div [
                prop.className "homeContent satorshi-font text-lg flex flex-col gap-3"
                prop.children [
                    Html.p [ prop.text "Good job making it this far, but this is just the beginning." ]
                    Html.p [ prop.text "Let's just say that light you're seeing...It AIN'T the end of the tunnel...." ]
                    Html.p [ prop.text "It would be dangerous to go alone...better grab a suitcase with some gear, some beer and call up the crew!!" ]
                    Html.button [
                        prop.className "btn btn-primary"
                        prop.onClick (fun _ -> dispatch GetAllProducts)
                        prop.text "TRY GETTING ALL PRODUCTS"
                    ]
                ]
            ]
        ]
    ]

let viewInput (viewType: string) (placeholder: string) (inputValue: string) toMsg dispatch =
    Html.div [
        Html.input [
            prop.className "input input-bordered w-full max-w-xs satorshi-font"
            prop.type' viewType
            prop.placeholder placeholder
            prop.value inputValue
            prop.onChange (fun (v: string) -> dispatch (toMsg v))
        ]
    ]

let signUpView (customerSignUpForm: CustomerSignUpForm) (validationResults: RequestResponse list) dispatch =
    Html.div [
        prop.className "p-6 flex flex-col gap-4"
        prop.children [
            Html.div [
                prop.className "contentNavigation clash-font text-2xl"
                prop.text "JOIN THE DEGENERATE SIDE"
            ]
            Html.div [
                prop.className "flex flex-col gap-2"
                prop.children [
                    viewInput "text" "First Name" customerSignUpForm.firstName (fun x -> UpdateCustomerForm (FirstNameField x)) dispatch
                    viewInput "text" "Last Name" customerSignUpForm.lastName (fun x -> UpdateCustomerForm (LastNameField x)) dispatch
                    viewInput "text" "User Name" customerSignUpForm.userName (fun x -> UpdateCustomerForm (UserNameField x)) dispatch
                    viewInput "password" "Password" customerSignUpForm.password (fun x -> UpdateCustomerForm (PasswordField x)) dispatch
                    viewInput "password" "Confirm Password" customerSignUpForm.confirmPassword (fun x -> UpdateCustomerForm (ConfirmPasswordField x)) dispatch
                    // Validation results mapped here (same as your Elm)
                    Html.div [
                        match validationResults with
                        | [SuccessfulRequest _] -> Html.p [ prop.className "text-green-500"; prop.text "Ok, no errors." ]
                        | results ->
                            Html.ul [
                                prop.className "text-red-500 list-disc list-inside"
                                prop.children [
                                    for r in results do
                                        match r with
                                        | SuccessfulRequest _ -> Html.none
                                        | FailedRequest fail -> Html.li [ prop.text fail ]
                                ]
                            ]
                    ]
                ]
            ]
            Html.button [
                prop.className "btn btn-secondary"
                prop.onClick (fun _ -> dispatch CheckUserSignUpForm)
                prop.text "Check User Sign Up Form"
            ]
        ]
    ]

let productColorVariationsView dispatch (product: SyncProduct) (selectedColor: ProductColor option) (variations: ProductColor list) =
    Html.div [
        prop.className "configurationOptionContainer"
        prop.children [
            Html.text "-- Colors --"
            Html.div [
                prop.children (
                    variations |> List.map (fun color ->
                        let isActive = selectedColor = Some color
                        Html.button [
                            prop.classes [ "variationSelection"; if isActive then "activeVariation" ]
                            prop.onClick (fun _ -> UpdateProductColor (product, color) |> dispatch)
                            prop.children [ Html.text (productColorToString color) ]
                        ]
                    )
                )
            ]
        ]
    ]

let productSizeVariationsView dispatch (product: SyncProduct) (selectedSize: ProductSize option) (variations: ProductSize list) =
    Html.div [
        prop.className "configurationOptionContainer"
        prop.children [
            Html.text "-- Sizes --"
            Html.div [
                prop.children (
                    variations |> List.map (fun size ->
                        let isActive = selectedSize = Some size
                        Html.button [
                            prop.classes [ "variationSelection"; if isActive then "activeVariation" ]
                            prop.onClick (fun _ -> UpdateProductSize (product, size) |> dispatch)
                            prop.children [ Html.text (productSizeToString size) ]
                        ]
                    )
                )
            ]
        ]
    ]

let productValidSelectionAddToCart dispatch (validSyncVariantId: string option) (product: SyncProduct) (inBag: bool) =
    Html.div [
        prop.className "addToCartContainer"
        prop.children [
            Html.text "-- Price --"
            match validSyncVariantId with
            | Some id ->
                product.productVariations
                |> List.tryFind (fun v -> v.externalSyncVariantId = id)
                |> function
                    | Some variant ->
                        if inBag then
                            Html.button [
                                prop.classes [ "variationSelection"; "existsInCart" ]
                                prop.children [ Html.text "Added to Bag" ]
                            ]
                        else
                            Html.button [
                                prop.classes [ "variationSelection"; "addToCartButton" ]
                                prop.onClick (fun _ -> AddVariantToShoppingBag variant |> dispatch)
                                prop.children [ Html.text (string variant.variantPrice) ]
                            ]
                    | None -> Html.span []
            | None -> Html.span []
        ]
    ]

let productDetailConfiguration 
    dispatch
    (product: SyncProduct)
    (selected: ProductColor option * ProductSize option)
    (price: ReactElement)
    (colors: ProductColor list)
    (sizes: ProductSize list) =
    let selectedColor, selectedSize = selected
    Html.div [
        prop.className "productConfigurationDetails"
        prop.children [
            Html.div [
                prop.children [
                    Html.p [ prop.children [ Html.text "- Some kind of short description about the item goes here.." ] ]
                    Html.p [ prop.children [ Html.text "- Some kind of short description about the item goes here.." ] ]
                    Html.p [ prop.children [ Html.text "- Some kind of short description about the item goes here.." ] ]
                ]
            ]
            Html.div [
                prop.className "navigationControls"
                prop.children [
                    productColorVariationsView dispatch product selectedColor colors
                    productSizeVariationsView dispatch product selectedSize sizes
                    price
                ]
            ]
        ]
    ]

// 404 Not Found page
let notFoundView =
    Html.div [
        prop.className "contentBackground"
        prop.children [
            Html.div [ prop.text "Who invited you here?! There's nothing to be seen here, so GTFO!" ]
        ]
    ]

let productView dispatch (syncProductId: int) (model: Model) collection =
    let syncProductOpt =
        // match model.allProducts with
        // | None -> None
        // | Some products ->
        //     products
            collection.products
            |> List.tryFind (fun p -> p.syncProductId = syncProductId)

    match syncProductOpt with
    | None -> notFoundView
    | Some product ->
        let variationOptions = productVariationOptions product.productVariations
        let selectedOptions = model.productVariationOptionsSelected
        let colors, sizes = variationOptions

        let selectedInCart =
            match model.validSyncVariantId with
            | Some id ->
                model.shoppingBag
                |> List.tryFind (fun (v, _) -> v.externalSyncVariantId = id)
                |> Option.isSome
            | None -> false

        let priceHtml = productValidSelectionAddToCart dispatch model.validSyncVariantId product selectedInCart

        Html.div [
            prop.children [
                contentHeader product.name (Some (headerLink ("/shop/" + collectionTagToString product.collectionTag)))
                Html.div [
                    prop.className "contentBackground"
                    prop.children [
                        Html.div [
                            prop.className "productViewContainer"
                            prop.children [
                                Html.div [
                                    prop.className "productImageContainer"
                                    prop.children (
                                        product.syncProductAltImagePaths
                                        |> List.map (fun path ->
                                            Html.div [
                                                prop.children [
                                                    Html.img [
                                                        prop.className "altImage"
                                                        prop.src ("../" + path)
                                                    ]
                                                ]
                                            ]
                                        )
                                        |> fun x -> x @ [ Html.img [ prop.className "productImage"; prop.src ("../" + product.syncProductHeroImagePath) ] ]
                                        |> React.fragment
                                    )
                                ]
                                productDetailConfiguration dispatch product selectedOptions priceHtml colors sizes
                            ]
                        ]
                    ]
                ]
            ]
        ]



let shoppingBagItem dispatch (variant, qty: int) =
    Html.div [ 
        prop.className "navigationControls"
        prop.children [
            Html.div [ 
                prop.className "shoppingBagLineItemControl" 
                prop.children [
                    Html.text variant.variantName
                ]  
            ]
            Html.div [ 
                prop.className "shoppingBagLineItemControl"
                prop.children [ 
                    Html.img [ 
                        prop.className "thumbnail"
                        prop.src variant.variantHeroImagePath
                    ]
                ]
            ]
            Html.div [ 
                prop.className "shoppingBagLineItemControl" 
                prop.children [
                    Html.text (productColorToString variant.variantColor)
                ]  
            ]
            Html.div [ 
                prop.className "shoppingBagLineItemControl" 
                prop.children [
                    Html.text (productSizeToString variant.variantSize)
                ]  
            ]
            Html.div [ 
                prop.className "shoppingBagLineItemControl" 
                prop.children [
                    Html.text (string variant.variantPrice)
                ]  
            ]
            Html.div [ 
                    prop.className "shoppingBagLineItemControl"
                    prop.children [
                        Html.button [ 
                            prop.onClick (fun _ -> AdjustLineItemQuantity (Decrement, variant) |> dispatch)
                            prop.children [ Html.text "-" ]
                        ]
                        Html.text (string qty)
                        Html.button [ 
                            prop.onClick (fun _ -> AdjustLineItemQuantity (Increment, variant) |> dispatch)
                            prop.children [ Html.text "+" ]
                        ]
                    ]
            ]
            Html.div [ 
                prop.className "shoppingBagLineItemControl" 
                prop.children [
                    Html.button [ 
                        prop.className "removeFromBag"
                        prop.onClick (fun _ -> DeleteVariantFromShoppingBag variant |> dispatch)
                        prop.children [ Html.text "remove" ]
                    ]
                ]
            ]
        ]
    ]

let shoppingBagView (model: Model) dispatch =
    let orderTotal =
        model.shoppingBag
        |> List.map (fun (v, q) -> v.variantPrice * float q)
        |> List.sum

    let checkoutButton =
        if orderTotal = 0.0 then None else Some (headerLink "/shop/checkout")

    Html.div [
        contentHeader "Shopping Bag" (Some (headerLink "/shop"))
        Html.div [ 
            prop.className "contentBackground" 
            prop.children [
                (model.shoppingBag |> List.map (shoppingBagItem dispatch) |> React.fragment)
                contentHeader (sprintf "Order Bag Total: %.2f" orderTotal) checkoutButton
            ]
        ]
    ]


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

let calculateOrderBagTotal (shoppingBag: (SyncProductVariant * int) list) : float =
    shoppingBag
    |> List.map (fun (variant, qty) -> variant.variantPrice * float qty)
    |> List.sum
    |> roundOrderTotal
    |> Option.defaultValue 0.0

let floatToString (f: float) = f.ToString()

let roundedGrandTotal (bag: float) (tax: float) (ship: float) : string =
    let taxAmount = bag * tax
    let total = bag + taxAmount + ship
    match roundOrderTotal total with
    | Some g -> g.ToString()
    | None -> ""

let createCustomerDraftOrderDetails (model: Model)
                                    (subTotal: string)
                                    (shippingTotal: string)
                                    (taxRate: string)
                                    (taxTotal: string)
                                    (orderTotal: string) : CustomerDraftOrder =
    let orderItems =
        model.shoppingBag
        |> List.map (fun (variant, qty) -> {
            externalVariantId = variant.externalSyncVariantId
            itemQuantity = qty
            itemRetailPrice = floatToString variant.variantPrice
        })

    {
        recipient = model.customerAddressForm
        orderItems = orderItems
        orderCosts = {
            orderSubTotal = subTotal
            orderShipping = shippingTotal
            orderTaxRate = taxRate
            orderTax = taxTotal
            orderTotal = orderTotal
        }
    }

// ----------------------------
// VIEWS
// ----------------------------

let customerAddressFormView (form: CustomerAddressForm) (validation: RequestResponse list) dispatch =
    Html.div [
        prop.children [
            contentHeader "Shipping Address" None
            Html.div [
                prop.className "contentBackground"
                prop.children [
                    viewInput "text" "First Name" form.firstName (fun x -> ShippingFirstNameField x |> UpdateAddressForm) dispatch
                    viewInput "text" "Last Name" form.lastName (fun x -> ShippingLastNameField x |> UpdateAddressForm) dispatch
                    viewInput "text" "Street Address" form.streetAddress (fun x -> ShippingStreetAddressField x |> UpdateAddressForm) dispatch
                    viewInput "text" "City" form.city (fun x -> ShippingCityField x |> UpdateAddressForm) dispatch
                    viewInput "text" "State" form.stateCode (fun x -> ShippingStateCodeField x |> UpdateAddressForm) dispatch
                    viewInput "text" "Country" form.countryCode (fun x -> ShippingCountryCodeField x |> UpdateAddressForm) dispatch
                    viewInput "text" "Zip Code" form.zipCode (fun x -> ShippingZipCodeField x |> UpdateAddressForm) dispatch
                    // viewValidation validation
                    Html.button [ prop.onClick (fun _ -> dispatch CheckAddressForm); prop.text "Check User Sign Up Form" ]
                    Html.button [ prop.onClick (fun _ -> dispatch TestApiTaxRate); prop.text "Test Tax Rate" ]
                    Html.button [ prop.onClick (fun _ -> dispatch TestApiShippingRate); prop.text "Test Shipping Rate" ]
                ]
            ]
        ]
    ]

let createDraftOrderButton (draft: CustomerDraftOrder) (dispatch: Shared.SharedShop.ShopMsg -> unit) =
    Html.button [
        prop.onClick (fun _ -> dispatch (TestApiCustomerDraft draft))
        prop.text "test"
    ]

let checkoutView (model: Model) (dispatch: Shared.SharedShop.ShopMsg -> unit) =
    let bagTotal = calculateOrderBagTotal model.shoppingBag
    let taxOpt, shippingOpt = model.checkoutTaxShipping

    let submitOrderButton =
        if bagTotal = 0.0 || shippingOpt.IsNone || not (checkIfAnyValidationErrors model.validationResults) then
            Html.none
        else
            match taxOpt, shippingOpt with
            | Some tax, Some shipping ->
                let grandTotalString = roundedGrandTotal bagTotal tax shipping
                if grandTotalString = "" then Html.none else
                    let draft =
                        createCustomerDraftOrderDetails
                            model
                            (bagTotal.ToString())
                            (shipping.ToString())
                            (tax.ToString())
                            (tax.ToString())
                            grandTotalString
                    contentHeader ($"Order Bag Total: {bagTotal} + Tax:{tax} + Ship:{shipping}") (Some (createDraftOrderButton draft dispatch))

            | None, Some shipping ->
                let grandTotalString = roundedGrandTotal bagTotal 0.0 shipping
                if grandTotalString = "" then Html.none
                else contentHeader ($"Order Bag Total: {grandTotalString}") (Some (headerLink "/payNow"))
            | _ -> Html.none

    Html.div [
        prop.children [
            contentHeader "Checkout" (Some (headerLink "/shoppingBag"))
            Html.div [ customerAddressFormView model.customerAddressForm model.validationResults dispatch ]
            submitOrderButton
        ]
    ]


// Payment page
let paymentView (model: Model) (dispatch: Shared.SharedShop.ShopMsg -> unit) =
    Html.div [
        contentHeader "Payment" None
        if model.payPalOrderReference.IsNone then
            Html.span []
        else
            Html.div [
                prop.id "paypal-button-container"
                prop.children [
                    Html.button [
                        prop.text "Proceed to PayPal"
                        prop.onClick (fun _ -> dispatch Send)
                        prop.className "btn btn-primary"
                    ]
                ]
            ]
    ]

// Social page
let socialView (dispatch: Shared.SharedShop.ShopMsg -> unit) =
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

let shopCatalogLink2 (product: CatalogProduct) =
    let title, category =
        product.name.Split " - "
        |> fun details ->
            let t = Array.tryItem 0 details |> Option.defaultValue "TITLE MISSING"
            let c = Array.tryItem 2 details |> Option.defaultValue "CATEGORY MISSING"
            t, c

    Html.div [
        prop.className "catalogProductTile"
        prop.children [
            Html.a [
                prop.href ("/shop/" + category + "/" + string product.id)
                prop.children [
                    Html.div [
                        prop.children [
                            Html.img [
                                prop.className "catalogProductImage"
                                prop.src product.thumbnailURL
                            ]
                        ]
                    ]
                    Html.div [
                        prop.className "navigationLink"
                        prop.children [ Html.text title ]
                    ]
                ]
            ]
        ]
    ]

//dispatch
let catalogView2 collectionString (model: Model) =
    match model.allProducts with
    | Some products ->
        Html.div [
            prop.children [
                // Optional header, uncomment if needed
                contentHeader ((collectionString) + " collection") (Some (headerLink "/shop"))
                Html.div [
                    prop.className "productCatalogGrid"
                    prop.children (products |> List.map shopCatalogLink2)
                ]
            ]
        ]
    | None ->
        Html.div [
            prop.children [ Html.text "No Products found" ]
        ]

let shopCategoryLink collectionTag =
    let collectionString = collectionTagToString collectionTag
    Html.div [
        prop.className "contentBackground"
        prop.children [
            Html.a [
                prop.href ("/shop/" + collectionString)
                prop.children [
                    Html.div [
                        prop.className "navigationLink"
                        prop.children [ Html.text (collectionString + " collection") ]
                    ]
                    Html.div [
                        prop.children [
                            Html.img [
                                prop.src ("../public/images/" + collectionString + ".png")
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

        
let collectionView =
    Html.div [
        prop.children [
            contentHeader "BROWSE BY COLLECTION" (Some (headerLink "/home"))
            Html.div [
                prop.className "navigationControls"
                prop.children [
                    shopCategoryLink Limited
                    shopCategoryLink Unlimited
                ]
            ]
        ]
    ]


// View dispatcher: select page content based on Model.CurrentPage
let view (model: Shared.SharedShop.Model) (dispatch: Shared.SharedShop.ShopMsg -> unit) =
    Html.div [
        match model.section with
        | ShopLanding ->
            homeView "" dispatch
        | Storefront ->
            collectionView
        | Catalog catalogName ->
            catalogView2 catalogName model
        | Product (productName, productId) ->
            productView (fun msg -> dispatch msg ) productId model shirtCollection
        | ShoppingBag ->
            shoppingBagView model dispatch
        | Checkout ->
            checkoutView model dispatch
        | Payment ->
            paymentView model dispatch
        | Contact ->
            contactView
        | Social ->
            socialView dispatch
        | NotFound ->
            notFoundView
        // Components.Footer.view model dispatch
    ]