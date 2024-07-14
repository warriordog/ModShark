# [Production Build Script]
# This is a placeholder until proper CI/CD pipelines are built.

# Requirements:
# * Powershell
# * .NET SDK
# * EF Core Tools
# * Run from the solution root (not this folder)

# Notes:
# * https://learn.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli
# * https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.archive/compress-archive?view=powershell-7.4
# * https://stackoverflow.com/questions/8095638/how-do-i-negate-a-condition-in-powershell
# * https://stackoverflow.com/a/71159216

$ReleaseVersion = '1.0.0-snapshot.0'
$ReleaseDir = './Release'
$PublishDir = './Publish'
$BuildConfig = 'Release'
$BuildProject = 'ModShark'

# Clean publish directory
if (Test-Path $PublishDir) {
    rm -Recurse $PublishDir
    mkdir $PublishDir
}

# Create release directory
if (-Not (Test-Path $ReleaseDir)) {
    mkdir $ReleaseDir
}

# Publish build
dotnet clean $BuildProject --configuration $BuildConfig
dotnet publish $BuildProject --configuration $BuildConfig --output $PublishDir -p:UseAppHost=false
dotnet ef migrations script --idempotent --project SharkeyDB --startup-project $BuildProject --output "$PublishDir/ModShark-migrations.sql"

# Package build
# Intentionally do *not* -Force in case someone forgets to update the release version.
Compress-Archive -Path "$PublishDir/*" -DestinationPath "$ReleaseDir/ModShark-$ReleaseVersion.zip"