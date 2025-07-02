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

    /// <summary>
    /// Demonstrates how to inject and use the service in a controller or other service
    /// </summary>
    public class ExampleUsageInController
    {
        // Example: In an API controller
        public static string ControllerUsageExample = @"
[ApiController]
[Route(""api/[controller]"")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowEventQueryService _workflowEventQueryService;

    public WorkflowController(IWorkflowEventQueryService workflowEventQueryService)
    {
        _workflowEventQueryService = workflowEventQueryService;
    }

    [HttpGet(""{orderId}/history"")]
    public async Task<IActionResult> GetWorkflowHistory(Guid orderId)
    {
        var history = await _workflowEventQueryService.GetWorkflowHistoryAsync(orderId);
        return Ok(history);
    }

    [HttpGet(""{orderId}/summary"")]
    public async Task<IActionResult> GetWorkflowSummary(Guid orderId)
    {
        var summary = await _workflowEventQueryService.GetWorkflowExecutionSummaryAsync(orderId);
        return Ok(summary);
    }

    [HttpGet(""{orderId}/activities"")]
    public async Task<IActionResult> GetActivityEvents(Guid orderId, [FromQuery] string? activityType = null)
    {
        var activities = await _workflowEventQueryService.GetWorkflowActivityEventsAsync(orderId, activityType);
        return Ok(activities);
    }
}";
    }

    /// <summary>
    /// Example HTTP responses when using the WorkflowEventQueryService
    /// </summary>
    public class ApiResponseExamples
    {
        // Example workflow history response
        public static string WorkflowHistoryResponse = @"
{
  ""count"": 5,
  ""events"": [
    {
      ""eventId"": 1,
      ""eventType"": ""WorkflowExecutionStarted"",
      ""timestamp"": ""2025-07-02T13:30:00Z"",
      ""activityType"": null,
      ""attributes"": {}
    },
    {
      ""eventId"": 2,
      ""eventType"": ""ActivityTaskScheduled"",
      ""timestamp"": ""2025-07-02T13:30:01Z"",
      ""activityType"": ""TransitionToPendingState"",
      ""attributes"": {
        ""ActivityId"": ""activity-123"",
        ""ActivityType"": ""TransitionToPendingState"",
        ""TaskQueue"": ""order-processing""
      }
    },
    {
      ""eventId"": 3,
      ""eventType"": ""ActivityTaskCompleted"",
      ""timestamp"": ""2025-07-02T13:30:02Z"",
      ""activityType"": null,
      ""attributes"": {
        ""ScheduledEventId"": 2
      }
    }
  ]
}";

        // Example workflow summary response
        public static string WorkflowSummaryResponse = @"
{
  ""workflowId"": ""order-550e8400-e29b-41d4-a716-446655440000"",
  ""runId"": ""01914c8a-1234-7890-abcd-ef1234567890"",
  ""status"": ""Running"",
  ""startTime"": ""2025-07-02T13:30:00Z"",
  ""closeTime"": null,
  ""totalEvents"": 15,
  ""completedActivities"": 3,
  ""failedActivities"": 0,
  ""scheduledActivities"": 3,
  ""activityTypes"": [
    ""TransitionToPendingState"",
    ""ProcessPayment"",
    ""SendConfirmationEmail""
  ]
}";

        // Example activity events response
        public static string ActivityEventsResponse = @"
{
  ""count"": 6,
  ""activityType"": ""TransitionToPendingState"",
  ""events"": [
    {
      ""eventId"": 2,
      ""eventType"": ""ActivityTaskScheduled"",
      ""timestamp"": ""2025-07-02T13:30:01Z"",
      ""activityType"": ""TransitionToPendingState"",
      ""attributes"": {
        ""ActivityId"": ""activity-123"",
        ""ActivityType"": ""TransitionToPendingState"",
        ""TaskQueue"": ""order-processing""
      }
    },
    {
      ""eventId"": 3,
      ""eventType"": ""ActivityTaskStarted"",
      ""timestamp"": ""2025-07-02T13:30:01Z"",
      ""activityType"": ""TransitionToPendingState"",
      ""attributes"": {
        ""ScheduledEventId"": 2
      }
    },
    {
      ""eventId"": 4,
      ""eventType"": ""ActivityTaskCompleted"",
      ""timestamp"": ""2025-07-02T13:30:02Z"",
      ""activityType"": ""TransitionToPendingState"",
      ""attributes"": {
        ""ScheduledEventId"": 2
      }
    }
  ]
}";
    }

    /// <summary>
    /// Service registration examples for dependency injection
    /// </summary>
    public class ServiceRegistrationExamples
    {
        public static string DependencyInjectionExample = @"
// In Program.cs or Startup.cs
// The WorkflowEventQueryService is already registered in WorkflowServiceExtensions.cs

// Usage in any service:
public class SomeService
{
    private readonly IWorkflowEventQueryService _workflowEventQueryService;

    public SomeService(IWorkflowEventQueryService workflowEventQueryService)
    {
        _workflowEventQueryService = workflowEventQueryService;
    }

    public async Task DoSomethingWithWorkflow(Guid orderId)
    {
        // Get workflow summary
        var summary = await _workflowEventQueryService.GetWorkflowExecutionSummaryAsync(orderId);
        
        // Check if workflow is still running
        if (summary.Status == ""Running"")
        {
            // Get activity events to see what's happening
            var activities = await _workflowEventQueryService.GetWorkflowActivityEventsAsync(orderId);
            // Process activities...
        }
    }
}";
    }
}
