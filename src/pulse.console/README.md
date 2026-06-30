# pulse.console

Python console project for operational email delivery tasks in Pulse.

It supports three commands:

- `pending-tasks-summary`: sends one daily summary email per recipient for open tasks.
- `notifications-table`: sends due notifications from the `NOTIFICATIONS` table.
- `milestone-ready-notifications`: sends an email when all tasks under a milestone are complete and the next milestone is still not started.

## Setup

1. Create a virtual environment.
2. Install the project in editable mode.
3. Copy `.env.example` to `.env` and fill in the Oracle and SMTP values.

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install -e .
```

## Configuration

The CLI reads settings from environment variables and, by default, from `.env` in the project root.

Required values:

- `PULSE_DB_CONNECTION_STRING` or the split `PULSE_DB_USERNAME` / `PULSE_DB_PASSWORD` / `PULSE_DB_DSN` values
- `PULSE_SMTP_HOST`
- `PULSE_SMTP_PORT`
- `PULSE_SMTP_FROM_ADDRESS`
- `PULSE_SMTP_FROM_DISPLAY`

Optional values:

- `PULSE_SMTP_USE_TLS`
- `PULSE_SMTP_USE_SSL`
- `PULSE_WEB_BASE_URL`
- `PULSE_CONSOLE_STATE_DB`
- `PULSE_DB_ORACLE_CLIENT_LIB_DIR`

Sample URL configuration:

- `PULSE_WEB_BASE_URL=http://pulse.qa.cal.st.com`

SMTP is configured for anonymous relay. The console app does not attempt username/password authentication.

## Usage

Daily pending-task summary:

```powershell
python -m pulse_console pending-tasks-summary --dry-run
python -m pulse_console pending-tasks-summary --as-of 2026-04-21
python -m pulse_console pending-tasks-summary --user-id 123456 --force
```

Notifications table delivery:

```powershell
python -m pulse_console notifications-table --dry-run
python -m pulse_console notifications-table --notification-id 9E35... --force
python -m pulse_console notifications-table --recipient test@company.com
```

Milestone-ready notifications:

```powershell
python -m pulse_console milestone-ready-notifications --dry-run
python -m pulse_console milestone-ready-notifications --project-no P1C7-2026-000130
python -m pulse_console milestone-ready-notifications --recipient test@company.com --force
```

## Delivery State

The project keeps a local SQLite state database so repeated runs do not resend the same milestone-ready and notifications-table messages.

- Notification emails are keyed by notification id and recipient.
- Milestone-ready emails are keyed by current milestone, next milestone, and recipient.

Pending task summaries intentionally send on every execution.

When `--recipient` is used as a test override, the run does not persist delivery state.

## Logging

The console app writes application logs to `logs/pulse-console.log` using rotating files.

- Default log level is `INFO`
- Override per run with `--log-level DEBUG`
- Scheduler scripts still keep their own command log files in `logs/`

## Windows Task Scheduler

Runner scripts are included under `scripts/`:

- `scripts/run-pending-tasks-summary.ps1`
- `scripts/run-notifications-table.ps1`
- `scripts/run-milestone-ready-notifications.ps1`
- `scripts/run-pulse-jobs.bat`
- `scripts/deploy-pulse-console.bat`

Current scheduled plan:

- `Pulse-PendingTasksSummary`: daily at `08:00`
- `Pulse-NotificationsTable`: every `30` minutes starting at `08:00`
- `Pulse-MilestoneReadyNotifications`: every `30` minutes starting at `08:00`

Both scripts write logs to the local `logs/` folder inside `pulse.console`.

For manual execution from Explorer or `cmd`, use:

- `scripts\run-pulse-jobs.bat both`
- `scripts\run-pulse-jobs.bat all`
- `scripts\run-pulse-jobs.bat pending`
- `scripts\run-pulse-jobs.bat notifications`
- `scripts\run-pulse-jobs.bat milestone-ready`

The runner scripts first look for Python in the workspace root `.venv`, then fall back to `pulse.console\.venv` if you later create a project-local environment.

## Deployment

For a fresh Windows deployment, copy the `pulse.console` folder to the target machine and run:

```powershell
Set-Location C:\Apps\pulse.console
.\scripts\deploy-pulse-console.ps1
```

From Explorer or `cmd`, you can also use:

- `scripts\deploy-pulse-console.bat`
- `scripts\deploy-pulse-console.bat deploy-and-register`

That script will:

- create a project-local `.venv` if needed
- install the package with `pip install .`
- create `logs/` and `.state/`
- create `.env` from `.env.example` if missing

To deploy and register the scheduled tasks in one step:

```powershell
.\scripts\deploy-pulse-console.ps1 -RegisterTasks
```

To register the scheduled tasks separately:

```powershell
.\scripts\register-scheduled-tasks.ps1
```

The registration script creates:

- `Pulse-PendingTasksSummary`: daily at `08:00`
- `Pulse-NotificationsTable`: every `30` minutes starting at `08:00`
- `Pulse-MilestoneReadyNotifications`: every `30` minutes starting at `08:00`

## Oracle Notes

The database adapter materializes Oracle `LOB` values while the connection is still open, which avoids the `DPY-1001: not connected to database` failure when `notifications-table` reads the `MESSAGE` CLOB.
