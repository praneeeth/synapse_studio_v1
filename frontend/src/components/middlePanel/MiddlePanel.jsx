// src/components/middlePanel/MiddlePanel.jsx
import React, { useState } from "react";
import ChatPanel from "./ChatPanel.jsx";

function MiddlePanel({
  apiKey,
  activeFileId,
  activeFileName,
  persona,
  systemPrompt,
  rules,
}) {
  const [botName, setBotName] = useState("Synapse Assistant");
  const [welcomeMessage, setWelcomeMessage] = useState(
    "Hi, I’m your RAG agent. Ask me anything based on your indexed documents."
  );

  return (
    <div className="h-full flex flex-col gap-4">
      {/* Bot name + welcome message editor */}
      <div className="border border-synPurpleSoft rounded-2xl p-3 bg-synPurpleSoft/30 flex flex-col gap-2">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-semibold text-slate-900">
            Agent welcome message
          </h2>
          <span className="text-[10px] text-slate-500">Center panel</span>
        </div>

        <label className="flex flex-col gap-1 text-[11px]">
          Bot name
          <input
            className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-white/80 text-slate-800"
            value={botName}
            onChange={(e) => setBotName(e.target.value)}
          />
        </label>

        <label className="flex flex-col gap-1 text-[11px]">
          Welcome message
          <textarea
            className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-white/80 text-slate-800 resize-none"
            rows={2}
            value={welcomeMessage}
            onChange={(e) => setWelcomeMessage(e.target.value)}
          />
        </label>

        <p className="text-[10px] text-slate-500">
          This is the greeting your agent shows in the chatbot below.
        </p>
      </div>

      {/* Chatbot */}
      <ChatPanel
        botName={botName}
        welcomeMessage={welcomeMessage}
        apiKey={apiKey}
        activeFileId={activeFileId}
        activeFileName={activeFileName}
        persona={persona}
        systemPrompt={systemPrompt}
        rules={rules}
      />
    </div>
  );
}

export default MiddlePanel;
