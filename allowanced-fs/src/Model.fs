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

type UserType = 
  | Parent
  | Child

type User = {
  id : UserId
  familyId : FamilyId
  // family   : Lazy<Family>
  userName : string
  password : string
  fullName : string
  userType : UserType
}

type AccountType = 
  | Save of interest:double
  | Spend
  | Give
  | Parent
  | Other of string

type Account = {
  id : AccountId
  name : string
  userId : UserId
  accountType : AccountType
}

type Transaction = {
  id : TransactionId
  userId : UserId
  timestamp : DateTime
  value : double
}

type TransferType =
  | Instant
  | DelayedUntil of DateTime
  | NeedsApproval of UserId

type TransferStatus =
  | Completed
  | Pending
  | NeedsApproval

// A transfer is between accounts
// The type of the transfer depends on the type of the source and destination
type Transfer = {
  id : TransferId
  transferType : TransferType
  transferStatus : TransferStatus
  initiated : DateTime
  completed : DateTime option
  fromAccount : AccountId
  toAccount : AccountId
  value : double
}


