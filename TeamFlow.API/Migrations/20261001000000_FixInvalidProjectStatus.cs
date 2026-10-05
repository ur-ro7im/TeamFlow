using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TeamFlow.API.Data;

#nullable disable

namespace TeamFlow.API.Migrations
{
    /// <summary>
    /// Projects created before ProjectService set a status were stored with the
    /// undefined enum value "0". Normalise them to "Planned". Data-only migration:
    /// the EF model is unchanged, so the model snapshot needs no update.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261001000000_FixInvalidProjectStatus")]
    public partial class FixInvalidProjectStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE [Projects] SET [Status] = 'Planned' " +
                "WHERE [Status] NOT IN ('Planned', 'InProgress', 'Completed', 'Archived')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible data fix; nothing to undo.
        }
    }
}
