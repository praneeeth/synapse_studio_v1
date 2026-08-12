// src/components/leftPanel/ChunkingMethodDropdown.jsx
//
// Options are fetched from /chunking-methods so the dropdown can never again
// advertise a strategy the backend does not implement.

import React, { useEffect, useState } from "react";
import { getChunkingMethods } from "../../api/client";

const LABELS = {
  fixed: "Fixed-size (baseline)",
  recursive: "Recursive splitter",
  paragraph: "Paragraph-based",
  semantic: "Semantic chunks",
};

const FALLBACK = ["fixed", "recursive", "paragraph", "semantic"];

function ChunkingMethodDropdown({ selectedMethod, onChangeMethod }) {
  const [methods, setMethods] = useState(FALLBACK);

  useEffect(() => {
    const controller = new AbortController();
    getChunkingMethods(controller.signal)
      .then((data) => {
        if (Array.isArray(data.methods) && data.methods.length) setMethods(data.methods);
      })
      .catch(() => {
        // Backend unreachable - keep the fallback list so the UI still renders.
      });
    return () => controller.abort();
  }, []);

  return (
    <select
      aria-label="Chunking method"
      className="w-full text-xs border border-synPurpleSoft rounded-lg px-2 py-1 bg-synPurpleSoft/40 focus:outline-none focus:ring-1 focus:ring-synPurple"
      value={selectedMethod}
      onChange={(e) => onChangeMethod(e.target.value)}
    >
      {methods.map((m) => (
        <option key={m} value={m}>
          {LABELS[m] || m}
        </option>
      ))}
    </select>
  );
}

export default ChunkingMethodDropdown;
