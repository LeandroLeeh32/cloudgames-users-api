using CloudGames.Users.Application.Interfaces.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Infrastructure.Persistence.Context;

namespace Users.Infrastructure.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAdminAsync(
            AppDbContext context,
            IPasswordHashService passwordHashService)
        {
            if (await context.Users.AnyAsync())
                return;

            //criar um usuário admin padrão
            var passwordHash = passwordHashService.Hash("Admin123!");

            var admin = User.Create(
                "Admin",
                "admin@cloudgames.com",
                passwordHash,
                UserRole.Admin
            );

            context.Users.Add(admin);

            await context.SaveChangesAsync();
        }
    }
}
