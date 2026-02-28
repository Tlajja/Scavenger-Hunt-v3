using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoScavengerHunt.Migrations
{
    /// <inheritdoc />
    public partial class AddHubFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HubId",
                table: "Photos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Hubs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HubMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HubId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HubMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HubMembers_Hubs_HubId",
                        column: x => x.HubId,
                        principalTable: "Hubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HubMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Photos",
                keyColumn: "Id",
                keyValue: 1,
                column: "HubId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Photos",
                keyColumn: "Id",
                keyValue: 2,
                column: "HubId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_HubMembers_HubId",
                table: "HubMembers",
                column: "HubId");

            migrationBuilder.CreateIndex(
                name: "IX_HubMembers_UserId",
                table: "HubMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Hubs_JoinCode",
                table: "Hubs",
                column: "JoinCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HubMembers");

            migrationBuilder.DropTable(
                name: "Hubs");

            migrationBuilder.DropColumn(
                name: "HubId",
                table: "Photos");
        }
    }
}
