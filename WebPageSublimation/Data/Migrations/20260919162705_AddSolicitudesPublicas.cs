using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPageSublimation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudesPublicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "solicitudes_publicas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    empresa = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: true),
                    telefono = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    detalle = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitudes_publicas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_solicitudes_publicas_created_at_utc",
                table: "solicitudes_publicas",
                column: "created_at_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "solicitudes_publicas");
        }
    }
}
