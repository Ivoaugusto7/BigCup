# Pendências — BigCup

Lista do que falta ou precisa melhorar. Marque com `[x]` ao concluir.

## Funcionalidades
- [ ] **Frontend: cadastro e login** (`/api/auth/register` e `/api/auth/login`), guardando o token e renovando com `/api/auth/refresh`.
- [ ] **Frontend: montar drink** — escolher recipiente, ingredientes e passos e salvar com `POST /api/drinks` (a API já está pronta).
- [ ] **Frontend: meus drinks e minhas avaliações** — listar, editar, apagar e avaliar (`PUT /api/drinks/{id}/avaliacao`, `GET /api/avaliacoes`).
- [ ] **Ligar o recipiente selecionado na tela ao drink** (hoje a seleção fica só no navegador, em `localStorage`).
- [ ] **Endpoints de Ingredientes** (`GET /api/ingredientes`) para sugerir nomes ao montar o drink.
- [ ] **Recuperação de senha / confirmação de e-mail**: os endpoints do Identity existem, mas falta configurar um `IEmailSender` real.

## Qualidade e manutenção
- [ ] **Enviar o CI do backend** (`.github/workflows/ci.yml`: build + verificação de migrations dos dois provedores). A alteração está pronta localmente, mas o push foi recusado porque a credencial do Git não tem o escopo `workflow`.
- [ ] **Testes automatizados do backend** (projeto de testes com WebApplicationFactory + SQLite; hoje o roteiro de testes é manual).
- [ ] **Favicon**: o `frontend/index.html` não define ícone, o que gera um erro 404 no console do navegador.
- [ ] **`dotnet build` dentro de `backend/`** pede o nome do projeto (há `.sln` e `.csproj` na mesma pasta); usar `dotnet build backend.csproj` ou mover a solução para a raiz.

## Produção
- [ ] **Definir `Cors:Origins`** em produção com o domínio do frontend (hoje vazio = nenhuma origem liberada).
- [ ] **Connection string de produção** via variável de ambiente/secret (`ConnectionStrings__SqlServer`) e rodar `dotnet ef database update --context SqlServerAppDbContext` no deploy.
- [ ] **URL da API no frontend** via `VITE_API_BASE_URL` (padrão `http://localhost:5000`).

## Concluído
- [x] Banco por configuração (SQLite em dev, SQL Server em produção), cada um com suas migrations; sem fallback automático
- [x] Usuários com login (ASP.NET Core Identity) — drinks e avaliações por usuário, avaliações privadas
- [x] Integridade no banco: nota 1–5, quantidade > 0, ingrediente único (sem diferenciar maiúsculas), ordem de passo única, uma avaliação por usuário/drink
- [x] Datas em UTC
- [x] Recipientes no banco com nomes distintos (Copo pequeno/médio/grande)
- [x] CORS restrito por configuração
- [x] Namespace das migrations (`BigCup.Migrations.*`) e `Unidade` só em `DrinkIngrediente`
- [x] POST/PUT de drinks com ingredientes e passos (DTOs) — commit `5052dc7`
- [x] Frontend lendo `capacidadeMl` dos recipientes — commit `2ad6a76`
- [x] Remover `node_modules`, `dist` e `.db` do repositório — commit `2ad6a76`
- [x] Pacotes com vulnerabilidades conhecidas (backend e frontend) — commit `2ad6a76`
