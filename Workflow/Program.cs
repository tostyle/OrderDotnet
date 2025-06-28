using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Temporalio.Extensions.Hosting;
using Temporalio.Client;
using Temporalio.Worker;
using Temporalio.Workflows;
using Temporalio.Activities;
using Workflow.Activities;
using Workflow.Workflows;
using Workflow.Extensions;
using Workflow.Configuration;

// Create host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure logging from appsettings.json
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Bind Temporal configuration from appsettings.json
var temporalConfig = new TemporalConfiguration();
builder.Configuration.GetSection(TemporalConfiguration.SectionName).Bind(temporalConfig);

// Register configuration for dependency injection
builder.Services.Configure<TemporalConfiguration>(
    builder.Configuration.GetSection(TemporalConfiguration.SectionName));

// Add Temporal client connection using configuration
builder.Services.AddTemporalClient(options =>
{
    // Configure Temporal server connection from config
    options.TargetHost = temporalConfig.ServerHost;
    options.Namespace = temporalConfig.Namespace;
});

// Register activity dependencies for dependency injection
builder.Services.AddScoped<OrderActivities>();

// Register and configure Temporal worker using configuration
// This automatically discovers and registers ALL activities in OrderActivities class
// marked with [Activity] attribute, plus the workflow
builder.Services.AddHostedTemporalWorker(temporalConfig.TaskQueue)
    .AddScopedActivities<OrderActivities>()   // Registers all [Activity] methods automatically
    .AddWorkflow<OrderProcessingWorkflow>();  // Registers the workflow
// builder.Services.Configure<TemporalWorkerServiceOptions>(options =>
// {
//     options.TaskQueue = "order-processing";

//     // Register workflow
//     options.Workflows.Add(WorkflowDefinition.Create<OrderProcessingWorkflow>());

//     // Register the entire OrderActivities class - this will automatically discover
//     // all methods marked with [Activity] attribute
//     options.Activities.Add(ActivityDefinition.CreateAll<OrderActivities>());

//     // Manual registration (commented out - replaced by class-level registration)
//     options.Activities.Add(ActivityDefinition.Create((OrderActivities a) => a.StartOrderWorkflowAsync(default!, default!)));
//     options.Activities.Add(ActivityDefinition.Create((OrderActivities a) => a.ReserveStockAsync(default!)));
//     options.Activities.Add(ActivityDefinition.Create((OrderActivities a) => a.BurnLoyaltyTransactionAsync(default!)));
//     options.Activities.Add(ActivityDefinition.Create((OrderActivities a) => a.EarnLoyaltyTransactionAsync(default!)));
//     options.Activities.Add(ActivityDefinition.Create((OrderActivities a) => a.ProcessPaymentAsync(default!)));
//     options.Activities.Add(ActivityDefinition.Create((OrderActivities a) => a.CompletedCartAsync(default!)));
//     options.Activities.Add(ActivityDefinition.Create((OrderActivities a) => a.GetOrderDetailAsync(default!)));
// });

// Build and run the host
var host = builder.Build();

// Log startup information using configuration values
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== {WorkerName} ===", temporalConfig.WorkerName);
logger.LogInformation("Temporal Server: {ServerHost}", temporalConfig.ServerHost);
logger.LogInformation("Namespace: {Namespace}", temporalConfig.Namespace);
logger.LogInformation("Task Queue: {TaskQueue}", temporalConfig.TaskQueue);
logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);
logger.LogInformation("Worker Status: Ready to process workflows");
logger.LogInformation("Press Ctrl+C to shutdown");
logger.LogInformation("============================================");

try
{
    // Start the host - this will start the Temporal worker
    // The worker will connect to Temporal server and listen for workflow executions
    await host.RunAsync();
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
