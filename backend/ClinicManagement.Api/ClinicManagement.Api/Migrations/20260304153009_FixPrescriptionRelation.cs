using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class FixPrescriptionRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 4, 15, 30, 9, 182, DateTimeKind.Utc).AddTicks(6486), "AQAAAAEAACcQAAAAEAXi2ALT2tSqvwljcjLFLVlQg+rxJ+F98iBX7js13MU6X70+al7aaqEGTRMYaY2vLw==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 3, 4, 13, 44, 2, 44, DateTimeKind.Utc).AddTicks(5929), "AQAAAAEAACcQAAAAEOW+QMG3YfJljpp4GtMAvH3CeKJK36DeGs/ESeSMPHonfn31WLUxhqjiaE0yZ3c/HA==" });
        }
    }
}
