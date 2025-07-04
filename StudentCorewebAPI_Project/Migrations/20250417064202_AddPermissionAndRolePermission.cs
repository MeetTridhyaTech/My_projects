using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentCorewebAPI_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionAndRolePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Roles",
                newName: "RoleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoleID",
                table: "Roles",
                newName: "RoleId");
        }
    }
}
