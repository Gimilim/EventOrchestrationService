using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventOrchestrationService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "ИД события")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Название события"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Описание события"),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата начала события"),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата окончания события"),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false, comment: "Общее количество мест на событие"),
                    AvailableSeats = table.Column<int>(type: "integer", nullable: false, comment: "Доступное количество мест на событие"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false, comment: "Версия строки для оптимистической блокировки")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "ИД бронирования")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false, comment: "ИД события"),
                    Status = table.Column<short>(type: "smallint", nullable: false, comment: "Статус бронирования:\r\n1 - В обработке (Pending)\r\n2 - Подтверждено (Confirmed)\r\n3 - Отклонено (Rejected)"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата создания бронирования"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Дата обработки события")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "public",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "Events",
                columns: new[] { "Id", "AvailableSeats", "Description", "EndAt", "StartAt", "Title", "TotalSeats" },
                values: new object[,]
                {
                    { 1, 10, "Description1", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Title1", 10 },
                    { 2, 10, "Description2", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Title2", 10 },
                    { 3, 10, "Description3", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Title3", 10 },
                    { 4, 10, "Description4", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), "ABC_Title4", 10 },
                    { 5, 10, "Description5", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), "abc_Title5", 10 },
                    { 6, 10, "Description6", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), "AbC_Title6", 10 },
                    { 7, 10, "Description7", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Title7", 10 },
                    { 8, 10, "Description8", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Title8", 10 },
                    { 9, 10, "Description9", new DateTime(2025, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Title9", 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventId",
                schema: "public",
                table: "Bookings",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "public");
        }
    }
}
