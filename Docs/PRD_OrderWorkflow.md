# Requirements
- start with one main workflow with receive orderId
- prepare temporal activity like this with and leave it as TODO first not yet do anything
1. StartOrderWorkflow with receive temporal workflowId and orderId first 
2. ReserveStock
3. BurnLoyaltyTransaction
4. EarnLoyaltyTransaction
5. ProcessPayment
6. CompletedCart
7. GetOrderDetail
- prepare temporal workflow as OrderProcessingWorkflow
- let implement it under Workflow project