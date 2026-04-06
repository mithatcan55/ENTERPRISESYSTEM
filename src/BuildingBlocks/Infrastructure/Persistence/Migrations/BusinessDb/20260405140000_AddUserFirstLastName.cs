using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.BusinessDb
{
    [DbContext(typeof(BusinessDbContext))]
    [Migration("20260405140000_AddUserFirstLastName")]
    /// <inheritdoc />
    public partial class AddUserFirstLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "authorizeSchema",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "authorizeSchema",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "authorizeSchema",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "authorizeSchema",
                table: "Users");
        }
    }
}
