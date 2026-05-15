namespace Fourth.Task5;

public static class UserStore
{
    private static int _nextId = 1;
    private static readonly Dictionary<int, UserIn> Users = new();

    public static UserOut Create(UserIn user)
    {
        var id = _nextId++;
        Users[id] = user;
        return new UserOut(id, user.Username, user.Age);
    }

    public static UserOut? Get(int id)
    {
        return Users.TryGetValue(id, out var user) ? new UserOut(id, user.Username, user.Age) : null;
    }

    public static bool Delete(int id)
    {
        return Users.Remove(id);
    }

    public static void Clear()
    {
        Users.Clear();
        _nextId = 1;
    }
}

public record UserIn(string Username, int Age);

public record UserOut(int Id, string Username, int Age);
