import React, { useState, useEffect } from 'react';
import Layout from './components/layout/Layout';
import Sidebar from './components/layout/Sidebar';
import DrinkCard from './components/drinks/DrinkCard';

const STATIC_CONTAINERS = [
  { id: 1, nome: 'Shot', volume: '50 ml', icone: '🥃' },
  { id: 2, nome: 'Copo', volume: '300 ml', icone: '🥛' },
  { id: 3, nome: 'Copo', volume: '500 ml', icone: '🍺' },
  { id: 4, nome: 'Copo', volume: '700 ml', icone: '🥤' },
  { id: 5, nome: 'Garrafa', volume: '2 Litros', icone: '🍾' },
  { id: 6, nome: 'Galão', volume: '5 Litros', icone: '🛢️' },
];

function App() {
  const [selectedContainer, setSelectedContainer] = useState(null);

  const [containers, setContainers] = useState(STATIC_CONTAINERS);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Hydrate from localStorage on mount
  useEffect(() => {
    try {
      const raw = localStorage.getItem('bigcup:selectedContainer');
      if (raw) setSelectedContainer(JSON.parse(raw));
    } catch (e) {
      // ignore parse errors
    }
  }, []);

  // Persist selection to localStorage
  useEffect(() => {
    try {
      if (selectedContainer) {
        localStorage.setItem('bigcup:selectedContainer', JSON.stringify(selectedContainer));
      } else {
        localStorage.removeItem('bigcup:selectedContainer');
      }
    } catch (e) {
      // ignore storage errors
    }
  }, [selectedContainer]);

  function handleToggle(container) {
    if (selectedContainer && selectedContainer.id === container.id) {
      setSelectedContainer(null);
    } else {
      setSelectedContainer(container);
    }
  }

  // Fetch containers from backend API with retry support
  useEffect(() => {
    let cancelled = false;

    async function fetchContainers() {
      setLoading(true);
      setError(null);
      const base = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
      const url = `${base}/api/recipientes`;

      try {
        const res = await fetch(url);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const data = await res.json();

        if (cancelled) return;

        const mapped = data.map((d) => {
          const vol = d.volume;
          let icone = '🥤';
          if (d.nome.toLowerCase().includes('shot') || vol === 50) icone = '🥃';
          else if (vol === 300) icone = '🥛';
          else if (vol === 500) icone = '🍺';
          else if (vol === 700) icone = '🥤';
          else if (d.nome.toLowerCase().includes('garrafa') || (vol >= 2000 && vol < 5000)) icone = '🍾';
          else if (d.nome.toLowerCase().includes('gal') || vol >= 5000) icone = '🛢️';

          const volumeLabel = vol >= 1000 ? `${vol / 1000} Litros` : `${vol} ml`;

          return { id: d.id, nome: d.nome, volume: volumeLabel, icone };
        });

        setContainers(mapped);
      } catch (err) {
        console.warn('Could not fetch recipientes:', err?.message ?? err);
        if (!cancelled) setError(err?.message ?? 'Failed to fetch');
        // keep fallback STATIC_CONTAINERS (already set as initial state)
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    // expose for retry by attaching to window for dev convenience
    fetchContainers();

    return () => {
      cancelled = true;
    };
  }, []);

  // Retry handler used by UI
  function handleRetry() {
    // trigger the effect by calling a fetch anew: call the same logic via a small wrapper
    const base = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
    const url = `${base}/api/recipientes`;

    setLoading(true);
    setError(null);

    fetch(url)
      .then((r) => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.json();
      })
      .then((data) => {
        const mapped = data.map((d) => {
          const vol = d.volume;
          let icone = '🥤';
          if (d.nome.toLowerCase().includes('shot') || vol === 50) icone = '🥃';
          else if (vol === 300) icone = '🥛';
          else if (vol === 500) icone = '🍺';
          else if (vol === 700) icone = '🥤';
          else if (d.nome.toLowerCase().includes('garrafa') || (vol >= 2000 && vol < 5000)) icone = '🍾';
          else if (d.nome.toLowerCase().includes('gal') || vol >= 5000) icone = '🛢️';

          const volumeLabel = vol >= 1000 ? `${vol / 1000} Litros` : `${vol} ml`;

          return { id: d.id, nome: d.nome, volume: volumeLabel, icone };
        });

        setContainers(mapped);
        setError(null);
      })
      .catch((err) => setError(err?.message ?? 'Failed to fetch'))
      .finally(() => setLoading(false));
  }

  return (
    <Layout>
      <div className="w-full grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-1">
          <Sidebar selectedContainer={selectedContainer} onRemove={() => setSelectedContainer(null)} />
        </div>

        <main className="lg:col-span-2">
          <div className="mb-6">
            <p className="mb-3 text-sm font-semibold uppercase tracking-[0.35em] text-primary">Drink Builder</p>
            <h1 className="text-3xl font-semibold text-white sm:text-4xl">Selecione um recipiente</h1>
            {loading && <p className="mt-2 text-sm text-slate-400">Carregando recipientes...</p>}
            {error && (
              <div className="mt-2 flex items-center gap-3">
                <p className="text-sm text-amber-400">Erro ao carregar: {error}</p>
                <button
                  onClick={handleRetry}
                  className="rounded-md bg-amber-600 px-3 py-1 text-sm font-semibold text-white hover:bg-amber-500"
                >
                  Tentar novamente
                </button>
              </div>
            )}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {containers.map((c) => (
              <DrinkCard
                key={c.id}
                container={c}
                isSelected={!!selectedContainer && selectedContainer.id === c.id}
                onSelect={handleToggle}
              />
            ))}
          </div>
        </main>
      </div>
    </Layout>
  );
}

export default App;
