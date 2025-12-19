using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pebtra.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyIsSecondaryField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSecondary",
                table: "Currencies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSecondary",
                table: "Currencies");
        }
    }
}
