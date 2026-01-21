module MigrationRunner

open System
open MongoDB.Driver
open MongoService.Migration

module AllMigrations =

    type MigrationScript = {
        Version: string
        Name: string
        Up: IMongoDatabase -> unit
    }
    let migrations : MigrationScript list =
        [
            {
                Version = Migration001.version
                Name = Migration001.name
                Up = Migration001.up
            }
            {
                Version = Migration002.version
                Name = Migration002.name
                Up = Migration002.up
            }
        ]

let runAll (db: IMongoDatabase) =
    let applied = MigrationStore.getAppliedVersions db
    let pending = 
        AllMigrations.migrations 
        |> List.filter (fun script-> not (applied.Contains script.Version))
        |> List.sortBy (fun script-> script.Version)

    if List.isEmpty pending then
        printfn "No pending migrations."
    else
        printfn "Running %d pending migrations..." pending.Length


    pending
    |> List.iter (fun script ->
        Console.WriteLine $"Applying migration {script.Version} - {script.Name}"
        script.Up db
        MigrationStore.recordApplied db script.Version script.Name
        Console.WriteLine $"Applied migration {script.Version} - {script.Name}"
    )

let runMigrations () =
    let envConfig = EnvService.EnvConfig.getConfiguredEnvironment ()
    let client = new MongoClient(envConfig.MongoDbConnectionString)
    let db = client.GetDatabase envConfig.MongoDbName
    runAll db
    client.Dispose()
