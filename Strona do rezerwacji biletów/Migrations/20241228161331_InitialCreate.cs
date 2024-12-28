using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Strona_do_rezerwacji_biletów.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AvailableSeats",
                table: "Events",
                newName: "AvailableVIPSeats");

            migrationBuilder.AddColumn<bool>(
                name: "IsVIP",
                table: "Reservations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AvailableNormalSeats",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_EventId",
                table: "Reservations",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Events_EventId",
                table: "Reservations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Events_EventId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_EventId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsVIP",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "AvailableNormalSeats",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "AvailableVIPSeats",
                table: "Events",
                newName: "AvailableSeats");
        }
    }
}
