using Application.DTOs;
using Application.UseCases;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Tests.Application.UseCases;

/// <summary>
/// Unit tests for TransitionOrderStateUseCase
/// Tests the use case functionality including success and error scenarios
/// </summary>
public class TransitionOrderStateUseCaseTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly TransitionOrderStateUseCase _useCase;

    public TransitionOrderStateUseCaseTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _useCase = new TransitionOrderStateUseCase(_mockOrderRepository.Object);
    }

    [Fact]
    public async Task TransitionOrderState_WhenOrderExists_ShouldReturnSuccessResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var order = Order.Create("TEST-REF-001");
        var targetState = OrderState.Pending;

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mockOrderRepository.Setup(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOrderRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.TransitionOrderState(orderId, targetState, "Test transition");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(OrderState.Initial.ToString(), result.PreviousState);
        Assert.Equal(targetState.ToString(), result.NewState);
        Assert.Equal("Test transition", result.Reason);
        Assert.Contains("successfully transitioned", result.Message);

        _mockOrderRepository.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransitionOrderState_WhenOrderNotFound_ShouldReturnErrorResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var targetState = OrderState.Pending;

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _useCase.TransitionOrderState(orderId, targetState, "Test transition");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(orderId, result.OrderId);
        Assert.Contains("not found", result.Message);
        Assert.Equal("Unknown", result.PreviousState);
        Assert.Equal(targetState.ToString(), result.NewState);

        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockOrderRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TransitionOrderState_WhenInvalidTransition_ShouldReturnErrorResponseWithValidStates()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var order = Order.Create("TEST-REF-002");
        var invalidTargetState = OrderState.Completed; // Cannot go from Initial to Completed directly

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.TransitionOrderState(orderId, invalidTargetState, "Invalid transition test");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(OrderState.Initial.ToString(), result.PreviousState);
        Assert.Equal(invalidTargetState.ToString(), result.NewState);
        Assert.NotNull(result.ErrorDetails);
        Assert.Contains("Pending", result.ErrorDetails.ValidNextStates);

        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockOrderRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var order = Order.Create("TEST-REF-003");
        var request = new TransitionOrderStateRequest(
            OrderId: orderId,
            OrderState: OrderState.Pending,
            Reason: "ExecuteAsync test",
            EnforceBusinessRules: true
        );

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mockOrderRepository.Setup(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOrderRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(OrderState.Initial.ToString(), result.PreviousState);
        Assert.Equal(OrderState.Pending.ToString(), result.NewState);
        Assert.Equal("ExecuteAsync test", result.Reason);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvalidRequest_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidRequest = new TransitionOrderStateRequest(
            OrderId: Guid.Empty, // Invalid empty GUID
            OrderState: OrderState.Pending,
            Reason: "Invalid request test"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(invalidRequest));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _useCase.ExecuteAsync(null!));
    }

    [Fact]
    public async Task GetValidNextStatesAsync_WhenOrderExists_ShouldReturnValidStatesResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var order = Order.Create("TEST-REF-004");

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.GetValidNextStatesAsync(orderId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(OrderState.Initial.ToString(), result.CurrentState);
        Assert.Contains("Pending", result.ValidNextStates);
        Assert.Single(result.ValidNextStates); // Only Pending is valid from Initial
    }

    [Fact]
    public async Task GetValidNextStatesAsync_WhenOrderNotFound_ShouldReturnErrorResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _useCase.GetValidNextStatesAsync(orderId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal("Unknown", result.CurrentState);
        Assert.Empty(result.ValidNextStates);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Theory]
    [InlineData(OrderState.Initial, OrderState.Pending, true)]
    [InlineData(OrderState.Pending, OrderState.Paid, true)]
    [InlineData(OrderState.Pending, OrderState.Cancelled, true)]
    [InlineData(OrderState.Initial, OrderState.Completed, false)]
    [InlineData(OrderState.Cancelled, OrderState.Paid, false)]
    public async Task TransitionOrderState_VariousStateTransitions_ShouldFollowStateMachine(
        OrderState initialState,
        OrderState targetState,
        bool shouldSucceed)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var order = Order.Create("TEST-REF-THEORY");

        // Set up the order to the desired initial state
        if (initialState != OrderState.Initial)
        {
            // Use reflection to set the internal OrderState for testing
            var orderStateProperty = typeof(Order).GetProperty("OrderState");
            orderStateProperty?.SetValue(order, initialState);
        }

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mockOrderRepository.Setup(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOrderRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.TransitionOrderState(orderId, targetState, $"Test {initialState} to {targetState}");

        // Assert
        if (shouldSucceed)
        {
            Assert.True(result.Success);
            Assert.Equal(targetState.ToString(), result.NewState);
        }
        else
        {
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorDetails);
            Assert.NotEmpty(result.ErrorDetails.ValidNextStates);
        }
    }
}
