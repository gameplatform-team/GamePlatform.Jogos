using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GamePlatform.Jogos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioJogos_Jogo_FK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UsuarioJogos_JogoId",
                table: "UsuarioJogos",
                column: "JogoId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuarioJogos_Jogo_JogoId",
                table: "UsuarioJogos",
                column: "JogoId",
                principalTable: "Jogo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuarioJogos_Jogo_JogoId",
                table: "UsuarioJogos");

            migrationBuilder.DropIndex(
                name: "IX_UsuarioJogos_JogoId",
                table: "UsuarioJogos");
        }
    }
}
