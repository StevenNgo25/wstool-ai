using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhatsAppCampaignManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTables_MessageAndJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "AppMessages");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "AppMessages");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppJobs");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AppJobs");

            migrationBuilder.AlterColumn<int>(
                name: "InstanceId",
                table: "AppMessages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "InstanceId",
                table: "AppMessages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                table: "AppMessages",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "AppMessages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppJobs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AppJobs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 7, 13, 11, 7, 42, 996, DateTimeKind.Utc).AddTicks(900), "$2a$11$DiRIhrfFwOnJRnq878pAUennOozFw8606./yNhbSbPFgdO4qqPTru" });

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2025, 7, 13, 11, 7, 43, 175, DateTimeKind.Utc).AddTicks(8837), "$2a$11$yTqkbsn2uHwza38xzoOlduES9R9vIsQ8KINeIhLHalBogk/WCLlQ." });
        }
    }
}
