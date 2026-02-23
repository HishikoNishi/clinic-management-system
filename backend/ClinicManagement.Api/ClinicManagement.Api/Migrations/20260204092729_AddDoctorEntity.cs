using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class AddDoctorEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("10a24d39-ea30-4b4f-8d1a-0211cc59c183"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4934a61f-b991-4909-803d-51e8777be120"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a7e24a3d-5e82-47b8-bca4-5394fb130ecf"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("3b2c1134-af8f-46ee-8531-83f6202fbee5"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6d6fdf04-9c61-4e14-b427-2cff66e75664"));

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_Users_UserId",
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
                    { new Guid("52c5d73d-2d26-4738-9c85-b6d4328ddeb3"), "Doctor" },
                    { new Guid("9a8b8bd1-a8d9-40a4-8347-f84949c12050"), "Staff" },
                    { new Guid("9d92991e-cb3c-4f6c-b95c-1a1fec0fe4d7"), "Guest" },
                    { new Guid("f620ba28-e59a-4da6-8284-e5b283fbd5f7"), "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("7fa98efa-1930-4e42-bdc3-160a7ffebadc"), new DateTime(2026, 2, 4, 9, 27, 29, 146, DateTimeKind.Utc).AddTicks(5857), "", "", true, "AQAAAAEAACcQAAAAEIAJ9S6TEUSouSxSFX98LW+OpY7uIzqTOjYljmWk6LVWynl3lIBjx7xRuWnMyc97yQ==", "", new Guid("f620ba28-e59a-4da6-8284-e5b283fbd5f7"), "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_UserId",
                table: "Doctors",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("52c5d73d-2d26-4738-9c85-b6d4328ddeb3"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("9a8b8bd1-a8d9-40a4-8347-f84949c12050"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("9d92991e-cb3c-4f6c-b95c-1a1fec0fe4d7"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("7fa98efa-1930-4e42-bdc3-160a7ffebadc"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("f620ba28-e59a-4da6-8284-e5b283fbd5f7"));

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("10a24d39-ea30-4b4f-8d1a-0211cc59c183"), "Guest" },
                    { new Guid("4934a61f-b991-4909-803d-51e8777be120"), "Staff" },
                    { new Guid("6d6fdf04-9c61-4e14-b427-2cff66e75664"), "Admin" },
                    { new Guid("a7e24a3d-5e82-47b8-bca4-5394fb130ecf"), "Doctor" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("3b2c1134-af8f-46ee-8531-83f6202fbee5"), new DateTime(2026, 1, 28, 7, 38, 31, 454, DateTimeKind.Utc).AddTicks(553), "", "", true, "AQAAAAEAACcQAAAAECdCZLO3yvCuTS8vojC17F/exw/OtburXL7SaYKVxqshae1A8dObCLOaaPWUaAn6lQ==", "", new Guid("6d6fdf04-9c61-4e14-b427-2cff66e75664"), "admin" });
        }
    }
}
