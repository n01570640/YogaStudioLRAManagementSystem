using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YogaStudioLRAManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IS_ACTIVE",
                table: "YS_USERS",
                type: "NUMBER(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "YS_USERS",
                keyColumn: "USER_ID",
                keyValue: 999,
                columns: new[] { "IS_ACTIVE", "HASH_PASSWORD" },
                values: new object[] { true, "AQAAAAIAAYagAAAAECnwM22fuVdUcdvOZk2zvO9t7ZQWp9+gnE4Q273VegdJuycMa9iWiphf6TyePThp0w==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IS_ACTIVE",
                table: "YS_USERS");

            migrationBuilder.UpdateData(
                table: "YS_USERS",
                keyColumn: "USER_ID",
                keyValue: 999,
                column: "HASH_PASSWORD",
                value: "AQAAAAIAAYagAAAAELuKNL/FFk+4QEBFAMVlIVdLgHU48pX2ZPOfNiNOjEoIyztm+HqID88zM/e4xyeDjw==");
        }
    }
}
