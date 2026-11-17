using Microsoft.EntityFrameworkCore.Migrations;

namespace BigCup.Data
{
    /// <summary>
    /// Views só para consulta no banco (DB Browser / SQL Server): mostram os nomes de drink e ingrediente ao lado dos IDs.
    /// Os nomes vêm sempre atualizados de Drinks/Ingredientes (nada é duplicado). Não são mapeadas no EF.
    /// O SQL é o mesmo para SQLite e SQL Server; usado pelas migrations AddViewsComNomes dos dois provedores.
    /// </summary>
    public static class DatabaseViews
    {
        private static readonly (string Nome, string Select)[] Views =
        {
            ("vw_DrinkIngredientes",
                @"SELECT di.DrinkId, d.Nome AS DrinkNome, di.IngredienteId, i.Nome AS IngredienteNome,
                         di.Quantidade, di.Unidade
                  FROM DrinkIngredientes di
                  INNER JOIN Drinks d ON d.Id = di.DrinkId
                  INNER JOIN Ingredientes i ON i.Id = di.IngredienteId"),

            ("vw_PassosPreparo",
                @"SELECT p.Id, p.DrinkId, d.Nome AS DrinkNome, p.Ordem, p.Instrucao
                  FROM PassosPreparo p
                  INNER JOIN Drinks d ON d.Id = p.DrinkId"),

            ("vw_Avaliacoes",
                @"SELECT a.Id, a.DrinkId, d.Nome AS DrinkNome, a.UsuarioId, a.Nota, a.Comentario, a.DataAvaliacao
                  FROM Avaliacoes a
                  INNER JOIN Drinks d ON d.Id = a.DrinkId")
        };

        public static void CreateViewsComNomes(this MigrationBuilder migrationBuilder)
        {
            // Um CREATE VIEW por comando: o SQL Server exige que seja o único statement do lote.
            foreach (var (nome, select) in Views)
            {
                migrationBuilder.Sql($"CREATE VIEW {nome} AS {select}");
            }
        }

        public static void DropViewsComNomes(this MigrationBuilder migrationBuilder)
        {
            foreach (var (nome, _) in Views.Reverse())
            {
                migrationBuilder.Sql($"DROP VIEW {nome}");
            }
        }
    }
}
