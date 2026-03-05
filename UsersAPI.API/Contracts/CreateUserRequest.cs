
using Users.Domain.Enums;

namespace Users.API.Contracts
{
    public class CreateUserRequest
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public UserRole Role { get; set; } = default!;
    }
}
