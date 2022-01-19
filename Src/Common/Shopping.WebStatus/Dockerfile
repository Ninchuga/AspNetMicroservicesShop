#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Common/Shopping.WebStatus/Shopping.WebStatus.csproj", "Common/Shopping.WebStatus/"]
COPY ["Services/Discount/Discount.Grpc/Discount.Grpc.csproj", "Services/Discount/Discount.Grpc/"]
COPY ["Common/Shopping.Common/Shopping.Common.csproj", "Common/Shopping.Common/"]
COPY ["Common/Shopping.HealthChecks/Shopping.HealthChecks.csproj", "Common/Shopping.HealthChecks/"]
RUN dotnet restore "Common/Shopping.WebStatus/Shopping.WebStatus.csproj"
COPY . .
WORKDIR "/src/Common/Shopping.WebStatus"
RUN dotnet build "Shopping.WebStatus.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.WebStatus.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Shopping.WebStatus.dll"]

COPY ./Common/Shopping.WebStatus/docker-entrypoint.sh /app/docker-entrypoint.sh

ENTRYPOINT ["./docker-entrypoint.sh"]
