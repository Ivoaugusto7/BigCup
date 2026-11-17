using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class IngredienteDeleteRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrinkIngredientes_Ingredientes_IngredienteId",
                table: "DrinkIngredientes");

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkIngredientes_Ingredientes_IngredienteId",
                table: "DrinkIngredientes",
                column: "IngredienteId",
                principalTable: "Ingredientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrinkIngredientes_Ingredientes_IngredienteId",
                table: "DrinkIngredientes");

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkIngredientes_Ingredientes_IngredienteId",
                table: "DrinkIngredientes",
                column: "IngredienteId",
                principalTable: "Ingredientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
