using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoScavengerHunt.Migrations
{
    /// <inheritdoc />
    public partial class AddMaxParticipantsToChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "Challenges",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "Challenges");
        }
    }
}
