using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class AddDbClinic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("45b8695a-11b5-497a-98da-0e169d7b12be"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("875ed6cc-33a2-409d-bcaf-6ffb45154d33"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e28c0d7d-5858-4403-8c57-2aa1e8a8ddf3"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f5b9308c-6f4e-48e6-9a8c-d2ff398e0c5c"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b8a8b0cc-b8a1-454a-a607-ec28ca3c6c1f"));

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staffs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Doctor" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Staff" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Guest" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2026, 2, 20, 12, 29, 15, 851, DateTimeKind.Utc).AddTicks(1984), "", "", true, "AQAAAAEAACcQAAAAELS3o3QrQRK8sQOCMoCHVpp0Z6XhVPTGQtSBwTuc+5P+asE8rod+VrxuWBIrfQlzUA==", "", new Guid("11111111-1111-1111-1111-111111111111"), "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_Code",
                table: "Doctors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Code",
                table: "Staffs",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_UserId",
                table: "Staffs",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_Code",
                table: "Doctors");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("45b8695a-11b5-497a-98da-0e169d7b12be"), "Staff" },
                    { new Guid("875ed6cc-33a2-409d-bcaf-6ffb45154d33"), "Doctor" },
                    { new Guid("b8a8b0cc-b8a1-454a-a607-ec28ca3c6c1f"), "Admin" },
                    { new Guid("e28c0d7d-5858-4403-8c57-2aa1e8a8ddf3"), "Guest" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("f5b9308c-6f4e-48e6-9a8c-d2ff398e0c5c"), new DateTime(2026, 2, 16, 12, 56, 12, 263, DateTimeKind.Utc).AddTicks(3930), "", "", true, "AQAAAAIAAYagAAAAEJUm6rKbiNF+lmBAE1TsYt2mbAWTjRUWqdkcH3nTP9prKesIB4yTJ3DbPTXJGsxQBw==", "", new Guid("b8a8b0cc-b8a1-454a-a607-ec28ca3c6c1f"), "admin" });
        }
    }
}
