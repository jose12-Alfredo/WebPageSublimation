using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPageSublimation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    nit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    telefono = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    whats_app = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    persona_contacto = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    notas = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clientes_promotores",
                columns: table => new
                {
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clientes_promotores", x => new { x.cliente_id, x.promotor_id });
                    table.ForeignKey(
                        name: "fk_clientes_promotores_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_clientes_promotores_promotores_promotor_id",
                        column: x => x.promotor_id,
                        principalTable: "promotores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clientes_nombre",
                table: "clientes",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "ix_clientes_promotores_promotor_id",
                table: "clientes_promotores",
                column: "promotor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clientes_promotores");

            migrationBuilder.DropTable(
                name: "clientes");
        }
    }
}
