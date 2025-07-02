// Example usage of Workflow Event Query Service in OrderDotnet
// This file demonstrates how to use the new WorkflowEventQueryService

using Domain.Services;
using Domain.Models;

namespace Examples;

/// <summary>
/// Examples of how to use the WorkflowEventQueryService for querying workflow event history
/// </summary>
public class WorkflowEventQueryExamples
{
    private readonly IWorkflowEventQueryService _workflowEventQueryService;

    public WorkflowEventQueryExamples(IWorkflowEventQueryService workflowEventQueryService)
    {
        _workflowEventQueryService = workflowEventQueryService;
    }

    /// <summary>
    /// Example of getting complete workflow history
    /// </summary>
    public async Task<IList<WorkflowHistoryEvent>> GetWorkflowHistoryExample(Guid orderId)
    {
        var history = await _workflowEventQueryService.GetWorkflowHistoryAsync(orderId);

        // Log some information about the history
        Console.WriteLine($"Workflow history contains {history.Count} events");
        foreach (var evt in history.Take(5)) // Show first 5 events
        {
            Console.WriteLine($"Event {evt.EventId}: {evt.EventType} at {evt.Timestamp}");
        }

        return history;
    }

    /// <summary>
    /// Example of getting activity events with filtering
    /// </summary>
    public async Task<IList<WorkflowHistoryEvent>> GetActivityEventsExample(Guid orderId, string? activityType = null)
    {
        var activityEvents = await _workflowEventQueryService.GetWorkflowActivityEventsAsync(orderId, activityType);

        Console.WriteLine($"Found {activityEvents.Count} activity events" +
                         (activityType != null ? $" for activity type '{activityType}'" : ""));

        return activityEvents;
    }

    /// <summary>
    /// Example of finding specific activity event ID (useful for workflow reset)
    /// </summary>
    public async Task<long?> FindActivityEventIdExample(Guid orderId, string activityType)
    {
        var eventId = await _workflowEventQueryService.FindActivityEventIdAsync(orderId, activityType);

        if (eventId.HasValue)
        {
            Console.WriteLine($"Found activity '{activityType}' at event ID {eventId.Value}");
        }
        else
        {
            Console.WriteLine($"Activity '{activityType}' not found in workflow history");
        }

        return eventId;
    }

