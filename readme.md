# ModShark - AutoMod for Sharkey instances

ModShark is an automated moderation tool for servers running the [Sharkey Fediverse server](https://activitypub.software/TransFem-org/Sharkey).
It runs as a background tool with direct integration to Sharkey's database and API, offering extended and flexible moderation features. 
With customizable rules and multiple reporting options, ModShark provides a smooth extension to Sharkey's native tooling.

## Basic Design

ModShark is based around an asynchronous, backend processing model.
New objects accumulate into queues (driven by database triggers) until the next scheduled run.
This allows flexible scheduling and burst-mode performance, at the cost of delayed reporting.

When ModShark runs, it processes each queue sequentially.
Objects are read from the queues in batches which are then scanned to detect any flags.
Wherever possible, filters are checked in-database to reduce extraneous data transfers.

[Rules](#Rules) are responsible for the actual scanning logic.
Each rule has a number of filters and common settings, along with a collection of "flags" that can each identify some target characteristic.
At present, all flags are in the form of regular expression lists.
These are compiled at runtime into an optimized scanner, which runs once per object to categorize it.
Any match from any flag will cause the entire rule to match.

All matched objects are collected into a single "report" as the run progresses.
Once all queues have been cleared, the final report is serialized and distributed by the enabled [reporters](#Reporters).
Each reporter may format the report in a different way, so take care to enable the best one(s) for your environment and staff.

## Rules

ModShark "rules" detect and flag objects according to configurable parameters.
Current rules can flag instances, notes (posts), and user profiles.
See the below section for specific documentation.

### Flagged Instance Rule

The **Flagged Instance rule** checks all newly-discovered instances against a set of flags. 
Any flagged instance is included on the report.
Filters are available to exclude instances that have already been actioned by the moderation team, including checks for suspended (delivery stopped), blocked (defederated), and silenced (limited).

This is a queued rule, meaning that it runs on a scheduled interval and scans all instances that have been discovered since the last scan.
New instances are enqueued via database trigger to minimize overhead.

#### Available Flags:

| Flag         | Description                                                                                                                                                                                                               |
|--------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Hostname     | Scans the domain / hostname.                                                                                                                                                                                              |
| Display Name | Scans the human-readable display name, if present.                                                                                                                                                                        |
| Description  | Scans the human-readable description line, if present.<br/>Note: this field may contain santized HTML.                                                                                                                    |
| Contact      | Scans the administrator name / email fields, if present.                                                                                                                                                                  |
| Software     | Scans the **reported** software name / version.<br/>**Note 1: these fields can be spoofed and are frequently outdated.**<br/>**Note 2: due to a Sharkey bug, these fields are only present for Misskey-based instances.** |


### Flagged User Rule

The **Flagged User rule** checks all newly-discovered users against a set of flags.
Any flagged user is included on the report.
Filters are available to exclude users who have already been actioned by the moderation team, including checks for suspended (blocked) and silenced (limited).
Additional filters can exclude local or remote users and users from previously-actioned instances.

This is a queued rule, meaning that it runs on a scheduled interval and scans all users that have been discovered since the last scan.
New users are enqueued via database trigger to minimize overhead.

#### Available Flags:

| Flag         | Description                                                                                                        |
|--------------|--------------------------------------------------------------------------------------------------------------------|
| Username     | Scans the unqualified username, excluding the `@` prefix.                                                          |
| Display Name | Scans the human-readable display name, if present.<br/>Note: this field may contain MFM or Markdown.               |
| Bio Text     | Scans the human-readable bio text, if present.<br/>Note: this field may contain MFM or Markdown.                   |
| Age Range    | Compares the user's listed birthday against a list of flagged ranges.<br/>The age range format is described below. |


#### Age Range Format

Flagged age ranges are defined by a pair of age definitions separated by a dash.
The first age defines the inclusive lower bound, while the second defines the exclusive upper bound.
Each age follows the same format: `#y#m#d`.
"y", "m", and "d", of course, refer to year, month, and day respectively.
All are optional, but each age must include at least one component.

For simplicity, the lower bound can be excluded to match any age below the end point.
The starting age will be inferred as zero years whenever there is *only* a single age, without any dash.

**Examples:**
* Flag ages that are likely fake: `0y - 4y`, `80y - 9999y`
* Flag users under 18: `18y`
* Flag users under 18, with a 1-day margin-of-error: `18y1d`
* Flag users under 18, excluding fake ages: `4y - 18y`


### Flagged Note Rule

The **Flagged Note rule** checks all newly-discovered notes against a set of flags.
Any flagged note is included on the report.
The note's subject (content warning) can also be scanned, which may be desired on instances with stricter moderation standards.
Filters are available to exclude notes by visibility, including unlisted (home timeline), followers-only, and/or private (direct message).
Additional filters can exclude notes by actioned users or from actioned instances.
Finally, a pair of scoping filters can exclude local or remote notes as desired.

This is a queued rule, meaning that it runs on a scheduled interval and scans all notes that have been discovered since the last scan.
New notes are enqueued via database trigger to minimize overhead.

#### Available Flags:

| Flag      | Description                                                                                       |
|-----------|---------------------------------------------------------------------------------------------------|
| Text / CW | Scans the text and CW field, if configured.                                                       |
| Emojis    | Scans the emojis by longcode, if present. <br/>Note: emojis are in `@shortcode@host.name` format. |

## Reporters

ModShark offers a variety of "reporters" to communicate reports in any desired format.
All reporters are optional, and multiple can be enabled simultaneously.
See the sections below for specific documentation.

### Console Reporter

The **Console reporter** is the simplest one - it simply writes the reported objects to ModShark's console output.
If installed under Systemd or a similar service architecture, this output will be captured into the system's native logging system.
The reporter's output is human-readable, but follows a predictable format that can be parsed via Regular Expression.

The console reporter is **on by default**, and should remain active under most environments.
Disabling this reporter can complicate diagnostic and audit tasks.

### SendGrid Reporter

The **SendGrid reporter** provides for email notification via [SendGrid](https://sendgrid.com/en-us).
Emails can be sent from any source address / name, and to any number of recipients.
This reporter's output is formatted for human consumption and produces screen-reader-friendly HTML emails.

As SendGrid is a commercial service, this reporter **requires a valid API key**.
The SendGrid reporter is disabled by default.

### Native Reporter

The **Native reporter** creates reports directly within Sharkey itself.
This reporter provides a closer integration with Sharkey and is ideal for teams accustomed to working with Sharkey's moderation controls.

Two modes are available: **API mode** and **database mode**.
API mode submits reports using a service account, triggering the report workflow including notification emails.
If this is not desired, then database mode can be used to "quietly" insert a report that will not trigger notifications.
Both modes will result in a valid report entry and trigger the "unresolved reports" dashboard message.

The native reporter is **on by default** and uses database mode if not otherwise configured.

### Post Reporter

The **Post reporter** creates an announcement post using a service account.
The post template, visibility, audience, and subject (content warning) can all be configured.
This feature is designed for internal staff notifications, but can be used for public alerts with adjustments to the template.

The template can be any valid post in MFM format, with special "variables" available to insert report contents.
* `$audience`- insert the configured audience as a string of @mentions.
* `$report_body` - contents of the report in a human-readable format.

The post reporter is disabled by default.

## Installing ModShark

These instructions are intended for Linux environments using Systemd, and other platforms may require adjustments to the commands.
Make sure to substitute all variables for their correct values.

1. Create a service account for ModShark: `sudo useradd -s /bin/bash -d /home/modshark -m modshark`
2. Log into the service account: `sudo su - modshark`
3. Download the [latest release package](https://github.com/warriordog/ModShark/releases/latest): `wget -O ModShark-latest.zip https://github.com/warriordog/ModShark/releases/latest/download/ModShark-latest.zip`
4. Extract the release package into a directory: `mkdir ModShark && bsdtar -xvf ModShark-latest.zip -C ModShark`
5. Create the production config file (see the [Configuration section](#Configuration) for details): `nano ModShark/appsettings.Production.json` 
6. Run the latest database migrations: `psql -U $postgres_user -W -d $sharkey_database -a -f ModShark/update-ModShark-migrations.sql`
7. Return to an admin account: `exit`
8. Install the Systemd service: `sudo cp ModShark/modshark.service /etc/systemd/system/modshark.service`
9. Register the service and start it: `sudo systemctl daemon-reload && sudo systemctl enable modshark --now`

## Updating ModShark

These instructions are intended for Linux environments using Systemd, but should be generally applicable to other platforms.
Make sure to substitute all variables for their correct values.

1. Stop the ModShark service, if it's running: `sudo systemctl stop modshark`
2. Log into the ModShark service account: `sudo su - modshark`
3. Download the [latest release package](https://github.com/warriordog/ModShark/releases/latest): `wget -O ModShark-latest.zip https://github.com/warriordog/ModShark/releases/latest/download/ModShark-latest.zip`
4. Extract the release package into your installation directory, overwriting any files: `mkdir ModShark && bsdtar -xvf ModShark-latest.zip -C ModShark`
5. Run the latest database migrations: `psql -U $postgres_user -W -d $sharkey_database -a -f ModShark/update-ModShark-migrations.sql`
6. Return to an admin account: `exit`
7. Start the ModShark service: `sudo systemctl start modshark`

## Removing ModShark

These instructions are intended for Linux environments using Systemd, and other platforms may require adjustments to the commands.
Make sure to substitute all variables for their correct values.

1. Stop the ModShark service, if it's running: `systemctl stop modshark`
2. Disable the service: `systemctl disable modshark`
3. Remove the service file: `rm /etc/systemd/system/modshark.service && systemctl daemon-reload`
4. Revert ModShark's database changes: `psql -U $postgres_user -W -d $sharkey_database -a -f uninstall-ModShark-migrations.sql`
5. Remove ModShark files: `rm -r $modshark_directory`

### System Requirements

* [.NET 8 (or later) Runtime](https://dotnet.microsoft.com/en-us/download)
* [A supported version of Windows, Linux, or macOS](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (Linux is recommended)
* At least 128 MB available RAM (256 MB recommended)
* A functional installation of [Sharkey](https://activitypub.software/TransFem-org/Sharkey)
* Network or localhost connection to Sharkey's backend API
* Network or localhost connection to Sharkey's PostgreSQL database, and a user with read/write permissions

## Configuration

ModShark uses a layered configuration approach that allows for automatic updates without clobbering changes.
The root configuration file is `appsettings.json`, which contains the default value for all options.
You may use this as a reference, but **please do not modify it directly**.
Any changes will be overwritten by updates.

To customize the default configuration, create a **new** file called `appsettings.Production.json`.
Populate this file with the same structure as `appsettings.json`, but include only the properties that you wish to modify.
Objects will be merged; arrays and all other values are replaced.

Tip: You can substitute "Production" for any other value to create environment-specific configurations.
Some common values are `appsettings.Development.json`, `appsettings.Testing.json`, and `appsettings.Staging.json`.
There is also a special `appsettings.Local.json`, which will be loaded as an **additional** layer on top of `appsettings.Development.json`.
This file exists to store local secrets that should not be committed to source control.


### Configuration Properties


| Property                                             | Type     | Description                                                                                                                                                                                                                                                                                                                                        |
|------------------------------------------------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Logging.LogLevel`                                   | Hash     | Sets the minimum log severity.<br/>See this [Microsoft article](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#configure-logging-without-code) for details.<br/><pre><code>{<br/>  "Default": "Warning",<br/>  "ModShark": "Information",<br/>  "Microsoft.Hosting.Lifetime": "Information"<br/>}</code></pre> |
| `ModShark.Postgres.Connection`                       | String   | **Required.** Connection string for the database.<br/><pre><code>"Host=localhost<br/>;Port=5432<br/>;Database=sharkey<br/>;Username=sharkey<br/>;Password=sharkey"</code></pre>                                                                                                                                                                    |
| `ModShark.Postgres.Timeout`                          | Integer  | Maximum time that a query can run before automatically terminating.<br/>Default: `30`                                                                                                                                                                                                                                                              |
| `ModShark.Reporters.Console.Enabled`                 | Boolean  | Whether the [Console reporter](#Console-Reporter) should be used.<br/>Default: `true`                                                                                                                                                                                                                                                              |
| `ModShark.Reporters.Native.Enabled`                  | Boolean  | Whether the [Native reporter](#Native-Reporter) should be used.<br/>Default: `true`                                                                                                                                                                                                                                                                |
| `ModShark.Reporters.Native.UseApi`                   | Boolean  | Whether reports should be sent by API instead of database insert.<br/>Default: `false`                                                                                                                                                                                                                                                             |
| `ModShark.Reporters.Post.Audience`                   | String[] | Array of usernames to be granted access to the post.<br/>Must be in @user@instance.tld format.                                                                                                                                                                                                                                                     |
| `ModShark.Reporters.Post.Enabled`                    | Boolean  | Whether the [Post reporter](#Post-Reporter) should be used.<br/>Default: `false`                                                                                                                                                                                                                                                                   |
| `ModShark.Reporters.Post.LocalOnly`                  | Boolean  | Whether the post should be sent to local users only (defederated).<br/>Required if `ModShark.Reporters.Post.Enabled` is true.<br/>Default: `true`                                                                                                                                                                                                  |
| `ModShark.Reporters.Post.Subject`                    | String   | Subject line / content warning for the post.<br/>Default: `"ModShark Report"`                                                                                                                                                                                                                                                                      |
| `ModShark.Reporters.Post.Template`                   | String   | Template for the post (use variables $audience and $report_body).<br/>Default: `"$report_body"`                                                                                                                                                                                                                                                    |
| `ModShark.Reporters.Post.Visibility`                 | Enum     | Visibility of the report post.<br/>Must be one of `"public"`, `"unlisted"`, `"followers"`, or `"private"`.<br/>Default: `"followers"`                                                                                                                                                                                                              |
| `ModShark.Reporters.SendGrid.ApiKey`                 | String   | SendGrid API key (must have send mail permissions).<br/>Required if `ModShark.Reporters.SendGrid.Enabled` is true.                                                                                                                                                                                                                                 |
| `ModShark.Reporters.SendGrid.Enabled`                | Boolean  | Whether the [SendGrid reporter](#SendGrid-Reporter) should be used.<br/>Default: `false`                                                                                                                                                                                                                                                           |
| `ModShark.Reporters.SendGrid.FromAddress`            | String   | Email address to send reports from.<br/>Required if `ModShark.Reporters.SendGrid.Enabled` is true.                                                                                                                                                                                                                                                 |
| `ModShark.Reporters.SendGrid.FromName`               | String   | Name to associate with the from address.<br/>Required if `ModShark.Reporters.SendGrid.Enabled` is true.<br/>Default: `"ModShark"`                                                                                                                                                                                                                  |
| `ModShark.Reporters.SendGrid.ToAddresses`            | String[] | Array of email addresses to send reports to.<br/>Required if `ModShark.Reporters.SendGrid.Enabled` is true.                                                                                                                                                                                                                                        |
| `ModShark.Rules.FlaggedInstance.BatchLimit`          | Integer  | Maximum number of instances to check at once.<br/>Default: `5000`                                                                                                                                                                                                                                                                                  |
| `ModShark.Rules.FlaggedInstance.ContactPatterns`     | String[] | Array of regular expressions to check against each instance's admin name / email.                                                                                                                                                                                                                                                                  |
| `ModShark.Rules.FlaggedInstance.DescriptionPatterns` | String[] | Array of regular expressions to check against each instance's description.                                                                                                                                                                                                                                                                         |
| `ModShark.Rules.FlaggedInstance.Enabled`             | Boolean  | Whether the [Flagged Instance rule](#Flagged-Instance-Rule) should be executed.<br/>Default: `false`                                                                                                                                                                                                                                               |
| `ModShark.Rules.FlaggedInstance.HostnamePatterns`    | String[] | Array of regular expressions to check against each instance's hostname.                                                                                                                                                                                                                                                                            |
| `ModShark.Rules.FlaggedInstance.IncludeBlocked`      | Boolean  | Whether blocked (defederated) instances should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                                    |
| `ModShark.Rules.FlaggedInstance.IncludeSilenced`     | Boolean  | Whether silenced (limited) instances should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                                       |
| `ModShark.Rules.FlaggedInstance.IncludeSuspended`    | Boolean  | Whether suspended (delivery stopped) instances should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                             |
| `ModShark.Rules.FlaggedInstance.NamePatterns`        | String[] | Array of regular expressions to check against each instance's name.                                                                                                                                                                                                                                                                                |
| `ModShark.Rules.FlaggedInstance.SoftwarePatterns`    | String[] | Array of regular expressions to check against each instance's software name / version.                                                                                                                                                                                                                                                             |
| `ModShark.Rules.FlaggedInstance.Timeout`             | Integer  | Maximum time in milliseconds to spend scanning each instance.<br/>Default: `1000`                                                                                                                                                                                                                                                                  |
| `ModShark.Rules.FlaggedNote.BatchLimit`              | Integer  | Maximum number of notes to check at once.<br/>Default: `5000`                                                                                                                                                                                                                                                                                      |
| `ModShark.Rules.FlaggedNote.EmojiPatterns`           | String[] | Array of regular expressions to check against each note's emoji longcodes.                                                                                                                                                                                                                                                                         |
| `ModShark.Rules.FlaggedNote.Enabled`                 | Boolean  | Whether the [Flagged Note rule](#Flagged-Note-Rule) should be executed.<br/>Default: `false`                                                                                                                                                                                                                                                       |
| `ModShark.Rules.FlaggedNote.IncludeBlockedInstance`  | Boolean  | Whether notes by users from blocked (defederated) instances should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                |
| `ModShark.Rules.FlaggedNote.IncludeCW`               | Boolean  | Whether the subject line / content warning should be scanned.<br/>Default: `true`                                                                                                                                                                                                                                                                  |
| `ModShark.Rules.FlaggedNote.IncludeDeletedUser`      | Boolean  | Whether notes by users marked as deleted should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                                   |
| `ModShark.Rules.FlaggedNote.IncludeFollowersVis`     | Boolean  | Whether followers-only notes should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                                               |
| `ModShark.Rules.FlaggedNote.IncludeLocal`            | Boolean  | Whether local notes should be scanned.<br/>Default: `true`                                                                                                                                                                                                                                                                                         |
| `ModShark.Rules.FlaggedNote.InlcudePrivateVis`       | Boolean  | Whether private (direct message) notes should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                                     |
| `ModShark.Rules.FlaggedNote.IncludeRemote`           | Boolean  | Whether remote notes should be scanned.<br/>Default: `true`                                                                                                                                                                                                                                                                                        |
| `ModShark.Rules.FlaggedNote.IncludeSilencedUser`     | Boolean  | Whether notes by silenced users should be scanned.<br/>Default: `true`                                                                                                                                                                                                                                                                             |
| `ModShark.Rules.FlaggedNote.IncludeSuspendedUser`    | Boolean  | Whether notes by suspended users should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                                           |
| `ModShark.Rules.FlaggedNote.IncludeSilencedInstance` | Boolean  | Whether notes by users from silenced (limited) instances should be scanned.<br/>Default: `true`                                                                                                                                                                                                                                                    |
| `ModShark.Rules.FlaggedNote.IncludeUnlistedVis`      | Boolean  | Whether unlisted (home only) notes should be scanned.<br/>Default: `true`                                                                                                                                                                                                                                                                          |
| `ModShark.Rules.FlaggedNote.TextPatterns`            | String[] | Array of regular expressions to check against each note's body / CW.                                                                                                                                                                                                                                                                               |
| `ModShark.Rules.FlaggedNote.Timeout`                 | Integer  | Maximum time in milliseconds to spend scanning each note.<br/>Default: `1000`                                                                                                                                                                                                                                                                      |
| `ModShark.Rules.FlaggedUser.AgeRanges`               | String[] | Array of age ranges to flag.                                                                                                                                                                                                                                                                                                                       |
| `ModShark.Rules.FlaggedUser.BatchLimit`              | Integer  | Maximum number of users to check at once.<br/>Default: `5000`                                                                                                                                                                                                                                                                                      |
| `ModShark.Rules.FlaggedUser.BioPatterns`             | String[] | Array of regular expressions to check against each user's bio text.                                                                                                                                                                                                                                                                                |
| `ModShark.Rules.FlaggedUser.DisplayNamePatterns`     | String[] | Array of regular expressions to check against each user's display name.                                                                                                                                                                                                                                                                            |
| `ModShark.Rules.FlaggedUser.Enabled`                 | Boolean  | Whether the [Flagged User rule](#Flagged-User-Rule) should be executed.<br/>Default: `false`                                                                                                                                                                                                                                                       |
| `ModShark.Rules.FlaggedUser.IncludeBlockedInstance`  | Boolean  | Whether users from blocked (defederated) instances should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                         |
| `ModShark.Rules.FlaggedUser.IncludeDeleted`          | Boolean  | Whether users who are marked as deleted (but still exist) should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                  |
| `ModShark.Rules.FlaggedUser.IncludeLocal`            | Boolean  | Whether local users should be scanned.<br/>Default: `true`                                                                                                                                                                                                                                                                                         |
| `ModShark.Rules.FlaggedUser.IncludeRemote`           | Boolean  | Whether remote users should be scanned.<br/>Default: `true`                                                                                                                                                                                                                                                                                        |
| `ModShark.Rules.FlaggedUser.IncludeSilenced`         | Boolean  | Whether silenced users should be scanned.<br/>Default: `false`                                                                                                                                                                                                                                                                                     |
| `ModShark.Rules.FlaggedUser.IncludeSilencedInstance` | Boolean  | Whether users from silenced (limited) instances should be scanned.<br/>Default: `true`                                                                                                                                                                                                                                                             |
| `ModShark.Rules.FlaggedUser.Timeout`                 | Integer  | Maximum time in milliseconds to spend scanning each username.<br/>Default: `1000`                                                                                                                                                                                                                                                                  |
| `ModShark.Rules.FlaggedUser.UsernamePatterns`        | String[] | Array of regular expressions to check against each user's username.                                                                                                                                                                                                                                                                                |
| `ModShark.Sharkey.ApiEndpoint`                       | String   | **Required.** URL of the instance's backend API.<br/>Default: `"https://127.0.0.1:3000"`                                                                                                                                                                                                                                                           |
| `ModShark.Sharkey.IdFormat`                          | Enum     | **Required.** ID format used by this instance.<br/>Must be one of `"aid"`, `"aidx"`, `"meid"`, `"meidg"`, `"ulid"`, or `"objectid"`.<br/>Default: `"aidx"`                                                                                                                                                                                         |
| `ModShark.Sharkey.PublicHost`                        | String   | **Required.** Public hostname / domain of the instance.                                                                                                                                                                                                                                                                                            |
| `ModShark.Sharkey.ServiceAccount`                    | String   | Username of ModShark's service account.<br/>Default: `"instance.actor"`                                                                                                                                                                                                                                                                            |
| `ModShark.Worker.PollInterval`                       | Integer  | Time in milliseconds to wait between each run.<br/>Default: `1800000`                                                                                                                                                                                                                                                                              |

