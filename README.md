# Solster Aspire

Shared Aspire integration packages for Solster applications.

## Packages

All packages target `net10.0`.

- `Solster.Aspire.Azure.CommunicationServices`
- `Solster.Aspire.Azure.FrontDoor`
- `Solster.Aspire.Hosting.Azure.CommunicationServices`
- `Solster.Aspire.Hosting.Azure.FrontDoor`
- `Solster.Aspire.Hosting.Azure.PostgreSQL`

## Restore

Packages are published to GitHub Packages:

```sh
dotnet nuget add source https://nuget.pkg.github.com/solster/index.json \
  --name solster \
  --username x-access-token \
  --password "$NUGET_AUTH_TOKEN" \
  --store-password-in-clear-text
```

## Build and Pack

```sh
dotnet restore Solster.Aspire.slnx
dotnet build Solster.Aspire.slnx --configuration Release --no-restore
dotnet pack Solster.Aspire.slnx --configuration Release --no-build --output artifacts/packages /p:PackageVersion=0.1.0-preview.1
```

## API Summary

`Solster.Aspire.Hosting.Azure.CommunicationServices` adds Aspire AppHost extensions for provisioning Azure Communication Services and exposing the connection settings to dependent resources.

`Solster.Aspire.Azure.CommunicationServices` adds `AddAzureCommunicationEmailClient` and `AddKeyedAzureCommunicationEmailClient` registration helpers for runtime applications and workers.

`Solster.Aspire.Hosting.Azure.FrontDoor` adds Aspire AppHost extensions for Azure Front Door provisioning.

`Solster.Aspire.Azure.FrontDoor` adds runtime Front Door configuration and middleware helpers for ASP.NET Core applications.

`Solster.Aspire.Hosting.Azure.PostgreSQL` adds an Aspire deploy pipeline extension for applying EF Core migrations against Azure PostgreSQL using `DefaultAzureCredential`.
