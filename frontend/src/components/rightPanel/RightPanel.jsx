// src/components/rightPanel/RightPanel.jsx
import React from "react";

function RightPanel({
  persona,
  systemPrompt,
  rules,
  onPersonaChange,
  onSystemPromptChange,
  onRulesChange,
}) {
  return (
    <div className="h-full flex flex-col gap-4">
      <header className="flex items-center justify-between">
        <h2 className="text-sm font-semibold text-slate-900">
          Persona & Prompt
        </h2>
        <span className="text-[10px] px-2 py-0.5 rounded-full bg-synPurpleSoft text-synPurple">
          Behaviour
        </span>
      </header>

      {/* Persona selector */}
      <div className="border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col gap-2 text-[11px]">
        <p className="text-xs font-semibold text-slate-900">Bot persona</p>
        <select
          className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-synPurpleSoft/40 text-slate-800"
          value={persona}
          onChange={(e) => onPersonaChange?.(e.target.value)}
        >
          <option>RAG expert (default)</option>
          <option>Patient teacher</option>
          <option>Concise analyst</option>
          <option>Friendly helper</option>
        </select>
        <p className="text-[10px] text-slate-500">
          In later versions, this will adjust how your final LLM prompt is
          constructed before sending to the model.
        </p>
      </div>

      {/* System prompt */}
      <div className="border border-synPurpleSoft rounded-2xl p-3 bg-synPurpleSoft/30 flex flex-col text-[11px] flex-1 min-h-0">
        <div className="flex items-center justify-between mb-1">
          <p className="text-xs font-semibold text-slate-900">
            LLM system prompt
          </p>
          <span className="text-[10px] text-slate-500">Controls agent role</span>
        </div>
        <textarea
          className="flex-1 border border-synPurpleSoft rounded-lg px-2 py-2 bg:white/80 bg-white text-[11px] text-slate-800 resize-none mb-2"
          value={systemPrompt}
          onChange={(e) => onSystemPromptChange?.(e.target.value)}
        />
        <p className="text-[10px] text-slate-500 mb-1">
          This will be sent as the system message along with retrieved chunks
          and user query.
        </p>
      </div>

      {/* Rules */}
      <div className="border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col text-[11px]">
        <div className="flex items-center justify-between mb-1">
          <p className="text-xs font-semibold text-slate-900">
            Rules & guardrails
          </p>
          <span className="text-[10px] text-slate-500">Optional</span>
        </div>
        <textarea
          className="border border-synPurpleSoft rounded-lg px-2 py-2 bg-synPurpleSoft/40 text-[11px] text-slate-800 resize-none"
          rows={4}
          value={rules}
          onChange={(e) => onRulesChange?.(e.target.value)}
        />
        <p className="text-[10px] text-slate-500 mt-1">
          These are appended to the system prompt and also kept for debugging.
        </p>
      </div>
    </div>
  );
}

export default RightPanel;
