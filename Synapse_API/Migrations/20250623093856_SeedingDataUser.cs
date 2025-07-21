using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Synapse_API.Migrations
{
    /// <inheritdoc />
    public partial class SeedingDataUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "Role", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2025, 6, 23, 9, 38, 56, 388, DateTimeKind.Utc).AddTicks(3393), "admin@example.com", "Administrator", true, "hashed_password_here", 1, new DateTime(2025, 6, 23, 9, 38, 56, 388, DateTimeKind.Utc).AddTicks(3394) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "UpdatedAt" },
                values: new object[] { 2, new DateTime(2025, 6, 23, 9, 38, 56, 388, DateTimeKind.Utc).AddTicks(3396), "student@example.com", "Student", true, "hashed_password_here", new DateTime(2025, 6, 23, 9, 38, 56, 388, DateTimeKind.Utc).AddTicks(3396) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 2);
        }
    }
}
