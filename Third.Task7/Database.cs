using Microsoft.Data.Sqlite;

namespace Third.Task7;

public static class Database
{
    private static readonly string ConnectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "users.db")}";

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
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL,
                password TEXT NOT NULL
            )
            """;
        command.ExecuteNonQuery();
    }
}
