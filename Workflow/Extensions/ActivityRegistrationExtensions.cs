using Temporalio.Activities;
using Workflow.Activities;

namespace Workflow.Extensions;

/// <summary>
/// Extension methods for registering Temporal activities
/// </summary>
public static class ActivityRegistrationExtensions
{
    /// <summary>
    /// Registers all OrderActivities in one convenient method call
    /// </summary>
    /// <param name="activities">The activities collection</param>
    public static void AddOrderActivities(this IList<ActivityDefinition> activities)
    {
        // Register all 7 activities from OrderActivities class
        activities.Add(ActivityDefinition.Create((OrderActivities a) => a.StartOrderWorkflowAsync(default!, default!)));
        activities.Add(ActivityDefinition.Create((OrderActivities a) => a.ReserveStockAsync(default!)));
        activities.Add(ActivityDefinition.Create((OrderActivities a) => a.BurnLoyaltyTransactionAsync(default!)));
        activities.Add(ActivityDefinition.Create((OrderActivities a) => a.EarnLoyaltyTransactionAsync(default!)));
        activities.Add(ActivityDefinition.Create((OrderActivities a) => a.ProcessPaymentAsync(default!)));
        activities.Add(ActivityDefinition.Create((OrderActivities a) => a.CompletedCartAsync(default!)));
        activities.Add(ActivityDefinition.Create((OrderActivities a) => a.GetOrderDetailAsync(default!)));
    }
}
