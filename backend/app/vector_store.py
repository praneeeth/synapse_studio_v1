from typing import List, Dict, Any, Optional

import chromadb
from chromadb.utils import embedding_functions

# Persistent Chroma client (creates "chroma_db" folder in backend/)
client = chromadb.PersistentClient(path="chroma_db")

# Open-source embedding model for indexing
EMBED_MODEL_NAME = "BAAI/bge-small-en-v1.5"

embedding_fn = embedding_functions.SentenceTransformerEmbeddingFunction(
    model_name=EMBED_MODEL_NAME
)

# Single collection for now; we filter by file_id using metadata
collection = client.get_or_create_collection(
    name="synapse_documents",
    embedding_function=embedding_fn,
)


def add_document_chunks(
    file_id: str,
    file_name: str,
    chunks: List[str],
) -> int:
    """
    Store chunks for a file in Chroma.
    Each chunk gets:
    - id: f"{file_id}::{i}"
    - metadata: file_id, file_name, chunk_index
    """
    ids = [f"{file_id}::{i}" for i in range(len(chunks))]
    metadatas: List[Dict[str, Any]] = [
        {
            "file_id": file_id,
            "file_name": file_name,
            "chunk_index": i,
        }
        for i in range(len(chunks))
    ]

    collection.add(ids=ids, documents=chunks, metadatas=metadatas)
    return len(chunks)


def query_candidates(
    query: str,
    n_results: int = 15,
    file_id: Optional[str] = None,
) -> List[Dict[str, Any]]:
    """
    Retrieve top-N semantic candidates from Chroma as dictionaries.
    These are later passed into the cross-encoder for reranking.
    """
    where = {"file_id": file_id} if file_id else None

    result = collection.query(
        query_texts=[query],
        n_results=n_results,
        where=where,
    )

    ids = result.get("ids", [[]])[0]
    docs = result.get("documents", [[]])[0]
    metas = result.get("metadatas", [[]])[0]
    dists = result.get("distances", [[]])[0]

    candidates: List[Dict[str, Any]] = []
    for i in range(len(ids)):
        meta = metas[i] or {}
        candidates.append(
            {
                "id": ids[i],
                "text": docs[i],
                "distance": float(dists[i]),
                "metadata": {
                    "file_id": meta.get("file_id", ""),
                    "file_name": meta.get("file_name", ""),
                    "chunk_index": int(meta.get("chunk_index", 0)),
                },
            }
        )

    return candidates
