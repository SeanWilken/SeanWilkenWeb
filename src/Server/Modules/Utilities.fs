module ServerUtilities

open System

module EmbeddedResources =
    open System.IO
    open System.Reflection

    let loadEmbedded (resourceName: string) =
        let asm = Assembly.GetExecutingAssembly()
        use s = asm.GetManifestResourceStream(resourceName)
        if isNull s then failwith $"Missing embedded resource: {resourceName}"
        use r = new StreamReader(s)
        r.ReadToEnd()


    let loadTemplate (path: string) =
        System.IO.File.ReadAllText(path)