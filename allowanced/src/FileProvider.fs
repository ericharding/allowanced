module FileProviders

open Scriban
open System.IO
open System.Collections.Immutable
open Microsoft.Extensions.FileProviders

type IFileProvider =
    abstract member getFileContent: string -> string
    abstract member getTemplate: string -> Scriban.Template

type LocalFileProvider(root: string) =
    interface IFileProvider with
        member this.getFileContent(path: string): string = 
            File.ReadAllText(Path.Combine(root, path))
        
        member this.getTemplate(path: string): Template = 
            let content = (this :> IFileProvider).getFileContent(path)
            Template.Parse(content)

type EmbeddedResourceProvider(assembly: System.Reflection.Assembly, root: string) =
    let embeddedFileProvider = new EmbeddedFileProvider(assembly, root)
    let mutable templateCache = ImmutableDictionary.Create<string, Template>()

    interface IFileProvider with
        member this.getFileContent(path: string): string = 
            let fileInfo = embeddedFileProvider.GetFileInfo(path)
            use stream = fileInfo.CreateReadStream()
            use reader = new StreamReader(stream)
            reader.ReadToEnd()
        
        member this.getTemplate(path: string): Template = 
            match templateCache.TryGetValue(path) with
            | true, template -> template
            | false, _ ->
                let content = (this :> IFileProvider).getFileContent(path)
                let template = Template.Parse(content)
                templateCache <- templateCache.Add(path, template)
                template
