using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace web.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeLostTimeDetectToOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeDetect",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeLost",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RecognizeTracks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CameraId = table.Column<int>(type: "integer", nullable: false),
                    PlateNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BestProbability = table.Column<double>(type: "double precision", nullable: false),
                    BestImagePath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecognizeTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecognizeTracks_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecognizeTracks_CameraId",
                table: "RecognizeTracks",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_RecognizeTracks_FirstSeen",
                table: "RecognizeTracks",
                column: "FirstSeen");

            migrationBuilder.CreateIndex(
                name: "IX_RecognizeTracks_LeftAt",
                table: "RecognizeTracks",
                column: "LeftAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecognizeTracks_PlateNumber",
                table: "RecognizeTracks",
                column: "PlateNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecognizeTracks");

            migrationBuilder.DropColumn(
                name: "TimeDetect",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "TimeLost",
                table: "Cameras");
        }
    }
}
