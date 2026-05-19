using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recall.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIndexToHnsw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_Embedding",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Embedding",
                table: "Items",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_Embedding",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Embedding",
                table: "Items",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "ivfflat")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" });
        }
    }
}
