using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Recall.Api.DTOs;
using Recall.Api.DTOs.Item;
using Recall.Api.Models;
using Recall.Api.Repositories.Interfaces;
using Recall.Api.Services.Interfaces;

namespace Recall.Api.Services
{
    public class ItemService : IITemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IMapper _mapper;

        public ItemService(IItemRepository itemRepository, IMapper mapper)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
        }
        public async Task<ItemResponseDto> CreateAsync(ItemCreateDto dto)
        {
            var item = _mapper.Map<Item>(dto);
            var created = await _itemRepository.CreateAsync(item);
            return _mapper.Map<ItemResponseDto>(created);
        }

        public async Task<bool> DeleteAsync(Guid id)
            => await _itemRepository.DeleteAsync(id);

        public async Task<IEnumerable<ItemResponseDto>> GetAllAsync()
        {
            var items = await _itemRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ItemResponseDto>>(items);
        }

        public async Task<ItemResponseDto?> GetByIdAsync(Guid id)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            return item is null ? null : _mapper.Map<ItemResponseDto>(item);
        }

        public async Task<ItemResponseDto?> UpdateAsync(Guid id, ItemCreateDto dto)
        {
            var existing = await _itemRepository.GetByIdAsync(id);
            if (existing is null) return null;

            _mapper.Map(dto, existing);
            var updated = await _itemRepository.UpdateAsync(id, existing);
            return updated is null ? null : _mapper.Map<ItemResponseDto>(updated);
        }

        public async Task<IEnumerable<TopicResponseDto>> GetTopicsAsync()
        {
            var topics = await _itemRepository.GetTopicsAsync();
            return topics.Select(t => new TopicResponseDto { Name = t.Name, Count = t.Count });
        }

        public async Task<IEnumerable<ItemResponseDto>> GetByTagAsync(string tag)
        {
            var items = await _itemRepository.GetByTagAsync(tag);
            return _mapper.Map<IEnumerable<ItemResponseDto>>(items);
        }
    }
}
