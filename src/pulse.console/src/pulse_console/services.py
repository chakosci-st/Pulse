from __future__ import annotations

from dataclasses import dataclass
from datetime import date, datetime, time
import hashlib
import html
import logging
import re
from typing import Iterable

from pulse_console.config import Settings
from pulse_console.db import fetch_all
from pulse_console.emailer import EmailClient
from pulse_console.state import DeliveryState


PENDING_TASKS_SQL = """
WITH recipient_map AS (
    SELECT pt.projecttasksysid, po.userid AS recipient_user_id
      FROM projecttasks pt
      INNER JOIN projectowners po
         ON po.parentsysid = pt.projecttasksysid
        AND UPPER(po.parenttype) = 'TASK'
        AND po.userid IS NOT NULL
        UNION
    SELECT pt.projecttasksysid, pm.userid AS recipient_user_id
      FROM projecttasks pt
      INNER JOIN projectmembers pm
         ON pm.projectno = pt.projectno
         WHERE pm.userid IS NOT NULL
),
task_due AS (
    SELECT pt.projecttasksysid,
           pt.projectno,
           COALESCE(pt.alttaskname, ra.activityname, 'Task') AS activityname,
           pt.status,
           pt.targetcompletiondate,
           pt.targetcompletionyear,
           pt.targetcompletionworkweek,
           p.projectname,
           p.plantcode,
           pl.plantname,
           cat.categoryname,
              prm.milestonealias,
              pm.milestonesysid
      FROM projecttasks pt
      INNER JOIN projects p
         ON p.projectno = pt.projectno
        LEFT OUTER JOIN roadmapactivities ra
         ON ra.roadmapactivitysysid = pt.roadmapactivitysysid
      INNER JOIN plants pl
         ON pl.plantcode = p.plantcode
      INNER JOIN categories cat
         ON cat.categorycode = p.categorycode
      LEFT OUTER JOIN projectmilestones pm
         ON pt.parenttype = 'MILESTONE'
        AND pm.milestonesysid = pt.parentsysid
        LEFT OUTER JOIN projectroadmapmilestones prm
            ON prm.projectno = pt.projectno
          AND prm.roadmapmilestonesysid = pm.roadmapmilestonesysid
     WHERE NVL(pt.isactive, '1') = '1'
       AND UPPER(NVL(pt.status, 'NOT STARTED')) NOT IN ('CLOSED', 'COMPLETED', 'CANCELLED', 'CANCELED')
       AND (:project_no IS NULL OR pt.projectno = :project_no)
)
SELECT DISTINCT
       rm.recipient_user_id,
       u.email AS recipient_email,
       u.firstname AS recipient_first_name,
       u.lastname AS recipient_last_name,
       td.projecttasksysid,
       td.projectno,
       td.projectname,
       td.plantcode,
       td.plantname,
       td.categoryname,
       td.milestonesysid,
       td.milestonealias,
       td.activityname,
       td.status,
       td.targetcompletiondate,
       td.targetcompletionyear,
       td.targetcompletionworkweek
  FROM task_due td
  INNER JOIN recipient_map rm
     ON rm.projecttasksysid = td.projecttasksysid
  INNER JOIN users u
     ON u.userid = rm.recipient_user_id
 WHERE NVL(u.isactive, '1') = '1'
   AND u.email IS NOT NULL
   AND (:user_id IS NULL OR rm.recipient_user_id = :user_id)
 ORDER BY recipient_email, targetcompletiondate NULLS LAST, projectno, activityname
"""


