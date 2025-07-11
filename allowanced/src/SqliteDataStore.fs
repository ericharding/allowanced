namespace Data
open Model
open Microsoft.Data.Sqlite
open System

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
        
