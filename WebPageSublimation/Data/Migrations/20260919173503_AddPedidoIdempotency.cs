using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPageSublimation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPedidoIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "clave_idempotencia",
                table: "pedidos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_clave_idempotencia",
                table: "pedidos",
                column: "clave_idempotencia",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_pedidos_clave_idempotencia",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "clave_idempotencia",
                table: "pedidos");
        }
    }
}
