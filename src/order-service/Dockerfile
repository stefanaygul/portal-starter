# Build context: repo koku.  docker build -f src/order-service/Dockerfile .
ARG MCR=mcr.microsoft.com
FROM ${MCR}/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NuGet.config ./
COPY src/order-service/*.csproj src/order-service/
RUN dotnet restore src/order-service
COPY src/order-service/ src/order-service/
RUN dotnet publish src/order-service -c Release -o /app

FROM ${MCR}/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# SQLite dosyasi /data altindaki volume'da durur; dizinin sahibi uygulama kullanicisidir.
RUN mkdir /data && chown $APP_UID /data
USER $APP_UID
ENTRYPOINT ["dotnet", "order-service.dll"]
