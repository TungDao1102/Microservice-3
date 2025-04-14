using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Contracts;
using MassTransit;
using TradingService.Entities;

namespace TradingService.Consumers
{
    public class UserUpdatedConsumer(IRepository<ApplicationUser> repository) : IConsumer<UserUpdated>
    {
        public async Task Consume(ConsumeContext<UserUpdated> context)
        {
            var message = context.Message;

            var user = await repository.GetAsync(message.UserId);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = message.UserId,
                    Gil = message.NewTotalGil
                };

                await repository.CreateAsync(user);
            }
            else
            {
                user.Gil = message.NewTotalGil;

                await repository.UpdateAsync(user);
            }
        }
    }
}