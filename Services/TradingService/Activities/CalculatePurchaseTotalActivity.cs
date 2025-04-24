using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using MassTransit;
using TradingService.Entities;
using TradingService.Exceptions;
using TradingService.StateMachines;

namespace TradingService.Activities
{
    public class CalculatePurchaseTotalActivity(IRepository<CatalogItem> repository) : IStateMachineActivity<PurchaseState, PurchaseRequested>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<PurchaseState, PurchaseRequested> context, IBehavior<PurchaseState, PurchaseRequested> next)
        {
            var message = context.Message;
            CatalogItem item = await repository.GetAsync(message.ItemId) ?? throw new UnknownItemException(message.ItemId);

            context.Saga.PurchaseTotal = item.Price * message.Quantity;
            context.Saga.LastUpdated = DateTimeOffset.UtcNow;
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context, IBehavior<PurchaseState, PurchaseRequested> next) where TException : Exception
        {
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("calculate-purchase-total");
        }
    }
}
