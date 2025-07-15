using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserForm.Migrations
{
    /// <inheritdoc />
    public partial class addedindexformcreatedby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Forms_AspNetUsers_CreatedById",
                table: "Forms");

            migrationBuilder.AddForeignKey(
                name: "FK_Forms_AspNetUsers_CreatedById",
                table: "Forms",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Forms_AspNetUsers_CreatedById",
                table: "Forms");

            migrationBuilder.AddForeignKey(
                name: "FK_Forms_AspNetUsers_CreatedById",
                table: "Forms",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
