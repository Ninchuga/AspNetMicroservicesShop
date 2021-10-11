#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Clients/Shopping.MVC/Shopping.MVC.csproj", "Clients/Shopping.MVC/"]
COPY ["Common/Shopping.Common/Shopping.Common.csproj", "Common/Shopping.Common/"]
RUN dotnet restore "Clients/Shopping.MVC/Shopping.MVC.csproj"
COPY . .
WORKDIR "/src/Clients/Shopping.MVC"
RUN dotnet build "Shopping.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.MVC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY ./Certificates/Root/shopping-root-cert.crt /usr/local/share/ca-certificates/shopping-root-cert.crt
COPY ./Certificates/Root/shopping-root-cert.crt /etc/ssl/certs/shopping-root-cert.crt
RUN chmod 644 /usr/local/share/ca-certificates/shopping-root-cert.crt
RUN update-ca-certificates

ENTRYPOINT ["dotnet", "Shopping.MVC.dll"]

#COPY ./docker-entrypoint.sh /root/ShoppingMVC/docker-entrypoint.sh <- no need for this
#RUN chmod +x /root/ShoppingMVC/docker-entrypoint.sh #it's already mounted via volume in docker compose, no need to copy it

#WORKDIR /root/Identity
#ENTRYPOINT ["./docker-entrypoint.sh"]