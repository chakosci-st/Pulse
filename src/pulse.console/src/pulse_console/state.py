from __future__ import annotations

from datetime import datetime, timezone
from pathlib import Path
import sqlite3


class DeliveryState:
    def __init__(self, db_path: Path) -> None:
        self._db_path = db_path
        self._db_path.parent.mkdir(parents=True, exist_ok=True)
        self._initialize()

    def has_delivery(self, delivery_key: str) -> bool:
        with self._connect() as connection:
            row = connection.execute(
                "SELECT 1 FROM deliveries WHERE delivery_key = ?",
                (delivery_key,),
            ).fetchone()
            return row is not None

    def remember_delivery(
        self,
        *,
        delivery_key: str,
        command_name: str,
        recipient: str,
        reference_id: str | None,
        payload_hash: str,
    ) -> None:
        with self._connect() as connection:
            connection.execute(
                """
                INSERT OR REPLACE INTO deliveries (
                    delivery_key,
                    command_name,
                    recipient,
                    reference_id,
                    payload_hash,
                    sent_at
                ) VALUES (?, ?, ?, ?, ?, ?)
                """,
                (
                    delivery_key,
                    command_name,
                    recipient,
                    reference_id,
                    payload_hash,
                    datetime.now(timezone.utc).isoformat(),
                ),
            )
            connection.commit()

    def _initialize(self) -> None:
        with self._connect() as connection:
            connection.execute(
                """
                CREATE TABLE IF NOT EXISTS deliveries (
                    delivery_key TEXT PRIMARY KEY,
                    command_name TEXT NOT NULL,
                    recipient TEXT NOT NULL,
                    reference_id TEXT,
                    payload_hash TEXT NOT NULL,
                    sent_at TEXT NOT NULL
                )
                """
            )
            connection.commit()

    def _connect(self) -> sqlite3.Connection:
        return sqlite3.connect(self._db_path)
