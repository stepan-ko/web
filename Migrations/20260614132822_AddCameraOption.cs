using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace web.Migrations
{
    /// <inheritdoc />
    public partial class AddCameraOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Option_OptionId",
                table: "Cameras");

            migrationBuilder.DropTable(
                name: "Option");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_OptionId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "OptionId",
                table: "Cameras");

            migrationBuilder.CreateTable(
                name: "CameraOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CameraId = table.Column<int>(type: "integer", nullable: false),
                    MinWidth = table.Column<int>(type: "integer", nullable: false),
                    MaxWidth = table.Column<int>(type: "integer", nullable: false),
                    MinWeight = table.Column<float>(type: "real", nullable: false),
                    Tracking = table.Column<bool>(type: "boolean", nullable: false),
                    NumberFrameForLose = table.Column<int>(type: "integer", nullable: false),
                    UseArea = table.Column<bool>(type: "boolean", nullable: false),
                    AreaX = table.Column<int>(type: "integer", nullable: false),
                    AreaY = table.Column<int>(type: "integer", nullable: false),
                    AreaWidth = table.Column<int>(type: "integer", nullable: false),
                    AreaHeight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraOption_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CameraOption_CameraId",
                table: "CameraOption",
                column: "CameraId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CameraOption");

            migrationBuilder.AddColumn<int>(
                name: "OptionId",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Option",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AreaHeight = table.Column<int>(type: "integer", nullable: false),
                    AreaWidth = table.Column<int>(type: "integer", nullable: false),
                    AreaX = table.Column<int>(type: "integer", nullable: false),
                    AreaY = table.Column<int>(type: "integer", nullable: false),
                    MaxWidth = table.Column<int>(type: "integer", nullable: false),
                    MinWeight = table.Column<float>(type: "real", nullable: false),
                    MinWidth = table.Column<int>(type: "integer", nullable: false),
                    NumberFrameForLose = table.Column<int>(type: "integer", nullable: false),
                    Tracking = table.Column<bool>(type: "boolean", nullable: false),
                    UseArea = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Option", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_OptionId",
                table: "Cameras",
                column: "OptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Option_OptionId",
                table: "Cameras",
                column: "OptionId",
                principalTable: "Option",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
