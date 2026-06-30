from __future__ import annotations

from contextlib import contextmanager
from datetime import datetime
from decimal import Decimal
from typing import Any, Iterator

import oracledb

from pulse_console.config import Settings


_ORACLE_INITIALIZED = False


def initialize_oracle_client(settings: Settings) -> None:
    global _ORACLE_INITIALIZED
    if _ORACLE_INITIALIZED or not settings.oracle_client_lib_dir:
        return

    oracledb.init_oracle_client(lib_dir=settings.oracle_client_lib_dir)
    _ORACLE_INITIALIZED = True


@contextmanager
def open_connection(settings: Settings) -> Iterator[oracledb.Connection]:
    initialize_oracle_client(settings)
    connection = oracledb.connect(
        user=settings.db_username,
        password=settings.db_password,
        dsn=settings.db_dsn,
    )
    try:
        yield connection
    finally:
        connection.close()


def fetch_all(settings: Settings, sql: str, params: dict[str, Any] | None = None) -> list[dict[str, Any]]:
    with open_connection(settings) as connection:
        with connection.cursor() as cursor:
            cursor.execute(sql, params or {})
            columns = [column[0].lower() for column in cursor.description]
            rows = cursor.fetchall()
            return [_normalize_row(columns, row) for row in rows]


def _normalize_row(columns: list[str], row: tuple[Any, ...]) -> dict[str, Any]:
    result: dict[str, Any] = {}
    for index, value in enumerate(row):
        if isinstance(value, Decimal):
            if value == value.to_integral_value():
                result[columns[index]] = int(value)
            else:
                result[columns[index]] = float(value)
            continue

        if isinstance(value, datetime):
            result[columns[index]] = value
            continue

        if isinstance(value, oracledb.LOB):
            lob_value = value.read()
            if isinstance(lob_value, str):
                result[columns[index]] = lob_value.strip()
            else:
                result[columns[index]] = lob_value
            continue

        result[columns[index]] = value
    return result
