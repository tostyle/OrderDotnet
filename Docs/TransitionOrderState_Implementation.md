# OrderAggregate TransitionOrderState Implementation

## Overview

The `TransitionOrderState` functionality has been implemented as a partial class of `OrderAggregate` following clean architecture principles and SOLID design patterns. This implementation provides robust state transition management with proper validation, error handling, and business rule enforcement.

## Key Components

### 1. StateTransitionException

A custom domain exception that provides detailed context when state transitions fail:

```csharp
public class StateTransitionException : Exception
{
    public OrderId OrderId { get; }
    public OrderState CurrentState { get; }
    public OrderState AttemptedState { get; }
    
    // Provides detailed error messages and valid next states
}
```

**Features:**
- Contains order context (OrderId, current state, attempted state)
- Provides detailed error messages
- Offers methods to get valid next states
- Follows domain exception patterns

### 2. OrderAggregate.TransitionOrderState Partial Class

The main implementation providing state transition functionality:

```csharp
public partial class OrderAggregate
{
    public void TransitionOrderState(OrderState nextState, string? reason = null)
    public void SafeTransitionOrderState(OrderState nextState, string? reason = null, bool enforceBusinessRules = true)
    public bool IsBusinessRuleCompliantTransition(OrderState nextState)
}
```

## Core Methods

### TransitionOrderState(OrderState nextState, string? reason = null)

The primary state transition method that implements the requirements:

1. **Idempotent Check**: Returns immediately if current state equals next state
2. **Validation**: Uses existing `CanTransitionTo()` function for state machine validation
3. **Success Path**: Updates `order.Status` to `nextState` if transition is valid
4. **Error Path**: Creates and throws `StateTransitionException` if transition is invalid

```csharp
public void TransitionOrderState(OrderState nextState, string? reason = null)
{
    // Validate input parameter
    if (!Enum.IsDefined(typeof(OrderState), nextState))
        throw new ArgumentException($"Invalid order state: {nextState}", nameof(nextState));

    var currentState = _order.OrderState;

    // Check if current state equals next state - idempotent operation
    if (currentState == nextState)
        return; // No transition needed

    // Check with CanTransitionTo function for validation
    if (!CanTransitionTo(nextState))
    {
        var exception = new StateTransitionException(
            orderId: _order.Id,
            currentState: currentState,
            attemptedState: nextState,
            message: CreateTransitionErrorMessage(currentState, nextState, reason));
        throw exception;
    }

    // Valid transition - update order status
    _order.OrderState = nextState;
    _order.UpdatedAt = DateTime.UtcNow;
    _order.Version++;
    
    LogStateTransition(previousState, nextState, reason);
}
```

### SafeTransitionOrderState(OrderState nextState, string? reason = null, bool enforceBusinessRules = true)

Enhanced transition method with optional business rule validation:

- Provides additional business rule validation beyond basic state machine rules
- Can be configured to enforce or bypass business rules
- Useful for admin overrides or special scenarios

### IsBusinessRuleCompliantTransition(OrderState nextState)

Validates business rules for specific state transitions:

- **Paid State**: Requires sufficient payments (`TotalPaidAmount >= OrderTotalAmount`)
- **Completed State**: Requires full payment and confirmed stock
- **Cancelled State**: Prevents cancellation of completed orders
- **Refunded State**: Only allows refunds of paid/completed orders

## State Machine Implementation

The implementation leverages the existing `OrderTransitionValidator` which defines valid state transitions:

```csharp
Initial -> Pending
Pending -> Paid, Cancelled
Paid -> Completed, Refunded
Refunded -> Cancelled
Completed -> Cancelled
Cancelled -> (terminal state)
```

## Features

### ✅ Idempotent Operations
- Calling the same transition multiple times has no additional effect
- Prevents unnecessary state changes and maintains system consistency

### ✅ Comprehensive Error Handling
- Custom `StateTransitionException` with detailed context
- Meaningful error messages with valid next states
- Proper exception hierarchy following domain patterns

### ✅ Business Rule Validation
- Separate validation layer for business rules beyond state machine
- Configurable enforcement for different scenarios
- Clear separation of concerns between state machine and business logic

### ✅ Audit Trail
- Logging of all state transitions
- Reason tracking for transitions
- Version incrementing for optimistic concurrency

### ✅ Clean Architecture Compliance
- Domain exceptions in Domain layer
- No external dependencies in domain logic
- Proper separation of concerns
- SOLID principles adherence

## Usage Examples

### Basic State Transition
```csharp
var orderAggregate = OrderAggregate.FromExistingOrder(order);

// Valid transition
orderAggregate.TransitionOrderState(OrderState.Pending, "Customer confirmed order");

// Idempotent - no effect
orderAggregate.TransitionOrderState(OrderState.Pending, "Duplicate request");
```

### Error Handling
```csharp
try 
{
    orderAggregate.TransitionOrderState(OrderState.Completed, "Invalid direct transition");
}
catch (StateTransitionException ex)
{
    Console.WriteLine($"Current: {ex.CurrentState}, Attempted: {ex.AttemptedState}");
    Console.WriteLine($"Valid next states: {string.Join(", ", ex.GetValidNextStates())}");
}
```

### Business Rule Validation
```csharp
// With business rules (default)
orderAggregate.SafeTransitionOrderState(OrderState.Paid, "Payment received", enforceBusinessRules: true);

// Bypass business rules (admin override)
orderAggregate.SafeTransitionOrderState(OrderState.Paid, "Admin override", enforceBusinessRules: false);
```

## Testing

Comprehensive unit tests cover:
- Valid state transitions
- Invalid state transitions
- Idempotent behavior
- Business rule validation
- Exception handling
- Edge cases

## Integration

The implementation integrates seamlessly with existing OrderAggregate functionality:
- Uses existing `CanTransitionTo()` method
- Leverages existing `TotalPaidAmount` property
- Maintains existing order state management patterns
- Compatible with existing temporal workflows

## Benefits

1. **Type Safety**: Strongly typed state transitions with compile-time validation
2. **Maintainability**: Clear separation of concerns and single responsibility
3. **Testability**: Highly testable with comprehensive unit test coverage
4. **Extensibility**: Easy to add new states and business rules
5. **Reliability**: Robust error handling and validation
6. **Auditability**: Complete audit trail of state changes
7. **Performance**: Efficient idempotent operations and minimal overhead

## Future Enhancements

- Domain event publishing for state transitions
- Enhanced logging with structured logging framework
- State transition history tracking
- Async state transition support for external integrations
- State machine visualization and documentation generation
