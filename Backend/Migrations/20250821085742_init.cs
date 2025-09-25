using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    DiscordId = table.Column<string>(type: "text", nullable: true),
                    GlobalName = table.Column<string>(type: "text", nullable: true),
                    Discriminator = table.Column<string>(type: "text", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    Nickname = table.Column<string>(type: "text", nullable: true),
                    IsBot = table.Column<bool>(type: "boolean", nullable: true),
                    PublicFlags = table.Column<int>(type: "integer", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsBoosting = table.Column<bool>(type: "boolean", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordVerifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DiscordUserId = table.Column<string>(type: "text", nullable: false),
                    DiscordId = table.Column<string>(type: "text", nullable: false),
                    VerificationCode = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolADUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    StudentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolADUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DiscordUserId = table.Column<string>(type: "text", nullable: false),
                    ActivityType = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    XPAwarded = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDailyActivities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DiscordUserId = table.Column<string>(type: "text", nullable: false),
                    ActivityType = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    TotalXPAwarded = table.Column<int>(type: "integer", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebsiteUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    WebsiteUserId = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_WebsiteUsers_WebsiteUserId",
                        column: x => x.WebsiteUserId,
                        principalTable: "WebsiteUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    DiscordUserId = table.Column<string>(type: "text", nullable: false),
                    WebsiteUserId = table.Column<string>(type: "text", nullable: false),
                    SchoolADUserId = table.Column<string>(type: "text", nullable: false),
                    Roles = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_DiscordUsers_DiscordUserId",
                        column: x => x.DiscordUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_SchoolADUsers_SchoolADUserId",
                        column: x => x.SchoolADUserId,
                        principalTable: "SchoolADUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_WebsiteUsers_WebsiteUserId",
                        column: x => x.WebsiteUserId,
                        principalTable: "WebsiteUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUsers_DiscordId",
                table: "DiscordUsers",
                column: "DiscordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordVerifications_DiscordUserId_DiscordId",
                table: "DiscordVerifications",
                columns: new[] { "DiscordUserId", "DiscordId" });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordVerifications_VerificationCode",
                table: "DiscordVerifications",
                column: "VerificationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_WebsiteUserId",
                table: "RefreshTokens",
                column: "WebsiteUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DiscordUserId",
                table: "Users",
                column: "DiscordUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SchoolADUserId",
                table: "Users",
                column: "SchoolADUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true,
                filter: "\"UserName\" != ''");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WebsiteUserId",
                table: "Users",
                column: "WebsiteUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteUsers_Email",
                table: "WebsiteUsers",
                column: "Email",
                unique: true,
                filter: "\"Email\" != ''");

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteUsers_UserName",
                table: "WebsiteUsers",
                column: "UserName",
                unique: true,
                filter: "\"UserName\" != ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordVerifications");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "UserDailyActivities");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DiscordUsers");

            migrationBuilder.DropTable(
                name: "SchoolADUsers");

            migrationBuilder.DropTable(
                name: "WebsiteUsers");
        }
    }
}
