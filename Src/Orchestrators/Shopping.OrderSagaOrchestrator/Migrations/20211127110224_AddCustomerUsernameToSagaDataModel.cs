using Microsoft.EntityFrameworkCore.Migrations;

namespace Shopping.OrderSagaOrchestrator.Migrations
{
    public partial class AddCustomerUsernameToSagaDataModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerUsername",
                table: "OrderStateData",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerUsername",
                table: "OrderStateData");
        }
    }
}
