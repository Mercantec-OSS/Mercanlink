using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddElectiveEnrollments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ElectiveEnrollments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ElectiveKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectiveEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectiveEnrollments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ElectiveEnrollments_ElectiveKey_UserId",
                table: "ElectiveEnrollments",
                columns: new[] { "ElectiveKey", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectiveEnrollments_UserId",
                table: "ElectiveEnrollments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ElectiveEnrollments");
        }
    }
}
