using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FolderCleaner.Worker.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CleaningRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(nullable: false),
                    RunsAt = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    Repeat = table.Column<bool>(nullable: false),
                    RepeatRange = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleaningRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CleaningHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    RunsAt = table.Column<DateTime>(nullable: false),
                    CleaningStatus = table.Column<int>(nullable: false),
                    CleaningStatusDescription = table.Column<string>(nullable: true),
                    TriggeredBy = table.Column<int>(nullable: false),
                    TotalFiles = table.Column<int>(nullable: true),
                    CleaningSize = table.Column<long>(nullable: true),
                    CleaningRecordId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleaningHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CleaningHistory_CleaningRecords_CleaningRecordId",
                        column: x => x.CleaningRecordId,
                        principalTable: "CleaningRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CleaningHistory_CleaningRecordId",
                table: "CleaningHistory",
                column: "CleaningRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CleaningHistory");

            migrationBuilder.DropTable(
                name: "CleaningRecords");
        }
    }
}
