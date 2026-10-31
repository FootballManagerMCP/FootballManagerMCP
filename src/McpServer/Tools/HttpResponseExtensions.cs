namespace McpServer.Tools;

public static class HttpResponseExtensions
{
    public static async Task<string> ReadContentOrError(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return string.IsNullOrWhiteSpace(content) ? "{}" : content;
        }

        return string.IsNullOrWhiteSpace(content)
            ? $"{{\"error\": \"{response.StatusCode}\", \"status\": {(int)response.StatusCode}}}"
            : $"{{\"error\": \"{response.StatusCode}\", \"status\": {(int)response.StatusCode}, \"detail\": {content}}}";
    }
}
