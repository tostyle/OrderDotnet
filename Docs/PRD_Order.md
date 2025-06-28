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