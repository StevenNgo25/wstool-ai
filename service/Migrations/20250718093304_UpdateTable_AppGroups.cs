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

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "AppSentMessages",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

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
                name: "Status",
                table: "AppInstances",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppGroups",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

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
                values: new object[] { new DateTime(2025, 7, 18, 16, 33, 4, 58, DateTimeKind.Local).AddTicks(7177), "$2a$11$YPpGg4hr7/pUYQO24mearOcWexXNDG.4kqFGCn4IT5Bo5OKw1ejoG" });

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 7, 18, 16, 33, 4, 180, DateTimeKind.Local).AddTicks(3006), "$2a$11$KTlEehOGMGfF6Iz2uRDHw.6EX3mlGd0.9xTebVPWB5f4AlqYAKz56" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "AppSentMessages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AppInstances");

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

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AppGroups",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

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
