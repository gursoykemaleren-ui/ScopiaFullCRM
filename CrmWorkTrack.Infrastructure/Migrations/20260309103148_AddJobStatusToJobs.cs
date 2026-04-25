using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmWorkTrack.Infrastructure.Migrations
{
    public partial class AddJobStatusToJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(@"
                UPDATE Jobs
                SET Status = CASE
                    WHEN IsCompleted = 1 THEN 3
                    ELSE 1
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Jobs");
        }
    }
}
