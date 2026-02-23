using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class SeedDoctors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    { new Guid("1e24ab81-f2b0-4c2b-bc3a-4c8c73d83874"), "Doctor" },
                    { new Guid("4a8a9bf3-145a-4b0e-a8ef-0e10e53ad092"), "Admin" },
                    { new Guid("d52fb928-a510-43ac-ba22-64fa7130d709"), "Guest" },
                    { new Guid("f200a9e1-1c5c-426f-a28a-4a88fb3a6362"), "Staff" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[,]
                {
                    { new Guid("55b18eb4-500c-496c-acc7-419c0ce53ce8"), new DateTime(2026, 2, 23, 14, 13, 24, 259, DateTimeKind.Utc).AddTicks(2480), "", "", true, "AQAAAAIAAYagAAAAEKlMV+tmoWKmkfQ1BljAjkyIk1Eu/XvCUhJlhi7VBWV/U1G8wrNdLzhPm4PECP7+BQ==", "", new Guid("4a8a9bf3-145a-4b0e-a8ef-0e10e53ad092"), "admin" },
                    { new Guid("59ab6848-1b08-469a-9870-eafb691e2f7a"), new DateTime(2026, 2, 23, 14, 13, 24, 335, DateTimeKind.Utc).AddTicks(8240), "", "", true, "AQAAAAIAAYagAAAAEPKVTzfuFeP5m/GXiwpnY6+JBLqffrFUuC5LM7PIXJ2wzepJpa2/DjixPh9zhHcKJA==", "", new Guid("1e24ab81-f2b0-4c2b-bc3a-4c8c73d83874"), "doctor2" },
                    { new Guid("b890920a-966a-40ca-845d-006ac6d9a558"), new DateTime(2026, 2, 23, 14, 13, 24, 297, DateTimeKind.Utc).AddTicks(8550), "", "", true, "AQAAAAIAAYagAAAAEJpuB/lefsMZkWLTBLE5nlMPqg8zPx5A1/3jlj4qYpw/HI5735C0fdBED7ZasD679w==", "", new Guid("1e24ab81-f2b0-4c2b-bc3a-4c8c73d83874"), "doctor1" },
                    { new Guid("daf60a66-994d-48ae-aa69-88171ef62066"), new DateTime(2026, 2, 23, 14, 13, 24, 373, DateTimeKind.Utc).AddTicks(8990), "", "", true, "AQAAAAIAAYagAAAAEDSEAXf6lm8Rp598qRwos94jm2UrXjDsTN+6NMU9pABu8o7GFZS0UgBo5NKMoe5pcA==", "", new Guid("1e24ab81-f2b0-4c2b-bc3a-4c8c73d83874"), "doctor3" }
                });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Code", "CreatedAt", "LicenseNumber", "Specialty", "Status", "UserId" },
                values: new object[] { new Guid("3ba442e6-9f9d-4224-98e8-172e378851ea"), "BS002", new DateTime(2026, 2, 23, 14, 13, 24, 412, DateTimeKind.Utc).AddTicks(5040), "LIC002", "Da liễu", 1, new Guid("59ab6848-1b08-469a-9870-eafb691e2f7a") });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Code", "CreatedAt", "LicenseNumber", "Specialty", "Status", "UserId" },
                values: new object[] { new Guid("7faed536-f799-4dcb-aa84-2d045890ff2a"), "BS001", new DateTime(2026, 2, 23, 14, 13, 24, 412, DateTimeKind.Utc).AddTicks(5030), "LIC001", "Nội tổng quát", 1, new Guid("b890920a-966a-40ca-845d-006ac6d9a558") });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Code", "CreatedAt", "LicenseNumber", "Specialty", "Status", "UserId" },
                values: new object[] { new Guid("b0c27b0b-e73f-422e-900e-d5e40b4de28e"), "BS003", new DateTime(2026, 2, 23, 14, 13, 24, 412, DateTimeKind.Utc).AddTicks(5050), "LIC003", "Tai mũi họng", 1, new Guid("daf60a66-994d-48ae-aa69-88171ef62066") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("3ba442e6-9f9d-4224-98e8-172e378851ea"));

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("7faed536-f799-4dcb-aa84-2d045890ff2a"));

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("b0c27b0b-e73f-422e-900e-d5e40b4de28e"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d52fb928-a510-43ac-ba22-64fa7130d709"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("f200a9e1-1c5c-426f-a28a-4a88fb3a6362"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55b18eb4-500c-496c-acc7-419c0ce53ce8"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4a8a9bf3-145a-4b0e-a8ef-0e10e53ad092"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("59ab6848-1b08-469a-9870-eafb691e2f7a"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("b890920a-966a-40ca-845d-006ac6d9a558"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("daf60a66-994d-48ae-aa69-88171ef62066"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("1e24ab81-f2b0-4c2b-bc3a-4c8c73d83874"));

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
    }
}
