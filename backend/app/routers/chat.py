"""Retrieval and chat endpoints."""

from __future__ import annotations

import json
import logging
import time

from fastapi import APIRouter, HTTPException
from fastapi.responses import StreamingResponse

from .. import llm, rag
from ..schemas import (
    ChatRequest,
    ChatResponse,
    QueryRequest,
    QueryResponse,
    QueryResultItem,
)

logger = logging.getLogger(__name__)
router = APIRouter(tags=["chat"])


@router.post("/query_rerank", response_model=QueryResponse)
def query_with_rerank(req: QueryRequest):
    """Retrieval only: vector candidates reranked by the cross-encoder."""
    if not req.query.strip():
        raise HTTPException(status_code=400, detail="Query text is empty")

    started = time.perf_counter()
    chunks = rag.retrieve(req.query, req.top_k, req.file_id)

    return QueryResponse(
        results=[
            QueryResultItem(
                chunk_id=c.chunk_id,
                text=c.text,
                score=c.score,
                file_id=c.file_id,
                file_name=c.file_name,
                chunk_index=c.chunk_index,
            )
            for c in chunks
        ],
        took_ms=int((time.perf_counter() - started) * 1000),
    )


@router.post("/chat_rag", response_model=ChatResponse)
async def chat_rag(req: ChatRequest):
    """Full RAG turn: retrieve -> rerank -> prompt -> LLM -> answer + context."""
    if not req.query.strip():
        raise HTTPException(status_code=400, detail="Query is empty")

    started = time.perf_counter()
    api_key = llm.resolve_api_key(req.llm_api_key)

    chunks, messages = rag.prepare(
        query=req.query,
        top_k=req.top_k,
        file_id=req.file_id,
        persona=req.persona,
        system_prompt=req.system_prompt,
        rules=req.rules,
        history=req.history,
    )

    result = await llm.complete(messages, api_key)
    answer = result["answer"] or (
        "I couldn't generate a clean answer from the model output. Try rephrasing your question."
    )

    return ChatResponse(
        answer=answer,
        model=result["model"],
        used_query=req.query,
        context=chunks if req.debug else [],
        took_ms=int((time.perf_counter() - started) * 1000),
    )


@router.post("/chat_rag/stream")
async def chat_rag_stream(req: ChatRequest):
    """Same as /chat_rag but streams the answer token-by-token over SSE.

    Event protocol:
      event: context -> {"context": [...]}      (sent first, so the UI can render sources)
      event: token   -> {"token": "..."}        (repeated)
      event: done    -> {"model": "...", "took_ms": N}
      event: error   -> {"detail": "..."}
    """
    if not req.query.strip():
        raise HTTPException(status_code=400, detail="Query is empty")

    started = time.perf_counter()
    api_key = llm.resolve_api_key(req.llm_api_key)

    chunks, messages = rag.prepare(
        query=req.query,
        top_k=req.top_k,
        file_id=req.file_id,
        persona=req.persona,
        system_prompt=req.system_prompt,
        rules=req.rules,
        history=req.history,
    )

    async def event_stream():
        def sse(event: str, data: dict) -> str:
            return f"event: {event}\ndata: {json.dumps(data)}\n\n"

        yield sse("context", {"context": [c.model_dump() for c in chunks] if req.debug else []})

        try:
            async for token in llm.stream_complete(messages, api_key):
                yield sse("token", {"token": token})
        except HTTPException as exc:
            yield sse("error", {"detail": exc.detail})
            return
        except Exception as exc:  # noqa: BLE001
            logger.exception("Streaming chat failed")
            yield sse("error", {"detail": str(exc)})
            return

        yield sse("done", {"model": None, "took_ms": int((time.perf_counter() - started) * 1000)})

    return StreamingResponse(
        event_stream(),
        media_type="text/event-stream",
        headers={"Cache-Control": "no-cache", "X-Accel-Buffering": "no"},
    )
