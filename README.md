# GadgetsOnline — ADO.NET Edition

A sample online shopping app on .NET 8, modernized from Entity Framework 6
(code-first) to **raw ADO.NET with stored procedures**. The database schema lives
in plain SQL (DDL) scripts instead of EF migrations, and all data access goes
through stored procedures via `Microsoft.Data.SqlClient`.

The solution is split into a shared data library plus two web apps: the
customer-facing store and a separate read-only reporting app.

## Projects

| Project | Type | Description |
|---------|------|-------------|
| `GadgetsOnline.Data` | Class library | Shared ADO.NET data layer: the `Database` connection helper, model classes, and repositories (products, categories, carts, orders, reports). |
| `GadgetsOnline` | ASP.NET Core MVC | The online store (browse, cart, checkout). Owns database bootstrapping. |
| `GadgetsOnline.Reporting` | ASP.NET Core MVC | Read-only reporting dashboard (sales by category, top products, revenue by month). |

Both apps reference `GadgetsOnline.Data` and connect to the same SQL Server database.

## Database

The schema is defined as SQL scripts under `GadgetsOnline/Database/`:

- `schema.sql` — table DDL (idempotent `CREATE TABLE` guards)
- `stored_procedures.sql` — all stored procedures (`CREATE OR ALTER`), including
  store CRUD, an atomic transactional checkout (`Checkout_PlaceOrder`),
  a "frequently bought together" recommendation query, and the reporting procs
- `seed.sql` — catalog seed data and a small set of sample orders (demo data)

On startup the **store** app runs these scripts via a small bootstrapper
(`Database.Initialize`): it creates the database if missing, then applies schema →
procedures → seed. Each script is idempotent, so it is safe to run on every launch.
The **reporting** app is read-only and does not bootstrap; it only calls procedures.

> To create the database manually instead, run the scripts in order with `sqlcmd`
> or SSMS. The bootstrapper is a convenience, not a requirement.

## Configuration

The connection string lives in each app's `appsettings.json` under
`ConnectionStrings:GadgetsOnlineEntities`, with placeholder values:

```json
"GadgetsOnlineEntities": "Server=YOUR_SERVER,1433;Initial Catalog=GadgetsOnlineADO;User ID=YOUR_DB_USER;Encrypt=True;TrustServerCertificate=True;"
```

The **password is not stored in the file**. It is supplied separately and merged
in at startup (`Database.BuildConnectionString`). For local development, set it
(and optionally the real server/user) via user secrets:

```bash
# from the GadgetsOnline project folder
dotnet user-secrets set "ConnectionStrings:GadgetsOnlineEntities" "Server=your-host,1433;Initial Catalog=GadgetsOnlineADO;User ID=your-user;Encrypt=True;TrustServerCertificate=True;"
dotnet user-secrets set "DbPassword" "your-password"
```

Do the same for the `GadgetsOnline.Reporting` project. User secrets only load in
the **Development** environment. For deployment, supply the password via the
`DbPassword` environment variable (or override the whole connection string with
`ConnectionStrings__GadgetsOnlineEntities`).

## Running

```bash
# Store
dotnet run --project GadgetsOnline            # http://localhost:5051

# Reporting (run separately)
dotnet run --project GadgetsOnline.Reporting --urls http://localhost:5080
```

Start the store first so the database schema, procedures, and seed data are
created before the reporting app queries them.

## Pre-requisites

1. .NET 8.0 SDK
2. SQL Server (any edition; LocalDB works with an adjusted connection string)
3. Visual Studio 2022 17.14.6+ or the .NET CLI
4. SQL Server Management Studio (optional, for inspecting the database)
5. Git for Windows

## Modernization artifacts

`non-code-artifacts/` contains anonymized output from the SQL Server extraction
and the SQL-Server-to-PostgreSQL conversion tooling used during modernization.
Server names, user names, file paths, and unrelated database names have been
replaced with neutral placeholders; product/edition/version details are preserved.

## Notes

- The reporting app has no authentication and exposes aggregate sales data. It is
  intended for local/internal use; add access controls before exposing it more widely.

## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This library is licensed under the MIT-0 License. See the LICENSE file.
