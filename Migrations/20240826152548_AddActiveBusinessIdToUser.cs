using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMS_API.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveBusinessIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActiveBusinessId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveBusinessId",
                table: "AspNetUsers");
        }
    }
}
