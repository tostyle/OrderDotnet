using Temporalio.Workflows;
using Workflow.Activities;

namespace Workflow.Workflows;

/// <summary>
/// State object to track order processing progress
/// </summary>
public sealed class OrderProcessingState
{
    /// <summary>
    /// Indicates if the workflow has been started
    /// </summary>
    public bool IsStartedWorkflow { get; set; } = false;

    /// <summary>
    /// Indicates if the order payment has been completed
    /// </summary>
    public bool IsPaid { get; set; } = false;

    /// <summary>
    /// Indicates if the order has been canceled
    /// </summary>
    public bool IsCanceled { get; set; } = false;

    /// <summary>
    /// Indicates if stock has been reserved for the order
    /// </summary>
    public bool IsReserveStock { get; set; } = false;

    /// <summary>
    /// Indicates if loyalty points have been burned
    /// </summary>
    public bool IsBurnedLoyalty { get; set; } = false;
}

/// <summary>
/// Main workflow for order processing
/// Receives orderId and orchestrates all order processing activities
/// Uses signal/condition pattern for payment handling with timeout
/// </summary>
[Workflow("OrderProcessingWorkflow")]
public class OrderProcessingWorkflow
{
    private readonly OrderProcessingState _state = new();

    /// <summary>
    /// Signal handler for payment success
    /// </summary>
    /// <param name="orderId">The order ID that was paid</param>
    [WorkflowSignal("PaymentSuccess")]
    public Task PaymentSuccessAsync(Guid orderId)
    {
        _state.IsPaid = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Signal handler for order cancellation
    /// </summary>
    /// <param name="orderId">The order ID to cancel</param>
    [WorkflowSignal("CancelOrder")]
    public Task CancelOrderSignalAsync(Guid orderId)
    {
        _state.IsCanceled = true;
        return Task.CompletedTask;
    }
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
                (OrderActivities activities) => activities.StartOrderWorkflowAsync(orderId, workflowId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            _state.IsStartedWorkflow = true;

            // Activity 2: Reserve Stock
            var reserveStockResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.ReserveStockAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            _state.IsReserveStock = true;

            // Activity 3: Burn Loyalty Transaction
            var burnLoyaltyResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.BurnLoyaltyTransactionAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            _state.IsBurnedLoyalty = true;

            // Wait for payment or cancellation with 30-minute timeout
            var paymentTimeout = TimeSpan.FromMinutes(30);
            var paymentReceived = await Temporalio.Workflows.Workflow.WaitConditionAsync(
                () => _state.IsPaid || _state.IsCanceled,
                paymentTimeout);

            if (_state.IsCanceled || !paymentReceived)
            {
                await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                    (OrderActivities activities) => activities.CancelOrderAsync(orderId),
                    new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

                return $"Order {orderId} was canceled";
            }


            // Activity 4: Earn Loyalty Transaction (after payment)
            var earnLoyaltyResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.EarnLoyaltyTransactionAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            // Activity 6: Complete Cart
            var completeCartResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.CompletedCartAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            // Activity 7: Get Order Detail
            var orderDetail = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (OrderActivities activities) => activities.GetOrderDetailAsync(orderId),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

            return $"Order {orderId} processed successfully. Details: {orderDetail}";
        }
        catch (Exception ex)
        {
            // If any unexpected error occurs, consider canceling the order
            // @TODO handle error gracefully, log it, and return a meaningful message

            return $"Order processing failed for order {orderId}: {ex.Message}";
        }
    }
}
