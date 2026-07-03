# Setup Guide

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| Node.js | 20.x or 22.x | `node --version` to check |
| npm | 10.x | bundled with Node |
| .NET SDK | 10.x | `dotnet --version` to check |
| dotnet-ef CLI | 10.x | see below |
| SQL Server | any | LocalDB works for dev — see below |
| Git | any | |

### Install / update the EF Core CLI tool

```bash
dotnet tool install --global dotnet-ef
# or if already installed:
dotnet tool update --global dotnet-ef
```

### SQL Server for local development

The default connection string in `appsettings.json` targets **SQL Server LocalDB**, which ships with Visual Studio. If you don't have Visual Studio, the easiest alternatives are:

- **SQL Server Express LocalDB** — install from [aka.ms/sqllocaldb](https://aka.ms/sqllocaldb)
- **SQL Server Developer Edition** — free, full-featured, runs as a Windows service
- **Docker** — `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123" -p 1433:1433 mcr.microsoft.com/mssql/server:latest`

To use Docker or a full SQL Server instance, update the connection string in `backend/DynamicHR.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=DynamicHR;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
}
```

---

## Clone the repo

```bash
git clone https://github.com/jfairbourn96/dynamic-hr-in-ef-core.git
cd dynamic-hr-in-ef-core
```

> **Corporate machines with SSL proxy:** if `git clone` fails with a certificate error, run:
> ```bash
> git -c http.sslVerify=false clone https://github.com/jfairbourn96/dynamic-hr-in-ef-core.git
> ```

---

## Backend

### First-time setup

```bash
cd backend
dotnet restore
```

### Apply database migrations

Migrations live in `DynamicHR.Data/Migrations/`. Run this from the `backend/` directory any time you pull new migration files:

```bash
dotnet ef database update --project DynamicHR.Data --startup-project DynamicHR.API
```

The API also auto-applies pending migrations on startup in the Development environment, so running `dotnet run` on a fresh clone will create the database automatically.

### Run the API

```bash
dotnet run --project DynamicHR.API
```

API runs at `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS).  
OpenAPI spec available at `http://localhost:5000/openapi/v1.json` in Development.

### Adding a new migration (after schema changes)

```bash
dotnet ef migrations add <MigrationName> --project DynamicHR.Data --startup-project DynamicHR.API
dotnet ef database update --project DynamicHR.Data --startup-project DynamicHR.API
```

---

## Frontend

### First-time setup

```bash
cd frontend
cp .env.example .env
npm install
```

Edit `.env` if your backend runs on a port other than `5000`:

```
VITE_API_BASE_URL=http://localhost:5000/api
```

### Run the dev server

```bash
npm run dev
```

App opens at `http://localhost:5173`.

### Build for production

```bash
npm run build        # outputs to frontend/dist/
npm run preview      # serves the production build locally
```

---

## Running both together

Open two terminals from the repo root:

```bash
# Terminal 1 — API
cd backend && dotnet run --project DynamicHR.API

# Terminal 2 — Frontend
cd frontend && npm run dev
```

---

## Project structure

```
dynamic-hr-in-ef-core/
  backend/
    DynamicHR.Core/         Entities, enums, DTOs, interfaces, services (packageable)
      Entities/             UserType, FieldDefinition, User
      Enums/                FieldType
      DTOs/                 Response DTOs and request models
      Interfaces/           IUserTypeService, IUserService, IUserTypeRepository, IUserRepository
      Services/             UserTypeService, UserService
    DynamicHR.Data/         EF Core DbContext, configurations, repositories (packageable)
      Configurations/       Fluent API entity configs
      Migrations/           EF Core migration history
      Repositories/         UserTypeRepository, UserRepository
      Extensions/           AddDynamicHRData() DI extension
    DynamicHR.API/          ASP.NET Core host — not packageable
      Controllers/          UserTypesController, UsersController
      Program.cs            DI wiring, middleware, CORS, auto-migrate
      appsettings.json      Connection string, CORS origins
    DynamicHR.sln
  frontend/                 Vite + React + TypeScript SPA
    src/
      api/                  Typed fetch wrappers (userTypes, users)
      components/           DynamicForm, DynamicSearch, FieldEditor
      lib/                  queryClient, cn utility
      pages/                UserTypesPage, AddUserPage, SearchUsersPage, ViewUserPage
      types/                schema.ts (UserType, FieldDefinition), records.ts (User)
    .env.example            Copy to .env and set VITE_API_BASE_URL
  SETUP.md                  This file
```

---

## Versioning plan

| Version | Scope |
|---|---|
| v1 | User type schema editor, create user, search users, view user (read-only) |
| v2 | Edit user records |
| v3 | User type inheritance (`parentTypeId`) |
