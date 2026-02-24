using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class AddAppointmentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppointmentTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("769158f9-bdfb-4d79-9186-9bfec6f1baa2"), "Staff" },
                    { new Guid("9876c534-be25-4839-95e9-6a974d3716a9"), "Doctor" },
                    { new Guid("99efc746-a8e9-4c48-8119-8234f6815ffc"), "Guest" },
                    { new Guid("c6be2a54-9a5d-4a99-82d0-8e8b3611c83b"), "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("378fca30-ac72-4872-8ee6-97db47fa787e"), new DateTime(2026, 2, 13, 16, 3, 44, 523, DateTimeKind.Utc).AddTicks(5920), "", "", true, "AQAAAAIAAYagAAAAEHjZw6dEpb7sHUG7slc1xAsGxGiw6VTcAfdExabl6fWIw1HYPygJHygSRGAnLHChUQ==", "", new Guid("c6be2a54-9a5d-4a99-82d0-8e8b3611c83b"), "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("769158f9-bdfb-4d79-9186-9bfec6f1baa2"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("9876c534-be25-4839-95e9-6a974d3716a9"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("99efc746-a8e9-4c48-8119-8234f6815ffc"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("378fca30-ac72-4872-8ee6-97db47fa787e"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c6be2a54-9a5d-4a99-82d0-8e8b3611c83b"));

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
    }
}
