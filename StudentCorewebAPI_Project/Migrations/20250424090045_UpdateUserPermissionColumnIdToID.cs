using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentCorewebAPI_Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserPermissionColumnIdToID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Roles_RoleId",
                table: "UserPermissions");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "UserPermissions",
                newName: "RoleID");

            migrationBuilder.RenameIndex(
                name: "IX_UserPermissions_RoleId",
                table: "UserPermissions",
                newName: "IX_UserPermissions_RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Roles_RoleID",
                table: "UserPermissions",
                column: "RoleID",
                principalTable: "Roles",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Roles_RoleID",
                table: "UserPermissions");

            migrationBuilder.RenameColumn(
                name: "RoleID",
                table: "UserPermissions",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPermissions_RoleID",
                table: "UserPermissions",
                newName: "IX_UserPermissions_RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Roles_RoleId",
                table: "UserPermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
