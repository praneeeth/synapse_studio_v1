// src/components/middlePanel/MiddlePanel.jsx
import React from "react";
import ChatPanel from "./ChatPanel.jsx";
import ModelInfoPanel from "./ModelInfoPanel.jsx";

function MiddlePanel({
  apiKey,
  activeFileId,
  activeFileName,
  persona,
  systemPrompt,
  rules,
  botName,
  onBotNameChange,
  welcomeMessage,
  onWelcomeMessageChange,
  topK,
  onTopKChange,
  streaming,
  onStreamingChange,
  stats,
  onError,
}) {
  return (
    <div className="h-full flex flex-col gap-3 min-h-0">
      <ModelInfoPanel stats={stats} />

      <details className="border border-synPurpleSoft rounded-2xl bg-synPurpleSoft/30 shrink-0">
        <summary className="cursor-pointer select-none px-3 py-2 text-sm font-semibold text-slate-900">
          Agent identity &amp; retrieval settings
        </summary>

        <div className="px-3 pb-3 flex flex-col gap-2">
          <label className="flex flex-col gap-1 text-[11px]">
            Bot name
            <input
              className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-white/80 text-slate-800"
              value={botName}
              onChange={(e) => onBotNameChange(e.target.value)}
            />
          </label>

          <label className="flex flex-col gap-1 text-[11px]">
            Welcome message
            <textarea
              className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-white/80 text-slate-800 resize-none"
              rows={2}
              value={welcomeMessage}
              onChange={(e) => onWelcomeMessageChange(e.target.value)}
            />
          </label>

          <div className="grid grid-cols-2 gap-2 text-[11px]">
            <label className="flex flex-col gap-1">
              <span>Chunks retrieved (top-k)</span>
              <input
                type="number"
                min={1}
                max={20}
                value={topK}
                onChange={(e) => onTopKChange(Number(e.target.value))}
                className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-white/80 text-slate-800"
              />
            </label>
            <label className="flex items-center gap-2 mt-5">
              <input
                type="checkbox"
                checked={streaming}
                onChange={(e) => onStreamingChange(e.target.checked)}
                className="accent-synPurple"
              />
              <span>Stream tokens</span>
            </label>
          </div>
        </div>
      </details>

      <ChatPanel
        botName={botName}
        welcomeMessage={welcomeMessage}
        apiKey={apiKey}
        activeFileId={activeFileId}
        activeFileName={activeFileName}
        persona={persona}
        systemPrompt={systemPrompt}
        rules={rules}
        topK={topK}
        streaming={streaming}
        onError={onError}
      />
    </div>
  );
}

export default MiddlePanel;
