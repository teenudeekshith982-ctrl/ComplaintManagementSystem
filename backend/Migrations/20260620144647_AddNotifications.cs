using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ComplaintManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ComplaintCategories_ComplaintCategoryCategoryId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_ComplaintCategoryCategoryId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ComplaintCategoryCategoryId",
                table: "Complaints");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RelatedComplaintId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Complaints_RelatedComplaintId",
                        column: x => x.RelatedComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "ComplaintId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CategoryId",
                table: "Complaints",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedComplaintId",
                table: "Notifications",
                column: "RelatedComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ComplaintCategories_CategoryId",
                table: "Complaints",
                column: "CategoryId",
                principalTable: "ComplaintCategories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_ComplaintCategories_CategoryId",
                table: "Complaints");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CategoryId",
                table: "Complaints");

            migrationBuilder.AddColumn<int>(
                name: "ComplaintCategoryCategoryId",
                table: "Complaints",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ComplaintCategoryCategoryId",
                table: "Complaints",
                column: "ComplaintCategoryCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_ComplaintCategories_ComplaintCategoryCategoryId",
                table: "Complaints",
                column: "ComplaintCategoryCategoryId",
                principalTable: "ComplaintCategories",
                principalColumn: "CategoryId");
        }
    }
}
