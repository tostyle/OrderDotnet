using Domain.Services;
using Temporalio.Client;
using Workflow.Workflows;

namespace Workflow.Services;

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
}
