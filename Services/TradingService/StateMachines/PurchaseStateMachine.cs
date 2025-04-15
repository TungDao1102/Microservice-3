using System.Diagnostics.Metrics;
using Automatonymous;
using BuildingBlocks.Common.Contracts;
using BuildingBlocks.Common.Settings;
using MassTransit;
using TradingService.Activities;
using TradingService.SignalR;

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
        public Event<InventoryItemsGranted>? InventoryItemsGranted { get; }
        public Event<GilDebited>? GilDebited { get; }
        public Event<Fault<GrantItems>>? GrantItemsFaulted { get; }
        public Event<Fault<DebitGil>>? DebitGilFaulted { get; }

        private readonly MessageHub _hub;
        private readonly ILogger<PurchaseStateMachine> _logger;
        private readonly IConfiguration _configuration;

        private readonly Counter<int> purchaseStartedCounter;
        private readonly Counter<int> purchaseSuccessCounter;
        private readonly Counter<int> purchaseFailedCounter;

        public PurchaseStateMachine(MessageHub hub, ILogger<PurchaseStateMachine> logger, IConfiguration configuration)
        {
            _hub = hub;
            _logger = logger;
            _configuration = configuration;

            var settings = _configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            Meter meter = new(settings?.ServiceName ?? string.Empty);
            purchaseStartedCounter = meter.CreateCounter<int>("PurchaseStarted");
            purchaseSuccessCounter = meter.CreateCounter<int>("PurchaseSuccess");
            purchaseFailedCounter = meter.CreateCounter<int>("PurchaseFailed");

            InstanceState(state => state.CurrentState);
            ConfigureEvents();
            ConfigureInitialState();
            ConfigureAny();
            ConfigureAccepted();
            ConfigItemsGranted();
            ConfigFaulted();
            ConfigCompleted();
        }

        public void ConfigureEvents()
        {
            Event(() => PurchaseRequested);
            Event(() => GetPurchaseState);
            Event(() => InventoryItemsGranted);
            Event(() => GilDebited);
            Event(() => GrantItemsFaulted, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
            Event(() => DebitGilFaulted, x => x.CorrelateById(context => context.Message.Message.CorrelationId));
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

                        _logger.LogInformation(
                         "Calculating total price for purchase with CorrelationId {CorrelationId}...",
                         context.Instance.CorrelationId);
                        purchaseStartedCounter.Add(1, new KeyValuePair<string, object?>(
                            nameof(context.Instance.ItemId),
                            context.Instance.ItemId));
                    })
                    .Activity(x => x.OfType<CalculatePurchaseTotalActivity>())
                    .Send(context => new GrantItems(
                        context.Instance.UserId,
                        context.Instance.ItemId,
                        context.Instance.Quantity,
                        context.Instance.CorrelationId))
                    .TransitionTo(Accepted)
                    .Catch<Exception>(ex => ex.Then(context =>
                    {
                        context.Instance.ErrorMessage = context.Exception.Message;
                        context.Instance.LastUpdated = DateTimeOffset.UtcNow;

                        _logger.LogError(
                               context.Exception,
                               "Could not calculate the total price of purchase with CorrelationId {CorrelationId}. Error: {ErrorMessage}",
                               context.Instance.CorrelationId,
                               context.Instance.ErrorMessage);
                        purchaseFailedCounter.Add(1, new KeyValuePair<string, object?>(
                            nameof(context.Instance.ItemId),
                            context.Instance.ItemId));
                    }).TransitionTo(Faulted)
                    .ThenAsync(async context => await _hub.SendStatusAsync(context.Instance)))
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

        private void ConfigureAccepted()
        {
            During(Accepted,
                Ignore(PurchaseRequested),
                When(InventoryItemsGranted)
                    .Then(context =>
                    {
                        context.Instance.LastUpdated = DateTimeOffset.UtcNow;

                        _logger.LogInformation(
                            "Items of purchase with CorrelationId {CorrelationId} have been granted to user {UserId}. ",
                            context.Instance.CorrelationId,
                            context.Instance.UserId);
                    })
                    .Send(context => new DebitGil(
                        context.Instance.UserId,
                        context.Instance.PurchaseTotal,
                        context.Instance.CorrelationId))
                    .TransitionTo(ItemsGranted),
                When(GrantItemsFaulted)
                    .Then(context =>
                    {
                        context.Instance.ErrorMessage = context.Data.Exceptions.First().Message;
                        context.Instance.LastUpdated = DateTimeOffset.UtcNow;

                        _logger.LogError(
                          "Could not grant items for purchase with CorrelationId {CorrelationId}. Error: {ErrorMessage}",
                          context.Instance.CorrelationId,
                          context.Instance.ErrorMessage);
                        purchaseFailedCounter.Add(1, new KeyValuePair<string, object?>(
                            nameof(context.Instance.ItemId),
                            context.Instance.ItemId));
                    })
                    .TransitionTo(Faulted)
                    .ThenAsync(async context => await _hub.SendStatusAsync(context.Instance))
            );
        }

        private void ConfigItemsGranted()
        {
            During(ItemsGranted,
                Ignore(PurchaseRequested),
                Ignore(InventoryItemsGranted),
                When(GilDebited)
                    .Then(context =>
                    {
                        context.Instance.LastUpdated = DateTimeOffset.UtcNow;

                        _logger.LogInformation(
                          "The total price of purchase with CorrelationId {CorrelationId} has been debited from user {UserId}. Purchase complete.",
                          context.Instance.CorrelationId,
                          context.Instance.UserId);
                        purchaseSuccessCounter.Add(1, new KeyValuePair<string, object?>(
                            nameof(context.Instance.ItemId),
                            context.Instance.ItemId));
                    })
                    .TransitionTo(Completed)
                    .ThenAsync(async context => await _hub.SendStatusAsync(context.Instance)),
                When(DebitGilFaulted)
                    .Send(context => new SubtractItems(
                        context.Instance.UserId,
                        context.Instance.ItemId,
                        context.Instance.Quantity,
                        context.Instance.CorrelationId))
                    .Then(context =>
                    {
                        context.Instance.ErrorMessage = context.Data.Exceptions.First().Message;
                        context.Instance.LastUpdated = DateTimeOffset.UtcNow;

                        _logger.LogError(
                           "Could not debit the total price of purchase with CorrelationId {CorrelationId} from user {UserId}. Error: {ErrorMessage}.",
                           context.Instance.CorrelationId,
                           context.Instance.UserId,
                           context.Instance.ErrorMessage);
                        purchaseFailedCounter.Add(1, new KeyValuePair<string, object?>(
                            nameof(context.Instance.ItemId),
                            context.Instance.ItemId));
                    })
                    .TransitionTo(Faulted)
                    .ThenAsync(async context => await _hub.SendStatusAsync(context.Instance))
                );
        }

        private void ConfigFaulted()
        {
            During(Faulted,
                Ignore(PurchaseRequested),
                Ignore(InventoryItemsGranted),
                Ignore(GilDebited)
            );
        }

        private void ConfigCompleted()
        {
            During(Completed,
                Ignore(PurchaseRequested),
                Ignore(InventoryItemsGranted),
                Ignore(GilDebited)
            );
        }
    }
}
