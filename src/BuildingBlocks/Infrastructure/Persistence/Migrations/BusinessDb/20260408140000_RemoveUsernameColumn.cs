using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.BusinessDb
{
    [DbContext(typeof(BusinessDbContext))]
    [Migration("20260408140000_RemoveUsernameColumn")]
    public partial class RemoveUsernameColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                schema: "authorizeSchema",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                schema: "authorizeSchema",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                schema: "authorizeSchema",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                schema: "authorizeSchema",
                table: "Users",
                column: "Username",
                unique: true);
        }
    }
}
