using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ComplaintManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class MoveDesignationsToDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Designation",
                table: "Employees",
                newName: "DesignationId");

            migrationBuilder.CreateTable(
                name: "EmployeeDesignations",
                columns: table => new
                {
                    DesignationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DesignationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDesignations", x => x.DesignationId);
                });

            migrationBuilder.Sql("INSERT INTO \"EmployeeDesignations\" (\"DesignationId\", \"DesignationName\", \"EscalationLevel\") VALUES (1, 'Employee / Support Agent', NULL), (2, 'Team Lead / Tier 1 Esc', 1), (3, 'Manager / Tier 2 Esc', 2), (4, 'Senior Manager / Tier 3 Esc', 3) ON CONFLICT DO NOTHING;");
            migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"EmployeeDesignations\"', 'DesignationId'), COALESCE(MAX(\"DesignationId\"), 1)) FROM \"EmployeeDesignations\";");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DesignationId",
                table: "Employees",
                column: "DesignationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeDesignations_DesignationId",
                table: "Employees",
                column: "DesignationId",
                principalTable: "EmployeeDesignations",
                principalColumn: "DesignationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeDesignations_DesignationId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "EmployeeDesignations");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DesignationId",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "DesignationId",
                table: "Employees",
                newName: "Designation");
        }
    }
}
