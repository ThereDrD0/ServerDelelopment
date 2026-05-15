namespace Second.Task7;

public record CommonHeaders(string UserAgent, string AcceptLanguage)
{
    public static CommonHeaders? From(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("User-Agent", out var userAgent) ||
            !request.Headers.TryGetValue("Accept-Language", out var acceptLanguage))
            return null;

        return new CommonHeaders(userAgent.ToString(), acceptLanguage.ToString());
    }

    public Dictionary<string, string> ToResponse()
    {
        return new Dictionary<string, string>
        {
            ["User-Agent"] = UserAgent,
            ["Accept-Language"] = AcceptLanguage
        };
    }
}
