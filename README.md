# BigCup 

This monorepo contains a backend ASP.NET Core Web API and a frontend workspace shell.

## Structure

- `backend/` - ASP.NET Core Web API project with Entity Framework Core domain entities and REST controllers
- `frontend/` - placeholder folder for future frontend application

## Backend

The backend is configured with:

- ASP.NET Core Web API
- Entity Framework Core for relational data modeling
- Domain entities: `Drink`, `Ingrediente`, `DrinkIngrediente`, `PassoPreparo`, `Avaliacao`
- REST controllers for `Drinks` and a static list of `Recipientes`
- CORS enabled via the `AllowFrontend` policy

### Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/` | API status |
| GET | `/api/drinks` | List drinks with ingredients, steps, ratings and average rating |
| GET | `/api/drinks/{id}` | Get one drink |
| POST | `/api/drinks` | Create a drink |
| PUT | `/api/drinks/{id}` | Update a drink (replaces its ingredients and steps) |
| DELETE | `/api/drinks/{id}` | Delete a drink |
| GET | `/api/recipientes` | List available containers |

Request body for POST/PUT (ingredients are referenced by name; missing ones are created automatically):

```json
{
  "nome": "Caipirinha",
  "descricao": "Clássico brasileiro.",
  "tempoPreparoMinutos": 4,
  "ingredientes": [
    { "nome": "Cachaça", "quantidade": 50, "unidade": "ml" }
  ],
  "passos": [
    { "ordem": 1, "instrucao": "Macere o limão com o açúcar." }
  ]
}
```

See `backend/backend.http` for ready-to-run requests.

## Getting Started

Quick start (Windows), from the repository root in a terminal:

```powershell
.\iniciar.ps1                # or: .\iniciar.cmd   (add -SemNavegador to skip opening the browser)
```

It installs frontend dependencies if needed and runs the backend (http://localhost:5000, Swagger at
`/swagger`) and the frontend (http://localhost:3000) in the same terminal, without opening new windows,
then opens the app in the browser. Press Ctrl+C to stop both.

Open items are tracked in [PENDENCIAS.md](PENDENCIAS.md).

Manually, from `backend/`:

```powershell
cd backend
dotnet restore
dotnet run
```
