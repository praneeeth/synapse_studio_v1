// src/components/leftPanel/RagFileUploadSection.jsx
import React from "react";

function RagFileUploadSection({ selectedFile, onFileSelected }) {
  const handleChange = (e) => {
    const file = e.target.files?.[0];
    if (file) {
      onFileSelected(file);
    }
  };

  return (
    <div className="border border-dashed border-synPurple/40 rounded-2xl p-4 bg-synPurpleSoft/40">
      <p className="text-xs font-medium text-slate-800 mb-2">
        Choose a document
      </p>
      <p className="text-[11px] text-slate-500 mb-3">
        Start with PDF files. We&apos;ll index them according to the selected
        chunking method.
      </p>
      <div className="flex items-center gap-2">
        <input
          type="file"
          className="text-[11px]"
          onChange={handleChange}
        />
      </div>
      <p className="text-[10px] text-slate-500 mt-2">
        Selected:{" "}
        {selectedFile ? (
          <span className="font-medium text-slate-800">
            {selectedFile.name}
          </span>
        ) : (
          <span className="text-slate-400">No file selected</span>
        )}
      </p>
      <p className="text-[10px] text-slate-400 mt-1">
        Upload & indexing will start when you click the button under the
        chunking method.
      </p>
    </div>
  );
}

export default RagFileUploadSection;
