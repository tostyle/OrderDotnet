using Serilog;
using Application.Extensions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Hosting;
using Domain.Services;
using OrderWorkflow.Services;

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

    // Add Infrastructure Services (Database, Temporal, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddScoped<IWorkflowService, WorkflowService>();

    // Configure CORS if needed
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
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

    app.UseHttpsRedirection();

    // Use CORS
    app.UseCors("AllowAll");

    app.UseAuthorization();

    app.MapControllers();

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
