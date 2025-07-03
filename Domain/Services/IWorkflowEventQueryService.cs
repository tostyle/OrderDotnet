using Domain.Models;

namespace Domain.Services;

/// <summary>
/// Interface for workflow event and history query operations
/// </summary>
public interface IWorkflowEventQueryService
{
    /// <summary>
    /// Gets the complete workflow execution history for debugging and analysis
    /// </summary>
    /// <param name="orderId">The order ID to get history for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workflow history events</returns>
    Task<IList<WorkflowHistoryEvent>> GetWorkflowHistoryAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets specific activity events from workflow history
    /// </summary>
    /// <param name="orderId">The order ID to get activity events for</param>
    /// <param name="activityType">Optional activity type to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of activity-related history events</returns>
    Task<IList<WorkflowHistoryEvent>> GetWorkflowActivityEventsAsync(
        Guid orderId,
        string? activityType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the event ID for a specific activity type (useful for workflow reset)
    /// </summary>
    /// <param name="orderId">The order ID to search</param>
    /// <param name="activityType">The activity type to find</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The event ID of the activity, or null if not found</returns>
    Task<long?> FindActivityEventIdAsync(Guid orderId, string activityType, CancellationToken cancellationToken = default);
    Task<long?> FindCheckPointEventIdAsync(Guid orderId, string activityType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a summary of workflow execution status and progress
    /// </summary>
    /// <param name="orderId">The order ID to get summary for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workflow execution summary</returns>
    Task<WorkflowExecutionSummary> GetWorkflowExecutionSummaryAsync(Guid orderId, CancellationToken cancellationToken = default);
}
