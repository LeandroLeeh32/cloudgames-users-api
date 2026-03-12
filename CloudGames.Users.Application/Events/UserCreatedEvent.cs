using MassTransit;

namespace CloudGames.Notifications.Application.IntegrationEvents.Users
{
    public class UserCreatedIntegrationEvent
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Email { get; }

        public UserCreatedIntegrationEvent(Guid id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }
    }
}
