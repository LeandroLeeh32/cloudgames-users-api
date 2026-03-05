using Users.Domain.Entities;

namespace Users.Application.Interfaces.Repositories
{
    public interface IJwtTokenGenerator
    {
        string Generate(User user);
    }
}
