using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recall.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameSaveAtToSavedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SaveAt",
                table: "Items",
                newName: "SavedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SavedAt",
                table: "Items",
                newName: "SaveAt");
        }
    }
}
