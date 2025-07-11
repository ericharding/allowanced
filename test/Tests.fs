module Tests

open System
open Xunit
open Allowanced

let dbPassword = System.Environment.GetEnvironmentVariable "DB_PASSWORD"
let dbUser = System.Environment.GetEnvironmentVariable "DB_USER"
let connectionString = $"Host=localhost;Database=allowanced;Username={dbUser};Password={dbPassword};Include Error Detail=true"

let dropAndCreateDatabase() =
  ()


[<Fact>]
let ``My test`` () =
    Assert.True(true)


