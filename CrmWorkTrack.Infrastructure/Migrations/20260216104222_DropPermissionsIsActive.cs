using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmWorkTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropPermissionsIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("IsActive", "Permissions");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
