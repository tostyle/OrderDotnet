using Domain.Services;
using Temporalio.Client;
using OrderWorkflow.OrderWorkflows;

namespace OrderWorkflow.Services;

/// <summary>
/// Implementation of IWorkflowService using Temporal
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly ITemporalClient _temporalClient;

    public WorkflowService(ITemporalClient temporalClient)
    {
        _temporalClient = temporalClient ?? throw new ArgumentNullException(nameof(temporalClient));
    }

    /// <summary>
    /// Generates a workflow ID for order processing.
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>The workflow ID string</returns>
    private static string GetWorkflowId(Guid orderId)
        => $"order-{orderId}";

    /// <summary>
    /// Starts a workflow for order processing
    /// </summary>
    /// <param name="orderId">The order ID to start workflow for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow ID</returns>
    public async Task<string> StartOrderProcessingWorkflowAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var workflowId = GetWorkflowId(orderId);

        var workflowHandle = await _temporalClient.StartWorkflowAsync(
            (OrderProcessingWorkflow wf) => wf.RunAsync(orderId),
            new WorkflowOptions
            {
                Id = workflowId,
                TaskQueue = "order-processing"
            });

        return workflowHandle.Id;
    }

    /// <summary>
    /// Sends a PaymentSuccess signal to the order processing workflow
    /// </summary>
    /// <param name="orderId">The order ID to signal</param>
    /// <param name="paymentId">The payment ID that was successful</param>
    /// <param name="transactionReference">The payment transaction reference</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SendPaymentSuccessSignalAsync(Guid orderId, Guid paymentId, string transactionReference, CancellationToken cancellationToken = default)
    {
        var workflowId = GetWorkflowId(orderId);

        try
        {
            var workflowHandle = _temporalClient.GetWorkflowHandle(workflowId);
            await workflowHandle.SignalAsync("PaymentSuccess", new object[] { paymentId, transactionReference });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send PaymentSuccess signal to workflow {workflowId} for order {orderId}", ex);
        }
    }

    /// <summary>
    /// Sends a CancelOrder signal to the order processing workflow
    /// </summary>
    /// <param name="orderId">The order ID to cancel</param>
    /// <param name="reason">The reason for cancellation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SendCancelOrderSignalAsync(Guid orderId, string reason = "Manual cancellation", CancellationToken cancellationToken = default)
    {
        var workflowId = GetWorkflowId(orderId);

        try
        {
            var workflowHandle = _temporalClient.GetWorkflowHandle(workflowId);
            await workflowHandle.SignalAsync("CancelOrder", new object[] { orderId, reason });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send CancelOrder signal to workflow {workflowId} for order {orderId}", ex);
        }
    }
}
