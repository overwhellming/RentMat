using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentMat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RentalBookingRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalBookings");

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DeviceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.CheckConstraint("CK_RentalBooking_EndAfterStart", "\"EndDate\" > \"StartDate\"");
                    table.CheckConstraint("CK_RentalBooking_TotalPrice_NonNegative", "\"TotalPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_Bookings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Bookings",
                columns: new[] { "Id", "DeviceId", "EndDate", "StartDate", "Status", "TotalPrice", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTimeOffset(new DateTime(2026, 6, 10, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 10, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Completed", 750m, 1 },
                    { 2, 2, new DateTimeOffset(new DateTime(2026, 6, 22, 18, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 20, 18, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Created", 5760m, 2 },
                    { 3, 3, new DateTimeOffset(new DateTime(2026, 6, 17, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 16, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Active", 4800m, 3 },
                    { 4, 2, new DateTimeOffset(new DateTime(2026, 6, 25, 17, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cancelled", 960m, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_DeviceId",
                table: "Bookings",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_DeviceId_StartDate_EndDate",
                table: "Bookings",
                columns: new[] { "DeviceId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.CreateTable(
                name: "RentalBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalBookings", x => x.Id);
                    table.CheckConstraint("CK_RentalBooking_EndAfterStart", "\"EndDate\" > \"StartDate\"");
                    table.CheckConstraint("CK_RentalBooking_TotalPrice_NonNegative", "\"TotalPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_RentalBookings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RentalBookings",
                columns: new[] { "Id", "DeviceId", "EndDate", "StartDate", "Status", "TotalPrice", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTimeOffset(new DateTime(2026, 6, 10, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 10, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Completed", 750m, 1 },
                    { 2, 2, new DateTimeOffset(new DateTime(2026, 6, 22, 18, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 20, 18, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Created", 5760m, 2 },
                    { 3, 3, new DateTimeOffset(new DateTime(2026, 6, 17, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 16, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Active", 4800m, 3 },
                    { 4, 2, new DateTimeOffset(new DateTime(2026, 6, 25, 17, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cancelled", 960m, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalBookings_DeviceId",
                table: "RentalBookings",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalBookings_DeviceId_StartDate_EndDate",
                table: "RentalBookings",
                columns: new[] { "DeviceId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RentalBookings_UserId",
                table: "RentalBookings",
                column: "UserId");
        }
    }
}
