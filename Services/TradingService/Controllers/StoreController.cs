using System.Security.Claims;
using BuildingBlocks.Common.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingService.Dtos;
using TradingService.Entities;

namespace TradingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreController(
        IRepository<CatalogItem> catalogRepository,
        IRepository<ApplicationUser> usersRepository,
        IRepository<InventoryItem> inventoryRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<StoreDto>> GetAsync()
        {
            string userId = User.FindFirstValue("sub") ?? string.Empty;

            var catalogItems = await catalogRepository.GetAllAsync();
            var inventoryItems = await inventoryRepository.GetAllAsync(
                item => item.UserId == Guid.Parse(userId)
            );
            var user = await usersRepository.GetAsync(Guid.Parse(userId));

            var storeDto = new StoreDto(
                catalogItems.Select(catalogItem =>
                    new StoreItemDto(
                        catalogItem.Id,
                        catalogItem.Name,
                        catalogItem.Description,
                        catalogItem.Price,
                        inventoryItems.FirstOrDefault(
                            inventoryItem => inventoryItem.CatalogItemId == catalogItem.Id)?.Quantity ?? 0
                        )
                ),
                user?.Gil ?? 0
            );

            return Ok(storeDto);
        }
    }
}