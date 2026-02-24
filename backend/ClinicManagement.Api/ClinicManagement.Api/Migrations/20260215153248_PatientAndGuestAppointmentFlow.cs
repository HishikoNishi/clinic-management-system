using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class PatientAndGuestAppointmentFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_PatientId",
                table: "Appointments");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("1a031fe4-1162-44e0-84e4-83b8ab5a9172"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b157ffc4-da35-42b0-bd98-434cd36a243b"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d253511f-3a46-4f78-b7a1-3498f98d38cd"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a9e111e1-2dd6-43c5-b934-df74b060798d"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("bfec63bf-b7f1-413c-b737-0d3b3aaaff63"));

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

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

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Patients");

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

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("1a031fe4-1162-44e0-84e4-83b8ab5a9172"), "Doctor" },
                    { new Guid("b157ffc4-da35-42b0-bd98-434cd36a243b"), "Guest" },
                    { new Guid("bfec63bf-b7f1-413c-b737-0d3b3aaaff63"), "Admin" },
                    { new Guid("d253511f-3a46-4f78-b7a1-3498f98d38cd"), "Staff" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("a9e111e1-2dd6-43c5-b934-df74b060798d"), new DateTime(2026, 2, 15, 14, 48, 50, 721, DateTimeKind.Utc).AddTicks(8570), "", "", true, "AQAAAAIAAYagAAAAEHbV8MLbxGY0+7nw8YDJDy8bB+l1A6DkmiCHtnLYpCyinHpDUUCNJx32dl3EtVUNdw==", "", new Guid("bfec63bf-b7f1-413c-b737-0d3b3aaaff63"), "admin" });

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
