using Domain.Aggregates;
using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace OrderDotnet.Examples;

/// <summary>
/// Example demonstrating the usage of TransitionOrderState functionality
/// Shows clean architecture implementation with proper error handling
/// </summary>
public class OrderStateTransitionExample
{
    /// <summary>
    /// Example of basic state transition usage
    /// </summary>
    public static void BasicStateTransitionExample()
    {
        Console.WriteLine("=== Basic State Transition Example ===");

        // Create a new order
        var order = Order.Create("REF-12345");
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        Console.WriteLine($"Initial Order State: {order.OrderState}");

        try
        {
            // Transition to Pending state
            orderAggregate.TransitionOrderState(OrderState.Pending, "Customer confirmed order");
            Console.WriteLine($"After transition to Pending: {order.OrderState}");

            // Try idempotent operation - should not change state
            orderAggregate.TransitionOrderState(OrderState.Pending, "Duplicate request");
            Console.WriteLine($"After idempotent call: {order.OrderState}");

            // Transition to Cancelled
            orderAggregate.TransitionOrderState(OrderState.Cancelled, "Customer cancelled order");
            Console.WriteLine($"After transition to Cancelled: {order.OrderState}");
        }
        catch (StateTransitionException ex)
        {
            Console.WriteLine($"State transition error: {ex.GetDetailedMessage()}");
            Console.WriteLine($"Valid next states: {string.Join(", ", ex.GetValidNextStates())}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Example of invalid state transition handling
    /// </summary>
    public static void InvalidStateTransitionExample()
    {
        Console.WriteLine("\n=== Invalid State Transition Example ===");

        var order = Order.Create("REF-67890");
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        Console.WriteLine($"Initial Order State: {order.OrderState}");

        try
        {
            // Try to transition directly from Initial to Completed (invalid)
            orderAggregate.TransitionOrderState(OrderState.Completed, "Attempting invalid transition");
        }
        catch (StateTransitionException ex)
        {
            Console.WriteLine($"Caught StateTransitionException:");
            Console.WriteLine($"  Order ID: {ex.OrderId}");
            Console.WriteLine($"  Current State: {ex.CurrentState}");
            Console.WriteLine($"  Attempted State: {ex.AttemptedState}");
            Console.WriteLine($"  Error Message: {ex.Message}");
            Console.WriteLine($"  Valid Next States: {string.Join(", ", ex.GetValidNextStates())}");
        }
    }

    /// <summary>
    /// Example of business rule validation
    /// </summary>
    public static void BusinessRuleValidationExample()
    {
        Console.WriteLine("\n=== Business Rule Validation Example ===");

        var order = Order.Create("REF-BUSINESS-RULES");
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        // Add some order items for business rule testing
        var orderItem = OrderItem.Create(
            orderId: order.Id,
            productId: ProductId.From(Guid.NewGuid()),
            quantity: 2,
            netAmount: 100.00m,
            grossAmount: 110.00m,
            currency: "THB");

        order.OrderItems.Add(orderItem);

        Console.WriteLine($"Order with items, total: {orderItem.CalculateTotalGrossAmount():C}");

        try
        {
            // Transition to Pending first
            orderAggregate.TransitionOrderState(OrderState.Pending, "Order submitted");
            Console.WriteLine($"Successfully transitioned to: {order.OrderState}");

            // Try to transition to Paid without sufficient payment (should fail with business rules)
            orderAggregate.SafeTransitionOrderState(
                OrderState.Paid,
                "Attempting to mark as paid without payment",
                enforceBusinessRules: true);
        }
        catch (StateTransitionException ex)
        {
            Console.WriteLine($"Business rule validation failed: {ex.GetDetailedMessage()}");
        }

        try
        {
            // Same transition but bypassing business rules
            orderAggregate.SafeTransitionOrderState(
                OrderState.Paid,
                "Marking as paid (admin override)",
                enforceBusinessRules: false);

            Console.WriteLine($"Successfully bypassed business rules, new state: {order.OrderState}");
        }
        catch (StateTransitionException ex)
        {
            Console.WriteLine($"Still failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Example of checking valid transitions
    /// </summary>
    public static void ValidTransitionCheckExample()
    {
        Console.WriteLine("\n=== Valid Transition Check Example ===");

        var order = Order.Create("REF-VALIDATION");
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        Console.WriteLine($"Current state: {order.OrderState}");

        // Check all possible transitions
        var allStates = Enum.GetValues<OrderState>();
        foreach (var state in allStates)
        {
            var canTransition = orderAggregate.CanTransitionTo(state);
            var businessRuleCompliant = orderAggregate.IsBusinessRuleCompliantTransition(state);

            Console.WriteLine($"  To {state}: CanTransition={canTransition}, BusinessRuleCompliant={businessRuleCompliant}");
        }

        // Get valid next states
        var validNextStates = orderAggregate.GetValidNextStates();
        Console.WriteLine($"Valid next states: {string.Join(", ", validNextStates)}");
    }

    /// <summary>
    /// Complete workflow example
    /// </summary>
    public static void CompleteWorkflowExample()
    {
        Console.WriteLine("\n=== Complete Workflow Example ===");

        var order = Order.Create("REF-WORKFLOW");
        var orderAggregate = OrderAggregate.FromExistingOrder(order);

        // Add order items
        var orderItem = OrderItem.Create(
            orderId: order.Id,
            productId: ProductId.From(Guid.NewGuid()),
            quantity: 1,
            netAmount: 500.00m,
            grossAmount: 550.00m,
            currency: "THB");

        order.OrderItems.Add(orderItem);

        Console.WriteLine($"Starting workflow for order {order.Id}");
        Console.WriteLine($"Order total: {orderItem.CalculateTotalGrossAmount():C}");

        try
        {
            // Step 1: Initial -> Pending
            Console.WriteLine($"\nStep 1: {order.OrderState} -> Pending");
            orderAggregate.TransitionOrderState(OrderState.Pending, "Customer submitted order");
            Console.WriteLine($"✓ Current state: {order.OrderState}");

            // Step 2: Pending -> Paid (bypassing business rules for demo)
            Console.WriteLine($"\nStep 2: {order.OrderState} -> Paid");
            orderAggregate.SafeTransitionOrderState(OrderState.Paid, "Payment processed", enforceBusinessRules: false);
            Console.WriteLine($"✓ Current state: {order.OrderState}");

            // Step 3: Paid -> Completed (bypassing business rules for demo)
            Console.WriteLine($"\nStep 3: {order.OrderState} -> Completed");
            orderAggregate.SafeTransitionOrderState(OrderState.Completed, "Order fulfilled", enforceBusinessRules: false);
            Console.WriteLine($"✓ Current state: {order.OrderState}");

            Console.WriteLine($"\n✅ Workflow completed successfully! Final state: {order.OrderState}");
        }
        catch (StateTransitionException ex)
        {
            Console.WriteLine($"❌ Workflow failed: {ex.GetDetailedMessage()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Run all examples
    /// </summary>
    public static void RunAllExamples()
    {
        try
        {
            BasicStateTransitionExample();
            InvalidStateTransitionExample();
            BusinessRuleValidationExample();
            ValidTransitionCheckExample();
            CompleteWorkflowExample();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running examples: {ex.Message}");
        }
    }
}
