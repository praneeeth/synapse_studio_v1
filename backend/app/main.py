from typing import Dict, List, Optional, Any
import os
import requests
import uuid
from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import Literal
from . import chunkers
from . import vector_store
from .reranker import rerank
import re
from io import BytesIO
from pypdf import PdfReader

app = FastAPI(
    title="Synapse Studio Backend",
    version="0.2.0",
    description="Backend API for semantic RAG + reranked retrieval",
)

# CORS etc...
# Configure CORS (adjust origins for production)
origins = ["*"]
app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# In-memory FILE_STORE used by the index endpoint.
# Structure: {file_id: {"text": str, "file_name": str}}
FILE_STORE: Dict[str, Dict[str, Any]] = {}

# Simple health endpoint
@app.get("/health")
def health():
    return {"status": "ok"}

# Upload endpoint to store file text in FILE_STORE
@app.post("/upload")
async def upload_file(file: UploadFile = File(...)):
    """
    Step 1: Upload a file and store its text in FILE_STORE.
    - For PDFs: extract readable text with pypdf.
    - For plain text / markdown: decode as UTF-8.
    """
    content = await file.read()

    # Detect PDF by content-type or extension
    is_pdf = (
        (file.content_type or "").lower() == "application/pdf"
        or file.filename.lower().endswith(".pdf")
    )

    if is_pdf:
        try:
            reader = PdfReader(BytesIO(content))
            pages_text = []
            for page in reader.pages:
                page_text = page.extract_text() or ""
                pages_text.append(page_text)
            text = "\n\n".join(pages_text)
        except Exception as e:
            raise HTTPException(
                status_code=400,
                detail=f"Failed to extract text from PDF: {e}",
            )
    else:
        # Fallback: normal text file
        try:
            text = content.decode("utf-8")
        except UnicodeDecodeError:
            # Last resort: best-effort decode, dropping bad bytes
            text = content.decode("latin-1", errors="ignore")

    if not text.strip():
        raise HTTPException(
            status_code=400,
            detail="No readable text extracted from file.",
        )

    file_id = str(uuid.uuid4())
    FILE_STORE[file_id] = {
        "text": text,
        "file_name": file.filename,
    }

    # (Optional) return num_chars if your UI uses it
    return {
        "file_id": file_id,
        "file_name": file.filename,
        "num_chars": len(text),
    }


# ---------- INDEX MODELS ---------- #

class IndexRequest(BaseModel):
    file_id: str


class IndexResponse(BaseModel):
    file_id: str
    file_name: str
    total_chunks: int


# ---------- INDEX ENDPOINT ---------- #

@app.post("/index", response_model=IndexResponse)
def index_file(req: IndexRequest):
    """
    Step 2: For a given file_id, perform semantic chunking
    and store chunks in Chroma.
    """
    if req.file_id not in FILE_STORE:
        raise HTTPException(status_code=404, detail="file_id not found")

    record = FILE_STORE[req.file_id]
    text = record["text"]
    file_name = record["file_name"]

    # semantic chunking (LangChain SemanticChunker + BGE)
    chunks = chunkers.chunk_semantic(text)

    if not chunks:
        raise HTTPException(status_code=400, detail="No chunks generated from file")

    total = vector_store.add_document_chunks(
        file_id=req.file_id,
        file_name=file_name,
        chunks=chunks,
    )

    return IndexResponse(
        file_id=req.file_id,
        file_name=file_name,
        total_chunks=total,
    )
class QueryRequest(BaseModel):
    query: str
    top_k: int = 5
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


@app.post("/query_rerank", response_model=QueryResponse)
def query_with_rerank(req: QueryRequest):
    """
    Step 3: retrieve candidates from Chroma, rerank with cross-encoder,
    and return best chunks for the query.
    """
    if not req.query.strip():
        raise HTTPException(status_code=400, detail="Query text is empty")

    # 1) semantic candidates from Chroma
    candidates = vector_store.query_candidates(
        query=req.query,
        n_results=max(req.top_k * 3, 15),
        file_id=req.file_id,
    )

    # 2) rerank with cross-encoder
    reranked = rerank(
        query=req.query,
        candidates=candidates,
        top_k=req.top_k,
    )

    results: List[QueryResultItem] = []
    for c in reranked:
        meta = c["metadata"]
        results.append(
            QueryResultItem(
                chunk_id=c["id"],
                text=c["text"],
                score=c["rerank_score"],
                file_id=meta.get("file_id", ""),
                file_name=meta.get("file_name", ""),
                chunk_index=int(meta.get("chunk_index", 0)),
            )
        )

    return QueryResponse(results=results)
class ContextChunk(BaseModel):
    chunk_id: str
    text: str
    score: float
    file_id: str
    file_name: str
    chunk_index: int


class ChatRequest(BaseModel):
    query: str
    file_id: Optional[str] = None
    top_k: int = 5

    # From right-panel UI
    persona: Optional[str] = None          # e.g. "RAG expert" / "teacher"
    system_prompt: Optional[str] = None    # base system prompt text
    rules: Optional[str] = None            # extra rules / guardrails

    # Hybrid / debug
    debug: bool = True                     # always return context chunks

    # For dev: allow passing key in body; otherwise use env var
    llm_api_key: Optional[str] = None


