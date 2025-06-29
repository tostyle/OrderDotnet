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

    /// <summary>
    /// Sends a PaymentSuccess signal to the order processing workflow
    /// </summary>
    /// <param name="orderId">The order ID to signal</param>
    /// <param name="paymentId">The payment ID that was successful</param>
    /// <param name="transactionReference">The payment transaction reference</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SendPaymentSuccessSignalAsync(Guid orderId, Guid paymentId, string transactionReference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a CancelOrder signal to the order processing workflow
    /// </summary>
    /// <param name="orderId">The order ID to cancel</param>
    /// <param name="reason">The reason for cancellation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task SendCancelOrderSignalAsync(Guid orderId, string reason = "Manual cancellation", CancellationToken cancellationToken = default);
}
