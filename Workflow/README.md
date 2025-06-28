# Temporal Workflow Service - Order Processing

This project implements a Temporal.io worker service for order processing workflows using .NET 9.

## Overview

The Temporal Workflow Service orchestrates complex order processing operations through a series of activities. It provides reliable, scalable, and fault-tolerant workflow execution for the order management system.

## Architecture

### Components

- **OrderProcessingWorkflow**: Main workflow that orchestrates order processing
- **OrderActivities**: Collection of 7 activities that handle specific order operations
- **Program.cs**: Temporal worker host configuration and startup

### Workflow Structure

```
OrderProcessingWorkflow (receives orderId)
├── 1. StartOrderWorkflow (workflowId, orderId)
├── 2. ReserveStock (orderId)
├── 3. BurnLoyaltyTransaction (orderId)
├── 4. EarnLoyaltyTransaction (orderId)
├── 5. ProcessPayment (orderId)
├── 6. CompletedCart (orderId)
└── 7. GetOrderDetail (orderId)
```

## Implementation Details

### Workflows

#### OrderProcessingWorkflow
- **Location**: `Workflows/OrderProcessingWorkflow.cs`
- **Purpose**: Main workflow that receives an `orderId` and orchestrates all order processing steps
- **Input**: `Guid orderId`
- **Output**: `string` (success/failure message with details)
- **Timeout**: 5 minutes per activity
- **Error Handling**: Comprehensive error handling with detailed failure messages

### Activities

#### OrderActivities
- **Location**: `Activities/OrderActivities.cs`
- **Purpose**: Contains all 7 order processing activities
- **Dependencies**: `ILogger<OrderActivities>` (injected via DI)
- **Status**: All activities are TODO placeholders ready for implementation

| Activity | Purpose | Input | Output | Status |
|----------|---------|-------|--------|---------|
| `StartOrderWorkflowAsync` | Initialize workflow | workflowId, orderId | bool | TODO |
| `ReserveStockAsync` | Reserve inventory | orderId | bool | TODO |
| `BurnLoyaltyTransactionAsync` | Deduct loyalty points | orderId | bool | TODO |
| `EarnLoyaltyTransactionAsync` | Award loyalty points | orderId | bool | TODO |
| `ProcessPaymentAsync` | Handle payment | orderId | bool | TODO |
| `CompletedCartAsync` | Finalize cart | orderId | bool | TODO |
| `GetOrderDetailAsync` | Retrieve order info | orderId | string | TODO |

## Configuration

### Temporal Server Connection
- **Host**: `localhost:7233`
- **Namespace**: `default`
- **Task Queue**: `order-processing`

### Dependencies
- `Temporalio.Extensions.Hosting` v1.7.0
- `Microsoft.Extensions.Hosting` v9.0.6

## Getting Started

### Prerequisites
1. .NET 9 SDK
2. Temporal server running on localhost:7233

### Start Temporal Server (Development)
```bash
temporal server start-dev
```

### Run the Worker
```bash
cd Workflow
dotnet run
```

### Expected Output
```
=== Temporal Worker for Order Processing ===
Temporal Server: localhost:7233
Namespace: default
Task Queue: order-processing
Registered Workflow: OrderProcessingWorkflow
Registered Activities: 7 activities from OrderActivities
  1. StartOrderWorkflowAsync
  2. ReserveStockAsync
  3. BurnLoyaltyTransactionAsync
  4. EarnLoyaltyTransactionAsync
  5. ProcessPaymentAsync
  6. CompletedCartAsync
  7. GetOrderDetailAsync
Worker Status: Ready to process workflows
Press Ctrl+C to shutdown
============================================
```

## Testing

### Start a Workflow Execution
```bash
temporal workflow start \
  --type OrderProcessingWorkflow \
  --task-queue order-processing \
  --input '"123e4567-e89b-12d3-a456-426614174000"'
```

### Query Workflow Status
```bash
temporal workflow show --workflow-id <workflow-id>
```

### List Running Workflows
```bash
temporal workflow list
```

## Development

### Current Status
- ✅ Temporal worker configuration complete
- ✅ Workflow definition implemented
- ✅ All 7 activities defined and registered
- ✅ Dependency injection setup
- ✅ Logging configuration
- ⏳ Activity implementations (marked as TODO)

### Next Steps
1. Implement actual logic in each activity
2. Add integration with Domain/Application layers
3. Add comprehensive error handling
4. Add activity retry policies
5. Add workflow testing
6. Add metrics and monitoring

### File Structure
```
Workflow/
├── Program.cs                    # Temporal worker host
├── Activities/
│   └── OrderActivities.cs       # 7 TODO activity implementations
├── Workflows/
│   └── OrderProcessingWorkflow.cs # Main workflow orchestration
├── Workflow.csproj              # Project dependencies
└── README.md                    # This documentation
```

## Integration

This Temporal worker integrates with:
- **Domain Layer**: Order entities and value objects
- **Application Layer**: Order services and DTOs
- **Infrastructure Layer**: Repositories and external services

## Monitoring

The worker provides structured logging for:
- Workflow execution start/completion
- Activity execution progress
- Error conditions and failures
- Performance metrics

## Error Handling

- **Activity Failures**: Individual activities can fail and be retried
- **Workflow Failures**: Complete workflow failure with detailed error messages
- **Timeout Handling**: 5-minute timeout per activity with configurable retry policies
- **Logging**: Comprehensive error logging for debugging

## Deployment

For production deployment:
1. Configure Temporal Cloud or self-hosted Temporal cluster
2. Update connection settings in `Program.cs`
3. Configure appropriate retry and timeout policies
4. Set up monitoring and alerting
5. Deploy as containerized service or hosted service