NOTIFICATIONS_SQL = """
WITH metadata AS (
    SELECT 'SYSTEM' AS entitysysid, 'SYSTEM' AS entitytype, 'PULSE' AS context FROM dual
    UNION ALL
    SELECT plantcode, 'PLANT', plantname FROM plants
    UNION ALL
    SELECT projectno, 'PROJECT', projectname FROM projects
    UNION ALL
        SELECT pm.milestonesysid, 'MILESTONE', prm.milestonealias
      FROM projectmilestones pm
            INNER JOIN projectroadmapmilestones prm
                 ON prm.projectno = pm.projectno
                AND prm.roadmapmilestonesysid = pm.roadmapmilestonesysid
    UNION ALL
    SELECT pt.projecttasksysid, 'TASK', NVL(pt.alttaskname, ra.activityname)
      FROM projecttasks pt
      INNER JOIN roadmapactivities ra
         ON ra.roadmapactivitysysid = pt.roadmapactivitysysid
)
SELECT n.notificationsysid,
       n.entitysysid,
       n.entitytype,
       n.title,
       n.message,
       n.recipients,
       n.notificationdate,
       n.expirydate,
       n.createdby,
       n.createddate,
       u.firstname AS created_first_name,
       u.lastname AS created_last_name,
       md.context AS context_label
  FROM notifications n
  LEFT OUTER JOIN users u
     ON u.userid = n.createdby
  LEFT OUTER JOIN metadata md
     ON md.entitysysid = n.entitysysid
    AND UPPER(md.entitytype) = UPPER(n.entitytype)
 WHERE n.notificationdate <= :as_of
   AND (:include_expired = 1 OR n.expirydate IS NULL OR n.expirydate >= :as_of)
   AND (:notification_id IS NULL OR n.notificationsysid = :notification_id)
 ORDER BY n.notificationdate, n.createddate, n.notificationsysid
"""


MILESTONE_READY_SQL = """
WITH task_tree AS (
    SELECT CONNECT_BY_ROOT pt.parentsysid AS milestonesysid,
           pt.projectno,
           pt.projecttasksysid,
           UPPER(NVL(pt.status, 'NOT STARTED')) AS task_status
      FROM projecttasks pt
     WHERE NVL(pt.isactive, '1') = '1'
     START WITH UPPER(NVL(pt.parenttype, '')) = 'MILESTONE'
     CONNECT BY NOCYCLE PRIOR pt.projecttasksysid = pt.parentsysid
            AND PRIOR pt.projectno = pt.projectno
            AND UPPER(NVL(pt.parenttype, '')) = 'TASK'
),
milestone_task_summary AS (
    SELECT tt.projectno,
           tt.milestonesysid,
           COUNT(*) AS total_task_count,
           SUM(
               CASE
                   WHEN tt.task_status IN ('CLOSED', 'COMPLETED', 'CANCELLED', 'CANCELED') THEN 0
                   ELSE 1
               END
           ) AS open_task_count
      FROM task_tree tt
     GROUP BY tt.projectno, tt.milestonesysid
),
ordered_milestones AS (
    SELECT pm.projectno,
           pm.milestonesysid AS current_milestonesysid,
           UPPER(NVL(pm.status, 'NOT STARTED')) AS current_milestone_status,
           COALESCE(prm.milestonealias, 'Milestone') AS current_milestone_name,
           prm.orderindex AS current_order_index,
           LEAD(pm.milestonesysid) OVER (
               PARTITION BY pm.projectno
               ORDER BY prm.orderindex, pm.milestonesysid
           ) AS next_milestonesysid,
           LEAD(UPPER(NVL(pm.status, 'NOT STARTED'))) OVER (
               PARTITION BY pm.projectno
               ORDER BY prm.orderindex, pm.milestonesysid
           ) AS next_milestone_status,
           LEAD(COALESCE(prm.milestonealias, 'Milestone')) OVER (
               PARTITION BY pm.projectno
               ORDER BY prm.orderindex, pm.milestonesysid
           ) AS next_milestone_name,
           LEAD(prm.orderindex) OVER (
               PARTITION BY pm.projectno
               ORDER BY prm.orderindex, pm.milestonesysid
           ) AS next_order_index
      FROM projectmilestones pm
      INNER JOIN projectroadmapmilestones prm
         ON prm.projectno = pm.projectno
        AND prm.roadmapmilestonesysid = pm.roadmapmilestonesysid
     WHERE NVL(pm.isactive, '1') = '1'
       AND (:project_no IS NULL OR pm.projectno = :project_no)
),
ready_milestones AS (
    SELECT om.projectno,
           om.current_milestonesysid,
           om.current_milestone_name,
           om.current_milestone_status,
           om.current_order_index,
           om.next_milestonesysid,
           om.next_milestone_name,
           om.next_milestone_status,
           om.next_order_index,
           mts.total_task_count
      FROM ordered_milestones om
      INNER JOIN milestone_task_summary mts
         ON mts.projectno = om.projectno
        AND mts.milestonesysid = om.current_milestonesysid
     WHERE mts.total_task_count > 0
       AND mts.open_task_count = 0
       AND om.next_milestonesysid IS NOT NULL
       AND om.next_milestone_status = 'NOT STARTED'
),
recipient_map AS (
    SELECT rm.projectno,
           rm.current_milestonesysid,
           rm.next_milestonesysid,
              pm.userid AS recipient_user_id
      FROM ready_milestones rm
      INNER JOIN projectmembers pm
         ON pm.projectno = rm.projectno
     WHERE pm.userid IS NOT NULL
    UNION
    SELECT rm.projectno,
           rm.current_milestonesysid,
           rm.next_milestonesysid,
           po.userid AS recipient_user_id
      FROM ready_milestones rm
      INNER JOIN projectowners po
         ON po.projectno = rm.projectno
          AND po.parentsysid = rm.current_milestonesysid
          AND UPPER(po.parenttype) = 'MILESTONE'
      WHERE po.userid IS NOT NULL
     UNION
     SELECT rm.projectno,
              rm.current_milestonesysid,
              rm.next_milestonesysid,
              po.userid AS recipient_user_id
        FROM ready_milestones rm
        INNER JOIN projectowners po
            ON po.projectno = rm.projectno
        AND po.parentsysid = rm.next_milestonesysid
        AND UPPER(po.parenttype) = 'MILESTONE'
     WHERE po.userid IS NOT NULL
     UNION
     SELECT rm.projectno,
              rm.current_milestonesysid,
              rm.next_milestonesysid,
              p.createdby AS recipient_user_id
        FROM ready_milestones rm
        INNER JOIN projects p
            ON p.projectno = rm.projectno
      WHERE p.createdby IS NOT NULL
)
SELECT DISTINCT
       rm.recipient_user_id,
       u.email AS recipient_email,
       u.firstname AS recipient_first_name,
       u.lastname AS recipient_last_name,
       ready.projectno,
       p.projectname,
       ready.current_milestonesysid,
       ready.current_milestone_name,
       ready.current_milestone_status,
       ready.current_order_index,
       ready.next_milestonesysid,
       ready.next_milestone_name,
       ready.next_milestone_status,
       ready.next_order_index,
       ready.total_task_count
  FROM ready_milestones ready
  INNER JOIN recipient_map rm
     ON rm.projectno = ready.projectno
    AND rm.current_milestonesysid = ready.current_milestonesysid
    AND rm.next_milestonesysid = ready.next_milestonesysid
  INNER JOIN projects p
     ON p.projectno = ready.projectno
  INNER JOIN users u
     ON u.userid = rm.recipient_user_id
 WHERE NVL(u.isactive, '1') = '1'
   AND u.email IS NOT NULL
 ORDER BY recipient_email, projectno, next_order_index, next_milestone_name
"""


