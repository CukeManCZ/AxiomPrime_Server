using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AxiomPrimeServer.Migrations
{
    /// <inheritdoc />
    public partial class ItemJsonPayloadFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEquipped",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Item");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEquipped",
                table: "Item",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "Item",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Item",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Item",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
