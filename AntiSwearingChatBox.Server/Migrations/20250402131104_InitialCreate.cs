using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AntiSwearingChatBox.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatThreads",
                columns: table => new
                {
                    ThreadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    AllowAnonymous = table.Column<bool>(type: "bit", nullable: false),
                    ModerationEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxParticipants = table.Column<int>(type: "int", nullable: true),
                    AutoDeleteAfterDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChatThre__688356845E42936A", x => x.ThreadId);
                });

            migrationBuilder.CreateTable(
                name: "FilteredWords",
                columns: table => new
                {
                    WordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Word = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SeverityLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Filtered__2C20F06639322A22", x => x.WordId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    VerificationToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResetToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    TokenExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "User"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrustScore = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 1.00m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4C037765CB", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "MessageHistory",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThreadId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OriginalMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModeratedMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WasModified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MessageH__C87C0C9CDC014970", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_MessageHistory_ChatThreads",
                        column: x => x.ThreadId,
                        principalTable: "ChatThreads",
                        principalColumn: "ThreadId");
                    table.ForeignKey(
                        name: "FK_MessageHistory_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ThreadParticipants",
                columns: table => new
                {
                    ThreadId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadParticipants", x => new { x.ThreadId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ThreadParticipants_ChatThreads",
                        column: x => x.ThreadId,
                        principalTable: "ChatThreads",
                        principalColumn: "ThreadId");
                    table.ForeignKey(
                        name: "FK_ThreadParticipants_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserWarnings",
                columns: table => new
                {
                    WarningId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ThreadId = table.Column<int>(type: "int", nullable: false),
                    WarningMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserWarn__21457158A1C6A6EF", x => x.WarningId);
                    table.ForeignKey(
                        name: "FK_UserWarnings_ChatThreads",
                        column: x => x.ThreadId,
                        principalTable: "ChatThreads",
                        principalColumn: "ThreadId");
                    table.ForeignKey(
                        name: "FK_UserWarnings_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Filtered__95B501084AE8F06F",
                table: "FilteredWords",
                column: "Word",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageHistory_ThreadId",
                table: "MessageHistory",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageHistory_UserId",
                table: "MessageHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadParticipants_ThreadId",
                table: "ThreadParticipants",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadParticipants_UserId",
                table: "ThreadParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__536C85E4D8DA359D",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D105343506988F",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWarnings_ThreadId",
                table: "UserWarnings",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWarnings_UserId",
                table: "UserWarnings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilteredWords");

            migrationBuilder.DropTable(
                name: "MessageHistory");

            migrationBuilder.DropTable(
                name: "ThreadParticipants");

            migrationBuilder.DropTable(
                name: "UserWarnings");

            migrationBuilder.DropTable(
                name: "ChatThreads");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