@dataclass(frozen=True)
class PendingTaskRecord:
    recipient_user_id: str
    recipient_email: str
    recipient_first_name: str | None
    recipient_last_name: str | None
    project_task_sysid: str
    project_no: str
    project_name: str | None
    plant_code: str | None
    plant_name: str | None
    category_name: str | None
    milestone_sysid: str | None
    milestone_alias: str | None
    activity_name: str | None
    status: str | None
    target_completion_date: datetime | None
    target_completion_year: int | None
    target_completion_workweek: str | None

    @property
    def recipient_display_name(self) -> str:
        full_name = " ".join(
            part for part in [self.recipient_first_name, self.recipient_last_name] if part
        ).strip()
        return full_name or self.recipient_user_id


@dataclass(frozen=True)
class NotificationRecord:
    notification_sysid: str
    entity_sysid: str | None
    entity_type: str | None
    title: str | None
    message: str | None
    recipients: str | None
    notification_date: datetime | None
    expiry_date: datetime | None
    created_by: str | None
    created_date: datetime | None
    created_first_name: str | None
    created_last_name: str | None
    context_label: str | None

    @property
    def created_by_display_name(self) -> str:
        full_name = " ".join(
            part for part in [self.created_first_name, self.created_last_name] if part
        ).strip()
        return full_name or (self.created_by or "Pulse")


@dataclass(frozen=True)
class MilestoneReadyRecord:
    recipient_user_id: str
    recipient_email: str
    recipient_first_name: str | None
    recipient_last_name: str | None
    project_no: str
    project_name: str | None
    current_milestone_sysid: str
    current_milestone_name: str | None
    current_milestone_status: str | None
    current_order_index: int | None
    next_milestone_sysid: str
    next_milestone_name: str | None
    next_milestone_status: str | None
    next_order_index: int | None
    total_task_count: int | None

    @property
    def recipient_display_name(self) -> str:
        full_name = " ".join(
            part for part in [self.recipient_first_name, self.recipient_last_name] if part
        ).strip()
        return full_name or self.recipient_user_id


