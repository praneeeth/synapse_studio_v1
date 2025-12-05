from typing import List

from langchain_experimental.text_splitter import SemanticChunker
from langchain_community.embeddings import HuggingFaceEmbeddings


def clean_text(text: str) -> str:
    """Basic cleanup for incoming text."""
    return text.replace("\r", "").strip()


def chunk_semantic(text: str, max_chars: int = 900) -> List[str]:
    """
    Main (and only) chunking strategy for Synapse Studio:
    semantic chunking using LangChain's SemanticChunker + BGE embeddings.
    """
    text = clean_text(text)
    if not text:
        return []

    # Open-source embedding model for semantic chunking
    embeddings = HuggingFaceEmbeddings(
        model_name="BAAI/bge-small-en-v1.5"
    )

    splitter = SemanticChunker(embeddings)

    # SemanticChunker returns LangChain Document objects
    docs = splitter.create_documents([text])
    chunks = [d.page_content for d in docs]

    # Safety: enforce hard max_chars per chunk
    processed: List[str] = []
    for c in chunks:
        if len(c) <= max_chars:
            processed.append(c)
        else:
            start = 0
            while start < len(c):
                end = min(start + max_chars, len(c))
                processed.append(c[start:end])
                start = end

    return processed
