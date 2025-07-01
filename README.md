# OrderDotnet
OrderDotnet is a .NET-based order management system designed to handle the complete lifecycle of an order, from creation to completion. It follows clean architecture principles, utilizing a domain-driven design (DDD) approach to model the order process. The system is built with a focus on scalability, maintainability, and testability, making it suitable for a wide range of e-commerce and order processing applications.

## Table of Contents
- [Features](#features)
- [Architecture](#architecture)
- [Technologies](#technologies)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Workflow](#workflow)
- [Database](#database)
- [Contributing](#contributing)

## Features
- **Order Management:** Create, update, and manage orders with a rich domain model.
- **State Management:** Utilizes a state machine to manage the order lifecycle (e.g., `PENDING`, `PAID`, `SHIPPED`, `DELIVERED`, `CANCELLED`).
- **Payment Processing:** Supports multiple payment methods and tracks payment status.
- **Loyalty Program:** Manages earning and burning of loyalty points.
- **Stock Reservation:** Handles stock reservation and tracking.
- **Workflow Automation:** Uses Temporal for orchestrating complex order workflows.
- **Clean Architecture:** Follows a clean architecture pattern for separation of concerns.
- **REST API:** Provides a comprehensive REST API for interacting with the system.

## Architecture
The project is structured based on the principles of Clean Architecture, which promotes a separation of concerns and a dependency rule that points inwards, from the outer layers to the inner layers. The main layers are:
- **Domain:** Contains the core business logic, entities, value objects, and aggregates. This layer is independent of any other layer.
- **Application:** Contains the application-specific business logic, use cases, and services. It orchestrates the domain objects to perform tasks.
- **Infrastructure:** Contains the implementation details, such as databases, external services, and UI frameworks. It depends on the Application and Domain layers.
- **Api:** The presentation layer, which exposes the application's functionality as a REST API.
- **Workflow:** The workflow layer, which uses Temporal to orchestrate the order processing workflow.

## Technologies
- **Backend:** .NET 8, ASP.NET Core
- **Database:** PostgreSQL
- **Workflow Engine:** Temporal
- **ORM:** Entity Framework Core
- **Architecture:** Clean Architecture, Domain-Driven Design (DDD)
- **Testing:** xUnit, Moq

## Getting Started
### Prerequisites
- .NET 8 SDK
- Docker
- Docker Compose
- Temporal CLI

### Installation
1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/OrderDotnet.git
   cd OrderDotnet
   ```
2. **Start the database and Temporal server:**
   ```bash
   docker-compose up -d
   ```
3. **Run the database migrations:**
   ```bash
   dotnet run --project DatabaseMigration
   ```

### Running the Application
1. **Start the API:**
   ```bash
   dotnet run --project Api
   ```
2. **Start the Workflow worker:**
   ```bash
   dotnet run --project Workflow
   ```
The API will be available at `http://localhost:5000`.

## API Endpoints
The following are the main API endpoints:
- `POST /api/orders`: Create a new order.
- `GET /api/orders/{id}`: Get the details of an order.
- `POST /api/orders/{id}/payment`: Process a payment for an order.
- `POST /api/orders/{id}/cancel`: Cancel an order.

For a full list of endpoints, please refer to the `Api/Controllers/OrdersController.cs` file or run the application and access the Swagger UI at `http://localhost:5000/swagger`.

## Workflow
The order processing workflow is managed by Temporal. The main workflow is `OrderProcessingWorkflow`, which orchestrates the following activities:
- **ReserveStock:** Reserves the stock for the order.
- **BurnLoyaltyTransaction:** Burns loyalty points for the order.
- **ProcessPayment:** Processes the payment for the order.
- **EarnLoyaltyTransaction:** Earns loyalty points for the order.
- **CompleteOrder:** Completes the order.

## Database
The application uses a PostgreSQL database to store the order data. The database schema is managed using Entity Framework Core migrations. The main tables are:
- **Orders:** Stores the main order information.
- **OrderItems:** Stores the items in an order.
- **OrderPayments:** Stores the payment information for an order.
- **OrderLoyalties:** Stores the loyalty program transactions for an order.
- **OrderStocks:** Stores the stock reservation information for an order.
- **OrderJourneys:** Stores the state transitions for an order.
- **OrderLogs:** Stores the logs for an order.

## Contributing
Contributions are welcome! Please feel free to submit a pull request or open an issue if you have any suggestions or find any bugs.
