// src/components/middlePanel/ModelInfoPanel.jsx
//
// This component existed but was never rendered, and its content was invented:
// it advertised "llama-3.1-70b (example)" and "llama-3.2-embedding (planned)"
// with hardcoded zero counters. It now reports what the backend actually runs,
// from GET /stats.

import React from "react";
import StatPill from "../shared/StatPill.jsx";

function ModelInfoPanel({ stats }) {
  return (
    <div className="border border-synPurpleSoft rounded-2xl p-3 bg-synPurpleSoft/30 flex flex-col gap-2 shrink-0">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-semibold text-slate-900">Model &amp; Index overview</h2>
        <span className="text-[10px] px-2 py-0.5 rounded-full bg-white text-synPurple border border-synPurpleSoft">
          LLM · Groq
        </span>
      </div>

      <div className="text-[11px] mt-1 space-y-0.5">
        <p className="text-slate-700 truncate" title={stats?.chat_model}>
          Chat model:{" "}
          <span className="font-semibold text-synPurple">{stats?.chat_model ?? "—"}</span>
        </p>
        <p className="text-slate-500 truncate" title={stats?.embed_model}>
          Embeddings: <span className="font-medium">{stats?.embed_model ?? "—"}</span>
        </p>
        <p className="text-slate-500 truncate" title={stats?.cross_encoder_model}>
          Reranker: <span className="font-medium">{stats?.cross_encoder_model ?? "—"}</span>
        </p>
      </div>

      <div className="mt-1 grid grid-cols-3 gap-2 text-[11px]">
        <StatPill label="Uploaded" value={stats?.files_uploaded ?? "—"} />
        <StatPill label="Indexed" value={stats?.files_indexed ?? "—"} />
        <StatPill label="Chunks" value={stats?.total_chunks ?? "—"} />
      </div>
    </div>
  );
}

export default ModelInfoPanel;
