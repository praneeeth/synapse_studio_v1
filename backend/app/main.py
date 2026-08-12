"""Synapse Studio backend -- application factory and wiring.

Route handlers live in `app/routers/`; this module only assembles the app,
middleware, lifecycle hooks, and error handling.
"""

from __future__ import annotations

import logging
import time
import uuid
from contextlib import asynccontextmanager

from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from . import llm, storage
from .config import settings
from .routers import chat, documents, health

logging.basicConfig(
    level=getattr(logging, settings.log_level.upper(), logging.INFO),
    format="%(asctime)s %(levelname)-8s %(name)s: %(message)s",
)
logger = logging.getLogger("synapse")


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("Starting %s v%s", settings.app_name, settings.version)
    storage.init()
    logger.info("Registry ready at %s", settings.data_dir)
    if not settings.groq_api_key:
        logger.warning(
            "GROQ_API_KEY is not set. /chat_rag will return 503 until it is "
            "configured in backend/.env or passed per request."
        )
    yield
    await llm.close_client()
    logger.info("Shutdown complete")


app = FastAPI(
    title=settings.app_name,
    version=settings.version,
    description="Backend API for semantic RAG + reranked retrieval",
    lifespan=lifespan,
)

# CORS is now an explicit allow-list. The previous `["*"]` combined with
# allow_credentials=True is rejected by browsers and would have broken any
# future cookie-based auth.
app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.cors_origins,
    allow_credentials=True,
    allow_methods=["GET", "POST", "DELETE", "OPTIONS"],
    allow_headers=["*"],
)


@app.middleware("http")
async def request_context(request: Request, call_next):
    """Attach a request id and log latency for every call."""
    request_id = str(uuid.uuid4())[:8]
    started = time.perf_counter()
    try:
        response = await call_next(request)
    except Exception:
        logger.exception("[%s] %s %s failed", request_id, request.method, request.url.path)
        raise
    took_ms = int((time.perf_counter() - started) * 1000)
    response.headers["X-Request-ID"] = request_id
    logger.info(
        "[%s] %s %s -> %s (%dms)",
        request_id,
        request.method,
        request.url.path,
        response.status_code,
        took_ms,
    )
    return response


@app.exception_handler(Exception)
async def unhandled_exception_handler(request: Request, exc: Exception):
    """Never leak a stack trace to the client; always return valid JSON."""
    logger.exception("Unhandled error on %s %s", request.method, request.url.path)
    return JSONResponse(
        status_code=500,
        content={"detail": "Internal server error. Check the backend logs for details."},
    )


app.include_router(health.router)
app.include_router(documents.router)
app.include_router(chat.router)


@app.get("/", include_in_schema=False)
def root():
    return {
        "name": settings.app_name,
        "version": settings.version,
        "docs": "/docs",
        "health": "/health",
    }
