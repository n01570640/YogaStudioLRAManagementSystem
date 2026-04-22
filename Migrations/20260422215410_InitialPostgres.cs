using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace YogaStudioLRAManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YS_LEAVE_TYPES",
                columns: table => new
                {
                    LEAVE_TYPE_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LEAVE_NAME = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IS_PAID = table.Column<bool>(type: "boolean", nullable: false),
                    AFFECTS_BALANCE = table.Column<bool>(type: "boolean", nullable: false),
                    MIN_DAYS = table.Column<int>(type: "integer", nullable: false),
                    MAX_DAYS = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YS_LEAVE_TYPES", x => x.LEAVE_TYPE_ID);
                });

            migrationBuilder.CreateTable(
                name: "YS_STUDIO_ROLES",
                columns: table => new
                {
                    STUDIO_ROLE_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ROLE_NAME = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    REQUIRES_CERT = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YS_STUDIO_ROLES", x => x.STUDIO_ROLE_ID);
                });

            migrationBuilder.CreateTable(
                name: "YS_EMPLOYEES",
                columns: table => new
                {
                    EMPLOYEE_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FIRST_NAME = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LAST_NAME = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HIRE_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CERTIFICATION = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    VACATION_BALANCE = table.Column<int>(type: "integer", nullable: false),
                    SICK_LEAVE_BALANCE = table.Column<int>(type: "integer", nullable: false),
                    STUDIO_ROLE_ID = table.Column<int>(type: "integer", nullable: false)
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
                    ATTENDANCE_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EMPLOYEE_ID = table.Column<int>(type: "integer", nullable: false),
                    ATTENDANCE_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CLOCK_IN = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CLOCK_OUT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TOTAL_HOURS = table.Column<double>(type: "double precision", nullable: true),
                    STATUS = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
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
                    REQUEST_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    START_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    END_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    REQUEST_STATUS = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    REASON = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EMPLOYEE_ID = table.Column<int>(type: "integer", nullable: false),
                    LEAVE_TYPE_ID = table.Column<int>(type: "integer", nullable: false)
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
                    USER_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    USER_NAME = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HASH_PASSWORD = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MUST_CHANGE_PASSWORD = table.Column<bool>(type: "boolean", nullable: false),
                    EMAIL_ID = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PROFILE_ROLE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EMPLOYEE_ID = table.Column<int>(type: "integer", nullable: false),
                    IS_ACTIVE = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.InsertData(
                table: "YS_EMPLOYEES",
                columns: new[] { "EMPLOYEE_ID", "CERTIFICATION", "FIRST_NAME", "HIRE_DATE", "LAST_NAME", "SICK_LEAVE_BALANCE", "STUDIO_ROLE_ID", "VACATION_BALANCE" },
                values: new object[] { 999, null, "Studio", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Owner", 0, 1, 0 });

            migrationBuilder.InsertData(
                table: "YS_USERS",
                columns: new[] { "USER_ID", "EMAIL_ID", "EMPLOYEE_ID", "IS_ACTIVE", "MUST_CHANGE_PASSWORD", "HASH_PASSWORD", "PROFILE_ROLE", "USER_NAME" },
                values: new object[] { 999, "admin@yogastudio.com", 999, true, false, "AQAAAAIAAYagAAAAEOCJGv2O55/MXqWHaloEU4JuFZUKXj7p5WTurrv4qfw5gEZ2wE1w8NUKQBCDwdB6tA==", "Admin", "admin" });

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
