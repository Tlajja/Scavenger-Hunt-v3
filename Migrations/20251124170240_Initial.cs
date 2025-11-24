using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PhotoScavengerHunt.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    JoinCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    WinnerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ChallengeId = table.Column<int>(type: "int", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Votes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    IsRegistered = table.Column<bool>(type: "bit", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhotoSubmissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Photos_PhotoSubmissionId",
                        column: x => x.PhotoSubmissionId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeTasks",
                columns: table => new
                {
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    TaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeTasks", x => new { x.ChallengeId, x.TaskId });
                    table.ForeignKey(
                        name: "FK_ChallengeTasks_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeTasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Photos",
                columns: new[] { "Id", "ChallengeId", "PhotoUrl", "TaskId", "UserId", "Votes" },
                values: new object[,]
                {
                    { 1, null, "https://example.com/photo1.jpg", 1, 1, 5 },
                    { 2, null, "https://example.com/photo2.jpg", 2, 2, 3 }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AuthorId", "CreatedAt", "Deadline", "Description" },
                values: new object[,]
                {
                    { 1, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Red car" },
                    { 2, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Blue mailbox" },
                    { 3, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Clock" },
                    { 4, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Reflection in water or glass" },
                    { 5, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bench with a view" },
                    { 6, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Fire hydrant" },
                    { 7, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Something with wheels (not a car)" },
                    { 8, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Something perfectly symmetrical" },
                    { 9, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tree with colorful leaves" },
                    { 10, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Animal or pet (no humans visible)" },
                    { 11, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Flower growing in an unexpected place" },
                    { 12, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cloud that looks like something" },
                    { 13, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Interesting rock or stone" },
                    { 14, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Door with a vibrant color" },
                    { 15, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Park" },
                    { 16, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Interesting street art or mural" },
                    { 17, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Building with more than 10 floors" },
                    { 18, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Statue" },
                    { 19, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Street sign with an interesting name" },
                    { 20, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Warning or caution sign" },
                    { 21, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Advertisement with an animal" },
                    { 22, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sign in a language other than your native one" },
                    { 23, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bicycle with a basket" },
                    { 24, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Vehicle with a funny bumper sticker" },
                    { 25, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Electric vehicle or charging station" },
                    { 26, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Public transportation (bus, train, tram)" },
                    { 27, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "House number that adds up to 10" },
                    { 28, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Something with stripes" },
                    { 29, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Three items of the same color in one photo" },
                    { 30, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Perfect circle in nature or architecture" },
                    { 31, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Coffee shop" },
                    { 32, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Something yellow you can eat" },
                    { 33, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ice cream shop" },
                    { 34, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bakery" },
                    { 38, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Flag flying in the wind" },
                    { 40, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Rainbow" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Age", "Email", "IsRegistered", "Name", "PasswordHash", "Wins" },
                values: new object[,]
                {
                    { 1, 20, "", false, "Ieva", "", 0 },
                    { 2, 35, "", false, "Kristina", "", 0 },
                    { 3, 40, "", false, "Ausra", "", 0 },
                    { 4, 61, "", false, "Ula", "", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_ChallengeId",
                table: "ChallengeParticipants",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_UserId",
                table: "ChallengeParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeTasks_TaskId",
                table: "ChallengeTasks",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PhotoSubmissionId",
                table: "Comments",
                column: "PhotoSubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengeParticipants");

            migrationBuilder.DropTable(
                name: "ChallengeTasks");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Photos");
        }
    }
}
