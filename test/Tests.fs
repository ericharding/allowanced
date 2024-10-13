module Tests

open System
open Xunit
open Allowanced

let util () =
  let _ = Resource.readEmbeddedFile
  ignore ref



[<Fact>]
let ``My test`` () =
    Assert.True(true)


