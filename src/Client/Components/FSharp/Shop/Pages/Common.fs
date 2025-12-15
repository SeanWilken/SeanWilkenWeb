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
        
        open Shared.PrintfulCommon
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


        // helper creators
        let withSort filters (sortType: string, sortDir: string) =
            { filters with
                SortType      = Some sortType
                SortDirection = Some sortDir
            }

        let withRegion filters (regionOpt: string option) =
            { filters with SellingRegion = regionOpt }

        let withDestination filters (destOpt: string option) =
            { filters with DestinationCountry = destOpt }

        let withTechnique filters (techOpt: string option) =
            let techniques =
                match techOpt with
                | None -> []
                | Some key -> [ key ]
            { filters with Techniques = techniques }

        let withNewOnly filters (flag: bool) =
            { filters with OnlyNew = flag }

        let withColor filters (colorOpt: string option) =
            let colors =
                match colorOpt with
                | None      -> []
                | Some name -> [ name ]
            { filters with Colors = colors }

        let private selectedSortDir (filters: Filters) =
            filters.SortDirection |> Option.defaultValue "asc"

        [<ReactComponent>]
        let private filterControl (title: string) currentOption (optionPairs: List<string * string>) onChange =
            Html.div [
                prop.className "flex flex-col gap-1"
                prop.children [
                    Html.span [
                        prop.className "text-[10px] uppercase tracking-[0.2em] text-base-content/50"
                        prop.text title
                    ]
                    Html.select [
                        prop.className
                            "select select-sm rounded-none text-xs uppercase tracking-[0.2em] min-w-[9rem]"
                        let current = currentOption |> Option.defaultValue ""
                        prop.value current
                        prop.onChange (fun (v: string) -> onChange v )
                        prop.children [
                            optionPairs
                            |> List.map ( fun (v, lbl) ->
                                Html.option [ prop.value v; prop.text lbl ]
                            )
                            |> React.fragment
                        ]
                    ]
                ]
            ]

        [<ReactComponent>]
        let headerRight (props: Props) =
            Html.div [
                prop.className "flex flex-col md:flex-row md:items-center gap-3 md:gap-4 justify-end"
                prop.children [

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
                                prop.isChecked props.Filters.OnlyNew
                                prop.onChange (fun (ckd: bool) ->
                                    withNewOnly props.Filters ckd |> props.OnFiltersChanged
                                )
                            ]
                        ]
                    ]

                    // --- Region (selling_region_name) ---
                    // filterControl
                    //     "Region"
                    //     f.SellingRegion
                    //     [
                    //         "", "All"
                    //         "US", "US"
                    //         "EU", "EU"
                    //         "GLOBAL", "Global" 
                    //     ]
                    //     (fun v ->
                    //         let value = if v = "" then None else Some v
                    //         withRegion props.Filters value |> props.OnFiltersChanged
                    //     )

                    // --- Color ---
                    filterControl
                        "Color"
                        (props.Filters.Colors |> List.tryHead)
                        [
                            "", "All"
                            "black", "Black"
                            "white", "White"
                            "grey", "Grey" 
                            "red", "Red"  
                            "blue", "Blue" 
                            "green", "Green"
                        ]
                        (fun v ->
                            let value = if v = "" then None else Some v
                            withColor props.Filters value |> props.OnFiltersChanged
                        )

                    // --- Destination Country ---
                    // filterControl
                    //     "Ship To"
                    //     props.Filters.DestinationCountry
                    //     [
                    //         "", "Any"
                    //         "US", "USA"
                    //         "CA", "Canada"
                    //         "GB", "UK" 
                    //     ]
                    //     (fun v ->
                    //         let value = if v = "" then None else Some v
                    //         withDestination props.Filters value |> props.OnFiltersChanged
                    //     )

                    // --- Technique ---
                    filterControl
                        "Technique"
                        (props.Filters.Techniques |> List.tryHead)
                        [
                            "", "Any"
                            "dtg", "DTG"
                            "embroidery", "Embroidery"
                            "sublimation", "Sublimation" 
                        ]
                        (fun v ->
                            let value = if v = "" then None else Some v
                            withTechnique props.Filters value |> props.OnFiltersChanged
                        )


                    // --- Sort ---
                    filterControl
                        "Sort"
                        (Some (
                            (props.Filters.SortType |> Option.defaultValue "featured")
                            + ":"
                            + (props.Filters.SortDirection |> Option.defaultValue "asc")
                        ))
                        [
                            "featured", "Featured"
                            "created_at", "Created"
                            "name", "Name"
                        ]
                        (fun v ->
                            let parts = v.Split(':')
                            let k = if parts.Length > 0 && parts[0] <> "" then parts[0] else "featured"
                            let d = if parts.Length > 1 && parts[1] <> "" then parts[1] else "asc"
                            withSort props.Filters (k, d) |> props.OnFiltersChanged
                        )
                ]
            ]

        [<ReactComponent>]
        let CatalogHeader (props: Props) =
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
                            headerRight props
                        ]
                    ]
                ]
            ]

