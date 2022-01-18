using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Shopping.OrderSagaOrchestrator.Migrations
{
    public partial class UpdateOrderStateDataModelToSupportOptimisticConcurrency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderCreationDateTime",
                table: "OrderStateData",
                newName: "OrderCreationDate");

            migrationBuilder.RenameColumn(
                name: "OrderCancelDateTime",
                table: "OrderStateData",
                newName: "OrderCancellationDate");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "OrderStateData",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "OrderStateData");

            migrationBuilder.RenameColumn(
                name: "OrderCreationDate",
                table: "OrderStateData",
                newName: "OrderCreationDateTime");

            migrationBuilder.RenameColumn(
                name: "OrderCancellationDate",
                table: "OrderStateData",
                newName: "OrderCancelDateTime");
        }
    }
}
