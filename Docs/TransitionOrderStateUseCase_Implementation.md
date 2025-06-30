# TransitionOrderStateUseCase Implementation

## Overview

The `TransitionOrderStateUseCase` has been implemented following clean architecture principles and your specific requirements. This use case serves as the application layer component that orchestrates order state transitions using the domain logic implemented in `OrderAggregate.TransitionOrderState`.

## Key Components

### 1. TransitionOrderStateUseCase Class

**Location**: `Application/UseCases/TransitionOrderStateUseCase.cs`

**Purpose**: Handles order state transitions at the application layer, coordinating between the presentation layer and domain layer.

**Dependencies**:
- `IOrderRepository` - For order persistence operations
- Console logging (can be easily replaced with proper logging framework)

### 2. Data Transfer Objects (DTOs)

**Location**: `Application/DTOs/OrderDtos.cs`

**New DTOs Added**:
- `TransitionOrderStateRequest` - Request model for state transitions
- `TransitionOrderStateResponse` - Response model with transition results
- `StateTransitionErrorDetails` - Error details for failed transitions
- `GetValidStatesResponse` - Response model for valid states query

## Core Methods

### TransitionOrderState(Guid orderId, OrderState orderState, string? reason = null)

The main function as requested that:

1. **Receives Parameters**: `orderId` and `orderState` as specified
2. **Retrieves Order**: Fetches order from repository using `IOrderRepository`
3. **Creates Aggregate**: Uses `OrderAggregate.FromExistingOrder(order)`
4. **Calls Domain Logic**: Executes `orderAggregate.TransitionOrderState(orderState, reason)`
5. **Persists Changes**: Saves updated order back to repository
6. **Returns Response**: Provides detailed success/failure response

```csharp
public async Task<TransitionOrderStateResponse> TransitionOrderState(
    Guid orderId,
    OrderState orderState,
    string? reason = null,
    CancellationToken cancellationToken = default)
{
    // Implementation handles all the orchestration
}
```

### ExecuteAsync(TransitionOrderStateRequest request)

Enhanced execution method that:

- Provides full request/response pattern
- Supports business rule enforcement configuration
- Offers comprehensive validation
- Follows async best practices

### GetValidNextStatesAsync(Guid orderId)

Utility method that:

- Returns valid next states for a given order
- Helps UI/API layers show available options
- Provides current state context

## Features

### ✅ **Clean Architecture Compliance**

**Application Layer Responsibilities**:
- Orchestrates use case execution
- Handles cross-cutting concerns (logging, validation)
- Transforms between DTOs and domain models
- Manages transaction boundaries

**Domain Layer Integration**:
- Uses domain aggregates for business logic
- Leverages domain exceptions for error handling
- Respects domain model encapsulation

**Infrastructure Abstraction**:
- Depends on repository interfaces, not implementations
- Can work with any persistence technology
- Supports dependency injection

### ✅ **Error Handling Strategy**

**Graceful Degradation**:
- Never throws exceptions to callers
- Always returns response objects with success/failure status
- Provides detailed error context for debugging

**Domain Exception Handling**:
- Catches `StateTransitionException` and provides user-friendly responses
- Includes valid next states in error responses
- Maintains error context for troubleshooting

**Unexpected Error Handling**:
- Catches all other exceptions gracefully
- Logs errors for investigation
- Returns safe error messages to consumers

### ✅ **Response Patterns**

**Success Response**:
```csharp
{
    "OrderId": "guid",
    "PreviousState": "Initial",
    "NewState": "Pending", 
    "Success": true,
    "Message": "Order successfully transitioned from Initial to Pending",
    "Reason": "Customer confirmed order",
    "TransitionedAt": "2025-06-30T10:30:00Z",
    "OrderVersion": 2
}
```

**Error Response**:
```csharp
{
    "OrderId": "guid",
    "PreviousState": "Initial",
    "NewState": "Completed",
    "Success": false,
    "Message": "Invalid state transition from Initial to Completed...",
    "Reason": "Invalid transition attempt",
    "TransitionedAt": "2025-06-30T10:30:00Z",
    "OrderVersion": null,
    "ErrorDetails": {
        "CurrentState": "Initial",
        "AttemptedState": "Completed", 
        "ValidNextStates": ["Pending"]
    }
}
```

### ✅ **Idempotent Operations**

- Multiple calls with same parameters are safe
- Underlying domain logic handles idempotency
- Repository operations are atomic
- Response includes version information for optimistic concurrency

### ✅ **Business Rule Integration**

**Configurable Enforcement**:
- `ExecuteAsync` method supports `EnforceBusinessRules` parameter
- Can bypass business rules for admin operations
- Maintains audit trail of enforcement decisions

**Business Rule Types**:
- Payment validation for Paid state
- Stock confirmation for Completed state
- Cancellation restrictions for Completed orders
- Refund eligibility checks