@dataclass(frozen=True)
class CommandReport:
    attempted: int
    sent: int
    skipped: int


def send_pending_task_summaries(
    *,
    settings: Settings,
    as_of: date,
    user_id: str | None,
    project_no: str | None,
    recipient_override: str | None,
    dry_run: bool,
    force: bool,
    limit: int | None,
    logger: logging.Logger,
) -> CommandReport:
    rows = fetch_all(
        settings,
        PENDING_TASKS_SQL,
        {
            "user_id": user_id,
            "project_no": project_no,
        },
    )
    records = [_map_pending_task(row) for row in rows]
    grouped = _group_pending_tasks(records)
    recipients = list(grouped.items())
    if limit is not None:
        recipients = recipients[:limit]

    logger.info(
        "Loaded pending task summaries recipients=%s records=%s dry_run=%s force=%s",
        len(recipients),
        len(records),
        dry_run,
        force,
    )

    email_client = EmailClient(settings)

    attempted = 0
    sent = 0
    skipped = 0

    for recipient_email, tasks in recipients:
        attempted += 1
        summary_key = f"pending:{as_of.isoformat()}:{tasks[0].recipient_user_id}:{recipient_email.lower()}"
        payload_hash = _hash_payload(
            "|".join(
                [summary_key]
                + [
                    f"{task.project_task_sysid}:{task.status}:{task.target_completion_date or ''}"
                    for task in tasks
                ]
            )
        )

        subject = f"PULSE: Pending Tasks Summary - {as_of.isoformat()}"
        html_body = _build_pending_summary_html(settings.web_base_url, as_of, tasks)
        target_recipient = recipient_override or recipient_email

        if dry_run:
            logger.info(
                "DRY RUN pending-tasks-summary recipient=%s tasks=%s",
                target_recipient,
                len(tasks),
            )
            continue

        email_client.send_html_email(
            subject=subject,
            html_body=html_body,
            to_addresses=[target_recipient],
        )
        sent += 1
        logger.info(
            "SENT pending-tasks-summary recipient=%s tasks=%s",
            target_recipient,
            len(tasks),
        )

    return CommandReport(attempted=attempted, sent=sent, skipped=skipped)


def send_notification_emails(
    *,
    settings: Settings,
    as_of: datetime,
    notification_ids: list[str],
    recipient_override: str | None,
    dry_run: bool,
    force: bool,
    include_expired: bool,
    limit: int | None,
    logger: logging.Logger,
) -> CommandReport:
    email_client = EmailClient(settings)
    state = DeliveryState(settings.state_db_path)

    records: list[NotificationRecord] = []
    if notification_ids:
        for notification_id in notification_ids:
            rows = fetch_all(
                settings,
                NOTIFICATIONS_SQL,
                {
                    "as_of": as_of,
                    "include_expired": 1 if include_expired else 0,
                    "notification_id": notification_id,
                },
            )
            records.extend(_map_notification(row) for row in rows)
    else:
        rows = fetch_all(
            settings,
            NOTIFICATIONS_SQL,
            {
                "as_of": as_of,
                "include_expired": 1 if include_expired else 0,
                "notification_id": None,
            },
        )
        records = [_map_notification(row) for row in rows]

    if limit is not None:
        records = records[:limit]

    logger.info(
        "Loaded notifications count=%s dry_run=%s force=%s include_expired=%s",
        len(records),
        dry_run,
        force,
        include_expired,
    )

    attempted = 0
    sent = 0
    skipped = 0
    persist_state = not dry_run and not recipient_override

    for record in records:
        recipients = [recipient_override] if recipient_override else _parse_recipients(record.recipients)
        if not recipients:
            skipped += 1
            logger.warning(
                "SKIP notifications-table notification=%s reason=no-recipients",
                record.notification_sysid,
            )
            continue

        for recipient in recipients:
            attempted += 1
            delivery_key = f"notification:{record.notification_sysid}:{recipient.lower()}"
            payload_hash = _hash_payload(
                "|".join(
                    [
                        delivery_key,
                        record.title or "",
                        record.message or "",
                        record.notification_date.isoformat() if record.notification_date else "",
                    ]
                )
            )

            if not force and persist_state and state.has_delivery(delivery_key):
                skipped += 1
                logger.info(
                    "SKIP notifications-table notification=%s recipient=%s reason=already-delivered",
                    record.notification_sysid,
                    recipient,
                )
                continue

            subject = f"PULSE: {record.title or 'Notification'}"
            html_body = _build_notification_html(settings.web_base_url, record)

            if dry_run:
                logger.info(
                    "DRY RUN notifications-table notification=%s recipient=%s",
                    record.notification_sysid,
                    recipient,
                )
                continue

            email_client.send_html_email(
                subject=subject,
                html_body=html_body,
                to_addresses=[recipient],
            )
            sent += 1
            logger.info(
                "SENT notifications-table notification=%s recipient=%s",
                record.notification_sysid,
                recipient,
            )

            if persist_state:
                state.remember_delivery(
                    delivery_key=delivery_key,
                    command_name="notifications-table",
                    recipient=recipient,
                    reference_id=record.notification_sysid,
                    payload_hash=payload_hash,
                )

    return CommandReport(attempted=attempted, sent=sent, skipped=skipped)


