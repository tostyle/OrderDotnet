namespace Domain.Models;

/// <summary>
/// Summary of workflow execution status and progress
/// </summary>
public class WorkflowExecutionSummary
{
    public string WorkflowId { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? CloseTime { get; set; }
    public int TotalEvents { get; set; }
    public int CompletedActivities { get; set; }
    public int FailedActivities { get; set; }
    public int ScheduledActivities { get; set; }
    public List<string> ActivityTypes { get; set; } = new();
}
