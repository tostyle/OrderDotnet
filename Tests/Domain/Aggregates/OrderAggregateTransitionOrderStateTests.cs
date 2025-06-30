using Domain.Aggregates;
using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;
using Xunit;

namespace Tests.Domain.Aggregates;

/// <summary>
/// Unit tests for OrderAggregate TransitionOrderState functionality
/// Tests state transition logic, validation, and exception handling
/// </summary>
public class OrderAggregateTransitionOrderStateTests
{
    [Fact]
    public void TransitionOrderState_WhenCurrentStateEqualsNextState_ShouldReturnWithoutChanges()
    {
        // Arrange
        var order = Order.Create();
        var aggregate = OrderAggregate.FromExistingOrder(order);
        var currentState = order.OrderState;

        // Act
        aggregate.TransitionOrderState(currentState, "Test idempotent behavior");

        // Assert
        Assert.Equal(currentState, aggregate.Order.OrderState);
    }

    [Fact]
    public void TransitionOrderState_WhenValidTransition_ShouldUpdateOrderState()
    {
        // Arrange
        var order = Order.Create();
        var aggregate = OrderAggregate.FromExistingOrder(order);
        var nextState = OrderState.Pending;

        // Act
        aggregate.TransitionOrderState(nextState, "Test valid transition");

        // Assert
        Assert.Equal(nextState, aggregate.Order.OrderState);
    }

    [Fact]
    public void TransitionOrderState_WhenInvalidTransition_ShouldThrowStateTransitionException()
    {
        // Arrange
        var order = Order.Create();
        var aggregate = OrderAggregate.FromExistingOrder(order);
        var invalidNextState = OrderState.Completed; // Cannot go from Initial to Completed directly

        // Act & Assert
        var exception = Assert.Throws<StateTransitionException>(() =>
            aggregate.TransitionOrderState(invalidNextState, "Test invalid transition"));

        Assert.Equal(order.Id, exception.OrderId);
        Assert.Equal(OrderState.Initial, exception.CurrentState);
        Assert.Equal(OrderState.Completed, exception.AttemptedState);
    }

    [Fact]
    public void TransitionOrderState_WhenInvalidOrderState_ShouldThrowArgumentException()
    {
        // Arrange
        var order = Order.Create();
        var aggregate = OrderAggregate.FromExistingOrder(order);
        var invalidState = (OrderState)999; // Invalid enum value

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            aggregate.TransitionOrderState(invalidState, "Test invalid enum"));

        Assert.Contains("Invalid order state", exception.Message);
    }

    [Fact]
    public void SafeTransitionOrderState_WhenBusinessRulesViolated_ShouldThrowStateTransitionException()
    {
        // Arrange
        var order = Order.Create();
        var aggregate = OrderAggregate.FromExistingOrder(order);

        // Transition to Pending first
        aggregate.TransitionOrderState(OrderState.Pending);

        // Try to transition to Paid without sufficient payment
        var paidState = OrderState.Paid;

        // Act & Assert
        var exception = Assert.Throws<StateTransitionException>(() =>
            aggregate.SafeTransitionOrderState(paidState, "Test business rule violation", enforceBusinessRules: true));

        Assert.Equal(order.Id, exception.OrderId);
        Assert.Equal(OrderState.Pending, exception.CurrentState);
        Assert.Equal(OrderState.Paid, exception.AttemptedState);
        Assert.Contains("Business rule validation failed", exception.Message);
    }

    [Fact]
    public void SafeTransitionOrderState_WhenBusinessRulesDisabled_ShouldAllowTransition()
    {
        // Arrange
        var order = Order.Create();
        var aggregate = OrderAggregate.FromExistingOrder(order);

        // Transition to Pending first
        aggregate.TransitionOrderState(OrderState.Pending);

        // Try to transition to Paid without sufficient payment but with business rules disabled
        var paidState = OrderState.Paid;

        // Act
        aggregate.SafeTransitionOrderState(paidState, "Test business rule bypass", enforceBusinessRules: false);

        // Assert
        Assert.Equal(paidState, aggregate.Order.OrderState);
    }

    [Fact]
    public void IsBusinessRuleCompliantTransition_WhenValidTransition_ShouldReturnTrue()
    {
        // Arrange
        var order = Order.Create();
        var aggregate = OrderAggregate.FromExistingOrder(order);
        var validNextState = OrderState.Pending;

        // Act
        var result = aggregate.IsBusinessRuleCompliantTransition(validNextState);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsBusinessRuleCompliantTransition_WhenInvalidTransition_ShouldReturnFalse()
    {
        // Arrange
        var order = Order.Create();
        var aggregate = OrderAggregate.FromExistingOrder(order);
        var invalidNextState = OrderState.Completed;

        // Act
        var result = aggregate.IsBusinessRuleCompliantTransition(invalidNextState);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(OrderState.Initial, OrderState.Pending, true)]
    [InlineData(OrderState.Pending, OrderState.Paid, true)]
    [InlineData(OrderState.Pending, OrderState.Cancelled, true)]
    [InlineData(OrderState.Paid, OrderState.Completed, true)]
    [InlineData(OrderState.Paid, OrderState.Refunded, true)]
    [InlineData(OrderState.Initial, OrderState.Completed, false)]
    [InlineData(OrderState.Cancelled, OrderState.Paid, false)]
    public void TransitionOrderState_VariousStateTransitions_ShouldFollowStateMachine(
        OrderState fromState,
        OrderState toState,
        bool shouldSucceed)
    {
        // Arrange
        var order = Order.Create();
        var aggregate = OrderAggregate.FromExistingOrder(order);

        // Set up the initial state
        if (fromState != OrderState.Initial)
        {
            // Force the order to the desired initial state for testing
            aggregate.TransitionOrderState(OrderState.Pending);
            if (fromState == OrderState.Paid)
            {
                aggregate.SafeTransitionOrderState(OrderState.Paid, enforceBusinessRules: false);
            }
            else if (fromState == OrderState.Cancelled)
            {
                aggregate.TransitionOrderState(OrderState.Cancelled);
            }
        }

        // Act & Assert
        if (shouldSucceed)
        {
            aggregate.TransitionOrderState(toState, $"Transition from {fromState} to {toState}");
            Assert.Equal(toState, aggregate.Order.OrderState);
        }
        else
        {
            Assert.Throws<StateTransitionException>(() =>
                aggregate.TransitionOrderState(toState, $"Invalid transition from {fromState} to {toState}"));
        }
    }
}
