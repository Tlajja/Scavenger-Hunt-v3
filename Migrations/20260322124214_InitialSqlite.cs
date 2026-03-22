using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PhotoScavengerHunt.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubmissionEndsAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VotingEndsAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsPrivate = table.Column<bool>(type: "INTEGER", nullable: false),
                    JoinCode = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxParticipants = table.Column<int>(type: "INTEGER", nullable: true),
                    WinnerId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChallengeId = table.Column<int>(type: "INTEGER", nullable: true),
                    PhotoUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Votes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TimerSeconds = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsRegistered = table.Column<bool>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PhotoSubmissionId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    ChallengeId = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: true)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChallengeId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhotoSubmissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_Photos_PhotoSubmissionId",
                        column: x => x.PhotoSubmissionId,
                        principalTable: "Photos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Votes_Users_UserId",
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
                columns: new[] { "Id", "AuthorId", "CreatedAt", "Deadline", "Description", "TimerSeconds" },
                values: new object[,]
                {
                    { 1, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Red car", null },
                    { 2, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Blue mailbox", null },
                    { 3, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Clock", null },
                    { 4, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Reflection in water or glass", null },
                    { 5, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bench with a view", null },
                    { 6, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Fire hydrant", null },
                    { 7, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Something with wheels (not a car)", null },
                    { 8, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Something perfectly symmetrical", null },
                    { 9, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tree with colorful leaves", null },
                    { 10, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Animal or pet (no humans visible)", null },
                    { 11, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Flower growing in an unexpected place", null },
                    { 12, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Cloud that looks like something", null },
                    { 13, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Interesting rock or stone", null },
                    { 14, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Door with a vibrant color", null },
                    { 15, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Park", null },
                    { 16, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Interesting street art or mural", null },
                    { 17, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Building with more than 10 floors", null },
                    { 18, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Statue", null },
                    { 19, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Street sign with an interesting name", null },
                    { 20, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Warning or caution sign", null },
                    { 21, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Advertisement with an animal", null },
                    { 22, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Sign in a language other than your native one", null },
                    { 23, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bicycle with a basket", null },
                    { 24, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Vehicle with a funny bumper sticker", null },
                    { 25, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Electric vehicle or charging station", null },
                    { 26, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Public transportation (bus, train, tram)", null },
                    { 27, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "House number that adds up to 10", null },
                    { 28, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Something with stripes", null },
                    { 29, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Three items of the same color in one photo", null },
                    { 30, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Perfect circle in nature or architecture", null },
                    { 31, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Coffee shop", null },
                    { 32, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Something yellow you can eat", null },
                    { 33, 0, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ice cream shop", null },
                    { 34, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Bakery", null },
                    { 38, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Flag flying in the wind", null },
                    { 40, 0, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, "Rainbow", null }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "IsRegistered", "Name", "PasswordHash", "Wins" },
                values: new object[,]
                {
                    { 1, "", false, "Ieva", "", 0 },
                    { 2, "", false, "Kristina", "", 0 },
                    { 3, "", false, "Ausra", "", 0 },
                    { 4, "", false, "Ula", "", 0 }
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

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PhotoSubmissionId_UserId",
                table: "Votes",
                columns: new[] { "PhotoSubmissionId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId",
                table: "Votes",
                column: "UserId");
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
                name: "Votes");

            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
