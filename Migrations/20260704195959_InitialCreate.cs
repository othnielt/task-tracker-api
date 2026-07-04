using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItems", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "WorkItems",
                columns: new[] { "Id", "AssignedTo", "CreatedDate", "Description", "LastUpdated", "Priority", "Reference", "Status", "Title" },
                values: new object[,]
                {
                    { 1, "A. Developer", new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Configure automated build and test on each push.", null, "High", "TASK-101", "InProgress", "Set up CI pipeline" },
                    { 2, "B. Engineer", new DateTime(2026, 3, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Validate required fields and length limits on all endpoints.", null, "Medium", "TASK-102", "Review", "Add input validation to API" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_Reference",
                table: "WorkItems",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkItems");
        }
    }
}