def send_milestone_ready_notifications(
    *,
    settings: Settings,
    project_no: str | None,
    recipient_override: str | None,
    dry_run: bool,
    force: bool,
    limit: int | None,
    logger: logging.Logger,
) -> CommandReport:
    rows = fetch_all(
        settings,
        MILESTONE_READY_SQL,
        {
            "project_no": project_no,
        },
    )
    records = [_map_milestone_ready(row) for row in rows]
    if limit is not None:
        records = records[:limit]

    logger.info(
        "Loaded milestone-ready notifications count=%s dry_run=%s force=%s",
        len(records),
        dry_run,
        force,
    )

    email_client = EmailClient(settings)
    state = DeliveryState(settings.state_db_path)

    attempted = 0
    sent = 0
    skipped = 0
    persist_state = not dry_run and not recipient_override

    for record in records:
        attempted += 1
        target_recipient = recipient_override or record.recipient_email
        delivery_key = (
            f"milestone-ready:{record.current_milestone_sysid}:{record.next_milestone_sysid}:"
            f"{target_recipient.lower()}"
        )
        payload_hash = _hash_payload(
            "|".join(
                [
                    delivery_key,
                    record.project_no,
                    record.current_milestone_name or "",
                    record.next_milestone_name or "",
                    str(record.total_task_count or 0),
                ]
            )
        )

        if not force and persist_state and state.has_delivery(delivery_key):
            skipped += 1
            logger.info(
                "SKIP milestone-ready-notifications project=%s next_milestone=%s recipient=%s reason=already-delivered",
                record.project_no,
                record.next_milestone_sysid,
                target_recipient,
            )
            continue

        subject = (
            f"PULSE: Next Milestone Ready - {record.project_no} - "
            f"{record.next_milestone_name or 'Milestone'}"
        )
        html_body = _build_milestone_ready_html(settings.web_base_url, record)

        if dry_run:
            logger.info(
                "DRY RUN milestone-ready-notifications project=%s next_milestone=%s recipient=%s",
                record.project_no,
                record.next_milestone_sysid,
                target_recipient,
            )
            continue

        email_client.send_html_email(
            subject=subject,
            html_body=html_body,
            to_addresses=[target_recipient],
        )
        sent += 1
        logger.info(
            "SENT milestone-ready-notifications project=%s next_milestone=%s recipient=%s",
            record.project_no,
            record.next_milestone_sysid,
            target_recipient,
        )

        if persist_state:
            state.remember_delivery(
                delivery_key=delivery_key,
                command_name="milestone-ready-notifications",
                recipient=target_recipient,
                reference_id=record.next_milestone_sysid,
                payload_hash=payload_hash,
            )

    return CommandReport(attempted=attempted, sent=sent, skipped=skipped)


def _group_pending_tasks(records: Iterable[PendingTaskRecord]) -> dict[str, list[PendingTaskRecord]]:
    grouped: dict[str, list[PendingTaskRecord]] = {}
    for record in records:
        grouped.setdefault(record.recipient_email, []).append(record)
    return grouped


