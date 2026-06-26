# Setup Guide

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| Node.js | 20.x or 22.x | `node --version` to check |
| npm | 10.x | bundled with Node |
| .NET SDK | 10.x | `dotnet --version` to check |
| Git | any | |

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

## Frontend

### First-time setup

```bash
cd frontend
cp .env.example .env
npm install
```

Edit `.env` if your backend runs on a port other than `5000`.

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

## Backend *(not yet scaffolded — coming soon)*

The backend will be a .NET 10 Web API. Once scaffolded:

```bash
cd backend
dotnet restore
dotnet run --project DynamicHR.API
```

Default API URL: `http://localhost:5000`

---

## Project structure

```
dynamic-hr-in-ef-core/
  frontend/               Vite + React + TypeScript SPA
    src/
      api/                Typed fetch wrappers (userTypes, users)
      components/         DynamicForm, DynamicSearch, FieldEditor
      lib/                queryClient, cn utility
      pages/              UserTypesPage, AddUserPage, SearchUsersPage, ViewUserPage
      types/              schema.ts (UserType, FieldDefinition), records.ts (User)
    .env.example          Copy to .env and set VITE_API_BASE_URL
  backend/                .NET 10 Web API (coming soon)
  SETUP.md                This file
```

---

## Versioning plan

| Version | Scope |
|---|---|
| v1 | User type schema editor, create user, search users, view user (read-only) |
| v2 | Edit user records |
| v3 | User type inheritance (`parentTypeId`) |
