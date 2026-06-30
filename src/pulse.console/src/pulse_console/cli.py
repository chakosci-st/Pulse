from __future__ import annotations

import argparse
from datetime import date, datetime
import logging
from pathlib import Path

from pulse_console.config import load_settings
from pulse_console.logging_utils import setup_logging
from pulse_console.services import (
    normalize_notification_as_of,
    send_milestone_ready_notifications,
    send_notification_emails,
    send_pending_task_summaries,
)


def main() -> int:
    parser = _build_parser()
    args = parser.parse_args()
    logger = setup_logging(args.log_level)

    if not args.command:
        parser.print_help()
        return 1

    try:
        logger.info("Starting command=%s", args.command)
        settings = load_settings(Path(args.env_file) if args.env_file else None)
        if args.command == "pending-tasks-summary":
            report = send_pending_task_summaries(
                settings=settings,
                as_of=_parse_date(args.as_of) if args.as_of else date.today(),
                user_id=args.user_id,
                project_no=args.project_no,
                recipient_override=args.recipient,
                dry_run=args.dry_run,
                force=args.force,
                limit=args.limit,
                logger=logger,
            )
        elif args.command == "notifications-table":
            notification_as_of = normalize_notification_as_of(
                _parse_datetime(args.as_of) if args.as_of else None
            )
            report = send_notification_emails(
                settings=settings,
                as_of=notification_as_of,
                notification_ids=args.notification_id or [],
                recipient_override=args.recipient,
                dry_run=args.dry_run,
                force=args.force,
                include_expired=args.include_expired,
                limit=args.limit,
                logger=logger,
            )
        else:
            report = send_milestone_ready_notifications(
                settings=settings,
                project_no=args.project_no,
                recipient_override=args.recipient,
                dry_run=args.dry_run,
                force=args.force,
                limit=args.limit,
                logger=logger,
            )
    except Exception as exc:
        logger.exception("Command failed: %s", exc)
        return 1

    logger.info(
        "DONE attempted=%s sent=%s skipped=%s",
        report.attempted,
        report.sent,
        report.skipped,
    )
    return 0


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="pulse-console")
    parser.add_argument("--env-file", help="Optional path to a .env file.")
    parser.add_argument(
        "--log-level",
        default="INFO",
        choices=["DEBUG", "INFO", "WARNING", "ERROR"],
        help="Application log level.",
    )

    subparsers = parser.add_subparsers(dest="command")

    pending_parser = subparsers.add_parser(
        "pending-tasks-summary",
        help="Send daily pending task summary emails.",
    )
    pending_parser.add_argument("--as-of", help="Summary date in YYYY-MM-DD format.")
    pending_parser.add_argument("--user-id", help="Filter to a specific recipient user id.")
    pending_parser.add_argument("--project-no", help="Filter to a specific project number.")
    pending_parser.add_argument("--recipient", help="Override recipient email for test sends.")
    pending_parser.add_argument("--limit", type=int, help="Limit the number of recipient summaries processed.")
    pending_parser.add_argument("--dry-run", action="store_true", help="Preview deliveries without sending email.")
    pending_parser.add_argument("--force", action="store_true", help="Ignore saved delivery state and send again.")

    notification_parser = subparsers.add_parser(
        "notifications-table",
        help="Send due notifications from the notifications table.",
    )
    notification_parser.add_argument(
        "--as-of",
        help="Cutoff timestamp in ISO format, for example 2026-04-21T08:00:00.",
    )
    notification_parser.add_argument(
        "--notification-id",
        action="append",
        help="Specific notification id to process. Can be repeated.",
    )
    notification_parser.add_argument("--recipient", help="Override recipient email for test sends.")
    notification_parser.add_argument("--limit", type=int, help="Limit the number of notifications processed.")
    notification_parser.add_argument("--include-expired", action="store_true", help="Include expired notifications.")
    notification_parser.add_argument("--dry-run", action="store_true", help="Preview deliveries without sending email.")
    notification_parser.add_argument("--force", action="store_true", help="Ignore saved delivery state and send again.")

    milestone_ready_parser = subparsers.add_parser(
        "milestone-ready-notifications",
        help="Notify project members and next milestone owners when a milestone's tasks are complete.",
    )
    milestone_ready_parser.add_argument("--project-no", help="Filter to a specific project number.")
    milestone_ready_parser.add_argument("--recipient", help="Override recipient email for test sends.")
    milestone_ready_parser.add_argument("--limit", type=int, help="Limit the number of milestone notifications processed.")
    milestone_ready_parser.add_argument("--dry-run", action="store_true", help="Preview deliveries without sending email.")
    milestone_ready_parser.add_argument("--force", action="store_true", help="Ignore saved delivery state and send again.")

    return parser


def _parse_date(value: str) -> date:
    return date.fromisoformat(value)


def _parse_datetime(value: str) -> datetime:
    parsed = datetime.fromisoformat(value)
    if isinstance(parsed, datetime):
        return parsed
    raise ValueError(f"Invalid datetime value: {value}")
