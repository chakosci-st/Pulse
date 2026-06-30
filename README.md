# Pulse

Pulse is a project, roadmap, task, and collaboration management platform. It supports project registration, execution tracking, task updates, milestone management, roadmap governance, template setup, reporting, notifications, attachments, chat, and role-based access.

The application is primarily a classic ASP.NET MVC / ASP.NET Web API solution backed by Oracle. The repository also includes a Vite + React migration workspace, a Python operational console for notification/email workflows, database schema artifacts, and supporting utilities.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Repository Structure](#repository-structure)
- [Applications and Components](#applications-and-components)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Database](#database)
- [Build and Run](#build-and-run)
- [React Workspace](#react-workspace)
- [Python Console](#python-console)
- [Testing and Validation](#testing-and-validation)
- [Documentation](#documentation)
- [Security Notes](#security-notes)
- [Development Workflow](#development-workflow)
- [Troubleshooting](#troubleshooting)
- [Current Status Notes](#current-status-notes)
- [Contributing](#contributing)
- [License](#license)

## Overview

Pulse is designed for managing project execution and visibility across multiple user roles, including:

- Daily users / task owners
- Project owners
- Project members
- Template managers
- Administrators
- Report viewers

The system includes workflows for:

- Creating and maintaining projects
- Managing project roadmaps and milestones
- Assigning and updating tasks
- Tracking project execution
- Managing project members
- Maintaining templates and forms
- Uploading and downloading attachments
- Sending project and task notifications
- Supporting chat/collaboration flows
- Reporting on project and task status
- Enforcing role-based access and visibility

## Features

- Project registration and maintenance
- Roadmap and milestone tracking
- Project task management
- Project member management
- Template and form configuration
- Field and metadata management
- File attachments
- Notification delivery
- Chat and collaboration features
- Admin user/module/access management
- Oracle-backed persistence
- ASP.NET MVC web interface
- ASP.NET Web API backend
- React migration workspace
- Python-based notification console
- JWT key generation utility

## Technology Stack

### Backend and Web

- .NET Framework 4.7.2
- ASP.NET MVC 5
- ASP.NET Web API
- ASP.NET SignalR
- Autofac
- OWIN
- JWT bearer authentication
- Cookie authentication
- log4net

### Data

- Oracle Database
- Oracle Managed Data Access
- Dapper
- Dapper.Oracle

### Frontend

- Razor views
- jQuery
- Bootstrap
- DataTables
- Vite
- React
- TypeScript

### Operations / Utilities

- Python 3.11+
- python-oracledb
- SMTP email delivery
- Local SQLite state for selected console workflows

## Repository Structure

```text
docs/
  github-guide.html
  Pulse_User_Acceptance_Test_Scenarios.md
  uat.html

scripts/

src/
  Pulse.sln

  Pulse.Web/
    ASP.NET MVC web application

  Pulse.Api/
    ASP.NET Web API application

  Pulse.Auth.Identity/
    Authentication and identity components

  Pulse.Core/
    Core application/domain logic

  Pulse.Services/
    Application service layer

  Pulse.Infrastructure/
    Data access, infrastructure, and Oracle/Dapper integration

  Pulse.SharedUtilities/
    Shared helper and utility code

  Pulse.ViewModels/
    View model types used by the web layer

  Pulse.DataTransformationObjects/
    DTOs used across application boundaries

  Pulse.EventHandlers/
    Event handling components

  Pulse.MediaWiki/
    MediaWiki integration components

  Pulse.Prompt/
    Prompt-related components

  pulse.react/
    Vite + React + TypeScript migration workspace

  pulse.console/
    Python operational console for notification/email workflows

  JwtKeyGenerator/
    Utility for generating JWT signing keys

  TableSchema/
    Oracle schema artifacts

tests/
  Reserved for tests
```

## Applications and Components

### Pulse.Web

`Pulse.Web` is the classic ASP.NET MVC web application.

It includes MVC areas and controllers for functionality such as:

- Projects
- Project tasks
- Project milestones
- Project members
- Project fields
- Project chat
- Roadmaps
- Templates
- Settings
- Sites
- Admin users
- Modules
- Access roles
- Operations

The web application uses:

- Cookie authentication
- Autofac dependency injection
- SignalR
- Razor views
- jQuery / Bootstrap / DataTables

Default local IIS Express URL:

```text
http://localhost:58096/
```

### Pulse.Api

`Pulse.Api` is the ASP.NET Web API host.

It exposes API endpoints for areas such as:

- Projects
- Project tasks
- Notifications
- Project attachments
- Files
- Chat
- Plants
- Roadmaps
- Forms
- Fields
- Users
- Modules
- Active Directory integration

The API uses:

- JWT bearer authentication
- CORS configuration
- JSON camel-case formatting
- Standard Web API routing

Default local IIS Express URL:

```text
http://localhost:51675/
```

Default route pattern:

```text
api/{controller}/{id}
```

### Pulse.Infrastructure

`Pulse.Infrastructure` contains infrastructure and data access code.

The main persistence stack is:

- Oracle Database
- Oracle Managed Data Access
- Dapper
- Dapper.Oracle

This project contains the central Oracle data access layer used by the application services.

### Pulse.Services

`Pulse.Services` contains application service logic used by the web and API layers.

### Pulse.Core

`Pulse.Core` contains core application logic and domain-related types.

### Pulse.Auth.Identity

`Pulse.Auth.Identity` contains authentication and identity-related components.

### Pulse.SharedUtilities

`Pulse.SharedUtilities` contains utility code shared across projects.

### Pulse.ViewModels

`Pulse.ViewModels` contains view models used by MVC views and controllers.

### Pulse.DataTransformationObjects

`Pulse.DataTransformationObjects` contains DTOs used for transferring data between layers and application boundaries.

### pulse.react

`pulse.react` is a Vite + React + TypeScript migration workspace.

It is intended to provide a modern frontend foundation for Pulse, but it should be treated as an active migration area rather than a complete replacement for every legacy MVC workflow.

Known migration-sensitive areas include:

- DataTables-heavy pages
- Razor partial composition
- Classic ASP.NET SignalR chat
- File upload/download flows

### pulse.console

`pulse.console` is a Python operational console for email and notification workflows.

It supports workflows such as:

- Pending task summaries
- Notification table delivery
- Milestone-ready notifications

Some workflows use local SQLite state to avoid duplicate sends.

### JwtKeyGenerator

`JwtKeyGenerator` is a small .NET Framework console utility that generates a base64 JWT signing key.

## Prerequisites

### Required for .NET Development

- Windows
- Visual Studio 2017 or newer
- .NET Framework 4.7.2 Developer Pack
- IIS Express or local IIS
- NuGet package restore enabled
- Access to an Oracle database
- Oracle client/runtime support appropriate for the target environment

### Required for React Development

- Node.js
- npm

### Required for Python Console Development

- Python 3.11 or newer
- pip
- Oracle database connectivity
- SMTP access for email delivery workflows

## Configuration

Pulse depends on environment-specific configuration for database access, authentication, SMTP, file storage, JWT settings, CORS, and application URLs.

Use the template/example files as the starting point for local configuration:

```text
src/Pulse.Web/Template.Web.config
src/Pulse.Api/Template.Web.config
src/pulse.react/.env.example
src/pulse.console/.env.example
```

Create local configuration files from these templates and fill in values for your environment.

Do not commit real secrets, connection strings, JWT keys, certificate passwords, SMTP credentials, or internal environment URLs.

### Common Web/API Configuration Areas

The ASP.NET applications require values for:

- Oracle connection string
- JWT issuer
- JWT audience
- JWT signing secret
- CORS origins
- SMTP host and port
- SMTP sender
- LDAP / Active Directory settings
- Web root URL
- API root URL
- File attachment paths
- Profile photo paths
- Upload limits
- Allowed file extensions
- Logging paths
- Optional external ticketing/service URLs

### React Environment Variables

The React workspace uses Vite environment variables.

Typical values are based on:

```text
src/pulse.react/.env.example
```

Expected variables include:

```text
VITE_APP_NAME
VITE_APP_ROOT
VITE_AUTH_ROOT
VITE_API_ROOT
VITE_WEB_PROXY_TARGET
VITE_API_PROXY_TARGET
```

### Python Console Environment Variables

The Python console uses values based on:

```text
src/pulse.console/.env.example
```

Typical settings include:

```text
PULSE_DB_CONNECTION_STRING
PULSE_DB_USER
PULSE_DB_PASSWORD
PULSE_DB_DSN
PULSE_SMTP_HOST
PULSE_SMTP_PORT
PULSE_SMTP_FROM
PULSE_WEB_BASE_URL
PULSE_STATE_DB_PATH
PULSE_ORACLE_CLIENT_LIB_DIR
```

## Database

Pulse uses Oracle as its primary database.

A schema artifact is available at:

```text
src/TableSchema/tables.sql
```

The schema includes tables for:

- Projects
- Roadmaps
- Tasks
- Forms
- Notifications
- Attachments
- Chat
- Plants
- Users
- Access groups
- Supporting lookup/configuration data

Before running the application locally, make sure the configured Oracle database has the required schema and data for your environment.

## Build and Run

### Build the Main Solution

Open the solution in Visual Studio:

```text
src/Pulse.sln
```

Restore NuGet packages, then build the solution.

Because this repository uses classic .NET Framework project files, Visual Studio/MSBuild is the recommended build path.

### Run Pulse.Web

1. Open `src/Pulse.sln` in Visual Studio.
2. Set `Pulse.Web` as the startup project.
3. Confirm local `Web.config` values are configured.
4. Start the project with IIS Express.

Default local URL:

```text
http://localhost:58096/
```

### Run Pulse.Api

1. Open `src/Pulse.sln` in Visual Studio.
2. Set `Pulse.Api` as the startup project.
3. Confirm local `Web.config` values are configured.
4. Start the project with IIS Express.

Default local URL:

```text
http://localhost:51675/
```

### Generate a JWT Signing Key

Use the `JwtKeyGenerator` utility when a local JWT signing key is needed.

From Visual Studio, run:

```text
src/JwtKeyGenerator/JwtKeyGenerator.csproj
```

Use the generated base64 value as the JWT signing secret in local configuration.

Do not commit generated secrets.

## React Workspace

The React workspace lives in:

```text
src/pulse.react
```

Install dependencies:

```bash
cd src/pulse.react
npm install
```

Create a local environment file from the example:

```powershell
Copy-Item .env.example .env
```

Run the development server:

```bash
npm run dev
```

Build the React app:

```bash
npm run build
```

Preview the production build:

```bash
npm run preview
```

The Vite dev server uses port `5173` by default.

The Vite config proxies routes such as:

```text
/api
/auth
/Account
/signalr
/files
```

Set proxy targets in `.env` to match your local `Pulse.Web` and `Pulse.Api` URLs.

## Python Console

The Python console lives in:

```text
src/pulse.console
```

Create and activate a virtual environment:

```powershell
cd src/pulse.console
python -m venv .venv
.venv\Scripts\Activate.ps1
```

Install the package:

```bash
pip install -e .
```

Create local configuration:

```powershell
Copy-Item .env.example .env
```

Then update `.env` with local Oracle, SMTP, and application URL settings.

The console supports notification and email delivery workflows. See the console README for command-specific usage:

```text
src/pulse.console/README.md
```

## Testing and Validation

No automated test projects were found in the current repository structure.

Recommended validation steps are:

### .NET Solution

- Restore NuGet packages
- Build `src/Pulse.sln` in Visual Studio
- Start `Pulse.Web` locally
- Start `Pulse.Api` locally
- Confirm login/authentication behavior
- Confirm API connectivity
- Confirm Oracle connectivity

### React Workspace

```bash
cd src/pulse.react
npm run build
```

Optionally run the dev server and smoke test the migrated routes:

```bash
npm run dev
```

### Python Console

- Create a local `.env`
- Verify Oracle connectivity
- Verify SMTP settings
- Run commands first in a safe/non-production environment
- Confirm duplicate-send protection behavior for workflows that use local state

### Manual UAT

Manual UAT scenarios are documented under:

```text
docs/Pulse_User_Acceptance_Test_Scenarios.md
```

These scenarios cover expected behavior for project users, project owners, template managers, administrators, and report viewers.

## Documentation

Documentation and supporting material can be found in:

```text
docs/
src/pulse.react/README.md
src/pulse.console/README.md
```

Important documents include:

- User acceptance test scenarios
- GitHub/repository guidance
- React migration notes
- Python console operating notes

## Security Notes

- Never commit real `Web.config` secrets.
- Never commit production connection strings.
- Never commit JWT signing keys.
- Never commit certificate passwords.
- Never commit SMTP credentials.
- Rotate any credential that is accidentally committed.
- Keep local, test, staging, and production secrets separate.
- Restrict CORS origins to trusted URLs.
- Review upload limits and allowed extensions before deployment.
- Validate file storage paths and permissions.
- Ensure logs do not expose sensitive data.
- Use environment-specific configuration for authentication, database, and SMTP settings.

## Development Workflow

Recommended workflow:

1. Create a feature branch.
2. Pull the latest changes from the main branch.
3. Restore NuGet/npm/Python dependencies as needed.
4. Make focused changes.
5. Build affected projects.
6. Run available validation checks.
7. Update documentation when setup, behavior, or configuration changes.
8. Open a pull request with a clear description and validation notes.

Avoid committing:

- Local config files with secrets
- Build outputs
- Generated packages
- Local logs
- Local SQLite state files
- IDE-specific temporary files
- Environment-specific credentials

## Troubleshooting

### NuGet packages are missing

Restore NuGet packages from Visual Studio before building the solution.

### .NET Framework targeting pack is missing

Install the .NET Framework 4.7.2 Developer Pack, then reload the solution.

### Oracle connection fails

Check:

- Oracle connection string
- Database host/service name
- User credentials
- Network/VPN access
- Oracle client/runtime requirements
- Database schema availability

### API calls fail from React dev server

Check:

- `VITE_API_ROOT`
- `VITE_WEB_PROXY_TARGET`
- `VITE_API_PROXY_TARGET`
- API local URL
- CORS origins
- JWT settings
- Whether `Pulse.Api` and/or `Pulse.Web` are running

### Authentication fails

Check:

- JWT issuer/audience/secret
- Cookie auth settings
- SSO settings
- LDAP / Active Directory settings
- Local environment URLs
- Clock/time synchronization between systems

### File uploads fail

Check:

- Attachment path configuration
- Folder permissions
- Upload size limits
- Allowed file extensions
- API/web configuration consistency

### Emails are not sent

Check:

- SMTP host
- SMTP port
- SMTP sender
- SMTP credentials, if required
- Network access to SMTP server
- Console `.env` values
- Application notification data

## Current Status Notes

- The primary production-style application is the ASP.NET MVC/Web API solution.
- The React workspace is an active migration foundation and may not yet provide full functional parity with the MVC application.
- No automated test projects were found in the current repository structure.
- Manual UAT documentation is available under `docs/`.
- Configuration templates exist, but local environment values must be supplied by each developer or deployment environment.

## Contributing

Contributions should keep changes focused and consistent with the existing architecture.

Before opening a pull request:

- Build affected .NET projects.
- Run React build checks if frontend migration code changed.
- Validate Python console behavior if operational scripts changed.
- Update documentation for setup or behavior changes.
- Confirm no secrets or local environment values are included.

Suggested pull request checklist:

```text
[ ] Change is focused and scoped
[ ] Solution or affected projects build successfully
[ ] React build passes, if applicable
[ ] Python console changes were validated, if applicable
[ ] Configuration/documentation updates are included, if needed
[ ] No secrets or local config values are committed
```

## License

No license needed.
