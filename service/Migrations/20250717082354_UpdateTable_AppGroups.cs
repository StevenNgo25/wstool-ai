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
            migrationBuilder.DropIndex(
                name: "IX_AppInstances_WhatsAppNumber",
                table: "AppInstances");

            migrationBuilder.AlterColumn<string>(
                name: "TargetData",
                table: "AppJobs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WhatsAppNumber",
                table: "AppInstances",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

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
                values: new object[] { new DateTime(2025, 7, 17, 15, 23, 54, 38, DateTimeKind.Local).AddTicks(6362), "$2a$11$2Ieqjvd9epAfe0wQwi49MuwZJ8877LI/A3ZECP9S1RO6jD6ldhZJq" });

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 7, 17, 15, 23, 54, 225, DateTimeKind.Local).AddTicks(3236), "$2a$11$zKjizxyp1Wui6L.sZe3cJuVyTq3n5dXifSZT34GoRXvZB8wwO.qPW" });
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

            migrationBuilder.AlterColumn<string>(
                name: "WhatsAppNumber",
                table: "AppInstances",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
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

            migrationBuilder.CreateIndex(
                name: "IX_AppInstances_WhatsAppNumber",
                table: "AppInstances",
                column: "WhatsAppNumber",
                unique: true);
        }
    }
}
