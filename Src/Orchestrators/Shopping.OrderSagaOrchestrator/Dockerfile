#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Orchestrators/Shopping.OrderSagaOrchestrator/Shopping.OrderSagaOrchestrator.csproj", "Orchestrators/Shopping.OrderSagaOrchestrator/"]
COPY ["Common/Shopping.Logging/Shopping.Logging.csproj", "Common/Shopping.Logging/"]
COPY ["Common/Shopping.Correlation/Shopping.Correlation.csproj", "Common/Shopping.Correlation/"]
COPY ["BuildingBlocks/EventBus.Messages/EventBus.Messages.csproj", "BuildingBlocks/EventBus.Messages/"]
COPY ["Common/Shopping.HealthChecks/Shopping.HealthChecks.csproj", "Common/Shopping.HealthChecks/"]
COPY ["BuildingBlocks/Host/Shopping.HostCustomization/Shopping.HostCustomization.csproj", "BuildingBlocks/Host/Shopping.HostCustomization/"]
RUN dotnet restore "Orchestrators/Shopping.OrderSagaOrchestrator/Shopping.OrderSagaOrchestrator.csproj"
COPY . .
WORKDIR "/src/Orchestrators/Shopping.OrderSagaOrchestrator"
RUN dotnet build "Shopping.OrderSagaOrchestrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.OrderSagaOrchestrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Shopping.OrderSagaOrchestrator.dll"]

COPY ./Orchestrators/Shopping.OrderSagaOrchestrator/docker-entrypoint.sh /app/docker-entrypoint.sh

ENTRYPOINT ["./docker-entrypoint.sh"]