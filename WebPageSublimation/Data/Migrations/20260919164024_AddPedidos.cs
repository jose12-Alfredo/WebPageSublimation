using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebPageSublimation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_requerida = table.Column<DateOnly>(type: "date", nullable: true),
                    estado = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    promotor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotor_nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    proforma_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observaciones = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: true),
                    total = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedidos", x => x.id);
                    table.ForeignKey(
                        name: "fk_pedidos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedidos_proformas_proforma_id",
                        column: x => x.proforma_id,
                        principalTable: "proformas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedidos_promotores_promotor_id",
                        column: x => x.promotor_id,
                        principalTable: "promotores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "detalles_pedido",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    variante_nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    especificacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cantidad = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_detalles_pedido", x => x.id);
                    table.ForeignKey(
                        name: "fk_detalles_pedido_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_detalles_pedido_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_detalles_pedido_pedido_id",
                table: "detalles_pedido",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix_detalles_pedido_producto_id",
                table: "detalles_pedido",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_cliente_id",
                table: "pedidos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_fecha_utc",
                table: "pedidos",
                column: "fecha_utc");

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_numero",
                table: "pedidos",
                column: "numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_proforma_id",
                table: "pedidos",
                column: "proforma_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_promotor_id",
                table: "pedidos",
                column: "promotor_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalles_pedido");

            migrationBuilder.DropTable(
                name: "pedidos");
        }
    }
}
