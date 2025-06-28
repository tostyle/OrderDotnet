using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// Create host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Build and run the host
var host = builder.Build();

// Log startup
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Temporal Worker for Order Processing");
logger.LogInformation("This is a placeholder implementation - will be fully implemented once Temporal server is available");
logger.LogInformation("Worker would listen on task queue: order-processing");
logger.LogInformation("Press Ctrl+C to shutdown");

try
{
    // For now, just keep the application running
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    // Expected when cancellation is requested
}
catch (Exception ex)
{
    logger.LogError(ex, "Error running Temporal Worker");
    throw;
}
finally
{
    logger.LogInformation("Temporal Worker shutdown complete");
}
