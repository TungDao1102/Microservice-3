using Automatonymous;
using BuildingBlocks.Common.Contracts;

namespace TradingService.StateMachines
{
    public class PurchaseStateMachine : MassTransitStateMachine<PurchaseState>
    {
        public State? Accepted { get; }
        public State? ItemsGranted { get; }
        public State? Completed { get; }
        public State? Faulted { get; }
        public Event<PurchaseRequested>? PurchaseRequested { get; }
        public Event<GetPurchaseState>? GetPurchaseState { get; }

        public PurchaseStateMachine()
        {
            InstanceState(state => state.CurrentState);
            ConfigureEvents();
            ConfigureInitialState();
            ConfigureAny();
        }

        public void ConfigureEvents()
        {
            Event(() => PurchaseRequested);
            Event(() => GetPurchaseState);
        }

        public void ConfigureInitialState()
        {
            Initially(
                When(PurchaseRequested)
                    .Then(context =>
                    {
                        context.Instance.UserId = context.Data.UserId;
                        context.Instance.ItemId = context.Data.ItemId;
                        context.Instance.Quantity = context.Data.Quantity;
                        context.Instance.Received = DateTimeOffset.UtcNow;
                        context.Instance.LastUpdated = context.Instance.Received;
                    })
                    .TransitionTo(Accepted)
                );
        }

        private void ConfigureAny()
        {
            // return type of response when GetPurchaseState is invoked
            DuringAny(
                When(GetPurchaseState)
                    .Respond(x => x.Instance)
            );
        }
    }
}
