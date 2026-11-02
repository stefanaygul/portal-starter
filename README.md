# Saha Ekipman Talep Portalı

BELBİM DevOps eğitiminin ortak örnek uygulaması. Saha ekipleri terminal adaptörü,
bağlantı kablosu, cihaz standı gibi ekipmanları listeler ve talep açar.
Veriler örnektir; gerçek sistemlere bağlanmaz.

Amaç uygulamanın kendisi değil, teslimat zinciridir: beş gün boyunca aynı repo
build edilir, test edilir, imaja dönüştürülür ve test ortamına dağıtılır.

## Bileşenler

| Klasör | Ne yapar? | Teknoloji |
|---|---|---|
| `src/product-service` | Ekipman kataloğu: `GET /api/products` | ASP.NET Core Web API |
| `src/order-service` | Talepler: `POST /api/orders`, `GET /api/orders` | ASP.NET Core Web API, EF Core, SQLite |
| `src/web-ui` | Katalog listesi, talep formu, talep listesi | React, Vite |
| `src/common-contracts` | İki API'nin paylaştığı DTO'lar | .NET class library |
| `tests` | API testleri | xUnit |
| `deploy` | Compose ve Nginx dosyaları (3. gün doldurulur) | |
| `Jenkinsfile` | Pipeline tanımı (4. gün doldurulur) | |

Her API'de `/health` (yanıt: `Healthy`) ve `/swagger` bulunur. Loglar standart çıktıya yazılır.

## Gereksinimler

- .NET SDK 10
- Node.js 24
- Paketler kurum içi Nexus'tan gelir: `NuGet.config` ve `src/web-ui/.npmrc`
  yalnızca Nexus group adreslerini gösterir.

## Komutlar

.NET tarafı (repo kökünde):

```bash
dotnet restore SahaPortal.sln
dotnet build SahaPortal.sln -c Release --no-restore
dotnet test SahaPortal.sln -c Release --no-build
dotnet list src/order-service package --include-transitive
```

React tarafı (`src/web-ui` içinde):

```bash
npm ci
npm test
npm run build
npm ls --depth=0
```

## Lokalde çalıştırma

Üç ayrı terminal açın:

```bash
dotnet run --project src/product-service      # http://localhost:5101
dotnet run --project src/order-service        # http://localhost:5102
cd src/web-ui && npm run dev                  # http://localhost:5173
```

Arayüz API'leri göreli adresle (`/api/...`) çağırır; geliştirme sunucusu bu
istekleri yukarıdaki iki adrese yönlendirir (`src/web-ui/vite.config.mjs`).

Kontrol:

```bash
curl http://localhost:5101/health
curl http://localhost:5101/api/products
curl -X POST http://localhost:5102/api/orders \
  -H "Content-Type: application/json" \
  -d '{ "productId": "ADP-01", "quantity": 2 }'
curl http://localhost:5102/api/orders
```

## Yapılandırma

| Ayar | Varsayılan | Ortam değişkeni |
|---|---|---|
| Talep veritabanı | `Data Source=orders.db` | `ConnectionStrings__Orders` |
| Dinlenen adres (lokal) | `launchSettings.json` içindeki port | `ASPNETCORE_URLS` |

Container imajında API'ler 8080 portunu dinler.
