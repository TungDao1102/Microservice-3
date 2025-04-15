using System.Security.Claims;
using BuildingBlocks.Common.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingService.Dtos;
using TradingService.StateMachines;

namespace TradingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseController(
        IPublishEndpoint publishEndpoint,
        IRequestClient<GetPurchaseState> purchaseClient) : ControllerBase
    {
        [HttpGet("Status/{idempotencyId}")]
        public async Task<IActionResult> GetStatusAsync(Guid idempotencyId)
        {
            Response<PurchaseState> response = await purchaseClient.GetResponse<PurchaseState>(new GetPurchaseState(
                idempotencyId));

            PurchaseState purchaseState = response.Message;

            var purchase = new PurchaseDto(
                purchaseState.UserId,
                purchaseState.ItemId,
                purchaseState.PurchaseTotal,
                purchaseState.Quantity,
                purchaseState.CurrentState,
                purchaseState.ErrorMessage,
                purchaseState.Received,
                purchaseState.LastUpdated);

            return Ok(purchase);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchaseDto)
        {
            var userId = User.FindFirstValue("sub") ?? string.Empty;

            var message = new PurchaseRequested(
                Guid.Parse(userId),
                purchaseDto.ItemId,
                purchaseDto.Quantity,
                purchaseDto.IdempotencyId);

            await publishEndpoint.Publish(message);
            return AcceptedAtAction(nameof(GetStatusAsync), new { purchaseDto.IdempotencyId });
        }
    }
}
