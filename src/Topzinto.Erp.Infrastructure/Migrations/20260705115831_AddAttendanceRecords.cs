using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Topzinto.Erp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    WorkDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckInTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    HoursWorked = table.Column<decimal>(type: "TEXT", precision: 6, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeId_WorkDate",
                table: "AttendanceRecords",
                columns: new[] { "EmployeeId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ProjectId_WorkDate",
                table: "AttendanceRecords",
                columns: new[] { "ProjectId", "WorkDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");
        }
    }
}
