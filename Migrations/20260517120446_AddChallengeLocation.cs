using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoScavengerHunt.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Challenges",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationName",
                table: "Challenges",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Challenges",
                type: "REAL",
                nullable: true);

            // Set default Vilnius location for existing challenges
            migrationBuilder.Sql(@"
                UPDATE Challenges
                SET Latitude = 54.6872,
                    Longitude = 25.2797,
                    LocationName = 'Vilnius, Lithuania'
                WHERE Latitude IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "LocationName",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Challenges");
        }
    }
}
