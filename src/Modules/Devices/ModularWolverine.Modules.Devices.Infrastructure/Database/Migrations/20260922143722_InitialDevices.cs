using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularWolverine.Modules.Devices.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "devices");

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "devices",
                columns: table => new
                {
                    imei = table.Column<Guid>(type: "uuid", maxLength: 15, nullable: false),
                    imei1 = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devices", x => x.imei);
                });

            migrationBuilder.CreateIndex(
                name: "ix_devices_imei",
                schema: "devices",
                table: "devices",
                column: "imei1",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devices",
                schema: "devices");
        }
    }
}
