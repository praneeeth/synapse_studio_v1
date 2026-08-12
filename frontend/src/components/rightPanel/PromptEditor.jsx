// src/components/rightPanel/PromptEditor.jsx
//
// Was a dead `disabled` textarea with a `defaultValue` that never reached the
// backend. It is now the controlled editor for the live system prompt.

import React from "react";

function PromptEditor({ systemPrompt, onSystemPromptChange }) {
  return (
    <div className="flex-1 min-h-0 border border-synPurpleSoft rounded-2xl p-3 bg-synPurpleSoft/30 flex flex-col text-[11px]">
      <div className="flex items-center justify-between mb-1 shrink-0">
        <p className="text-xs font-semibold text-slate-900">LLM system prompt</p>
        <span className="text-[10px] text-slate-500">{systemPrompt.length} chars</span>
      </div>

      <textarea
        aria-label="LLM system prompt"
        className="flex-1 min-h-0 border border-synPurpleSoft rounded-lg px-2 py-2 bg-white text-[11px] text-slate-800 resize-none focus:outline-none focus:ring-1 focus:ring-synPurple"
        value={systemPrompt}
        onChange={(e) => onSystemPromptChange(e.target.value)}
      />

      <p className="mt-1 text-[10px] text-slate-500 shrink-0">
        Sent as the system message alongside the retrieved chunks and your question.
      </p>
    </div>
  );
}

export default PromptEditor;
