open Microsoft.Extensions.FileProviders
module Resource

// <PackageReference Include="Microsoft.Extensions.FileProviders.Embedded" Version="8.0.5" />
let readEmbeddedFile name =
  let assembly = System.Reflection.Assembly.GetEntryAssembly()
  let ep = new EmbeddedFileProvider(assembly)
  use reader = ep.GetFileInfo(name).CreateReadStream()
  use sr = new StreamReader(reader)
  sr.ReadToEnd()

