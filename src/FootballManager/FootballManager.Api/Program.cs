using System.Text.Json.Serialization;
using Asp.Versioning;
using FootballManager.Api.ExceptionHandler;
using FootballManager.Api.Mappings;
using FootballManager.Application;
using FootballManager.Infrastructure;
using FootballManager.Infrastructure.Settings;
using Mapster;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Bootstrap Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Application", "FootballManager.Api")
    .CreateBootstrapLogger();

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("Application", "FootballManager.Api")
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341");
});

// Configure Mapster
var mappingConfig = new MappingConfig();
mappingConfig.Register(TypeAdapterConfig.GlobalSettings);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// TypedResults serialise with the HTTP JSON options, not the MVC ones above. Unrequested fields (attributes, hidden
// data) are left out rather than sent as nulls, keeping answers small for the model reading them.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// Add application services
builder.Services.AddApplicationServices();

// Add infrastructure services
var infrastructureSettings = builder.Configuration
    .GetSection("InfrastructureSettings")
    .Get<InfrastructureSettings>();

if (infrastructureSettings == null)
{
    throw new InvalidOperationException("InfrastructureSettings configuration is missing");
}

builder.Services.AddInfrastructureServices(infrastructureSettings);

// Add exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
