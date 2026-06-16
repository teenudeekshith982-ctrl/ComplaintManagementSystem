using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class changedcolumnsofcomplainthistories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OldStatus",
                table: "ComplaintHistories",
                newName: "Details");

            migrationBuilder.RenameColumn(
                name: "NewStatus",
                table: "ComplaintHistories",
                newName: "Action");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Details",
                table: "ComplaintHistories",
                newName: "OldStatus");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "ComplaintHistories",
                newName: "NewStatus");
        }
    }
}
