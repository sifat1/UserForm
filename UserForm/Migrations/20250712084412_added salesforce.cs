using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserForm.Migrations
{
    /// <inheritdoc />
    public partial class addedsalesforce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalesforceAccountId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SalesforceContactId",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesforceAccountId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SalesforceContactId",
                table: "AspNetUsers");
        }
    }
}
