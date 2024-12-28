using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MinimalApi_Test.Migrations
{
    /// <inheritdoc />
    public partial class mig_UserConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "DeletedAt", "FirstName", "IsDelete", "LastName", "ModifiedAt", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 12, 22, 6, 37, 23, 974, DateTimeKind.Utc).AddTicks(8895), null, "مدیر", false, "سیستم", null, "AQAAAAIAAYagAAAAELmh3PhOllrvnOiw7X5kGt+KhGnUi9w4WfLbWnUKJh1Xb50dog5SLeBdMLk9/B1ZNA==", "Admin", "admin@localhost.com" },
                    { 2, new DateTime(2024, 12, 22, 6, 37, 24, 21, DateTimeKind.Utc).AddTicks(9402), null, "کاربر", false, "عادی", null, "AQAAAAIAAYagAAAAEFfVc18ehBkM8Nfi9fgqd2eKFjsRDcVx+YS7uq4QApk40SntOMDWi3n/FiPeQwzE7Q==", "User", "user@localhost.com" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
