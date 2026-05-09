using CloudGames.Users.Application.Interfaces.Security;
using MongoDB.Driver;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Infrastructure.Persistence.Context;

namespace Users.Infrastructure.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAdminAsync(
            MongoContext context,
            IPasswordHashService passwordHashService)
        {
            var anyUser = await context.Users
                .Find(FilterDefinition<User>.Empty)
                .Limit(1)
                .AnyAsync();

            if (anyUser)
                return;

            var passwordHash = passwordHashService.Hash("Admin123!");

            var admin = User.Create(
                "Admin",
                "admin@cloudgames.com",
                passwordHash,
                UserRole.Admin
            );

            await context.Users.InsertOneAsync(admin);
        }
    }
}
