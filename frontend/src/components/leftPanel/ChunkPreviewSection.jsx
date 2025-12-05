// src/components/leftPanel/ChunkPreviewSection.jsx
import React from "react";

function statusBadge(status) {
  if (status === "indexing") {
    return (
      <span className="text-[10px] px-2 py-0.5 rounded-full bg-amber-100 text-amber-700 border border-amber-200">
        Indexing…
      </span>
    );
  }
  if (status === "indexed") {
    return (
      <span className="text-[10px] px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-700 border border-emerald-200">
        Indexed
      </span>
    );
  }
  return (
    <span className="text-[10px] px-2 py-0.5 rounded-full bg-slate-100 text-slate-600 border border-slate-200">
      Pending
    </span>
  );
}

function ChunkPreviewSection({ files, activePreviewId, onSelectFile }) {
  const activeFile = files.find((f) => f.id === activePreviewId) || null;

  return (
    <div className="flex-1 border border-synPurpleSoft rounded-2xl p-3 bg-synPurpleSoft/20 flex flex-col">
      <p className="text-xs font-semibold text-slate-800 mb-1">
        Preview
      </p>

      {/* File list */}
      {files.length === 0 ? (
        <p className="text-[11px] text-slate-500">
          No files yet. Select a file, choose a chunking method, and click
          &quot;Upload & index&quot; to see it here.
        </p>
      ) : (
        <div className="max-h-32 overflow-auto text-[11px] mb-2 space-y-1">
          {files.map((file) => (
            <button
              key={file.id}
              onClick={() => onSelectFile(file.id)}
              className={`w-full flex items-center justify-between px-2 py-1 rounded-lg border ${
                activePreviewId === file.id
                  ? "bg-white border-synPurpleSoft"
                  : "bg-white/60 border-synPurpleSoft/60 hover:bg-white"
              }`}
            >
              <div className="flex flex-col text-left">
                <span className="font-semibold text-slate-800 truncate max-w-[160px]">
                  {file.name}
                </span>
                <span className="text-[10px] text-slate-500">
                  Method: {file.method}
                </span>
              </div>
              {statusBadge(file.status)}
            </button>
          ))}
        </div>
      )}

      {/* Preview window */}
      <div className="mt-1 flex-1 rounded-xl bg-white/80 border border-synPurpleSoft px-2 py-2 text-[11px]">
        {activeFile ? (
          <>
            <p className="font-semibold text-slate-800 mb-1">
              Preview: {activeFile.name}
            </p>
            <p className="text-slate-600 mb-1">
              Status:{" "}
              <span className="font-medium capitalize">
                {activeFile.status}
              </span>
            </p>
            <p className="text-slate-600 mb-1">
              Chunking method:{" "}
              <span className="font-medium">{activeFile.method}</span>
            </p>
            <p className="text-slate-500 mt-1">
              Here we will show the indexing details: number of chunks, sample
              snippets, and metadata. Once we connect the backend, this area
              becomes your &quot;indexing inspector&quot; for each file.
            </p>
          </>
        ) : (
          <p className="text-slate-500">
            Click a file above to see its preview and indexing details.
          </p>
        )}
      </div>
    </div>
  );
}

export default ChunkPreviewSection;
