using System.Security.Claims;
using BuildingBlocks.Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingService.Dtos;

namespace TradingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseController(IPublishEndpoint publishEndpoint) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchaseDto)
        {
            var userId = User.FindFirstValue("sub") ?? string.Empty;
            var correlationId = Guid.NewGuid();

            var message = new PurchaseRequested(
                Guid.Parse(userId),
                purchaseDto.ItemId,
                purchaseDto.Quantity,
                correlationId);

            await publishEndpoint.Publish(message);
            return Accepted();
        }
    }
}
