#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Services/Delivery/Delivery.API/Delivery.API.csproj", "Services/Delivery/Delivery.API/"]
COPY ["Common/Shopping.HealthChecks/Shopping.HealthChecks.csproj", "Common/Shopping.HealthChecks/"]
COPY ["Common/Shopping.Logging/Shopping.Logging.csproj", "Common/Shopping.Logging/"]
COPY ["Common/Shopping.Correlation/Shopping.Correlation.csproj", "Common/Shopping.Correlation/"]
COPY ["BuildingBlocks/EventBus.Messages/EventBus.Messages.csproj", "BuildingBlocks/EventBus.Messages/"]
RUN dotnet restore "Services/Delivery/Delivery.API/Delivery.API.csproj"
COPY . .
WORKDIR "/src/Services/Delivery/Delivery.API"
RUN dotnet build "Delivery.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Delivery.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Delivery.API.dll"]

COPY ./Services/Delivery/Delivery.API/docker-entrypoint.sh /app/docker-entrypoint.sh

ENTRYPOINT ["./docker-entrypoint.sh"]
