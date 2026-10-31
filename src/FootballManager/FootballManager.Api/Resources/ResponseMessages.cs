namespace FootballManager.Api.Resources;

public static class ResponseMessages
{
    public const string Status400Title = "One or more validation errors occurred.";
    public const string Status400JsonErrorsTitle = "errors";
    public const string Status500Title = "An error occurred while processing your request.";
    public const string Status500Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
    public const string Status503Title = "Football Manager isn't available.";
}
