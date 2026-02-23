using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagement.Api.Migrations
{
    public partial class UpdateAppointmentFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
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

            migrationBuilder.AlterColumn<Guid>(
                name: "DoctorId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorId",
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

            migrationBuilder.AlterColumn<Guid>(
                name: "DoctorId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
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
        }
    }
}
