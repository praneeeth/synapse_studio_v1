"""Liveness / readiness."""

from fastapi import APIRouter

from .. import vector_store
from ..config import settings
from ..schemas import HealthResponse

router = APIRouter(tags=["health"])


@router.get("/health", response_model=HealthResponse)
def health():
    return HealthResponse(
        status="ok",
        version=settings.version,
        llm_configured=bool(settings.groq_api_key),
        chunks_indexed=vector_store.total_chunks(),
    )
