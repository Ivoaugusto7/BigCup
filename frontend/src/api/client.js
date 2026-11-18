// Cliente da API do BigCup: monta as requisições, envia o token (Bearer) e renova a sessão automaticamente.

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

const STORAGE_KEY = 'bigcup:auth';

/** Erro de resposta da API (status HTTP + corpo já convertido de JSON, quando houver). */
export class ApiError extends Error {
  constructor(status, data) {
    super(`HTTP ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.data = data;
  }
}

// ---------- Sessão (tokens no localStorage) ----------

export function getSession() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

/** Salva os tokens retornados por /api/auth/login ou /api/auth/refresh. */
export function saveSession({ accessToken, refreshToken, expiresIn }) {
  const session = { accessToken, refreshToken, expiresAt: Date.now() + (expiresIn ?? 0) * 1000 };
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  } catch {
    // armazenamento indisponível (aba anônima, bloqueado etc.): a sessão vale só até recarregar
  }
  return session;
}

export function clearSession() {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    // ignora
  }
}

const sessionExpiredListeners = new Set();

/** Registra uma função chamada quando a sessão expira e não pode ser renovada. Retorna a função para cancelar. */
export function onSessionExpired(listener) {
  sessionExpiredListeners.add(listener);
  return () => {
    sessionExpiredListeners.delete(listener);
  };
}

function expireSession() {
  clearSession();
  sessionExpiredListeners.forEach((listener) => listener());
}

// ---------- Requisições ----------

async function parseBody(res) {
  const text = await res.text();
  if (!text) return null;
  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

let refreshPromise = null;

/**
 * Troca o refreshToken por tokens novos. Várias chamadas ao mesmo tempo compartilham uma única renovação.
 * Retorna a sessão nova ou null se não foi possível renovar.
 */
export function refreshSession() {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      const session = getSession();
      if (!session?.refreshToken) return null;

      const res = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
        body: JSON.stringify({ refreshToken: session.refreshToken }),
      });
      if (!res.ok) return null;
      return saveSession(await parseBody(res));
    })()
      .catch(() => null)
      .finally(() => {
        refreshPromise = null;
      });
  }
  return refreshPromise;
}

/**
 * Chama a API. Com `auth` (padrão), envia o token e, se receber 401, renova a sessão uma vez e repete a chamada.
 * Retorna o corpo da resposta (JSON) ou lança ApiError.
 */
export async function apiFetch(path, { method = 'GET', body, auth = true, headers = {} } = {}) {
  const send = () => {
    const finalHeaders = { Accept: 'application/json', ...headers };
    if (body !== undefined) finalHeaders['Content-Type'] = 'application/json';

    const session = auth ? getSession() : null;
    if (session?.accessToken) finalHeaders.Authorization = `Bearer ${session.accessToken}`;

    return fetch(`${API_BASE_URL}${path}`, {
      method,
      headers: finalHeaders,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  };

  let res = await send();

  if (res.status === 401 && auth && getSession()?.refreshToken) {
    const renewed = await refreshSession();
    if (renewed) res = await send();
    if (!renewed || res.status === 401) expireSession();
  }

  const data = await parseBody(res);
  if (!res.ok) throw new ApiError(res.status, data);
  return data;
}
