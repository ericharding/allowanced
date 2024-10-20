module Allowanced
open Falco
open Falco.Routing
open Falco.HostBuilder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.FileProviders
open System.IO

let config = {|
  wwwRoot = "www"
|}

let renderTemplateFromResource (resourceName:string) (data:obj) ctx =
  printfn "** %s" resourceName
  let templateText = Resource.tryReadEmbeddedFile resourceName
  let template = Scriban.Template.Parse(templateText)
  let result = template.Render(data)
  Response.ofHtmlString result ctx

let renderTemplate (path:string) (data:obj) ctx =
  let path = Path.Combine (config.wwwRoot,path)
  // TODO: use async
  let templateText = File.ReadAllText(path)
  let template = Scriban.Template.Parse(templateText)
  let result = template.Render(data)
  Response.ofHtmlString result ctx 

let nameRoute (ctx: HttpContext) =
    let route = Request.getRoute ctx
    let name = route.GetString "name"
    // renderTemplate "index.html" {| name = name |} ctx
    renderTemplateFromResource "allowanced/www/index.html" {| name = name |} ctx

let assembly = config.GetType().Assembly

[<EntryPoint>]
let main args =
  webHost [||] {
      endpoints [
          get "/" (Response.ofPlainText "Hello World!")
          get "/hello/{name:alpha}" nameRoute 
      ]
  }
  // Return 0. This indicates success.
  0 

