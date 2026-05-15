using Microsoft.Data.Sqlite;

namespace Fourth.Task1;

public static class MigrationRunner
{
    private static readonly string ConnectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "products.db")}";

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    public static void Migrate()
    {
        using var connection = GetConnection();
        connection.Open();

        Exec(connection, "CREATE TABLE IF NOT EXISTS __migrations (name TEXT PRIMARY KEY)");
        Apply(connection, "001_products", """
            CREATE TABLE Product (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                price REAL NOT NULL,
                count INTEGER NOT NULL
            );
            INSERT INTO Product (title, price, count) VALUES ('Phone', 599.99, 5);
            INSERT INTO Product (title, price, count) VALUES ('Case', 19.99, 20);
            """);
        Apply(connection, "002_product_description", """
            ALTER TABLE Product ADD COLUMN description TEXT NOT NULL DEFAULT '';
            """);
    }

    private static void Apply(SqliteConnection connection, string name, string sql)
    {
        using var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM __migrations WHERE name = $name";
        check.Parameters.AddWithValue("$name", name);

        if (Convert.ToInt32(check.ExecuteScalar()) > 0)
            return;

        Exec(connection, sql);
        using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO __migrations (name) VALUES ($name)";
        insert.Parameters.AddWithValue("$name", name);
        insert.ExecuteNonQuery();
    }

    private static void Exec(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
