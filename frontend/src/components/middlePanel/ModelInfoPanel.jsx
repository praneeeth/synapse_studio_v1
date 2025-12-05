import React from "react";
import StatPill from "../shared/StatPill.jsx";

function ModelInfoPanel() {
  return (
    <div className="border border-synPurpleSoft rounded-2xl p-3 bg-synPurpleSoft/30 flex flex-col gap-2">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-semibold text-slate-900">
          Model & Index overview
        </h2>
        <span className="text-[10px] px-2 py-0.5 rounded-full bg-white text-synPurple border border-synPurpleSoft">
          LLM · Groq
        </span>
      </div>

      <div className="text-xs mt-1">
        <p className="text-slate-700">
          Active chat model:{" "}
          <span className="font-semibold text-synPurple">
            llama-3.1-70b (example)
          </span>
        </p>
        <p className="text-slate-500 mt-1">
          Embedding model:{" "}
          <span className="font-medium">llama-3.2-embedding (planned)</span>
        </p>
      </div>

      <div className="mt-3 grid grid-cols-3 gap-2 text-[11px]">
        <StatPill label="Files uploaded" value="0" />
        <StatPill label="Files indexed" value="0" />
        <StatPill label="Total chunks" value="0" />
      </div>
    </div>
  );
}

export default ModelInfoPanel;
