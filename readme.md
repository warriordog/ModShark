# ModShark - AutoMod for Sharkey instances

ModShark is a standalone moderation tool for servers running the [Sharkey fediverse server](https://activitypub.software/TransFem-org/Sharkey).

## Helpful Commands

### Apply migrations

```powershell
dotnet ef database update --project ModShark
```

### Create migration

```powershell
dotnet ef migrations add $name --project ModShark
```

### Remove migration (but keep changes)

```powershell
dotnet ef migrations remove --project ModShark
```
