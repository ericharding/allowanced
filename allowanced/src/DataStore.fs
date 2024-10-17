module Data
open Model
open System.Collections.Immutable

type IDataStore =
  abstract member Families: unit -> Family seq
  abstract member getFamily: FamilyId -> Family
  abstract member Users: FamilyId -> User seq
  abstract member getUser: UserId -> User
  abstract member getTransactions : UserId -> Transaction seq


let families = ImmutableDictionary.Create<FamilyId, Family>()

type InMemoryDataStore() =
  let mutable families = ImmutableList.Create<Family>()
  let mutable users = ImmutableDictionary.Create<FamilyId, ImmutableList<User>>()
  let mutable transactions = ImmutableDictionary.Create<UserId, ImmutableList<Transaction>>()

  interface IDataStore with
    member this.Families() = families :> Family seq

    member this.getFamily(familyId: FamilyId) =
      families |> Seq.find (fun f -> f.id = familyId)

    member this.Users(familyId) =
      match users.TryGetValue(familyId) with
      | true, userList -> userList :> User seq
      | false, _ -> Seq.empty

    member this.getUser(userId) =
      users.Values
      |> Seq.concat
      |> Seq.find (fun u -> u.id = userId)

    member this.getTransactions(userId) =
      match transactions.TryGetValue(userId) with
      | true, transactionList -> transactionList :> Transaction seq
      | false, _ -> Seq.empty

  // Helper methods to modify the data
  member this.AddFamily(family: Family) =
    families <- families.Add(family)

  member this.AddUser(familyId: FamilyId, user: User) =
    users <- users.SetItem(familyId, 
      match users.TryGetValue(familyId) with
      | true, existingUsers -> existingUsers.Add(user)
      | false, _ -> ImmutableList.Create(user))

  member this.AddTransaction(userId: UserId, transaction: Transaction) =
    transactions <- transactions.SetItem(userId,
      match transactions.TryGetValue(userId) with
      | true, existingTransactions -> existingTransactions.Add(transaction)
      | false, _ -> ImmutableList.Create(transaction))

