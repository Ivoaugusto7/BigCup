using BigCup.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigCup.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddViewsComNomes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateViewsComNomes();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropViewsComNomes();
        }
    }
}
