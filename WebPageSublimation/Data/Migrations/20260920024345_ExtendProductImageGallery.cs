using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPageSublimation.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendProductImageGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_imagenes_producto_producto_id",
                table: "imagenes_producto");

            migrationBuilder.AddColumn<bool>(
                name: "es_principal",
                table: "imagenes_producto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "orden",
                table: "imagenes_producto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE imagenes_producto SET es_principal = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_imagenes_producto_producto_id_es_principal",
                table: "imagenes_producto",
                columns: new[] { "producto_id", "es_principal" });

            migrationBuilder.CreateIndex(
                name: "ix_imagenes_producto_producto_id_orden",
                table: "imagenes_producto",
                columns: new[] { "producto_id", "orden" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_imagenes_producto_producto_id_es_principal",
                table: "imagenes_producto");

            migrationBuilder.DropIndex(
                name: "ix_imagenes_producto_producto_id_orden",
                table: "imagenes_producto");

            migrationBuilder.DropColumn(
                name: "es_principal",
                table: "imagenes_producto");

            migrationBuilder.DropColumn(
                name: "orden",
                table: "imagenes_producto");

            migrationBuilder.CreateIndex(
                name: "ix_imagenes_producto_producto_id",
                table: "imagenes_producto",
                column: "producto_id",
                unique: true);
        }
    }
}
