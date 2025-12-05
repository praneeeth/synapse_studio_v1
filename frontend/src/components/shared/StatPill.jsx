import React from "react";

function StatPill({ label, value }) {
  return (
    <div className="rounded-xl bg-white/80 border border-synPurpleSoft px-2 py-2 flex flex-col">
      <span className="text-[10px] text-slate-500">{label}</span>
      <span className="text-sm font-semibold text-slate-900">{value}</span>
    </div>
  );
}

export default StatPill;
