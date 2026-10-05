using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeamFlow___Project___Team_Management_SaaS.Migrations
{
    /// <inheritdoc />
    public partial class AddEnumsAndSeedPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "MaxMembers", "MaxProjects", "MaxStorage", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 3, 1, 104857600L, "Free", 0m },
                    { 2, 20, 20, 10737418240L, "Pro", 19.99m },
                    { 3, 100, 100, 53687091200L, "Business", 49.99m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