def _map_pending_task(row: dict[str, object]) -> PendingTaskRecord:
    return PendingTaskRecord(
        recipient_user_id=str(row["recipient_user_id"]),
        recipient_email=str(row["recipient_email"]),
        recipient_first_name=_to_optional_str(row.get("recipient_first_name")),
        recipient_last_name=_to_optional_str(row.get("recipient_last_name")),
        project_task_sysid=str(row["projecttasksysid"]),
        project_no=str(row["projectno"]),
        project_name=_to_optional_str(row.get("projectname")),
        plant_code=_to_optional_str(row.get("plantcode")),
        plant_name=_to_optional_str(row.get("plantname")),
        category_name=_to_optional_str(row.get("categoryname")),
        milestone_sysid=_to_optional_str(row.get("milestonesysid")),
        milestone_alias=_to_optional_str(row.get("milestonealias")),
        activity_name=_to_optional_str(row.get("activityname")),
        status=_to_optional_str(row.get("status")),
        target_completion_date=_to_optional_datetime(row.get("targetcompletiondate")),
        target_completion_year=_to_optional_int(row.get("targetcompletionyear")),
        target_completion_workweek=_to_optional_str(row.get("targetcompletionworkweek")),
    )


def _map_notification(row: dict[str, object]) -> NotificationRecord:
    return NotificationRecord(
        notification_sysid=str(row["notificationsysid"]),
        entity_sysid=_to_optional_str(row.get("entitysysid")),
        entity_type=_to_optional_str(row.get("entitytype")),
        title=_to_optional_str(row.get("title")),
        message=_to_optional_str(row.get("message")),
        recipients=_to_optional_str(row.get("recipients")),
        notification_date=_to_optional_datetime(row.get("notificationdate")),
        expiry_date=_to_optional_datetime(row.get("expirydate")),
        created_by=_to_optional_str(row.get("createdby")),
        created_date=_to_optional_datetime(row.get("createddate")),
        created_first_name=_to_optional_str(row.get("created_first_name")),
        created_last_name=_to_optional_str(row.get("created_last_name")),
        context_label=_to_optional_str(row.get("context_label")),
    )


def _map_milestone_ready(row: dict[str, object]) -> MilestoneReadyRecord:
    return MilestoneReadyRecord(
        recipient_user_id=str(row["recipient_user_id"]),
        recipient_email=str(row["recipient_email"]),
        recipient_first_name=_to_optional_str(row.get("recipient_first_name")),
        recipient_last_name=_to_optional_str(row.get("recipient_last_name")),
        project_no=str(row["projectno"]),
        project_name=_to_optional_str(row.get("projectname")),
        current_milestone_sysid=str(row["current_milestonesysid"]),
        current_milestone_name=_to_optional_str(row.get("current_milestone_name")),
        current_milestone_status=_to_optional_str(row.get("current_milestone_status")),
        current_order_index=_to_optional_int(row.get("current_order_index")),
        next_milestone_sysid=str(row["next_milestonesysid"]),
        next_milestone_name=_to_optional_str(row.get("next_milestone_name")),
        next_milestone_status=_to_optional_str(row.get("next_milestone_status")),
        next_order_index=_to_optional_int(row.get("next_order_index")),
        total_task_count=_to_optional_int(row.get("total_task_count")),
    )


def _build_pending_summary_html(web_base_url: str, as_of: date, tasks: list[PendingTaskRecord]) -> str:
    recipient_name = html.escape(tasks[0].recipient_display_name)
    total = len(tasks)
    overdue = sum(
        1
        for task in tasks
        if task.target_completion_date is not None and task.target_completion_date.date() < as_of
    )
    due_today = sum(
        1
        for task in tasks
        if task.target_completion_date is not None and task.target_completion_date.date() == as_of
    )
    no_due_date = sum(1 for task in tasks if task.target_completion_date is None)
    rows = "".join(_build_pending_task_row(web_base_url, task, as_of) for task in tasks)
    content_html = f"""
<p>Dear {recipient_name},</p>
<p>You currently have <strong>{total}</strong> pending task(s) in Pulse.</p>
<p>
    Overdue: <strong>{overdue}</strong><br>
    Due today: <strong>{due_today}</strong><br>
    No due date: <strong>{no_due_date}</strong>
</p>
<table width="100%" cellpadding="8" cellspacing="0" style="border-collapse: collapse; font-size: 13px; margin-top: 20px;">
    <thead>
        <tr style="background: #eef3fb; text-align: left;">
            <th style="border: 1px solid #d8e0ea;">Project</th>
            <th style="border: 1px solid #d8e0ea;">Task</th>
            <th style="border: 1px solid #d8e0ea;">Milestone</th>
            <th style="border: 1px solid #d8e0ea;">Status</th>
            <th style="border: 1px solid #d8e0ea;">Due</th>
        </tr>
    </thead>
    <tbody>
        {rows}
    </tbody>
</table>
<p>Snapshot date: <strong>{html.escape(as_of.isoformat())}</strong></p>
"""
    return _build_email_shell(
        content_html=content_html,
        action_url=f"{web_base_url.rstrip('/')}/projects",
        action_text="Please visit the Pulse to view the complete details.",
    )


