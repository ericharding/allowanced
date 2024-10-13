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

let renderTemplateFromResource resourceName data =
  let content = 2
  Response.ofPlainText resourceName 

let renderTemplate (path:string) (data:obj) ctx =
  let path = Path.Combine (config.wwwRoot,path)
  // TODO: async
  let templateText = File.ReadAllText(path)
  let template = Scriban.Template.Parse(templateText)
  let result = template.Render(data)
  Response.ofHtmlString result ctx 

let getName (ctx: HttpContext) =
    let route = Request.getRoute ctx
    let name = route.GetString "name"
    // let message = sprintf "Hello %s" name
    // Response.ofPlainText message ctx
    renderTemplate "index.html" {| name = name |} ctx

let assembly = config.GetType().Assembly

webHost [||] {
    endpoints [
        get "/" (Response.ofPlainText "Hello World!")
        get "/hello/{name:alpha}" getName 
    ]
}
