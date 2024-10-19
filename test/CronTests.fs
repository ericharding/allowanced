module CronTests

open System
open Xunit
open Allowanced


// Things to test
// 1 - tasks do not run when checked before due
// 2 - tasks run if we miss their deadline by a small amount

[<Fact>]
let ``add`` () =
    Assert.True(true)
