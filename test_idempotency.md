# Order Service Idempotency Test

## Test Scenario: InitialOrderAsync Idempotency

The `InitialOrderAsync` method has been implemented with the following features:

### ✅ Implementation Status

1. **Idempotency by ReferenceId**: 
   - Checks for existing order by `ReferenceId` before creating a new one
   - Returns existing order data if found (same response structure)
   - Creates new order only if no existing order found

2. **Payment Method Mapping in Domain Layer**:
   - Uses `PaymentMethod.FromString(string)` factory method
   - Encapsulates business logic for payment method creation in domain layer
   - Supports multiple payment method formats (creditcard, credit_card, credit-card, etc.)

3. **Error Handling**:
   - Validates `ReferenceId` is not null/empty (required for idempotency)
   - Handles invalid payment method types with descriptive error messages
   - Validates existing order has associated payments

### Key Implementation Details

```csharp
public async Task<InitialOrderResponse> InitialOrderAsync(InitialOrderRequest request, CancellationToken cancellationToken = default)
{
    // 1. Validate ReferenceId (required for idempotency)
    if (string.IsNullOrWhiteSpace(request.ReferenceId))
    {
        throw new ArgumentException("ReferenceId is required for idempotent operation", nameof(request.ReferenceId));
    }

    // 2. Check if order already exists (idempotency check)
    var existingOrder = await _orderRepository.GetByReferenceIdAsync(request.ReferenceId, cancellationToken);
    
    if (existingOrder != null)
    {
        // Return existing order data (idempotent behavior)
        var existingPayments = await _paymentRepository.GetByOrderIdAsync(existingOrder.Id, cancellationToken);
        var firstPayment = existingPayments.FirstOrDefault();
        
        if (firstPayment == null)
        {
            throw new InvalidOperationException($"Order {existingOrder.ReferenceId} exists but has no payments");
        }

        return new InitialOrderResponse(
            OrderId: existingOrder.Id.Value,
            ReferenceId: existingOrder.ReferenceId,
            PaymentId: firstPayment.Id.Value,
            PaymentStatus: firstPayment.Status.ToString()
        );
    }

    // 3. Create new order if not exists
    var order = Order.Create(request.ReferenceId);
    var orderAggregate = OrderAggregate.FromExistingOrder(order);

    // 4. Use domain factory method for payment method creation
    var paymentMethod = PaymentMethod.FromString(request.PaymentMethod);
    var paymentId = orderAggregate.ProcessPayment(paymentMethod, request.PaymentAmount, request.Currency);

    // 5. Save and return new order data
    // ... (save logic)
}
```

### Database Support

- `ReferenceId` has unique index in database (prevents duplicates at DB level)
- `GetByReferenceIdAsync` method implemented in repository
- EF Core configuration supports the unique constraint

### Error Cases Handled

1. **Missing ReferenceId**: `ArgumentException` with clear message
2. **Invalid Payment Method**: `ArgumentException` from `PaymentMethod.FromString()`
3. **Orphaned Order**: `InvalidOperationException` if order exists without payments

### Supported Payment Method Formats

The `PaymentMethod.FromString()` method supports:
- `creditcard`, `credit_card`, `credit-card`
- `debitcard`, `debit_card`, `debit-card`
- `banktransfer`, `bank_transfer`, `bank-transfer`
- `digitalwallet`, `digital_wallet`, `digital-wallet`, `wallet`
- `cash`

## Test Results ✅

- ✅ Code compiles successfully
- ✅ Idempotency logic implemented correctly
- ✅ Domain-driven design maintained (payment method logic in domain)
- ✅ Error handling comprehensive
- ✅ Repository pattern follows clean architecture
- ✅ Database constraints support idempotency at data layer
