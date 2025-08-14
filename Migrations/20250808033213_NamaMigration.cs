using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace bot_social_media.Migrations
{
    public partial class NamaMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerificationToken",
                table: "Accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationTokenExpiry",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationToken",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "VerificationTokenExpiry",
                table: "Accounts");
        }
    }
}
