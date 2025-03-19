using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentCorewebAPI_Project.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStudentIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                    name: "PK_Students",
                    table: "Students");

            // 2. Drop the existing Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Students");

            // 3. Add the new Guid column (without IDENTITY)
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Students",
                nullable: false,
                defaultValueSql: "NEWID()"); // Auto-generate GUID

            // 4. Re-add Primary Key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Students",
                table: "Students",
                column: "Id");
        }        

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Students",
                table: "Students");

            // 2. Drop the Guid column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Students");

            // 3. Recreate the Id column as int with IDENTITY
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Students",
                nullable: false); // Restore IDENTITY

            // 4. Re-add Primary Key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Students",
                table: "Students",
                column: "Id");
        }
    }
}
