using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VfxApi.Migrations
{
    /// <inheritdoc />
    public partial class Teste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrencyPair",
                table: "ExchangeRates",
                newName: "CurrencyPairTo");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyPairFrom",
                table: "ExchangeRates",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyPairFrom",
                table: "ExchangeRates");

            migrationBuilder.RenameColumn(
                name: "CurrencyPairTo",
                table: "ExchangeRates",
                newName: "CurrencyPair");
        }
    }
}
