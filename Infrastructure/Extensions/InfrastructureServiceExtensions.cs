using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Temporalio.Client;
using Domain.Repositories;
using Domain.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;

namespace Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering Infrastructure services
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers only the database context in the DI container
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Postgresdb")
                ?? "Host=localhost;Port=5051;Database=OrderDb;Username=postgres;Password=postgres";
            // Console.WriteLine($"Connection string: {connectionString}");
            options.UseNpgsql(connectionString);
        });
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderPaymentRepository, OrderPaymentRepository>();
        services.AddScoped<IOrderStockRepository, OrderStockRepository>();
        services.AddScoped<IOrderLoyaltyRepository, OrderLoyaltyRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IOrderJourneyRepository, OrderJourneyRepository>();
        services.AddScoped<IOrderLogRepository, OrderLogRepository>();
        return services;
    }

    /// <summary>
    /// Registers all Infrastructure services in the DI container
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Database Context
        services.AddDatabase(configuration);

        // Register Temporal Client
        services.AddSingleton<ITemporalClient>(provider =>
        {
            var temporalAddress = configuration["Temporal:Address"] ?? "localhost:7233";
            var temporalNamespace = configuration["Temporal:Namespace"] ?? "default";

            return TemporalClient.ConnectAsync(new TemporalClientConnectOptions
            {
                TargetHost = temporalAddress,
                Namespace = temporalNamespace
            }).GetAwaiter().GetResult();
        });

        // Register Temporal Service

        return services;
    }

    /// <summary>
    /// Ensures database is created and migrated
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}
