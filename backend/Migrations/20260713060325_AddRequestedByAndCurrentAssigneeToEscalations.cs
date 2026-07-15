using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestedByAndCurrentAssigneeToEscalations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentAssigneeId",
                table: "EscalatedComplaints",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedById",
                table: "EscalatedComplaints",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalatedComplaints_CurrentAssigneeId",
                table: "EscalatedComplaints",
                column: "CurrentAssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalatedComplaints_RequestedById",
                table: "EscalatedComplaints",
                column: "RequestedById");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalatedComplaints_Employees_CurrentAssigneeId",
                table: "EscalatedComplaints",
                column: "CurrentAssigneeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EscalatedComplaints_Employees_RequestedById",
                table: "EscalatedComplaints",
                column: "RequestedById",
                principalTable: "Employees",
                principalColumn: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EscalatedComplaints_Employees_CurrentAssigneeId",
                table: "EscalatedComplaints");

            migrationBuilder.DropForeignKey(
                name: "FK_EscalatedComplaints_Employees_RequestedById",
                table: "EscalatedComplaints");

            migrationBuilder.DropIndex(
                name: "IX_EscalatedComplaints_CurrentAssigneeId",
                table: "EscalatedComplaints");

            migrationBuilder.DropIndex(
                name: "IX_EscalatedComplaints_RequestedById",
                table: "EscalatedComplaints");

            migrationBuilder.DropColumn(
                name: "CurrentAssigneeId",
                table: "EscalatedComplaints");

            migrationBuilder.DropColumn(
                name: "RequestedById",
                table: "EscalatedComplaints");
        }
    }
}
