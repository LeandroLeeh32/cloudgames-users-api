
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces.Repositories;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence.Context;


namespace Users.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
            AppDbContext context,
            ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(User user)
        {
            _logger.LogInformation(
                "[Infrastructure][UserRepository] Persisting new user {UserId} to database",
                user.Id);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation(
                "[Infrastructure][UserRepository] Fetching user by id {UserId}",
                id);

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogInformation(
                "[Infrastructure][UserRepository] Fetching user by email {Email}",
                email);

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.Value == email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            _logger.LogInformation("[Infrastructure][UserRepository] Fetching all users");

            return await _context.Users
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _logger.LogInformation(
                "[Infrastructure][UserRepository] Updating user {UserId}",
                user.Id);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _logger.LogWarning(
                "[Infrastructure][UserRepository] Removing user {UserId}",
                user.Id);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }


        public Task SaveChangesAsync()
        {
            _logger.LogInformation("[Infrastructure][UserRepository] Saving changes");

            return _context.SaveChangesAsync();
        }
    }
}
