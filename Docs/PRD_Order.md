# Requirements
this service is rest api service to handle order management system

## Components
- Api, Application, Domain, Infrastructure is same pattern as clean architecture
- Worker will handle temporal workflow


### Order Modules
basically for POC project 
will has single order model
Order
- OrderId
- OrderState 
    - PENDING
    - PAID
    - NEW
    - PACKED
    - DELIVERED
    - CANCELED
    - REFUND
- Use State design pattern to handle order state

---
# 2nd iterate
create models
1. OrderLoyalty - to record loyalty program - earn/burn point
2. OrderPayment - record payment - payment method, paid date
3. OrderStock - record reservation
create aggregate
- read Domain/Aggregates directory that created and make parital class of Order Aggregate
- has primary function in OrderAggregate class about check orderTranstion (look at Domain/Entites/OrderState class for more info)
- try to fill in function in each Aggregate class u can leave it todo if u want of can fill u think nessesary
- focus on domain layer first dont need to implement other layer for now
---
# 3rd iterate
implement repository layer
1. Order - add field ReferenceId make index of it
2. Order Repository implement CRUD and findByWorkflowId, findByReferenceId
3. for other model let implement CRUD first
implement infra layer for entity framework
- implement and update model
implement application layer
- Create OrderService class in class will has methods
1. InitialOrder - create DTO, init Order, OrderPayment with Payment Pending status and save it to each repo model
2. ReserveStock - create DTO, find Order By Id and then call revervestock method in OrderAggregate save to repository
3. BurnLoyaly, EarnLoyalty make method calculate and save to repo
4. Proces Payment update OrderPayment to Status Completed
---
# 4th iterate
implement OrderItem models
fields
- productId, quantity, netAmount, grossAmount, currency = THB
- Order 1 to many to OrderItem
- implement IRepository
- Get Order Detail need to Include OrderItem too
- implement service
--- 
# 5th Iterate
## Spec of InitialOrderAsync
- refactor InitialOrderAsync in order service
- change DTO like this 
    - referenceId
    - orderItems [{productId, quantity, netAmount, grossAmount, currency}]
    - paymentMethod
- flows
1. create order first by refernce Id
2. create order items
3. create order payment with payment method and set status to pending
4. create temporal workflow
---
# 6th Iterate
## Spec of ReserveStock Usecase
- create new ReserveStock usecase
- it not an endpoint do not create new endpoint in controller
- input will be { orderId, productId } 
- query Order Item by productId and then insert OrderStock record from orderItem Info
- make it idempotence - check if Order
- set ReservationStatus = 'Reserved'
- for idempotence like reserve stock 
I want to check if stock already reserved then system should not reserved again I will give Pseudo code idea
```c#
if (existingReservation is not null)
    {
        // Already reserved, return existing record
        return existingReservation.ToResult(isAlreadyReserved: true);
    }
```

---
# 7th Iterate
## OrderJourneys and OrderLogs modules
- create OrderJourneys model - that record state transition container old state, new state, transition date 
    - relation order 1 to many OrderJourneys
    - add compund index to OrderId + new state and OrderId + old state
- create OrderLogs Model - record any order Log action
    - relation order 1 to many OrderLogs
- implement IRepository
- add relation to entity framework
- that enough not yet do any at application layer
---
# 8th Iterate
## Implement Change Order Status To Pending usecase
- implment new usecase call ChangeOrderStatusToPending (by orderId)
- basiccally change order model and change status = Pending
- add new method WorflowService that can reset workflow and find ActivityType = TransitionToPendingState -> reset temporatal workflow to this state
- and then add to usecase 
