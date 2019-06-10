using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MatchesData.Migrations
{
    public partial class InitialMatchesMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Match",
                table => new
                {
                    Id = table.Column<int>(nullable: false),
                    StartDateTime = table.Column<DateTime>(nullable: false),
                    Format = table.Column<string>(nullable: false),
                    BlueScore = table.Column<int>(nullable: false, defaultValue: 0),
                    RedScore = table.Column<int>(nullable: false, defaultValue: 0),
                    IsFinished = table.Column<bool>(nullable: false, defaultValue: false)
                },
                constraints: table => { table.PrimaryKey("PK_Match", x => x.Id); });

            migrationBuilder.CreateTable(
                "Team",
                table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    LogoUrl = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Team", x => x.Id); });

            migrationBuilder.CreateTable(
                "MatchParticipation",
                table => new
                {
                    MatchId = table.Column<int>(nullable: false),
                    Side = table.Column<string>(nullable: false),
                    TeamId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchParticipation", x => new {x.MatchId, x.Side});
                    table.ForeignKey(
                        "FK_MatchParticipation_Match_MatchId",
                        x => x.MatchId,
                        "Match",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_MatchParticipation_Team_TeamId",
                        x => x.TeamId,
                        "Team",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_MatchParticipation_TeamId",
                "MatchParticipation",
                "TeamId");

            migrationBuilder.CreateIndex(
                "IX_MatchParticipation_MatchId_TeamId",
                "MatchParticipation",
                new[] {"MatchId", "TeamId"},
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_Team_Name",
                "Team",
                "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "MatchParticipation");

            migrationBuilder.DropTable(
                "Match");

            migrationBuilder.DropTable(
                "Team");
        }
    }
}