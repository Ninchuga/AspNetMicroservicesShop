#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["ApiGateways/OcelotApiGateway/OcelotApiGateway.csproj", "ApiGateways/OcelotApiGateway/"]
COPY ["Common/Shopping.Logging/Shopping.Logging.csproj", "Common/Shopping.Logging/"]
COPY ["Common/Shopping.Correlation/Shopping.Correlation.csproj", "Common/Shopping.Correlation/"]
COPY ["Common/Shopping.HealthChecks/Shopping.HealthChecks.csproj", "Common/Shopping.HealthChecks/"]
RUN dotnet restore "ApiGateways/OcelotApiGateway/OcelotApiGateway.csproj"
COPY . .
WORKDIR "/src/ApiGateways/OcelotApiGateway"
RUN dotnet build "OcelotApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OcelotApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "OcelotApiGateway.dll"]

COPY ./ApiGateways/OcelotApiGateway/docker-entrypoint.sh /app/docker-entrypoint.sh

ENTRYPOINT ["./docker-entrypoint.sh"]