def _build_pending_task_row(web_base_url: str, task: PendingTaskRecord, as_of: date) -> str:
    due_label = _format_due_label(task, as_of)
    project_href = f"{web_base_url.rstrip('/')}/projects/{html.escape(task.project_no)}"
    project_text = html.escape(f"{task.project_no} - {task.project_name or ''}".strip(" -"))
    task_name = html.escape(task.activity_name or task.project_task_sysid)
    milestone = html.escape(task.milestone_alias or "-")
    status = html.escape(task.status or "-")
    return (
        "<tr>"
        f"<td style=\"border: 1px solid #d8e0ea;\"><a href=\"{project_href}\" target=\"_blank\">{project_text}</a></td>"
        f"<td style=\"border: 1px solid #d8e0ea;\">{task_name}</td>"
        f"<td style=\"border: 1px solid #d8e0ea;\">{milestone}</td>"
        f"<td style=\"border: 1px solid #d8e0ea;\">{status}</td>"
        f"<td style=\"border: 1px solid #d8e0ea;\">{html.escape(due_label)}</td>"
        "</tr>"
    )


def _build_notification_html(web_base_url: str, record: NotificationRecord) -> str:
    title = html.escape(record.title or "Notification")
    message = html.escape(record.message or "").replace("\n", "<br>")
    context_label = html.escape(record.context_label or record.entity_type or "General")
    created_by = html.escape(record.created_by_display_name)
    notification_date = html.escape(_format_datetime(record.notification_date))
    expiry_date = html.escape(_format_datetime(record.expiry_date)) if record.expiry_date else "-"
    notifications_href = f"{web_base_url.rstrip('/')}/Home/Notifications"
    content_html = f"""
<p>Dear User,</p>
<p><strong>Title:</strong> {title}</p>
<p><strong>Context:</strong> {context_label}</p>
<p><strong>Created by:</strong> {created_by}</p>
<p><strong>Notification date:</strong> {notification_date}</p>
<p><strong>Expiry date:</strong> {expiry_date}</p>
<div style="margin-top: 20px; padding: 16px; background-color: #f8f9fa; border-left: 4px solid #004aad;">
    {message}
</div>
"""
    return _build_email_shell(
        content_html=content_html,
        action_url=notifications_href,
        action_text="Please visit the Pulse to view the complete details.",
    )


def _build_milestone_ready_html(web_base_url: str, record: MilestoneReadyRecord) -> str:
    recipient_name = html.escape(record.recipient_display_name)
    project_text = html.escape(
        f"{record.project_no} - {record.project_name or ''}".strip(" -")
    )
    current_milestone_name = html.escape(record.current_milestone_name or "Current milestone")
    next_milestone_name = html.escape(record.next_milestone_name or "Next milestone")
    current_status = html.escape(record.current_milestone_status or "-")
    next_status = html.escape(record.next_milestone_status or "NOT STARTED")
    total_task_count = record.total_task_count or 0
    project_href = f"{web_base_url.rstrip('/')}/projects/{html.escape(record.project_no)}"
    content_html = f"""
<p>Dear {recipient_name},</p>
<p>
    All <strong>{total_task_count}</strong> task(s) under milestone <strong>{current_milestone_name}</strong>
    for project <strong>{project_text}</strong> are now complete.
</p>
<p>
    The next milestone <strong>{next_milestone_name}</strong> remains in
    <strong>{next_status}</strong> status and can now be started.
</p>
<table width="100%" cellpadding="8" cellspacing="0" style="border-collapse: collapse; font-size: 13px; margin-top: 20px;">
    <thead>
        <tr style="background: #eef3fb; text-align: left;">
            <th style="border: 1px solid #d8e0ea;">Project</th>
            <th style="border: 1px solid #d8e0ea;">Completed milestone</th>
            <th style="border: 1px solid #d8e0ea;">Completed milestone status</th>
            <th style="border: 1px solid #d8e0ea;">Next milestone</th>
            <th style="border: 1px solid #d8e0ea;">Next milestone status</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td style="border: 1px solid #d8e0ea;"><a href="{project_href}" target="_blank">{project_text}</a></td>
            <td style="border: 1px solid #d8e0ea;">{current_milestone_name}</td>
            <td style="border: 1px solid #d8e0ea;">{current_status}</td>
            <td style="border: 1px solid #d8e0ea;">{next_milestone_name}</td>
            <td style="border: 1px solid #d8e0ea;">{next_status}</td>
        </tr>
    </tbody>
</table>
"""
    return _build_email_shell(
        content_html=content_html,
        action_url=project_href,
        action_text="Please visit the Pulse to view the complete details.",
    )


