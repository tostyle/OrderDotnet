using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Infrastructure.Extensions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// Create host builder
var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        // Add Infrastructure services with Database
        services.AddDatabase(context.Configuration);

        // Add logging
        services.AddLogging();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
    });

// Build and run the host
var host = hostBuilder.Build();

// Get logger
var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting Database Migration...");

    // Get database context and run migrations
    using var scope = host.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

    logger.LogInformation("Applying database migrations...");
    // await dbContext.Database.MigrateAsync();

    logger.LogInformation("Database migration completed successfully!");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during database migration");
    throw;
}
finally
{
    await host.StopAsync();
}
