# Order State Transition System

## Overview

The Order State Transition System is implemented in `Domain/Entities/OrderState.cs` and provides a robust state management mechanism for order processing. This system ensures that orders can only transition between valid states according to predefined business rules.

## Order States

The system defines six distinct order states:

| State | Description |
|-------|-------------|
| `Initial` | The starting state when an order is first created |
| `Pending` | Order is awaiting payment or processing |
| `Paid` | Payment has been received and confirmed |
| `Refunded` | Payment has been refunded to the customer |
| `Completed` | Order has been successfully fulfilled |
| `Cancelled` | Order has been cancelled and will not be processed |

## State Transition Rules

The system enforces strict transition rules to maintain data integrity and business logic consistency:

### Valid Transitions

1. **Initial → Pending**
   - New orders must first move to pending state for processing

2. **Pending → Paid** or **Pending → Cancelled**
   - From pending, orders can either receive payment or be cancelled

3. **Paid → Completed** or **Paid → Refunded**
   - Paid orders can either be completed successfully or refunded

4. **Refunded → Cancelled**
   - Refunded orders must be cancelled as they cannot proceed

5. **Completed → Cancelled**
   - Even completed orders can be cancelled (e.g., for returns or disputes)

6. **Cancelled → Pending**
   - Cancelled orders can be reactivated and moved back to pending

### State Transition Diagram

```
Initial
   ↓
Pending ←──────────────┐
   ↓                   │
   ├─→ Paid            │
   │    ↓              │
   │    ├─→ Completed ─┘
   │    │       ↓
   │    └─→ Refunded
   │            ↓
   └─→ Cancelled ──────┘
```

## Implementation Details

### OrderTransitionRule Record

```csharp
record OrderTransitionRule(OrderState State, List<OrderState> NextStates);
```

This record defines the relationship between a current state and its allowable next states. It uses C# records for immutability and value semantics.

### OrderTransitionValidator Class

The `OrderTransitionValidator` class contains:

1. **Static Rules Array**: Defines all valid state transitions
2. **IsValidTransition Method**: Validates whether a transition from one state to another is allowed

### Validation Logic

```csharp
public static bool IsValidTransition(OrderState from, OrderState to)
    => Rules.Any(rule => rule.State == from && rule.NextStates.Contains(to));
```

This method:
- Takes the current state (`from`) and desired state (`to`)
- Searches the rules array for a matching rule
- Returns `true` if the transition is valid, `false` otherwise

## Usage Examples

### Valid Transitions
```csharp
// Valid transitions
OrderTransitionValidator.IsValidTransition(OrderState.Initial, OrderState.Pending);     // true
OrderTransitionValidator.IsValidTransition(OrderState.Pending, OrderState.Paid);       // true
OrderTransitionValidator.IsValidTransition(OrderState.Paid, OrderState.Completed);     // true
```

### Invalid Transitions
```csharp
// Invalid transitions
OrderTransitionValidator.IsValidTransition(OrderState.Initial, OrderState.Completed);  // false
OrderTransitionValidator.IsValidTransition(OrderState.Completed, OrderState.Paid);     // false
OrderTransitionValidator.IsValidTransition(OrderState.Refunded, OrderState.Pending);   // false
```

## Design Principles

### 1. **Immutability**
- Uses records and readonly collections to prevent modification of rules
- Ensures thread safety and predictable behavior

### 2. **Single Responsibility**
- `OrderTransitionValidator` has one job: validate state transitions
- Clear separation of concerns

### 3. **Open/Closed Principle**
- Easy to add new states by extending the Rules array
- Existing code doesn't need modification for new states

### 4. **Fail-Fast Validation**
- Invalid transitions are caught immediately
- Prevents inconsistent state changes

## Benefits

1. **Data Integrity**: Prevents invalid state changes that could corrupt business logic
2. **Predictable Behavior**: Clear rules make system behavior predictable
3. **Easy Maintenance**: Centralized rules make it easy to modify business logic
4. **Performance**: Static rules array provides O(n) lookup performance
5. **Type Safety**: Enum-based states prevent invalid state values

## Considerations

1. **Rule Complexity**: As business rules grow, consider more sophisticated validation mechanisms
2. **Performance**: For high-throughput scenarios, consider caching or optimization
3. **Extensibility**: Future requirements might need conditional transitions based on order properties
4. **Audit Trail**: Consider integrating with workflow history for state change tracking

## Integration with Domain Layer

This state transition system is part of the Domain layer and follows clean architecture principles:
- No external dependencies
- Pure business logic
- Framework-agnostic implementation
- Can be easily tested in isolation
