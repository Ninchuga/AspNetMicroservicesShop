#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# If the .csproj paths are surronded with brackets [] COPY will fail on GitHub workflow build and in Azure ACR Task build
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY "Clients/Shopping.Razor/Shopping.Razor.csproj" "Clients/Shopping.Razor/"
COPY "Common/Shopping.Correlation/Shopping.Correlation.csproj" "Common/Shopping.Correlation/"
COPY "Common/Shopping.Logging/Shopping.Logging.csproj" "Common/Shopping.Logging/"
COPY "Common/Shopping.HealthChecks/Shopping.HealthChecks.csproj" "Common/Shopping.HealthChecks/"
RUN dotnet restore "Clients/Shopping.Razor/Shopping.Razor.csproj"
COPY . .
WORKDIR "/src/Clients/Shopping.Razor"
RUN dotnet build "Shopping.Razor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.Razor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Shopping.Razor.dll"]

RUN apt-get update # update linux packages for install

#COPY ./certs/shopping-root-cert.crt /usr/local/share/ca-certificates/shopping-root-cert.crt
#COPY ./certs/shopping-root-cert.crt /etc/ssl/certs/shopping-root-cert.crt
#RUN chmod 644 /usr/local/share/ca-certificates/shopping-root-cert.crt
#RUN update-ca-certificates

COPY ./Clients/Shopping.Razor/docker-entrypoint.sh /app/docker-entrypoint.sh

ENTRYPOINT ["./docker-entrypoint.sh"]