using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPageSublimation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceProductPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "precio_es_referencial",
                table: "productos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "precio_es_referencial",
                table: "productos");
        }
    }
}
