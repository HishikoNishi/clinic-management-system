using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class InitialClinic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    { new Guid("68590a3b-7378-4b39-b10c-4388b46bb20c"), "Guest" },
                    { new Guid("7d35577f-f9b8-4259-b272-b4dcc64a0212"), "Admin" },
                    { new Guid("bf96b140-00d1-49e8-81b7-a4a72a6d11ba"), "Doctor" },
                    { new Guid("f3e32a71-4956-4316-988c-aaee62d78626"), "Staff" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("b00cbca7-722b-4071-b601-aab336310eef"), new DateTime(2026, 2, 11, 5, 6, 29, 363, DateTimeKind.Utc).AddTicks(8170), "", "", true, "AQAAAAIAAYagAAAAEJQVwAHyTB54YJY8LjmU73pqdpNbP4iqgsG7GGuN0oczrRPwgvdYjVWoWVLiAD/Dgg==", "", new Guid("7d35577f-f9b8-4259-b272-b4dcc64a0212"), "admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("68590a3b-7378-4b39-b10c-4388b46bb20c"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("bf96b140-00d1-49e8-81b7-a4a72a6d11ba"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("f3e32a71-4956-4316-988c-aaee62d78626"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b00cbca7-722b-4071-b601-aab336310eef"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("7d35577f-f9b8-4259-b272-b4dcc64a0212"));

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
        }
    }
}
