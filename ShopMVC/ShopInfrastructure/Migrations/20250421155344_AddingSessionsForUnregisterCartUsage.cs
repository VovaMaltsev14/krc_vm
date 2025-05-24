using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingSessionsForUnregisterCartUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "session_id",
                table: "carts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            // Унікальний індекс для SessionId, якщо не null
            migrationBuilder.CreateIndex(
                name: "IX_carts_session_id_unique",
                table: "carts",
                column: "session_id",
                unique: true,
                filter: "[session_id] IS NOT NULL");

            // Унікальний індекс для UserId, якщо не null
            migrationBuilder.CreateIndex(
                name: "IX_carts_user_id_unique",
                table: "carts",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_carts_session_id_unique",
                table: "carts");

            migrationBuilder.DropIndex(
                name: "IX_carts_user_id_unique",
                table: "carts");

            migrationBuilder.DropColumn(
                name: "session_id",
                table: "carts");
        }

    }
}
