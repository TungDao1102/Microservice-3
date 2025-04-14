using Automatonymous;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using GreenPipes;
using TradingService.Entities;
using TradingService.Exceptions;
using TradingService.StateMachines;

namespace TradingService.Activities
{
    public class CalculatePurchaseTotalActivity(IRepository<CatalogItem> repository) : Activity<PurchaseState, PurchaseRequested>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<PurchaseState, PurchaseRequested> context, Behavior<PurchaseState, PurchaseRequested> next)
        {
            var message = context.Data;
            CatalogItem item = await repository.GetAsync(message.ItemId) ?? throw new UnknownItemException(message.ItemId);

            context.Instance.PurchaseTotal = item.Price * message.Quantity;
            context.Instance.LastUpdated = DateTimeOffset.UtcNow;
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context, Behavior<PurchaseState, PurchaseRequested> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("calculate-purchase-total");
        }
    }
}
