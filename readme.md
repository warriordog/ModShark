# ModShark - AutoMod for Sharkey instances

ModShark is a standalone moderation tool for servers running the [Sharkey fediverse server](https://activitypub.software/TransFem-org/Sharkey).

**⚠️ ModShark is work-in-progress software and not ready for production use. ⚠️**

## Rules

Rules detect flagged behavior according to user-configured parameters.
See each subsection for detailed instructions.

### Flagged User - detect and alert when any new user matches a flag

Currently, the only supported flag is Regular Expression matching against username.
Filters and exclusions are supported.
Complete documentation TBD.

### Flagged Instance - detect and alert when any new instance matches a flag

Currently, the only supported flag is Regular Expression matching against hostname.
Filters and exclusions are supported.
Complete documentation TBD.

## Reporters

Various "reporters" are available to communicate alerts in whatever format is desired.
Multiple reporters can be active at once.

### Console - log to the system log via console output

Documentation TBD.

### SendGrid - send a notification email via the SendGrid API

Requires a valid SendGrid subscription.
Documentation TBD.

### Native - create a native Sharkey report

Documentation TBD.

### Post - create a post from a Sharkey user account

Documentation TBD.

## Installation

Instructions are coming soon, but for now just contact the author or open an issue for assistance.

### System Requirements

* [.NET 8 (or later) Runtime](https://dotnet.microsoft.com/en-us/download)
* [A supported version of Windows, Linux, or macOS](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (Linux is recommended)
* At least 128 MB available RAM (256 MB recommended)
* A functional installation of [Sharkey](https://activitypub.software/TransFem-org/Sharkey)
* Network or localhost connection to Sharkey's backend API
* Network or localhost connection to Sharkey's PostgreSQL database, and a user with read/write permissions

## Configuration

Full documentation is coming soon.
You may review the `appsettings.json`, `appsettings.Production.json`, and `appsettings.Development.json` files for example configurations.
Local development will load an option `appsettings.Local.json` file that is automatically excluded from git.
