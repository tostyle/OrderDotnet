using Application.DTOs;
using Application.UseCases;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Tests.Application.UseCases;

/// <summary>
/// Unit tests for InitialOrderUseCase - 5th Iteration
/// </summary>
public class InitialOrderUseCaseTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IOrderPaymentRepository> _paymentRepoMock;
    private readonly Mock<IOrderItemRepository> _orderItemRepoMock;
    private readonly Mock<IOrderWorkflowService> _workflowServiceMock;
    private readonly InitialOrderUseCase _useCase;

    public InitialOrderUseCaseTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _paymentRepoMock = new Mock<IOrderPaymentRepository>();
        _orderItemRepoMock = new Mock<IOrderItemRepository>();
        _workflowServiceMock = new Mock<IOrderWorkflowService>();

        _useCase = new InitialOrderUseCase(
            _orderRepoMock.Object,
            _paymentRepoMock.Object,
            _orderItemRepoMock.Object,
            _workflowServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenReferenceIdIsEmpty()
    {
        // Arrange
        var request = new InitialOrderRequest(
            ReferenceId: "",
            OrderItems: new[] { new InitialOrderItemRequest(Guid.NewGuid(), 1, 10.0m, 11.0m) },
            PaymentMethod: "CreditCard"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenOrderItemsIsEmpty()
    {
        // Arrange
        var request = new InitialOrderRequest(
            ReferenceId: "ref-001",
            OrderItems: Array.Empty<InitialOrderItemRequest>(),
            PaymentMethod: "CreditCard"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExistingOrder_WhenReferenceIdExists()
    {
        // Arrange
        var referenceId = "ref-001";
        var orderId = OrderId.New();
        var paymentId = PaymentId.New();

        var order = Order.Create(referenceId);
        typeof(Order).GetProperty("Id")!.SetValue(order, orderId);

        var payment = OrderPayment.Create(orderId, PaymentMethod.CreditCard("1234", "Visa"), 100, "THB");
        typeof(OrderPayment).GetProperty("Id")!.SetValue(payment, paymentId);

        var request = new InitialOrderRequest(
            ReferenceId: referenceId,
            OrderItems: new[] { new InitialOrderItemRequest(Guid.NewGuid(), 1, 10.0m, 11.0m) },
            PaymentMethod: "CreditCard"
        );

        _orderRepoMock.Setup(r => r.GetByReferenceIdAsync(referenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _paymentRepoMock.Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderPayment> { payment });

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.Equal(orderId.Value, result.OrderId);
        Assert.Equal(referenceId, result.ReferenceId);
        Assert.Equal(paymentId.Value, result.PaymentId);
        Assert.Equal(payment.Status.ToString(), result.PaymentStatus);
    }

    [Fact]
    public async Task ExecuteAsync_CreatesNewOrderWithItemsAndWorkflow_WhenReferenceIdDoesNotExist()
    {
        // Arrange
        var referenceId = "ref-002";
        var workflowId = "workflow-123";
        var productId = Guid.NewGuid();

        var request = new InitialOrderRequest(
            ReferenceId: referenceId,
            OrderItems: new[] {
                new InitialOrderItemRequest(productId, 2, 50.0m, 55.0m, "THB")
            },
            PaymentMethod: "CreditCard"
        );

        _orderRepoMock.Setup(r => r.GetByReferenceIdAsync(referenceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _orderRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _orderRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _orderItemRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _orderItemRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderPayment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _paymentRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _workflowServiceMock.Setup(w => w.StartOrderProcessingWorkflowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowId);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.Equal(referenceId, result.ReferenceId);
        Assert.Equal("Pending", result.PaymentStatus);

        // Verify workflow was started
        _workflowServiceMock.Verify(w => w.StartOrderProcessingWorkflowAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

        // Verify order items were created
        _orderItemRepoMock.Verify(r => r.AddAsync(It.IsAny<OrderItem>(), It.IsAny<CancellationToken>()), Times.Once);

        // Verify order was updated with workflow ID
        _orderRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
