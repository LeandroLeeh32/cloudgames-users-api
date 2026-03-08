using MassTransit;

namespace CloudGames.Users.Application.Events
{
    public record UserCreatedIntegrationEvent(
         Guid Id,
         string Name,
         string Email
     );
}
