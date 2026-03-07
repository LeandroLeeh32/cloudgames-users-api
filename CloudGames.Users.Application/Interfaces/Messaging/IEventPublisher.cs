using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudGames.Users.Application.Interfaces.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : class;
    }
}
