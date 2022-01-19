using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Shopping.OrderSagaOrchestrator.Migrations
{
    public partial class UpdateOrderSagaDataModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderStateData");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OrderStateData");

            migrationBuilder.AddColumn<decimal>(
                name: "OrderTotalPrice",
                table: "OrderStateData",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderTotalPrice",
                table: "OrderStateData");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderStateData",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OrderStateData",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
