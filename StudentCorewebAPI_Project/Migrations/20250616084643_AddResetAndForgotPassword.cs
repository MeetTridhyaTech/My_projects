using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentCorewebAPI_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddResetAndForgotPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OTPExpiryTime",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetOTP",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OTPExpiryTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetOTP",
                table: "Users");
        }
    }
}
