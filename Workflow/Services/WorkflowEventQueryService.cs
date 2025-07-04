using Domain.Services;
using Domain.Models;
using Temporalio.Client;
using Temporalio.Api.WorkflowService.V1;
using Temporalio.Api.Enums.V1;
using Microsoft.Extensions.Logging;

namespace OrderWorkflow.Services;

/// <summary>
/// Implementation of IWorkflowEventQueryService using Temporal
/// Handles all workflow event querying and history operations
/// </summary>
public class WorkflowEventQueryService : IWorkflowEventQueryService
{
    private readonly ITemporalClient _temporalClient;
    private readonly ILogger<WorkflowEventQueryService> _logger;

    public WorkflowEventQueryService(ITemporalClient temporalClient, ILogger<WorkflowEventQueryService> logger)
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
    /// Gets the complete workflow execution history for debugging and analysis
    /// </summary>
    /// <param name="orderId">The order ID to get history for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of workflow history events</returns>
    public async Task<IList<WorkflowHistoryEvent>> GetWorkflowHistoryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var temporalEvents = await GetTemporalWorkflowHistoryAsync(orderId, cancellationToken);
        return temporalEvents.Select(ConvertToWorkflowHistoryEvent).ToList();
    }

    /// <summary>
    /// Gets specific activity events from workflow history
    /// </summary>
    /// <param name="orderId">The order ID to get activity events for</param>
    /// <param name="activityType">Optional activity type to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of activity-related history events</returns>
    public async Task<IList<WorkflowHistoryEvent>> GetWorkflowActivityEventsAsync(
        Guid orderId,
        string? activityType = null,
        CancellationToken cancellationToken = default)
    {
        var allEvents = await GetTemporalWorkflowHistoryAsync(orderId, cancellationToken);

        // Filter for activity-related events
        var activityEvents = allEvents.Where(e =>
            e.EventType == EventType.ActivityTaskScheduled ||
            e.EventType == EventType.ActivityTaskStarted ||
            e.EventType == EventType.ActivityTaskCompleted ||
            e.EventType == EventType.ActivityTaskFailed ||
            e.EventType == EventType.ActivityTaskTimedOut ||
            e.EventType == EventType.ActivityTaskCanceled
        ).ToList();

        // Further filter by activity type if specified
        if (!string.IsNullOrEmpty(activityType))
        {
            activityEvents = activityEvents.Where(e =>
            {
                return e.EventType switch
                {
                    EventType.ActivityTaskScheduled => e.ActivityTaskScheduledEventAttributes?.ActivityType?.Name == activityType,
                    EventType.ActivityTaskStarted => GetScheduledEventActivityType(allEvents, e.ActivityTaskStartedEventAttributes?.ScheduledEventId ?? 0) == activityType,
                    EventType.ActivityTaskCompleted => GetScheduledEventActivityType(allEvents, e.ActivityTaskCompletedEventAttributes?.ScheduledEventId ?? 0) == activityType,
                    EventType.ActivityTaskFailed => GetScheduledEventActivityType(allEvents, e.ActivityTaskFailedEventAttributes?.ScheduledEventId ?? 0) == activityType,
                    EventType.ActivityTaskTimedOut => GetScheduledEventActivityType(allEvents, e.ActivityTaskTimedOutEventAttributes?.ScheduledEventId ?? 0) == activityType,
                    EventType.ActivityTaskCanceled => GetScheduledEventActivityType(allEvents, e.ActivityTaskCanceledEventAttributes?.ScheduledEventId ?? 0) == activityType,
                    _ => false
                };
            }).ToList();
        }

        _logger.LogInformation("Found {ActivityEventCount} activity events for workflow order {OrderId} with activity type filter '{ActivityType}'",
            activityEvents.Count, orderId, activityType ?? "none");

        return activityEvents.Select(ConvertToWorkflowHistoryEvent).ToList();
    }


    public async Task<IList<WorkflowHistoryEvent>> GetCheckPointWorkflowAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var allEvents = await GetTemporalWorkflowHistoryAsync(orderId, cancellationToken);

        // Filter for activity-related events
        var activityEvents = allEvents.Where(e =>
            e.EventType == EventType.ActivityTaskScheduled ||
            e.EventType == EventType.WorkflowTaskCompleted
        ).Select(ConvertToWorkflowHistoryEvent).ToList();

        return activityEvents;
    }

    public async Task<long?> FindCheckPointEventIdAsync(Guid orderId, string activityType, CancellationToken cancellationToken = default)
    {
        var allEvents = await GetTemporalWorkflowHistoryAsync(orderId, cancellationToken);
        var eventList = allEvents.ToList();
        // Lambda function to recursively find the first WorkflowCompleted event by index
        Func<int, Temporalio.Api.History.V1.HistoryEvent?> FindWorkflowCompletedEvent = null!;
        FindWorkflowCompletedEvent = (index) =>
        {
            if (index >= allEvents.Count)
                return null;
            var currentEvent = allEvents[index];
            var isWorkflowTaskCompleted = currentEvent.EventType == EventType.WorkflowTaskCompleted;
            return isWorkflowTaskCompleted ? currentEvent : FindWorkflowCompletedEvent(index + 1);
        };
        var scheduledEventIndex = eventList.FindIndex(e =>
            e.EventType == EventType.ActivityTaskScheduled &&
            e.ActivityTaskScheduledEventAttributes?.ActivityType?.Name == activityType);

        var workflowCompletedEvent = FindWorkflowCompletedEvent(scheduledEventIndex);
        return workflowCompletedEvent?.EventId;
    }


    /// <summary>
    /// Finds the event ID for a specific activity type (useful for workflow reset)
    /// </summary>
    /// <param name="orderId">The order ID to search</param>
    /// <param name="activityType">The activity type to find</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The event ID of the activity, or null if not found</returns>
    public async Task<long?> FindActivityEventIdAsync(Guid orderId, string activityType, CancellationToken cancellationToken = default)
    {
        try
        {
            var allEvents = await GetTemporalWorkflowHistoryAsync(orderId, cancellationToken);

            // Look for the scheduled event of the specified activity type
            var scheduledEvent = allEvents.FirstOrDefault(e =>
                e.EventType == EventType.ActivityTaskScheduled &&
                e.ActivityTaskScheduledEventAttributes?.ActivityType?.Name == activityType);

            if (scheduledEvent != null)
            {
                _logger.LogInformation("Found activity '{ActivityType}' at event ID {EventId} for order {OrderId}",
                    activityType, scheduledEvent.EventId, orderId);
                return scheduledEvent.EventId;
            }

            _logger.LogWarning("Activity '{ActivityType}' not found in workflow history for order {OrderId}", activityType, orderId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find activity event ID for '{ActivityType}' in order {OrderId}", activityType, orderId);
            throw;
        }
    }

    /// <summary>
    /// Gets a summary of workflow execution status and progress
    /// </summary>
    /// <param name="orderId">The order ID to get summary for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Workflow execution summary</returns>
    public async Task<Domain.Models.WorkflowExecutionSummary> GetWorkflowExecutionSummaryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var workflowId = GetWorkflowId(orderId);
            var workflowHandle = _temporalClient.GetWorkflowHandle(workflowId);
            var description = await workflowHandle.DescribeAsync();
            var history = await GetTemporalWorkflowHistoryAsync(orderId, cancellationToken);

            var summary = new WorkflowExecutionSummary
            {
                WorkflowId = workflowId,
                RunId = description?.RunId ?? "",
                Status = description?.Status.ToString() ?? "Unknown",
                StartTime = description?.StartTime ?? DateTime.MinValue,
                CloseTime = description?.CloseTime,
                TotalEvents = history.Count,
                CompletedActivities = history.Count(e => e.EventType == EventType.ActivityTaskCompleted),
                FailedActivities = history.Count(e => e.EventType == EventType.ActivityTaskFailed),
                ScheduledActivities = history.Count(e => e.EventType == EventType.ActivityTaskScheduled),
                ActivityTypes = history
                    .Where(e => e.EventType == EventType.ActivityTaskScheduled)
                    .Select(e => e.ActivityTaskScheduledEventAttributes?.ActivityType?.Name ?? "Unknown")
                    .Distinct()
                    .ToList()
            };

            _logger.LogInformation("Generated workflow execution summary for order {OrderId}: {Status}, {TotalEvents} events, {CompletedActivities} completed activities",
                orderId, summary.Status, summary.TotalEvents, summary.CompletedActivities);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get workflow execution summary for order {OrderId}", orderId);
            throw;
        }
    }

    /// <summary>
    /// Gets the complete Temporal workflow execution history (internal method)
    /// </summary>
    /// <param name="orderId">The order ID to get history for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of Temporal workflow history events</returns>
    private async Task<IList<Temporalio.Api.History.V1.HistoryEvent>> GetTemporalWorkflowHistoryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var workflowId = GetWorkflowId(orderId);

        try
        {
            var workflowHandle = _temporalClient.GetWorkflowHandle(workflowId);
            var description = await workflowHandle.DescribeAsync();
            var runId = description?.RunId;

            if (string.IsNullOrEmpty(runId))
            {
                _logger.LogWarning("Cannot get history for workflow {WorkflowId} - no valid RunId found", workflowId);
                throw new InvalidOperationException($"Workflow {workflowId} does not have a valid RunId");
            }

            // Get workflow history using WorkflowService
            var request = new GetWorkflowExecutionHistoryRequest
            {
                Namespace = "default",
                Execution = new Temporalio.Api.Common.V1.WorkflowExecution
                {
                    WorkflowId = workflowId,
                    RunId = runId
                },
                MaximumPageSize = 1000, // Adjust as needed
                WaitNewEvent = false
            };

            var response = await _temporalClient.WorkflowService.GetWorkflowExecutionHistoryAsync(request);

            _logger.LogInformation("Retrieved {EventCount} history events for workflow {WorkflowId}",
                response.History.Events.Count, workflowId);

            return response.History.Events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get workflow history for {WorkflowId}", workflowId);
            throw new InvalidOperationException($"Failed to get workflow history for {workflowId}", ex);
        }
    }

    /// <summary>
    /// Converts a Temporal history event to domain model
    /// </summary>
    /// <param name="temporalEvent">The Temporal history event</param>
    /// <returns>Domain workflow history event</returns>
    private static WorkflowHistoryEvent ConvertToWorkflowHistoryEvent(Temporalio.Api.History.V1.HistoryEvent temporalEvent)
    {
        var eventType = temporalEvent.EventType.ToString();
        var activityType = GetActivityTypeFromEvent(temporalEvent);

        var attributes = new Dictionary<string, object>();

        // Add relevant attributes based on event type
        switch (temporalEvent.EventType)
        {
            case EventType.ActivityTaskScheduled:
                if (temporalEvent.ActivityTaskScheduledEventAttributes != null)
                {
                    attributes["ActivityId"] = temporalEvent.ActivityTaskScheduledEventAttributes.ActivityId ?? "";
                    attributes["ActivityType"] = temporalEvent.ActivityTaskScheduledEventAttributes.ActivityType?.Name ?? "";
                    attributes["TaskQueue"] = temporalEvent.ActivityTaskScheduledEventAttributes.TaskQueue?.Name ?? "";
                }
                break;
            case EventType.ActivityTaskCompleted:
                if (temporalEvent.ActivityTaskCompletedEventAttributes != null)
                {
                    attributes["ScheduledEventId"] = temporalEvent.ActivityTaskCompletedEventAttributes.ScheduledEventId;
                }
                break;
            case EventType.ActivityTaskFailed:
                if (temporalEvent.ActivityTaskFailedEventAttributes != null)
                {
                    attributes["ScheduledEventId"] = temporalEvent.ActivityTaskFailedEventAttributes.ScheduledEventId;
                    attributes["Failure"] = temporalEvent.ActivityTaskFailedEventAttributes.Failure?.Message ?? "";
                }
                break;
        }

        return new WorkflowHistoryEvent
        {
            EventId = temporalEvent.EventId,
            EventType = eventType,
            Timestamp = temporalEvent.EventTime?.ToDateTime() ?? DateTime.MinValue,
            ActivityType = activityType,
            Attributes = attributes
        };
    }

    /// <summary>
    /// Extracts activity type from a Temporal history event
    /// </summary>
    /// <param name="temporalEvent">The Temporal history event</param>
    /// <returns>The activity type if available</returns>
    private static string? GetActivityTypeFromEvent(Temporalio.Api.History.V1.HistoryEvent temporalEvent)
    {
        return temporalEvent.EventType switch
        {
            EventType.ActivityTaskScheduled => temporalEvent.ActivityTaskScheduledEventAttributes?.ActivityType?.Name,
            _ => null
        };
    }

    /// <summary>
    /// Helper method to get activity type from a scheduled event ID
    /// </summary>
    /// <param name="allEvents">All workflow history events</param>
    /// <param name="scheduledEventId">The scheduled event ID to look up</param>
    /// <returns>The activity type name</returns>
    private static string? GetScheduledEventActivityType(IList<Temporalio.Api.History.V1.HistoryEvent> allEvents, long scheduledEventId)
    {
        var scheduledEvent = allEvents.FirstOrDefault(e => e.EventId == scheduledEventId);
        return scheduledEvent?.ActivityTaskScheduledEventAttributes?.ActivityType?.Name;
    }
}
