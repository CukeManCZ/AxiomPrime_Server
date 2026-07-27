using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AxiomPrimeServer.Migrations
{
    /// <inheritdoc />
    public partial class ShipJsonPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grid_Cells",
                table: "Ships");

            migrationBuilder.RenameColumn(
                name: "IsLocked",
                table: "Ships",
                newName: "Locked");

            migrationBuilder.RenameColumn(
                name: "Grid_Width",
                table: "Ships",
                newName: "MaxExp");

            migrationBuilder.RenameColumn(
                name: "Grid_Height",
                table: "Ships",
                newName: "Level");

            migrationBuilder.AddColumn<int>(
                name: "CurrentExp",
                table: "Ships",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Ships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipData",
                table: "Ships",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Ships",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentExp",
                table: "Ships");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Ships");

            migrationBuilder.DropColumn(
                name: "ShipData",
                table: "Ships");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Ships");

            migrationBuilder.RenameColumn(
                name: "MaxExp",
                table: "Ships",
                newName: "Grid_Width");

            migrationBuilder.RenameColumn(
                name: "Locked",
                table: "Ships",
                newName: "IsLocked");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Ships",
                newName: "Grid_Height");

            migrationBuilder.AddColumn<List<string>>(
                name: "Grid_Cells",
                table: "Ships",
                type: "text[]",
                nullable: false);
        }
    }
}
