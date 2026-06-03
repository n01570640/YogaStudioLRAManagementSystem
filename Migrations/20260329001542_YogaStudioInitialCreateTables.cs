using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace YogaStudioLRAManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class YogaStudioInitialCreateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YS_LEAVE_TYPES",
                columns: table => new
                {
                    LEAVE_TYPE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    LEAVE_NAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    IS_PAID = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    AFFECTS_BALANCE = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    MIN_DAYS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MAX_DAYS = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YS_LEAVE_TYPES", x => x.LEAVE_TYPE_ID);
                });

            migrationBuilder.CreateTable(
                name: "YS_STUDIO_ROLES",
                columns: table => new
                {
                    STUDIO_ROLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ROLE_NAME = table.Column<string>(type: "NVARCHAR2(15)", maxLength: 15, nullable: false),
                    REQUIRES_CERT = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YS_STUDIO_ROLES", x => x.STUDIO_ROLE_ID);
                });

            migrationBuilder.CreateTable(
                name: "YS_EMPLOYEES",
                columns: table => new
                {
                    EMPLOYEE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FIRST_NAME = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    LAST_NAME = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                    HIRE_DATE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CERTIFICATION = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    VACATION_BALANCE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    SICK_LEAVE_BALANCE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    STUDIO_ROLE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YS_EMPLOYEES", x => x.EMPLOYEE_ID);
                    table.ForeignKey(
                        name: "FK_YS_EMPLOYEES_YS_STUDIO_ROLES_STUDIO_ROLE_ID",
                        column: x => x.STUDIO_ROLE_ID,
                        principalTable: "YS_STUDIO_ROLES",
                        principalColumn: "STUDIO_ROLE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YS_ATTENDANCES",
                columns: table => new
                {
                    ATTENDANCE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    EMPLOYEE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DATE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CLOCK_IN = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CLOCK_OUT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    TOTAL_HOURS = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    STATUS = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YS_ATTENDANCES", x => x.ATTENDANCE_ID);
                    table.ForeignKey(
                        name: "FK_YS_ATTENDANCES_YS_EMPLOYEES_EMPLOYEE_ID",
                        column: x => x.EMPLOYEE_ID,
                        principalTable: "YS_EMPLOYEES",
                        principalColumn: "EMPLOYEE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YS_LEAVE_REQUESTS",
                columns: table => new
                {
                    REQUEST_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    START_DATE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    END_DATE = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    REQUEST_STATUS = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    REASON = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    EMPLOYEE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    LEAVE_TYPE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YS_LEAVE_REQUESTS", x => x.REQUEST_ID);
                    table.ForeignKey(
                        name: "FK_YS_LEAVE_REQUESTS_YS_EMPLOYEES_EMPLOYEE_ID",
                        column: x => x.EMPLOYEE_ID,
                        principalTable: "YS_EMPLOYEES",
                        principalColumn: "EMPLOYEE_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YS_LEAVE_REQUESTS_YS_LEAVE_TYPES_LEAVE_TYPE_ID",
                        column: x => x.LEAVE_TYPE_ID,
                        principalTable: "YS_LEAVE_TYPES",
                        principalColumn: "LEAVE_TYPE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YS_USERS",
                columns: table => new
                {
                    USER_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    USER_NAME = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    HASH_PASSWORD = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    EMAIL_ID = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PROFILE_ROLE = table.Column<string>(type: "NVARCHAR2(10)", maxLength: 10, nullable: false),
                    EMPLOYEE_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YS_USERS", x => x.USER_ID);
                    table.ForeignKey(
                        name: "FK_YS_USERS_YS_EMPLOYEES_EMPLOYEE_ID",
                        column: x => x.EMPLOYEE_ID,
                        principalTable: "YS_EMPLOYEES",
                        principalColumn: "EMPLOYEE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "YS_LEAVE_TYPES",
                columns: new[] { "LEAVE_TYPE_ID", "AFFECTS_BALANCE", "IS_PAID", "MAX_DAYS", "MIN_DAYS", "LEAVE_NAME" },
                values: new object[,]
                {
                    { 1, true, true, 14, 1, "Vacation" },
                    { 2, true, true, 3, 1, "Sick - Paid" },
                    { 3, false, false, 30, 1, "Sick - Unpaid" },
                    { 4, false, true, 365, 1, "Parental Leave" },
                    { 5, false, true, 5, 1, "Bereavement" },
                    { 6, false, false, 7, 1, "Certification" }
                });

            migrationBuilder.InsertData(
                table: "YS_STUDIO_ROLES",
                columns: new[] { "STUDIO_ROLE_ID", "REQUIRES_CERT", "ROLE_NAME" },
                values: new object[,]
                {
                    { 1, true, "Instructor" },
                    { 2, false, "Receptionist" },
                    { 3, false, "Cleaner" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_YS_ATTENDANCES_EMPLOYEE_ID",
                table: "YS_ATTENDANCES",
                column: "EMPLOYEE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_YS_EMPLOYEES_STUDIO_ROLE_ID",
                table: "YS_EMPLOYEES",
                column: "STUDIO_ROLE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_YS_LEAVE_REQUESTS_EMPLOYEE_ID",
                table: "YS_LEAVE_REQUESTS",
                column: "EMPLOYEE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_YS_LEAVE_REQUESTS_LEAVE_TYPE_ID",
                table: "YS_LEAVE_REQUESTS",
                column: "LEAVE_TYPE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_YS_USERS_EMPLOYEE_ID",
                table: "YS_USERS",
                column: "EMPLOYEE_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YS_ATTENDANCES");

            migrationBuilder.DropTable(
                name: "YS_LEAVE_REQUESTS");

            migrationBuilder.DropTable(
                name: "YS_USERS");

            migrationBuilder.DropTable(
                name: "YS_LEAVE_TYPES");

            migrationBuilder.DropTable(
                name: "YS_EMPLOYEES");

            migrationBuilder.DropTable(
                name: "YS_STUDIO_ROLES");
        }
    }
}
