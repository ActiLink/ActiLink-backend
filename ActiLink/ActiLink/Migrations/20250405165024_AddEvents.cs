using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActiLink.Migrations
{
    /// <inheritdoc />
    public partial class AddEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_AspNetUsers_OrganizerId1",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_OrganizerId1",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "OrganizerId1",
                table: "Events");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Events",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Events",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Events");

            migrationBuilder.AddColumn<int>(
                name: "Location_Latitude",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Location_Longitude",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OrganizerId1",
                table: "Events",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganizerId1",
                table: "Events",
                column: "OrganizerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AspNetUsers_OrganizerId1",
                table: "Events",
                column: "OrganizerId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
