using CatalogService.Dtos;
using CatalogService.Entities;
using CatalogService.Extensions;
using CatalogService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController(IItemRepository itemRepository) : ControllerBase
    {
        [HttpGet("GetAllItem")]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAllItemAsync()
        {
            var items = (await itemRepository.GetAllItemAsync()).Select(item => item.AsDto());
            return Ok(items);
        }

        [HttpGet("GetItem/{id}")]
        public async Task<ActionResult<ItemDto>> GetItemAsync([FromRoute] Guid id)
        {
            var item = await itemRepository.GetItemAsync(id);
            return item is null ? NotFound() : item.AsDto();
        }

        [HttpPost("CreateItem")]
        public async Task<ActionResult<ItemDto>> CreateItemAsync([FromBody] CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };
            await itemRepository.CreateItemAsync(item);
            return CreatedAtAction(nameof(GetItemAsync), new { id = item.Id }, item);
        }

        [HttpPut("UpdateItem/{id}")]
        public async Task<IActionResult> PutAsync([FromRoute] Guid id, [FromBody] UpdateItemDto updateItemDto)
        {
            var existingItem = await itemRepository.GetItemAsync(id);

            if (existingItem is null)
            {
                return NotFound();
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await itemRepository.UpdateItemAsync(existingItem);
            return Ok();
        }

        [HttpDelete("DeleteItem/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
        {
            var item = await itemRepository.GetItemAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            await itemRepository.DeleteItemAsync(item.Id);
            return Ok();
        }
    }
}
