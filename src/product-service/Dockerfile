# Build context: repo koku.  docker build -f src/product-service/Dockerfile .
ARG MCR=mcr.microsoft.com
FROM ${MCR}/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NuGet.config ./
COPY src/product-service/*.csproj src/product-service/
RUN dotnet restore src/product-service
COPY src/product-service/ src/product-service/
RUN dotnet publish src/product-service -c Release -o /app

FROM ${MCR}/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "product-service.dll"]
