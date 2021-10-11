#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 8021

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Identity/Shopping.IDP/Shopping.IDP.csproj", "Identity/Shopping.IDP/"]
RUN dotnet restore "Identity/Shopping.IDP/Shopping.IDP.csproj"
COPY . .
WORKDIR "/src/Identity/Shopping.IDP"
RUN dotnet build "Shopping.IDP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.IDP.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY ./Certificates/Root/shopping-root-cert.crt /usr/local/share/ca-certificates/shopping-root-cert.crt
COPY ./Certificates/Root/shopping-root-cert.crt /etc/ssl/certs/shopping-root-cert.crt
RUN chmod 644 /usr/local/share/ca-certificates/shopping-root-cert.crt
RUN update-ca-certificates

ENTRYPOINT ["dotnet", "Shopping.IDP.dll"]
#ENTRYPOINT ["./docker-entrypoint.sh"]

#COPY ./docker-entrypoint.sh /root/ShoppingMVC/docker-entrypoint.sh <- no need for this
#RUN chmod +x /root/ShoppingMVC/docker-entrypoint.sh #it's already mounted via volume in docker compose, no need to copy it

#WORKDIR /root/Identity
#ENTRYPOINT ["./docker-entrypoint.sh"]
