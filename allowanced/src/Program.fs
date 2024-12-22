module Allowanced

open Falco
open Falco.Routing
open Falco.HostBuilder
open Falco.Security
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.FileProviders
open System.IO
open Microsoft.Extensions.DependencyInjection
open FileProviders

let inline templateRoute path (dataContext:'a) (ctx:HttpContext) =
  let fileProvider = ctx.RequestServices.GetService<IFileProvider>()
  let template = fileProvider.getTemplate path
  Response.ofHtmlString (template.Render dataContext) ctx

let staticRoute path (ctx:HttpContext) =
  let fileProvider = ctx.RequestServices.GetService<IFileProvider>()
  let content = fileProvider.getFileContent path
  Response.ofHtmlString content ctx

let nameRoute (ctx: HttpContext) =
  let route = Request.getRoute ctx
  let name = route.GetString ("name", "blank")
  templateRoute "name.html" {| name = name |} ctx

let loginRoute (ctx: HttpContext) =
  let fileProvider = ctx.RequestServices.GetService<IFileProvider>()
  let template = fileProvider.getTemplate "login.html"
  Response.ofHtmlString (template.Render {||}) ctx


let dashboardRoute (ctx: HttpContext) =
  let fileProvider = ctx.RequestServices.GetService<IFileProvider>()
  let template = fileProvider.getTemplate "index.html"
  Response.ofHtmlString (template.Render {||}) ctx

let indexRoute ctx
  Request.ifAuthenticated dashboardRoute loginRoute



let configureServices (services:IServiceCollection) =
#if DEBUG
  services.AddSingleton<IFileProvider>(new LocalFileProvider("www"))
#else
  services.AddSingleton<IFileProvider>(new EmbeddedResourceProvider(System.Reflection.Assembly.GetExecutingAssembly(), "allowanced/www"))
#endif

// let secureResourc 


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

