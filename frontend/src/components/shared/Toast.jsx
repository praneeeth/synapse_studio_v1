// src/components/shared/Toast.jsx
//
// The original app reported every failure with console.error, so users saw
// nothing at all when an upload or chat call failed.

import React, { useEffect } from "react";

export function Toast({ toast, onDismiss }) {
  useEffect(() => {
    if (!toast) return undefined;
    const timer = setTimeout(onDismiss, toast.tone === "error" ? 8000 : 4000);
    return () => clearTimeout(timer);
  }, [toast, onDismiss]);

  if (!toast) return null;

  const tone =
    toast.tone === "error"
      ? "bg-rose-50 border-rose-300 text-rose-800"
      : toast.tone === "success"
        ? "bg-emerald-50 border-emerald-300 text-emerald-800"
        : "bg-white border-synPurpleSoft text-slate-800";

  return (
    <div
      role="status"
      aria-live="polite"
      className={`fixed bottom-4 right-4 z-50 max-w-sm rounded-2xl border px-4 py-3 shadow-soft text-xs ${tone}`}
    >
      <div className="flex items-start gap-3">
        <div className="flex-1">
          <p className="font-semibold">{toast.title}</p>
          {toast.message && <p className="mt-0.5 opacity-90 break-words">{toast.message}</p>}
        </div>
        <button
          onClick={onDismiss}
          aria-label="Dismiss notification"
          className="text-base leading-none opacity-60 hover:opacity-100"
        >
          ×
        </button>
      </div>
    </div>
  );
}

export default Toast;
