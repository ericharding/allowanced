module Model
open System


[<Struct>]
type FamilyId = FamilyId of int

[<Struct>]
type UserId = UserId of int

[<Struct>]
type AccountId = AccountId of int

[<Struct>]
type TransactionId = TransactionId of int

[<Struct>]
type TransferId = TransferId of int

type Family = {
  id : FamilyId
  name : string
}

type User = {
  id : UserId
  family : FamilyId
}

type AccountType = 
  | Saving
  | Spending
  | Give
  | External

type Account = {
  id : AccountId
  name : string
  owner : UserId
}

type Transaction = {
  id : TransactionId
  timestamp : DateTime
  value : double
}

type TransferType =
  | Instant
  | DelayedUntil of DateTime
  | NeedsApproval of UserId

// A transfer is between accounts
// The type of the transfer depends on the type of the source and destination
type Transfer = {
  id : TransferId
  transferType : TransferType
  initiated : DateTime
  completed : DateTime
  value : double
}


