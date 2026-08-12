// src/components/middlePanel/ChatPanel.jsx
//
// Fixes vs. the original:
// - message ids came from `messages.length`, which collides whenever two
//   messages are appended in one turn and makes React reuse the wrong nodes
// - the welcome-message effect wiped the whole conversation on every keystroke
//   in the "Welcome message" box
// - answers rendered as plain text, so all model markdown showed as literal **
// - the view never scrolled to new messages
// - each turn was stateless; follow-up questions had no conversation history
// - failures were only console.error'd

import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { chatRag, chatRagStream } from "../../api/client";

let messageCounter = 0;
const nextId = () => `msg-${++messageCounter}`;

function ChatPanel({
  botName,
  welcomeMessage,
  apiKey,
  activeFileId,
  activeFileName,
  persona,
  systemPrompt,
  rules,
  topK,
  streaming,
  onError,
}) {
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState("");
  const [isSending, setIsSending] = useState(false);

  const scrollRef = useRef(null);
  const abortRef = useRef(null);

  // Auto-scroll to the newest content, including during token streaming.
  useEffect(() => {
    const el = scrollRef.current;
    if (el) el.scrollTop = el.scrollHeight;
  }, [messages]);

  // Cancel any in-flight request when the panel unmounts.
  useEffect(() => () => abortRef.current?.abort(), []);

  const resetConversation = useCallback(() => {
    abortRef.current?.abort();
    setMessages([]);
    setIsSending(false);
  }, []);

  const appendMessage = (msg) => setMessages((prev) => [...prev, { id: nextId(), ...msg }]);

  const handleSend = async () => {
    const trimmed = input.trim();
    if (!trimmed || isSending) return;

    if (!activeFileId) {
      appendMessage({ role: "user", text: trimmed });
      appendMessage({
        role: "assistant",
        text: "Please upload and index a file first — then I can answer using its context.",
      });
      setInput("");
      return;
    }

    // Snapshot the prior turns before this question, for multi-turn context.
    const history = messages
      .filter((m) => m.text?.trim() && !m.isError)
      .map((m) => ({ role: m.role, content: m.text }));

    appendMessage({ role: "user", text: trimmed });
    setInput("");
    setIsSending(true);

    const controller = new AbortController();
    abortRef.current = controller;

    const payload = {
      query: trimmed,
      fileId: activeFileId,
      topK,
      persona,
      systemPrompt,
      rules,
      history,
      apiKey: apiKey || undefined,
    };

    try {
      if (streaming) {
        const assistantId = nextId();
        setMessages((prev) => [
          ...prev,
          { id: assistantId, role: "assistant", text: "", context: [], isStreaming: true },
        ]);

        const patch = (updater) =>
          setMessages((prev) =>
            prev.map((m) => (m.id === assistantId ? { ...m, ...updater(m) } : m))
          );

        await chatRagStream(payload, {
          signal: controller.signal,
          onContext: (context) => patch(() => ({ context })),
          onToken: (token) => patch((m) => ({ text: m.text + token })),
          onDone: (info) => patch(() => ({ isStreaming: false, tookMs: info?.took_ms })),
        });

        patch((m) => ({
          isStreaming: false,
          text: m.text || "The model returned an empty answer. Try rephrasing your question.",
        }));
      } else {
        const data = await chatRag(payload, controller.signal);
        appendMessage({
          role: "assistant",
          text: data.answer,
          context: data.context || [],
          model: data.model,
          tookMs: data.took_ms,
        });
      }
    } catch (err) {
      if (err.name === "AbortError") return;
      onError?.(err);
      setMessages((prev) => {
        // Drop the empty streaming placeholder before appending the error.
        const cleaned = prev.filter((m) => !(m.isStreaming && !m.text));
        return [
          ...cleaned,
          {
            id: nextId(),
            role: "assistant",
            isError: true,
            text: err.detail || err.message || "Request failed.",
          },
        ];
      });
    } finally {
      setIsSending(false);
      abortRef.current = null;
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const visibleMessages = useMemo(
    () =>
      messages.length
        ? messages
        : [{ id: "welcome", role: "assistant", text: welcomeMessage, isWelcome: true }],
    [messages, welcomeMessage]
  );

  return (
    <div className="flex-1 min-h-0 border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col">
      <div className="flex items-center justify-between mb-2 gap-2 shrink-0">
        <h3 className="text-sm font-semibold text-slate-900 truncate">{botName} · Test chat</h3>
        <div className="flex items-center gap-2 shrink-0">
          <span className="text-[10px] text-slate-400 truncate max-w-[110px]">
            {activeFileName ? `Using: ${activeFileName}` : "No active file"}
          </span>
          {messages.length > 0 && (
            <button
              onClick={resetConversation}
              className="text-[10px] px-2 py-0.5 rounded-full border border-synPurpleSoft text-slate-600 hover:bg-synPurpleSoft/50"
            >
              Clear
            </button>
          )}
        </div>
      </div>

      <div
        ref={scrollRef}
        className="flex-1 min-h-0 rounded-xl bg-synPurpleSoft/40 border border-synPurpleSoft p-2 flex flex-col gap-2 overflow-y-auto"
      >
        {visibleMessages.map((m) => (
          <MessageBubble key={m.id} message={m} />
        ))}
        {isSending && !streaming && (
          <div className="text-[10px] text-slate-500 italic px-1">
            Retrieving context and generating…
          </div>
        )}
      </div>

      <div className="mt-2 flex gap-2 text-[11px] shrink-0">
        <textarea
          className="flex-1 border border-synPurpleSoft rounded-2xl px-3 py-1.5 bg-synPurpleSoft/40 text-slate-700 resize-none focus:outline-none focus:ring-1 focus:ring-synPurple"
          rows={1}
          placeholder={
            activeFileId
              ? "Ask a question about your indexed file…  (Enter to send, Shift+Enter for a new line)"
              : "Upload & index a file first…"
          }
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={isSending}
        />
        {isSending ? (
          <button
            className="px-3 py-1 rounded-full font-medium text-white bg-rose-500 hover:bg-rose-600"
            onClick={() => abortRef.current?.abort()}
          >
            Stop
          </button>
        ) : (
          <button
            className={`px-3 py-1 rounded-full font-medium text-white ${
              input.trim() ? "bg-synPurple hover:bg-synPurple/90" : "bg-slate-400 cursor-not-allowed"
            }`}
            onClick={handleSend}
            disabled={!input.trim()}
          >
            Send
          </button>
        )}
      </div>
    </div>
  );
}

function MessageBubble({ message }) {
  const isUser = message.role === "user";
  const hasContext = Boolean(message.context?.length);

  const bubbleTone = isUser
    ? "bg-synPurple text-white"
    : message.isError
      ? "bg-rose-50 text-rose-800 border border-rose-200"
      : "bg-white/90 text-slate-800";

  return (
    <div className={`flex flex-col ${isUser ? "items-end" : "items-start"} text-[11px] gap-1`}>
      <div className={`max-w-[85%] rounded-lg px-2 py-1 shadow-sm ${bubbleTone}`}>
        {isUser ? (
          <span className="whitespace-pre-wrap break-words">{message.text}</span>
        ) : (
          <div className="prose-chat break-words">
            <ReactMarkdown remarkPlugins={[remarkGfm]}>{message.text || ""}</ReactMarkdown>
            {message.isStreaming && (
              <span className="inline-block w-1.5 h-3 ml-0.5 bg-synPurple animate-pulse align-middle" />
            )}
          </div>
        )}
      </div>

      {!isUser && (message.model || message.tookMs != null) && (
        <span className="text-[9px] text-slate-400 px-1">
          {message.model}
          {message.tookMs != null && ` · ${(message.tookMs / 1000).toFixed(1)}s`}
        </span>
      )}

      {!isUser && hasContext && (
        <details className="max-w-[85%] w-full text-[10px] bg-synPurpleSoft/60 border border-synPurpleSoft rounded-md px-2 py-1">
          <summary className="cursor-pointer text-slate-700 select-none">
            View retrieved context ({message.context.length} chunks)
          </summary>
          <div className="mt-1 flex flex-col gap-1 max-h-48 overflow-y-auto">
            {message.context.map((c) => (
              <div
                key={c.chunk_id}
                className="border border-synPurpleSoft rounded p-1 bg-white/80"
              >
                <div className="text-[9px] text-slate-500 mb-0.5">
                  chunk {c.chunk_index} · {c.file_name} · score {c.score.toFixed(3)}
                </div>
                <div className="text-[10px] text-slate-700 whitespace-pre-wrap break-words">
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
