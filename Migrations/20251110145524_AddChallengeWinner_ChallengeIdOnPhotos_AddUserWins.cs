using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoScavengerHunt.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeWinner_ChallengeIdOnPhotos_AddUserWins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HubId",
                table: "Photos",
                newName: "ChallengeId");

            migrationBuilder.AddColumn<int>(
                name: "Wins",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WinnerId",
                table: "Challenges",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Wins",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Wins",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                column: "Wins",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4,
                column: "Wins",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Wins",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WinnerId",
                table: "Challenges");

            migrationBuilder.RenameColumn(
                name: "ChallengeId",
                table: "Photos",
                newName: "HubId");
        }
    }
}
