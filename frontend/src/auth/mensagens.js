import { ApiError } from '../api/client';

// Códigos de erro do ASP.NET Core Identity -> mensagens em português
const MENSAGENS = {
  PasswordTooShort: 'A senha precisa ter pelo menos 8 caracteres.',
  PasswordRequiresUpper: 'A senha precisa ter pelo menos uma letra maiúscula.',
  PasswordRequiresLower: 'A senha precisa ter pelo menos uma letra minúscula.',
  PasswordRequiresDigit: 'A senha precisa ter pelo menos um número.',
  PasswordRequiresNonAlphanumeric: 'A senha precisa ter pelo menos um símbolo (ex.: # ! @).',
  PasswordRequiresUniqueChars: 'A senha precisa ter mais caracteres diferentes.',
  DuplicateUserName: 'Este e-mail já está cadastrado.',
  DuplicateEmail: 'Este e-mail já está cadastrado.',
  InvalidEmail: 'E-mail inválido.',
  InvalidUserName: 'E-mail inválido.',
};

/**
 * Converte um erro de login/cadastro em uma lista de mensagens para mostrar ao usuário.
 * @param {unknown} err erro lançado por apiFetch (ou erro de rede)
 * @param {'login' | 'cadastro'} contexto
 */
export function mensagensDeErro(err, contexto) {
  if (!(err instanceof ApiError)) {
    return ['Não foi possível conectar ao servidor. Verifique se a API está rodando e tente novamente.'];
  }

  if (contexto === 'login' && err.status === 401) {
    return ['E-mail ou senha incorretos.'];
  }

  const errors = err.data?.errors;
  if (errors && typeof errors === 'object') {
    const mensagens = Object.entries(errors).map(([codigo, detalhes]) =>
      MENSAGENS[codigo] ?? (Array.isArray(detalhes) ? detalhes.join(' ') : 'Dados inválidos.'),
    );
    return [...new Set(mensagens)];
  }

  return ['Ocorreu um erro inesperado. Tente novamente.'];
}
