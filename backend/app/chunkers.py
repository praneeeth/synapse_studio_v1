"""Chunking strategies.

The UI offers four methods; previously only `semantic` existed on the backend
and the other three silently fell through to it. All four are implemented here
behind one `chunk_text()` entry point.

The embedding model used by the semantic splitter is loaded once and cached --
the old code constructed `HuggingFaceEmbeddings` on every single /index call,
which re-read the model from disk each time.
"""

from __future__ import annotations

import logging
import re
from functools import lru_cache
from typing import List

from langchain_text_splitters import RecursiveCharacterTextSplitter

from .config import settings

logger = logging.getLogger(__name__)

# Order matters: the recursive splitter tries these boundaries top-down.
_RECURSIVE_SEPARATORS = ["\n\n", "\n", ". ", "! ", "? ", "; ", ", ", " ", ""]


def clean_text(text: str) -> str:
    """Normalise line endings and collapse runs of blank lines."""
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    text = re.sub(r"[ \t]+\n", "\n", text)       # trailing whitespace per line
    text = re.sub(r"\n{3,}", "\n\n", text)       # >2 blank lines -> paragraph break
    return text.strip()


@lru_cache(maxsize=1)
def _semantic_embeddings():
    """Load the embedding model once per process (lazily, on first use)."""
    from langchain_huggingface import HuggingFaceEmbeddings

    logger.info("Loading semantic-chunking embeddings: %s", settings.embed_model)
    return HuggingFaceEmbeddings(model_name=settings.embed_model)


def _enforce_max_chars(chunks: List[str], max_chars: int) -> List[str]:
    """Hard cap chunk length, splitting oversized chunks on whitespace when possible."""
    out: List[str] = []
    for c in chunks:
        c = c.strip()
        if not c:
            continue
        if len(c) <= max_chars:
            out.append(c)
            continue

        start = 0
        while start < len(c):
            end = min(start + max_chars, len(c))
            if end < len(c):
                # Back off to the last whitespace so we don't slice mid-word.
                pivot = c.rfind(" ", start + max_chars // 2, end)
                if pivot != -1:
                    end = pivot
            piece = c[start:end].strip()
            if piece:
                out.append(piece)
            start = end
    return out


# ------------------------------------------------------------------ methods --


def chunk_fixed(text: str, chunk_size: int, chunk_overlap: int) -> List[str]:
    """Equal-width character windows with overlap. The baseline."""
    text = clean_text(text)
    if not text:
        return []

    step = max(1, chunk_size - chunk_overlap)
    chunks: List[str] = []
    for start in range(0, len(text), step):
        piece = text[start : start + chunk_size].strip()
        if piece:
            chunks.append(piece)
        if start + chunk_size >= len(text):
            break
    return chunks


def chunk_recursive(text: str, chunk_size: int, chunk_overlap: int) -> List[str]:
    """Split on the largest natural boundary that fits, falling back downward."""
    text = clean_text(text)
    if not text:
        return []

    splitter = RecursiveCharacterTextSplitter(
        chunk_size=chunk_size,
        chunk_overlap=chunk_overlap,
        separators=_RECURSIVE_SEPARATORS,
        length_function=len,
    )
    return _enforce_max_chars(splitter.split_text(text), chunk_size)


def chunk_paragraph(text: str, chunk_size: int, chunk_overlap: int) -> List[str]:
    """Paragraph boundaries, greedily packed up to chunk_size."""
    text = clean_text(text)
    if not text:
        return []

    paragraphs = [p.strip() for p in text.split("\n\n") if p.strip()]

    packed: List[str] = []
    buffer = ""
    for para in paragraphs:
        candidate = f"{buffer}\n\n{para}" if buffer else para
        if len(candidate) <= chunk_size:
            buffer = candidate
        else:
            if buffer:
                packed.append(buffer)
            buffer = para
    if buffer:
        packed.append(buffer)

    return _enforce_max_chars(packed, chunk_size)


def chunk_semantic(text: str, chunk_size: int | None = None, chunk_overlap: int = 0) -> List[str]:
    """Group sentences by embedding similarity (LangChain SemanticChunker + BGE).

    Falls back to the recursive splitter if the semantic stack is unavailable,
    so a missing optional dependency degrades quality instead of erroring out.
    """
    max_chars = chunk_size or settings.default_chunk_size
    text = clean_text(text)
    if not text:
        return []

    try:
        from langchain_experimental.text_splitter import SemanticChunker

        splitter = SemanticChunker(_semantic_embeddings())
        docs = splitter.create_documents([text])
        chunks = [d.page_content for d in docs]
    except Exception as exc:  # noqa: BLE001 - degrade, don't fail ingestion
        logger.warning("Semantic chunking unavailable (%s); using recursive splitter", exc)
        return chunk_recursive(text, max_chars, settings.default_chunk_overlap)

    return _enforce_max_chars(chunks, max_chars)


# ----------------------------------------------------------------- dispatch --

_DISPATCH = {
    "fixed": chunk_fixed,
    "recursive": chunk_recursive,
    "paragraph": chunk_paragraph,
    "semantic": chunk_semantic,
}


def chunk_text(
    text: str,
    method: str = "semantic",
    chunk_size: int | None = None,
    chunk_overlap: int | None = None,
) -> List[str]:
    """Single entry point used by the ingestion pipeline."""
    fn = _DISPATCH.get(method)
    if fn is None:
        raise ValueError(f"Unknown chunking method: {method!r}. Expected one of {sorted(_DISPATCH)}")

    size = chunk_size or settings.default_chunk_size
    overlap = settings.default_chunk_overlap if chunk_overlap is None else chunk_overlap
    if overlap >= size:
        overlap = size // 4

    return fn(text, size, overlap)


def available_methods() -> List[str]:
    return sorted(_DISPATCH)
