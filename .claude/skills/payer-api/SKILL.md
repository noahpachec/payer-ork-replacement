---
name: payer-api
description: Use for any work in payer-ork-replacement's PayerApi project, the exemplar ASP.NET Core + EF Core app that connects to a copy of the Payer database (zivadb_copy on postgresP). Covers build/run/verify, the Payer entity model, refreshing the database copy, and the rules for porting more payer code from ~/project/ork (copy, never reference; DB-only methods; keep Ork.* namespaces). Use before adding endpoints, porting services or entities, or debugging the EF connection.
---

# payer-api: exemplar EF app for the Payer database

`PayerApi/` is a minimal .NET 10 web app. Controllers get `IPayerService` and `IWalletService` through DI. Those services use `PayerDataContext` (EF Core + Npgsql) against `zivadb_copy`, a copy of the payer ORK's `zivadb` database.

The code is **copied** from `~/project/ork`. Never add a project reference to ork or Cryptide.

## Build, run, verify

```bash
cd PayerApi && dotnet build      # expect 0 errors; ~21 CS8618/CS8603 nullable warnings are inherited from ork
cd PayerApi && dotnet run        # http://localhost:5080 (Properties/launchSettings.json, Development env)
curl "http://localhost:5080/Payer/Subscription/GetSubscriptionDetails?customerId=<id>"
```

- Get a valid customer ID from `SELECT "CustomerId" FROM "Subscriptions";` (see below).
- An unknown customer ID returns a 500. This is expected: ork's ProblemDetails middleware was not copied.
- As an agent, start the app with `run_in_background`. Stop it with `pkill -f '[n]et10.0/PayerApi'`. The bracket stops the pattern from matching your own shell. A plain `pkill -f PayerApi` kills the calling shell (exit 144).
- To verify a response, run the same query in SQL and compare.

## Database

| | |
|---|---|
| Container | `postgresP` (local Tide stack), host port 5432 |
| Database | `zivadb_copy` |
| Source | `zivadb`, the live payer ORK (Ork-4) database. **Never point the app at it or write to it.** |
| Connection string | `Payer_DB_Connection_String` in `PayerApi/appsettings.Development.json` |

Query the copy:

```bash
docker exec -i postgresP psql -U tideadmin -d zivadb_copy -c 'SELECT "Id", "CustomerId" FROM "Subscriptions";'
```

To refresh the copy from `zivadb`, drop and recreate it. This destroys anything written to the copy, so confirm with the user first:

```bash
docker exec -i postgresP psql -U tideadmin -d postgres -c 'DROP DATABASE zivadb_copy;' -c 'CREATE DATABASE zivadb_copy;'
docker exec -i postgresP sh -c 'pg_dump -U tideadmin --no-owner zivadb | psql -q -U tideadmin -d zivadb_copy'
```

`CREATE DATABASE ... TEMPLATE zivadb` fails while Ork-4 holds connections to `zivadb`, so use `pg_dump`.

The app never calls `Database.Migrate()` and has no migrations. The schema comes from the copy, which has ork's migration `20260409075715_EntityRefactoring` applied. Entity changes must still match that schema.

## Entity model

Source of truth: `~/project/ork/Ork/Ork/Entities/Payer/PAYER_ARCHITECTURE.md`. Read it before changing payer logic.

```
Customer (PK StripeCustomerId, GVZK = AuthorizerPack hex)
  └─1:N─ Subscription (PK Id = UUID string, FK CustomerId; ordered by CreatedAt, not Id)
         ├─1:N─ Wallet (PK WalletPK identity; FK SubscriptionId nullable; Index = gVRK hex, unique;
         │              Id = obfGVVK/Y'', indexed; Status "Pending" | "Active"; Expiry epoch seconds)
         └─1:N─ SubscriptionUser (PK UniqUID, FK SubscriptionId, LastLoginTime)
Validation (table "PayerValidations", standalone)
```

Table names come from the `DbSet` property names in `PayerDataContext`. Don't rename a `DbSet`.

## Layout

| Path | Origin |
|---|---|
| `Entities/Payer/*.cs`, `Helpers/PayerDataContext.cs`, `Models/Payer/CustomerModel.cs`, `Services/Payment/IWalletService.cs` | Copied unchanged from `Ork/Ork/` |
| `Services/Payment/PayerService.cs` | Ork's PayerService, keeping only the DB-only methods, plus `GetAllSubscriptions` (new, not in ork) |
| `Services/Payment/WalletService.cs` | Ork's WalletService, all methods kept; unused `Settings` dependency dropped; plus `GetAllWallets` (new, not in ork) |
| `Helpers/Utils.cs`, `Helpers/Serialization.cs`, `Models/Payer/Vouchers.cs` | Cut-down copies of Cryptide types (`GetEpochSeconds`, `DeserializeTo`, `ToBase64String`, `RedemptContent`, `SubscriptionTransportModel`) |
| `Exceptions/TideException.cs`, `Errors/TideErrorCodes.cs`, `Shared/ExpiryConstants.cs` | Cut down to the types and constants the copied services use |
| `Controllers/Payer/SubscriptionController.cs` | New: the single controller, with example `GET` endpoints `GetSubscriptionDetails?customerId=`, `Test?id=` (wallet by Id), `GetAllSubscriptions` and `GetAllWallets` |

## Rules for porting more code from ork

1. **Copy, never reference.** Keep the original `Ork.*` namespace so later copies drop in unchanged.
2. **Copy method bodies exactly.** Remove only what can't compile without ork (`using Cryptide.*`, unused injected dependencies).
3. **Port only methods that just read or write the database.** Leave out anything using Cryptide crypto (`TideKey`, `AuthorizerPack`, `Ed25519*`, signature verification) or business rules. Examples of business rules: `CheckRequestedActionAllowed` (quotas, debit amounts) and `CheckVRKisValid` (validation gate). Also leave out methods that call omitted ones, such as `IsActiveSubscriptionByCustomer`. When a method is borderline, ask the user.
4. **Bring dependencies in small.** If a ported method needs a helper, copy only the members it uses into the matching local file. Start the file with a `// Copied from <ork path> - only X is kept.` comment.
5. **Register new services** in `Program.cs` the way ork's `Program.cs` does (`AddScoped<IX, X>()`).
6. Keep the list of omitted methods at the top of `PayerService.cs` up to date.
