# Example of a web "Shopping portal" using microservice architecture

## You can run the project with HTTPS using Docker 🐳

When running from docker compose, start the Razor web project with this URL  
*https://host.docker.internal:8999*  
in order to work properly with the **Identity Server 4**.

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

