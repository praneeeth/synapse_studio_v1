// src/api/client.js
//
// Single place where the frontend talks to the backend. Previously this file
// held only a hardcoded URL and every component hand-rolled its own fetch,
// duplicating error handling (and mostly just console.error-ing failures).

export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") || "http://localhost:8000";

/** Error carrying the backend's HTTP status and `detail` message. */
export class ApiError extends Error {
  constructor(message, status, detail) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.detail = detail;
  }
}

async function toApiError(response) {
  let detail = response.statusText;
  try {
    const body = await response.json();
    if (body?.detail) {
      // FastAPI validation errors arrive as an array of objects.
      detail = Array.isArray(body.detail)
        ? body.detail.map((d) => d.msg || JSON.stringify(d)).join("; ")
        : body.detail;
    }
  } catch {
    // Non-JSON error body - keep the status text.
  }
  return new ApiError(detail, response.status, detail);
}

async function request(path, { method = "GET", body, signal, headers } = {}) {
  const isFormData = body instanceof FormData;

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    signal,
    headers: {
      ...(isFormData || body === undefined ? {} : { "Content-Type": "application/json" }),
      ...headers,
    },
    body: isFormData ? body : body === undefined ? undefined : JSON.stringify(body),
  });

  if (!response.ok) throw await toApiError(response);
  if (response.status === 204) return null;
  return response.json();
}

// ------------------------------------------------------------------ health --

export const getHealth = (signal) => request("/health", { signal });
export const getStats = (signal) => request("/stats", { signal });
export const getChunkingMethods = (signal) => request("/chunking-methods", { signal });

// --------------------------------------------------------------- documents --

export function uploadFile(file, signal) {
  const formData = new FormData();
  formData.append("file", file);
  return request("/upload", { method: "POST", body: formData, signal });
}

export const indexFile = ({ fileId, method = "semantic", chunkSize, chunkOverlap }, signal) =>
  request("/index", {
    method: "POST",
    signal,
    body: {
      file_id: fileId,
      method,
      ...(chunkSize ? { chunk_size: chunkSize } : {}),
      ...(chunkOverlap != null ? { chunk_overlap: chunkOverlap } : {}),
    },
  });

export const listDocuments = (signal) => request("/documents", { signal });
export const getFilePreview = (fileId, signal) =>
  request(`/documents/${fileId}/preview`, { signal });
export const getChunkPreview = (fileId, { limit = 50, offset = 0 } = {}, signal) =>
  request(`/documents/${fileId}/chunks?limit=${limit}&offset=${offset}`, { signal });
export const deleteDocument = (fileId, signal) =>
  request(`/documents/${fileId}`, { method: "DELETE", signal });

// -------------------------------------------------------------------- chat --

export const queryRerank = ({ query, fileId, topK = 5 }, signal) =>
  request("/query_rerank", {
    method: "POST",
    signal,
    body: { query, file_id: fileId ?? null, top_k: topK },
  });

function chatBody({ query, fileId, topK, persona, systemPrompt, rules, history, apiKey }) {
  return {
    query,
    file_id: fileId ?? null,
    top_k: topK ?? 4,
    persona: persona ?? null,
    system_prompt: systemPrompt ?? null,
    rules: rules ?? null,
    history: history ?? [],
    debug: true,
    ...(apiKey ? { llm_api_key: apiKey } : {}),
  };
}

export const chatRag = (options, signal) =>
  request("/chat_rag", { method: "POST", signal, body: chatBody(options) });

/**
 * Streaming chat over SSE.
 *
 * `fetch` is used rather than EventSource because the request must be a POST
 * with a JSON body, which EventSource cannot do.
 *
 * Callbacks: onContext(chunks), onToken(text), onDone({took_ms}).
 */
export async function chatRagStream(options, { onContext, onToken, onDone, signal } = {}) {
  const response = await fetch(`${API_BASE_URL}/chat_rag/stream`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(chatBody(options)),
    signal,
  });

  if (!response.ok) throw await toApiError(response);
  if (!response.body) throw new ApiError("Streaming is not supported by this browser", 0);

  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    buffer += decoder.decode(value, { stream: true });

    // SSE frames are separated by a blank line.
    const frames = buffer.split("\n\n");
    buffer = frames.pop() ?? "";

    for (const frame of frames) {
      const eventLine = frame.split("\n").find((l) => l.startsWith("event: "));
      const dataLine = frame.split("\n").find((l) => l.startsWith("data: "));
      if (!eventLine || !dataLine) continue;

      const event = eventLine.slice(7).trim();
      let payload;
      try {
        payload = JSON.parse(dataLine.slice(6));
      } catch {
        continue;
      }

      if (event === "context") onContext?.(payload.context || []);
      else if (event === "token") onToken?.(payload.token || "");
      else if (event === "done") onDone?.(payload);
      else if (event === "error") throw new ApiError(payload.detail || "Stream failed", 502);
    }
  }
}
