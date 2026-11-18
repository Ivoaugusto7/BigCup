import React, { useState } from 'react';
import { useAuth } from '../../auth/AuthContext';
import { mensagensDeErro } from '../../auth/mensagens';

const inputClass =
  'mt-1 w-full rounded-lg border border-border bg-background px-3 py-2 text-white placeholder-slate-500 ' +
  'focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary';

function AuthPage() {
  const { entrar, cadastrar } = useAuth();
  const [modo, setModo] = useState('entrar'); // 'entrar' | 'cadastrar'
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [confirmacao, setConfirmacao] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [erros, setErros] = useState([]);

  const cadastro = modo === 'cadastrar';

  function trocarModo(novo) {
    setModo(novo);
    setErros([]);
    setConfirmacao('');
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setErros([]);

    if (cadastro && senha !== confirmacao) {
      setErros(['As senhas não conferem.']);
      return;
    }

    setEnviando(true);
    try {
      if (cadastro) await cadastrar(email.trim(), senha);
      else await entrar(email.trim(), senha);
    } catch (err) {
      setErros(mensagensDeErro(err, cadastro ? 'cadastro' : 'login'));
    } finally {
      setEnviando(false);
    }
  }

  const abaClass = (ativa) =>
    `flex-1 rounded-md px-4 py-2 text-sm font-semibold transition-colors duration-150 ${
      ativa ? 'bg-primary text-gray-900' : 'text-slate-300 hover:text-white'
    }`;

  return (
    <section className="w-full max-w-md rounded-2xl border border-border bg-card p-8 shadow-lg shadow-black/20">
      <p className="mb-2 text-sm font-semibold uppercase tracking-[0.35em] text-primary">Drink Builder</p>
      <h1 className="text-3xl font-semibold text-white">{cadastro ? 'Crie sua conta' : 'Bem-vindo de volta'}</h1>
      <p className="mt-2 text-sm text-slate-400">
        {cadastro
          ? 'Monte seus drinks e guarde suas avaliações. Só você vê o que criar.'
          : 'Entre para ver seus drinks e avaliações.'}
      </p>

      <div role="tablist" className="mt-6 flex gap-2 rounded-lg border border-border bg-background p-1">
        <button type="button" role="tab" aria-selected={!cadastro} className={abaClass(!cadastro)} onClick={() => trocarModo('entrar')}>
          Entrar
        </button>
        <button type="button" role="tab" aria-selected={cadastro} className={abaClass(cadastro)} onClick={() => trocarModo('cadastrar')}>
          Criar conta
        </button>
      </div>

      <form onSubmit={handleSubmit} className="mt-6 space-y-4" noValidate>
        <div>
          <label htmlFor="auth-email" className="text-sm font-medium text-slate-300">E-mail</label>
          <input
            id="auth-email"
            type="email"
            autoComplete="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className={inputClass}
            placeholder="voce@exemplo.com"
          />
        </div>

        <div>
          <label htmlFor="auth-senha" className="text-sm font-medium text-slate-300">Senha</label>
          <input
            id="auth-senha"
            type="password"
            autoComplete={cadastro ? 'new-password' : 'current-password'}
            required
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            className={inputClass}
          />
          {cadastro && (
            <p className="mt-1 text-xs text-slate-500">
              Mínimo de 8 caracteres, com letra maiúscula, minúscula, número e símbolo.
            </p>
          )}
        </div>

        {cadastro && (
          <div>
            <label htmlFor="auth-confirmacao" className="text-sm font-medium text-slate-300">Confirmar senha</label>
            <input
              id="auth-confirmacao"
              type="password"
              autoComplete="new-password"
              required
              value={confirmacao}
              onChange={(e) => setConfirmacao(e.target.value)}
              className={inputClass}
            />
          </div>
        )}

        {erros.length > 0 && (
          <ul role="alert" className="space-y-1 rounded-lg border border-danger/40 bg-danger/10 p-3 text-sm text-red-300">
            {erros.map((m) => (
              <li key={m}>{m}</li>
            ))}
          </ul>
        )}

        <button
          type="submit"
          disabled={enviando || !email || !senha}
          className="w-full rounded-md bg-primary px-4 py-2 font-semibold text-gray-900 transition-colors duration-150 hover:bg-emerald-400 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {enviando ? 'Aguarde...' : cadastro ? 'Criar conta' : 'Entrar'}
        </button>
      </form>
    </section>
  );
}

export default AuthPage;
