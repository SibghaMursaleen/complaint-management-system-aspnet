using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIBA.ComplaintSystem.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 2, 16, 49, 37, 354, DateTimeKind.Local).AddTicks(5268));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 2, 16, 27, 19, 841, DateTimeKind.Local).AddTicks(775));
        }
    }
}
