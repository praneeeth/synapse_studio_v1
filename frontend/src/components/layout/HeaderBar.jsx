// src/components/layout/HeaderBar.jsx
import React from "react";

function HeaderBar({ apiKey, onApiKeyChange }) {
  return (
    <header className="w-full border-b border-synPurpleSoft bg-white/80 backdrop-blur-sm">
      <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between gap-4">
        <div className="flex items-center gap-2">
          <div className="w-9 h-9 rounded-2xl bg-synPurple text-white flex items-center justify-center text-sm font-bold shadow-soft">
            SS
          </div>
          <div>
            <h1 className="text-lg font-semibold text-slate-900">
              Synapse Studio
            </h1>
            <p className="text-xs text-slate-500">Semantic RAG · Hybrid Chat</p>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 text-xs">
            <span className="text-[11px] text-slate-500">Groq API key</span>
            <input
              type="password"
              className="rounded-full border border-synPurpleSoft px-3 py-1 bg-synPurpleSoft/40 text-[11px] text-slate-800 focus:outline-none focus:ring-1 focus:ring-synPurple"
              placeholder="sk-..."
              value={apiKey}
              onChange={(e) => onApiKeyChange?.(e.target.value)}
            />
          </div>
          <span className="px-2 py-1 rounded-full bg-synPurpleSoft text-synPurple font-medium text-[11px]">
            Dev
          </span>
        </div>
      </div>
    </header>
  );
}

export default HeaderBar;
