module Allowanced

open Falco
open Falco.Routing
open Falco.HostBuilder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.FileProviders
open System.IO
open Microsoft.Extensions.DependencyInjection
open FileProviders

let nameRoute (ctx: HttpContext) =
  let fileProvider = ctx.RequestServices.GetService<IFileProvider>()
  let route = Request.getRoute ctx
  let name = route.GetString ("name", "blank")
  let template = fileProvider.getTemplate "index.html"
  Response.ofHtmlString (template.Render {| name = name |}) ctx

let configureServices (services:IServiceCollection) =
#if DEBUG
  services.AddSingleton<IFileProvider>(new LocalFileProvider("www"))
#else
  services.AddSingleton<IFileProvider>(new EmbeddedResourceProvider(System.Reflection.Assembly.GetExecutingAssembly(), "allowanced/www"))
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

