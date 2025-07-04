using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentCorewebAPI_Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMenuAndUserPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PermissionId",
                table: "Menus");

            migrationBuilder.RenameColumn(
                name: "PermissionName",
                table: "Menus",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Menus",
                newName: "Path");

            migrationBuilder.AddColumn<Guid>(
                name: "MenuId",
                table: "UserPermissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Menus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_MenuId",
                table: "UserPermissions",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_RoleId",
                table: "UserPermissions",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Menus_MenuId",
                table: "UserPermissions",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermissions_Roles_RoleId",
                table: "UserPermissions",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Menus_MenuId",
                table: "UserPermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermissions_Roles_RoleId",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_MenuId",
                table: "UserPermissions");

            migrationBuilder.DropIndex(
                name: "IX_UserPermissions_RoleId",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "MenuId",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Menus");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Menus",
                newName: "PermissionName");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Menus",
                newName: "Name");

            migrationBuilder.AddColumn<Guid>(
                name: "PermissionId",
                table: "Menus",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
