using Users.Domain.Enums;

namespace CloudGames.Users.API.Contracts.Request.User
{
    public class UpdateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
