using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddKnowledgeSubmissionFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KnowledgeSubmissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LinkToPost = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DiscordId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true),
                    ModMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    PublishedMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    PublishedToDiscordAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeSubmissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeSubmissions_CreatedAt",
                table: "KnowledgeSubmissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeSubmissions_ModMessageId",
                table: "KnowledgeSubmissions",
                column: "ModMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeSubmissions_Status",
                table: "KnowledgeSubmissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeSubmissions_UserId",
                table: "KnowledgeSubmissions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnowledgeSubmissions");
        }
    }
}
