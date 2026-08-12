// src/components/leftPanel/ChunkPreviewSection.jsx
//
// Document list + inspector. The previous version rendered the placeholder
// "This will later show actual file or chunk content from the backend" -- it
// now fetches the real extracted text and the real indexed chunks.

import React, { useCallback, useEffect, useState } from "react";
import { getChunkPreview, getFilePreview } from "../../api/client";

function StatusBadge({ status }) {
  const styles = {
    indexing: "bg-amber-100 text-amber-700 border-amber-200",
    indexed: "bg-emerald-100 text-emerald-700 border-emerald-200",
    failed: "bg-rose-100 text-rose-700 border-rose-200",
    uploaded: "bg-slate-100 text-slate-600 border-slate-200",
  };
  const labels = {
    indexing: "Indexing…",
    indexed: "Indexed",
    failed: "Failed",
    uploaded: "Pending",
  };
  return (
    <span
      className={`text-[10px] px-2 py-0.5 rounded-full border shrink-0 ${
        styles[status] || styles.uploaded
      }`}
    >
      {labels[status] || status}
    </span>
  );
}

function ChunkPreviewSection({
  documents,
  isLoading,
  busyIds,
  activeFileId,
  onSelectFile,
  onReindex,
  onDelete,
  currentMethod,
}) {
  const [mode, setMode] = useState("chunks"); // "chunks" | "file"
  const [preview, setPreview] = useState(null);
  const [previewError, setPreviewError] = useState(null);
  const [isFetching, setIsFetching] = useState(false);

  const activeDoc = documents.find((d) => d.file_id === activeFileId) || null;

  const loadPreview = useCallback(
    async (signal) => {
      if (!activeFileId || !activeDoc) {
        setPreview(null);
        return;
      }
      setIsFetching(true);
      setPreviewError(null);
      try {
        const data =
          mode === "file"
            ? await getFilePreview(activeFileId, signal)
            : await getChunkPreview(activeFileId, { limit: 100 }, signal);
        setPreview(data);
      } catch (err) {
        if (err.name !== "AbortError") {
          setPreview(null);
          setPreviewError(err.detail || err.message);
        }
      } finally {
        setIsFetching(false);
      }
    },
    // activeDoc.status is in the deps so the preview refreshes when indexing ends.
    [activeFileId, mode, activeDoc?.status] // eslint-disable-line react-hooks/exhaustive-deps
  );

  useEffect(() => {
    const controller = new AbortController();
    loadPreview(controller.signal);
    return () => controller.abort();
  }, [loadPreview]);

  return (
    <div className="flex-1 min-h-0 border border-synPurpleSoft rounded-2xl p-3 bg-synPurpleSoft/20 flex flex-col gap-2">
      <div className="flex items-center justify-between shrink-0">
        <p className="text-xs font-semibold text-slate-800">
          Uploaded files{documents.length > 0 && ` (${documents.length})`}
        </p>
        {activeDoc && (
          <div className="flex rounded-full border border-synPurpleSoft overflow-hidden text-[10px]">
            {["chunks", "file"].map((m) => (
              <button
                key={m}
                onClick={() => setMode(m)}
                className={`px-2 py-0.5 ${
                  mode === m ? "bg-synPurple text-white" : "bg-white text-slate-600"
                }`}
              >
                {m === "chunks" ? "Chunks" : "Raw text"}
              </button>
            ))}
          </div>
        )}
      </div>

      {/* File list */}
      {isLoading ? (
        <p className="text-[11px] text-slate-500">Loading documents…</p>
      ) : documents.length === 0 ? (
        <p className="text-[11px] text-slate-500">
          No files yet. Select a file, choose a chunking method, and click
          &quot;Upload &amp; index&quot;.
        </p>
      ) : (
        <div className="max-h-36 overflow-auto text-[11px] space-y-1 shrink-0">
          {documents.map((doc) => {
            const isActive = doc.file_id === activeFileId;
            const isBusy = busyIds.has(doc.file_id);
            return (
              <div
                key={doc.file_id}
                onClick={() => onSelectFile(doc.file_id, doc.file_name)}
                className={`w-full flex items-center justify-between gap-2 px-2 py-1 rounded-lg border cursor-pointer ${
                  isActive
                    ? "bg-white border-synPurple"
                    : "bg-white/60 border-synPurpleSoft/60 hover:bg-white"
                }`}
              >
                <div className="flex flex-col text-left min-w-0">
                  <span className="font-semibold text-slate-800 truncate max-w-[150px]">
                    {doc.file_name}
                  </span>
                  <span className="text-[10px] text-slate-500">
                    {doc.method || "—"}
                    {doc.total_chunks != null && ` · ${doc.total_chunks} chunks`}
                    {isActive && " · active"}
                  </span>
                </div>

                <div className="flex items-center gap-1 shrink-0">
                  <StatusBadge status={doc.status} />
                  <button
                    title="Re-index with the currently selected method"
                    disabled={isBusy}
                    onClick={(e) => {
                      e.stopPropagation();
                      onReindex(doc.file_id);
                    }}
                    className="w-6 h-6 rounded-full hover:bg-synPurpleSoft disabled:opacity-40 text-slate-600"
                  >
                    ↻
                  </button>
                  <button
                    title={`Delete ${doc.file_name}`}
                    disabled={isBusy}
                    onClick={(e) => {
                      e.stopPropagation();
                      if (window.confirm(`Delete "${doc.file_name}" and its vectors?`)) {
                        onDelete(doc.file_id);
                      }
                    }}
                    className="w-6 h-6 rounded-full hover:bg-rose-100 disabled:opacity-40 text-rose-600"
                  >
                    ×
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Inspector */}
      <div className="flex-1 min-h-0 rounded-xl bg-white/80 border border-synPurpleSoft px-2 py-2 text-[11px] overflow-auto">
        {!activeDoc ? (
          <p className="text-slate-500">Click a file above to inspect its content.</p>
        ) : previewError ? (
          <p className="text-rose-600">Could not load preview: {previewError}</p>
        ) : isFetching ? (
          <p className="text-slate-500">Loading preview…</p>
        ) : activeDoc.status === "failed" ? (
          <p className="text-rose-600">
            Indexing failed: {activeDoc.error || "unknown error"}. Try a different chunking method.
          </p>
        ) : mode === "file" ? (
          <>
            <p className="font-semibold text-slate-800 mb-1">
              {preview?.file_name} · {preview?.num_chars?.toLocaleString()} chars
              {preview?.truncated && " (truncated)"}
            </p>
            <pre className="whitespace-pre-wrap break-words text-slate-700 font-sans text-[10px]">
              {preview?.text}
            </pre>
          </>
        ) : preview?.chunks?.length ? (
          <>
            <p className="font-semibold text-slate-800 mb-1">
              {preview.total_chunks} chunks · method {preview.method || currentMethod}
            </p>
            <div className="space-y-1">
              {preview.chunks.map((c) => (
                <div
                  key={c.chunk_index}
                  className="border border-synPurpleSoft/70 rounded-md p-1.5 bg-synPurpleSoft/20"
                >
                  <div className="text-[9px] text-slate-500 mb-0.5">
                    chunk {c.chunk_index} · {c.num_chars} chars
                  </div>
                  <div className="text-[10px] text-slate-700 whitespace-pre-wrap break-words">
                    {c.text}
                  </div>
                </div>
              ))}
            </div>
          </>
        ) : (
          <p className="text-slate-500">
            No chunks stored yet{activeDoc.status === "indexing" ? " — indexing in progress." : "."}
          </p>
        )}
      </div>
    </div>
  );
}

export default ChunkPreviewSection;
