module Resource
open Microsoft.Extensions.FileProviders
open System.IO

let private assembly = System.Reflection.Assembly.GetEntryAssembly()
let private embeddedFileProvider = new EmbeddedFileProvider(assembly)

let tryReadEmbeddedFile name =
  printfn "%A" <| assembly.GetManifestResourceNames()
  let data = assembly.GetManifestResourceStream("allowanced.www.index.html")
  printfn "%A" ((new StreamReader(data)).ReadToEnd())
  let fileInfo = embeddedFileProvider.GetFileInfo(name)
  use reader = fileInfo.CreateReadStream()
  use reader = new StreamReader(reader)
  reader.ReadToEnd()

