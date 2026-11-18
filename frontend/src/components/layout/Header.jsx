import React from 'react';

function Header({ email, onSair }) {
  return (
    <header className="border-b border-border bg-card/60">
      <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-6 py-4 sm:px-8 lg:px-10">
        <span className="text-lg font-semibold text-white">
          Big<span className="text-primary">Cup</span>
        </span>
        <div className="flex min-w-0 items-center gap-4">
          <span className="truncate text-sm text-slate-300" title={email}>
            {email}
          </span>
          <button
            type="button"
            onClick={onSair}
            className="shrink-0 rounded-md border border-border px-3 py-1 text-sm font-semibold text-slate-200 transition-colors duration-150 hover:border-danger hover:text-white"
          >
            Sair
          </button>
        </div>
      </div>
    </header>
  );
}

export default Header;
