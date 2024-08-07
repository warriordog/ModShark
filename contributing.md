# Contributing to ModShark

Thank you for your interest in ModShark!
This document contains notes, commands, and other resources to help with local development.

## Development Requirements

For local development, please make sure that your system meets the following requirements:

* [.NET 8 (or later) SDK](https://dotnet.microsoft.com/en-us/download)
* [A supported version of Windows, Linux, or macOS](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (Linux is recommended)
* [Entity Framework Core Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
* 512 MB of available RAM
* Network or localhost connection to a **non-production copy** of Sharkey's PostgreSQL database, and a user with read/write/DDL permissions
* [recommended] a .NET IDE such as [Microsoft Visual Studio](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/)
* [recommended] [Microsoft PowerShell Core](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.4) to execute resource scripts
* [optional] A **non-production** installation of [Sharkey](https://activitypub.software/TransFem-org/Sharkey)
* [optional] Network or localhost connection to Sharkey's backend API

## Development Setup

1. [Fork the ModShark repository](https://github.com/warriordog/ModShark/fork).
2. Clone your personal fork: `git clone https://github.com/$your_username/ModShark`.
3. Create `appsettings.Local.json` and populate according to the [instructions in the readme](readme.md). **Make sure to use a non-production database / instance**!
4. Apply database migrations: `dotnet ef database update --project SharkeyDB --startup-project ModShark`.
5. Open the solution in your editor of choice.

## Contributing Changes

Please contribute changes in the form of Pull Requests (PRs).
If you have followed the setup options above, then you will have a personal fork that can be used to submit PRs through the GitHub UI.
Please ensure that your changes have been committed and pushed to a branch, then click "create pull request" and fill out the form.

A few things to keep in mind:
* If your PR relates to [an Issue](https://github.com/warriordog/ModShark/issues), then please reference it via link or shortcode (`#123` syntax).
* Please limit PRs to a single feature, bug fix, or other change. You can submit multiple pull requests for different features.
* Summarize the change in the PR title, but keep it concise. If necessary, the project maintainers will help draft PR titles.
* The PR body should describe the change in detail. As with titles, the project maintainers can help write this upon request.
* Project maintainers may request clarification or even changes to your PR. Please feel welcome to politely challenge these requests, but maintainers have the final authority to accept or reject any PR. 
* If accepted, your PR may be "squashed" by the project maintainers. This will replace multiple small commits with a single larger one. You will still be credited as the "author" of the squashed commit, but you may need to update your fork against the official main branch.

## Helpful Commands

### Database migrations

These commands all require the [.NET SDK](https://dotnet.microsoft.com/en-us/download) and [Entity Framework Core Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet).

#### Apply database migrations

This will only apply new migrations.
Does nothing if the database is already up-to-date.

```powershell
dotnet ef database update --project SharkeyDB --startup-project ModShark
```

#### Create new database migration

Set or replace `$name` with a name / ID for your new migration.
This does not modify the database or apply any migrations.

```powershell
dotnet ef migrations add $name --project SharkeyDB --startup-project ModShark
```

#### Remove database migration (but keep changes)

This does not modify the database or model, *only* the migrations.

```powershell
dotnet ef migrations remove --project SharkeyDB --startup-project ModShark
```

#### Create migrations bundle

This will produce a SQL file that deploys all missing migrations.
Set or replace `$migrations_file` with the path to save the SQL file.

```powershell
dotnet ef migrations script --idempotent --project SharkeyDB --startup-project ModShark --output $migrations_file
```

#### Reset database (clear all ModShark objects)

This will **remove** all ModShark tables, triggers, and other objects from the database.
Useful for testing.

```powershell
dotnet ef database update 0 --project SharkeyDB --startup-project ModShark
```

### Publishing

#### Produce a release package

This will produce a distributable ZIP archive in `/Release`.
Binaries and migrations are included.
Please ensure that PowerShell Core is installed, and make sure to set the ReleaseVersion correctly in `/Resources/build-prod.ps1`.

```powershell
pwsh ./Resources/build-prod.ps1
```

## Tips and Tricks

### Cutting a Release

1. Update the version numbers in [Directory.Build.props](Directory.Build.props).
2. Commit and push changes.
3. Run `pwsh .\Resources\build-prod.ps1`.
4. Create a new GitHub release with the version number chosen before. Make sure the tag is formated as `v#.#.#` and the title as `Version #.#.#`.
5. Attach the files `ModShark-latest.zip` and `ModShark-#.#.#.zip`.
6. Publish the release.

### Creating a Snapshot

1. Identify the next available snapshot number for the current version.
2. Run `pwsh .\Resources\build-prod.ps1 -VersionSuffix "-snapshot.#"`.
3. Distribute `ModShark-latest.zip` or `ModShark-v#.#.#-snapshot.#.zip`.

### Rebuilding a Snapshot

1. Run `pwsh .\Resources\build-prod.ps1 -VersionSuffix "-snapshot.#" -Overwrite`.
2. Distribute `ModShark-latest.zip` or `ModShark-v#.#.#-snapshot.#.zip`.
