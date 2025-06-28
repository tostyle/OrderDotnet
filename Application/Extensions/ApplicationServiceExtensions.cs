using Microsoft.Extensions.DependencyInjection;
using Application.Services;

namespace Application.Extensions;

/// <summary>
/// Extension methods for registering Application layer services
/// </summary>
public static class ApplicationServiceExtensions
{
    /// <summary>
    /// Adds Application layer services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register OrderService with scoped lifetime
        services.AddScoped<OrderService>();
        
        return services;
    }
}
