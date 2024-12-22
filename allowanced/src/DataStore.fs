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

open Microsoft.Data.Sqlite

type SQLiteDataStore(connectionString: string) =
    let createTables() =
        use connection = new SqliteConnection(connectionString)
        connection.Open()
        use command = connection.CreateCommand()
        command.CommandText <- """
            CREATE TABLE IF NOT EXISTS Families (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY,
                FamilyId INTEGER NOT NULL,
                UserName TEXT NOT NULL,
                Password TEXT NOT NULL,
                FullName TEXT NOT NULL,
                UserType TEXT NOT NULL,
                FOREIGN KEY (FamilyId) REFERENCES Families(Id)
            );
            CREATE TABLE IF NOT EXISTS Accounts (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                UserId INTEGER NOT NULL,
                AccountType TEXT NOT NULL, -- json or custom?
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );
            CREATE TABLE IF NOT EXISTS Transactions (
                Id INTEGER PRIMARY KEY,
                UserId INTEGER NOT NULL,
                Timestamp TEXT NOT NULL,
                Value REAL NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );
            CREATE TABLE IF NOT EXISTS Transfers (
                Id INTEGER PRIMARY KEY,
                TransferType TEXT NOT NULL,
                TransferStatus TEXT NOT NULL,
                Initiated INTEGER NOT NULL,
                Completed INTEGER,
                FromAccount INTEGER NOT NULL,
                ToAccount INTEGER NOT NULL,
                Value REAL NOT NULL,
                FOREIGN KEY (FromAccount) REFERENCES Accounts(Id),
                FOREIGN KEY (ToAccount) REFERENCES Accounts(Id)
            );
        """
        command.ExecuteNonQuery() |> ignore

    do createTables()

    interface IDataStore with
        member this.getFamilies(): Family seq =
            use connection = new SqliteConnection(connectionString)
            connection.Open()
            use command = connection.CreateCommand()
            command.CommandText <- "SELECT Id, Name FROM Families"
            use reader = command.ExecuteReader()
            [
                while reader.Read() do
                    yield {
                        id = FamilyId(reader.GetInt32(0))
                        name = reader.GetString(1)
                    }
            ]

        member this.getFamily(familyId: FamilyId): Family =
            use connection = new SqliteConnection(connectionString)
            connection.Open()
            use command = connection.CreateCommand()
            command.CommandText <- "SELECT Id, Name FROM Families WHERE Id = @Id"
            command.Parameters.AddWithValue("@Id", familyId |> function FamilyId id -> id) |> ignore
            use reader = command.ExecuteReader()
            if reader.Read() then
                {
                    id = FamilyId(reader.GetInt32(0))
                    name = reader.GetString(1)
                }
            else
                failwith "Family not found"

        member this.addFamily(family: Family): unit =
            use connection = new SqliteConnection(connectionString)
            connection.Open()
            use command = connection.CreateCommand()
            command.CommandText <- "INSERT INTO Families (Id, Name) VALUES (@Id, @Name)"
            command.Parameters.AddWithValue("@Id", family.id |> function FamilyId id -> id) |> ignore
            command.Parameters.AddWithValue("@Name", family.name) |> ignore
            command.ExecuteNonQuery() |> ignore

        member this.addTransaction(arg1: Transaction): unit = 
            failwith "Not Implemented"
        member this.addTransfer(arg1: Transfer): unit = 
            failwith "Not Implemented"
        member this.addUser(arg1: User): unit = 
            failwith "Not Implemented"
        member this.getActiveTransfers(accountId: AccountId): Transfer seq = 
            failwith "Not Implemented"
        member this.getTransactions(arg1: UserId): Transaction seq = 
            failwith "Not Implemented"
        member this.getTransfers(accountId: AccountId) (startDate: DateTime) (endDate: DateTime): Transfer seq = 
            failwith "Not Implemented"
        member this.getUser(arg1: UserId): User = 
            failwith "Not Implemented"
        member this.getUsers(arg1: FamilyId): User seq = 
            failwith "Not Implemented"
        

