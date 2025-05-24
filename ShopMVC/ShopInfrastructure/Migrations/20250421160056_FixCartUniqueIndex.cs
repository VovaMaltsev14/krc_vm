using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCartUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing unique constraint if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'unique_user_cart' AND parent_object_id = OBJECT_ID('carts'))
                BEGIN
                    ALTER TABLE carts DROP CONSTRAINT unique_user_cart;
                END
            ");

            // Drop existing indexes if they exist
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_carts_user_id_unique' AND object_id = OBJECT_ID('carts'))
                BEGIN
                    DROP INDEX IX_carts_user_id_unique ON carts;
                END

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_carts_session_id_unique' AND object_id = OBJECT_ID('carts'))
                BEGIN
                    DROP INDEX IX_carts_session_id_unique ON carts;
                END
            ");

            // Create new filtered unique indexes
            migrationBuilder.CreateIndex(
                name: "IX_carts_user_id_unique",
                table: "carts",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_carts_session_id_unique",
                table: "carts",
                column: "session_id",
                unique: true,
                filter: "[session_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_carts_user_id_unique",
                table: "carts");

            migrationBuilder.DropIndex(
                name: "IX_carts_session_id_unique",
                table: "carts");

            migrationBuilder.CreateIndex(
                name: "unique_user_cart",
                table: "carts",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");
        }
    }
}
