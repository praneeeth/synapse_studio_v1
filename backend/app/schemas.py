"""All request/response models in one place.

Keeping schemas out of the route modules makes the API surface reviewable at a
glance and lets the frontend be generated/checked against a single file.
"""

from typing import List, Literal, Optional

from pydantic import BaseModel, Field

ChunkMethod = Literal["fixed", "recursive", "paragraph", "semantic"]


# ---------------------------------------------------------------- documents --


class UploadResponse(BaseModel):
    file_id: str
    file_name: str
    num_chars: int
    content_type: str
    indexed: bool = False


class IndexRequest(BaseModel):
    file_id: str
    method: ChunkMethod = "semantic"
    chunk_size: Optional[int] = Field(default=None, ge=100, le=8000)
    chunk_overlap: Optional[int] = Field(default=None, ge=0, le=2000)


class IndexResponse(BaseModel):
    file_id: str
    file_name: str
    method: ChunkMethod
    total_chunks: int
    avg_chunk_chars: int
    took_ms: int


class DocumentSummary(BaseModel):
    file_id: str
    file_name: str
    num_chars: int
    content_type: str
    status: Literal["uploaded", "indexing", "indexed", "failed"]
    method: Optional[ChunkMethod] = None
    total_chunks: Optional[int] = None
    error: Optional[str] = None
    created_at: str


class DocumentListResponse(BaseModel):
    documents: List[DocumentSummary]


class ChunkPreviewItem(BaseModel):
    chunk_index: int
    text: str
    num_chars: int


class ChunkPreviewResponse(BaseModel):
    file_id: str
    file_name: str
    method: Optional[ChunkMethod]
    total_chunks: int
    chunks: List[ChunkPreviewItem]


class FilePreviewResponse(BaseModel):
    file_id: str
    file_name: str
    num_chars: int
    truncated: bool
    text: str


class DeleteResponse(BaseModel):
    file_id: str
    deleted_chunks: int


class StatsResponse(BaseModel):
    files_uploaded: int
    files_indexed: int
    total_chunks: int
    embed_model: str
    cross_encoder_model: str
    chat_model: str


# ---------------------------------------------------------------- retrieval --


class QueryRequest(BaseModel):
    query: str = Field(min_length=1)
    top_k: int = Field(default=5, ge=1, le=50)
    file_id: Optional[str] = None


class QueryResultItem(BaseModel):
    chunk_id: str
    text: str
    score: float
    file_id: str
    file_name: str
    chunk_index: int


class QueryResponse(BaseModel):
    results: List[QueryResultItem]
    took_ms: int


# --------------------------------------------------------------------- chat --


class ChatMessage(BaseModel):
    role: Literal["user", "assistant"]
    content: str


class ContextChunk(BaseModel):
    chunk_id: str
    text: str
    score: float
    file_id: str
    file_name: str
    chunk_index: int


class ChatRequest(BaseModel):
    query: str = Field(min_length=1)
    file_id: Optional[str] = None
    top_k: int = Field(default=5, ge=1, le=20)

    # Right-panel behaviour controls
    persona: Optional[str] = None
    system_prompt: Optional[str] = None
    rules: Optional[str] = None

    # Multi-turn support: prior turns, oldest first (excludes the current query)
    history: List[ChatMessage] = Field(default_factory=list)

    debug: bool = True

    # Dev convenience: allow a per-request key; otherwise the server env is used
    llm_api_key: Optional[str] = None


class ChatResponse(BaseModel):
    answer: str
    model: str
    used_query: str
    context: List[ContextChunk]
    took_ms: int


class HealthResponse(BaseModel):
    status: str
    version: str
    llm_configured: bool
    chunks_indexed: int
