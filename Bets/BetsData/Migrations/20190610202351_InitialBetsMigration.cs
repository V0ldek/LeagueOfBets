using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BetsData.Migrations
{
    public partial class InitialBetsMigration : Migration
    {
        private static readonly string ScriptUpName = $"{nameof(InitialBetsMigration)}_Up.sql";
        private static readonly string ScriptDownName = $"{nameof(InitialBetsMigration)}_Down.sql";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    IsFinished = table.Column<bool>(nullable: false, defaultValue: false),
                    WinningSide = table.Column<string>(nullable: true),
                    LosersScore = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Match", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stake",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MatchId = table.Column<int>(nullable: false),
                    WinningSide = table.Column<string>(nullable: false),
                    LosersScore = table.Column<long>(nullable: false),
                    Ratio = table.Column<float>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stake", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stake_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bet",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(maxLength: 256, nullable: false),
                    StakeId = table.Column<int>(nullable: false),
                    Amount = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bet_Stake_StakeId",
                        column: x => x.StakeId,
                        principalTable: "Stake",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bet_StakeId",
                table: "Bet",
                column: "StakeId");

            migrationBuilder.CreateIndex(
                name: "IX_Stake_MatchId",
                table: "Stake",
                column: "MatchId");

            migrationBuilder.RunSqlScript(ScriptUpName);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RunSqlScript(ScriptDownName);

            migrationBuilder.DropTable(
                name: "Bet");

            migrationBuilder.DropTable(
                name: "Stake");

            migrationBuilder.DropTable(
                name: "Match");
        }
    }
}
