// src/components/middlePanel/ChatPanel.jsx
import React, { useState } from "react";
import { API_BASE_URL } from "../../api/client";

function ChatPanel({
  botName,
  welcomeMessage,
  apiKey,
  activeFileId,
  activeFileName,
  persona,
  systemPrompt,
  rules,
}) {
  const [messages, setMessages] = useState([
    { id: 1, role: "assistant", text: welcomeMessage, context: null },
  ]);
  const [input, setInput] = useState("");
  const [isSending, setIsSending] = useState(false);

  // keep welcome message in sync when it changes
  React.useEffect(() => {
    setMessages([
      { id: 1, role: "assistant", text: welcomeMessage, context: null },
    ]);
  }, [welcomeMessage]);

  const handleSend = async () => {
    const trimmed = input.trim();
    if (!trimmed || isSending) return;

    if (!activeFileId) {
      setMessages((prev) => [
        ...prev,
        { id: prev.length + 1, role: "user", text: trimmed },
        {
          id: prev.length + 2,
          role: "assistant",
          text: "Please upload and index a file first. Then I can answer using its context.",
          context: null,
        },
      ]);
      setInput("");
      return;
    }

    setIsSending(true);

    const newUserMsg = {
      id: messages.length + 1,
      role: "user",
      text: trimmed,
      context: null,
    };
    setMessages((prev) => [...prev, newUserMsg]);
    setInput("");

    try {
      const res = await fetch(`${API_BASE_URL}/chat_rag`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          query: trimmed,
          file_id: activeFileId,
          top_k: 4,
          persona,
          system_prompt: systemPrompt,
          rules,
          debug: true,
          llm_api_key: apiKey || undefined,
        }),
      });

      if (!res.ok) {
        const text = await res.text();
        console.error("chat_rag failed:", text);
        setMessages((prev) => [
          ...prev,
          {
            id: prev.length + 1,
            role: "assistant",
            text:
              "Backend error while calling /chat_rag. Check console / logs for details.",
            context: null,
          },
        ]);
        setIsSending(false);
        return;
      }

      const data = await res.json();
      const assistantMsg = {
        id: messages.length + 2,
        role: "assistant",
        text: data.answer,
        context: data.context || [],
      };

      setMessages((prev) => [...prev, assistantMsg]);
    } catch (err) {
      console.error("chat_rag error:", err);
      setMessages((prev) => [
        ...prev,
        {
          id: prev.length + 1,
          role: "assistant",
          text: "Network or server error while calling /chat_rag.",
          context: null,
        },
      ]);
    } finally {
      setIsSending(false);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="flex-1 border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col">
      <div className="flex items-center justify-between mb-2">
        <h3 className="text-sm font-semibold text-slate-900">
          {botName} · Test chat
        </h3>
        <span className="text-[10px] text-slate-400">
          {activeFileName
            ? `Using: ${activeFileName}`
            : "No active file selected"}
        </span>
      </div>

      {/* messages */}
      <div className="flex-1 rounded-xl bg-synPurpleSoft/40 border border-synPurpleSoft p-2 flex flex-col gap-2 overflow-y-auto">
        {messages.map((m) => (
          <MessageBubble key={m.id} message={m} />
        ))}
      </div>

      {/* input */}
      <div className="mt-2 flex gap-2 text-[11px]">
        <textarea
          className="flex-1 border border-synPurpleSoft rounded-full px-3 py-1 bg-synPurpleSoft/40 text-slate-700 resize-none"
          rows={1}
          placeholder={
            activeFileId
              ? "Ask a question based on your indexed file..."
              : "Upload & index a file first..."
          }
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={isSending}
        />
        <button
          className={`px-3 py-1 rounded-full font-medium text-white ${
            input.trim() && !isSending
              ? "bg-synPurple hover:bg-synPurple/90"
              : "bg-slate-400 cursor-not-allowed"
          }`}
          onClick={handleSend}
          disabled={!input.trim() || isSending}
        >
          {isSending ? "Sending..." : "Send"}
        </button>
      </div>
    </div>
  );
}

function MessageBubble({ message }) {
  const isUser = message.role === "user";
  const hasContext = !!(message.context && message.context.length);

  return (
    <div
      className={`flex flex-col ${
        isUser ? "items-end" : "items-start"
      } text-[11px] gap-1`}
    >
      <div
        className={`max-w-[80%] rounded-lg px-2 py-1 shadow-sm ${
          isUser
            ? "bg-synPurple text-white self-end"
            : "bg-white/90 text-slate-800 self-start"
        }`}
      >
        {message.text}
      </div>

      {!isUser && hasContext && (
        <details className="max-w-[80%] text-[10px] bg-synPurpleSoft/60 border border-synPurpleSoft rounded-md px-2 py-1">
          <summary className="cursor-pointer text-slate-700">
            View context chunks ({message.context.length})
          </summary>
          <div className="mt-1 flex flex-col gap-1 max-h-40 overflow-y-auto">
            {message.context.map((c, idx) => (
              <div
                key={idx}
                className="border border-synPurpleSoft rounded p-1 bg-white/80"
              >
                <div className="text-[9px] text-slate-500 mb-0.5">
                  chunk {c.chunk_index} · score {c.score.toFixed(3)}
                </div>
                <div className="text-[10px] text-slate-700 whitespace-pre-wrap">
                  {c.text}
                </div>
              </div>
            ))}
          </div>
        </details>
      )}
    </div>
  );
}

export default ChatPanel;
