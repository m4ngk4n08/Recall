using AutoMapper;
using Recall.Api.DTOs.Item;
using Recall.Api.Models;

namespace Recall.Api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ItemCreateDto, Item>();
            CreateMap<Item, ItemResponseDto>();
            CreateMap<ItemUpdateDto, Item>();
        }
    }
}
