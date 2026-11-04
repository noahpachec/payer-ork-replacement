# payer-ork-replacement

A small ASP.NET Core (.NET 10) app that uses Entity Framework Core to talk to a copy of the Payer database. The payer and wallet services were copied from the ork project.

## Prerequisites

- .NET 10 SDK
- The local Tide stack running, so the `postgresP` container is available on `localhost:5432`

## Build

```bash
cd PayerApi
dotnet build
```

## Run

```bash
cd PayerApi
dotnet run
```

The app listens on http://localhost:5080. Stop it with Ctrl+C.

## Try it

```bash
curl "http://localhost:5080/Payer/Subscription/GetSubscriptionDetails?customerId=<stripe customer id>"
# {"currentUserAcc":1,"expiryDate":1796281459}
```

To list the customer IDs you can use:

```bash
docker exec -i postgresP psql -U tideadmin -d zivadb_copy -c 'SELECT "CustomerId" FROM "Subscriptions";'
```
