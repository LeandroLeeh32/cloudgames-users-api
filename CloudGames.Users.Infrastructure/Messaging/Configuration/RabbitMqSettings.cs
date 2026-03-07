using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudGames.Users.Infrastructure.Messaging.Configuration
{
    public class RabbitMqSettings
    {
        public string Host { get; set; } = "";
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public QueueSettings Queues { get; set; } = new();
    }

    public class QueueSettings
    {
        public string UserCreated { get; set; } = "";
    }
}
