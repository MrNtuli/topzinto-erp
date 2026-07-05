using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Topzinto.Erp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailFromAddress",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailFromName",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpEnabled",
                table: "CompanySettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "CompanySettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpUseSsl",
                table: "CompanySettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SmtpUsername",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailFromAddress",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "EmailFromName",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SmtpEnabled",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SmtpUseSsl",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SmtpUsername",
                table: "CompanySettings");
        }
    }
}
