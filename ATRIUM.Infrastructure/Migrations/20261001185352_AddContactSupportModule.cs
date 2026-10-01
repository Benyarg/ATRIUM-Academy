using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATRIUM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactSupportModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsultasContacto",
                columns: table => new
                {
                    IdConsulta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TipoConsulta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroPedido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Asunto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorreoNotificacionEnviado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCorreoEnviado = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultasContacto", x => x.IdConsulta);
                    table.ForeignKey(
                        name: "FK_ConsultasContacto_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasContacto_Estado_FechaCreacion",
                table: "ConsultasContacto",
                columns: new[] { "Estado", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasContacto_UsuarioId",
                table: "ConsultasContacto",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultasContacto");
        }
    }
}
