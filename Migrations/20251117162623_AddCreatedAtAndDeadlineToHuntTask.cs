using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoScavengerHunt.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtAndDeadlineToHuntTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tasks",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Deadline" },
                values: new object[] { new DateTime(2025, 11, 17, 16, 26, 22, 678, DateTimeKind.Utc).AddTicks(6592), null });

            migrationBuilder.UpdateData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Deadline" },
                values: new object[] { new DateTime(2025, 11, 17, 16, 26, 22, 678, DateTimeKind.Utc).AddTicks(7450), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "Tasks");
        }
    }
}
