using CloudGames.Users.Application.Interfaces.Messaging;
using MassTransit;

namespace CloudGames.Users.Infrastructure.Messaging.EventBus
{
    public class MassTransitEventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishAsync<T>(T message) where T : class
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

                await _publishEndpoint.Publish(message, cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Message broker is unavailable. Please contact support to enable your user.");
            }
        }
    }
}
