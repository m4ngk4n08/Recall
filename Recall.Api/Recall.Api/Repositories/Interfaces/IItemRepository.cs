using Recall.Api.Models;

namespace Recall.Api.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task<IEnumerable<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(Guid id);
        Task<Item> CreateAsync(Item item);
        Task<Item?> UpdateAsync(Guid id, Item updated);
        Task<bool> DeleteAsync(Guid id);
    }
}
