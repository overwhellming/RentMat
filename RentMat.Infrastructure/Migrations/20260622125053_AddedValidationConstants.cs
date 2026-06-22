using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentMat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedValidationConstants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceCategory_CategoryId",
                table: "Devices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceCategory",
                table: "DeviceCategory");

            migrationBuilder.RenameTable(
                name: "DeviceCategory",
                newName: "DeviceCategories");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceCategory_Name",
                table: "DeviceCategories",
                newName: "IX_DeviceCategories_Name");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "HashedPassword",
                table: "Users",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DeviceCategories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceCategories",
                table: "DeviceCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceCategories_CategoryId",
                table: "Devices",
                column: "CategoryId",
                principalTable: "DeviceCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceCategories_CategoryId",
                table: "Devices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceCategories",
                table: "DeviceCategories");

            migrationBuilder.RenameTable(
                name: "DeviceCategories",
                newName: "DeviceCategory");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceCategories_Name",
                table: "DeviceCategory",
                newName: "IX_DeviceCategory_Name");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "HashedPassword",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DeviceCategory",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceCategory",
                table: "DeviceCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceCategory_CategoryId",
                table: "Devices",
                column: "CategoryId",
                principalTable: "DeviceCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
