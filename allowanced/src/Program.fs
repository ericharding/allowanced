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
open System.Security.Claims
open Microsoft.AspNetCore.Authentication.Cookies

//
// Route Helpers

let templateRoute path (fileProvider: IFileProvider) (dataContext:obj) (ctx:HttpContext) =
  let template = fileProvider.getTemplate path
  Response.ofHtmlString (template.Render dataContext) ctx

let staticRoute path (fileProvider : IFileProvider) (ctx:HttpContext) =
  let content = fileProvider.getFileContent path
  Response.ofHtmlString content ctx

let validateCreds username password =
  match username,password with
  | Some "admin", Some "password" -> true
  | _ -> false

//
// Routes

let styleRoute = staticRoute "styles.css"

let loginRoute (fileProvider : IFileProvider) (ctx: HttpContext) =
  let template = fileProvider.getTemplate "login.html"
  Response.ofHtmlString (template.Render {||}) ctx

let dashboardRoute (fileProvider : IFileProvider) (ctx: HttpContext) =
  let template = fileProvider.getTemplate "home.html"
  Response.ofHtmlString (template.Render {||}) ctx

let createUserClaim username =
  let claims = [|
    new Claim(ClaimTypes.Name, username)
  |]
  let identity = new ClaimsIdentity(claims, "cookie")
  let user = new ClaimsPrincipal(identity)
  user

let indexRoute (fileProvider :IFileProvider) : HttpHandler =
  Request.ifAuthenticated (dashboardRoute fileProvider) (loginRoute fileProvider)

let loginHandler (ctx:HttpContext) =
  task {
    let! formData = ctx.Request.ReadFormAsync()
    let userName = string formData["username"] |> Option.ofObj
    let password = string formData["password"] |> Option.ofObj
    if validateCreds userName password then
      let user = createUserClaim userName.Value
      printfn $"Logged in {userName.Value} / {password.Value}"
      return! Response.signInAndRedirect "cookie" user "/" ctx
    else
      return! Response.redirectTemporarily "/login?error=1" ctx
  }

let logoutHandler (ctx:HttpContext) =
  Response.signOutAndRedirect "cookie" "/" ctx
  // task {
  //   do! Auth.signOut "cookie" ctx
  //   do! Response.redirectTemporarily "/" ctx
  // }

//
// Services

#if DEBUG
let fileProvider = new LocalFileProvider("www")
#else
let fileProvider=new EmbeddedResourceProvider(System.Reflection.Assembly.GetExecutingAssembly(), "allowanced/www")
#endif

let configureServices (services:IServiceCollection) = 
  // Note: unclear how to do this with `add_cookie`
  services
    .AddAuthentication("cookie")
    .AddCookie("cookie")
    |> ignore
  services

let configureCookie (c:CookieAuthenticationOptions) =
  ()

[<EntryPoint>]
let main args =
  webHost [||] {
    add_service configureServices
    // use_authentication
    // add_cookie "cookie" configureCookie
    endpoints [
        get "/" (indexRoute fileProvider)
        get "/login" (loginRoute fileProvider)
        get "/home" (dashboardRoute fileProvider)
        get "/styles.css" (styleRoute fileProvider)

        post "/login" (loginHandler >> unbox)
        get "/logout" logoutHandler
    ]
  }
  // Return 0. This indicates success.
  0 

