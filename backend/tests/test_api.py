"""End-to-end API tests over the real retrieval stack (no LLM calls)."""

import io

import pytest


def test_health(client):
    r = client.get("/health")
    assert r.status_code == 200
    body = r.json()
    assert body["status"] == "ok"
    assert body["llm_configured"] is False  # conftest clears GROQ_API_KEY


def test_chunking_methods_endpoint(client):
    r = client.get("/chunking-methods")
    assert r.status_code == 200
    assert set(r.json()["methods"]) == {"fixed", "recursive", "paragraph", "semantic"}


def test_empty_upload_rejected(client):
    r = client.post("/upload", files={"file": ("empty.txt", b"", "text/plain")})
    assert r.status_code == 400


def test_unreadable_upload_rejected(client):
    r = client.post("/upload", files={"file": ("blob.bin", b"\x00\x00\x00\x00", "application/octet-stream")})
    assert r.status_code in (400, 415)


def test_index_unknown_file_id(client):
    r = client.post("/index", json={"file_id": "does-not-exist", "method": "fixed"})
    assert r.status_code == 404


def test_index_rejects_unknown_method(client):
    r = client.post("/index", json={"file_id": "x", "method": "telepathy"})
    assert r.status_code == 422  # pydantic Literal rejects it before the handler


@pytest.fixture
def indexed_file(client, sample_text):
    upload = client.post(
        "/upload", files={"file": ("fitness.txt", sample_text.encode(), "text/plain")}
    )
    assert upload.status_code == 200, upload.text
    file_id = upload.json()["file_id"]

    index = client.post(
        "/index", json={"file_id": file_id, "method": "recursive", "chunk_size": 300}
    )
    assert index.status_code == 200, index.text
    yield file_id, index.json()

    client.delete(f"/documents/{file_id}")


def test_upload_index_roundtrip(client, indexed_file):
    file_id, index_body = indexed_file
    assert index_body["total_chunks"] > 0
    assert index_body["method"] == "recursive"
    assert index_body["avg_chunk_chars"] > 0

    listing = client.get("/documents").json()["documents"]
    row = next(d for d in listing if d["file_id"] == file_id)
    assert row["status"] == "indexed"
    assert row["total_chunks"] == index_body["total_chunks"]


def test_file_and_chunk_previews(client, indexed_file):
    file_id, index_body = indexed_file

    preview = client.get(f"/documents/{file_id}/preview").json()
    assert "Progressive overload" in preview["text"]
    assert preview["truncated"] is False

    chunks = client.get(f"/documents/{file_id}/chunks", params={"limit": 100}).json()
    assert chunks["total_chunks"] == index_body["total_chunks"]
    indices = [c["chunk_index"] for c in chunks["chunks"]]
    assert indices == sorted(indices), "chunks must come back in order"


def test_query_rerank_finds_relevant_chunk(client, indexed_file):
    file_id, _ = indexed_file
    r = client.post(
        "/query_rerank", json={"query": "How much sleep do I need?", "file_id": file_id, "top_k": 2}
    )
    assert r.status_code == 200
    results = r.json()["results"]
    assert results, "expected at least one hit"
    assert "sleep" in " ".join(x["text"].lower() for x in results)


def test_reindexing_replaces_chunks_instead_of_duplicating(client, sample_text):
    """Re-indexing with a new method must not leave the old vectors behind."""
    file_id = client.post(
        "/upload", files={"file": ("re.txt", sample_text.encode(), "text/plain")}
    ).json()["file_id"]

    first = client.post("/index", json={"file_id": file_id, "method": "fixed", "chunk_size": 200})
    second = client.post("/index", json={"file_id": file_id, "method": "paragraph", "chunk_size": 800})
    assert first.status_code == second.status_code == 200

    stored = client.get(f"/documents/{file_id}/chunks", params={"limit": 500}).json()
    assert stored["total_chunks"] == second.json()["total_chunks"]
    assert stored["total_chunks"] != first.json()["total_chunks"]

    client.delete(f"/documents/{file_id}")


def test_delete_removes_document_and_vectors(client, sample_text):
    file_id = client.post(
        "/upload", files={"file": ("gone.txt", sample_text.encode(), "text/plain")}
    ).json()["file_id"]
    client.post("/index", json={"file_id": file_id, "method": "fixed"})

    deleted = client.delete(f"/documents/{file_id}")
    assert deleted.status_code == 200
    assert deleted.json()["deleted_chunks"] > 0

    assert client.get(f"/documents/{file_id}/preview").status_code == 404
    assert client.delete(f"/documents/{file_id}").status_code == 404


def test_chat_without_api_key_returns_503(client):
    """A missing key is a configuration problem (503), not a generic 500."""
    r = client.post("/chat_rag", json={"query": "hello"})
    assert r.status_code == 503
    assert "not configured" in r.json()["detail"].lower()


def test_stats_reflect_indexed_documents(client, indexed_file):
    stats = client.get("/stats").json()
    assert stats["files_indexed"] >= 1
    assert stats["total_chunks"] >= 1
    assert stats["embed_model"]
