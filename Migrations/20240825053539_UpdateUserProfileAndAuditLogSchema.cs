using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMS_API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserProfileAndAuditLogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_UserProfiles_UserId",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AuditLogs",
                newName: "UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_UserProfiles_UserProfileId",
                table: "AuditLogs",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_UserProfiles_UserProfileId",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                table: "AuditLogs",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_UserProfileId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_UserProfiles_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
