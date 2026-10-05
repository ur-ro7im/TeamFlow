using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamFlow___Project___Team_Management_SaaS.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadlineReminderSentAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeadlineReminderSentAt",
                table: "Tasks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadlineReminderSentAt",
                table: "Tasks");
        }
    }
}