class ChatResponse(BaseModel):
    answer: str
    model: str
    used_query: str
    context: List[ContextChunk]


def build_system_prompt(persona: Optional[str], base_prompt: Optional[str], rules: Optional[str]) -> str:
    """Combine persona + system prompt + rules into one system message."""
    parts: List[str] = []

    if persona:
        parts.append(f"Persona: {persona}")

    if base_prompt:
        parts.append(base_prompt)
    else:
        parts.append(
            "You are a helpful RAG assistant. Answer strictly from the provided document "
            "context. If the answer is not in the context, say you don't know."
        )

    if rules:
        parts.append("Strict rules the assistant must follow:\n" + rules)

    return "\n\n".join(parts)

# ---------- CHAT RAG ENDPOINT (Hybrid: answer + context) ---------- #

# ---------- CHAT RAG ENDPOINT (Hybrid: answer + context) ---------- #

GROQ_API_URL = "https://api.groq.com/openai/v1/chat/completions"
GROQ_MODEL = "qwen/qwen3-32b"  # adjust if you prefer another Groq model


def clean_llm_output(text: str) -> str:
    """
    Remove any <think>...</think> blocks or similar reasoning tags
    so the user only sees the final answer.
    """
    # remove <think>...</think> (case-insensitive, multiline)
    text = re.sub(r"<think>.*?</think>", "", text, flags=re.DOTALL | re.IGNORECASE)
    # also clean up extra whitespace
    return text.strip()


@app.post("/chat_rag", response_model=ChatResponse)
def chat_rag(req: ChatRequest):
    """
    Full RAG flow:
    1) Retrieve semantic candidates from Chroma
    2) Rerank with cross-encoder
    3) Build system + user prompt using persona / rules
    4) Call Groq LLM to generate answer
    5) Return answer AND context chunks (hybrid debug mode)
    """
    if not req.query.strip():
        raise HTTPException(status_code=400, detail="Query is empty")

    # 1) retrieve + rerank (same logic as /query_rerank)
    candidates = vector_store.query_candidates(
        query=req.query,
        n_results=max(req.top_k * 3, 15),
        file_id=req.file_id,
    )
    reranked = rerank(query=req.query, candidates=candidates, top_k=req.top_k)

    context_chunks: List[ContextChunk] = []
    for c in reranked:
        meta = c["metadata"]
        context_chunks.append(
            ContextChunk(
                chunk_id=c["id"],
                text=c["text"],
                score=c["rerank_score"],
                file_id=meta.get("file_id", ""),
                file_name=meta.get("file_name", ""),
                chunk_index=int(meta.get("chunk_index", 0)),
            )
        )

    # Build a combined context string for the LLM
    context_block_parts = []
    for cc in context_chunks:
        context_block_parts.append(
            f"[chunk {cc.chunk_index} | file: {cc.file_name}]\n{cc.text}"
        )
    context_block = (
        "\n\n-----\n\n".join(context_block_parts)
        if context_block_parts
        else "NO CONTEXT FOUND"
    )

    # 2) Build system + user prompt
    system_prompt_text = build_system_prompt(
        persona=req.persona,
        base_prompt=req.system_prompt,
        rules=req.rules,
    )

    user_prompt_text = (
        "Use ONLY the following context from the user's indexed documents to answer the question.\n\n"
        f"Context:\n{context_block}\n\n"
        f"User question:\n{req.query}\n\n"
        "If the answer is not clearly contained in the context, say you don't know from these documents."
    )

    # 3) Figure out API key (body > env)
    api_key = req.llm_api_key or os.getenv("GROQ_API_KEY")
    if not api_key:
        raise HTTPException(
            status_code=500,
            detail="LLM API key not configured. Pass 'llm_api_key' in the request or set GROQ_API_KEY env var.",
        )

    # 4) Call Groq LLM
    try:
        response = requests.post(
            GROQ_API_URL,
            headers={
                "Authorization": f"Bearer {api_key}",
                "Content-Type": "application/json",
            },
            json={
                "model": GROQ_MODEL,
                "messages": [
                    {"role": "system", "content": system_prompt_text},
                    {"role": "user", "content": user_prompt_text},
                ],
                "temperature": 0.1,
            },
            timeout=40,
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error calling Groq API: {e}")

    if response.status_code != 200:
        raise HTTPException(
            status_code=500,
            detail=f"Groq API error: {response.status_code} {response.text}",
        )

    data = response.json()
    try:
        raw_answer = data["choices"][0]["message"]["content"]
        model_used = data.get("model", GROQ_MODEL)
    except Exception:
        raise HTTPException(
            status_code=500,
            detail=f"Unexpected Groq response format: {data}",
        )

    # 🔹 Clean out any <think> reasoning blocks before sending to UI
    answer_text = clean_llm_output(raw_answer)
    if not answer_text:
        answer_text = "I’m sorry, I couldn’t generate a clean answer from the model output."

    # 5) Return hybrid response: final answer + context chunks
    return ChatResponse(
        answer=answer_text,
        model=model_used,
        used_query=req.query,
        context=context_chunks,
    )

