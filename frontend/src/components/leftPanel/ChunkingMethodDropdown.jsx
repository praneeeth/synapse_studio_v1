// src/components/leftPanel/ChunkingMethodDropdown.jsx
import React from "react";

function ChunkingMethodDropdown({ selectedMethod, onChangeMethod }) {
  return (
    <select
      className="w-full text-xs border border-synPurpleSoft rounded-lg px-2 py-1 bg-synPurpleSoft/40 focus:outline-none focus:ring-1 focus:ring-synPurple"
      value={selectedMethod}
      onChange={(e) => onChangeMethod(e.target.value)}
    >
      <option value="fixed">Fixed-size (baseline)</option>
      <option value="recursive">Recursive splitter</option>
      <option value="paragraph">Paragraph-based</option>
      <option value="semantic">Semantic chunks</option>
    </select>
  );
}

export default ChunkingMethodDropdown;
