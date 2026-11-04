# Build context: repo koku.  docker build -f src/order-service/Dockerfile .
ARG MCR=mcr.microsoft.com
FROM ${MCR}/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NuGet.config ./
COPY src/common-contracts/ src/common-contracts/
COPY src/order-service/*.csproj src/order-service/
RUN dotnet restore src/order-service
COPY src/order-service/ src/order-service/
RUN dotnet publish src/order-service -c Release -o /app

FROM ${MCR}/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "order-service.dll"]
