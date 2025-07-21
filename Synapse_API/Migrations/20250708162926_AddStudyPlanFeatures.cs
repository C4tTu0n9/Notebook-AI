using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Synapse_API.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyPlanFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailableTimeSlots",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DailyStudyHours",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredStudyTime",
                table: "UserProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CourseID",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentEventID",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 8, 16, 29, 22, 890, DateTimeKind.Utc).AddTicks(2648), "$2a$11$tp/XWgvd4xVhLk5AhA3D0eJ85c5gVm70bnerCXjU0nV9Hd0XMzuvi", new DateTime(2025, 7, 8, 16, 29, 22, 890, DateTimeKind.Utc).AddTicks(2648) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 8, 16, 29, 22, 890, DateTimeKind.Utc).AddTicks(2651), "$2a$11$tp/XWgvd4xVhLk5AhA3D0eJ85c5gVm70bnerCXjU0nV9Hd0XMzuvi", new DateTime(2025, 7, 8, 16, 29, 22, 890, DateTimeKind.Utc).AddTicks(2651) });

            migrationBuilder.CreateIndex(
                name: "IX_Events_CourseID",
                table: "Events",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ParentEventID",
                table: "Events",
                column: "ParentEventID");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Courses_CourseID",
                table: "Events",
                column: "CourseID",
                principalTable: "Courses",
                principalColumn: "CourseID");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Events_ParentEventID",
                table: "Events",
                column: "ParentEventID",
                principalTable: "Events",
                principalColumn: "EventID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Courses_CourseID",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Events_ParentEventID",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_CourseID",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ParentEventID",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "AvailableTimeSlots",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DailyStudyHours",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredStudyTime",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CourseID",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ParentEventID",
                table: "Events");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 23, 9, 38, 56, 388, DateTimeKind.Utc).AddTicks(3393), "hashed_password_here", new DateTime(2025, 6, 23, 9, 38, 56, 388, DateTimeKind.Utc).AddTicks(3394) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 23, 9, 38, 56, 388, DateTimeKind.Utc).AddTicks(3396), "hashed_password_here", new DateTime(2025, 6, 23, 9, 38, 56, 388, DateTimeKind.Utc).AddTicks(3396) });
        }
    }
}
