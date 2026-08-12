// src/components/leftPanel/RagFileUploadSection.jsx
import React, { useRef, useState } from "react";

const ACCEPT = ".pdf,.docx,.txt,.md,.markdown,.csv,.json,.log,.rst,.yaml,.yml";
const MAX_MB = 25;

function formatSize(bytes) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

function RagFileUploadSection({ selectedFile, onFileSelected }) {
  const [isDragging, setIsDragging] = useState(false);
  const [localError, setLocalError] = useState(null);
  const inputRef = useRef(null);

  // Reject oversized files here so the user finds out instantly instead of
  // after a full upload round-trip that ends in a 413.
  const accept = (file) => {
    if (!file) return;
    if (file.size > MAX_MB * 1024 * 1024) {
      setLocalError(`"${file.name}" is ${formatSize(file.size)} - the limit is ${MAX_MB} MB.`);
      return;
    }
    setLocalError(null);
    onFileSelected(file);
  };

  const handleDrop = (e) => {
    e.preventDefault();
    setIsDragging(false);
    accept(e.dataTransfer.files?.[0]);
  };

  return (
    <div
      onDragOver={(e) => {
        e.preventDefault();
        setIsDragging(true);
      }}
      onDragLeave={() => setIsDragging(false)}
      onDrop={handleDrop}
      className={`border border-dashed rounded-2xl p-3 transition-colors ${
        isDragging
          ? "border-synPurple bg-synPurpleSoft"
          : "border-synPurple/40 bg-synPurpleSoft/40"
      }`}
    >
      <p className="text-xs font-medium text-slate-800 mb-1">Choose a document</p>
      <p className="text-[11px] text-slate-500 mb-2">
        Drag &amp; drop, or browse. PDF, DOCX, and text formats up to {MAX_MB} MB.
      </p>

      <div className="flex items-center gap-2">
        <input
          ref={inputRef}
          type="file"
          accept={ACCEPT}
          className="text-[11px] w-full file:mr-2 file:rounded-full file:border-0 file:bg-synPurple file:px-3 file:py-1 file:text-white file:text-[11px] file:cursor-pointer"
          onChange={(e) => accept(e.target.files?.[0])}
        />
      </div>

      {localError && <p className="text-[10px] text-rose-600 mt-2">{localError}</p>}

      <p className="text-[10px] text-slate-500 mt-2">
        Selected:{" "}
        {selectedFile ? (
          <span className="font-medium text-slate-800">
            {selectedFile.name}{" "}
            <span className="text-slate-500">({formatSize(selectedFile.size)})</span>
          </span>
        ) : (
          <span className="text-slate-400">No file selected</span>
        )}
        {selectedFile && (
          <button
            onClick={() => {
              onFileSelected(null);
              if (inputRef.current) inputRef.current.value = "";
            }}
            className="ml-2 text-synPurple hover:underline"
          >
            clear
          </button>
        )}
      </p>
    </div>
  );
}

export default RagFileUploadSection;
