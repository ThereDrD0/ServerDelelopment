namespace Fourth.Task2;

public record ErrorResponse(int StatusCode, string Message);

public class CustomExceptionA : Exception
{
    public CustomExceptionA() : base("Condition A failed")
    {
    }

    public int StatusCode => 400;
}

public class CustomExceptionB : Exception
{
    public CustomExceptionB() : base("Resource not found")
    {
    }

    public int StatusCode => 404;
}
