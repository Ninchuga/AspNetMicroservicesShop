#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 8999

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Clients/Shopping.MVC/Shopping.MVC.csproj", "Clients/Shopping.MVC/"]
RUN dotnet restore "Clients/Shopping.MVC/Shopping.MVC.csproj"
COPY . .
WORKDIR "/src/Clients/Shopping.MVC"
RUN dotnet build "Shopping.MVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shopping.MVC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY shopping-root-cert.crt /usr/local/share/ca-certificates/shopping-root-cert.crt
RUN chmod 644 /usr/local/share/ca-certificates/shopping-root-cert.crt
RUN update-ca-certificates

ENTRYPOINT ["dotnet", "Shopping.MVC.dll"]

#COPY docker-entrypoint.sh ./
#RUN chmod +x ./docker-entrypoint.sh
#WORKDIR /root/ShoppingMVC
#ENTRYPOINT ["./docker-entrypoint.sh"]

#WORKDIR /root/ShoppingMVC
#ENTRYPOINT ["./docker-entrypoint.sh"]

# This is where the certificate gets imported
#WORKDIR /
#RUN openssl x509 -inform DER -in /https-root/shopping-root-cert.cer -out /https-root/shopping-root-cert.crt
#COPY /https-root/shopping-root-cert.crt /usr/local/share/ca-certificates/
#RUN update-ca-certificates

#ENTRYPOINT ["./docker-entrypoint.sh"]

#COPY ./Clients/Shopping.MVC/*.sh ./
#RUN chmod +x *.sh
#CMD ["./docker-entrypoint.sh"]
