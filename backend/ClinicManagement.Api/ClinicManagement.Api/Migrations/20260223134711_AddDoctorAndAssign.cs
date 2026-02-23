using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class AddDoctorAndAssign : Migration
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

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("12969bbd-8b20-4530-ab68-21539719c581"), "Admin" },
                    { new Guid("41b710e7-2116-4dc1-a500-7ec87b31f2e3"), "Doctor" },
                    { new Guid("692c4f84-49ee-4dab-902d-c5849db5189c"), "Staff" },
                    { new Guid("a0b877bf-5e3b-4430-bfc0-2f8951ac4db1"), "Guest" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("0ab793c9-c294-4605-84dd-b7a5cc8a72c6"), new DateTime(2026, 2, 23, 13, 47, 11, 325, DateTimeKind.Utc).AddTicks(700), "", "", true, "AQAAAAIAAYagAAAAEJyeEoXSraIwqzPYiUJYcTZX2+EjnIWtMIRnoYspYYTMeP3v40hAmq6Q4Wzuqvuvig==", "", new Guid("12969bbd-8b20-4530-ab68-21539719c581"), "admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("41b710e7-2116-4dc1-a500-7ec87b31f2e3"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("692c4f84-49ee-4dab-902d-c5849db5189c"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a0b877bf-5e3b-4430-bfc0-2f8951ac4db1"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("0ab793c9-c294-4605-84dd-b7a5cc8a72c6"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("12969bbd-8b20-4530-ab68-21539719c581"));

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
