from __future__ import annotations

from dataclasses import dataclass
import os
from pathlib import Path


PROJECT_ROOT = Path(__file__).resolve().parents[2]


@dataclass(frozen=True)
class Settings:
    db_username: str
    db_password: str
    db_dsn: str
    smtp_host: str
    smtp_port: int
    smtp_from_address: str
    smtp_from_display: str
    smtp_use_tls: bool
    smtp_use_ssl: bool
    web_base_url: str
    state_db_path: Path
    oracle_client_lib_dir: str | None


def load_settings(env_file: Path | None = None) -> Settings:
    env_values = _load_env_file(env_file or PROJECT_ROOT / ".env")

    def get_value(name: str, *, required: bool = True, default: str | None = None) -> str:
        value = os.getenv(name, env_values.get(name, default))
        if required and (value is None or not str(value).strip()):
            raise ValueError(f"Missing required configuration: {name}")
        return "" if value is None else str(value).strip()

    state_db_raw = get_value(
        "PULSE_CONSOLE_STATE_DB",
        required=False,
        default=str(PROJECT_ROOT / ".state" / "pulse-console.sqlite3"),
    )

    state_db_path = Path(state_db_raw)
    if not state_db_path.is_absolute():
        state_db_path = PROJECT_ROOT / state_db_path

    oracle_client_lib_dir = get_value(
        "PULSE_DB_ORACLE_CLIENT_LIB_DIR",
        required=False,
        default="",
    )

    db_username, db_password, db_dsn = _load_database_settings(get_value)

    return Settings(
        db_username=db_username,
        db_password=db_password,
        db_dsn=db_dsn,
        smtp_host=get_value("PULSE_SMTP_HOST"),
        smtp_port=int(get_value("PULSE_SMTP_PORT")),
        smtp_from_address=get_value("PULSE_SMTP_FROM_ADDRESS"),
        smtp_from_display=get_value("PULSE_SMTP_FROM_DISPLAY"),
        smtp_use_tls=_parse_bool(get_value("PULSE_SMTP_USE_TLS", required=False, default="false")),
        smtp_use_ssl=_parse_bool(get_value("PULSE_SMTP_USE_SSL", required=False, default="false")),
        web_base_url=get_value("PULSE_WEB_BASE_URL", required=False, default="http://localhost:58096"),
        state_db_path=state_db_path,
        oracle_client_lib_dir=_empty_to_none(oracle_client_lib_dir),
    )


def _load_env_file(path: Path) -> dict[str, str]:
    if not path.exists():
        return {}

    result: dict[str, str] = {}
    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, value = line.split("=", 1)
        result[key.strip()] = value.strip().strip('"').strip("'")
    return result


def _load_database_settings(get_value: callable) -> tuple[str, str, str]:
    connection_string = _empty_to_none(
        get_value("PULSE_DB_CONNECTION_STRING", required=False, default="")
    )
    if connection_string:
        parts = _parse_connection_string(connection_string)
        return (
            _require_connection_string_value(parts, "user id", "userid"),
            _require_connection_string_value(parts, "password", "pwd"),
            _require_connection_string_value(parts, "data source", "datasource"),
        )

    return (
        get_value("PULSE_DB_USERNAME"),
        get_value("PULSE_DB_PASSWORD"),
        get_value("PULSE_DB_DSN"),
    )


def _parse_connection_string(value: str) -> dict[str, str]:
    parts: dict[str, str] = {}
    for item in value.split(";"):
        segment = item.strip()
        if not segment or "=" not in segment:
            continue
        key, raw_value = segment.split("=", 1)
        parts[key.strip().lower()] = raw_value.strip()
    return parts


def _require_connection_string_value(parts: dict[str, str], *candidate_keys: str) -> str:
    for key in candidate_keys:
        value = parts.get(key)
        if value:
            return value
    joined = ", ".join(candidate_keys)
    raise ValueError(f"Missing {joined} in PULSE_DB_CONNECTION_STRING")


def _parse_bool(value: str) -> bool:
    return value.strip().lower() in {"1", "true", "yes", "y", "on"}


def _empty_to_none(value: str | None) -> str | None:
    if value is None:
        return None
    return value or None
