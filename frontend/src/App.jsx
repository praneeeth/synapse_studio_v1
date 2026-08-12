// src/App.jsx
import React, { useCallback, useEffect, useState } from "react";
import HeaderBar from "./components/layout/HeaderBar.jsx";
import LeftPanel from "./components/leftPanel/LeftPanel.jsx";
import MiddlePanel from "./components/middlePanel/MiddlePanel.jsx";
import RightPanel from "./components/rightPanel/RightPanel.jsx";
import Toast from "./components/shared/Toast.jsx";
import { useDocuments } from "./hooks/useDocuments.js";
import { useLocalStorage } from "./hooks/useLocalStorage.js";
import { getHealth } from "./api/client.js";
import { DEFAULTS, STORAGE_KEYS } from "./constants.js";

function App() {
  const [toast, setToast] = useState(null);
  const [health, setHealth] = useState({ state: "checking" });

  // The API key stays in component state only -- deliberately not persisted to
  // localStorage, where any script on the page could read it.
  const [apiKey, setApiKey] = useState("");

  const [activeFileId, setActiveFileId] = useState(null);
  const [activeFileName, setActiveFileName] = useState(null);

  const [persona, setPersona] = useLocalStorage(STORAGE_KEYS.persona, DEFAULTS.persona);
  const [systemPrompt, setSystemPrompt] = useLocalStorage(
    STORAGE_KEYS.systemPrompt,
    DEFAULTS.systemPrompt
  );
  const [rules, setRules] = useLocalStorage(STORAGE_KEYS.rules, DEFAULTS.rules);
  const [botName, setBotName] = useLocalStorage(STORAGE_KEYS.botName, DEFAULTS.botName);
  const [welcomeMessage, setWelcomeMessage] = useLocalStorage(
    STORAGE_KEYS.welcomeMessage,
    DEFAULTS.welcomeMessage
  );
  const [topK, setTopK] = useLocalStorage(STORAGE_KEYS.topK, DEFAULTS.topK);
  const [streaming, setStreaming] = useLocalStorage(STORAGE_KEYS.streaming, DEFAULTS.streaming);

  const showError = useCallback((err) => {
    setToast({
      tone: "error",
      title: err?.status ? `Request failed (${err.status})` : "Request failed",
      message: err?.detail || err?.message || String(err),
    });
  }, []);

  const { documents, stats, isLoading, busyIds, refresh, uploadAndIndex, reindex, remove } =
    useDocuments({ onError: showError });

  // Connectivity check -- the old UI gave no signal at all when the backend
  // was down; every action just failed silently.
  useEffect(() => {
    const controller = new AbortController();
    getHealth(controller.signal)
      .then((h) => setHealth({ state: "online", ...h }))
      .catch((err) => {
        if (err.name !== "AbortError") setHealth({ state: "offline" });
      });
    return () => controller.abort();
  }, []);

  const handleActiveFileChange = useCallback((fileId, fileName) => {
    setActiveFileId(fileId);
    setActiveFileName(fileName || null);
  }, []);

  const handleUploadAndIndex = useCallback(
    async (file, options) => {
      const created = await uploadAndIndex(file, options);
      if (created) {
        setToast({
          tone: "success",
          title: "Indexed",
          message: `"${created.file_name}" is ready to query.`,
        });
      }
      return created;
    },
    [uploadAndIndex]
  );

  const handleDelete = useCallback(
    async (fileId) => {
      const ok = await remove(fileId);
      if (ok && fileId === activeFileId) {
        setActiveFileId(null);
        setActiveFileName(null);
      }
      return ok;
    },
    [activeFileId, remove]
  );

  const handleReindex = useCallback(
    async (fileId, options) => {
      const ok = await reindex(fileId, options);
      if (ok) {
        setToast({ tone: "success", title: "Re-indexed", message: `Method: ${options.method}.` });
      }
      return ok;
    },
    [reindex]
  );

  const resetPromptDefaults = useCallback(() => {
    setPersona(DEFAULTS.persona);
    setSystemPrompt(DEFAULTS.systemPrompt);
    setRules(DEFAULTS.rules);
  }, [setPersona, setRules, setSystemPrompt]);

  return (
    <div className="h-screen flex flex-col bg-synBg overflow-hidden">
      <HeaderBar
        apiKey={apiKey}
        onApiKeyChange={setApiKey}
        health={health}
        onRetryHealth={() => {
          setHealth({ state: "checking" });
          getHealth()
            .then((h) => setHealth({ state: "online", ...h }))
            .catch(() => setHealth({ state: "offline" }));
          refresh();
        }}
      />

      <main className="flex-1 min-h-0 flex flex-col">
        <div className="w-full max-w-[1600px] mx-auto px-4 py-3 flex-1 min-h-0 flex flex-col gap-2">
          <p className="text-xs text-slate-500 shrink-0">
            Upload → chunk → index → retrieve → rerank → chat.
            {health.state === "online" && !health.llm_configured && (
              <span className="ml-2 text-amber-700">
                No server-side LLM key: enter a Groq key in the header to chat.
              </span>
            )}
          </p>

          <div className="flex-1 min-h-0 grid grid-cols-1 lg:grid-cols-[minmax(0,38%)_minmax(0,30%)_minmax(0,32%)] gap-3">
            <section className="min-h-0 bg-white/80 rounded-3xl shadow-soft p-3 overflow-hidden">
              <LeftPanel
                documents={documents}
                isLoading={isLoading}
                busyIds={busyIds}
                activeFileId={activeFileId}
                onActiveFileChange={handleActiveFileChange}
                onUploadAndIndex={handleUploadAndIndex}
                onReindex={handleReindex}
                onDelete={handleDelete}
              />
            </section>

            <section className="min-h-0 bg-white/80 rounded-3xl shadow-soft p-3 overflow-hidden">
              <MiddlePanel
                apiKey={apiKey}
                activeFileId={activeFileId}
                activeFileName={activeFileName}
                persona={persona}
                systemPrompt={systemPrompt}
                rules={rules}
                botName={botName}
                onBotNameChange={setBotName}
                welcomeMessage={welcomeMessage}
                onWelcomeMessageChange={setWelcomeMessage}
                topK={topK}
                onTopKChange={setTopK}
                streaming={streaming}
                onStreamingChange={setStreaming}
                stats={stats}
                onError={showError}
              />
            </section>

            <section className="min-h-0 bg-white/80 rounded-3xl shadow-soft p-3 overflow-hidden">
              <RightPanel
                persona={persona}
                systemPrompt={systemPrompt}
                rules={rules}
                onPersonaChange={setPersona}
                onSystemPromptChange={setSystemPrompt}
                onRulesChange={setRules}
                onResetDefaults={resetPromptDefaults}
              />
            </section>
          </div>
        </div>
      </main>

      <Toast toast={toast} onDismiss={() => setToast(null)} />
    </div>
  );
}

export default App;
