using Temporalio.Workflows;
using Workflow.Activities;

namespace Workflow.Workflows;

/// <summary>
/// Main workflow for order processing
/// Receives orderId and orchestrates all order processing activities
/// TODO: All activities are placeholders and will be implemented later
/// </summary>
[Workflow("OrderProcessingWorkflow")]
public class OrderProcessingWorkflow
{
    /// <summary>
    /// Main workflow method that receives orderId and orchestrates the order processing
    /// </summary>
    /// <param name="orderId">The order ID to process</param>
    /// <returns>Workflow result indicating success/failure</returns>
    [WorkflowRun]
    public async Task<string> RunAsync(Guid orderId)
    {
        var workflowId = Temporalio.Workflows.Workflow.Info.WorkflowId;

        try
        {
            // Activity 1: Start Order Workflow
            var startResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.StartOrderWorkflowAsync(workflowId, orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            if (!startResult)
            {
                return $"Failed to start workflow for order {orderId}";
            }

            // Activity 2: Reserve Stock
            var reserveStockResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.ReserveStockAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            if (!reserveStockResult)
            {
                return $"Failed to reserve stock for order {orderId}";
            }

            // Activity 3: Burn Loyalty Transaction
            var burnLoyaltyResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.BurnLoyaltyTransactionAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            if (!burnLoyaltyResult)
            {
                return $"Failed to burn loyalty points for order {orderId}";
            }

            // Activity 4: Earn Loyalty Transaction
            var earnLoyaltyResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.EarnLoyaltyTransactionAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            if (!earnLoyaltyResult)
            {
                return $"Failed to earn loyalty points for order {orderId}";
            }

            // Activity 5: Process Payment
            var processPaymentResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.ProcessPaymentAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            if (!processPaymentResult)
            {
                return $"Failed to process payment for order {orderId}";
            }

            // Activity 6: Complete Cart
            var completeCartResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.CompletedCartAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            if (!completeCartResult)
            {
                return $"Failed to complete cart for order {orderId}";
            }

            // Activity 7: Get Order Detail
            var orderDetail = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.GetOrderDetailAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            return $"Order {orderId} processed successfully. Details: {orderDetail}";
        }
        catch (Exception ex)
        {
            return $"Order processing failed for order {orderId}: {ex.Message}";
        }
    }
}
