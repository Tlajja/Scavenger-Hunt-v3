using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PhotoScavengerHunt.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "Challenges");

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

            // idempotent seed that allows inserting explicit identity values:
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Tasks]') IS NOT NULL
BEGIN
  SET IDENTITY_INSERT [Tasks] ON;

  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 3)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (3,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Clock');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 4)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (4,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Reflection in water or glass');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 5)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (5,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Bench with a view');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 6)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (6,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Fire hydrant');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 7)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (7,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Something with wheels (not a car)');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 8)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (8,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Something perfectly symmetrical');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 9)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (9,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Tree with colorful leaves');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 10)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (10,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Animal or pet (no humans visible)');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 11)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (11,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Flower growing in an unexpected place');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 12)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (12,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Cloud that looks like something');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 13)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (13,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Interesting rock or stone');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 14)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (14,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Door with a vibrant color');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 15)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (15,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Park');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 16)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (16,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Interesting street art or mural');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 17)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (17,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Building with more than 10 floors');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 18)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (18,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Statue');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 19)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (19,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Street sign with an interesting name');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 20)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (20,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Warning or caution sign');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 21)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (21,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Advertisement with an animal');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 22)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (22,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Sign in a language other than your native one');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 23)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (23,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Bicycle with a basket');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 24)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (24,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Vehicle with a funny bumper sticker');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 25)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (25,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Electric vehicle or charging station');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 26)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (26,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Public transportation (bus, train, tram)');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 27)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (27,0,'2025-01-01T00:00:00.0000000Z',NULL,N'House number that adds up to 10');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 28)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (28,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Something with stripes');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 29)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (29,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Three items of the same color in one photo');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 30)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (30,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Perfect circle in nature or architecture');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 31)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (31,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Coffee shop');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 32)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (32,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Something yellow you can eat');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 33)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (33,0,'2025-01-01T00:00:00.0000000Z',NULL,N'Ice cream shop');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 34)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (34,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Bakery');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 38)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (38,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Flag flying in the wind');
  IF NOT EXISTS (SELECT 1 FROM [Tasks] WHERE [Id] = 40)
    INSERT INTO [Tasks] ([Id],[AuthorId],[CreatedAt],[Deadline],[Description]) VALUES (40,0,'2025-01-02T00:00:00.0000000Z',NULL,N'Rainbow');

  SET IDENTITY_INSERT [Tasks] OFF;
END
");
             migrationBuilder.CreateIndex(
                 name: "IX_ChallengeTasks_TaskId",
                 table: "ChallengeTasks",
                 column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallengeTasks");

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Tasks",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
