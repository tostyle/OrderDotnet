using Application.DTOs;
using Application.UseCases;
using Domain.Entities;
using Domain.Repositories;

namespace OrderDotnet.Examples;

/// <summary>
/// Example demonstrating the usage of TransitionOrderStateUseCase
/// Shows clean architecture implementation at the application layer
/// </summary>
public class TransitionOrderStateUseCaseExample
{
    /// <summary>
    /// Example of using the TransitionOrderState method directly
    /// </summary>
    public static async Task DirectTransitionExample(IOrderRepository orderRepository)
    {
        Console.WriteLine("=== Direct Transition Example ===");

        var useCase = new TransitionOrderStateUseCase(orderRepository);
        var orderId = Guid.NewGuid(); // In real scenario, this would be an existing order ID

        try
        {
            // Transition order to Pending state
            var result = await useCase.TransitionOrderState(
                orderId: orderId,
                orderState: OrderState.Pending,
                reason: "Customer confirmed order via API"
            );

            if (result.Success)
            {
                Console.WriteLine($"✅ Order {result.OrderId} successfully transitioned");
                Console.WriteLine($"   From: {result.PreviousState} → To: {result.NewState}");
                Console.WriteLine($"   Reason: {result.Reason}");
                Console.WriteLine($"   Transition Time: {result.TransitionedAt}");
                Console.WriteLine($"   Order Version: {result.OrderVersion}");
            }
            else
            {
                Console.WriteLine($"❌ Transition failed: {result.Message}");
                if (result.ErrorDetails != null)
                {
                    Console.WriteLine($"   Valid next states: {string.Join(", ", result.ErrorDetails.ValidNextStates)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Example of using the ExecuteAsync method with full request/response pattern
    /// </summary>
    public static async Task ExecuteAsyncExample(IOrderRepository orderRepository)
    {
        Console.WriteLine("\n=== ExecuteAsync Example ===");

        var useCase = new TransitionOrderStateUseCase(orderRepository);
        var orderId = Guid.NewGuid(); // In real scenario, this would be an existing order ID

        try
        {
            // Create transition request
            var request = new TransitionOrderStateRequest(
                OrderId: orderId,
                OrderState: OrderState.Paid,
                Reason: "Payment successfully processed via payment gateway",
                EnforceBusinessRules: true // Enable business rule validation
            );

            // Execute the transition
            var result = await useCase.ExecuteAsync(request);

            if (result.Success)
            {
                Console.WriteLine($"✅ ExecuteAsync successful for Order {result.OrderId}");
                Console.WriteLine($"   State transition: {result.PreviousState} → {result.NewState}");
                Console.WriteLine($"   Business rules enforced: Yes");
                Console.WriteLine($"   Message: {result.Message}");
            }
            else
            {
                Console.WriteLine($"❌ ExecuteAsync failed: {result.Message}");
                if (result.ErrorDetails != null)
                {
                    Console.WriteLine($"   Current state: {result.ErrorDetails.CurrentState}");
                    Console.WriteLine($"   Attempted state: {result.ErrorDetails.AttemptedState}");
                    Console.WriteLine($"   Valid options: {string.Join(", ", result.ErrorDetails.ValidNextStates)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Example of getting valid next states for an order
    /// </summary>
    public static async Task GetValidStatesExample(IOrderRepository orderRepository)
    {
        Console.WriteLine("\n=== Get Valid States Example ===");

        var useCase = new TransitionOrderStateUseCase(orderRepository);
        var orderId = Guid.NewGuid(); // In real scenario, this would be an existing order ID

        try
        {
            var result = await useCase.GetValidNextStatesAsync(orderId);

            if (result.Success)
            {
                Console.WriteLine($"✅ Valid states retrieved for Order {result.OrderId}");
                Console.WriteLine($"   Current state: {result.CurrentState}");
                Console.WriteLine($"   Valid next states:");
                foreach (var state in result.ValidNextStates)
                {
                    Console.WriteLine($"     - {state}");
                }
            }
            else
            {
                Console.WriteLine($"❌ Failed to get valid states: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Example of a complete order workflow using the use case
    /// </summary>
    public static async Task CompleteWorkflowExample(IOrderRepository orderRepository)
    {
        Console.WriteLine("\n=== Complete Workflow Example ===");

        var useCase = new TransitionOrderStateUseCase(orderRepository);
        var orderId = Guid.NewGuid(); // In real scenario, this would be an existing order ID

        Console.WriteLine($"Starting workflow for Order {orderId}");

        // Step 1: Initial → Pending
        await ExecuteTransitionStep(useCase, orderId, OrderState.Pending, "Customer submitted order", 1);

        // Step 2: Check valid next states
        await ShowValidStates(useCase, orderId);

        // Step 3: Pending → Paid (with business rules)
        await ExecuteTransitionStepWithBusinessRules(useCase, orderId, OrderState.Paid, "Payment processed", 2, enforceRules: false);

        // Step 4: Paid → Completed (with business rules)
        await ExecuteTransitionStepWithBusinessRules(useCase, orderId, OrderState.Completed, "Order fulfilled and delivered", 3, enforceRules: false);

        Console.WriteLine("🎉 Workflow completed!");
    }

    /// <summary>
    /// Helper method to execute a transition step
    /// </summary>
    private static async Task ExecuteTransitionStep(TransitionOrderStateUseCase useCase, Guid orderId, OrderState targetState, string reason, int stepNumber)
    {
        Console.WriteLine($"\nStep {stepNumber}: Transitioning to {targetState}");

        var result = await useCase.TransitionOrderState(orderId, targetState, reason);

        if (result.Success)
        {
            Console.WriteLine($"  ✅ Success: {result.PreviousState} → {result.NewState}");
        }
        else
        {
            Console.WriteLine($"  ❌ Failed: {result.Message}");
        }
    }

    /// <summary>
    /// Helper method to execute a transition step with business rules
    /// </summary>
    private static async Task ExecuteTransitionStepWithBusinessRules(TransitionOrderStateUseCase useCase, Guid orderId, OrderState targetState, string reason, int stepNumber, bool enforceRules = true)
    {
        Console.WriteLine($"\nStep {stepNumber}: Transitioning to {targetState} (Business Rules: {(enforceRules ? "Enabled" : "Disabled")})");

        var request = new TransitionOrderStateRequest(
            OrderId: orderId,
            OrderState: targetState,
            Reason: reason,
            EnforceBusinessRules: enforceRules
        );

        var result = await useCase.ExecuteAsync(request);

        if (result.Success)
        {
            Console.WriteLine($"  ✅ Success: {result.PreviousState} → {result.NewState}");
        }
        else
        {
            Console.WriteLine($"  ❌ Failed: {result.Message}");
            if (result.ErrorDetails != null)
            {
                Console.WriteLine($"     Valid states: {string.Join(", ", result.ErrorDetails.ValidNextStates)}");
            }
        }
    }

    /// <summary>
    /// Helper method to show valid states
    /// </summary>
    private static async Task ShowValidStates(TransitionOrderStateUseCase useCase, Guid orderId)
    {
        Console.WriteLine("\n📋 Checking valid next states...");

        var result = await useCase.GetValidNextStatesAsync(orderId);

        if (result.Success)
        {
            Console.WriteLine($"  Current: {result.CurrentState}");
            Console.WriteLine($"  Valid next: {string.Join(", ", result.ValidNextStates)}");
        }
        else
        {
            Console.WriteLine($"  ❌ Error: {result.ErrorMessage}");
        }
    }

    /// <summary>
    /// Example of error handling scenarios
    /// </summary>
    public static async Task ErrorHandlingExample(IOrderRepository orderRepository)
    {
        Console.WriteLine("\n=== Error Handling Example ===");

        var useCase = new TransitionOrderStateUseCase(orderRepository);
        var invalidOrderId = Guid.NewGuid(); // Non-existent order

        // Example 1: Order not found
        Console.WriteLine("\n1. Testing order not found scenario:");
        var result1 = await useCase.TransitionOrderState(invalidOrderId, OrderState.Pending, "Test error");
        Console.WriteLine($"   Result: {(result1.Success ? "Success" : "Failed")}");
        Console.WriteLine($"   Message: {result1.Message}");

        // Example 2: Invalid state transition
        Console.WriteLine("\n2. Testing invalid state transition:");
        var existingOrderId = Guid.NewGuid(); // Assume this exists in Initial state
        var result2 = await useCase.TransitionOrderState(existingOrderId, OrderState.Completed, "Invalid transition");
        Console.WriteLine($"   Result: {(result2.Success ? "Success" : "Failed")}");
        Console.WriteLine($"   Message: {result2.Message}");
        if (result2.ErrorDetails != null)
        {
            Console.WriteLine($"   Valid states: {string.Join(", ", result2.ErrorDetails.ValidNextStates)}");
        }

        // Example 3: Business rule validation failure
        Console.WriteLine("\n3. Testing business rule validation:");
        var request3 = new TransitionOrderStateRequest(
            OrderId: existingOrderId,
            OrderState: OrderState.Paid,
            Reason: "Attempting to mark as paid without sufficient payment",
            EnforceBusinessRules: true
        );
        var result3 = await useCase.ExecuteAsync(request3);
        Console.WriteLine($"   Result: {(result3.Success ? "Success" : "Failed")}");
        Console.WriteLine($"   Message: {result3.Message}");
    }
}
