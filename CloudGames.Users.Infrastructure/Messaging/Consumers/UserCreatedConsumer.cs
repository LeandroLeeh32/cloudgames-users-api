using CloudGames.Notifications.Application.IntegrationEvents.Users;
using MassTransit;

namespace CloudGames.Users.Infrastructure.Messaging.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
        {
            var message = context.Message;

            Console.WriteLine($"User created: {message.Email}");

            await Task.CompletedTask;
        }
    }
}
