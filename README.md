# BigCup Monorepo

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

## Getting Started

From `backend/`:

```powershell
cd backend
dotnet restore
dotnet run
```
