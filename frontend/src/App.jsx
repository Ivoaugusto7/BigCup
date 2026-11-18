import React, { useState, useEffect, useCallback } from 'react';
import Layout from './components/layout/Layout';
import Header from './components/layout/Header';
import Sidebar from './components/layout/Sidebar';
import DrinkCard from './components/drinks/DrinkCard';
import AuthPage from './components/auth/AuthPage';
import { useAuth } from './auth/AuthContext';
import { apiFetch } from './api/client';

const STATIC_CONTAINERS = [
  { id: 1, nome: 'Shot', volume: '50 ml', icone: '🥃' },
  { id: 2, nome: 'Copo pequeno', volume: '300 ml', icone: '🥛' },
  { id: 3, nome: 'Copo médio', volume: '500 ml', icone: '🍺' },
  { id: 4, nome: 'Copo grande', volume: '700 ml', icone: '🥤' },
  { id: 5, nome: 'Garrafa', volume: '2 Litros', icone: '🍾' },
  { id: 6, nome: 'Galão', volume: '5 Litros', icone: '🛢️' },
];

// Converte um RecipienteDto da API ({ id, nome, capacidadeMl }) para o formato usado pelos cards.
export function mapRecipiente(d) {
  const vol = Number(d.capacidadeMl ?? d.volume ?? 0);
  const nome = (d.nome ?? '').toLowerCase();

  let icone = '🥤';
  if (nome.includes('shot') || vol === 50) icone = '🥃';
  else if (vol === 300) icone = '🥛';
  else if (vol === 500) icone = '🍺';
  else if (vol === 700) icone = '🥤';
  else if (nome.includes('garrafa') || (vol >= 2000 && vol < 5000)) icone = '🍾';
  else if (nome.includes('gal') || vol >= 5000) icone = '🛢️';

  const volume = vol >= 1000 ? `${vol / 1000} Litros` : `${vol} ml`;

  return { id: d.id, nome: d.nome, volume, icone };
}

/** Escolha de recipiente do usuário logado (a seleção fica salva por usuário neste navegador). */
function EscolherRecipiente({ email }) {
  const storageKey = `bigcup:selectedContainer:${email}`;
  const [selectedContainer, setSelectedContainer] = useState(() => {
    try {
      const raw = localStorage.getItem(storageKey);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  });

  const [containers, setContainers] = useState(STATIC_CONTAINERS);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Persist selection to localStorage
  useEffect(() => {
    try {
      if (selectedContainer) {
        localStorage.setItem(storageKey, JSON.stringify(selectedContainer));
      } else {
        localStorage.removeItem(storageKey);
      }
    } catch {
      // ignore storage errors
    }
  }, [selectedContainer, storageKey]);

  function handleToggle(container) {
    if (selectedContainer && selectedContainer.id === container.id) {
      setSelectedContainer(null);
    } else {
      setSelectedContainer(container);
    }
  }

  // Busca os recipientes na API; em caso de erro mantém a lista estática (fallback).
  const fetchContainers = useCallback(async (isCancelled = () => false) => {
    setLoading(true);
    setError(null);

    try {
      const data = await apiFetch('/api/recipientes', { auth: false });
      if (!isCancelled()) setContainers(data.map(mapRecipiente));
    } catch (err) {
      console.warn('Could not fetch recipientes:', err?.message ?? err);
      if (!isCancelled()) setError(err?.message ?? 'Failed to fetch');
    } finally {
      if (!isCancelled()) setLoading(false);
    }
  }, []);

  useEffect(() => {
    let cancelled = false;
    fetchContainers(() => cancelled);
    return () => {
      cancelled = true;
    };
  }, [fetchContainers]);

  return (
    <div className="w-full grid grid-cols-1 gap-6 lg:grid-cols-3">
      <div className="lg:col-span-1">
        <Sidebar selectedContainer={selectedContainer} onRemove={() => setSelectedContainer(null)} />
      </div>

      <section className="lg:col-span-2">
        <div className="mb-6">
          <p className="mb-3 text-sm font-semibold uppercase tracking-[0.35em] text-primary">Drink Builder</p>
          <h1 className="text-3xl font-semibold text-white sm:text-4xl">Selecione um recipiente</h1>
          {loading && <p className="mt-2 text-sm text-slate-400">Carregando recipientes...</p>}
          {error && (
            <div className="mt-2 flex items-center gap-3">
              <p className="text-sm text-amber-400">Erro ao carregar: {error}</p>
              <button
                onClick={() => fetchContainers()}
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
      </section>
    </div>
  );
}

function App() {
  const { usuario, carregando, sair } = useAuth();

  if (carregando) {
    return (
      <Layout>
        <p className="text-slate-400">Carregando...</p>
      </Layout>
    );
  }

  if (!usuario) {
    return (
      <Layout>
        <AuthPage />
      </Layout>
    );
  }

  return (
    <Layout header={<Header email={usuario.email} onSair={sair} />}>
      {/* key: troca de usuário recria a tela com a seleção dele */}
      <EscolherRecipiente key={usuario.email} email={usuario.email} />
    </Layout>
  );
}

export default App;
