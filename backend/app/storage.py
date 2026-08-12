"""Durable document registry.

The original backend kept uploads in a module-level dict, so every restart
orphaned the vectors already sitting in Chroma: the collection still held the
chunks but no endpoint could name the file they came from.

This replaces that with a small SQLite-backed registry plus the extracted text
on disk. SQLite (stdlib, zero extra dependency) gives us atomic writes and
concurrent reads without inventing a locking scheme around a JSON file.
"""

from __future__ import annotations

import sqlite3
import threading
import uuid
from contextlib import contextmanager
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Dict, List, Optional

from .config import settings

_DB_PATH = settings.data_dir / "registry.db"
_TEXT_DIR = settings.data_dir / "texts"
_lock = threading.Lock()

_SCHEMA = """
CREATE TABLE IF NOT EXISTS documents (
    file_id      TEXT PRIMARY KEY,
    file_name    TEXT NOT NULL,
    content_type TEXT NOT NULL DEFAULT '',
    num_chars    INTEGER NOT NULL DEFAULT 0,
    status       TEXT NOT NULL DEFAULT 'uploaded',
    method       TEXT,
    total_chunks INTEGER,
    error        TEXT,
    created_at   TEXT NOT NULL
);
"""


@contextmanager
def _connect():
    conn = sqlite3.connect(_DB_PATH, timeout=15)
    conn.row_factory = sqlite3.Row
    try:
        yield conn
        conn.commit()
    finally:
        conn.close()


def init() -> None:
    _TEXT_DIR.mkdir(parents=True, exist_ok=True)
    with _connect() as conn:
        conn.execute("PRAGMA journal_mode=WAL")
        conn.executescript(_SCHEMA)


def _text_path(file_id: str) -> Path:
    return _TEXT_DIR / f"{file_id}.txt"


def _now() -> str:
    return datetime.now(timezone.utc).isoformat(timespec="seconds")


# ------------------------------------------------------------------- writes --


def create_document(file_name: str, content_type: str, text: str) -> Dict[str, Any]:
    file_id = str(uuid.uuid4())
    _text_path(file_id).write_text(text, encoding="utf-8")

    record = {
        "file_id": file_id,
        "file_name": file_name,
        "content_type": content_type or "",
        "num_chars": len(text),
        "status": "uploaded",
        "method": None,
        "total_chunks": None,
        "error": None,
        "created_at": _now(),
    }

    with _lock, _connect() as conn:
        conn.execute(
            """INSERT INTO documents
               (file_id, file_name, content_type, num_chars, status, method,
                total_chunks, error, created_at)
               VALUES (:file_id, :file_name, :content_type, :num_chars, :status,
                       :method, :total_chunks, :error, :created_at)""",
            record,
        )
    return record


def update_status(
    file_id: str,
    status: str,
    method: Optional[str] = None,
    total_chunks: Optional[int] = None,
    error: Optional[str] = None,
) -> None:
    with _lock, _connect() as conn:
        conn.execute(
            """UPDATE documents
               SET status = ?, method = COALESCE(?, method),
                   total_chunks = COALESCE(?, total_chunks), error = ?
               WHERE file_id = ?""",
            (status, method, total_chunks, error, file_id),
        )


def delete_document(file_id: str) -> None:
    with _lock, _connect() as conn:
        conn.execute("DELETE FROM documents WHERE file_id = ?", (file_id,))
    _text_path(file_id).unlink(missing_ok=True)


# -------------------------------------------------------------------- reads --


def get_document(file_id: str) -> Optional[Dict[str, Any]]:
    with _connect() as conn:
        row = conn.execute("SELECT * FROM documents WHERE file_id = ?", (file_id,)).fetchone()
    return dict(row) if row else None


def list_documents() -> List[Dict[str, Any]]:
    with _connect() as conn:
        rows = conn.execute("SELECT * FROM documents ORDER BY created_at DESC").fetchall()
    return [dict(r) for r in rows]


def get_text(file_id: str) -> str:
    path = _text_path(file_id)
    return path.read_text(encoding="utf-8") if path.exists() else ""


def counts() -> Dict[str, int]:
    with _connect() as conn:
        row = conn.execute(
            """SELECT COUNT(*) AS uploaded,
                      SUM(CASE WHEN status = 'indexed' THEN 1 ELSE 0 END) AS indexed
               FROM documents"""
        ).fetchone()
    return {"uploaded": row["uploaded"] or 0, "indexed": row["indexed"] or 0}
