using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TradingService.StateMachines;

namespace TradingService.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        public async Task SendStatusAsync(PurchaseState status)
        {
            if (Clients != null && Context.UserIdentifier != null)
            {
                await Clients.User(Context.UserIdentifier)
                       .SendAsync("ReceivePurchaseStatus", status);
            }
        }
    }
}