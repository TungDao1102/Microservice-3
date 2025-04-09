using System.ComponentModel.DataAnnotations;

namespace TradingService.Dtos
{
    public record SubmitPurchaseDto([Required] Guid ItemId, [Range(1, 100)] int Quantity);

}
