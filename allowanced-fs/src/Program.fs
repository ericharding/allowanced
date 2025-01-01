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

//
// Route Helpers

let inline templateRoute path (dataContext:obj) (ctx:HttpContext) =
  let fileProvider = ctx.RequestServices.GetService<IFileProvider>()
  let template = fileProvider.getTemplate path
  Response.ofHtmlString (template.Render dataContext) ctx

let staticRoute path (ctx:HttpContext) =
  let fileProvider = ctx.RequestServices.GetService<IFileProvider>()
  let content = fileProvider.getFileContent path
  Response.ofHtmlString content ctx

let validateCreds username password =
  match username,password with
  | Some "admin", Some "password" -> true
  | _ -> false

//
// Routes

let nameRoute (ctx: HttpContext) =
  let route = Request.getRoute ctx
  let name = route.GetString ("name", "blank")
  templateRoute "name.html" {| name = name |} ctx

let loginRoute (ctx: HttpContext) =
  let fileProvider = ctx.RequestServices.GetService<IFileProvider>()
  let template = fileProvider.getTemplate "login.html"
  Response.ofHtmlString (template.Render {||}) ctx


let dashboardRoute (fileProvider : IFileProvider) (ctx: HttpContext) =
  let template = fileProvider.getTemplate "index.html"
  Response.ofHtmlString (template.Render {||}) ctx

let indexRoute fileProvider: HttpHandler =
  Request.ifAuthenticated (dashboardRoute fileProvider) loginRoute

// let logoutRoute : HttpHandler =
  // Response.removeCookie

// type LoginRequest = {
//   username : string
//   password : string
// }
// let loginRoute : HttpHandler =
//   let a = Request.mapForm (fun (form) -> 
//     let username = form.TryGetString "username"
//     let password = form.TryGetString "password"
//     ()
//     // if validateCreds username password then
//     //   let authCookie = Response.withCookie "auth" username.Value
//     //   authCookie >> Response.redirectTemporarily "/"
//     // else
//     //   Response.withStatusCode 401
//     //   >> Response.ofPlainText "Invalid credentials"
//   )
//   a


//
// Server



// let configureServices (fileProvider : IFileProvider) (services:IServiceCollection) =
// #if DEBUG
//   services.AddSingleton<IFileProvider>(new LocalFileProvider("www"))
// #else
//   services.AddSingleton<IFileProvider>(new EmbeddedResourceProvider(System.Reflection.Assembly.GetExecutingAssembly(), "allowanced/www"))
// #endif

let fileProvider : IFileProvider = 
#if DEBUG
 new LocalFileProvider("www")
#else
  new EmbeddedResourceProvider(System.Reflection.Assembly.GetExecutingAssembly(), "allowanced/www")
#endif

[<EntryPoint>]
let main args =
  webHost [||] {
    add_service (fun s -> s.AddSingleton(fileProvider))
    use_authentication
    endpoints [
        get "/" (indexRoute fileProvider)
        post "/login" (indexRoute fileProvider)
        get "/hello/{name:alpha}" nameRoute 
    ]
  }
  // Return 0. This indicates success.
  0 

