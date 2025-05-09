🛒 E-Commerce Payment Integration API
.NET 8 ile geliştirilmiş, Balance Management API ile entegre çalışan bir e-ticaret ödeme altyapısı. Temel işlevler:

- Ürünleri listeleme

- Sipariş oluşturma ve bakiye ayırma

- Siparişi tamamlama ve ödeme alma

Swagger UI

🚀 Özellikler
- ✅ Ürün listeleme (GET /api/products)
- ✅ Sipariş oluşturma (POST /api/orders/create)
- ✅ Sipariş tamamlama (POST /api/orders/{id}/complete)
- ✅ SQLite veritabanı ile kalıcı kayıt
- ✅ Refit + Polly ile dış API hatalarına karşı dayanıklılık
- ✅ Serilog ile dosyaya loglama
- ✅ FluentValidation ile veri doğrulama
- ✅ xUnit ile %100'e yakın unit test coverage
- ✅ Integration testler
- ✅ Docker ve docker-compose desteği

# 🧱 Katmanlar ve Mimari

Proje Clean Architecture yaklaşımı ile inşa edilmiştir. Bu yapı sayesinde katmanlar bağımsız ve test edilebilir halde organize edilmiştir.

### ECommercePI.Domain
- 🔹 Varlık sınıfları, temel domain modelleri

### ECommercePI.Application
- 🔹 CQRS mimarisi uygulanarak Command, Query ve Handler yapıları ayrıştırılmıştır
- 🔹 MediatR kütüphanesi ile yönlendirme (dispatching) işlemleri yapılmıştır
- 🔹 FluentValidation ile istek doğrulama işlemleri

### ECommercePI.Infrastructure
- 🔹 Refit kullanılarak dış servis entegrasyonları (Balance API)
- 🔹 Polly ile resilience (retry, timeout) stratejileri
- 🔹 EF Core + SQLite altyapısı ile veri erişimi

### ECommercePI.WebAPI
- 🔹 ASP.NET Core REST API controller’ları
- 🔹 Global exception yakalayıcı (ExceptionMiddleware)
- 🔹 Serilog ile dosyaya log yazımı

### ECommercePI.Tests.Unit / Integration
- 🔹 Unit test'ler CQRS handler seviyesinde
- 🔹 Integration test'ler controller ve API endpoint'leri üzerinde

# ⚙️ Kurulum

### **🖥️ Yerel Geliştirme için**

`dotnet run --project ECommercePI.WebAPI`

Swagger arayüzü: http://localhost:5209/swagger

### 🐳 **Docker ile Çalıştırmak İçin**

Docker image oluştur ve başlat:

`docker compose up --build -d`

_Uygulamayı aç:_ http://localhost/swagger

### _Log ve veri klasörleri:_

* Logs/log-*.txt → Uygulama log dosyaları
* data/app.db → SQLite veritabanı

## 🛠️ Geliştirici Notları

* Refit + Polly ile zaman aşımı ve retry logic uygulanmıştır.
* ExceptionMiddleware ile global exception yakalama yapılmıştır.
* Validasyon hataları detaylı biçimde dönülür.
* Test sınıfları Tests.Unit ve Tests.Integration altında ayrılmıştır.
* appsettings.json ile konfigürasyon yönetimi yapılır.
* Docker logları Logs/ klasörüne mount edilir.

## 📦 Gereksinimler
| Araç                                                    | Versiyon / Açıklama                               |
| ------------------------------------------------------- | ------------------------------------------------- |
| [.NET SDK](https://dotnet.microsoft.com/en-us/download) | **8.0** veya üzeri                                |
| [Docker](https://www.docker.com/)                       | **v28+** (Docker Engine) & **Docker Compose v2+** |
| [JetBrains Rider](https://www.jetbrains.com/rider/)     | **2023.3+** önerilir                              |
| İşletim Sistemi                                         | macOS Sequoia 15.4.1 veya Windows 10+         |

💡 Geliştirme ortamı olarak JetBrains Rider kullanılmıştır. Proje yapılandırmaları, test çalıştırmaları ve Docker entegrasyonu Rider IDE’si ile sorunsuz çalışmaktadır.
