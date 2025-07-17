using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppCampaignManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable_AppGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TargetData",
                table: "AppJobs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Participants",
                table: "AppGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 7, 17, 11, 0, 55, 588, DateTimeKind.Local).AddTicks(5948), "$2a$11$KiqQAV/oKOyrmjhYUN76uO0cQXH5XfWcvBHL3kGNRx9H/ecE.ItFS" });

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 7, 17, 11, 0, 55, 699, DateTimeKind.Local).AddTicks(901), "$2a$11$HN.MEEp8zY13ehZCNpicDeg983KYgJQuA/Zew2nuxecZ7xp1seOki" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Participants",
                table: "AppGroups");

            migrationBuilder.AlterColumn<string>(
                name: "TargetData",
                table: "AppJobs",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 7, 16, 11, 2, 21, 361, DateTimeKind.Utc).AddTicks(3704), "$2a$11$vjal.naaLBMQzFLxYD5BUuV0/PPJy2mh1E6F1RNiTNWqEY6l28Q56" });

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 7, 16, 11, 2, 21, 472, DateTimeKind.Utc).AddTicks(5881), "$2a$11$qMX7euQc8/aOu7fkvplGHOUsQjgJON2WK0XU0vM/lsaXGXAWCD7za" });
        }
    }
}
