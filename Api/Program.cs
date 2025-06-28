using Serilog;
using FluentValidation;
using Application.Services;
using Application.Commands;
using Application.Queries;
using Application.Validators;
using Infrastructure.Extensions;
using Microsoft.Extensions.Hosting;
using MediatR;

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

    // Add MediatR
    builder.Services.AddMediatR(typeof(CreateOrderCommand).Assembly);

    // Add FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<CancelOrderCommandValidator>();

    // Add Application Services
    builder.Services.AddScoped<IOrderApplicationService, OrderApplicationService>();

    // Add Infrastructure Services (Database, Temporal, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

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
    Log.Information("  POST   /api/orders           - Create new order");
    Log.Information("  GET    /api/orders/{{id}}      - Get order by ID");
    Log.Information("  GET    /api/orders           - Get all orders");
    Log.Information("  PUT    /api/orders/{{id}}/state - Update order state");
    Log.Information("  DELETE /api/orders/{{id}}      - Cancel order");

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
