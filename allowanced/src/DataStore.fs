module Data
open Model
open System.Collections.Immutable
open System

type IReadOnlyDataStore =
  abstract member getFamilies: unit -> Family seq
  abstract member getFamily: FamilyId -> Family
  abstract member getUsers: FamilyId -> User seq
  abstract member getUser: UserId -> User
  abstract member getTransactions : UserId -> Transaction seq
  abstract member getTransfers : accountId:AccountId -> startDate: DateTime -> endDate:DateTime -> Transfer seq
  abstract member getActiveTransfers : accountId: AccountId -> Transfer seq

type IDataStore =
  inherit IReadOnlyDataStore
  abstract member addFamily: Family -> unit
  abstract member addUser: User -> unit
  abstract member addTransaction: Transaction -> unit
  abstract member addTransfer: Transfer -> unit


type InMemoryDataStore() =
  let mutable families = ImmutableDictionary.Create<FamilyId, Family>()
  let mutable usersByFamily = ImmutableDictionary.Create<FamilyId, ImmutableList<User>>()
  let mutable users = ImmutableDictionary.Create<UserId, User>()
  let mutable transactionsByUser = ImmutableDictionary.Create<UserId, ImmutableList<Transaction>>()
  let mutable transfersByAccount = ImmutableDictionary.Create<AccountId, ImmutableList<Transfer>>()

  member this.transfers() = transfersByAccount

  interface IDataStore with
    member this.getFamilies (): Family seq = 
      families.Values

    member this.getFamily(familyId: FamilyId) =
      families[familyId]

    member this.getUsers (familyId): User seq = 
      usersByFamily[familyId]

    member this.getUser(userId) =
      users[userId]

    member this.getTransactions(userId) =
      transactionsByUser[userId]
    
    member this.getTransfers (accountId: AccountId) (startDate: DateTime) (endDate: DateTime): Transfer seq = 
      transfersByAccount[accountId]
      |> Seq.where (fun t -> t.initiated >= startDate && t.initiated <= endDate)
    
    member this.getActiveTransfers (accountId: AccountId): Transfer seq = 
      transfersByAccount[accountId]
      |> Seq.where (fun t -> t.transferStatus <> Completed)

    member this.addFamily (family:Family): unit = 
      families <- families.Add(family.id, family)

    member this.addUser(user: User) =
      let familyId = user.familyId
      if families.ContainsKey(familyId) then
        users <- users.Add(user.id, user)
        if usersByFamily.ContainsKey familyId then
          usersByFamily <- usersByFamily.SetItem(familyId, usersByFamily[familyId].Add(user))
        else
          usersByFamily <- usersByFamily.Add(familyId, ImmutableList.Create(user))

    member this.addTransaction(transaction: Transaction) =
      let userId = transaction.userId
      if transactionsByUser.ContainsKey userId then
        transactionsByUser <- transactionsByUser.SetItem(userId, transactionsByUser[userId].Add(transaction))
      else
        transactionsByUser <- transactionsByUser.Add(userId, ImmutableList.Create(transaction))
    
    member this.addTransfer(transfer: Transfer) =
      for t in [transfer.fromAccount; transfer.toAccount] do
        if transfersByAccount.ContainsKey t then
          transfersByAccount <- transfersByAccount.SetItem(t, transfersByAccount[t].Add(transfer))
        else
          transfersByAccount <- transfersByAccount.Add(t, ImmutableList.Create<Transfer>())
