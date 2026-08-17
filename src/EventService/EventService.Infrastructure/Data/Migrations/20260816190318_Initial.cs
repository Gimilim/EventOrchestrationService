using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventService.Infrastructure.Data.Migrations
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events",
                schema: "public");
        }
    }
}
