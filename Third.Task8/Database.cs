using Microsoft.Data.Sqlite;

namespace Third.Task8;

public static class Database
{
    private static readonly string ConnectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "todos.db")}";

    public static SqliteConnection GetDbConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    public static void Init()
    {
        using var connection = GetDbConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS todos (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                description TEXT NOT NULL,
                completed INTEGER NOT NULL
            )
            """;
        command.ExecuteNonQuery();
    }
}
