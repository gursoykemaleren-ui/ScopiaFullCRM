# CrmWorkTrack (CRM & Work Tracking)

Backend tarafı: **.NET 8 Web API** + **Clean Architecture** + **EF Core (SQL Server)**  
Amaç: CRM + iş/işlem takibi (ERP panel) için rol & yetki kontrollü API altyapısı.

## Tech Stack
- .NET 8 (ASP.NET Core Web API)
- EF Core + SQL Server
- JWT Authentication + Refresh Token
- Role-based Authorization
- Permission/Policy-based Authorization (`perm:*`)
- Swagger (Authorize ile test)

## Architecture (Clean Architecture)
- **CrmWorkTrack.Domain**: Entity’ler, core kurallar
- **CrmWorkTrack.Application**: Use case / interface katmanı
- **CrmWorkTrack.Infrastructure**: EF Core DbContext, migrations, persistence
- **CrmWorkTrack.WebApi**: Controller’lar, DI, Swagger, API endpoints

## Implemented Features
- ✅ JWT Login (token üretimi)
- ✅ Role claim token’a eklendi, `[Authorize(Roles="Admin")]` test edildi
- ✅ Permission/Policy tabanlı yetkilendirme (`perm:jobs.read`, `perm:jobs.create`, ...)
- ✅ Refresh Token altyapısı
- ✅ Admin controller’ları (Users/Roles/Permissions yönetimi)
- ✅ Jobs CRUD endpointleri
- ✅ JobActivity + JobComment altyapısı
- ✅ EF Core Migrations ile DB versiyonlama

## Local Setup
### 1) Requirements
- .NET SDK 8
- SQL Server (LocalDB veya MSSQL)

### 2) Database Migration
```bash
dotnet ef database update

## API Quick Test (Swagger)
1. Projeyi çalıştır:
```bash
dotnet run --project CrmWorkTrack.WebApi