using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YogaStudioLRAManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMustChangePasswordToUsersAndSeedDataToUserEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MUST_CHANGE_PASSWORD",
                table: "YS_USERS",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "YS_EMPLOYEES",
                columns: new[] { "EMPLOYEE_ID", "CERTIFICATION", "FIRST_NAME", "HIRE_DATE", "LAST_NAME", "SICK_LEAVE_BALANCE", "STUDIO_ROLE_ID", "VACATION_BALANCE" },
                values: new object[] { 999, null, "Studio", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Owner", 0, 1, 0 });

            migrationBuilder.InsertData(
                table: "YS_USERS",
                columns: new[] { "USER_ID", "EMAIL_ID", "EMPLOYEE_ID", "MUST_CHANGE_PASSWORD", "HASH_PASSWORD", "PROFILE_ROLE", "USER_NAME" },
                values: new object[] { 999, "admin@yogastudio.com", 999, false, "AQAAAAIAAYagAAAAELuKNL/FFk+4QEBFAMVlIVdLgHU48pX2ZPOfNiNOjEoIyztm+HqID88zM/e4xyeDjw==", "Admin", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "YS_USERS",
                keyColumn: "USER_ID",
                keyValue: 999);

            migrationBuilder.DeleteData(
                table: "YS_EMPLOYEES",
                keyColumn: "EMPLOYEE_ID",
                keyValue: 999);

            migrationBuilder.DropColumn(
                name: "MUST_CHANGE_PASSWORD",
                table: "YS_USERS");
        }
    }
}
