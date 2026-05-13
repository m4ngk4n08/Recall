
using Pgvector;

namespace Recall.Api.Models
{
    public class Item
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public string SourceType { get; set; } = "note";
        public string? SourceUrl { get; set; }
        public DateTime SavedAt { get; set; }
        public List<string> Tags { get; set; }

        // Chunking & vector search
        public Guid? ParentId { get; set; } // null for original, points to parent for chunks
        public int ChunkIndex { get; set; } // order of chunk within parent
        public Vector? Embedding { get; set; }
    }
}
