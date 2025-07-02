// Example usage of the new Order State endpoint
// PUT /api/orders/state/{state}

namespace Examples;

/// <summary>
/// Example demonstrating how to use the new order state endpoint
/// </summary>
public class OrderStateEndpointExample
{
    /// <summary>
    /// Example HTTP requests for the new order state endpoint
    /// </summary>
    public class HttpExamples
    {
        // 1. Valid request to change order to Pending state
        public static string ChangeToPendingExample = @"
PUT /api/orders/state/pending
Content-Type: application/json

{
  ""orderId"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""reason"": ""Reset order to pending state for reprocessing"",
  ""initiatedBy"": ""admin@company.com""
}

Expected Response (200 OK):
{
  ""orderId"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""previousState"": ""Cancelled"",
  ""newState"": ""Pending"",
  ""isAlreadyPending"": false,
  ""message"": ""Order status successfully changed to Pending""
}";

        // 2. Invalid state request
        public static string InvalidStateExample = @"
PUT /api/orders/state/invalidstate
Content-Type: application/json

{
  ""orderId"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""reason"": ""Testing invalid state""
}

Expected Response (400 Bad Request):
{
  ""error"": ""Invalid order state: invalidstate. Valid states are: Initial, Pending, Paid, Refunded, Completed, Cancelled""
}";

        // 3. Unsupported state transition
        public static string UnsupportedStateExample = @"
PUT /api/orders/state/paid
Content-Type: application/json

{
  ""orderId"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""reason"": ""Attempting unsupported transition""
}

Expected Response (400 Bad Request):
{
  ""error"": ""Order state transition to 'paid' is not currently supported. Only 'Pending' state transitions are implemented.""
}";

        // 4. Order not found
        public static string OrderNotFoundExample = @"
PUT /api/orders/state/pending
Content-Type: application/json

{
  ""orderId"": ""00000000-0000-0000-0000-000000000000"",
  ""reason"": ""Testing with non-existent order""
}

Expected Response (404 Not Found):
{
  ""error"": ""Order with ID 00000000-0000-0000-0000-000000000000 not found""
}";
    }

    /// <summary>
    /// State transition rules and business logic
    /// </summary>
    public class BusinessRules
    {
        public static string Documentation = @"
ORDER STATE TRANSITION RULES:
============================

Current Implementation:
- Only 'Pending' state transitions are supported in this endpoint
- Orders can only transition to Pending from Cancelled state
- The operation is idempotent (calling it multiple times has the same effect)

Switch Case Logic:
- case Pending: Calls ChangeOrderStatusToPendingUseCase
- default: Throws NotSupportedException

Future Extensions:
To add support for other states, you would:
1. Add new use cases (e.g., ChangeOrderStatusToPaidUseCase)
2. Add new case statements in the switch block
3. Implement the corresponding business logic

Example Future Implementation:
case OrderState.Paid:
    var paidRequest = new ChangeOrderStatusToPaidRequest { ... };
    var paidResponse = await _changeOrderStatusToPaidUseCase.ExecuteAsync(paidRequest);
    return Ok(paidResponse);

case OrderState.Cancelled:
    var cancelRequest = new ChangeOrderStatusToCancelledRequest { ... };
    var cancelResponse = await _changeOrderStatusToCancelledUseCase.ExecuteAsync(cancelRequest);
    return Ok(cancelResponse);
";
    }

    /// <summary>
    /// Integration with existing endpoints
    /// </summary>
    public class EndpointComparison
    {
        public static string Comparison = @"
ENDPOINT COMPARISON:
==================

New Generic Endpoint:
PUT /api/orders/state/{state}
- Generic approach for any state transition
- Uses switch case for different state handlers
- Extensible for future state transitions

Existing Specific Endpoint:
POST /api/orders/{orderId}/status/pending
- Specific endpoint for pending state only
- OrderId passed in URL route
- More specific and focused

Both endpoints:
- Use the same underlying ChangeOrderStatusToPendingUseCase
- Follow clean architecture principles
- Include proper logging and error handling
- Return the same response format for pending transitions
";
    }
}
