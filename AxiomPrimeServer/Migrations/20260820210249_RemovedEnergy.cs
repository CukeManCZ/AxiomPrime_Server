using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AxiomPrimeServer.Migrations
{
    /// <inheritdoc />
    public partial class RemovedEnergy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Energies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Energies",
                columns: table => new
                {
                    PlayerID = table.Column<string>(type: "text", nullable: false),
                    CurrentEnergy = table.Column<float>(type: "real", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxEnergy = table.Column<float>(type: "real", nullable: false),
                    RegenSpeed = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Energies", x => x.PlayerID);
                    table.ForeignKey(
                        name: "FK_Energies_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
