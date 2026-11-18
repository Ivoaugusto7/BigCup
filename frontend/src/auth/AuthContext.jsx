import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { apiFetch, clearSession, getSession, onSessionExpired, saveSession } from '../api/client';

export const AuthContext = createContext(null);

async function buscarUsuario() {
  const info = await apiFetch('/api/auth/manage/info');
  return { email: info.email };
}

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null);
  const [carregando, setCarregando] = useState(true);

  // Ao abrir o app: se há sessão salva, confirma com a API quem é o usuário (renovando o token se preciso)
  useEffect(() => {
    let cancelado = false;

    (async () => {
      if (!getSession()) {
        setCarregando(false);
        return;
      }
      try {
        const u = await buscarUsuario();
        if (!cancelado) setUsuario(u);
      } catch {
        clearSession();
      } finally {
        if (!cancelado) setCarregando(false);
      }
    })();

    return () => {
      cancelado = true;
    };
  }, []);

  // Sessão expirou sem possibilidade de renovar -> volta para a tela de login
  useEffect(() => onSessionExpired(() => setUsuario(null)), []);

  const entrar = useCallback(async (email, senha) => {
    const tokens = await apiFetch('/api/auth/login', {
      method: 'POST',
      body: { email, password: senha },
      auth: false,
    });
    saveSession(tokens);
    setUsuario(await buscarUsuario());
  }, []);

  const cadastrar = useCallback(
    async (email, senha) => {
      await apiFetch('/api/auth/register', {
        method: 'POST',
        body: { email, password: senha },
        auth: false,
      });
      await entrar(email, senha);
    },
    [entrar],
  );

  const sair = useCallback(() => {
    // Em modo Bearer o Identity não tem logout no servidor: basta descartar os tokens
    clearSession();
    setUsuario(null);
  }, []);

  const value = useMemo(
    () => ({ usuario, carregando, entrar, cadastrar, sair }),
    [usuario, carregando, entrar, cadastrar, sair],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth precisa estar dentro de <AuthProvider>.');
  return ctx;
}
