using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPageSublimation.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendCatalogoImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "imagenes_producto",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_imagenes_producto", x => x.id);
                    table.ForeignKey(
                        name: "fk_imagenes_producto_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_imagenes_producto_producto_id",
                table: "imagenes_producto",
                column: "producto_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "imagenes_producto");
        }
    }
}
