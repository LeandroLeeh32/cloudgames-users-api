using MassTransit;

namespace CloudGames.Users.Application.Events
{
    public record UserCreatedEvent(
         Guid Id,
         string Name,
         string Email
     );
}
