import '@testing-library/jest-dom';

// Node 25+ define um `localStorage` global próprio, que fica indisponível sem a opção --localstorage-file
// e esconde o do jsdom nos testes. Nesse caso, usa um armazenamento em memória equivalente.
function storageDisponivel() {
  try {
    return typeof globalThis.localStorage?.getItem === 'function';
  } catch {
    return false;
  }
}

if (!storageDisponivel()) {
  const dados = new Map();
  const memoria = {
    getItem: (chave) => (dados.has(String(chave)) ? dados.get(String(chave)) : null),
    setItem: (chave, valor) => dados.set(String(chave), String(valor)),
    removeItem: (chave) => dados.delete(String(chave)),
    clear: () => dados.clear(),
    key: (i) => [...dados.keys()][i] ?? null,
    get length() {
      return dados.size;
    },
  };
  Object.defineProperty(globalThis, 'localStorage', { value: memoria, configurable: true, writable: true });
}
