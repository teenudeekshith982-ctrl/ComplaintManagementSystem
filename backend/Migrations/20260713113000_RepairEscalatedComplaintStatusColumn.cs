using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplaintManagementSystem.Migrations
{
    /// <summary>
    /// Repairs databases whose migration history records the Status migration
    /// even though the physical column was never created.
    /// </summary>
    public partial class RepairEscalatedComplaintStatusColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"EscalatedComplaints\" " +
                "ADD COLUMN IF NOT EXISTS \"Status\" integer NOT NULL DEFAULT 0;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty. The column may have been created by the
            // preceding migration, so a rollback must not remove valid data.
        }
    }
}
