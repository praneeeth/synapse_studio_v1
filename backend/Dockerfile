FROM python:3.12-slim

WORKDIR /app

# System deps for scientific-python wheels (numpy/scipy/onnxruntime) that ship
# sdists on some platforms, plus certs for the HuggingFace model downloads.
RUN apt-get update \
    && apt-get install -y --no-install-recommends build-essential ca-certificates \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir --index-url https://download.pytorch.org/whl/cpu torch \
    && pip install --no-cache-dir -r requirements.txt

COPY app ./app

# Pre-download the embedding + reranker models into the image so the first
# request doesn't pay a multi-hundred-MB download on a cold container start.
RUN python -c "from app.chunkers import _semantic_embeddings; _semantic_embeddings()" \
    && python -c "from app.reranker import warmup; warmup()"

ENV CHROMA_DIR=/data/chroma_db \
    DATA_DIR=/data \
    HOST=0.0.0.0 \
    PORT=8000

VOLUME ["/data"]
EXPOSE 8000

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s \
    CMD python -c "import urllib.request; urllib.request.urlopen('http://localhost:8000/health', timeout=3)"

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
