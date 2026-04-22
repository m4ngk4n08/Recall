using Recall.Api.DTOs;

namespace Recall.Api.Services.Interfaces
{
    public interface IITemService
    {
        Task<IEnumerable<ItemResponseDto>> GetAllAsync();
        Task<ItemResponseDto?> GetByIdAsync(Guid id);
        Task<ItemResponseDto> CreateAsync(ItemCreateDto dto);
        Task<ItemResponseDto?> UpdateAsync(Guid id, ItemCreateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
