# BigCup

Drink Builder: each user signs in, builds their own drinks (ingredients, steps, container) and rates them.
Ratings are private — only the user who wrote them can see them — and are stored for the next visit.

## Structure

- `backend/` - ASP.NET Core Web API (.NET 10) + Entity Framework Core + ASP.NET Core Identity
- `frontend/` - React + Vite + Tailwind

## Getting Started

Quick start (Windows), from the repository root in a terminal:

```powershell
.\iniciar.ps1                # or: .\iniciar.cmd   (add -SemNavegador to skip opening the browser)
```

It installs frontend dependencies if needed and runs the backend (http://localhost:5000, Swagger at
`/swagger`) and the frontend (http://localhost:3000) in the same terminal, without opening new windows,
then opens the app in the browser. Press Ctrl+C to stop both.

Open items are tracked in [PENDENCIAS.md](PENDENCIAS.md).

## Database

The provider is chosen **by configuration** — there is no automatic fallback:

| Setting | Development (`appsettings.Development.json`) | Production (`appsettings.json`) |
|---|---|---|
| `Database:Provider` | `Sqlite` | `SqlServer` |
| `Database:MigrateOnStartup` | `true` (migrations + seed applied when the API starts) | `false` |
| Connection string | `ConnectionStrings:Sqlite` → `backend/App_Data/bigcup.db` | `ConnectionStrings:SqlServer` |

Override with environment variables, e.g. to use LocalDB in development:

```powershell
$env:Database__Provider = 'SqlServer'
.\iniciar.ps1
```

Each provider has its own DbContext and migrations (`SqliteAppDbContext` → `Migrations/Sqlite`,
`SqlServerAppDbContext` → `Migrations/SqlServer`). **Every model change needs a migration for both**:

```powershell
cd backend
dotnet ef migrations add <Name> --context SqliteAppDbContext    --output-dir Migrations/Sqlite
dotnet ef migrations add <Name> --context SqlServerAppDbContext --output-dir Migrations/SqlServer
```

CI fails if either provider has pending model changes.

Apply migrations to a production SQL Server (also runs the seed):

```powershell
cd backend
$env:ConnectionStrings__SqlServer = '<production connection string>'
dotnet ef database update --context SqlServerAppDbContext
```

Data rules enforced by the database: one rating per user per drink, rating between 1 and 5, quantity > 0,
unique ingredient names (case-insensitive), unique step order per drink. Dates are stored and returned in UTC.

## API

Authentication uses ASP.NET Core Identity bearer tokens: register, log in, then send
`Authorization: Bearer <accessToken>`. In Swagger, use the **Authorize** button.

| Method | Route | Auth | Description |
|---|---|---|---|
| GET | `/` | – | API status |
| POST | `/api/auth/register` | – | Create account `{ email, password }` (min. 8 chars, upper, lower, digit, symbol) |
| POST | `/api/auth/login` | – | Log in `{ email, password }` → `{ accessToken, refreshToken, expiresIn }` |
| POST | `/api/auth/refresh` | – | New token from `{ refreshToken }` |
| GET | `/api/auth/manage/info` | ✔ | Logged-in user's info |
| GET | `/api/recipientes` | – | Available containers |
| GET | `/api/drinks` | ✔ | My drinks + public sample drinks (with **my** rating only) |
| GET | `/api/drinks/{id}` | ✔ | One drink (404 if it belongs to another user) |
| POST | `/api/drinks` | ✔ | Create a drink |
| PUT | `/api/drinks/{id}` | ✔ | Update my drink (replaces ingredients and steps) |
| DELETE | `/api/drinks/{id}` | ✔ | Delete my drink |
| PUT | `/api/drinks/{id}/avaliacao` | ✔ | Create or update my rating `{ nota: 1-5, comentario }` |
| GET | `/api/drinks/{id}/avaliacao` | ✔ | My rating for a drink |
| DELETE | `/api/drinks/{id}/avaliacao` | ✔ | Delete my rating |
| GET | `/api/avaliacoes` | ✔ | All my ratings |

Drink body for POST/PUT (ingredients are referenced by name; missing ones are created automatically):

```json
{
  "nome": "Caipirinha",
  "descricao": "Clássico brasileiro.",
  "tempoPreparoMinutos": 4,
  "recipienteId": 2,
  "ingredientes": [
    { "nome": "Cachaça", "quantidade": 50, "unidade": "ml" }
  ],
  "passos": [
    { "ordem": 1, "instrucao": "Macere o limão com o açúcar." }
  ]
}
```

See `backend/backend.http` for ready-to-run requests.
