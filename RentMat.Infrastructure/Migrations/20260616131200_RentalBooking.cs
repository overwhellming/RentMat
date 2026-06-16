using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentMat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RentalBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Devices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "RentalBooking",
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
                    table.PrimaryKey("PK_RentalBooking", x => x.Id);
                    table.CheckConstraint("CK_RentalBooking_EndAfterStart", "\"EndDate\" > \"StartDate\"");
                    table.CheckConstraint("CK_RentalBooking_TotalPrice_NonNegative", "\"TotalPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_RentalBooking_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalBooking_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 1,
                column: "Status",
                value: "Available");

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 2,
                column: "Status",
                value: "Rented");

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 3,
                column: "Status",
                value: "Rented");

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 4,
                column: "Status",
                value: "Maintenance");

            migrationBuilder.InsertData(
                table: "RentalBooking",
                columns: new[] { "Id", "DeviceId", "EndDate", "StartDate", "Status", "TotalPrice", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTimeOffset(new DateTime(2026, 6, 10, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 10, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Completed", 750m, 1 },
                    { 4, 2, new DateTimeOffset(new DateTime(2026, 6, 25, 17, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Cancelled", 960m, 1 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HashedPassword" },
                values: new object[] { new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "hash" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Balance", "CreatedAt", "Email", "HashedPassword", "Login" },
                values: new object[,]
                {
                    { 2, 15000m, new DateTime(2024, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), "geralt@kaermorhen.com", "hash", "Geralt" },
                    { 3, 25000m, new DateTime(2024, 3, 20, 0, 0, 0, 0, DateTimeKind.Utc), "shepard@normandy.com", "hash", "Shepard" }
                });

            migrationBuilder.InsertData(
                table: "RentalBooking",
                columns: new[] { "Id", "DeviceId", "EndDate", "StartDate", "Status", "TotalPrice", "UserId" },
                values: new object[,]
                {
                    { 2, 2, new DateTimeOffset(new DateTime(2026, 6, 22, 18, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 20, 18, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Created", 5760m, 2 },
                    { 3, 3, new DateTimeOffset(new DateTime(2026, 6, 17, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 6, 16, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Active", 4800m, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceCategory_Name",
                table: "DeviceCategory",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RentalBooking_DeviceId",
                table: "RentalBooking",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalBooking_DeviceId_StartDate_EndDate",
                table: "RentalBooking",
                columns: new[] { "DeviceId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RentalBooking_UserId",
                table: "RentalBooking",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalBooking");

            migrationBuilder.DropIndex(
                name: "IX_DeviceCategory_Name",
                table: "DeviceCategory");

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Devices",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 1,
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 2,
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 3,
                column: "Status",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 4,
                column: "Status",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "HashedPassword" },
                values: new object[] { new DateTime(2007, 8, 29, 0, 0, 0, 0, DateTimeKind.Utc), "$2a$11$MX9xI7v09h4Lz6e6PZ7hCOgK8I.C2vS8iWx9wG1R1Wq8M2kM5G2t." });
        }
    }
}
