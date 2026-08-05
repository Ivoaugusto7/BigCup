function Sidebar({ selectedContainer, onRemove }) {
  return (
    <aside className="w-full max-w-sm rounded-2xl border-r border-border bg-card p-6 transition-colors duration-200">
      <h2 className="text-sm font-semibold uppercase tracking-[0.35em] text-slate-300">Drink Atual</h2>

      <div className="mt-4">
        {selectedContainer ? (
          <div className="rounded-xl border border-primary bg-background p-4 ring-1 ring-primary/10 transition-shadow duration-200">
            <div className="flex items-center justify-between gap-3">
              <div className="flex items-center gap-3">
                <div className="text-3xl">{selectedContainer.icone}</div>
                <div>
                  <div className="text-lg font-semibold text-white">{selectedContainer.nome}</div>
                  <div className="text-sm text-slate-400">{selectedContainer.volume}</div>
                </div>
              </div>

              <div>
                <button
                  onClick={() => onRemove && onRemove()}
                  className="rounded-md bg-danger px-3 py-1 text-sm font-semibold text-white hover:bg-red-600 transition-colors duration-150"
                >
                  REMOVER
                </button>
              </div>
            </div>

            <p className="mt-3 text-sm text-slate-300">Recipiente selecionado atualmente.</p>
          </div>
        ) : (
          <div className="mt-4 rounded-xl border border-border bg-background p-6 text-center text-slate-300">
            Nenhum selecionado
          </div>
        )}
      </div>
    </aside>
  );
}

export default Sidebar;
