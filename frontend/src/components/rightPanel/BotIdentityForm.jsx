import React from "react";

function BotIdentityForm() {
  return (
    <div className="border border-synPurpleSoft rounded-2xl p-3 bg-white flex flex-col gap-2 text-[11px]">
      <label className="flex flex-col gap-1">
        Bot name
        <input
          className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-synPurpleSoft/40 text-slate-800"
          placeholder="Synapse Assistant"
          disabled
        />
      </label>

      <label className="flex flex-col gap-1">
        Short persona
        <input
          className="border border-synPurpleSoft rounded-lg px-2 py-1 bg-synPurpleSoft/40 text-slate-800"
          placeholder="Helpful RAG expert focusing only on uploaded docs."
          disabled
        />
      </label>
    </div>
  );
}

export default BotIdentityForm;
