
using CloudGames.Users.Application.Interfaces.Security;
using Microsoft.Extensions.Logging;
using Users.Application.Interfaces.Repositories;
using Users.Domain.Entities;
using Users.Domain.Enums;
using Users.Domain.Exceptions;

namespace Users.Application.UseCases.Users
{
    public class CreateUserUseCase
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<CreateUserUseCase> _logger;
        private readonly IPasswordHashService _passwordHashService;

        public CreateUserUseCase(IUserRepository repository, IPasswordHashService passwordHashService, ILogger<CreateUserUseCase> logger)
        {
            _repository = repository;
            _passwordHashService = passwordHashService;
            _logger = logger;
        }

        public async Task<Guid> ExecuteAsync(string name, string email, string password, UserRole role)
        {
            _logger.LogInformation("[App][CreateUserUseCase] Starting user creation. Email: {Email}", email);

            var existingUser = await _repository.GetByEmailAsync(email);

            if (existingUser is not null)
                throw new DomainException("User already exists.");

            var passwordHash = _passwordHashService.Hash(password);

            var user = User.Create(name, email, passwordHash, role);

            await _repository.AddAsync(user);

            _logger.LogInformation("[App][CreateUserUseCase] User created successfully. Id: {UserId}", user.Id);

            return user.Id;
        }
    }
}

