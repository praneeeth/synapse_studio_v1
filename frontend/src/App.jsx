// src/App.jsx
import React, { useState } from "react";
import HeaderBar from "./components/layout/HeaderBar.jsx";
import LeftPanel from "./components/leftPanel/LeftPanel.jsx";
import MiddlePanel from "./components/middlePanel/MiddlePanel.jsx";
import RightPanel from "./components/rightPanel/RightPanel.jsx";

function App() {
  // shared state
  const [apiKey, setApiKey] = useState("");
  const [activeFileId, setActiveFileId] = useState(null);
  const [activeFileName, setActiveFileName] = useState(null);

  const [persona, setPersona] = useState("RAG expert (default)");
  const [systemPrompt, setSystemPrompt] = useState(
    "You are Synapse Assistant, a RAG-focused chatbot. Answer only from provided context."
  );
  const [rules, setRules] = useState(
    "1. Never hallucinate.\n2. If the answer is not clearly in the context, say you don't know from these documents."
  );

  const handleApiKeyChange = (value) => {
    setApiKey(value);
  };

  const handleActiveFileChange = (fileId, fileName) => {
    setActiveFileId(fileId);
    setActiveFileName(fileName || null);
  };

  return (
    <div className="min-h-screen flex flex-col bg-synBg">
      <HeaderBar apiKey={apiKey} onApiKeyChange={handleApiKeyChange} />

      <main className="flex-1 flex flex-col">
        <div className="max-w-7xl mx-auto px-4 py-4 h-full">
          <div className="text-xs text-slate-500 mb-3">
            UI v1: Upload → Semantic Chunking → Index → Chat with RAG + rerank + Qwen
          </div>

          {/* three-panel layout */}
          <div className="grid grid-cols-[42%_25%_33%] gap-4 h-[calc(100vh-120px)]">
            <section className="h-full">
              <div className="bg-white/80 rounded-3xl shadow-soft p-4 h-full">
                <LeftPanel
                  onActiveFileChange={handleActiveFileChange}
                  activeFileId={activeFileId}
                />
              </div>
            </section>

            <section className="h-full">
              <div className="bg-white/80 rounded-3xl shadow-soft p-4 h-full">
                <MiddlePanel
                  apiKey={apiKey}
                  activeFileId={activeFileId}
                  activeFileName={activeFileName}
                  persona={persona}
                  systemPrompt={systemPrompt}
                  rules={rules}
                />
              </div>
            </section>

            <section className="h-full">
              <div className="bg-white/80 rounded-3xl shadow-soft p-4 h-full">
                <RightPanel
                  persona={persona}
                  systemPrompt={systemPrompt}
                  rules={rules}
                  onPersonaChange={setPersona}
                  onSystemPromptChange={setSystemPrompt}
                  onRulesChange={setRules}
                />
              </div>
            </section>
          </div>
        </div>
      </main>
    </div>
  );
}

export default App;
