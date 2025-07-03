using Microsoft.Extensions.DependencyInjection;
using Domain.Services;
using OrderWorkflow.Services;

namespace OrderWorkflow.Extensions;

/// <summary>
/// Extension methods for registering Workflow application services
/// </summary>
public static class WorkflowServiceExtensions
{
    /// <summary>
    /// Adds Workflow application services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddWorkflowApplication(this IServiceCollection services)
    {
        // Register WorkflowEventQueryService with scoped lifetime
        services.AddScoped<IWorkflowEventQueryService, WorkflowEventQueryService>();

        // Register WorkflowService with scoped lifetime
        services.AddScoped<IOrderWorkflowService, OrderWorkflowService>();

        return services;
    }
}
