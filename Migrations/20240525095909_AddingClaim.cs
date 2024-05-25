using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ztlme.Migrations
{
    /// <inheritdoc />
    public partial class AddingClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claim_Users_UserId",
                table: "Claim");

            migrationBuilder.DropForeignKey(
                name: "FK_Contribution_Users_UserId",
                table: "Contribution");

            migrationBuilder.DropForeignKey(
                name: "FK_UserContributionSummary_Users_UserId",
                table: "UserContributionSummary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserContributionSummary",
                table: "UserContributionSummary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contribution",
                table: "Contribution");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Claim",
                table: "Claim");

            migrationBuilder.RenameTable(
                name: "UserContributionSummary",
                newName: "UserContributionSummaries");

            migrationBuilder.RenameTable(
                name: "Contribution",
                newName: "Contributions");

            migrationBuilder.RenameTable(
                name: "Claim",
                newName: "Claims");

            migrationBuilder.RenameIndex(
                name: "IX_UserContributionSummary_UserId",
                table: "UserContributionSummaries",
                newName: "IX_UserContributionSummaries_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Contribution_UserId",
                table: "Contributions",
                newName: "IX_Contributions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Claim_UserId",
                table: "Claims",
                newName: "IX_Claims_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Users",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserContributionSummaries",
                table: "UserContributionSummaries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contributions",
                table: "Contributions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Claims",
                table: "Claims",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Pools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TotalContribution = table.Column<double>(type: "double precision", nullable: false),
                    ClaimedThisYear = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserClaimSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserClaimTotal = table.Column<double>(type: "double precision", nullable: false),
                    ClaimCurrentYear = table.Column<double>(type: "double precision", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaimSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaimSummaries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserClaimSummaries_UserId",
                table: "UserClaimSummaries",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Users_UserId",
                table: "Claims",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contributions_Users_UserId",
                table: "Contributions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserContributionSummaries_Users_UserId",
                table: "UserContributionSummaries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Users_UserId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Contributions_Users_UserId",
                table: "Contributions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserContributionSummaries_Users_UserId",
                table: "UserContributionSummaries");

            migrationBuilder.DropTable(
                name: "Pools");

            migrationBuilder.DropTable(
                name: "UserClaimSummaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserContributionSummaries",
                table: "UserContributionSummaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contributions",
                table: "Contributions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Claims",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "UserContributionSummaries",
                newName: "UserContributionSummary");

            migrationBuilder.RenameTable(
                name: "Contributions",
                newName: "Contribution");

            migrationBuilder.RenameTable(
                name: "Claims",
                newName: "Claim");

            migrationBuilder.RenameIndex(
                name: "IX_UserContributionSummaries_UserId",
                table: "UserContributionSummary",
                newName: "IX_UserContributionSummary_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Contributions_UserId",
                table: "Contribution",
                newName: "IX_Contribution_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_UserId",
                table: "Claim",
                newName: "IX_Claim_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserContributionSummary",
                table: "UserContributionSummary",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contribution",
                table: "Contribution",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Claim",
                table: "Claim",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claim_Users_UserId",
                table: "Claim",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contribution_Users_UserId",
                table: "Contribution",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserContributionSummary_Users_UserId",
                table: "UserContributionSummary",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