## Usage Patterns

### 1. Simple State Transition

```csharp
var useCase = new TransitionOrderStateUseCase(orderRepository);

var result = await useCase.TransitionOrderState(
    orderId: orderId,
    orderState: OrderState.Pending,
    reason: "Customer confirmed order"
);

if (result.Success)
{
    // Handle success
    Console.WriteLine($"Order transitioned to {result.NewState}");
}
else
{
    // Handle failure
    Console.WriteLine($"Transition failed: {result.Message}");
}
```

### 2. Request/Response Pattern

```csharp
var request = new TransitionOrderStateRequest(
    OrderId: orderId,
    OrderState: OrderState.Paid,
    Reason: "Payment processed",
    EnforceBusinessRules: true
);

var result = await useCase.ExecuteAsync(request);
```

### 3. Getting Valid States

```csharp
var validStates = await useCase.GetValidNextStatesAsync(orderId);
foreach (var state in validStates.ValidNextStates)
{
    Console.WriteLine($"Can transition to: {state}");
}
```

## Integration Points

### **API Controllers**

```csharp
[HttpPost("orders/{orderId}/transition")]
public async Task<IActionResult> TransitionOrderState(
    Guid orderId, 
    [FromBody] TransitionOrderStateRequest request)
{
    var result = await _transitionOrderStateUseCase.TransitionOrderState(
        orderId, request.OrderState, request.Reason);
    
    return result.Success ? Ok(result) : BadRequest(result);
}
```

### **Temporal Workflows**

```csharp
// Inside workflow activity
var result = await transitionOrderStateUseCase.TransitionOrderState(
    orderId: workflowInput.OrderId,
    orderState: OrderState.Paid,
    reason: "Payment completed via workflow"
);

if (!result.Success)
{
    throw new WorkflowException($"State transition failed: {result.Message}");
}
```

### **Event Handlers**

```csharp
public async Task Handle(PaymentProcessedEvent paymentEvent)
{
    var result = await _transitionOrderStateUseCase.TransitionOrderState(
        orderId: paymentEvent.OrderId,
        orderState: OrderState.Paid,
        reason: $"Payment {paymentEvent.PaymentId} processed"
    );
    
    if (result.Success)
    {
        await _eventPublisher.PublishAsync(new OrderStateChangedEvent(result));
    }
}
```

## Testing

### **Unit Tests**

**Coverage Areas**:
- Successful state transitions
- Order not found scenarios  
- Invalid state transitions
- Business rule validation
- Error handling
- Idempotent behavior
- Request validation

**Test Structure**:
- Arrange: Mock repositories and create test data
- Act: Execute use case methods
- Assert: Verify response properties and repository interactions

### **Example Test**

```csharp
[Fact]
public async Task TransitionOrderState_WhenValidTransition_ShouldReturnSuccess()
{
    // Arrange
    var order = Order.Create("TEST-REF");
    _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<OrderId>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(order);
    
    // Act
    var result = await _useCase.TransitionOrderState(order.Id.Value, OrderState.Pending);
    
    // Assert
    Assert.True(result.Success);
    Assert.Equal(OrderState.Pending.ToString(), result.NewState);
}
```

## Performance Considerations

### **Database Operations**
- Single repository call to fetch order
- Single repository call to update order
- Atomic transaction boundary
- Optimistic concurrency with version checking

### **Memory Usage**
- Minimal object allocation
- Efficient DTO mapping
- Proper disposal of resources

### **Scalability**
- Stateless use case design
- Thread-safe operations
- Supports horizontal scaling
- Compatible with distributed architectures

## Security Considerations

### **Input Validation**
- Validates all input parameters
- Checks for valid enum values
- Sanitizes reason strings
- Guards against injection attacks

### **Authorization** (Future Enhancement)
- Can be extended with user context
- Supports role-based transition permissions
- Audit trail includes user information

### **Data Protection**
- Sensitive information not logged
- Secure error messages
- Proper exception handling

## Benefits

1. **Separation of Concerns**: Clear boundary between application and domain logic
2. **Testability**: Easy to unit test with mocked dependencies
3. **Maintainability**: Simple, focused responsibility
4. **Extensibility**: Easy to add new features like authorization, caching
5. **Reliability**: Comprehensive error handling and validation
6. **Performance**: Efficient data access patterns
7. **Monitoring**: Built-in logging and response tracking

## Future Enhancements

- **Caching**: Add caching for frequently accessed orders
- **Authorization**: Integrate with user authentication/authorization
- **Metrics**: Add performance monitoring and metrics
- **Events**: Publish domain events for state transitions
- **Batch Operations**: Support for bulk state transitions
- **Validation**: Enhanced business rule validation
- **Audit**: Comprehensive audit logging
- **Retry Logic**: Automatic retry for transient failures
