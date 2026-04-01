using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class AddAvatarUrlToStaff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Staffs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 1, 15, 57, 18, 751, DateTimeKind.Utc).AddTicks(5720), "AQAAAAIAAYagAAAAEOE3ufMTc7wPCElKSfMOc+mr9fHQtwbsyIgulT2ZfDsdsi7t7Faf/nq3Ds0eweimlQ==" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Staffs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 4, 1, 15, 28, 37, 122, DateTimeKind.Utc).AddTicks(3360), "AQAAAAIAAYagAAAAEAsSAgWYoqqRhU/t9IuFRbDLDmijvBzCkSNlgQUf8rEUH9WVV8FatJokK9VkHs7tCA==" });
        }
    }
}
