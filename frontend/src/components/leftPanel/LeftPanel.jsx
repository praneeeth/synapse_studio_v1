// src/components/leftPanel/LeftPanel.jsx
//
// Ingestion panel. This now actually uses ChunkingMethodSection /
// RagFileUploadSection / ChunkPreviewSection -- components that existed in the
// repo but were never imported, so the chunking-method dropdown the UI
// advertised was unreachable and every upload silently used semantic chunking.

import React, { useState } from "react";
import RagFileUploadSection from "./RagFileUploadSection.jsx";
import ChunkingMethodSection from "./ChunkingMethodSection.jsx";
import ChunkPreviewSection from "./ChunkPreviewSection.jsx";

function LeftPanel({
  documents,
  isLoading,
  busyIds,
  activeFileId,
  onActiveFileChange,
  onUploadAndIndex,
  onReindex,
  onDelete,
}) {
  const [selectedFile, setSelectedFile] = useState(null);
  const [method, setMethod] = useState("semantic");
  const [chunkSize, setChunkSize] = useState(900);
  const [chunkOverlap, setChunkOverlap] = useState(120);
  const [isUploading, setIsUploading] = useState(false);

  const handleUploadAndIndex = async () => {
    if (!selectedFile || isUploading) return;
    setIsUploading(true);
    try {
      const created = await onUploadAndIndex(selectedFile, { method, chunkSize, chunkOverlap });
      if (created) {
        setSelectedFile(null);
        onActiveFileChange(created.file_id, created.file_name);
      }
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="h-full flex flex-col gap-3 min-h-0">
      <header className="flex items-center justify-between shrink-0">
        <h2 className="text-sm font-semibold text-slate-900">Files &amp; Indexing</h2>
        <span className="text-[10px] px-2 py-0.5 rounded-full bg-synPurpleSoft text-synPurple">
          Ingestion
        </span>
      </header>

      <div className="shrink-0">
        <RagFileUploadSection selectedFile={selectedFile} onFileSelected={setSelectedFile} />
      </div>

      <div className="shrink-0">
        <ChunkingMethodSection
          selectedMethod={method}
          onChangeMethod={setMethod}
          chunkSize={chunkSize}
          onChangeChunkSize={setChunkSize}
          chunkOverlap={chunkOverlap}
          onChangeChunkOverlap={setChunkOverlap}
          onUploadAndIndex={handleUploadAndIndex}
          hasPendingFile={Boolean(selectedFile)}
          isUploading={isUploading}
        />
      </div>

      <ChunkPreviewSection
        documents={documents}
        isLoading={isLoading}
        busyIds={busyIds}
        activeFileId={activeFileId}
        onSelectFile={onActiveFileChange}
        onReindex={(fileId) => onReindex(fileId, { method, chunkSize, chunkOverlap })}
        onDelete={onDelete}
        currentMethod={method}
      />
    </div>
  );
}

export default LeftPanel;
