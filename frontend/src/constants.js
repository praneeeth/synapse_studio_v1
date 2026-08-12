// src/constants.js
// Shared defaults, kept out of components so nothing has to import a sibling
// component just to read a constant.

export const PERSONAS = [
  {
    value: "RAG expert (default)",
    hint: "Balanced, cites chunks, refuses to answer beyond the context.",
  },
  { value: "Patient teacher", hint: "Explains step by step and defines jargon." },
  { value: "Concise analyst", hint: "Short, factual answers with minimal preamble." },
  { value: "Friendly helper", hint: "Warm, conversational tone." },
];

export const DEFAULTS = {
  persona: PERSONAS[0].value,
  systemPrompt:
    "You are Synapse Assistant, a RAG-focused chatbot. Answer only from the provided context.",
  rules:
    "1. Never hallucinate.\n2. If the answer is not clearly in the context, say you don't know from these documents.",
  botName: "Synapse Assistant",
  welcomeMessage:
    "Hi, I'm your RAG agent. Ask me anything based on your indexed documents.",
  topK: 4,
  streaming: true,
  chunkMethod: "semantic",
};

export const STORAGE_KEYS = {
  persona: "synapse.persona",
  systemPrompt: "synapse.systemPrompt",
  rules: "synapse.rules",
  botName: "synapse.botName",
  welcomeMessage: "synapse.welcomeMessage",
  topK: "synapse.topK",
  streaming: "synapse.streaming",
};
