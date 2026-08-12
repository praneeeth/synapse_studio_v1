// src/components/rightPanel/BotIdentityForm.jsx
//
// Was a pair of permanently `disabled` inputs with placeholder text. Now it is
// the live persona selector and feeds the system prompt sent to the LLM.

import React from "react";
import { PERSONAS } from "../../constants.js";

function BotIdentityForm({ persona, onPersonaChange }) {
  const active = PERSONAS.find((p) => p.value === persona);

  return (
    <div className="border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col gap-2 text-[11px]">
      <p className="text-xs font-semibold text-slate-900">Bot persona</p>

      <select
        aria-label="Bot persona"
        className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-synPurpleSoft/40 text-slate-800"
        value={persona}
        onChange={(e) => onPersonaChange(e.target.value)}
      >
        {PERSONAS.map((p) => (
          <option key={p.value} value={p.value}>
            {p.value}
          </option>
        ))}
      </select>

      <p className="text-[10px] text-slate-500">
        {active?.hint ?? "Prepended to the system prompt as the assistant's persona."}
      </p>
    </div>
  );
}

export default BotIdentityForm;