def _build_email_shell(content_html: str, action_url: str, action_text: str) -> str:
    safe_url = html.escape(action_url)
    safe_action_text = html.escape(action_text)
    return f"""
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=us-ascii">
</head>
<body style="font-family: Arial, sans-serif; background-color: #f9f9f9; margin: 0; padding: 0;">
<div>
<table width="100%" cellpadding="0" cellspacing="0" style="background-color: #f9f9f9; padding: 20px 0;">
<tbody>
<tr>
<td align="center">
<table width="600" cellpadding="0" cellspacing="0" style="background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);">
<tbody>
<tr>
<td style="padding: 20px; text-align: center; background-color: #004aad; color: #ffffff; border-top-left-radius: 8px; border-top-right-radius: 8px;">
<h1 style="margin: 0; font-size: 24px;">Notification from P.U.L.S.E.</h1>
</td>
</tr>
<tr>
<td style="padding: 30px; color: #333333; font-size: 16px; line-height: 1.5;">
{content_html}
<p>{safe_action_text} <a href="{safe_url}" target="_blank">Pulse</a>.</p>
</td>
</tr>
<tr>
<td style="padding: 15px 30px; font-size: 12px; color: #777777; text-align: center; border-bottom-left-radius: 8px; border-bottom-right-radius: 8px; background-color: #f0f0f0;">
<p style="margin: 0;">This is a system-generated email. Please do not reply to this message.</p>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
</tbody>
</table>
</div>
</body>
</html>
"""


def _format_due_label(task: PendingTaskRecord, as_of: date) -> str:
    if task.target_completion_date is not None:
        due_date = task.target_completion_date.date()
        if due_date < as_of:
            return f"{due_date.isoformat()} (Overdue)"
        if due_date == as_of:
            return f"{due_date.isoformat()} (Today)"
        return due_date.isoformat()

    if task.target_completion_year and task.target_completion_workweek:
        return f"CW {task.target_completion_workweek} / {task.target_completion_year}"
    return "No due date"


def _format_datetime(value: datetime | None) -> str:
    if value is None:
        return "-"
    return value.strftime("%Y-%m-%d %H:%M")


def _parse_recipients(raw_value: str | None) -> list[str]:
    if not raw_value:
        return []
    recipients = [item.strip() for item in re.split(r"[;,]", raw_value) if item.strip()]
    seen: set[str] = set()
    result: list[str] = []
    for recipient in recipients:
        lowered = recipient.lower()
        if lowered in seen:
            continue
        seen.add(lowered)
        result.append(recipient)
    return result


def _hash_payload(payload: str) -> str:
    return hashlib.sha256(payload.encode("utf-8")).hexdigest()


def _to_optional_str(value: object | None) -> str | None:
    if value is None:
        return None
    text = str(value).strip()
    return text or None


def _to_optional_datetime(value: object | None) -> datetime | None:
    if value is None:
        return None
    if isinstance(value, datetime):
        return value
    return None


def _to_optional_int(value: object | None) -> int | None:
    if value is None:
        return None
    return int(value)


def normalize_notification_as_of(value: datetime | date | None) -> datetime:
    if value is None:
        return datetime.now()
    if isinstance(value, datetime):
        return value
    return datetime.combine(value, time.max)
