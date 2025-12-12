namespace Client.Components.Shop.Common

open Feliz


module Types =
    type MockProduct = {
        Id       : int
        Name     : string
        Price    : decimal
        Category : string
        Colors   : int
        Tag      : string option
    }

module Ui =
    /// Simple helper to concatenate tailwind classes cleanly
    let inline tw (classes: string list) =
        classes
        |> List.filter (System.String.IsNullOrWhiteSpace >> not)
        |> String.concat " "

    /// DaisyUI button variants
    module Btn =
        let primary extra =
            tw ("btn btn-primary btn-lg tracking-[0.25em]" :: extra)

        let outline extra =
            tw ("btn btn-outline btn-lg tracking-[0.25em]" :: extra)

        let ghost extra =
            tw ("btn btn-ghost" :: extra)

    module Section =
        let container (children: ReactElement list) =
            Html.section [
                prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8"
                prop.children children
            ]

    
    module LoadMoreProductsButton =
        type Props = {
            CanLoadMore : bool
            IsLoading : bool
            OnLoadMore : unit -> unit
        }
        let loadMoreProductsButton props =
            Html.div [
                prop.className "text-center pt-6"
                prop.children [
                    Html.button [
                        prop.className (Btn.outline [ "px-10 py-3 rounded-none" ])
                        prop.disabled (not props.CanLoadMore || props.IsLoading)
                        prop.text (
                            if props.IsLoading then "Loading…"
                            elif not props.CanLoadMore then "No more products"
                            else "Load More Products"
                        )
                        prop.onClick (fun _ ->
                            if props.CanLoadMore && not props.IsLoading
                            then props.OnLoadMore()
                        )
                    ]
                ]
            ]

    module CatalogHeader =
        
        open Feliz
        open Feliz.DaisyUI
        open Shared.SharedShopV2
        open Shared.SharedShopV2.PrintfulCatalog
        open Shared.PrintfulCommon
        open Components.FSharp.Layout.Elements.Pagination

        type SortOption = {
            Key   : string   // maps to sort_type
            Label : string
        }

        type SortDirection =
            | Asc
            | Desc

            member this.ToKey() =
                match this with
                | Asc  -> "asc"
                | Desc -> "desc"

        /// Props for the shared header
        type Props = {
            Title         : string
            Subtitle      : string option
            TotalCount    : int
            Filters       : Filters
            Paging        : PagingInfoDTO
            SearchTerm    : string option

            // callbacks
            OnSearchChanged : string -> unit
            // OnSortChanged   : string * string -> unit
            // OnToggleNewOnly : bool -> unit
            OnPageChange    : int -> unit
            OnFiltersChanged : Filters -> unit
        }

        let private sortOptions : SortOption list = [
            { Key = "featured"; Label = "Featured" }
            { Key = "price";    Label = "Price"    }
            { Key = "newest";   Label = "Newest"   }
        ]

        let private selectedSortKey (filters: Filters) =
            filters.SortType |> Option.defaultValue "featured"

        let private selectedSortDir (filters: Filters) =
            filters.SortDirection |> Option.defaultValue "asc"

        let headerRight (props: Props) =
            let f = props.Filters

            // helper creators
            let withSort (sortType: string, sortDir: string) =
                { f with
                    SortType      = Some sortType
                    SortDirection = Some sortDir
                }

            let withRegion (regionOpt: string option) =
                { f with SellingRegion = regionOpt }

            let withDestination (destOpt: string option) =
                { f with DestinationCountry = destOpt }

            let withTechnique (techOpt: string option) =
                let techniques =
                    match techOpt with
                    | None -> []
                    | Some key -> [ key ]
                { f with Techniques = techniques }

            let withNewOnly (flag: bool) =
                { f with OnlyNew = flag }

            let withColor (colorOpt: string option) =
                let colors =
                    match colorOpt with
                    | None      -> []
                    | Some name -> [ name ]
                { f with Colors = colors }

            Html.div [
                prop.className "flex flex-col md:flex-row md:items-center gap-3 md:gap-4 justify-end"
                prop.children [

                    // --- Sort ---
                    // let currentSortKey = f.SortType |> Option.defaultValue "featured"
                    // let currentSortDir = f.SortDirection |> Option.defaultValue "asc"

                    // let sortOptions =
                    //     [|
                    //         "featured", "Featured"
                    //         "created_at", "Created"
                    //         "name", "Name"
                    //     |]

                    // Html.div [
                    //     prop.className "flex flex-col gap-1"
                    //     prop.children [
                    //         Html.span [
                    //             prop.className "text-[10px] uppercase tracking-[0.2em] text-base-content/50"
                    //             prop.text "Sort"
                    //         ]
                    //         Html.select [
                    //             prop.className
                    //                 "select select-sm select-bordered text-xs uppercase tracking-[0.2em] min-w-[9rem]"
                    //             prop.value $"{currentSortKey}:{currentSortDir}"
                    //             prop.onChange (fun (value: string) ->
                    //                 let parts = value.Split(':')
                    //                 let k = if parts.Length > 0 && parts[0] <> "" then parts[0] else "featured"
                    //                 let d = if parts.Length > 1 && parts[1] <> "" then parts[1] else "asc"
                    //                 withSort (k, d) |> props.OnFiltersChanged
                    //             )
                    //             prop.children [
                    //                 for (key,label) in sortOptions do
                    //                     for dirKey, dirLabel in [ "asc","↑"; "desc","↓" ] do
                    //                         let fullKey = $"{key}:{dirKey}"
                    //                         Html.option [
                    //                             prop.value fullKey
                    //                             prop.selected ( if fullKey = $"{currentSortKey}:{currentSortDir}" then true else false )
                    //                             prop.text $"{label} {dirLabel}"
                    //                         ]
                    //             ]
                    //         ]
                    //     ]
                    // ]

                    // --- Region (selling_region_name) ---
                    // Html.div [
                    //     prop.className "flex flex-col gap-1"
                    //     prop.children [
                    //         Html.span [
                    //             prop.className "text-[10px] uppercase tracking-[0.2em] text-base-content/50"
                    //             prop.text "Region"
                    //         ]
                    //         Html.select [
                    //             prop.className
                    //                 "select select-sm select-bordered text-xs uppercase tracking-[0.2em] min-w-[9rem]"
                    //             let current = f.SellingRegion |> Option.defaultValue ""
                    //             prop.value current
                    //             prop.onChange (fun v ->
                    //                 let value = if v = "" then None else Some v
                    //                 withRegion value |> props.OnFiltersChanged
                    //             )
                    //             prop.children [
                    //                 Html.option [ prop.value "";    prop.text "All"    ]
                    //                 Html.option [ prop.value "US";  prop.text "US"     ]
                    //                 Html.option [ prop.value "EU";  prop.text "EU"     ]
                    //                 Html.option [ prop.value "GLOBAL"; prop.text "Global" ]
                    //             ]
                    //         ]
                    //     ]
                    // ]
                    Html.div [
                        prop.className "flex flex-col gap-1"
                        prop.children [
                            Html.span [
                                prop.className "text-[10px] uppercase tracking-[0.2em] text-base-content/50"
                                prop.text "Color"
                            ]
                            Html.select [
                                prop.className
                                    "select select-sm select-bordered text-xs uppercase tracking-[0.2em] min-w-[9rem]"

                                // use first color in Filters.Colors as the “current”
                                let currentColor =
                                    f.Colors
                                    |> List.tryHead
                                    |> Option.defaultValue ""
                                prop.value currentColor

                                prop.onChange (fun v ->
                                    let value = if v = "" then None else Some v
                                    withColor value |> props.OnFiltersChanged
                                )

                                // NOTE: keys here should match whatever you pass to Printful as `colors`
                                prop.children [
                                    Html.option [ prop.value "";        prop.text "All"      ]
                                    Html.option [ prop.value "black";   prop.text "Black"    ]
                                    Html.option [ prop.value "white";   prop.text "White"    ]
                                    Html.option [ prop.value "grey";    prop.text "Grey"     ]
                                    Html.option [ prop.value "red";     prop.text "Red"      ]
                                    Html.option [ prop.value "blue";    prop.text "Blue"     ]
                                    Html.option [ prop.value "green";   prop.text "Green"    ]
                                ]
                            ]
                        ]
                    ]

                    // --- Destination Country ---
                    Html.div [
                        prop.className "flex flex-col gap-1"
                        prop.children [
                            Html.span [
                                prop.className "text-[10px] uppercase tracking-[0.2em] text-base-content/50"
                                prop.text "Ship To"
                            ]
                            Html.select [
                                prop.className
                                    "select select-sm select-bordered text-xs uppercase tracking-[0.2em] min-w-[9rem]"
                                let current = f.DestinationCountry |> Option.defaultValue ""
                                prop.value current
                                prop.onChange (fun v ->
                                    let value = if v = "" then None else Some v
                                    withDestination value |> props.OnFiltersChanged
                                )
                                prop.children [
                                    Html.option [ prop.value "";    prop.text "Any"    ]
                                    Html.option [ prop.value "US";  prop.text "US"     ]
                                    Html.option [ prop.value "CA";  prop.text "Canada" ]
                                    Html.option [ prop.value "GB";  prop.text "UK"     ]
                                ]
                            ]
                        ]
                    ]

                    // --- Technique ---
                    Html.div [
                        prop.className "flex flex-col gap-1"
                        prop.children [
                            Html.span [
                                prop.className "text-[10px] uppercase tracking-[0.2em] text-base-content/50"
                                prop.text "Technique"
                            ]
                            Html.select [
                                prop.className
                                    "select select-sm select-bordered text-xs uppercase tracking-[0.2em] min-w-[9rem]"
                                let current =
                                    f.Techniques
                                    |> List.tryHead
                                    |> Option.defaultValue ""
                                prop.value current
                                prop.onChange (fun v ->
                                    let value = if v = "" then None else Some v
                                    withTechnique value |> props.OnFiltersChanged
                                )
                                prop.children [
                                    Html.option [ prop.value "";           prop.text "Any"       ]
                                    Html.option [ prop.value "dtg";        prop.text "DTG"       ]
                                    Html.option [ prop.value "embroidery"; prop.text "Embroidery"]
                                    Html.option [ prop.value "sublimation"; prop.text "Sublimation" ]
                                ]
                            ]
                        ]
                    ]

                    // --- "new only" toggle ---
                    Html.label [
                        prop.className "label cursor-pointer gap-2"
                        prop.children [
                            Html.span [
                                prop.className "label-text text-[11px] uppercase tracking-[0.2em] text-base-content/60"
                                prop.text "New Only"
                            ]
                            Html.input [
                                prop.type'.checkbox
                                prop.className "checkbox checkbox-xs"
                                prop.isChecked f.OnlyNew
                                prop.onChange (fun (checked: bool) ->
                                    withNewOnly checked |> props.OnFiltersChanged
                                )
                            ]
                        ]
                    ]
                ]
            ]

        [<ReactComponent>]
        let CatalogHeader (props: Props) =
            let currentSortKey  = selectedSortKey props.Filters
            let currentSortDir  = selectedSortDir props.Filters
            let searchValue     = props.SearchTerm |> Option.defaultValue ""

            Html.div [
                prop.className "flex flex-col gap-4 pb-4 md:pb-6 p-6"

                prop.children [
                    // Top row: title + counts
                    Html.div [
                        prop.className "flex flex-col md:flex-row md:items-end md:justify-between gap-3"
                        prop.children [
                            Html.div [
                                prop.className "space-y-1"
                                prop.children [
                                    Html.h1 [
                                        prop.className "text-2xl md:text-4xl font-light tracking-tight text-base-content"
                                        prop.text props.Title
                                    ]
                                    match props.Subtitle with
                                    | Some sub ->
                                        Html.p [
                                            prop.className "text-sm text-base-content/60"
                                            prop.text sub
                                        ]
                                    | None -> Html.none
                                    Html.p [
                                        prop.className "text-xs text-base-content/50"
                                        prop.text $"{props.TotalCount} item(s)"
                                    ]
                                ]
                            ]

                            // Right side: search + sort + new-only
                            // Html.div [
                            //     prop.className "flex flex-col md:flex-row md:items-center gap-3"
                            //     prop.children [

                            //         // search
                            //         // Html.div [
                            //         //     prop.className "form-control"
                            //         //     prop.children [
                            //         //         Html.input [
                            //         //             prop.className
                            //         //                 "input input-sm input-bordered w-full  text-sm"
                            //         //             prop.placeholder "Search products…"
                            //         //             prop.value searchValue
                            //         //             prop.onChange (fun s -> props.OnSearchChanged s)
                            //         //         ]
                            //         //     ]
                            //         // ]

                            //         // sort select
                            //         Html.select [
                            //             prop.className
                            //                 "select select-sm select-bordered text-xs uppercase tracking-[0.2em]"
                            //             prop.value $"{currentSortKey}:{currentSortDir}"
                            //             prop.onChange (fun (value: string) ->
                            //                 // value is "key:dir"
                            //                 let parts = value.Split(':')
                            //                 let k =
                            //                     if parts.Length > 0 && parts[0] <> "" then parts[0]
                            //                     else "featured"
                            //                 let d =
                            //                     if parts.Length > 1 && parts[1] <> "" then parts[1]
                            //                     else "asc"
                            //                 props.OnSortChanged (k, d)
                            //             )
                            //             prop.children [
                            //                 for opt in sortOptions do
                            //                     for dir in [ Asc; Desc ] do
                            //                         let dirKey = dir.ToKey()
                            //                         let fullKey = $"{opt.Key}:{dirKey}"
                            //                         let labelSuffix = if dir = Asc then "↑" else "↓"
                            //                         Html.option [
                            //                             prop.value fullKey
                            //                             prop.selected ( if fullKey = $"{currentSortKey}:{currentSortDir}" then true else false )
                            //                             prop.text $"{opt.Label} {labelSuffix}"
                            //                         ]
                            //             ]
                            //         ]

                            //         // "new only" toggle
                            //         Html.label [
                            //             prop.className "label cursor-pointer gap-2"
                            //             prop.children [
                            //                 Html.span [
                            //                     prop.className "label-text text-[11px] uppercase tracking-[0.2em] text-base-content/60"
                            //                     prop.text "New Only"
                            //                 ]
                            //                 Html.input [
                            //                     prop.type'.checkbox
                            //                     prop.className "checkbox checkbox-xs"
                            //                     prop.isChecked props.Filters.OnlyNew
                            //                     prop.onChange (fun (checked: bool) -> props.OnToggleNewOnly checked)
                            //                 ]
                            //             ]
                            //         ]
                            //     ]
                            // ]
                            headerRight props
                        ]
                    ]

                    // Bottom row: pagination
                    // Html.div [
                    //     prop.className "flex items-center justify-end"
                    //     prop.children [
                    //         Pagination {
                    //             offset      = props.Paging.offset
                    //             limit       = props.Paging.limit
                    //             total       = props.Paging.total
                    //             onPageChange = props.OnPageChange
                    //         }
                    //     ]
                    // ]
                ]
            ]

