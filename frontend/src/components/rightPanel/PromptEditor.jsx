import React from "react";

function PromptEditor() {
  return (
    <div className="flex-1 border border-synPurpleSoft rounded-2xl p-3 bg-synPurpleSoft/30 flex flex-col text-[11px]">
      <div className="flex items-center justify-between mb-1">
        <p className="text-xs font-semibold text-slate-900">
          System prompt / role
        </p>
        <span className="text-[10px] text-slate-500">Editable later</span>
      </div>
      <textarea
        className="flex-1 border border-synPurpleSoft rounded-lg px-2 py-2 bg-white/80 text-[11px] text-slate-800 resize-none"
        defaultValue={`You are Synapse Assistant, a RAG-focused chatbot.

• Answer strictly from the provided document context.
• If the answer is not in the context, say you don't know.
• When possible, mention which document sections you used.`}
        disabled
      />
      <p className="mt-1 text-[10px] text-slate-500">
        In later versions, this prompt will directly control how the backend LLM
        behaves.
      </p>
    </div>
  );
}

export default PromptEditor;
