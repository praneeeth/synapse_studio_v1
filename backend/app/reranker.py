from typing import List, Dict

from sentence_transformers import CrossEncoder

# CrossEncoder model for reranking (query, chunk_text) pairs
CROSS_ENCODER_MODEL = "cross-encoder/ms-marco-MiniLM-L-6-v2"

cross_encoder = CrossEncoder(CROSS_ENCODER_MODEL)


def rerank(query: str, candidates: List[Dict], top_k: int = 5) -> List[Dict]:
    """
    Re-rank candidate chunks using a cross-encoder.
    Each candidate must have keys: "text", "metadata", "id", "distance".
    Adds "rerank_score" to each and returns the top_k.
    """
    if not candidates:
        return []

    pairs = [(query, c["text"]) for c in candidates]
    scores = cross_encoder.predict(pairs)

    for c, s in zip(candidates, scores):
        c["rerank_score"] = float(s)

    candidates_sorted = sorted(
        candidates,
        key=lambda x: x["rerank_score"],
        reverse=True,
    )

    return candidates_sorted[:top_k]
