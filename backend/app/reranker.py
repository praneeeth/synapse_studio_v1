"""Cross-encoder reranking.

The model is now loaded lazily and cached rather than at import time, so simply
importing the app (tests, `--reload` workers, CLI tooling) no longer pays a
multi-hundred-megabyte model load.
"""

from __future__ import annotations

import logging
from functools import lru_cache
from typing import Dict, List

from .config import settings

logger = logging.getLogger(__name__)


@lru_cache(maxsize=1)
def _model():
    from sentence_transformers import CrossEncoder

    logger.info("Loading cross-encoder: %s", settings.cross_encoder_model)
    return CrossEncoder(settings.cross_encoder_model)


def warmup() -> None:
    """Force the model load (called from the startup hook)."""
    _model().predict([("warmup", "warmup")])


def rerank(query: str, candidates: List[Dict], top_k: int = 5) -> List[Dict]:
    """Re-score (query, chunk) pairs with a cross-encoder and keep the best top_k.

    Falls back to the vector distance ordering if the cross-encoder fails, so a
    reranker problem degrades result quality instead of failing the request.
    """
    if not candidates:
        return []

    try:
        scores = _model().predict([(query, c["text"]) for c in candidates])
        for c, s in zip(candidates, scores):
            c["rerank_score"] = float(s)
    except Exception as exc:  # noqa: BLE001
        logger.warning("Reranking failed (%s); falling back to vector distance", exc)
        for c in candidates:
            # Lower distance == better, so negate to keep "higher is better".
            c["rerank_score"] = -float(c.get("distance", 0.0))

    return sorted(candidates, key=lambda x: x["rerank_score"], reverse=True)[:top_k]
