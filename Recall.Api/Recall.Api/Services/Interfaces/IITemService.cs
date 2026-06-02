using Recall.Api.DTOs;
using Recall.Api.DTOs.Item;

namespace Recall.Api.Services.Interfaces
{
    public interface IITemService
    {
        Task<IEnumerable<ItemResponseDto>> GetAllAsync();
        Task<ItemResponseDto?> GetByIdAsync(Guid id);
        Task<ItemResponseDto> CreateAsync(ItemCreateDto dto);
        Task<ItemResponseDto?> UpdateAsync(Guid id, ItemCreateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<TopicResponseDto>> GetTopicsAsync();
        Task<IEnumerable<ItemResponseDto>> GetByTagAsync(string tag);
    }
}
