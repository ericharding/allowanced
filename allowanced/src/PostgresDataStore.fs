namespace Data
open Dapper
open Model
open Npgsql
open System

type PostgresDataStore(connectionString: string) =
    let createTables() =
        use connection = new NpgsqlConnection(connectionString)
        connection.Open()
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS Families (
                Id SERIAL PRIMARY KEY,
                Name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Users (
                Id SERIAL PRIMARY KEY,
                FamilyId INTEGER NOT NULL,
                UserName TEXT NOT NULL,
                Password TEXT NOT NULL,
                FullName TEXT NOT NULL,
                UserType TEXT NOT NULL,
                FOREIGN KEY (FamilyId) REFERENCES Families(Id)
            );
            CREATE TABLE IF NOT EXISTS Accounts (
                Id SERIAL PRIMARY KEY,
                Name TEXT NOT NULL,
                UserId INTEGER NOT NULL,
                AccountType TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );
            CREATE TABLE IF NOT EXISTS Transactions (
                Id SERIAL PRIMARY KEY,
                UserId INTEGER NOT NULL,
                Timestamp TIMESTAMP NOT NULL,
                Value DECIMAL NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id)
            );
            CREATE TABLE IF NOT EXISTS Transfers (
                Id SERIAL PRIMARY KEY,
                TransferType TEXT NOT NULL,
                TransferStatus TEXT NOT NULL,
                Initiated TIMESTAMP NOT NULL,
                Completed TIMESTAMP,
                FromAccount INTEGER NOT NULL,
                ToAccount INTEGER NOT NULL,
                Value DECIMAL NOT NULL,
                FOREIGN KEY (FromAccount) REFERENCES Accounts(Id),
                FOREIGN KEY (ToAccount) REFERENCES Accounts(Id)
            );
        """) |> ignore

    do createTables()

    interface IDataStore with
        member this.getFamilies(): Family seq =
            use connection = new NpgsqlConnection(connectionString)
            connection.Query<{| Id: int; Name: string |}>(
                "SELECT Id, Name FROM Families"
            )
            |> Seq.map (fun row -> { id = FamilyId(row.Id); name = row.Name })

        member this.getFamily(familyId: FamilyId): Family =
            use connection = new NpgsqlConnection(connectionString)
            let (FamilyId id) = familyId
            let result = connection.QuerySingle<{| Id: int; Name: string |}>(
                "SELECT Id, Name FROM Families WHERE Id = @Id",
                {| Id = id |}
            )
            { id = FamilyId(result.Id); name = result.Name }

        member this.getUsers(familyId: FamilyId): User seq =
            use connection = new NpgsqlConnection(connectionString)
            let (FamilyId id) = familyId
            connection.Query<{| Id: int; FamilyId: int; UserName: string; Password: string; FullName: string; UserType: string |}>(
                "SELECT Id, FamilyId, UserName, Password, FullName, UserType FROM Users WHERE FamilyId = @FamilyId",
                {| FamilyId = id |}
            )
            |> Seq.map (fun row -> 
                { 
                    id = UserId(row.Id)
                    familyId = FamilyId(row.FamilyId)
                    username = row.UserName
                    password = row.Password
                    fullName = row.FullName
                    userType = match row.UserType with 
                               | "Parent" -> Parent 
                               | "Child" -> Child 
                               | _ -> failwith "Invalid user type"
                })

        member this.getUser(userId: UserId): User =
            use connection = new NpgsqlConnection(connectionString)
            let (UserId id) = userId
            let result = connection.QuerySingle<{| Id: int; FamilyId: int; UserName: string; Password: string; FullName: string; UserType: string |}>(
                "SELECT Id, FamilyId, UserName, Password, FullName, UserType FROM Users WHERE Id = @Id",
                {| Id = id |}
            )
            { 
                id = UserId(result.Id)
                familyId = FamilyId(result.FamilyId)
                username = result.UserName
                password = result.Password
                fullName = result.FullName
                userType = match result.UserType with 
                           | "Parent" -> Parent 
                           | "Child" -> Child 
                           | _ -> failwith "Invalid user type"
            }

        member this.getTransactions(userId: UserId): Transaction seq =
            use connection = new NpgsqlConnection(connectionString)
            let (UserId id) = userId
            connection.Query<{| Id: int; UserId: int; Timestamp: DateTime; Value: decimal |}>(
                "SELECT Id, UserId, Timestamp, Value FROM Transactions WHERE UserId = @UserId",
                {| UserId = id |}
            )
            |> Seq.map (fun row -> 
                { 
                    id = TransactionId(row.Id)
                    userId = UserId(row.UserId)
                    timestamp = row.Timestamp
                    value = row.Value
                })

        member this.getTransfers(accountId: AccountId) (startDate: DateTime) (endDate: DateTime): Transfer seq =
            use connection = new NpgsqlConnection(connectionString)
            let (AccountId id) = accountId
            connection.Query<{| Id: int; TransferType: string; TransferStatus: string; Initiated: DateTime; Completed: DateTime option; FromAccount: int; ToAccount: int; Value: decimal |}>(
                "SELECT Id, TransferType, TransferStatus, Initiated, Completed, FromAccount, ToAccount, Value FROM Transfers WHERE (FromAccount = @AccountId OR ToAccount = @AccountId) AND Initiated >= @StartDate AND Initiated <= @EndDate",
                {| AccountId = id; StartDate = startDate; EndDate = endDate |}
            )
            |> Seq.map (fun row -> 
                { 
                    id = TransferId(row.Id)
                    transferType = match row.TransferType with 
                                   | "Allowance" -> Allowance 
                                   | "Chore" -> Chore 
                                   | "Bonus" -> Bonus
                                   | _ -> failwith "Invalid transfer type"
                    transferStatus = match row.TransferStatus with 
                                     | "Pending" -> Pending 
                                     | "Completed" -> Completed 
                                     | _ -> failwith "Invalid transfer status"
                    initiated = row.Initiated
                    completed = row.Completed
                    fromAccount = AccountId(row.FromAccount)
                    toAccount = AccountId(row.ToAccount)
                    value = row.Value
                })

        member this.getActiveTransfers(accountId: AccountId): Transfer seq =
            use connection = new NpgsqlConnection(connectionString)
            let (AccountId id) = accountId
            connection.Query<{| Id: int; TransferType: string; TransferStatus: string; Initiated: DateTime; Completed: DateTime option; FromAccount: int; ToAccount: int; Value: decimal |}>(
                "SELECT Id, TransferType, TransferStatus, Initiated, Completed, FromAccount, ToAccount, Value FROM Transfers WHERE (FromAccount = @AccountId OR ToAccount = @AccountId) AND TransferStatus != 'Completed'",
                {| AccountId = id |}
            )
            |> Seq.map (fun row -> 
                { 
                    id = TransferId(row.Id)
                    transferType = match row.TransferType with 
                                   | "Allowance" -> Allowance 
                                   | "Chore" -> Chore 
                                   | "Bonus" -> Bonus
                                   | _ -> failwith "Invalid transfer type"
                    transferStatus = match row.TransferStatus with 
                                     | "Pending" -> Pending 
                                     | "Completed" -> Completed 
                                     | _ -> failwith "Invalid transfer status"
                    initiated = row.Initiated
                    completed = row.Completed
                    fromAccount = AccountId(row.FromAccount)
                    toAccount = AccountId(row.ToAccount)
                    value = row.Value
                })

        member this.addFamily(family: Family): unit =
            use connection = new NpgsqlConnection(connectionString)
            let (FamilyId id) = family.id
            connection.Execute(
                "INSERT INTO Families (Id, Name) VALUES (@Id, @Name)",
                {| Id = id; Name = family.name |}
            ) |> ignore

        member this.addUser(user: User): unit =
            use connection = new NpgsqlConnection(connectionString)
            let (UserId id) = user.id
            let (FamilyId familyId) = user.familyId
            let userTypeStr = match user.userType with Parent -> "Parent" | Child -> "Child"
            connection.Execute(
                "INSERT INTO Users (Id, FamilyId, UserName, Password, FullName, UserType) VALUES (@Id, @FamilyId, @UserName, @Password, @FullName, @UserType)",
                {| Id = id; FamilyId = familyId; UserName = user.username; Password = user.password; FullName = user.fullName; UserType = userTypeStr |}
            ) |> ignore

        member this.addTransaction(transaction: Transaction): unit =
            use connection = new NpgsqlConnection(connectionString)
            let (TransactionId id) = transaction.id
            let (UserId userId) = transaction.userId
            connection.Execute(
                "INSERT INTO Transactions (Id, UserId, Timestamp, Value) VALUES (@Id, @UserId, @Timestamp, @Value)",
                {| Id = id; UserId = userId; Timestamp = transaction.timestamp; Value = transaction.value |}
            ) |> ignore

        member this.addTransfer(transfer: Transfer): unit =
            use connection = new NpgsqlConnection(connectionString)
            let (TransferId id) = transfer.id
            let (AccountId fromAccount) = transfer.fromAccount
            let (AccountId toAccount) = transfer.toAccount
            let transferTypeStr = match transfer.transferType with Allowance -> "Allowance" | Chore -> "Chore" | Bonus -> "Bonus"
            let transferStatusStr = match transfer.transferStatus with Pending -> "Pending" | Completed -> "Completed"
            connection.Execute(
                "INSERT INTO Transfers (Id, TransferType, TransferStatus, Initiated, Completed, FromAccount, ToAccount, Value) VALUES (@Id, @TransferType, @TransferStatus, @Initiated, @Completed, @FromAccount, @ToAccount, @Value)",
                {| Id = id; TransferType = transferTypeStr; TransferStatus = transferStatusStr; Initiated = transfer.initiated; Completed = transfer.completed; FromAccount = fromAccount; ToAccount = toAccount; Value = transfer.value |}
            ) |> ignore
