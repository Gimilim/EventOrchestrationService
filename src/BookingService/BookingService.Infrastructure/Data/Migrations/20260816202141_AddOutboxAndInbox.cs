using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxAndInbox : Migration
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

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Уникальный идентификатор сообщения"),
                    Topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Название топика Kafka, куда должно быть отправлено сообщение"),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Ключ сообщения для Kafka (используется для партиционирования)"),
                    Payload = table.Column<string>(type: "jsonb", nullable: false, comment: "Тело сообщения в формате JSON"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время создания сообщения"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата и время успешной отправки сообщения в Kafka"),
                    Attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Количество попыток отправки сообщения")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_EventId_Topic",
                schema: "public",
                table: "InboxMessages",
                columns: new[] { "EventId", "Topic" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt",
                schema: "public",
                table: "OutboxMessages",
                column: "ProcessedAt",
                filter: "\"ProcessedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "public");
        }
    }
}
