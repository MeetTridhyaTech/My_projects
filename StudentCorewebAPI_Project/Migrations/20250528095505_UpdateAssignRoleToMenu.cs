using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentCorewebAPI_Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssignRoleToMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenuPermissions_Permissions_PermissionId",
                table: "RoleMenuPermissions");

            migrationBuilder.DropIndex(
                name: "IX_RoleMenuPermissions_PermissionId",
                table: "RoleMenuPermissions");

            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "RoleMenuPermissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId",
                table: "RoleMenuPermissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuPermissions_PermissionId",
                table: "RoleMenuPermissions",
                column: "PermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenuPermissions_Permissions_PermissionId",
                table: "RoleMenuPermissions",
                column: "PermissionId",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
