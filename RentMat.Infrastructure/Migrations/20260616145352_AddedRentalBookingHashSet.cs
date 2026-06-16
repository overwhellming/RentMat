using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentMat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedRentalBookingHashSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalBooking_Devices_DeviceId",
                table: "RentalBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_RentalBooking_Users_UserId",
                table: "RentalBooking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RentalBooking",
                table: "RentalBooking");

            migrationBuilder.RenameTable(
                name: "RentalBooking",
                newName: "RentalBookings");

            migrationBuilder.RenameIndex(
                name: "IX_RentalBooking_UserId",
                table: "RentalBookings",
                newName: "IX_RentalBookings_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RentalBooking_DeviceId_StartDate_EndDate",
                table: "RentalBookings",
                newName: "IX_RentalBookings_DeviceId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_RentalBooking_DeviceId",
                table: "RentalBookings",
                newName: "IX_RentalBookings_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RentalBookings",
                table: "RentalBookings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalBookings_Devices_DeviceId",
                table: "RentalBookings",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RentalBookings_Users_UserId",
                table: "RentalBookings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalBookings_Devices_DeviceId",
                table: "RentalBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_RentalBookings_Users_UserId",
                table: "RentalBookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RentalBookings",
                table: "RentalBookings");

            migrationBuilder.RenameTable(
                name: "RentalBookings",
                newName: "RentalBooking");

            migrationBuilder.RenameIndex(
                name: "IX_RentalBookings_UserId",
                table: "RentalBooking",
                newName: "IX_RentalBooking_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RentalBookings_DeviceId_StartDate_EndDate",
                table: "RentalBooking",
                newName: "IX_RentalBooking_DeviceId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_RentalBookings_DeviceId",
                table: "RentalBooking",
                newName: "IX_RentalBooking_DeviceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RentalBooking",
                table: "RentalBooking",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RentalBooking_Devices_DeviceId",
                table: "RentalBooking",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RentalBooking_Users_UserId",
                table: "RentalBooking",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
