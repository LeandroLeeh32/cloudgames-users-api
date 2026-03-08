using CloudGames.Users.Application.Events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;

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
