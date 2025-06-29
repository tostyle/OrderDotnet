# Navigation Properties Test Documentation

## Overview
This document demonstrates the bidirectional navigation properties between Order entity and related entities.

## Bidirectional Relationships Implemented

### 1. Order ↔ OrderLoyalty (1-to-many)
- **Order.LoyaltyTransactions**: `ICollection<OrderLoyalty>`
- **OrderLoyalty.Order**: `Order` (back reference)

### 2. Order ↔ OrderStock (1-to-many)
- **Order.StockReservations**: `ICollection<OrderStock>`
- **OrderStock.Order**: `Order` (back reference)

### 3. Order ↔ OrderPayment (1-to-1)
- **Order.Payment**: `OrderPayment?`
- **OrderPayment.Order**: `Order` (back reference)

## Usage Examples

### Loading Order with Related Entities
```csharp
// Using the new repository methods
var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(orderId);

// Access related entities from Order
var loyaltyTransactions = orderWithDetails.LoyaltyTransactions;
var stockReservations = orderWithDetails.StockReservations;
var payment = orderWithDetails.Payment;

// Access parent Order from related entities
foreach (var loyalty in loyaltyTransactions)
{
    var parentOrder = loyalty.Order; // Back reference to Order
}

foreach (var stock in stockReservations)
{
    var parentOrder = stock.Order; // Back reference to Order
}

if (payment != null)
{
    var parentOrder = payment.Order; // Back reference to Order
}
```

### API Endpoints
- `GET /api/orders/{orderId}/details` - Returns detailed order with all related entities
- `GET /api/orders/reference/{referenceId}/details` - Returns detailed order by reference ID

### Benefits of Bidirectional Navigation Properties
1. **Easier Navigation**: Can navigate from child entities back to parent Order
2. **Cleaner Code**: Reduces need for separate repository calls
3. **Performance**: Entity Framework can optimize queries with proper relationships
4. **Type Safety**: Compile-time checking of relationships
5. **IntelliSense Support**: Better development experience with auto-completion

## Entity Framework Configuration
The relationships are configured in `OrderConfiguration.cs`:

```csharp
// One-to-many relationship with OrderLoyalty
builder.HasMany(o => o.LoyaltyTransactions)
    .WithOne(l => l.Order)  // <-- Back reference
    .HasForeignKey(l => l.OrderId)
    .OnDelete(DeleteBehavior.Cascade);
```

## Database Schema
No changes to the database schema are required. The navigation properties are purely for object-relational mapping and don't affect the physical database structure.
