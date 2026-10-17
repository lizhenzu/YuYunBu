using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YunYunBu2025.Areas.Hanmadi.Data.Migrations
{
    /// <inheritdoc />
    public partial class HmParticipantIpAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "Participants",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "Participants");
        }
    }
}
