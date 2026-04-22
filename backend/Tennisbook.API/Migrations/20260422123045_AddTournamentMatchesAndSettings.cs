using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Tennisbook.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentMatchesAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DrawGenerated",
                table: "Tournaments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Format",
                table: "Tournaments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SetsToWin",
                table: "Tournaments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Surface",
                table: "Tournaments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TournamentMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TournamentId = table.Column<int>(type: "integer", nullable: false),
                    Round = table.Column<int>(type: "integer", nullable: false),
                    MatchNumber = table.Column<int>(type: "integer", nullable: false),
                    Player1Id = table.Column<int>(type: "integer", nullable: true),
                    Player2Id = table.Column<int>(type: "integer", nullable: true),
                    WinnerId = table.Column<int>(type: "integer", nullable: true),
                    NextMatchId = table.Column<int>(type: "integer", nullable: true),
                    Score = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Player1Set1 = table.Column<int>(type: "integer", nullable: false),
                    Player2Set1 = table.Column<int>(type: "integer", nullable: false),
                    Player1Set2 = table.Column<int>(type: "integer", nullable: false),
                    Player2Set2 = table.Column<int>(type: "integer", nullable: false),
                    Player1Set3 = table.Column<int>(type: "integer", nullable: false),
                    Player2Set3 = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CourtName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_Players_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_Players_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_Players_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_TournamentMatches_NextMatchId",
                        column: x => x.NextMatchId,
                        principalTable: "TournamentMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_NextMatchId",
                table: "TournamentMatches",
                column: "NextMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_Player1Id",
                table: "TournamentMatches",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_Player2Id",
                table: "TournamentMatches",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_TournamentId",
                table: "TournamentMatches",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_WinnerId",
                table: "TournamentMatches",
                column: "WinnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentMatches");

            migrationBuilder.DropColumn(
                name: "DrawGenerated",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Format",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "SetsToWin",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Surface",
                table: "Tournaments");
        }
    }
}
