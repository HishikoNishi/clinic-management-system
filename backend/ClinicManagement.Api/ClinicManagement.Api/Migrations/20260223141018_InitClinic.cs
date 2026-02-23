using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class InitClinic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    { new Guid("0dd796e9-3420-4f2e-8e70-335f0f158461"), "Staff" },
                    { new Guid("44e02b51-6878-4381-aed5-3966c57ff58f"), "Doctor" },
                    { new Guid("4dbabcf7-4707-4f5c-a5ac-6a920a5de1a0"), "Admin" },
                    { new Guid("bfdc9e83-db44-4374-afdc-47310b201f72"), "Guest" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[,]
                {
                    { new Guid("4a53c617-10dc-4832-a950-32831cc00c01"), new DateTime(2026, 2, 23, 14, 10, 18, 162, DateTimeKind.Utc).AddTicks(7670), "", "", true, "AQAAAAIAAYagAAAAEEVkjY0fRpanRMI4MdXWL7a29E87QsqLLgc703nxU4iLao6lS2QKVswWqAP00hozsw==", "", new Guid("4dbabcf7-4707-4f5c-a5ac-6a920a5de1a0"), "admin" },
                    { new Guid("b0aed582-58f5-4964-acd0-e3045a2bdf2b"), new DateTime(2026, 2, 23, 14, 10, 18, 199, DateTimeKind.Utc).AddTicks(6210), "", "", true, "AQAAAAIAAYagAAAAEGBhKjjVDSMi4OKLEYQ4T0EbfOqEjcKn1hvM3AHxB0A4SShhZXmCqZSnDPMLzgS8Lw==", "", new Guid("44e02b51-6878-4381-aed5-3966c57ff58f"), "doctor1" },
                    { new Guid("b23c8276-8b78-432e-8e2b-8b00424478c3"), new DateTime(2026, 2, 23, 14, 10, 18, 236, DateTimeKind.Utc).AddTicks(8050), "", "", true, "AQAAAAIAAYagAAAAEN2XdQv6Erb5qxcLb9O6Q7cfizMbQ9KQeyz2ETK9vnnTi/6pY04Bl8i/kmIafAzuDA==", "", new Guid("44e02b51-6878-4381-aed5-3966c57ff58f"), "doctor2" },
                    { new Guid("eb775b13-b4b2-407a-a7d0-b260beb596bf"), new DateTime(2026, 2, 23, 14, 10, 18, 273, DateTimeKind.Utc).AddTicks(400), "", "", true, "AQAAAAIAAYagAAAAEONmQ+jw6R1LtQlpcUeOGSVFRj5TVDJChjL7eSzTeJuohZYMpY2Bkwyh2xm1UQkUFw==", "", new Guid("44e02b51-6878-4381-aed5-3966c57ff58f"), "doctor3" }
                });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Code", "CreatedAt", "LicenseNumber", "Specialty", "Status", "UserId" },
                values: new object[] { new Guid("b8bc6a6b-008d-4c40-8905-73cce3488a0a"), "BS002", new DateTime(2026, 2, 23, 14, 10, 18, 309, DateTimeKind.Utc).AddTicks(1730), "LIC002", "Da liễu", 1, new Guid("b23c8276-8b78-432e-8e2b-8b00424478c3") });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Code", "CreatedAt", "LicenseNumber", "Specialty", "Status", "UserId" },
                values: new object[] { new Guid("bda96feb-e268-4162-99ae-74b63e84247c"), "BS003", new DateTime(2026, 2, 23, 14, 10, 18, 309, DateTimeKind.Utc).AddTicks(1730), "LIC003", "Tai mũi họng", 1, new Guid("eb775b13-b4b2-407a-a7d0-b260beb596bf") });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Code", "CreatedAt", "LicenseNumber", "Specialty", "Status", "UserId" },
                values: new object[] { new Guid("e344a26e-de2c-4adc-adbc-7a8527ab774b"), "BS001", new DateTime(2026, 2, 23, 14, 10, 18, 309, DateTimeKind.Utc).AddTicks(1720), "LIC001", "Nội tổng quát", 1, new Guid("b0aed582-58f5-4964-acd0-e3045a2bdf2b") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("b8bc6a6b-008d-4c40-8905-73cce3488a0a"));

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("bda96feb-e268-4162-99ae-74b63e84247c"));

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("e344a26e-de2c-4adc-adbc-7a8527ab774b"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("0dd796e9-3420-4f2e-8e70-335f0f158461"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("bfdc9e83-db44-4374-afdc-47310b201f72"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4a53c617-10dc-4832-a950-32831cc00c01"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4dbabcf7-4707-4f5c-a5ac-6a920a5de1a0"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b0aed582-58f5-4964-acd0-e3045a2bdf2b"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b23c8276-8b78-432e-8e2b-8b00424478c3"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("eb775b13-b4b2-407a-a7d0-b260beb596bf"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44e02b51-6878-4381-aed5-3966c57ff58f"));

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
    }
}
