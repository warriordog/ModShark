# ModShark - AutoMod for Sharkey instances

ModShark is a standalone moderation tool for servers running the [Sharkey fediverse server](https://activitypub.software/TransFem-org/Sharkey).

## Helpful Commands

### Apply migrations

```powershell
cd ModShark # Must run from the directory with appsettings.Development.json
dotnet ef database update --project ../SharkeyDB
```

### Create migration

```powershell
cd ModShark # Must run from the directory with appsettings.Development.json
dotnet ef migrations add $name --project ../SharkeyDB
```

### Remove migration (but keep changes)

```powershell
cd ModShark # Must run from the directory with appsettings.Development.json
dotnet ef migrations remove --project ../SharkeyDB
```
