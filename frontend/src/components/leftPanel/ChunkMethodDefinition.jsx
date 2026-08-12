// src/components/leftPanel/ChunkMethodDefinition.jsx
import React from "react";

function ChunkMethodDefinition({ method }) {
  let title = "";
  let description = "";
  let usage = "";

  if (method === "fixed") {
    title = "Fixed-size chunks (baseline)";
    description =
      "Splits text into equal character windows with a small overlap.";
    usage =
      "Use for simple, well-formatted documents to establish a baseline before trying smarter methods.";
  } else if (method === "recursive") {
    title = "Recursive character splitter";
    description =
      "Tries to split on larger boundaries first (paragraphs, sentences) then falls back to characters.";
    usage =
      "Use when your data is messy or mixed (Markdown, HTML, PDFs with headers) and you want chunks to respect structure.";
  } else if (method === "paragraph") {
    title = "Paragraph-based chunks";
    description =
      "Uses paragraphs or line breaks as natural boundaries. May still cap long paragraphs by size.";
    usage =
      "Use for clean docs, reports, blog posts, or knowledge base articles where paragraphs are meaningful units.";
  } else if (method === "semantic") {
    title = "Semantic chunks";
    description =
      "Groups sentences or paragraphs by meaning instead of just length.";
    usage =
      "Use for high-quality retrieval when accuracy matters most (complex reasoning, FAQs, policy docs). Usually more expensive.";
  }

  return (
    <div className="mt-1 text-[11px] bg-synPurpleSoft/60 border border-synPurpleSoft rounded-xl p-2">
      <p className="font-semibold text-slate-800 mb-1">{title}</p>
      <p className="text-slate-700 mb-1">{description}</p>
      <p className="text-slate-600">
        <span className="font-semibold">When to use: </span>
        {usage}
      </p>
    </div>
  );
}

export default ChunkMethodDefinition;
