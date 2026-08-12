"""Chroma-backed vector store.

Changes vs. the original:
- The client/collection are created lazily on first use instead of at import
  time, so the process starts fast and importing the module never touches disk.
- The persistence directory is absolute (was relative to the CWD, which meant
  running uvicorn from a different folder silently created a second database).
- Adds delete / fetch-by-file / stats, which the UI needs.
- Batched writes so large documents don't exceed Chroma's max batch size.
"""

from __future__ import annotations

import logging
from functools import lru_cache
from typing import Any, Dict, List, Optional

from .config import settings

logger = logging.getLogger(__name__)

_MAX_BATCH = 512


@lru_cache(maxsize=1)
def _collection():
    """Create (once) the persistent client and the documents collection."""
    import chromadb
    from chromadb.utils import embedding_functions

    logger.info("Opening Chroma at %s (embed model: %s)", settings.chroma_dir, settings.embed_model)

    client = chromadb.PersistentClient(path=str(settings.chroma_dir))
    embedding_fn = embedding_functions.SentenceTransformerEmbeddingFunction(
        model_name=settings.embed_model
    )
    return client.get_or_create_collection(
        name=settings.chroma_collection,
        embedding_function=embedding_fn,
        metadata={"hnsw:space": "cosine"},
    )


def warmup() -> None:
    """Force model + collection load (called from the startup hook)."""
    _collection().count()


def add_document_chunks(
    file_id: str,
    file_name: str,
    chunks: List[str],
    method: str = "semantic",
) -> int:
    """Store chunks for a file, replacing anything previously indexed for it."""
    if not chunks:
        return 0

    # Re-indexing the same file with a different method must not leave orphans.
    delete_document(file_id)

    col = _collection()
    for offset in range(0, len(chunks), _MAX_BATCH):
        batch = chunks[offset : offset + _MAX_BATCH]
        col.add(
            ids=[f"{file_id}::{offset + i}" for i in range(len(batch))],
            documents=batch,
            metadatas=[
                {
                    "file_id": file_id,
                    "file_name": file_name,
                    "chunk_index": offset + i,
                    "method": method,
                }
                for i in range(len(batch))
            ],
        )

    return len(chunks)


def query_candidates(
    query: str,
    n_results: int = 15,
    file_id: Optional[str] = None,
) -> List[Dict[str, Any]]:
    """Retrieve top-N semantic candidates, later passed to the cross-encoder."""
    col = _collection()
    total = col.count()
    if total == 0:
        return []

    result = col.query(
        query_texts=[query],
        n_results=min(n_results, total),
        where={"file_id": file_id} if file_id else None,
    )

    ids = (result.get("ids") or [[]])[0]
    docs = (result.get("documents") or [[]])[0]
    metas = (result.get("metadatas") or [[]])[0]
    dists = (result.get("distances") or [[]])[0]

    candidates: List[Dict[str, Any]] = []
    for i, chunk_id in enumerate(ids):
        meta = metas[i] or {}
        candidates.append(
            {
                "id": chunk_id,
                "text": docs[i],
                "distance": float(dists[i]) if i < len(dists) else 0.0,
                "metadata": {
                    "file_id": meta.get("file_id", ""),
                    "file_name": meta.get("file_name", ""),
                    "chunk_index": int(meta.get("chunk_index", 0)),
                    "method": meta.get("method", ""),
                },
            }
        )
    return candidates


def get_document_chunks(file_id: str, limit: int = 50, offset: int = 0) -> List[Dict[str, Any]]:
    """Return a file's stored chunks in order -- powers the chunk inspector."""
    result = _collection().get(where={"file_id": file_id}, include=["documents", "metadatas"])

    docs = result.get("documents") or []
    metas = result.get("metadatas") or []

    rows = [
        {
            "chunk_index": int((metas[i] or {}).get("chunk_index", i)),
            "text": docs[i],
        }
        for i in range(len(docs))
    ]
    rows.sort(key=lambda r: r["chunk_index"])
    return rows[offset : offset + limit]


def count_document_chunks(file_id: str) -> int:
    result = _collection().get(where={"file_id": file_id}, include=[])
    return len(result.get("ids") or [])


def delete_document(file_id: str) -> int:
    """Remove every chunk belonging to a file. Returns how many were removed."""
    col = _collection()
    existing = col.get(where={"file_id": file_id}, include=[])
    ids = existing.get("ids") or []
    if ids:
        col.delete(ids=ids)
    return len(ids)


def total_chunks() -> int:
    try:
        return _collection().count()
    except Exception:  # noqa: BLE001 - stats must never break a health check
        return 0
