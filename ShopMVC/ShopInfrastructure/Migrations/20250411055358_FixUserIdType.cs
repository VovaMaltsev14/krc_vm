using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Видаляємо foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_orders_users",
                table: "Orders");

            // 2. Міняємо тип колонки
            migrationBuilder.AlterColumn<string>(
                name: "od_user",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // 3. Знову додаємо foreign key з правильним типом
            migrationBuilder.AddForeignKey(
                name: "FK_orders_users",
                table: "Orders",
                column: "od_user",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
