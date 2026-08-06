using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AxiomPrimeServer.Migrations
{
    /// <inheritdoc />
    public partial class ActiveShipSelection2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "activeShip",
                table: "ShipInventories",
                newName: "ActiveShip");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActiveShip",
                table: "ShipInventories",
                newName: "activeShip");
        }
    }
}
