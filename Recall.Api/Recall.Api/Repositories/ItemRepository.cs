using Microsoft.EntityFrameworkCore;
using Recall.Api.Data;
using Recall.Api.Models;
using Recall.Api.Repositories.Interfaces;

namespace Recall.Api.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;
        public ItemRepository(AppDbContext context) => _context = context;

        public async Task<Item> CreateAsync(Item item)
        {
            item.Id = Guid.NewGuid();
            item.SavedAt = DateTime.UtcNow;
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var item = await _context.Items.FindAsync(id);
            if(item == null) return false;

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
            => await _context.Items.ToListAsync();

        public async Task<Item?> GetByIdAsync(Guid id)
            => await _context.Items.FindAsync(id);

        public async Task<Item?> UpdateAsync(Guid id, Item updated)
        {
            var existing = await _context.Items.FindAsync(id);
            if(existing == null) return null;

            existing.Title = updated.Title;
            existing.Content = updated.Content;
            existing.SourceType = updated.SourceType;
            existing.SourceUrl = updated.SourceUrl;
            existing.Tags = updated.Tags;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<IEnumerable<(string Name, int Count)>> GetTopicsAsync()
        {
            var items = await _context.Items.ToListAsync();
            return items
                .SelectMany(i => i.Tags)
                .GroupBy(t => t)
                .Select(g => (Name: g.Key, Count: g.Count()))
                .OrderByDescending(t => t.Count);
        }

        public async Task<IEnumerable<Item>> GetByTagAsync(string tag)
        {
            var result = await _context.Items.ToListAsync();
            return result.Where(j => j.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        }
    }
}
