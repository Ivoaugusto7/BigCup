import React from 'react';

function DrinkCard({ container, isSelected, onSelect }) {
  const borderClass = isSelected ? 'border-primary' : 'border-border';

  return (
    <article
      className={`mt-4 rounded-2xl border ${borderClass} bg-card p-6 shadow-lg shadow-black/20 transition-colors duration-200 ease-out transform hover:scale-[1.02] focus-within:scale-[1.01]`}
      tabIndex={0}
    >
      <div className="flex items-center gap-4">
        <div className="text-4xl transition-transform duration-200">{container.icone}</div>
        <div>
          <h3 className="text-lg font-semibold text-white">{container.nome}</h3>
          <p className="mt-1 text-sm text-slate-400">{container.volume}</p>
        </div>
      </div>

      <div className="mt-4 flex justify-end">
        {isSelected ? (
          <button
            onClick={() => onSelect(container)}
            className="rounded-md bg-danger px-4 py-2 text-sm font-semibold text-white hover:bg-red-600 transition-colors duration-150"
          >
            REMOVER
          </button>
        ) : (
          <button
            onClick={() => onSelect(container)}
            className="rounded-md bg-primary px-4 py-2 text-sm font-semibold text-gray-900 hover:bg-emerald-400 transition-colors duration-150"
          >
            APLICAR
          </button>
        )}
      </div>
    </article>
  );
}

export default DrinkCard;
