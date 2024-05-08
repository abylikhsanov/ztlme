using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ztlme.Migrations
{
    /// <inheritdoc />
    public partial class AddingCreditScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CreditScoreOk",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditScoreOk",
                table: "Users");
        }
    }
}
