using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ztlme.Migrations
{
    /// <inheritdoc />
    public partial class AddingSignedBlobToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "SignedBlob",
                table: "Users",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignedBlob",
                table: "Users");
        }
    }
}
