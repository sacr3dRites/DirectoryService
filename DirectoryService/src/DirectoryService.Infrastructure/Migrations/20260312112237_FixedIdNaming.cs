using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixedIdNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "positions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "locations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "departments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DepartmentPositionId",
                table: "department_positions",
                newName: "department_position_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentLocationId",
                table: "department_locations",
                newName: "department_location_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "positions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "locations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "departments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "department_position_id",
                table: "department_positions",
                newName: "DepartmentPositionId");

            migrationBuilder.RenameColumn(
                name: "department_location_id",
                table: "department_locations",
                newName: "DepartmentLocationId");
        }
    }
}
