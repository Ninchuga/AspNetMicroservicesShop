using Microsoft.EntityFrameworkCore.Migrations;

namespace Ordering.Infrastructure.Migrations
{
    public partial class PaymentDataPropertiesWithPrivateSetters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderPlaced",
                table: "Orders",
                newName: "OrderDate");

            migrationBuilder.RenameColumn(
                name: "OrderCanceled",
                table: "Orders",
                newName: "OrderCancellationDate");

            migrationBuilder.AddColumn<int>(
                name: "PaymentData_CVV",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentData_CardName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentData_CardNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentData_CVV",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentData_CardName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentData_CardNumber",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "Orders",
                newName: "OrderPlaced");

            migrationBuilder.RenameColumn(
                name: "OrderCancellationDate",
                table: "Orders",
                newName: "OrderCanceled");
        }
    }
}
