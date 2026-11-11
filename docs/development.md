# Development setup

This covers Phase 0: the API, the web app, Microsoft 365 sign-in, roles and the audit log.

## What's in the repository

| Path | What it is |
| --- | --- |
| `src/ChMS.Domain` | Entities and rules with no database or web code: `Money`, roles, users, audit log |
| `src/ChMS.Infrastructure` | EF Core `ChmsDbContext`, the audit interceptor, migrations |
| `src/ChMS.Api` | ASP.NET Core API: Entra ID sign-in, authorization policies, controllers |
| `tests/` | xUnit tests; API tests use in-memory SQLite and a fake sign-in |
| `web/` | React + TypeScript app (Vite) using MSAL to sign in with Microsoft 365 |
| `.github/workflows/ci.yml` | Builds and tests both on every pull request |

## Prerequisites

- .NET 10 SDK
- Node.js 22
- SQL Server for local development. Either run it in Docker:

  ```bash
  docker run -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='<a strong password>' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
  ```

  or use SQL Server LocalDB on Windows and change the connection string.

## One-time Microsoft Entra setup

You need two app registrations in the church's Microsoft 365 tenant (Entra admin center → App registrations).

**1. API app ("ChMS API")**

1. New registration, single tenant, no redirect URI.
2. *Expose an API*: set the Application ID URI to `api://<api-client-id>` and add a scope `access_as_user` (admins and users can consent).
3. *App roles*: create these roles, allowed member type Users/Groups, with these exact values:
   `Administrator`, `Treasurer`, `FinanceAssistant`, `Counter`, `Staff`.
4. Enterprise applications → ChMS API → *Users and groups*: assign each staff member their role(s). Turn on *Assignment required* so only assigned people can sign in.

**2. Web app ("ChMS Web")**

1. New registration, single tenant, platform *Single-page application*, redirect URI `http://localhost:5173` (add the production URL later).
2. *API permissions* → My APIs → ChMS API → delegated `access_as_user` → grant admin consent.

Require multi-factor authentication for these apps with a Conditional Access policy, or rely on your tenant's security defaults.

Roles live in Entra rather than in our database, so every role grant is recorded in Entra's audit log.

## Configure and run

```bash
# API settings (kept out of git with user secrets)
cd src/ChMS.Api
dotnet user-secrets set "AzureAd:TenantId" "<tenant-id>"
dotnet user-secrets set "AzureAd:ClientId" "<api-client-id>"
dotnet user-secrets set "AzureAd:Audience" "api://<api-client-id>"
dotnet user-secrets set "ConnectionStrings:Chms" "Server=localhost,1433;Database=Chms;User Id=sa;Password=<password>;TrustServerCertificate=True"
cd ../..

# Create the database
dotnet tool restore
CHMS_DESIGN_CONNECTION="<same connection string>" \
  dotnet ef database update --project src/ChMS.Infrastructure --startup-project src/ChMS.Infrastructure

# Run the API on http://localhost:5169
dotnet run --project src/ChMS.Api --launch-profile http

# In another terminal: the web app on http://localhost:5173
cd web
cp .env.example .env.local   # then fill in the three values
npm install
npm run dev
```

Open http://localhost:5173, sign in, and you should see your name and roles. The first sign-in creates your user record and an audit log entry.

## Everyday commands

```bash
dotnet build
dotnet test
cd web && npm run lint && npm run build
```

Adding a migration after changing the model:

```bash
dotnet ef migrations add <Name> --project src/ChMS.Infrastructure --startup-project src/ChMS.Infrastructure --output-dir Persistence/Migrations
```

CI fails if the model and migrations drift apart.

## How the Phase 0 pieces work

- **Sign-in:** the API validates Entra access tokens with Microsoft.Identity.Web. Every endpoint requires a signed-in user unless marked `[AllowAnonymous]` (only `/health` is).
- **Roles and policies:** `Roles` in `ChMS.Domain/Security` must match the Entra app role values. Policies: `CanSeeGiving` (Treasurer, FinanceAssistant), `CanReadAuditLog` (Treasurer, Administrator), `CanAdminister` (Administrator).
- **Audit log:** any entity implementing `IAudited` gets a row in `AuditLog` for every create, update and delete, with before and after values as JSON, written in the same transaction. Properties marked `[AuditIgnore]` (like `LastSignInAt`) don't trigger entries. The app refuses to change or delete audit rows, and a database trigger refuses too.
- **Money:** use the `Money` type in domain code. It is `decimal`-backed and refuses fractions of a cent. All `decimal` columns are `decimal(19,4)`.

## Endpoints

| Method | Path | Who |
| --- | --- | --- |
| GET | `/health` | Anyone |
| GET | `/api/me` | Any signed-in staff member |
| GET | `/api/audit-log?recordType=&recordId=&take=` | Treasurer, Administrator |
| GET | `/openapi/v1.json` | Development only |
