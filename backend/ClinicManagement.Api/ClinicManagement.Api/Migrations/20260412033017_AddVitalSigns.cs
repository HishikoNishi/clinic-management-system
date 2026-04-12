using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class AddVitalSigns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 12, 3, 30, 17, 351, DateTimeKind.Utc).AddTicks(9779), "AQAAAAEAACcQAAAAEC2c+s+6IItAzqZhq9UWE6oKmU0gmOlnEexuF8QySGJ7CY1I9xPAdHhykXk9Ypxn3g==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 12, 3, 18, 49, 570, DateTimeKind.Utc).AddTicks(4651), "AQAAAAEAACcQAAAAEOZjqIXzhBTr9mHMFZ56URX74fsG0RFh9v0CtIV5jULOYlreUNM7IRVJ2IpvJ3co5A==" });
        }
    }
}
