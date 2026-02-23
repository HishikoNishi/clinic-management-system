using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class UpdateAppointmentStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_PatientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments");

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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Appointments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("413104c0-88be-48d5-922c-131396427f69"), "Guest" },
                    { new Guid("4dc97c4e-e005-40eb-992c-ec5bfa0f36cb"), "Staff" },
                    { new Guid("794f11a2-3ff5-44cc-93fa-1fbd25ca741a"), "Doctor" },
                    { new Guid("d2531c1e-e382-4c32-a973-42c037aeb031"), "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "RoleId", "Username" },
                values: new object[] { new Guid("4bc86cfa-8774-45fe-b16b-8583678bae9f"), new DateTime(2026, 2, 13, 16, 43, 11, 117, DateTimeKind.Utc).AddTicks(4610), "", "", true, "AQAAAAIAAYagAAAAEFZOYobMHAi66yFqcO/t6TBDsaEesOJYTOF7ZsQTJYRl4SwHppNu2rJUEyzmGupvHw==", "", new Guid("d2531c1e-e382-4c32-a973-42c037aeb031"), "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId_AppointmentDate_AppointmentTime",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDate", "AppointmentTime" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_PatientId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId_AppointmentDate_AppointmentTime",
                table: "Appointments");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("413104c0-88be-48d5-922c-131396427f69"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4dc97c4e-e005-40eb-992c-ec5bfa0f36cb"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("794f11a2-3ff5-44cc-93fa-1fbd25ca741a"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4bc86cfa-8774-45fe-b16b-8583678bae9f"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d2531c1e-e382-4c32-a973-42c037aeb031"));

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Appointments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
