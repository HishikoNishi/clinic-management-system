using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class AddAppointmentCodeIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("28fcdeb9-fbaf-4613-9242-9b4ae3fe044d"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("73aedaa7-eb46-4b77-9655-24bcfeab7aea"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e0e18313-2d3e-40e5-9c12-3d2a675c91be"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("32e43546-6e66-4620-b5b9-56f7a736aeb5"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("0612d1f7-e8a8-449d-bb5a-a08bc357d1dd"));

            migrationBuilder.AddColumn<string>(
                name: "AppointmentCode",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentCode",
                table: "Appointments",
                column: "AppointmentCode",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_AppointmentCode",
                table: "Appointments");

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

            migrationBuilder.DropColumn(
                name: "AppointmentCode",
                table: "Appointments");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("0612d1f7-e8a8-449d-bb5a-a08bc357d1dd"), "Admin" },
                    { new Guid("28fcdeb9-fbaf-4613-9242-9b4ae3fe044d"), "Doctor" },
                    { new Guid("73aedaa7-eb46-4b77-9655-24bcfeab7aea"), "Guest" },
                    { new Guid("e0e18313-2d3e-40e5-9c12-3d2a675c91be"), "Staff" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("32e43546-6e66-4620-b5b9-56f7a736aeb5"), new DateTime(2026, 2, 15, 15, 32, 48, 759, DateTimeKind.Utc).AddTicks(9050), "", "", true, "AQAAAAIAAYagAAAAECdeon2RXN0QI0H813sHWMfsQMXVSeh+vUlmdm608Hia388M2fXzumkqjxTsOdGHEw==", "", new Guid("0612d1f7-e8a8-449d-bb5a-a08bc357d1dd"), "admin" });
        }
    }
}
