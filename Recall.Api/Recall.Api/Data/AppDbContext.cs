using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Recall.Api.Models;
using System.Text.Json;


namespace Recall.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt)
            : base(opt)
        {
            
        }

        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Build the conversion and then set the value comparer on the property's metadata.
                var tagsProperty = entity.Property(e => e.Tags)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                    );

                tagsProperty.Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                ));

                entity.Property(e => e.SaveAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasIndex(e => e.Embedding)
                    .HasMethod("ivfflat")
                    .HasOperators("vector_cosine_ops");
            });
            
        }
    }
}
