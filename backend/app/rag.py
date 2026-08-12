"""RAG orchestration: retrieve -> rerank -> assemble prompt.

This logic was duplicated between /query_rerank and /chat_rag in the original
main.py. It lives here once so both routes (and any future one) stay in sync.
"""

from __future__ import annotations

from typing import Dict, List, Optional, Tuple

from . import reranker, vector_store
from .schemas import ChatMessage, ContextChunk

DEFAULT_SYSTEM_PROMPT = (
    "You are a helpful RAG assistant. Answer strictly from the provided document "
    "context. If the answer is not in the context, say you don't know."
)

CITATION_INSTRUCTION = (
    "Cite the chunks you used inline as [chunk N]. If the context does not "
    "contain the answer, say so plainly instead of guessing."
)

# Keep the prompt bounded regardless of top_k / chunk size.
MAX_CONTEXT_CHARS = 12_000

# How many turns of prior conversation to replay to the model.
MAX_HISTORY_TURNS = 8


def retrieve(query: str, top_k: int, file_id: Optional[str]) -> List[ContextChunk]:
    """Vector search for a wide candidate set, then cross-encoder rerank to top_k."""
    candidates = vector_store.query_candidates(
        query=query,
        n_results=max(top_k * 3, 15),
        file_id=file_id,
    )
    reranked = reranker.rerank(query=query, candidates=candidates, top_k=top_k)

    chunks: List[ContextChunk] = []
    for c in reranked:
        meta = c.get("metadata") or {}
        chunks.append(
            ContextChunk(
                chunk_id=c["id"],
                text=c["text"],
                score=c["rerank_score"],
                file_id=meta.get("file_id", ""),
                file_name=meta.get("file_name", ""),
                chunk_index=int(meta.get("chunk_index", 0)),
            )
        )
    return chunks


def build_context_block(chunks: List[ContextChunk]) -> str:
    """Concatenate chunks for the prompt, truncating at MAX_CONTEXT_CHARS."""
    if not chunks:
        return "NO CONTEXT FOUND"

    parts: List[str] = []
    used = 0
    for c in chunks:
        part = f"[chunk {c.chunk_index} | file: {c.file_name}]\n{c.text}"
        if used + len(part) > MAX_CONTEXT_CHARS:
            break
        parts.append(part)
        used += len(part)

    return "\n\n-----\n\n".join(parts) if parts else "NO CONTEXT FOUND"


def build_system_prompt(
    persona: Optional[str], base_prompt: Optional[str], rules: Optional[str]
) -> str:
    """Combine persona + system prompt + rules into a single system message."""
    parts: List[str] = []
    if persona:
        parts.append(f"Persona: {persona}")
    parts.append(base_prompt.strip() if base_prompt and base_prompt.strip() else DEFAULT_SYSTEM_PROMPT)
    if rules and rules.strip():
        parts.append("Strict rules the assistant must follow:\n" + rules.strip())
    parts.append(CITATION_INSTRUCTION)
    return "\n\n".join(parts)


def build_messages(
    query: str,
    chunks: List[ContextChunk],
    persona: Optional[str],
    system_prompt: Optional[str],
    rules: Optional[str],
    history: Optional[List[ChatMessage]] = None,
) -> List[Dict[str, str]]:
    """Assemble the full message list sent to the LLM."""
    messages: List[Dict[str, str]] = [
        {"role": "system", "content": build_system_prompt(persona, system_prompt, rules)}
    ]

    for turn in (history or [])[-MAX_HISTORY_TURNS:]:
        messages.append({"role": turn.role, "content": turn.content})

    messages.append(
        {
            "role": "user",
            "content": (
                "Use ONLY the following context from the user's indexed documents "
                "to answer the question.\n\n"
                f"Context:\n{build_context_block(chunks)}\n\n"
                f"User question:\n{query}\n\n"
                "If the answer is not clearly contained in the context, say you "
                "don't know from these documents."
            ),
        }
    )
    return messages


def prepare(
    query: str,
    top_k: int,
    file_id: Optional[str],
    persona: Optional[str],
    system_prompt: Optional[str],
    rules: Optional[str],
    history: Optional[List[ChatMessage]] = None,
) -> Tuple[List[ContextChunk], List[Dict[str, str]]]:
    """Retrieve context and build the message list in one call."""
    chunks = retrieve(query, top_k, file_id)
    return chunks, build_messages(query, chunks, persona, system_prompt, rules, history)
