"""Central configuration.

Everything that used to be a magic constant scattered across modules lives here
and can be overridden through environment variables or `backend/.env`.
"""

from functools import lru_cache
from pathlib import Path
from typing import Annotated, List

import truststore
from pydantic import Field, field_validator
from pydantic_settings import BaseSettings, NoDecode, SettingsConfigDict

# Make Python's ssl module verify against the OS trust store (Windows
# Certificate Store / macOS Keychain / system CA bundle) instead of the
# bundled `certifi` list. On networks behind a TLS-inspecting proxy (common
# on corporate laptops), the proxy re-signs HTTPS traffic with a corporate
# root CA that Windows trusts but `certifi` doesn't, which makes httpx fail
# with CERTIFICATE_VERIFY_FAILED even though the connection is legitimate.
# This must run before any ssl.SSLContext is created, so it happens here, at
# the top of the module nearly everything else imports first.
truststore.inject_into_ssl()

# backend/app/config.py -> backend/
BACKEND_ROOT = Path(__file__).resolve().parent.parent


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=BACKEND_ROOT / ".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    # ---- App ----
    app_name: str = "Synapse Studio Backend"
    version: str = "0.3.0"
    log_level: str = "INFO"

    # ---- LLM ----
    groq_api_key: str | None = None
    groq_model: str = "llama-3.3-70b-versatile"
    groq_api_url: str = "https://api.groq.com/openai/v1/chat/completions"
    llm_temperature: float = 0.1
    llm_max_tokens: int = 1024
    llm_timeout_seconds: float = 60.0

    # ---- Retrieval ----
    embed_model: str = "BAAI/bge-small-en-v1.5"
    cross_encoder_model: str = "cross-encoder/ms-marco-MiniLM-L-6-v2"
    chroma_collection: str = "synapse_documents"

    # ---- Storage ----
    chroma_dir: Path = Path("chroma_db")
    data_dir: Path = Path("data")

    # ---- Ingestion ----
    max_upload_mb: int = 25
    default_chunk_size: int = 900
    default_chunk_overlap: int = 120

    # ---- CORS ----
    # NoDecode: pydantic-settings normally JSON-decodes complex-typed env vars
    # before validators run, which fails on a plain comma-separated string.
    # NoDecode hands the raw string to the `mode="before"` validator instead.
    cors_origins: Annotated[List[str], NoDecode] = Field(
        default_factory=lambda: ["http://localhost:5173", "http://127.0.0.1:5173"]
    )

    @field_validator("cors_origins", mode="before")
    @classmethod
    def _split_origins(cls, v):
        """Accept a comma-separated string from the environment."""
        if isinstance(v, str):
            return [o.strip() for o in v.split(",") if o.strip()]
        return v

    @field_validator("chroma_dir", "data_dir", mode="after")
    @classmethod
    def _absolutise(cls, v: Path) -> Path:
        """Resolve relative paths against backend/ so the CWD stops mattering."""
        return v if v.is_absolute() else (BACKEND_ROOT / v).resolve()

    @property
    def max_upload_bytes(self) -> int:
        return self.max_upload_mb * 1024 * 1024


@lru_cache(maxsize=1)
def get_settings() -> Settings:
    settings = Settings()
    settings.chroma_dir.mkdir(parents=True, exist_ok=True)
    settings.data_dir.mkdir(parents=True, exist_ok=True)
    return settings


settings = get_settings()
