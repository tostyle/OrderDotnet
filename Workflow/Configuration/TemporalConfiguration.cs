namespace OrderWorkflow.Configuration;

/// <summary>
/// Configuration options for Temporal connection and worker settings
/// </summary>
public class TemporalConfiguration
{
    public const string SectionName = "Temporal";

    /// <summary>
    /// Temporal server host and port (e.g., "localhost:7233")
    /// </summary>
    public string ServerHost { get; set; } = "localhost:7233";

    /// <summary>
    /// Temporal namespace (e.g., "default", "production", "staging")
    /// </summary>
    public string Namespace { get; set; } = "default";

    /// <summary>
    /// Task queue name for the worker to listen on
    /// </summary>
    public string TaskQueue { get; set; } = "order-processing";

    /// <summary>
    /// Worker name for identification and logging
    /// </summary>
    public string WorkerName { get; set; } = "OrderProcessingWorker";
}
