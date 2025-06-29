namespace Domain.Services;

/// <summary>
/// Interface for workflow management service
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Starts a workflow for order processing
    /// </summary>
    /// <param name="orderId">The order ID to start workflow for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The workflow ID</returns>
    Task<string> StartOrderProcessingWorkflowAsync(Guid orderId, CancellationToken cancellationToken = default);
}
