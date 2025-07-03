using Application.DTOs;
using Application.UseCases;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using Domain.Aggregates;
using Moq;
using Xunit;

namespace Tests.Application.UseCases;

/// <summary>
/// Unit tests for ChangeOrderStatusToPendingUseCase
/// Tests the 8th iteration functionality for changing order status to Pending
/// </summary>
public class ChangeOrderStatusToPendingUseCaseTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IOrderJourneyRepository> _mockOrderJourneyRepository;
    private readonly Mock<IOrderLogRepository> _mockOrderLogRepository;
    private readonly Mock<IOrderWorkflowService> _mockWorkflowService;
    private readonly ChangeOrderStatusToPendingUseCase _useCase;

    public ChangeOrderStatusToPendingUseCaseTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockOrderJourneyRepository = new Mock<IOrderJourneyRepository>();
        _mockOrderLogRepository = new Mock<IOrderLogRepository>();
        _mockWorkflowService = new Mock<IOrderWorkflowService>();

        _useCase = new ChangeOrderStatusToPendingUseCase(
            _mockOrderRepository.Object,
            _mockOrderJourneyRepository.Object,
            _mockOrderLogRepository.Object,
            _mockWorkflowService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldChangeStatusToPending()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var order = Order.Create("test-ref-001");

        // Use OrderAggregate to transition from Initial to Cancelled state
        var orderAggregate = OrderAggregate.FromExistingOrder(order);
        orderAggregate.TransitionOrderState(OrderState.Pending);   // Initial -> Pending
        orderAggregate.TransitionOrderState(OrderState.Cancelled); // Pending -> Cancelled

        var request = new ChangeOrderStatusToPendingRequest
        {
            OrderId = orderId,
            Reason = "Test reason",
            InitiatedBy = "test-user"
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mockOrderRepository.Setup(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOrderRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockOrderJourneyRepository.Setup(x => x.AddAsync(It.IsAny<OrderJourney>()))
            .Returns(Task.CompletedTask);
        _mockOrderJourneyRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockOrderLogRepository.Setup(x => x.AddAsync(It.IsAny<OrderLog>()))
            .Returns(Task.CompletedTask);
        _mockOrderLogRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockWorkflowService.Setup(x => x.ResetWorkflowToPendingStateAsync(orderId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(OrderState.Cancelled, result.PreviousState);
        Assert.Equal(OrderState.Pending, result.NewState);
        Assert.False(result.IsAlreadyPending);
        Assert.Equal("Order status successfully changed to Pending", result.Message);

        // Verify order state was changed
        Assert.Equal(OrderState.Pending, order.OrderState);

        // Verify repositories were called
        _mockOrderRepository.Verify(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderRepository.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderJourneyRepository.Verify(x => x.AddAsync(It.IsAny<OrderJourney>()), Times.Once);
        _mockOrderJourneyRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderLogRepository.Verify(x => x.AddAsync(It.IsAny<OrderLog>()), Times.Once);
        _mockOrderLogRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockWorkflowService.Verify(x => x.ResetWorkflowToPendingStateAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrderAlreadyPending_ShouldReturnIdempotentResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var order = Order.Create("test-ref-002");

        // Use OrderAggregate to transition from Initial to Pending state
        var orderAggregate = OrderAggregate.FromExistingOrder(order);
        orderAggregate.TransitionOrderState(OrderState.Pending); // Initial -> Pending

        var request = new ChangeOrderStatusToPendingRequest
        {
            OrderId = orderId,
            Reason = "Test reason"
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(OrderState.Pending, result.PreviousState);
        Assert.Equal(OrderState.Pending, result.NewState);
        Assert.True(result.IsAlreadyPending);
        Assert.Equal("Order is already in Pending state", result.Message);

        // Verify no state change operations were called
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockOrderRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockOrderJourneyRepository.Verify(x => x.AddAsync(It.IsAny<OrderJourney>()), Times.Never);
        _mockOrderLogRepository.Verify(x => x.AddAsync(It.IsAny<OrderLog>()), Times.Never);
        _mockWorkflowService.Verify(x => x.ResetWorkflowToPendingStateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentOrder_ShouldThrowException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);

        var request = new ChangeOrderStatusToPendingRequest
        {
            OrderId = orderId,
            Reason = "Test reason"
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(request));

        Assert.Equal($"Order with ID {orderId} not found", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _useCase.ExecuteAsync(null!));
    }

    [Fact]
    public async Task ExecuteAsync_WithNonCancelledOrder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var order = Order.Create("test-ref-005");

        // Use OrderAggregate to transition from Initial to Paid state (not Cancelled)
        var orderAggregate = OrderAggregate.FromExistingOrder(order);
        orderAggregate.TransitionOrderState(OrderState.Pending); // Initial -> Pending
        orderAggregate.TransitionOrderState(OrderState.Paid);    // Pending -> Paid

        var request = new ChangeOrderStatusToPendingRequest
        {
            OrderId = orderId,
            Reason = "Test invalid state"
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(request));

        Assert.Equal("Only Cancelled orders can be changed to Pending status. Current order state is Paid", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWorkflowResetFails_ShouldStillCompleteOperation()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderIdValue = OrderId.From(orderId);
        var order = Order.Create("test-ref-004");

        // Use OrderAggregate to transition from Initial to Cancelled state
        var orderAggregate = OrderAggregate.FromExistingOrder(order);
        orderAggregate.TransitionOrderState(OrderState.Pending);   // Initial -> Pending
        orderAggregate.TransitionOrderState(OrderState.Cancelled); // Pending -> Cancelled

        var request = new ChangeOrderStatusToPendingRequest
        {
            OrderId = orderId,
            Reason = "Test workflow failure"
        };

        _mockOrderRepository.Setup(x => x.GetByIdAsync(orderIdValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mockOrderRepository.Setup(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOrderRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockOrderJourneyRepository.Setup(x => x.AddAsync(It.IsAny<OrderJourney>()))
            .Returns(Task.CompletedTask);
        _mockOrderJourneyRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockOrderLogRepository.Setup(x => x.AddAsync(It.IsAny<OrderLog>()))
            .Returns(Task.CompletedTask);
        _mockOrderLogRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Simulate workflow service failure
        _mockWorkflowService.Setup(x => x.ResetWorkflowToPendingStateAsync(orderId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Workflow reset failed"));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(OrderState.Cancelled, result.PreviousState);
        Assert.Equal(OrderState.Pending, result.NewState);
        Assert.False(result.IsAlreadyPending);

        // Verify order state was still changed despite workflow failure
        Assert.Equal(OrderState.Pending, order.OrderState);
    }
}
