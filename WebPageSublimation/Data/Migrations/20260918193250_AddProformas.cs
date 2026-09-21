using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebPageSublimation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProformas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proformas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    promotor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotor_nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    total = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proformas", x => x.id);
                    table.ForeignKey(
                        name: "fk_proformas_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_proformas_promotores_promotor_id",
                        column: x => x.promotor_id,
                        principalTable: "promotores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalles_proforma",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proforma_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    variante_nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    cantidad = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_detalles_proforma", x => x.id);
                    table.ForeignKey(
                        name: "fk_detalles_proforma_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_detalles_proforma_proformas_proforma_id",
                        column: x => x.proforma_id,
                        principalTable: "proformas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_detalles_proforma_producto_id",
                table: "detalles_proforma",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_detalles_proforma_proforma_id",
                table: "detalles_proforma",
                column: "proforma_id");

            migrationBuilder.CreateIndex(
                name: "ix_proformas_cliente_id",
                table: "proformas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_proformas_numero",
                table: "proformas",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proformas_promotor_id",
                table: "proformas",
                column: "promotor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalles_proforma");

            migrationBuilder.DropTable(
                name: "proformas");
        }
    }
}
