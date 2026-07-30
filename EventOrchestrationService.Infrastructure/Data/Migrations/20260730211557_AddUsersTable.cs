using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventOrchestrationService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Status",
                schema: "public",
                table: "Bookings",
                type: "smallint",
                nullable: false,
                comment: "Статус бронирования:\r\n1 - В обработке (Pending)\r\n2 - Подтверждено (Confirmed)\r\n3 - Отклонено (Rejected)\r\n3 - Отменено (Cancelled)",
                oldClrType: typeof(short),
                oldType: "smallint",
                oldComment: "Статус бронирования:\r\n1 - В обработке (Pending)\r\n2 - Подтверждено (Confirmed)\r\n3 - Отклонено (Rejected)");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "public",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "ИД пользователя");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "ИД пользователя")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Login = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false, comment: "Хэш пароля"),
                    Role = table.Column<short>(type: "smallint", nullable: false, comment: "Роль Пользователя:\r\n1 - Пользователь (User)\r\n2 - Администратор (Admin)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                schema: "public",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_UserId",
                schema: "public",
                table: "Bookings",
                column: "UserId",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_UserId",
                schema: "public",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_UserId",
                schema: "public",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "public",
                table: "Bookings");

            migrationBuilder.AlterColumn<short>(
                name: "Status",
                schema: "public",
                table: "Bookings",
                type: "smallint",
                nullable: false,
                comment: "Статус бронирования:\r\n1 - В обработке (Pending)\r\n2 - Подтверждено (Confirmed)\r\n3 - Отклонено (Rejected)",
                oldClrType: typeof(short),
                oldType: "smallint",
                oldComment: "Статус бронирования:\r\n1 - В обработке (Pending)\r\n2 - Подтверждено (Confirmed)\r\n3 - Отклонено (Rejected)\r\n3 - Отменено (Cancelled)");
        }
    }
}
