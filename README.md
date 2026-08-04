# HRM API - Database Migration Guide

Backend được phát triển bằng .NET 10 Clean Architecture. Hệ thống sử dụng Entity Framework Core với PostgreSQL.

## 1. Cơ chế Tự động Tạo và Cập nhật Database

Hệ thống **tự tạo database (UTF-8) nếu chưa tồn tại**, rồi mới chạy migration. Điều này áp dụng cho cả:

- **`dotnet run`** (khởi chạy API)
- **`dotnet ef database update`** (chạy migration bằng CLI)

Luồng xử lý:

1. Kết nối PostgreSQL qua database `postgres`
2. Kiểm tra database trong connection string đã tồn tại chưa
3. Nếu **chưa có** → tạo mới với `ENCODING UTF8`
4. Nếu **đã có** nhưng **không phải UTF-8** → dừng và báo lỗi (cần drop database cũ)
5. Chạy toàn bộ migration còn thiếu

Các file liên quan:

- `DatabaseBootstrap.cs` — tạo/kiểm tra database UTF-8
- `ApplicationDbContextFactory.cs` — hook cho lệnh `dotnet ef`

Do đó, bạn chỉ cần cấu hình đúng Connection String trong [appsettings.json](HrmApi.WebApi/appsettings.json), PostgreSQL phải đang chạy, rồi chạy API hoặc lệnh migration — database sẽ được tạo tự động.

---

## 2. Hướng dẫn chạy lệnh tạo Migration bằng CLI

Nếu bạn thay đổi cấu trúc Entities trong project `HrmApi.Domain` và muốn tạo một file Migration mới, hãy làm theo các bước sau:

### Bước 2.1: Cài đặt công cụ Entity Framework Core CLI (Nếu chưa cài)

Mở terminal ở máy của bạn và chạy lệnh cài đặt global:

```bash
dotnet tool install --global dotnet-ef
```

_Lưu ý: Nếu đã cài đặt trước đó, bạn có thể cập nhật qua lệnh:_

```bash
dotnet tool update --global dotnet-ef
```

### Bước 2.2: Tạo Migration mới (Generate Migration)

Di chuyển terminal về thư mục gốc chứa file solution `HrmApi.slnx` hoặc `HrmApi/` (thư mục `f:\Projects\hrm\HrmApi`) và thực hiện chạy lệnh:

```bash
dotnet ef migrations add <TenMigration> --project HrmApi.Infrastructure --startup-project HrmApi.WebApi --output-dir Persistence/Migrations
```

_Ví dụ tạo migration khởi tạo:_

```bash
dotnet ef migrations add InitialCreate --project HrmApi.Infrastructure --startup-project HrmApi.WebApi --output-dir Persistence/Migrations
```

---

## 3. Hướng dẫn cập nhật Database thủ công bằng CLI

Chạy từ thư mục `HrmApi/`:

```bash
dotnet ef database update --project HrmApi.Infrastructure --startup-project HrmApi.WebApi
```

Lệnh này sẽ:

1. **Tự tạo database** (UTF-8) nếu chưa tồn tại
2. **Áp dụng migration** còn thiếu

Bạn sẽ thấy log dạng:

```text
Checking PostgreSQL database before EF migrations...
Database HrmApiDb does not exist. Creating with UTF8 encoding...
Database HrmApiDb is ready with UTF8 encoding.
Applying migration '...'
Done.
```

---

## 4. Cấu hình Connection String

Hãy đảm bảo cấu hình kết nối PostgreSQL chính xác tại file [appsettings.json](file:///f:/Projects/hrm/HrmApi/HrmApi.WebApi/appsettings.json):

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=HrmApiDb;Username=postgres;Password=postgres;Encoding=UTF8;Client Encoding=UTF8"
}
```

> **Lưu ý:** Database mới luôn được tạo với **UTF-8** để hỗ trợ tiếng Việt. Nếu database cũ dùng encoding khác (ví dụ WIN1252), hãy drop và chạy lại migration:

```sql
DROP DATABASE IF EXISTS "HrmApiDb" WITH (FORCE);
```

---

## 5. Xử lý lỗi kết nối PostgreSQL

### Lỗi: `connection was forcibly closed` / `database system is in recovery mode`

PostgreSQL đang **recovery mode** hoặc chưa sẵn sàng nhận kết nối.

**Cách xử lý:**

1. Mở **Services** (`services.msc`)
2. Tìm service **PostgreSQL**
3. **Restart** service (cần quyền Administrator)
4. Chạy lại API hoặc migration

Kiểm tra nhanh:

```bash
psql -h localhost -U postgres -d HrmApiDb -c "SELECT 1;"
```

### Cơ chế retry trong code

API đã được cấu hình:

- **Chờ PostgreSQL sẵn sàng** khi khởi động (retry tối đa 30 lần)
- **EnableRetryOnFailure** cho EF Core (retry query khi lỗi tạm thời)
- **Connection pool** với `Keepalive`, `Timeout` ổn định hơn
