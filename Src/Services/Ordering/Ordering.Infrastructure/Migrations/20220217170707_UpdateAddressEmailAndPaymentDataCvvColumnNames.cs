using Microsoft.EntityFrameworkCore.Migrations;

namespace Ordering.Infrastructure.Migrations
{
    public partial class UpdateAddressEmailAndPaymentDataCvvColumnNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentData_CVV_Value",
                table: "Orders",
                newName: "PaymentData_CVV");

            migrationBuilder.RenameColumn(
                name: "Address_Email_Value",
                table: "Orders",
                newName: "Address_Email");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentData_CVV",
                table: "Orders",
                newName: "PaymentData_CVV_Value");

            migrationBuilder.RenameColumn(
                name: "Address_Email",
                table: "Orders",
                newName: "Address_Email_Value");
        }
    }
}
