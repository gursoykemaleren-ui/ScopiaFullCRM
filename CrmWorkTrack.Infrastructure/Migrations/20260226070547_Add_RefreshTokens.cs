using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmWorkTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobActivities_Users_PerformedByUserId",
                table: "JobActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_JobComments_Users_CreatedByUserId",
                table: "JobComments");
            
            
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Jobs', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Jobs', 'Id') IS NOT NULL AND COL_LENGTH('dbo.Jobs', 'JobId') IS NULL
        EXEC sp_rename N'dbo.Jobs.Id', N'JobId', N'COLUMN';
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Customers', 'Id') IS NOT NULL AND COL_LENGTH('dbo.Customers', 'CustomerId') IS NULL
        EXEC sp_rename N'dbo.Customers.Id', N'CustomerId', N'COLUMN';
END
");



            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "JobComments",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "JobActivities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "MetaJson",
                table: "JobActivities",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "JobActivities",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshTokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TokenSalt = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenId);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_JobActivities_Users_PerformedByUserId",
                table: "JobActivities",
                column: "PerformedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobComments_Users_CreatedByUserId",
                table: "JobComments",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobActivities_Users_PerformedByUserId",
                table: "JobActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_JobComments_Users_CreatedByUserId",
                table: "JobComments");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Jobs', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Jobs', 'JobId') IS NOT NULL AND COL_LENGTH('dbo.Jobs', 'Id') IS NULL
        EXEC sp_rename N'dbo.Jobs.JobId', N'Id', N'COLUMN';
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.Customers', 'CustomerId') IS NOT NULL AND COL_LENGTH('dbo.Customers', 'Id') IS NULL
        EXEC sp_rename N'dbo.Customers.CustomerId', N'Id', N'COLUMN';
END
");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "JobComments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "JobActivities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "MetaJson",
                table: "JobActivities",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "JobActivities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JobActivities_Users_PerformedByUserId",
                table: "JobActivities",
                column: "PerformedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JobComments_Users_CreatedByUserId",
                table: "JobComments",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
