namespace Domain.Models;

/// <summary>
/// Generic representation of a workflow history event
/// </summary>
public class WorkflowHistoryEvent
{
    public long EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ActivityType { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}
