#!/usr/bin/env pwsh

##########################
# ModShark Build Utility #
##########################

# Requirements:
# - PowerShell Core
# - .NET 8+ SDK
# - EF Core Tools 8+
# - Run from the solution root (not this folder)

# Notes:
# - https://learn.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli
# - https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.archive/compress-archive?view=powershell-7.4
# - https://stackoverflow.com/questions/8095638/how-do-i-negate-a-condition-in-powershell
# - https://stackoverflow.com/a/71159216
# - https://github.com/MicrosoftDocs/PowerShell-Docs/issues/4975

param(
    [string]$VersionSuffix,
    [switch]$Overwrite,
    [string]$ReleaseDir = './Release',
    [string]$PublishDir = './Publish',
    [string]$BuildConfig = 'Release',
    [string]$BuildProject = 'ModShark',
    [string]$DBProject = 'SharkeyDB'
);

# Read project version and compute the full release version string.
# - https://stackoverflow.com/questions/36057041/setting-the-version-number-for-net-core-projects
# - https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/select-xml?view=powershell-7.4
# - https://stackoverflow.com/questions/5033955/xpath-select-text-node
$ProjectVersion = (Select-Xml -Path "Directory.Build.props" -XPath "/Project/PropertyGroup/Version/text()").Node.Value
$ReleaseVersion = "$ProjectVersion$VersionSuffix"
Write-Output "Building release version $ReleaseVersion"

# Clean publish directory
if (Test-Path $PublishDir) {
    Remove-Item -Recurse $PublishDir
    New-Item -Path $PublishDir -Type Directory
}

# Create release directory
if (-Not (Test-Path $ReleaseDir)) {
    New-Item -Path $ReleaseDir -Type Directory
}

# Publish build
dotnet clean $BuildProject --configuration $BuildConfig
dotnet publish $BuildProject --configuration $BuildConfig --output $PublishDir -p:UseAppHost=false --version-suffix $VersionSuffix

# Publish migrations
$lastMigration = (Get-ChildItem -Path "./$DBProject/Migrations/" -Exclude 'SharkeyContextModelSnapshot.cs','*.Designer.cs' | Select-Object -ExpandProperty Name -Last 1).Replace('.cs', '');
dotnet ef migrations script --idempotent --no-build --project $DBProject --configuration $BuildConfig --startup-project $BuildProject --output "$PublishDir/uninstall-ModShark-migrations.sql" $lastMigration 0
dotnet ef migrations script --idempotent --no-build --project $DBProject --configuration $BuildConfig --startup-project $BuildProject --output "$PublishDir/update-ModShark-migrations.sql"

# Remove UTF-8 Byte Order Mark - works around a file encoding bug in EF Core Tools.
# - https://stackoverflow.com/a/35454558
# - https://stackoverflow.com/a/32951824
[IO.File]::WriteAllText("$PublishDir/uninstall-ModShark-migrations.sql", [IO.File]::ReadAllText("$PublishDir/uninstall-ModShark-migrations.sql"))
[IO.File]::WriteAllText("$PublishDir/update-ModShark-migrations.sql", [IO.File]::ReadAllText("$PublishDir/update-ModShark-migrations.sql"))

# Publish documentation
Copy-Item readme.md "$PublishDir/readme.md"
Copy-Item license.md "$PublishDir/license.md"
Copy-Item security.md "$PublishDir/security.md"

# Publish metadata
Write-Output $ReleaseVersion > "$PublishDir/version"

# Package release
Compress-Archive -Path "$PublishDir/*" -DestinationPath "$ReleaseDir/ModShark-$ReleaseVersion.zip" -Force:$Overwrite
Copy-Item -Force "$ReleaseDir/ModShark-$ReleaseVersion.zip" "$ReleaseDir/ModShark-latest.zip"