namespace FootballManager.Plugin;

internal sealed record Reply(int Status, string Body, string ContentType)
{
    public static Reply Text(int status, string body)
    {
        return new Reply(status, body, "text/plain; charset=utf-8");
    }

    public static Reply Json(string body)
    {
        return new Reply(200, body, "application/json; charset=utf-8");
    }
}
