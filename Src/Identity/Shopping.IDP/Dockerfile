#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Identity/Shopping.IDP/Shopping.IDP.csproj", "Identity/Shopping.IDP/"]
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
ENTRYPOINT ["dotnet", "Shopping.IDP.dll"]