using Application.DTOs;
using Application.UseCases;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Tests.Application.UseCases;

/// <summary>
/// Unit tests for ReserveStockUseCase - 6th Iteration
/// </summary>
public class ReserveStockUseCaseTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IOrderItemRepository> _orderItemRepoMock;
    private readonly Mock<IOrderStockRepository> _stockRepoMock;
    private readonly ReserveStockUseCase _useCase;

    public ReserveStockUseCaseTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _orderItemRepoMock = new Mock<IOrderItemRepository>();
        _stockRepoMock = new Mock<IOrderStockRepository>();

        _useCase = new ReserveStockUseCase(
            _orderRepoMock.Object,
            _orderItemRepoMock.Object,
            _stockRepoMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenOrderIdIsEmpty()
    {
        // Arrange
        var request = new ReserveStockRequest(
            OrderId: Guid.Empty,
            ProductId: Guid.NewGuid()
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentException_WhenProductIdIsEmpty()
    {
        // Arrange
        var request = new ReserveStockRequest(
            OrderId: Guid.NewGuid(),
            ProductId: Guid.Empty
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsInvalidOperationException_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new ReserveStockRequest(orderId, productId);

        _orderRepoMock.Setup(r => r.GetByIdAsync(OrderId.From(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExistingReservation_WhenStockAlreadyReserved()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new ReserveStockRequest(orderId, productId);

        var order = Order.Create("ref-001");
        typeof(Order).GetProperty("Id")!.SetValue(order, OrderId.From(orderId));

        var existingReservation = OrderStock.Create(OrderId.From(orderId), ProductId.From(productId), 5);

        _orderRepoMock.Setup(r => r.GetByIdAsync(OrderId.From(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _stockRepoMock.Setup(r => r.GetByOrderIdAndProductIdAsync(
                OrderId.From(orderId),
                ProductId.From(productId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReservation);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsAlreadyReserved);
        Assert.Equal(existingReservation.Id.Value, result.ReservationId);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal(5, result.Quantity);
        Assert.Equal("Reserved", result.Status);

        // Verify that no new reservation was created
        _stockRepoMock.Verify(r => r.AddAsync(It.IsAny<OrderStock>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsInvalidOperationException_WhenOrderItemNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new ReserveStockRequest(orderId, productId);

        var order = Order.Create("ref-001");
        typeof(Order).GetProperty("Id")!.SetValue(order, OrderId.From(orderId));

        _orderRepoMock.Setup(r => r.GetByIdAsync(OrderId.From(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _stockRepoMock.Setup(r => r.GetByOrderIdAndProductIdAsync(
                OrderId.From(orderId),
                ProductId.From(productId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderStock?)null);

        _orderItemRepoMock.Setup(r => r.GetByOrderIdAsync(OrderId.From(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderItem>()); // Empty list - no order items

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_CreatesNewReservation_WhenValidRequestAndNoExistingReservation()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new ReserveStockRequest(orderId, productId);

        var order = Order.Create("ref-001");
        typeof(Order).GetProperty("Id")!.SetValue(order, OrderId.From(orderId));

        var orderItem = OrderItem.Create(
            OrderId.From(orderId),
            ProductId.From(productId),
            quantity: 3,
            netAmount: 100m,
            grossAmount: 110m,
            currency: "THB"
        );

        _orderRepoMock.Setup(r => r.GetByIdAsync(OrderId.From(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _stockRepoMock.Setup(r => r.GetByOrderIdAndProductIdAsync(
                OrderId.From(orderId),
                ProductId.From(productId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderStock?)null);

        _orderItemRepoMock.Setup(r => r.GetByOrderIdAsync(OrderId.From(orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderItem> { orderItem });

        _stockRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderStock>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _stockRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.False(result.IsAlreadyReserved);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal(3, result.Quantity); // Quantity from OrderItem
        Assert.Equal("Reserved", result.Status);

        // Verify that new reservation was created and saved
        _stockRepoMock.Verify(r => r.AddAsync(It.IsAny<OrderStock>(), It.IsAny<CancellationToken>()), Times.Once);
        _stockRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