    /// <summary>
    /// Example of getting workflow execution summary
    /// </summary>
    public async Task<WorkflowExecutionSummary> GetWorkflowSummaryExample(Guid orderId)
    {
        var summary = await _workflowEventQueryService.GetWorkflowExecutionSummaryAsync(orderId);

        Console.WriteLine($"Workflow Summary for {orderId}:");
        Console.WriteLine($"  Status: {summary.Status}");
        Console.WriteLine($"  Total Events: {summary.TotalEvents}");
        Console.WriteLine($"  Completed Activities: {summary.CompletedActivities}");
        Console.WriteLine($"  Failed Activities: {summary.FailedActivities}");
        Console.WriteLine($"  Activity Types: {string.Join(", ", summary.ActivityTypes)}");

        return summary;
    }
{
    /// <summary>
    /// Example HTTP endpoint responses when querying workflow history
    /// </summary>
    public class ApiExamples
    {
        // 1. Get complete workflow history
        public static string GetWorkflowHistoryExample = @"
GET /api/workflows/{orderId}/history
Response:
{
  ""workflowId"": ""order-550e8400-e29b-41d4-a716-446655440000"",
  ""events"": [
    {
      ""eventId"": 1,
      ""eventType"": ""WorkflowExecutionStarted"",
      ""timestamp"": ""2025-07-02T13:30:00Z""
    },
    {
      ""eventId"": 2,
      ""eventType"": ""ActivityTaskScheduled"",
      ""activityType"": ""TransitionToPendingState"",
      ""timestamp"": ""2025-07-02T13:30:01Z""
    },
    {
      ""eventId"": 3,
      ""eventType"": ""ActivityTaskCompleted"",
      ""result"": ""Pending"",
      ""timestamp"": ""2025-07-02T13:30:02Z""
    }
  ]
}";

        // 2. Get workflow execution summary
        public static string GetWorkflowSummaryExample = @"
GET /api/workflows/{orderId}/summary
Response:
{
  ""workflowId"": ""order-550e8400-e29b-41d4-a716-446655440000"",
  ""runId"": ""01914c8a-1234-7890-abcd-ef1234567890"",
  ""status"": ""Running"",
  ""startTime"": ""2025-07-02T13:30:00Z"",
  ""closeTime"": null,
  ""totalEvents"": 15,
  ""completedActivities"": 3,
  ""failedActivities"": 0,
  ""scheduledActivities"": 5,
  ""activityTypes"": [
    ""TransitionToPendingState"",
    ""ReserveStock"",
    ""ProcessPayment"",
    ""BurnLoyaltyTransaction"",
    ""CompleteOrder""
  ]
}";

        // 3. Get specific activity events
        public static string GetActivityEventsExample = @"
GET /api/workflows/{orderId}/activities?type=TransitionToPendingState
Response:
{
  ""activityType"": ""TransitionToPendingState"",
  ""events"": [
    {
      ""eventId"": 2,
      ""eventType"": ""ActivityTaskScheduled"",
      ""timestamp"": ""2025-07-02T13:30:01Z""
    },
    {
      ""eventId"": 5,
      ""eventType"": ""ActivityTaskStarted"",
      ""timestamp"": ""2025-07-02T13:30:01Z""
    },
    {
      ""eventId"": 6,
      ""eventType"": ""ActivityTaskCompleted"",
      ""result"": ""Pending"",
      ""timestamp"": ""2025-07-02T13:30:02Z""
    }
  ]
}";
    }

    /// <summary>
    /// Code examples showing how to use the WorkflowService methods
    /// </summary>
    public class CodeExamples
    {
        public static string UsageExample = @"
// Example: Using WorkflowService to query workflow history

public class OrderWorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;
    
    public OrderWorkflowController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    // 1. Get complete workflow history
    [HttpGet(""{orderId}/history"")]
    public async Task<ActionResult> GetWorkflowHistory(Guid orderId)
    {
        var history = await _workflowService.GetWorkflowHistoryAsync(orderId);
        return Ok(new { events = history });
    }

    // 2. Get workflow execution summary
    [HttpGet(""{orderId}/summary"")]
    public async Task<ActionResult> GetWorkflowSummary(Guid orderId)
    {
        var summary = await _workflowService.GetWorkflowExecutionSummaryAsync(orderId);
        return Ok(summary);
    }

    // 3. Get specific activity events
    [HttpGet(""{orderId}/activities"")]
    public async Task<ActionResult> GetActivityEvents(Guid orderId, string? type = null)
    {
        var events = await _workflowService.GetWorkflowActivityEventsAsync(orderId, type);
        return Ok(new { activityType = type, events });
    }

    // 4. Find specific activity event ID (useful for workflow reset)
    [HttpGet(""{orderId}/activities/{activityType}/event-id"")]
    public async Task<ActionResult> FindActivityEventId(Guid orderId, string activityType)
    {
        var eventId = await _workflowService.FindActivityEventIdAsync(orderId, activityType);
        
        if (eventId.HasValue)
        {
            return Ok(new { activityType, eventId = eventId.Value });
        }
        
        return NotFound(new { message = $""Activity '{activityType}' not found in workflow history"" });
    }
}";

        public static string ResetWorkflowExample = @"
// Example: Using workflow history to reset workflow to specific activity

public async Task ResetOrderToPaymentStep(Guid orderId)
{
    try
    {
        // Find the ProcessPayment activity event ID
        var paymentEventId = await _workflowService.FindActivityEventIdAsync(orderId, ""ProcessPayment"");
        
        if (paymentEventId.HasValue)
        {
            // Reset workflow to ProcessPayment activity
            await _workflowService.ResetWorkflowToPendingStateAsync(orderId);
            _logger.LogInformation(""Reset order {OrderId} workflow to ProcessPayment activity at event {EventId}"", 
                orderId, paymentEventId.Value);
        }
        else
        {
            _logger.LogWarning(""ProcessPayment activity not found in workflow history for order {OrderId}"", orderId);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, ""Failed to reset workflow for order {OrderId}"", orderId);
        throw;
    }
}";

        public static string DebuggingExample = @"
// Example: Using workflow history for debugging and monitoring

public async Task<WorkflowDiagnostics> DiagnoseWorkflowIssues(Guid orderId)
{
    var summary = await _workflowService.GetWorkflowExecutionSummaryAsync(orderId);
    var allEvents = await _workflowService.GetWorkflowHistoryAsync(orderId);
    
    var diagnostics = new WorkflowDiagnostics
    {
        OrderId = orderId,
        Status = summary.Status,
        IsStuck = summary.Status == ""Running"" && summary.CompletedActivities == 0,
        HasFailures = summary.FailedActivities > 0,
        ExecutionTimeMinutes = summary.CloseTime.HasValue 
            ? (summary.CloseTime.Value - summary.StartTime).TotalMinutes
            : (DateTime.UtcNow - summary.StartTime).TotalMinutes,
        
        FailedActivities = allEvents
            .Where(e => e.EventType == EventType.ActivityTaskFailed)
            .Select(e => new FailedActivity
            {
                ActivityType = GetScheduledEventActivityType(allEvents, e.ActivityTaskFailedEventAttributes?.ScheduledEventId ?? 0),
                FailureReason = e.ActivityTaskFailedEventAttributes?.Failure?.Message,
                Timestamp = e.EventTime?.ToDateTime() ?? DateTime.MinValue
            })
            .ToList(),
            
        Recommendations = GenerateRecommendations(summary, allEvents)
    };
    
    return diagnostics;
}

public class WorkflowDiagnostics
{
    public Guid OrderId { get; set; }
    public string Status { get; set; }
    public bool IsStuck { get; set; }
    public bool HasFailures { get; set; }
    public double ExecutionTimeMinutes { get; set; }
    public List<FailedActivity> FailedActivities { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class FailedActivity
{
    public string ActivityType { get; set; }
    public string FailureReason { get; set; }
    public DateTime Timestamp { get; set; }
}";
    }

    /// <summary>
    /// Common workflow event types you might encounter
    /// </summary>
    public class EventTypeReference
    {
        public static string EventTypesDocumentation = @"
COMMON TEMPORAL WORKFLOW EVENT TYPES:
====================================

Workflow Events:
- WorkflowExecutionStarted: Workflow began execution
- WorkflowExecutionCompleted: Workflow finished successfully
- WorkflowExecutionFailed: Workflow failed with an error
- WorkflowExecutionTimedOut: Workflow exceeded timeout
- WorkflowExecutionCanceled: Workflow was cancelled

Activity Events:
- ActivityTaskScheduled: Activity was scheduled to run
- ActivityTaskStarted: Activity began execution
- ActivityTaskCompleted: Activity finished successfully
- ActivityTaskFailed: Activity failed with an error
- ActivityTaskTimedOut: Activity exceeded timeout
- ActivityTaskCanceled: Activity was cancelled

Signal Events:
- WorkflowExecutionSignaled: Workflow received a signal
- SignalExternalWorkflowExecutionInitiated: Signal sent to external workflow

Timer Events:
- TimerStarted: Timer was started
- TimerFired: Timer elapsed
- TimerCanceled: Timer was cancelled

Child Workflow Events:
- StartChildWorkflowExecutionInitiated: Child workflow start requested
- ChildWorkflowExecutionStarted: Child workflow began
- ChildWorkflowExecutionCompleted: Child workflow completed
- ChildWorkflowExecutionFailed: Child workflow failed

Decision Events:
- WorkflowTaskScheduled: Decision task scheduled
- WorkflowTaskStarted: Decision task started
- WorkflowTaskCompleted: Decision task completed
- WorkflowTaskFailed: Decision task failed
";
    }
}
