using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web.Migrations
{
    /// <inheritdoc />
    public partial class ChangOptionCamera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinWeight",
                table: "CameraOptions");

            migrationBuilder.RenameColumn(
                name: "MinWidth",
                table: "CameraOptions",
                newName: "MinPlateWidth");

            migrationBuilder.RenameColumn(
                name: "MaxWidth",
                table: "CameraOptions",
                newName: "MaxPlateWidth");

            migrationBuilder.AddColumn<double>(
                name: "MinProbability",
                table: "CameraOptions",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinProbability",
                table: "CameraOptions");

            migrationBuilder.RenameColumn(
                name: "MinPlateWidth",
                table: "CameraOptions",
                newName: "MinWidth");

            migrationBuilder.RenameColumn(
                name: "MaxPlateWidth",
                table: "CameraOptions",
                newName: "MaxWidth");

            migrationBuilder.AddColumn<float>(
                name: "MinWeight",
                table: "CameraOptions",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
