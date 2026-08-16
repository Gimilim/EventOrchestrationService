using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Уникальный идентификатор записи о полученном сообщении"),
                    EventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Уникальный идентификатор события (используется для дедупликации)"),
                    Topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Название топика Kafka, из которого получено сообщение"),
                    Payload = table.Column<string>(type: "jsonb", nullable: false, comment: "Тело сообщения в формате JSON"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время обработки сообщения")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_EventId_Topic",
                schema: "public",
                table: "InboxMessages",
                columns: new[] { "EventId", "Topic" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "public");
        }
    }
}
