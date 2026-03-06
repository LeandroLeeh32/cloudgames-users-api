using Users.Domain.Entities;

namespace CloudGames.Users.Application.Interfaces.Security
{
    public interface IJwtTokenGenerator
    {
        string Generate(User user);
    }
}
