[![Build and test](https://github.com/Ninchuga/AspNetMicroservicesShop/actions/workflows/build_and_test.yml/badge.svg?branch=main)](https://github.com/Ninchuga/AspNetMicroservicesShop/actions/workflows/build_and_test.yml)

# Example of a web "Shopping portal" using microservice architecture

## Run the app with HTTPS using Docker or Docker Compose 🐳

- **Using Razor Web Client**  
run the app using this URL
*https://host.docker.internal:8999* in order to work properly with the **Identity Server 4**
- **Using Angular Web Client**  
run the app using this URL *https://host.docker.internal:8200*

## UI - Web Clients
- Razor Pages
- Angular -> when running from docker compose it will use **nginx** as a reverse proxy

## Authentication & Authorization
- Identity Server 4 for authentication and authorization of services using refresh tokens

## Databases used in the solution:
- Sql Server
- Mongo DB
- PostgreSql
- Redis

## Azure Functions ⚡
- Used with SendGrid for sending emails
- Integration with Azure Key Vault for retrieving secrets

## Gateways 🚪
- Ocelot Api Gateway which communicates with all the downstream services
- Aggregator pattern used to communicate to downstream services and retreive specific response needed for the requested UI

## MassTransit 🚌
- Order saga orchestrator pattern using MassTransit Saga for long running operation like placing an order
- RabbitMq implementation using MassTransit
- Azure Service Bus implementation using MassTransit

## Other things like... ⭐
- Discount API implementation using gRpc
- Correlation id implementation for tracking of requests in distributed system
- Services Health Checks
- Unit & integration tests using xUnit framework with fluent assertion
- Fluent validation
- DDD modeling in Order API using aggregates and value objects
- Services resilience using Polly
- Structured logging using Serilog, Elastic Search and Kibana
- Nginx as reverse proxy

