// src/components/leftPanel/LeftPanel.jsx
import React, { useState } from "react";
import { API_BASE_URL } from "../../api/client";

function LeftPanel({ onActiveFileChange, activeFileId }) {
  const [selectedFile, setSelectedFile] = useState(null);
  const [files, setFiles] = useState([]);
  const [menuOpenFor, setMenuOpenFor] = useState(null);
  const [activePreview, setActivePreview] = useState(null);
  const [isUploading, setIsUploading] = useState(false);

  const handleFileChange = (e) => {
    const file = e.target.files?.[0];
    if (file) setSelectedFile(file);
  };

  const handleUploadClick = async () => {
    if (!selectedFile || isUploading) return;

    try {
      setIsUploading(true);

      const formData = new FormData();
      formData.append("file", selectedFile);

      const uploadRes = await fetch(`${API_BASE_URL}/upload`, {
        method: "POST",
        body: formData,
      });

      if (!uploadRes.ok) {
        console.error("Upload failed:", await uploadRes.text());
        setIsUploading(false);
        return;
      }

      const uploadData = await uploadRes.json();

      const newFile = {
        id: uploadData.file_id,
        name: uploadData.file_name,
        status: "indexing",
        numChars: uploadData.num_chars,
        totalChunks: null,
      };

      setFiles((prev) => [...prev, newFile]);
      setSelectedFile(null);

      await indexFileOnServer(uploadData.file_id, uploadData.file_name);
    } catch (err) {
      console.error("Upload error:", err);
    } finally {
      setIsUploading(false);
    }
  };

  const indexFileOnServer = async (fileId, fileName) => {
    try {
      setFiles((prev) =>
        prev.map((f) =>
          f.id === fileId ? { ...f, status: "indexing" } : f
        )
      );

      const res = await fetch(`${API_BASE_URL}/index`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ file_id: fileId }),
      });

      if (!res.ok) {
        console.error("Index failed:", await res.text());
        setFiles((prev) =>
          prev.map((f) =>
            f.id === fileId ? { ...f, status: "index failed" } : f
          )
        );
        return;
      }

      const data = await res.json();

      setFiles((prev) =>
        prev.map((f) =>
          f.id === fileId
            ? { ...f, status: "indexed", totalChunks: data.total_chunks }
            : f
        )
      );

      // Automatically make this the active file for chat
      onActiveFileChange?.(fileId, fileName);
    } catch (err) {
      console.error("Index error:", err);
      setFiles((prev) =>
        prev.map((f) =>
          f.id === fileId ? { ...f, status: "index failed" } : f
        )
      );
    }
  };

  const toggleMenu = (fileId) => {
    setMenuOpenFor((prev) => (prev === fileId ? null : fileId));
  };

  const handlePreviewClick = (fileId, type) => {
    setActivePreview({ fileId, type });
    setMenuOpenFor(null);
  };

  const handleRowClick = (file) => {
    onActiveFileChange?.(file.id, file.name);
  };

  const currentFile =
    activePreview && files.find((f) => f.id === activePreview.fileId);

  return (
    <div className="h-full flex flex-col gap-4">
      <header className="flex items-center justify-between">
        <h2 className="text-sm font-semibold text-slate-900">
          Files & Indexing
        </h2>
        <span className="text-[10px] px-2 py-0.5 rounded-full bg-synPurpleSoft text-synPurple">
          Ingestion
        </span>
      </header>

      {/* Upload area */}
      <div className="border border-dashed border-synPurple/40 rounded-2xl p-4 bg-synPurpleSoft/40 flex flex-col gap-3">
        <div>
          <p className="text-xs font-medium text-slate-800 mb-1">
            Upload document
          </p>
          <p className="text-[11px] text-slate-500">
            Start with text / PDF files. We&apos;ll upload, semantically chunk, and
            index them for RAG.
          </p>
        </div>

        <div className="flex items-center gap-2 text-[11px]">
          <input type="file" className="text-[11px]" onChange={handleFileChange} />
          <button
            onClick={handleUploadClick}
            disabled={!selectedFile || isUploading}
            className={`px-3 py-1 rounded-full text-white font-medium ${
              selectedFile && !isUploading
                ? "bg-synPurple hover:bg-synPurple/90"
                : "bg-slate-400 cursor-not-allowed"
            }`}
          >
            {isUploading ? "Uploading…" : "Upload & Index"}
          </button>
        </div>

        <p className="text-[10px] text-slate-500">
          Selected:{" "}
          {selectedFile ? (
            <span className="font-medium text-slate-800">
              {selectedFile.name}
            </span>
          ) : (
            <span className="text-slate-400">No file selected</span>
          )}
        </p>
      </div>

      {/* Uploaded files list */}
      <div className="border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col flex-1 min-h-0">
        <p className="text-xs font-semibold text-slate-800 mb-2">
          Uploaded files
        </p>

        {files.length === 0 ? (
          <p className="text-[11px] text-slate-500">
            No files yet. Upload a document above to see it listed here.
          </p>
        ) : (
          <div className="flex-1 min-h-0 overflow-auto space-y-1 text-[11px]">
            {files.map((file) => {
              const isActive = file.id === activeFileId;
              return (
                <div
                  key={file.id}
                  className={`flex items-center justify-between gap-2 px-2 py-1 rounded-lg border ${
                    isActive
                      ? "border-synPurple bg-synPurpleSoft/50"
                      : "border-synPurpleSoft/60 bg-synPurpleSoft/20"
                  } cursor-pointer`}
                  onClick={() => handleRowClick(file)}
                >
                  <div className="flex flex-col">
                    <span className="font-semibold text-slate-800 truncate max-w-[150px]">
                      {file.name}
                    </span>
                    <span className="text-[10px] text-slate-500">
                      Status: {file.status}
                      {file.totalChunks != null &&
                        ` · ${file.totalChunks} chunks`}
                      {isActive && " · Active for chat"}
                    </span>
                  </div>

                  <div className="relative" onClick={(e) => e.stopPropagation()}>
                    <button
                      onClick={() => toggleMenu(file.id)}
                      className="w-7 h-7 rounded-full flex items-center justify-center hover:bg-synPurpleSoft/80"
                    >
                      <span className="text-lg leading-none text-slate-700">
                        ⋯
                      </span>
                    </button>

                    {menuOpenFor === file.id && (
                      <div className="absolute right-0 mt-1 w-36 rounded-lg bg-white border border-synPurpleSoft shadow-soft z-10">
                        <button
                          onClick={() => handlePreviewClick(file.id, "file")}
                          className="w-full text-left text-[11px] px-3 py-1.5 hover:bg-synPurpleSoft/40"
                        >
                          File preview
                        </button>
                        <button
                          onClick={() => handlePreviewClick(file.id, "chunk")}
                          className="w-full text-left text-[11px] px-3 py-1.5 hover:bg-synPurpleSoft/40"
                        >
                          Chunk preview
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Preview box */}
      <div className="border border-synPurpleSoft rounded-2xl p-3 bg-synPurpleSoft/20 text-[11px]">
        <p className="text-xs font-semibold text-slate-800 mb-1">Preview</p>
        {currentFile && activePreview ? (
          <>
            <p className="font-semibold text-slate-800 mb-1">
              {activePreview.type === "file" ? "File preview" : "Chunk preview"}
              :{" "}
              <span className="font-normal">{currentFile.name}</span>
            </p>
            <p className="text-slate-600">
              This will later show actual file or chunk content from the
              backend. For now it marks which file and view mode you picked.
            </p>
          </>
        ) : (
          <p className="text-slate-500">
            Use the ⋯ menu for a file to open file preview or chunk preview.
          </p>
        )}
      </div>
    </div>
  );
}

export default LeftPanel;
