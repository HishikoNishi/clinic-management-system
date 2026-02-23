using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class SeedStaffUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    { new Guid("1a749cab-5344-41f4-aa93-061051e7019b"), "Doctor" },
                    { new Guid("5311f7a0-fdf1-4cac-895a-bb3f186b694d"), "Guest" },
                    { new Guid("845ff739-bead-48f2-a9ab-173b09f5e2f3"), "Staff" },
                    { new Guid("d0be8f2f-2100-4c9d-8649-dc598c29fbb8"), "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[,]
                {
                    { new Guid("34f376b3-9901-43e6-864e-3de6f566b90e"), new DateTime(2026, 2, 23, 14, 59, 54, 546, DateTimeKind.Utc).AddTicks(2700), "", "", true, "AQAAAAIAAYagAAAAELhdMv6MdphuEnG5hoRg5FjuR4AMSjUW/dIb1k0XGt5jKt26m+u3vozivCXSWK2mew==", "", new Guid("845ff739-bead-48f2-a9ab-173b09f5e2f3"), "staff1" },
                    { new Guid("42ca2a8b-5852-40e0-998b-42ae7e80fd6a"), new DateTime(2026, 2, 23, 14, 59, 54, 654, DateTimeKind.Utc).AddTicks(670), "", "", true, "AQAAAAIAAYagAAAAEGekuHdO1O+UfJq4zP6ePcGBsna9krzyRh2CenLH0DHef0hsIxDHPJOo7Y+jYVXlKg==", "", new Guid("1a749cab-5344-41f4-aa93-061051e7019b"), "doctor3" },
                    { new Guid("66f6e59e-9eb0-40bc-836f-a87fe2c86e7e"), new DateTime(2026, 2, 23, 14, 59, 54, 618, DateTimeKind.Utc).AddTicks(2150), "", "", true, "AQAAAAIAAYagAAAAEOypnQ1YbS3/IZxqzwncfd2+k5Zku5Og91dRDEytl32AbpJ5bA43Ooc+hBlMj3aXBw==", "", new Guid("1a749cab-5344-41f4-aa93-061051e7019b"), "doctor2" },
                    { new Guid("789dc153-6864-48a7-86b1-4d81848767ac"), new DateTime(2026, 2, 23, 14, 59, 54, 510, DateTimeKind.Utc).AddTicks(3240), "", "", true, "AQAAAAIAAYagAAAAEO7LBSg3aNRt9D5Xv6jVIpl/caeyOBseJ7mx5KCpvrOcFf2B3OV97nzYyAM6PGNaGw==", "", new Guid("d0be8f2f-2100-4c9d-8649-dc598c29fbb8"), "admin" },
                    { new Guid("d2568579-de44-4a0a-bfaf-b481d0eccfe8"), new DateTime(2026, 2, 23, 14, 59, 54, 582, DateTimeKind.Utc).AddTicks(2950), "", "", true, "AQAAAAIAAYagAAAAEOm5W5K13sFaYQYd5S8h4wWmJm4m4gOqPGoIHCgNeUWAPAabuHATAz7/eENSC+3gsA==", "", new Guid("1a749cab-5344-41f4-aa93-061051e7019b"), "doctor1" }
                });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Code", "CreatedAt", "LicenseNumber", "Specialty", "Status", "UserId" },
                values: new object[] { new Guid("02c66e35-b1ab-4c70-b832-36c7e16767e4"), "BS003", new DateTime(2026, 2, 23, 14, 59, 54, 689, DateTimeKind.Utc).AddTicks(9410), "LIC003", "Tai mũi họng", 1, new Guid("42ca2a8b-5852-40e0-998b-42ae7e80fd6a") });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Code", "CreatedAt", "LicenseNumber", "Specialty", "Status", "UserId" },
                values: new object[] { new Guid("50ef0ed6-74da-40d8-86a6-29d6a4a4f2e1"), "BS002", new DateTime(2026, 2, 23, 14, 59, 54, 689, DateTimeKind.Utc).AddTicks(9400), "LIC002", "Da liễu", 1, new Guid("66f6e59e-9eb0-40bc-836f-a87fe2c86e7e") });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Code", "CreatedAt", "LicenseNumber", "Specialty", "Status", "UserId" },
                values: new object[] { new Guid("da4555ba-01ef-4274-970e-1315e2bfde22"), "BS001", new DateTime(2026, 2, 23, 14, 59, 54, 689, DateTimeKind.Utc).AddTicks(9390), "LIC001", "Nội tổng quát", 1, new Guid("d2568579-de44-4a0a-bfaf-b481d0eccfe8") });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("02c66e35-b1ab-4c70-b832-36c7e16767e4"));

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("50ef0ed6-74da-40d8-86a6-29d6a4a4f2e1"));

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: new Guid("da4555ba-01ef-4274-970e-1315e2bfde22"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("5311f7a0-fdf1-4cac-895a-bb3f186b694d"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("34f376b3-9901-43e6-864e-3de6f566b90e"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("789dc153-6864-48a7-86b1-4d81848767ac"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("845ff739-bead-48f2-a9ab-173b09f5e2f3"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d0be8f2f-2100-4c9d-8649-dc598c29fbb8"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("42ca2a8b-5852-40e0-998b-42ae7e80fd6a"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("66f6e59e-9eb0-40bc-836f-a87fe2c86e7e"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("d2568579-de44-4a0a-bfaf-b481d0eccfe8"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("1a749cab-5344-41f4-aa93-061051e7019b"));

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
    }
}
