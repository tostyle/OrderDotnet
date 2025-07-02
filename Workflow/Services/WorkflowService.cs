using Domain.Services;
using Temporalio.Client;
using OrderWorkflow.OrderWorkflows;
using Temporalio.Api.WorkflowService.V1;
using Temporalio.Api.Enums.V1;
using Microsoft.Extensions.Logging;

namespace OrderWorkflow.Services;

/// <summary>
/// Implementation of IWorkflowService using Temporal
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly ITemporalClient _temporalClient;
    private readonly ILogger<WorkflowService> _logger;

    public WorkflowService(ITemporalClient temporalClient, ILogger<WorkflowService> logger)
    {
        _temporalClient = temporalClient ?? throw new ArgumentNullException(nameof(temporalClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    /// <summary>
    /// Resets workflow to TransitionToPendingState activity for the specified order
    /// This method finds the ActivityType = TransitionToPendingState and resets the temporal workflow to this state
    /// </summary>
    /// <param name="orderId">The order ID to reset workflow for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task ResetWorkflowToPendingStateAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var workflowId = GetWorkflowId(orderId);

        try
        {
            var workflowHandle = _temporalClient.GetWorkflowHandle(workflowId);
            var description = await workflowHandle.DescribeAsync();
            var runId = description?.RunId;

            if (string.IsNullOrEmpty(runId))
            {
                _logger.LogWarning("Workflow {WorkflowId} does not have a valid RunId. The workflow may not be running or may have completed.", workflowId);
                throw new InvalidOperationException($"Workflow {workflowId} does not have a valid RunId. The workflow may not be running or may have completed.");
            }

            _logger.LogInformation("Resetting workflow {WorkflowId} with RunId {RunId}", workflowId, runId);

            var executionOption = new ResetWorkflowExecutionRequest
            {
                Namespace = "default", // Required namespace field
                RequestId = Guid.NewGuid().ToString(), // Required unique request ID
                Reason = "Resetting workflow to TransitionToPendingState activity",
                ResetReapplyType = ResetReapplyType.None,
                ResetReapplyExcludeTypes = { ResetReapplyExcludeType.Signal },
                WorkflowTaskFinishEventId = 10,
                WorkflowExecution = new Temporalio.Api.Common.V1.WorkflowExecution
                {
                    WorkflowId = workflowId,
                    RunId = runId
                }
            };
            // If you need to set ResetReapplyExcludeTypes, you can do it after creation:
            // workflowExecution.ResetReapplyExcludeTypes.Add(ResetReapplyExcludeType.Signal);
            // workflowExecution.ResetReapplyExcludeTypes.Add(ResetReapplyExcludeType.Update);

            await _temporalClient.WorkflowService.ResetWorkflowExecutionAsync(executionOption);
            _logger.LogInformation("Successfully reset workflow {WorkflowId} to TransitionToPendingState activity for order {OrderId} ", workflowId, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset workflow {WorkflowId} to TransitionToPendingState activity for order {OrderId}", workflowId, orderId);
            throw new InvalidOperationException($"Failed to reset workflow {workflowId} to TransitionToPendingState activity for order {orderId}", ex);
        }
    }
}
