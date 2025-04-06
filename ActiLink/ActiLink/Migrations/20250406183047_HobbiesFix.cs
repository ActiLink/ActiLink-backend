using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ActiLink.Migrations
{
    /// <inheritdoc />
    public partial class HobbiesFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hobbies_AspNetUsers_UserId",
                table: "Hobbies");

            migrationBuilder.DropForeignKey(
                name: "FK_Hobbies_Events_EventId",
                table: "Hobbies");

            migrationBuilder.DropIndex(
                name: "IX_Hobbies_EventId",
                table: "Hobbies");

            migrationBuilder.DropIndex(
                name: "IX_Hobbies_UserId",
                table: "Hobbies");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Hobbies");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Hobbies");

            migrationBuilder.CreateTable(
                name: "EventHobby",
                columns: table => new
                {
                    EventsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedHobbiesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventHobby", x => new { x.EventsId, x.RelatedHobbiesId });
                    table.ForeignKey(
                        name: "FK_EventHobby_Events_EventsId",
                        column: x => x.EventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventHobby_Hobbies_RelatedHobbiesId",
                        column: x => x.RelatedHobbiesId,
                        principalTable: "Hobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HobbyUser",
                columns: table => new
                {
                    HobbiesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HobbyUser", x => new { x.HobbiesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_HobbyUser_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HobbyUser_Hobbies_HobbiesId",
                        column: x => x.HobbiesId,
                        principalTable: "Hobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventHobby_RelatedHobbiesId",
                table: "EventHobby",
                column: "RelatedHobbiesId");

            migrationBuilder.CreateIndex(
                name: "IX_HobbyUser_UsersId",
                table: "HobbyUser",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventHobby");

            migrationBuilder.DropTable(
                name: "HobbyUser");

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "Hobbies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Hobbies",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hobbies_EventId",
                table: "Hobbies",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Hobbies_UserId",
                table: "Hobbies",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hobbies_AspNetUsers_UserId",
                table: "Hobbies",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Hobbies_Events_EventId",
                table: "Hobbies",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");
        }
    }
}
