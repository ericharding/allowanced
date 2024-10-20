module Allowanced

open Falco
open Falco.Routing
open Falco.HostBuilder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.FileProviders
open System.IO
open Microsoft.Extensions.DependencyInjection
open FileProviders

let renderTemplateFromResource (resourceName:string) (data:obj) ctx =
  printfn "** %s" resourceName
  let templateText = Resource.tryReadEmbeddedFile resourceName
  let template = Scriban.Template.Parse(templateText)
  let result = template.Render(data)
  Response.ofHtmlString result ctx

let renderTemplate (path:string) (data:obj) ctx =
  // TODO: use async
  let templateText = File.ReadAllText(path)
  let template = Scriban.Template.Parse(templateText)
  let result = template.Render(data)
  Response.ofHtmlString result ctx 

let nameRoute (ctx: HttpContext) =
    let route = Request.getRoute ctx
    let name = route.GetString "name"
    renderTemplate "www/index.html" {| name = name |} ctx
    // renderTemplateFromResource "www/index.html" {| name = name |} ctx

let configureServices (services:IServiceCollection) =
#if DEBUG
  services.AddSingleton<IFileProvider>(new LocalFileProvider("www"));
#else
  services.AddSingleton<IFileProvider(new EmbeddedResourceProvider(System.Reflection.Assembly.GetExecutingAssembly(), "allowanced/www"))
#endif
  // services.Add<IFileProvider>(LocalFileProvider("www"))

[<EntryPoint>]
let main args =
  webHost [||] {
    add_service configureServices
    endpoints [
        get "/" (Response.ofPlainText "Hello World!")
        get "/hello/{name:alpha}" nameRoute 
    ]
  }
  // Return 0. This indicates success.
  0 

