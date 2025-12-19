using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pebtra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddImportSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImportSessionId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImportSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AccountId = table.Column<string>(type: "char(10)", unicode: false, fixedLength: true, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportSessions_Accounts",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ImportSessionId",
                table: "Transactions",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportSessions_AccountId",
                table: "ImportSessions",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_ImportSessions",
                table: "Transactions",
                column: "ImportSessionId",
                principalTable: "ImportSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_ImportSessions",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "ImportSessions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ImportSessionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ImportSessionId",
                table: "Transactions");
        }
    }
}
