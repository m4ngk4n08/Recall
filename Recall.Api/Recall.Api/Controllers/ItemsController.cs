using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Recall.Api.DTOs;
using Recall.Api.Services.Interfaces;

namespace Recall.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        private readonly IITemService _iItemService;
        private readonly IMapper _mapper;

        public ItemsController(IITemService iTemService, IMapper mapper)
        {
            _iItemService = iTemService;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ItemResponseDto>), 200)]
        public async Task<IActionResult> GetAll() =>
            Ok(await _iItemService.GetAllAsync());

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _iItemService.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] ItemCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _iItemService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Update(Guid id, [FromBody] ItemCreateDto dto)
        {
            var updated = await _iItemService.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _iItemService.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }
    }
}
