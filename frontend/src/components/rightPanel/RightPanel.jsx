// src/components/rightPanel/RightPanel.jsx
//
// Persona / prompt controls. Now composed from BotIdentityForm and
// PromptEditor, which previously existed as dead, permanently-disabled stubs.

import React from "react";
import BotIdentityForm from "./BotIdentityForm.jsx";
import PromptEditor from "./PromptEditor.jsx";

function RightPanel({
  persona,
  systemPrompt,
  rules,
  onPersonaChange,
  onSystemPromptChange,
  onRulesChange,
  onResetDefaults,
}) {
  return (
    <div className="h-full flex flex-col gap-3 min-h-0">
      <header className="flex items-center justify-between shrink-0">
        <h2 className="text-sm font-semibold text-slate-900">Persona &amp; Prompt</h2>
        <div className="flex items-center gap-2">
          <button
            onClick={onResetDefaults}
            className="text-[10px] px-2 py-0.5 rounded-full border border-synPurpleSoft text-slate-600 hover:bg-synPurpleSoft/50"
          >
            Reset
          </button>
          <span className="text-[10px] px-2 py-0.5 rounded-full bg-synPurpleSoft text-synPurple">
            Behaviour
          </span>
        </div>
      </header>

      <div className="shrink-0">
        <BotIdentityForm persona={persona} onPersonaChange={onPersonaChange} />
      </div>

      <PromptEditor systemPrompt={systemPrompt} onSystemPromptChange={onSystemPromptChange} />

      <div className="border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col text-[11px] shrink-0">
        <div className="flex items-center justify-between mb-1">
          <p className="text-xs font-semibold text-slate-900">Rules &amp; guardrails</p>
          <span className="text-[10px] text-slate-500">Appended to system prompt</span>
        </div>
        <textarea
          aria-label="Rules and guardrails"
          className="border border-synPurpleSoft rounded-lg px-2 py-2 bg-synPurpleSoft/40 text-[11px] text-slate-800 resize-none"
          rows={4}
          value={rules}
          onChange={(e) => onRulesChange(e.target.value)}
        />
        <p className="text-[10px] text-slate-500 mt-1">
          Sent with every request. Changes apply to your next message.
        </p>
      </div>
    </div>
  );
}

export default RightPanel;
