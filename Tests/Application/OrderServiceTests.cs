using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using Moq;
using Xunit;

namespace Tests.Application
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock = new();
        private readonly Mock<IOrderPaymentRepository> _paymentRepoMock = new();
        private readonly Mock<IOrderLoyaltyRepository> _loyaltyRepoMock = new();
        private readonly Mock<IOrderStockRepository> _stockRepoMock = new();
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _service = new OrderService(
                _orderRepoMock.Object,
                _paymentRepoMock.Object,
                _loyaltyRepoMock.Object,
                _stockRepoMock.Object
            );
        }

        [Fact]
        public async Task InitialOrderAsync_ReturnsExistingOrder_WhenReferenceIdExists()
        {
            // Arrange
            var referenceId = "ref-123";
            var orderId = OrderId.New();
            var paymentId = PaymentId.New();
            var order = Order.Create(referenceId);
            typeof(Order).GetProperty("Id")!.SetValue(order, orderId);
            var payment = OrderPayment.Create(orderId, PaymentMethod.Cash(), 100, "USD");
            typeof(OrderPayment).GetProperty("Id")!.SetValue(payment, paymentId);
            _orderRepoMock.Setup(r => r.GetByReferenceIdAsync(referenceId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
            _paymentRepoMock.Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<OrderPayment> { payment });

            // Act
            var result = await _service.InitialOrderAsync(new InitialOrderRequest(referenceId, "cash", 100, "USD"));

            // Assert
            Assert.Equal(orderId.Value, result.OrderId);
            Assert.Equal(referenceId, result.ReferenceId);
            Assert.Equal(paymentId.Value, result.PaymentId);
            Assert.Equal(payment.Status.ToString(), result.PaymentStatus);
        }

        [Fact]
        public async Task InitialOrderAsync_CreatesNewOrder_WhenReferenceIdDoesNotExist()
        {
            // Arrange
            var referenceId = "ref-456";
            _orderRepoMock.Setup(r => r.GetByReferenceIdAsync(referenceId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
            _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _orderRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderPayment>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _paymentRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _service.InitialOrderAsync(new InitialOrderRequest(referenceId, "cash", 100, "USD"));

            // Assert
            Assert.Equal(referenceId, result.ReferenceId);
            Assert.Equal("Pending", result.PaymentStatus);
        }

        [Fact]
        public async Task InitialOrderAsync_ThrowsArgumentException_WhenReferenceIdIsMissing()
        {
            // Arrange
            var request = new InitialOrderRequest(null, "cash", 100, "USD");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.InitialOrderAsync(request));
        }

        [Fact]
        public async Task InitialOrderAsync_ThrowsArgumentException_WhenPaymentMethodIsInvalid()
        {
            // Arrange
            var request = new InitialOrderRequest("ref-789", "invalid_method", 100, "USD");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.InitialOrderAsync(request));
        }
    }
}
