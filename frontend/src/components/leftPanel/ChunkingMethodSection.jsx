// src/components/leftPanel/ChunkingMethodSection.jsx
import React from "react";
import ChunkingMethodDropdown from "./ChunkingMethodDropdown.jsx";
import ChunkMethodDefinition from "./ChunkMethodDefinition.jsx";

function ChunkingMethodSection({
  selectedMethod,
  onChangeMethod,
  onUploadAndIndex,
  hasPendingFile,
}) {
  return (
    <div className="border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col gap-3">
      <div className="flex items-center justify-between gap-2">
        <p className="text-xs font-semibold text-slate-800">Chunking method</p>
        <span className="text-[10px] text-slate-400">
          Choose strategy · then index
        </span>
      </div>

      {/* Dropdown – select method */}
      <ChunkingMethodDropdown
        selectedMethod={selectedMethod}
        onChangeMethod={onChangeMethod}
      />

      {/* Definition – what it is & when to use */}
      <ChunkMethodDefinition method={selectedMethod} />

      {/* Upload & index button */}
      <div className="pt-1 flex items-center justify-between gap-2 text-[11px]">
        <button
          onClick={onUploadAndIndex}
          className={`px-3 py-1 rounded-full text-white font-medium ${
            hasPendingFile
              ? "bg-synPurple hover:bg-synPurple/90"
              : "bg-slate-400 cursor-not-allowed"
          }`}
        >
          Upload & index with this method
        </button>
        <span className="text-[10px] text-slate-500">
          {hasPendingFile
            ? "Ready to index selected file"
            : "Select a file above to enable"}
        </span>
      </div>
    </div>
  );
}

export default ChunkingMethodSection;
