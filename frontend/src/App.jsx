import Layout from './components/layout/Layout';

function App() {
  return (
    <Layout>
      <div className="rounded-2xl border border-border bg-card p-8 shadow-2xl shadow-black/20">
        <p className="mb-3 text-sm font-semibold uppercase tracking-[0.35em] text-primary">
          Drink Builder
        </p>
        <h1 className="text-3xl font-semibold text-white sm:text-4xl">
          Drink Builder - Fase 2 Concluída
        </h1>
        <p className="mt-4 max-w-2xl text-base text-slate-300">
          Layout base com tema dark mode e Tailwind configurado para o próximo passo do projeto.
        </p>
      </div>
    </Layout>
  );
}

export default App;
