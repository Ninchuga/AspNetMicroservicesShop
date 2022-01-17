#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Identity/Shopping.IDP/Shopping.IDP.csproj", "Identity/Shopping.IDP/"]
COPY ["Common/Shopping.Logging/Shopping.Logging.csproj", "Common/Shopping.Logging/"]
COPY ["Common/Shopping.Correlation/Shopping.Correlation.csproj", "Common/Shopping.Correlation/"]
COPY ["Common/Shopping.HealthChecks/Shopping.HealthChecks.csproj", "Common/Shopping.HealthChecks/"]
RUN dotnet restore "Identity/Shopping.IDP/Shopping.IDP.csproj"
COPY . .
WORKDIR "/src/Identity/Shopping.IDP"
RUN dotnet build "Shopping.IDP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.IDP.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Shopping.IDP.dll"]

COPY ./Identity/Shopping.IDP/docker-entrypoint.sh /app/docker-entrypoint.sh

ENTRYPOINT ["./docker-entrypoint.sh"]