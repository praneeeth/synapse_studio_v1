// src/components/leftPanel/ChunkingMethodSection.jsx
import React from "react";
import ChunkingMethodDropdown from "./ChunkingMethodDropdown.jsx";
import ChunkMethodDefinition from "./ChunkMethodDefinition.jsx";

function ChunkingMethodSection({
  selectedMethod,
  onChangeMethod,
  chunkSize,
  onChangeChunkSize,
  chunkOverlap,
  onChangeChunkOverlap,
  onUploadAndIndex,
  hasPendingFile,
  isUploading,
}) {
  // The semantic splitter decides boundaries by embedding similarity, so
  // overlap is not a parameter it accepts -- chunk size only acts as a cap.
  const overlapDisabled = selectedMethod === "semantic";
  const canSubmit = hasPendingFile && !isUploading;

  return (
    <div className="border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col gap-2">
      <div className="flex items-center justify-between gap-2">
        <p className="text-xs font-semibold text-slate-800">Chunking method</p>
        <span className="text-[10px] text-slate-400">Choose strategy · then index</span>
      </div>

      <ChunkingMethodDropdown selectedMethod={selectedMethod} onChangeMethod={onChangeMethod} />
      <ChunkMethodDefinition method={selectedMethod} />

      <div className="grid grid-cols-2 gap-2 text-[11px]">
        <label className="flex flex-col gap-1">
          <span className="text-slate-600">
            {overlapDisabled ? "Max chunk chars" : "Chunk size (chars)"}
          </span>
          <input
            type="number"
            min={100}
            max={8000}
            step={50}
            value={chunkSize}
            onChange={(e) => onChangeChunkSize(Number(e.target.value))}
            className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-synPurpleSoft/40 text-slate-800"
          />
        </label>
        <label className="flex flex-col gap-1">
          <span className={overlapDisabled ? "text-slate-400" : "text-slate-600"}>
            Overlap (chars)
          </span>
          <input
            type="number"
            min={0}
            max={2000}
            step={10}
            value={overlapDisabled ? 0 : chunkOverlap}
            disabled={overlapDisabled}
            onChange={(e) => onChangeChunkOverlap(Number(e.target.value))}
            title={overlapDisabled ? "Semantic chunking derives its own boundaries" : undefined}
            className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-synPurpleSoft/40 text-slate-800 disabled:opacity-50 disabled:cursor-not-allowed"
          />
        </label>
      </div>

      <div className="pt-1 flex items-center justify-between gap-2 text-[11px]">
        <button
          onClick={onUploadAndIndex}
          disabled={!canSubmit}
          className={`px-3 py-1 rounded-full text-white font-medium ${
            canSubmit ? "bg-synPurple hover:bg-synPurple/90" : "bg-slate-400 cursor-not-allowed"
          }`}
        >
          {isUploading ? "Indexing…" : "Upload & index with this method"}
        </button>
        <span className="text-[10px] text-slate-500">
          {isUploading
            ? "Embedding & storing…"
            : hasPendingFile
              ? "Ready to index"
              : "Select a file above"}
        </span>
      </div>
    </div>
  );
}

export default ChunkingMethodSection;
