using Serilog;
using Application.Extensions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Hosting;
using Domain.Services;
using OrderWorkflow.Services;
using OrderWorkflow.Extensions;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();
    // Add Serilog
    builder.Host.UseSerilog();



    // Add services to the container
    builder.Services.AddControllers();

    // Add OpenAPI/Swagger
    builder.Services.AddOpenApi();

    // Add Application Services
    builder.Services.AddApplication();
    builder.Services.AddWorkflowApplication();

    // Add Infrastructure Services (Database, Temporal, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Configure CORS to explicitly allow PUT, DELETE, PATCH methods
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS", "HEAD")
                  .AllowAnyHeader()
                  .SetPreflightMaxAge(TimeSpan.FromHours(1)); // Cache preflight for 1 hour
        });

        // Add a more permissive policy for development
        options.AddPolicy("Development", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .SetPreflightMaxAge(TimeSpan.FromHours(24));
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "Order Management API v1");
        });
    }

    // Use Serilog for request logging
    app.UseSerilogRequestLogging();

    // Add custom middleware to log HTTP methods for debugging
    app.Use(async (context, next) =>
    {
        var method = context.Request.Method;
        var path = context.Request.Path;
        Log.Information("Incoming request: {Method} {Path}", method, path);

        // Log CORS preflight requests
        if (method == "OPTIONS")
        {
            Log.Information("CORS preflight request for {Path}", path);
        }

        await next();

        Log.Information("Response status: {StatusCode} for {Method} {Path}",
            context.Response.StatusCode, method, path);
    });

    app.UseHttpsRedirection();

    // Use CORS with environment-specific policy
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("Development");
    }
    else
    {
        app.UseCors("AllowAll");
    }

    app.UseAuthorization();
    app.MapControllers();

    // Log all registered routes for debugging
    var routeLogger = app.Services.GetRequiredService<ILogger<Program>>();



    // Ensure database is created
    await app.Services.EnsureDatabaseCreatedAsync();

    Log.Information("Order Management API starting up...");
    Log.Information("Available endpoints:");
    Log.Information("  API endpoints have been removed - controller is empty");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
