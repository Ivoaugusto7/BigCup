import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import AuthPage from './AuthPage';
import { AuthContext } from '../../auth/AuthContext';
import { ApiError } from '../../api/client';

function renderizar({ entrar = vi.fn().mockResolvedValue(), cadastrar = vi.fn().mockResolvedValue() } = {}) {
  render(
    <AuthContext.Provider value={{ entrar, cadastrar, usuario: null, carregando: false, sair: vi.fn() }}>
      <AuthPage />
    </AuthContext.Provider>,
  );
  return { entrar, cadastrar };
}

function preencher(rotulo, valor) {
  fireEvent.change(screen.getByLabelText(rotulo), { target: { value: valor } });
}

describe('AuthPage', () => {
  test('login com sucesso chama entrar com e-mail (sem espaços) e senha', async () => {
    const { entrar } = renderizar();

    preencher('E-mail', '  eu@exemplo.com ');
    preencher('Senha', 'Senha#123');
    fireEvent.click(screen.getByRole('button', { name: 'Entrar' }));

    await waitFor(() => expect(entrar).toHaveBeenCalledWith('eu@exemplo.com', 'Senha#123'));
  });

  test('login recusado (401) mostra "E-mail ou senha incorretos."', async () => {
    renderizar({ entrar: vi.fn().mockRejectedValue(new ApiError(401, null)) });

    preencher('E-mail', 'eu@exemplo.com');
    preencher('Senha', 'errada');
    fireEvent.click(screen.getByRole('button', { name: 'Entrar' }));

    expect(await screen.findByText('E-mail ou senha incorretos.')).toBeInTheDocument();
  });

  test('erros de cadastro aparecem traduzidos', async () => {
    const erro = new ApiError(400, {
      errors: { PasswordRequiresUpper: ['x'], DuplicateUserName: ['y'], DuplicateEmail: ['z'] },
    });
    renderizar({ cadastrar: vi.fn().mockRejectedValue(erro) });

    fireEvent.click(screen.getByRole('tab', { name: 'Criar conta' }));
    preencher('E-mail', 'eu@exemplo.com');
    preencher('Senha', 'senha#123');
    preencher('Confirmar senha', 'senha#123');
    fireEvent.click(screen.getByRole('button', { name: 'Criar conta' }));

    expect(await screen.findByText('A senha precisa ter pelo menos uma letra maiúscula.')).toBeInTheDocument();
    // DuplicateUserName e DuplicateEmail viram a mesma mensagem, mostrada uma vez só
    expect(screen.getAllByText('Este e-mail já está cadastrado.')).toHaveLength(1);
  });

  test('senhas diferentes no cadastro são bloqueadas antes de chamar a API', async () => {
    const { cadastrar } = renderizar();

    fireEvent.click(screen.getByRole('tab', { name: 'Criar conta' }));
    preencher('E-mail', 'eu@exemplo.com');
    preencher('Senha', 'Senha#123');
    preencher('Confirmar senha', 'Senha#124');
    fireEvent.click(screen.getByRole('button', { name: 'Criar conta' }));

    expect(await screen.findByText('As senhas não conferem.')).toBeInTheDocument();
    expect(cadastrar).not.toHaveBeenCalled();
  });

  test('servidor fora do ar mostra mensagem de conexão', async () => {
    renderizar({ entrar: vi.fn().mockRejectedValue(new TypeError('Failed to fetch')) });

    preencher('E-mail', 'eu@exemplo.com');
    preencher('Senha', 'Senha#123');
    fireEvent.click(screen.getByRole('button', { name: 'Entrar' }));

    expect(await screen.findByText(/Não foi possível conectar ao servidor/)).toBeInTheDocument();
  });
});
