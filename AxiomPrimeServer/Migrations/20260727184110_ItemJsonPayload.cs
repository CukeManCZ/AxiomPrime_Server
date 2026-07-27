using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AxiomPrimeServer.Migrations
{
    /// <inheritdoc />
    public partial class ItemJsonPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size_Height",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Size_Values",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Size_Width",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "StatsData",
                table: "Item");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "Item",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "ItemData",
                table: "Item",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemData",
                table: "Item");

            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                table: "Item",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Size_Height",
                table: "Item",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<bool>>(
                name: "Size_Values",
                table: "Item",
                type: "boolean[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "Size_Width",
                table: "Item",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatsData",
                table: "Item",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
