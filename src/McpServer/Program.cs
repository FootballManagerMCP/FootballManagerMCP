using McpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();

builder.Services.AddTransient<ApiUnreachableHandler>();
builder.Services.AddHttpClient("FootballManagerApi", c =>
{
    c.BaseAddress = new Uri("http://localhost:5120");
    // A whole tactic written to the game, one write after another, takes up to a minute or two.
    c.Timeout = TimeSpan.FromMinutes(3);
}).AddHttpMessageHandler<ApiUnreachableHandler>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
