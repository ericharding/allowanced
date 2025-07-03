open System
open Npgsql
open Dapper

let dbPassword = System.Environment.GetEnvironmentVariable("DB_PASSWORD")
let dbUser = System.Environment.GetEnvironmentVariable("DB_USER")

let connectionString = $"Host=localhost;Database=allowanced;Username={dbUser};Password={dbPassword};Include Error Detail=true"

[<CLIMutable>]
type User = {
    Id: int
    Name: string
    Email: string
    CreatedAt: DateTime
}

let createTableIfNotExists (conn: NpgsqlConnection) =
    let sql = """
        CREATE TABLE IF NOT EXISTS users (
            id SERIAL PRIMARY KEY,
            name VARCHAR(100) NOT NULL,
            email VARCHAR(100) UNIQUE NOT NULL,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        )
    """
    conn.Execute(sql) |> ignore

let insertUser (conn: NpgsqlConnection) name email =
    let sql = "INSERT INTO users (name, email) VALUES (@Name, @Email)"
    conn.Execute(sql, {| Name = name; Email = email |})

let queryUsers (conn: NpgsqlConnection) =
    let sql = "SELECT id, name, email, created_at FROM users"
    conn.Query<User>(sql)

[<EntryPoint>]
let main argv =
    try
        use conn = new NpgsqlConnection(connectionString)
        conn.Open()
        
        // Create table if it doesn't exist
        createTableIfNotExists conn
        printfn "Table created or already exists"
        
        // Insert some test data
        let rowsInserted = insertUser conn "John Doe" "john@example.com"
        printfn "Inserted %d row(s)" rowsInserted
        
        let rowsInserted2 = insertUser conn "Jane Smith" "jane@example.com"
        printfn "Inserted %d row(s)" rowsInserted2
        
        // Query and display users
        let users = queryUsers conn
        printfn "\nUsers in database:"
        users |> Seq.iter (fun user ->
            printfn "ID: %d, Name: %s, Email: %s, Created: %A"
                user.Id user.Name user.Email user.CreatedAt
        )
        
        0 // Success
    with
    | ex ->
        printfn "Error: %s" ex.Message
        1 // Error
