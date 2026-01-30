using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class AddUserFieldsAndRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a10f7142-1e47-4c52-ad68-a8433d011405"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("218eece9-8534-4b1a-b652-a7eb590c1bdb"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("48ace9ce-942e-42d6-8dd0-1afdc121a30f"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("48ace9ce-942e-42d6-8dd0-1afdc121a30f"), "Admin" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("a10f7142-1e47-4c52-ad68-a8433d011405"), "User" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "RoleId", "Username" },
                values: new object[] { new Guid("218eece9-8534-4b1a-b652-a7eb590c1bdb"), "AQAAAAEAACcQAAAAEL35VOW62wtrTpjybT7CvtkjatWgL3Hba/A57oNtu+ozlJa/u6jBFVeFQxzWH7GnMA==", new Guid("48ace9ce-942e-42d6-8dd0-1afdc121a30f"), "admin" });
        }
    }
}
