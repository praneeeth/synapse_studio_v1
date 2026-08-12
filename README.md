# Synapse Studio

Enterprise-grade RAG chatbot studio: upload a document, choose a chunking
strategy, index it into a vector store, then chat against it with cross-encoder
reranked retrieval.

- **Backend**: FastAPI + Chroma (vector store) + sentence-transformers
  (embeddings + reranking) + Groq (LLM).
- **Frontend**: React 19 + Vite + Tailwind.

See [ARCHITECTURE.md](ARCHITECTURE.md) for how the pieces fit together.

## Quick start (local, no Docker)

**Prerequisites**: Python 3.12+, Node.js 22+ (both installable via `winget
install Python.Python.3.12` / `winget install OpenJS.NodeJS.LTS` on Windows).

```powershell
# Terminal 1 - backend (creates a venv on first run)
.\scripts\dev-backend.ps1

# Terminal 2 - frontend
.\scripts\dev-frontend.ps1
```

Then open http://localhost:5173. Paste a Groq API key into the header (get
one free at https://console.groq.com/keys), or set `GROQ_API_KEY` in
`backend/.env` so every user of your deployment doesn't need their own.

Manual setup, if you'd rather not use the scripts:

```powershell
# Backend
cd backend
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install --index-url https://download.pytorch.org/whl/cpu torch   # CPU-only build; drop this line for a CUDA machine
pip install -r requirements-dev.txt
copy .env.example .env   # then edit .env and set GROQ_API_KEY
uvicorn app.main:app --reload --port 8000

# Frontend (separate terminal)
cd frontend
npm install
copy .env.example .env   # only needed if the backend isn't on localhost:8000
npm run dev
```

## Quick start (Docker)

```bash
GROQ_API_KEY=gsk_your_key docker compose up --build
```

Frontend at http://localhost:8080, backend at http://localhost:8000/docs.

## Testing

```powershell
.\scripts\test-backend.ps1        # 29 tests: chunkers, API, prompt assembly
cd frontend; npm run lint; npm run build
```

## Project layout

```
backend/
  app/
    main.py        - FastAPI app, middleware, error handling
    config.py       - all settings, env-driven (backend/.env)
    routers/         - documents.py, chat.py, health.py
    chunkers.py       - fixed / recursive / paragraph / semantic strategies
    vector_store.py    - Chroma wrapper
    reranker.py         - cross-encoder reranking
    llm.py                - Groq client: retries, streaming, error mapping
    rag.py                 - retrieve -> rerank -> prompt assembly
    storage.py               - SQLite document registry (survives restarts)
    parsers.py                - PDF / DOCX / text extraction
  tests/                       - pytest suite
frontend/
  src/
    api/client.js    - typed fetch wrapper, SSE streaming support
    components/        - leftPanel (ingestion), middlePanel (chat),
                          rightPanel (persona/prompt), layout, shared
    hooks/                - useDocuments, useLocalStorage
scripts/                    - PowerShell dev/test scripts
docker-compose.yml
.github/workflows/ci.yml     - lint + test on push/PR
```

## Environment variables

See `backend/.env.example` and `frontend/.env.example` for the full,
commented list. The two that matter to get chatting:

| Variable | Where | Purpose |
|---|---|---|
| `GROQ_API_KEY` | `backend/.env` | LLM calls. Can also be pasted per-session into the header field instead. |
| `VITE_API_BASE_URL` | `frontend/.env` | Only needed if the backend isn't at `http://localhost:8000`. |
