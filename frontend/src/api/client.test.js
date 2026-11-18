import { vi } from 'vitest';
import { apiFetch, ApiError, getSession, onSessionExpired, saveSession } from './client';

// Resposta mínima no formato que o client usa (ok, status, text)
function resposta(status, body) {
  return {
    ok: status >= 200 && status < 300,
    status,
    text: async () => (body === undefined ? '' : JSON.stringify(body)),
  };
}

describe('apiFetch', () => {
  let fetchMock;

  beforeEach(() => {
    localStorage.clear();
    fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  test('envia o token Bearer da sessão', async () => {
    saveSession({ accessToken: 'tok-1', refreshToken: 'ref-1', expiresIn: 3600 });
    fetchMock.mockResolvedValueOnce(resposta(200, [{ id: 1 }]));

    const data = await apiFetch('/api/drinks');

    expect(data).toEqual([{ id: 1 }]);
    expect(fetchMock.mock.calls[0][1].headers.Authorization).toBe('Bearer tok-1');
  });

  test('com auth: false não envia token', async () => {
    saveSession({ accessToken: 'tok-1', refreshToken: 'ref-1', expiresIn: 3600 });
    fetchMock.mockResolvedValueOnce(resposta(200, []));

    await apiFetch('/api/recipientes', { auth: false });

    expect(fetchMock.mock.calls[0][1].headers.Authorization).toBeUndefined();
  });

  test('em 401 renova a sessão, repete a chamada e salva os tokens novos', async () => {
    saveSession({ accessToken: 'velho', refreshToken: 'ref-1', expiresIn: 3600 });
    fetchMock
      .mockResolvedValueOnce(resposta(401))
      .mockResolvedValueOnce(resposta(200, { accessToken: 'novo', refreshToken: 'ref-2', expiresIn: 3600 }))
      .mockResolvedValueOnce(resposta(200, { email: 'a@b.com' }));

    const data = await apiFetch('/api/auth/manage/info');

    expect(data).toEqual({ email: 'a@b.com' });
    expect(fetchMock.mock.calls[1][0]).toMatch(/\/api\/auth\/refresh$/);
    expect(JSON.parse(fetchMock.mock.calls[1][1].body)).toEqual({ refreshToken: 'ref-1' });
    expect(fetchMock.mock.calls[2][1].headers.Authorization).toBe('Bearer novo');
    expect(getSession()).toMatchObject({ accessToken: 'novo', refreshToken: 'ref-2' });
  });

  test('se a renovação falhar, limpa a sessão, avisa os ouvintes e lança 401', async () => {
    saveSession({ accessToken: 'velho', refreshToken: 'ref-1', expiresIn: 3600 });
    const ouvinte = vi.fn();
    const cancelar = onSessionExpired(ouvinte);
    fetchMock.mockResolvedValueOnce(resposta(401)).mockResolvedValueOnce(resposta(401));

    await expect(apiFetch('/api/drinks')).rejects.toMatchObject({ status: 401 });

    expect(getSession()).toBeNull();
    expect(ouvinte).toHaveBeenCalledTimes(1);
    cancelar();
  });

  test('duas chamadas simultâneas com 401 fazem uma única renovação', async () => {
    saveSession({ accessToken: 'velho', refreshToken: 'ref-1', expiresIn: 3600 });
    fetchMock.mockImplementation(async (url, options) => {
      if (url.endsWith('/api/auth/refresh')) {
        return resposta(200, { accessToken: 'novo', refreshToken: 'ref-2', expiresIn: 3600 });
      }
      return options.headers.Authorization === 'Bearer novo' ? resposta(200, { ok: true }) : resposta(401);
    });

    await Promise.all([apiFetch('/api/drinks'), apiFetch('/api/avaliacoes')]);

    const refreshes = fetchMock.mock.calls.filter(([url]) => url.endsWith('/api/auth/refresh'));
    expect(refreshes).toHaveLength(1);
  });

  test('erro da API vira ApiError com o corpo da resposta', async () => {
    fetchMock.mockResolvedValueOnce(resposta(400, { errors: { PasswordTooShort: ['...'] } }));

    const erro = await apiFetch('/api/auth/register', { method: 'POST', body: {}, auth: false }).catch((e) => e);

    expect(erro).toBeInstanceOf(ApiError);
    expect(erro.status).toBe(400);
    expect(erro.data.errors.PasswordTooShort).toBeDefined();
  });
});
