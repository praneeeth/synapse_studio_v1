// src/components/layout/HeaderBar.jsx
import React, { useState } from "react";

function HealthDot({ health, onRetry }) {
  const map = {
    checking: { color: "bg-amber-400", label: "Connecting…" },
    online: { color: "bg-emerald-500", label: "Backend online" },
    offline: { color: "bg-rose-500", label: "Backend unreachable" },
  };
  const { color, label } = map[health.state] || map.checking;

  return (
    <button
      onClick={onRetry}
      title={
        health.state === "online"
          ? `v${health.version} · ${health.chunks_indexed} chunks indexed · click to refresh`
          : "Click to retry"
      }
      className="flex items-center gap-1.5 text-[11px] text-slate-600 hover:text-slate-900"
    >
      <span className={`w-2 h-2 rounded-full ${color}`} />
      {label}
    </button>
  );
}

function HeaderBar({ apiKey, onApiKeyChange, health, onRetryHealth }) {
  const [revealed, setRevealed] = useState(false);

  return (
    <header className="w-full border-b border-synPurpleSoft bg-white/80 backdrop-blur-sm shrink-0">
      <div className="w-full max-w-[1600px] mx-auto px-4 py-2.5 flex items-center justify-between gap-4">
        <div className="flex items-center gap-2">
          <div className="w-9 h-9 rounded-2xl bg-synPurple text-white flex items-center justify-center text-sm font-bold shadow-soft">
            SS
          </div>
          <div>
            <h1 className="text-lg font-semibold text-slate-900 leading-tight">Synapse Studio</h1>
            <p className="text-xs text-slate-500">Semantic RAG · Reranked · Hybrid Chat</p>
          </div>
        </div>

        <div className="flex items-center gap-3">
          <HealthDot health={health} onRetry={onRetryHealth} />

          <div className="flex items-center gap-1.5 text-xs">
            <label htmlFor="groq-key" className="text-[11px] text-slate-500">
              Groq API key
            </label>
            <input
              id="groq-key"
              type={revealed ? "text" : "password"}
              autoComplete="off"
              spellCheck={false}
              className="rounded-full border border-synPurpleSoft px-3 py-1 bg-synPurpleSoft/40 text-[11px] text-slate-800 focus:outline-none focus:ring-1 focus:ring-synPurple w-44"
              placeholder={
                health.state === "online" && health.llm_configured
                  ? "using server key"
                  : "gsk_..."
              }
              value={apiKey}
              onChange={(e) => onApiKeyChange(e.target.value)}
            />
            <button
              type="button"
              onClick={() => setRevealed((v) => !v)}
              aria-label={revealed ? "Hide API key" : "Show API key"}
              className="text-[10px] text-slate-500 hover:text-slate-800 px-1"
            >
              {revealed ? "hide" : "show"}
            </button>
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
