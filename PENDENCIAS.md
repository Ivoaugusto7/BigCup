# Pendências — BigCup

Lista do que falta ou precisa melhorar, levantada na revisão e no teste da aplicação (17/11/2026).
Marque com `[x]` ao concluir.

## Funcionalidades
- [ ] **Tela de drinks no frontend**: listar, ver detalhes, criar, editar e apagar drinks (a API `/api/drinks` já está pronta).
- [ ] **Ligar o recipiente selecionado ao drink** (hoje a seleção fica só no navegador, em `localStorage`).
- [ ] **Endpoints de Ingredientes** (`/api/ingredientes`): listar, renomear e apagar (apagar é bloqueado se o ingrediente estiver em uma receita).
- [ ] **Endpoints de Avaliações** (`/api/drinks/{id}/avaliacoes`): criar e listar avaliações (nota de 1 a 5).
- [ ] **Recipientes no banco de dados** em vez da lista fixa no `RecipientesController`.
- [ ] **Nomes distintos para os copos** (hoje há três recipientes chamados "Copo"; diferenciar, ex.: "Copo pequeno/médio/grande").

## Qualidade e manutenção
- [ ] **Testes automatizados do backend** (não existe nenhum projeto de testes).
- [ ] **CI compilar e testar o backend** (`.github/workflows/ci.yml` hoje só roda o frontend).
- [ ] **Favicon**: o `frontend/index.html` não define ícone, o que gera um erro 404 no console do navegador.
- [ ] **Namespace das migrations**: `backend.Migrations` vs `BigCup.*` no resto do código (definir `RootNamespace` no `backend.csproj`).
- [ ] **Campo `Unidade` duplicado** em `Ingrediente` e `DrinkIngrediente` — decidir qual vale.
- [ ] **`dotnet build` dentro de `backend/`** pede o nome do projeto (há `.sln` e `.csproj` na mesma pasta); usar `dotnet build backend.csproj` ou mover a solução para a raiz.
- [ ] **Fallback para SQLite** (`Program.cs`): as migrations são de SQL Server, então no SQLite o banco é criado com `EnsureCreated` e não recebe migrations futuras.

## Segurança / produção
- [ ] **CORS restrito em produção**: a política `AllowFrontend` aceita qualquer origem.
- [ ] **URL da API no frontend** via `VITE_API_BASE_URL` documentada (padrão `http://localhost:5000`).

## Concluído
- [x] POST/PUT de drinks com ingredientes e passos (DTOs) — commit `5052dc7`
- [x] Bloquear exclusão de ingrediente usado em receita — commit `5052dc7`
- [x] Frontend lendo `capacidadeMl` dos recipientes — commit `2ad6a76`
- [x] Remover `node_modules`, `dist` e `.db` do repositório — commit `2ad6a76`
- [x] Pacotes com vulnerabilidades conhecidas (backend e frontend) — commit `2ad6a76`
