"""Document ingestion: upload, chunk, index, inspect, delete."""

from __future__ import annotations

import logging
import time

from fastapi import APIRouter, File, HTTPException, Query, UploadFile

from .. import chunkers, storage, vector_store
from ..config import settings
from ..parsers import UnsupportedFileError, extract_text
from ..schemas import (
    ChunkPreviewItem,
    ChunkPreviewResponse,
    DeleteResponse,
    DocumentListResponse,
    DocumentSummary,
    FilePreviewResponse,
    IndexRequest,
    IndexResponse,
    StatsResponse,
    UploadResponse,
)

logger = logging.getLogger(__name__)
router = APIRouter(tags=["documents"])

PREVIEW_CHAR_LIMIT = 20_000


@router.post("/upload", response_model=UploadResponse)
async def upload_file(file: UploadFile = File(...)):
    """Store an uploaded file's extracted text and return its file_id."""
    content = await file.read()

    if not content:
        raise HTTPException(status_code=400, detail="Uploaded file is empty.")
    if len(content) > settings.max_upload_bytes:
        raise HTTPException(
            status_code=413,
            detail=f"File exceeds the {settings.max_upload_mb} MB limit "
            f"({len(content) / 1024 / 1024:.1f} MB).",
        )

    try:
        text, kind = extract_text(content, file.filename or "upload", file.content_type or "")
    except UnsupportedFileError as exc:
        raise HTTPException(status_code=415, detail=str(exc)) from exc
    except Exception as exc:  # noqa: BLE001
        logger.exception("Text extraction failed for %s", file.filename)
        raise HTTPException(status_code=400, detail=f"Failed to read file: {exc}") from exc

    record = storage.create_document(
        file_name=file.filename or "upload",
        content_type=file.content_type or kind,
        text=text,
    )

    return UploadResponse(
        file_id=record["file_id"],
        file_name=record["file_name"],
        num_chars=record["num_chars"],
        content_type=record["content_type"],
        indexed=False,
    )


@router.post("/index", response_model=IndexResponse)
def index_file(req: IndexRequest):
    """Chunk a stored document with the requested strategy and index it in Chroma."""
    record = storage.get_document(req.file_id)
    if record is None:
        raise HTTPException(status_code=404, detail="file_id not found")

    text = storage.get_text(req.file_id)
    if not text.strip():
        raise HTTPException(status_code=400, detail="Stored document has no text.")

    started = time.perf_counter()
    storage.update_status(req.file_id, "indexing", method=req.method)

    try:
        chunks = chunkers.chunk_text(
            text,
            method=req.method,
            chunk_size=req.chunk_size,
            chunk_overlap=req.chunk_overlap,
        )
        if not chunks:
            raise ValueError("Chunking produced no chunks.")

        total = vector_store.add_document_chunks(
            file_id=req.file_id,
            file_name=record["file_name"],
            chunks=chunks,
            method=req.method,
        )
    except Exception as exc:  # noqa: BLE001
        logger.exception("Indexing failed for %s", req.file_id)
        storage.update_status(req.file_id, "failed", method=req.method, error=str(exc))
        raise HTTPException(status_code=500, detail=f"Indexing failed: {exc}") from exc

    storage.update_status(req.file_id, "indexed", method=req.method, total_chunks=total)

    return IndexResponse(
        file_id=req.file_id,
        file_name=record["file_name"],
        method=req.method,
        total_chunks=total,
        avg_chunk_chars=sum(len(c) for c in chunks) // max(len(chunks), 1),
        took_ms=int((time.perf_counter() - started) * 1000),
    )


@router.get("/documents", response_model=DocumentListResponse)
def list_documents():
    """All known documents, newest first. Survives a backend restart."""
    return DocumentListResponse(
        documents=[DocumentSummary(**row) for row in storage.list_documents()]
    )


@router.get("/documents/{file_id}/preview", response_model=FilePreviewResponse)
def preview_file(file_id: str):
    """Raw extracted text (truncated) -- powers the 'File preview' menu item."""
    record = storage.get_document(file_id)
    if record is None:
        raise HTTPException(status_code=404, detail="file_id not found")

    text = storage.get_text(file_id)
    return FilePreviewResponse(
        file_id=file_id,
        file_name=record["file_name"],
        num_chars=len(text),
        truncated=len(text) > PREVIEW_CHAR_LIMIT,
        text=text[:PREVIEW_CHAR_LIMIT],
    )


@router.get("/documents/{file_id}/chunks", response_model=ChunkPreviewResponse)
def preview_chunks(
    file_id: str,
    limit: int = Query(default=50, ge=1, le=500),
    offset: int = Query(default=0, ge=0),
):
    """Indexed chunks in order -- powers the 'Chunk preview' inspector."""
    record = storage.get_document(file_id)
    if record is None:
        raise HTTPException(status_code=404, detail="file_id not found")

    rows = vector_store.get_document_chunks(file_id, limit=limit, offset=offset)
    return ChunkPreviewResponse(
        file_id=file_id,
        file_name=record["file_name"],
        method=record.get("method"),
        total_chunks=vector_store.count_document_chunks(file_id),
        chunks=[
            ChunkPreviewItem(
                chunk_index=r["chunk_index"], text=r["text"], num_chars=len(r["text"])
            )
            for r in rows
        ],
    )


@router.delete("/documents/{file_id}", response_model=DeleteResponse)
def delete_document(file_id: str):
    """Remove a document's vectors, extracted text, and registry row."""
    if storage.get_document(file_id) is None:
        raise HTTPException(status_code=404, detail="file_id not found")

    deleted = vector_store.delete_document(file_id)
    storage.delete_document(file_id)
    return DeleteResponse(file_id=file_id, deleted_chunks=deleted)


@router.get("/chunking-methods")
def chunking_methods():
    """Strategies the backend actually implements (the UI dropdown reads this)."""
    return {"methods": chunkers.available_methods(), "default": "semantic"}


@router.get("/stats", response_model=StatsResponse)
def stats():
    """Live counters for the 'Model & Index overview' panel."""
    c = storage.counts()
    return StatsResponse(
        files_uploaded=c["uploaded"],
        files_indexed=c["indexed"],
        total_chunks=vector_store.total_chunks(),
        embed_model=settings.embed_model,
        cross_encoder_model=settings.cross_encoder_model,
        chat_model=settings.groq_model,
    )
