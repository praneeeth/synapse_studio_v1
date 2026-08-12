"""LLM provider client (Groq's OpenAI-compatible chat completions API).

Improvements over the inline `requests.post` that lived in main.py:
- async httpx with a shared connection pool instead of a blocking call
- automatic retry with exponential backoff on 429/5xx
- token streaming support for the SSE chat endpoint
- provider errors mapped to sensible HTTP status codes instead of a blanket 500
"""

from __future__ import annotations

import json
import logging
import re
from typing import AsyncIterator, Dict, List, Optional

import httpx
from fastapi import HTTPException
from tenacity import retry, retry_if_exception_type, stop_after_attempt, wait_exponential

from .config import settings

logger = logging.getLogger(__name__)

_client: Optional[httpx.AsyncClient] = None

# Reasoning models (Qwen, DeepSeek-R1, ...) emit chain-of-thought in these tags.
_THINK_RE = re.compile(r"<(think|thinking|reasoning)>.*?</\1>", re.DOTALL | re.IGNORECASE)


class RetryableProviderError(Exception):
    """Provider returned 429/5xx -- worth retrying."""


def get_client() -> httpx.AsyncClient:
    global _client
    if _client is None:
        _client = httpx.AsyncClient(
            timeout=httpx.Timeout(settings.llm_timeout_seconds, connect=10.0),
            limits=httpx.Limits(max_connections=20, max_keepalive_connections=10),
        )
    return _client


async def close_client() -> None:
    global _client
    if _client is not None:
        await _client.aclose()
        _client = None


def clean_output(text: str) -> str:
    """Strip reasoning blocks so the UI only ever sees the final answer."""
    return _THINK_RE.sub("", text or "").strip()


def resolve_api_key(override: Optional[str]) -> str:
    key = (override or "").strip() or (settings.groq_api_key or "").strip()
    if not key:
        raise HTTPException(
            status_code=503,
            detail=(
                "LLM API key not configured. Set GROQ_API_KEY in backend/.env "
                "or supply 'llm_api_key' in the request body."
            ),
        )
    return key


def _payload(messages: List[Dict[str, str]], stream: bool) -> Dict:
    return {
        "model": settings.groq_model,
        "messages": messages,
        "temperature": settings.llm_temperature,
        "max_tokens": settings.llm_max_tokens,
        "stream": stream,
    }


def _headers(api_key: str) -> Dict[str, str]:
    return {"Authorization": f"Bearer {api_key}", "Content-Type": "application/json"}


def _raise_for_provider(status: int, body: str) -> None:
    """Map provider failures onto meaningful client-facing statuses."""
    if status in (429,) or status >= 500:
        raise RetryableProviderError(f"{status}: {body[:500]}")
    if status in (401, 403):
        raise HTTPException(status_code=502, detail="LLM provider rejected the API key.")
    raise HTTPException(status_code=502, detail=f"LLM provider error {status}: {body[:500]}")


@retry(
    retry=retry_if_exception_type((RetryableProviderError, httpx.TransportError)),
    wait=wait_exponential(multiplier=1, min=1, max=8),
    stop=stop_after_attempt(3),
    reraise=True,
)
async def complete(messages: List[Dict[str, str]], api_key: str) -> Dict[str, str]:
    """Single-shot completion. Returns {"answer": str, "model": str}."""
    try:
        response = await get_client().post(
            settings.groq_api_url, headers=_headers(api_key), json=_payload(messages, stream=False)
        )
    except httpx.TimeoutException as exc:
        raise HTTPException(status_code=504, detail=f"LLM provider timed out: {exc}") from exc

    if response.status_code != 200:
        _raise_for_provider(response.status_code, response.text)

    data = response.json()
    try:
        answer = data["choices"][0]["message"]["content"]
    except (KeyError, IndexError, TypeError) as exc:
        raise HTTPException(
            status_code=502, detail=f"Unexpected LLM response shape: {str(data)[:300]}"
        ) from exc

    return {"answer": clean_output(answer), "model": data.get("model", settings.groq_model)}


async def stream_complete(messages: List[Dict[str, str]], api_key: str) -> AsyncIterator[str]:
    """Yield answer tokens as they arrive from the provider."""
    in_think_block = False

    try:
        async with get_client().stream(
            "POST",
            settings.groq_api_url,
            headers=_headers(api_key),
            json=_payload(messages, stream=True),
        ) as response:
            if response.status_code != 200:
                body = (await response.aread()).decode("utf-8", errors="ignore")
                _raise_for_provider(response.status_code, body)

            async for line in response.aiter_lines():
                if not line.startswith("data: "):
                    continue
                payload = line[6:].strip()
                if payload == "[DONE]":
                    break
                try:
                    delta = json.loads(payload)["choices"][0]["delta"].get("content") or ""
                except (json.JSONDecodeError, KeyError, IndexError, TypeError):
                    continue
                if not delta:
                    continue

                # Suppress reasoning tokens inline; they can span many deltas.
                lowered = delta.lower()
                if "<think" in lowered:
                    in_think_block = True
                if in_think_block:
                    if "</think" in lowered:
                        in_think_block = False
                    continue

                yield delta
    except httpx.TimeoutException as exc:
        raise HTTPException(status_code=504, detail=f"LLM provider timed out: {exc}") from exc
